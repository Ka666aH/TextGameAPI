using Microsoft.AspNetCore.Diagnostics;
using TextGame;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IItemFactory, ItemFactory>();
//Сессионные
builder.Services.AddSingleton<IRoomNumberFactory, RoomNumberFactory>();
builder.Services.AddSingleton<IRoomFactory, RoomFactory>();
builder.Services.AddSingleton<IItemIdFactory, ItemIdFactory>();

//Контроллерные
//builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddSingleton<GameSession>();
builder.Services.AddSingleton<IGetCurrentRoomRepository, GetCurrentRoomRepository>();
builder.Services.AddSingleton<IChestRepository, ChestRepository>();
//builder.Services.AddSingleton<IInventoryRepository, InventoryRepository>();
builder.Services.AddSingleton<IGameStatsRepository, GameStatsRepository>();
builder.Services.AddSingleton<IGameOverStatsRepository, GameOverStatsRepository>();
builder.Services.AddSingleton<IGetRoomByIdRepository, GetRoomByIdRepository>();
builder.Services.AddSingleton<IGetItemByIdRepository, GetItemByIdRepository>();
builder.Services.AddSingleton<IGameControllerRepository, GameControllerRepository>();
builder.Services.AddSingleton<IRoomControllerRepository, RoomControllerRepository>();


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
                    case NullIdException or EmptyException:
                        result = Results.NotFound(new ErrorResponse(gameEx));
                        break;
                    case InvalidIdException or UncarryableException:
                        result = Results.UnprocessableEntity(new ErrorResponse(gameEx));
                        break;
                    case LockedException or NoKeyException or NoMapException or ClosedException or UndiscoveredRoomException:
                        result = Results.Json(new ErrorResponse(gameEx), statusCode: 403);
                        break;
                    case DefeatException or WinException:
                        EndExeption endEx = (EndExeption)gameEx;
                        result = Results.Ok(new SuccessfulResponse(
                            new GameOverDTO(endEx.Message, endEx.GameOverStats)
                        ));
                        break;
                    default: //UnstartedGameException
                        result = Results.BadRequest(new ErrorResponse(gameEx));
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