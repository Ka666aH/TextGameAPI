using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGetCurrentRoomService
    {
        Room GetCurrentRoom();
    }
}