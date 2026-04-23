using Microsoft.EntityFrameworkCore;
using TextGame.Application.Factories;
using TextGame.Application.Generators;
using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
using TextGame.Application.Interfaces.Services;
using TextGame.Application.Services;
using TextGame.Infrastructure.Database;

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

//DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql());

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();


app.UseExceptionHandler("/exception");

app.MapControllers();

app.Run();