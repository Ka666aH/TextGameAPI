using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Generators
{
    public interface IMapGenerator
    {
        List<Room> Generate();
    }
}