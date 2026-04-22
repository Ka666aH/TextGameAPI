using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Generators
{
    public interface IRoomContentGenerator
    {
        void GenerateContent(Room room);
    }
}