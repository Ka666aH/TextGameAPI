using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Globalization;
using System.Text.Json;
using TextGame.Application.DTO;
using TextGame.Application.Factories;
using TextGame.Application.Generators;
using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
using TextGame.Application.Interfaces.Orchestrators;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Application.GuardAttributes;
using TextGame.Application.Orchestrators;
using TextGame.Application.Services;
using TextGame.Application.Validators;
using TextGame.Domain.GameText;
using TextGame.Infrastructure.Cache;
using TextGame.Infrastructure.Configuration;
using TextGame.Infrastructure.Database;
using TextGame.Infrastructure.Database.Repositories;
using TextGame.Infrastructure.PasswordHasher;
using TextGame.Infrastructure.Token;
using TextGame.Infrastructure.Token.JWT;
using TextGame.Presentation.Attributes;
using TextGame.Presentation.Authorization;
using TextGame.Presentation.Middleware;
using TextGame.Presentation.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
//Ядро состояния
builder.Services.AddScoped<ISessionProvider, SessionProvider>();
builder.Services.AddScoped<IStateService, StateService>();

//Оркестраторные
builder.Services.AddScoped<StateOrchestrator>();
builder.Services.AddScoped<IStateOrchestrator>(sp =>
    GuardProxyFactory.Create<IStateOrchestrator>(
        sp.GetRequiredService<StateOrchestrator>(),
        sp.GetRequiredService<IStateService>()));

//Зависимые
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IGameInfoService, GameInfoService>();
builder.Services.AddScoped<IGetRoomService, GetRoomService>();
builder.Services.AddScoped<ICombatService, CombatService>();
builder.Services.AddScoped<ICheckItemService, CheckItemService>();

//Счётчики
builder.Services.AddScoped<IRoomIdService, RoomIdService>();
builder.Services.AddScoped<IItemIdService, ItemIdService>();
builder.Services.AddScoped<IEnemyIdService, EnemyIdService>();

//Фабрики
builder.Services.AddSingleton<ISessionFactory, SessionFactory>();
builder.Services.AddScoped<ISaveFactory, SaveFactory>();
builder.Services.AddScoped<IRoomFactory, RoomFactory>();
builder.Services.AddScoped<IItemFactory, ItemFactory>();
builder.Services.AddScoped<IEnemyFactory, EnemyFactory>();

//Генераторы
builder.Services.AddScoped<IMapGenerator, MapGenerator>();
builder.Services.AddScoped<IRoomContentGenerator, RoomContentGenerator>();

//Синглтоны
builder.Services.AddSingleton<IGetItemService, GetItemService>();
builder.Services.AddSingleton<IChestService, ChestService>();

//Настройки конфигурации
TimeSettings.Initialize(builder.Configuration);

//База данных
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

//Кэширование
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
    ?? throw new InvalidOperationException("Redis is not configured.");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<ICacheRepository, RedisRepository>();
//Репозитории
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ISaveRepository, SaveRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<ISessionAccessGuard, SessionAccessGuard>();

builder.Services.AddSingleton<IStateCacheService, StateCacheService>();

builder.Services.AddSingleton<ITokenRepository, JWTRepository>();

builder.Services.AddSingleton<IHasher, BCryptRepository>();

builder.Services.AddSingleton<IValidator<RegisterCommand>, RegisterCommandValidator>();

//Сервисы
builder.Services.AddSingleton<RefreshReplayCache>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISaveService, SaveService>();

//Фоновые сервисы
builder.Services.AddHostedService<AutoSaveService>();

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Исключения
builder.Services.AddProblemDetails();

//Аутентификация и авторизация
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JWT secret is not configured.");
JwtKeyProvider.Initialize(jwtSecret);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(JWTOptions.Configure);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.RequireSession, policy => policy.RequireClaim(AccessClaims.SessionId));
// 403 от policy и 401 от забытой куки отдают ProblemDetails, как и игровые исключения (п. 13).
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProblemDetailsAuthorizationHandler>();

builder.Services.AddHttpContextAccessor();
// camelCase в теле 429, как у остальных ProblemDetails (notes/backend-requests.md п. 12).
var problemJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // Партиция по клиенту: у каждого свой лимит вместо одного bucket на всех — глобальный
    // лимит валил 6-й логин в минуту у любого юзера (notes/AGENTS.md §1b, п.12).
    // По логину партицию сознательно не делаем: она даёт атакующему «залочить чужой вход».
    options.AddPolicy(TextGame.Presentation.Options.RateLimiter.AuthPolicyName, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = TimeSettings.RateLimitPermitLimit,
                Window = TimeSettings.RateLimitWindow,
                QueueLimit = TimeSettings.RateLimitQueueLimit
            }));

    // 429 без тела и Retry-After заставлял клиента гадать, когда повторять (п. 12).
    options.OnRejected = async (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
            context.HttpContext.Response.Headers["Retry-After"] =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = ExceptionsLabels.RateLimitedCode,
            Detail = ExceptionsLabels.RateLimitedMessage,
            Instance = context.HttpContext.Request.Path
        };

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(problem, problemJsonOptions, "application/problem+json", token);
    };
});
builder.Services.AddHealthChecks();

var app = builder.Build();

// За reverse-proxy (nginx): клиентский IP восстанавливается из X-Forwarded-For до того,
// как его прочитают fingerprint и лимитер. Блок включается заполненным
// ForwardedHeaders:KnownProxies (IP своего nginx) или ForwardedHeaders:KnownNetworks (CIDR,
// для Docker-сети с плавающим IP прокси) — без конфига он инертен. Доверять произвольным
// адресам нельзя: клиент подделает заголовок и подменит свой IP. XFF на nginx должен
// аппендиться ($proxy_add_x_forwarded_for), иначе клиент подделает и его.
var knownProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
var knownNetworks = builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [];
if (knownProxies.Length > 0 || knownNetworks.Length > 0)
{
    var forwardedOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    };
    foreach (string proxy in knownProxies) forwardedOptions.KnownProxies.Add(IPAddress.Parse(proxy));
    foreach (string network in knownNetworks) forwardedOptions.KnownNetworks.Add(ParseNetwork(network));
    app.UseForwardedHeaders(forwardedOptions);
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
    app.UseSwagger();
    app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseExceptionHandler("/exception");

app.UseRefreshAuthTokens();
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.MapHealthChecks("/health").WithMetadata(new BypassRefreshAttribute());

ApplyMigrations();

app.Run();

void ApplyMigrations()
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

// net8: KnownNetworks принимает Microsoft.AspNetCore.HttpOverrides.IPNetwork, а строку
// вида "172.18.0.0/16" он сам не парсит — разбираем addr/len руками.
static Microsoft.AspNetCore.HttpOverrides.IPNetwork ParseNetwork(string network)
{
    string[] parts = network.Split('/', 2);
    if (parts.Length != 2
        || !IPAddress.TryParse(parts[0], out IPAddress? address)
        || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int prefixLength))
        throw new InvalidOperationException($"ForwardedHeaders:KnownNetworks содержит некорректный CIDR: {network}");

    return new Microsoft.AspNetCore.HttpOverrides.IPNetwork(address, prefixLength);
}