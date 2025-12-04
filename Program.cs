using Microsoft.AspNetCore.Diagnostics;
using TextGame;

var builder = WebApplication.CreateBuilder(args);

//Сессионные
builder.Services.AddScoped<GameSession>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();

builder.Services.AddScoped<IGetCurrentRoomRepository, GetCurrentRoomRepository>();
builder.Services.AddScoped<IChestRepository, ChestRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IGameInfoRepository, GameInfoRepository>();
builder.Services.AddScoped<IGetRoomByIdRepository, GetRoomByIdRepository>();
builder.Services.AddScoped<IGameControllerRepository, GameControllerRepository>();
builder.Services.AddScoped<IRoomControllerRepository, RoomControllerRepository>();
builder.Services.AddScoped<IGetEnemyByIdRepository, GetEnemyByIdRepository>();
builder.Services.AddScoped<ICombatRepository, CombatRepository>();
builder.Services.AddScoped<ICheckItemService, CheckItemService>();
builder.Services.AddScoped<IMapGenerator, MapGenerator>();

//Общие
builder.Services.AddSingleton<IRoomFactory, RoomFactory>();
builder.Services.AddSingleton<IItemFactory, ItemFactory>();
builder.Services.AddSingleton<IEnemyFactory, EnemyFactory>();
builder.Services.AddSingleton<IGetItemByIdRepository, GetItemByIdRepository>();




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