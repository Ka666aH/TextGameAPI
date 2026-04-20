using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class GetRoomService : IGetRoomService
    {
        private readonly IGameSessionService _gameSessionService;
        public GetRoomService(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }
        public Room GetRoom(int roomId)
        {
            if (roomId < 0 || roomId > _gameSessionService.Rooms.Count) throw new NullRoomIdException();
            Room room = _gameSessionService.Rooms[roomId];
            if (!room.IsDiscovered) throw new UndiscoveredRoomException();
            return room;
        }
    }
}