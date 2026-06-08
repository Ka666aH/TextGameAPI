using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Application.Services
{
    public class GetRoomService : IGetRoomService
    {
        private readonly IStateService _stateService;
        public GetRoomService(IStateService stateService)
        {
            _stateService = stateService;
        }
        public Room GetRoom(int roomId)
        {
            if (roomId < 0 || roomId > _stateService.Rooms.Count) throw new NullRoomIdException();
            Room room = _stateService.Rooms[roomId];
            if (!room.IsDiscovered) throw new UndiscoveredRoomException();
            return room;
        }
    }
}