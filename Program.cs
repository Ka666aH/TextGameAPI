using Microsoft.AspNetCore.Diagnostics;
using TextGame;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IRoomItemFactory, RoomItemFactory>();
builder.Services.AddSingleton<IChestItemFactory, ChestItemFactory>();

//Сессионные
builder.Services.AddSingleton<IRoomNumberFactory, RoomNumberFactory>();
builder.Services.AddSingleton<IRoomFactory, RoomFactory>();
builder.Services.AddSingleton<IItemIdFactory, ItemIdFactory>();

builder.Services.AddSingleton<IGameRepository, GameRepository>();

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

/*Вариант1
//app.UseExceptionHandler(exceptionHandlerApp =>
//{
//    exceptionHandlerApp.Run(async context =>
//    {
//        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

//        IResult result = exception switch
//        {
//            GameException gameEx => gameEx switch
//            {
//                NullIdException             => Results.NotFound(new ErrorResponse((GameException)exception)),
//                InvalidIdException          => Results.UnprocessableEntity(new ErrorResponse((GameException)exception)),
//                UnstartedGameException      => Results.BadRequest(new ErrorResponse((GameException)exception)),
//                EmptyException              => Results.NotFound(new ErrorResponse((GameException)exception)),
//                UncarryableException        => Results.UnprocessableEntity(new ErrorResponse((GameException)exception)),
//                LockedException             => Results.Json(new ErrorResponse((GameException)exception), statusCode: 403),
//                NoKeyException              => Results.Json(new ErrorResponse((GameException)exception), statusCode: 403),
//                ClosedException             => Results.Json(new ErrorResponse((GameException)exception), statusCode: 403),
//                DefeatException defeatEx    => Results.Ok(new SuccessfulResponse(
//                    new GameOverDTO(defeatEx.Message, defeatEx.GameOverStats)
//                    )),

//                _ => Results.BadRequest(new ErrorResponse((GameException)exception)),
//            },

//            _ => Results.Json(new ErrorResponse("INTERNAL_SERVER_ERROR", exception!.Message), statusCode: 500)
//        };
//        await result.ExecuteAsync(context);
//    });
//});
*/ // не работает
/*Вариант 3
//app.UseExceptionHandler(exceptionHandlerApp =>
//{
//    exceptionHandlerApp.Run(async context =>
//    {
//        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

//        IResult result = exception switch
//        {
//            GameException gameEx => gameEx switch
//            {
//                _ when gameEx is NullIdException => Results.NotFound(new ErrorResponse((GameException)exception)),
//                _ when gameEx is InvalidIdException => Results.UnprocessableEntity(new ErrorResponse((GameException)exception)),
//                _ when gameEx is UnstartedGameException => Results.BadRequest(new ErrorResponse((GameException)exception)),
//                _ when gameEx is EmptyException => Results.NotFound(new ErrorResponse((GameException)exception)),
//                _ when gameEx is UncarryableException => Results.UnprocessableEntity(new ErrorResponse((GameException)exception)),
//                _ when gameEx is LockedException => Results.Json(new ErrorResponse((GameException)exception), statusCode: 403),
//                _ when gameEx is NoKeyException => Results.Json(new ErrorResponse((GameException)exception), statusCode: 403),
//                _ when gameEx is ClosedException => Results.Json(new ErrorResponse((GameException)exception), statusCode: 403),
//                _ when gameEx is DefeatException defeatEx => Results.Ok(new SuccessfulResponse(
//                    new GameOverDTO(defeatEx.Message, defeatEx.GameOverStats)
//                    )),

//                _ => Results.BadRequest(new ErrorResponse((GameException)exception)),
//            },

//            _ => Results.Json(new ErrorResponse("INTERNAL_SERVER_ERROR", exception!.Message), statusCode: 500)
//        };
//        await result.ExecuteAsync(context);
//    });
//});
*/ // не работает
//Вариант 2 не работает
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        IResult result;

        switch (exception)
        {
            case NullIdException nullIdEx:
                result = Results.NotFound(new ErrorResponse(nullIdEx));
                break;
            case InvalidIdException invalidIdEx:
                result = Results.UnprocessableEntity(new ErrorResponse(invalidIdEx));
                break;
            case UnstartedGameException unstartedEx:
                result = Results.BadRequest(new ErrorResponse(unstartedEx));
                break;
            case EmptyException emptyEx:
                result = Results.NotFound(new ErrorResponse(emptyEx));
                break;
            case UncarryableException uncarryableEx:
                result = Results.UnprocessableEntity(new ErrorResponse(uncarryableEx));
                break;
            case LockedException lockedEx:
                result = Results.Json(new ErrorResponse(lockedEx), statusCode: 403);
                break;
            case NoKeyException noKeyEx:
                result = Results.Json(new ErrorResponse(noKeyEx), statusCode: 403);
                break;
            case ClosedException closedEx:
                result = Results.Json(new ErrorResponse(closedEx), statusCode: 403);
                break;
            case DefeatException defeatEx:
                result = Results.Ok(new SuccessfulResponse(
                    new GameOverDTO(defeatEx.Message, defeatEx.GameOverStats)
                ));
                break;
            case GameException gameEx:
                result = Results.BadRequest(new ErrorResponse(gameEx));
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