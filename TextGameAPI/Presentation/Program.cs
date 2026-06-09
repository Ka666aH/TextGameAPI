using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
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
using TextGame.Infrastructure.Cache;
using TextGame.Infrastructure.Database;
using TextGame.Infrastructure.Database.Repositories;
using TextGame.Infrastructure.PasswordHasher;
using TextGame.Infrastructure.Token;
using TextGame.Infrastructure.Token.JWT;
using TextGame.Presentation.Attributes;
using TextGame.Presentation.Middleware;
using TextGame.Presentation.Options;
using Microsoft.AspNetCore.RateLimiting;

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

//База данных
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

//Кэширование
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
    ?? throw new InvalidOperationException("Reids is not configured.");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});
builder.Services.AddSingleton<ICacheRepository, RedisRepository>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(RateLimiter.AuthPolicyName,opt =>
    {
        opt.PermitLimit = RateLimiter.AuthPermitLimit;
        opt.Window = RateLimiter.AuthWindow;
        opt.QueueLimit = RateLimiter.AuthQueueLimit;
    });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

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

app.MapControllers();

app.UseRateLimiter();

app.MapHealthChecks("/health").WithMetadata(new BypassRefreshAttribute());

ApplyMigrations();

app.Run();

void ApplyMigrations()
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}