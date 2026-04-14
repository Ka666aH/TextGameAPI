using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Generators
{
    public interface IMapGenerator
    {
        List<Room> Generate(IGameSessionService sessionService);
    }
}