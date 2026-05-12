using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using TextGame.Application.DTO;
using TextGame.Application.Factories;
using TextGame.Application.Generators;
using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Application.Services;
using TextGame.Application.Validators;
using TextGame.Infrastructure.Database;
using TextGame.Infrastructure.Database.Repositories;
using TextGame.Infrastructure.PasswordHasher;
using TextGame.Infrastructure.Token;
using TextGame.Infrastructure.Token.JWT;
using TextGame.Presentation.Middleware;
using TextGame.Presentation.Options;

var builder = WebApplication.CreateBuilder(args);

//Ядро состояния
builder.Services.AddScoped<IGameSessionProvider, GameSessionProvider>();
//builder.Services.AddSingleton<IGameSessionProvider, GameSessionProvider>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();

//Оркестраторные
builder.Services.AddScoped<IRoomControllerService, RoomControllerService>();
builder.Services.AddScoped<IGameControllerService, GameControllerService>();

//Зависимые
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IGameInfoService, GameInfoService>();
builder.Services.AddScoped<IGetEnemyService, GetEnemyService>();
builder.Services.AddScoped<IGetRoomService, GetRoomService>();
builder.Services.AddScoped<ICombatService, CombatService>();
builder.Services.AddScoped<ICheckItemService, CheckItemService>();

//Счётчики
builder.Services.AddScoped<IRoomIdService, RoomIdService>();
builder.Services.AddScoped<IItemIdService, ItemIdService>();
builder.Services.AddScoped<IEnemyIdService, EnemyIdService>();

//Фабрики
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

//Репозитории
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGameSessionRepository, GameSessionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddSingleton<ITokenRepository, JWTRepository>();

builder.Services.AddSingleton<IHasher, BCryptRepository>();

builder.Services.AddSingleton<IValidator<RegisterCommand>, RegisterCommandValidator>();

//Сервисы
builder.Services.AddScoped<IAuthService, AuthService>();

//Фоновые сервисы
builder.Services.AddHostedService<TokenCleaningService>();

builder.Services.AddControllers();
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
    .AddPolicy(Policies.RequireGameSession, policy => policy.RequireClaim(AccessClaims.GameSessionId));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler("/exception");

app.UseRefreshAuthTokens();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();