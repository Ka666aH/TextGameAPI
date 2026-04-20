using Microsoft.AspNetCore.Diagnostics;
using TextGame.Application.Factories;
using TextGame.Application.Generators;
using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
using TextGame.Application.Interfaces.Services;
using TextGame.Application.Services;
using TextGame.Domain;
using TextGame.Domain.GameExceptions;
using TextGame.Presentation.DTO;
using TextGame.Presentation.ResponseTemplates;

var builder = WebApplication.CreateBuilder(args);

//Ядро состояния
builder.Services.AddScoped<GameSession>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();

//Сервисы
//builder.Services.AddScoped<IGetCurrentRoomService, GetCurrentRoomService>();
builder.Services.AddScoped<IChestService, ChestService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IGameInfoService, GameInfoService>();
builder.Services.AddScoped<IGetRoomService, GetRoomService>();
builder.Services.AddScoped<IGameControllerService, GameControllerService>();
builder.Services.AddScoped<IRoomControllerService, RoomControllerService>();
builder.Services.AddScoped<IGetEnemyService, GetEnemyService>();
builder.Services.AddScoped<ICombatService, CombatService>(); //зависимый
builder.Services.AddScoped<ICheckItemService, CheckItemService>(); //зависимый

builder.Services.AddScoped<IRoomIdService, RoomIdService>();
builder.Services.AddScoped<IItemIdService, ItemIdService>();
builder.Services.AddScoped<IEnemyIdService, EnemyIdService>();

//Общие
builder.Services.AddTransient<IRoomFactory, RoomFactory>();
builder.Services.AddTransient<IItemFactory, ItemFactory>();
builder.Services.AddTransient<IEnemyFactory, EnemyFactory>();
builder.Services.AddTransient<IMapGenerator, MapGenerator>();
builder.Services.AddTransient<IRoomContentGenerator, RoomContentGenerator>();
builder.Services.AddTransient<IGetItemService, GetItemService>();



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        IResult result;

        switch (exception)
        {
            case GameException gameEx:

                switch (gameEx)
                {
                    case NullRoomIdException or NullItemIdException or EmptyException:
                        result = Results.NotFound(new ErrorResponse(gameEx));
                        break;
                    case InvalidIdException or UncarryableException or UnsellableItemException:
                        result = Results.UnprocessableEntity(new ErrorResponse(gameEx));
                        break;
                    case 
                    LockedException or 
                    NoKeyException or 
                    NoMapException or 
                    ClosedException or 
                    UndiscoveredRoomException or 
                    InBattleException or 
                    UnsearchedRoomException or 
                    NotShopException or 
                    NoMoneyException:
                        result = Results.Json(new ErrorResponse(gameEx), statusCode: 403);
                        break;
                    case DefeatException or WinException:
                        EndExeption endEx = (EndExeption)gameEx;
                        result = Results.Ok(new SuccessfulResponse(
                            new GameOverDTO(endEx.Message, endEx.GameInfo)
                        ));
                        break;
                    case BattleWinException battleWinEx:
                        result = Results.Ok(new SuccessfulResponse(
                            new BattleWinDTO(battleWinEx.Message, battleWinEx.BattleLog)
                            ));
                        break;
                    default: //UnstartedGameException ImpossibleStealException
                        //result = Results.BadRequest(new ErrorResponse(gameEx));
                        result = Results.Json(new ErrorResponse(gameEx), statusCode: 401);
                        break;
                }
                break;

            case Exception ex:
                result = Results.Json(
                    new ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message),
                    statusCode: 500);
                break;
            //exception == null
            default:
                result = Results.Json(
                    new ErrorResponse("INTERNAL_SERVER_ERROR", "Неизвестная ошибка сервера."),
                    statusCode: 500);
                break;
        }

        await result.ExecuteAsync(context);
    });
});

app.MapControllers();

app.Run();