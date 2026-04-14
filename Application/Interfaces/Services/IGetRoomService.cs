using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGetRoomService
    {
        Room GetRoom(int roomId);
    }
}