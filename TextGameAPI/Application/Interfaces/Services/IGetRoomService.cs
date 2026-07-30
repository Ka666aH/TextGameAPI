using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGetRoomService
    {
        Room GetRoom(int roomId);
    }
}