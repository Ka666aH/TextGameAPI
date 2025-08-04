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

app.MapControllers();

app.Run();
