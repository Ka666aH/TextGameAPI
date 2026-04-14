using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class GetRoomService : IGetRoomService
    {
        private readonly IGameSessionService _sessionService;
        private readonly IGameInfoService _gameInfoRepository;
        public GetRoomService(
            IGameSessionService sessionService,
            IGameInfoService gameInfoRepository
            )
        {
            _sessionService = sessionService;
            _gameInfoRepository = gameInfoRepository;
        }
        public Room GetRoom(int roomId)
        {
            /*//Старый вариант
            Room? room = Rooms.FirstOrDefault(r => r.Number == roomId);
            if (room == null) throw new NullIdException("ROOM_NOT_FOUND", "Комната с таким номером не найдена.");*/
            if (!_sessionService.IsGameStarted && _sessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            if (roomId < 0 || roomId > _sessionService.Rooms.Count) throw new NullRoomIdException();
            Room room = _sessionService.Rooms[roomId];
            if (!room.IsDiscovered) throw new UndiscoveredRoomException();
            _sessionService.SetCurrentRoom(room);
            if (_sessionService.CurrentRoom is EndRoom) throw new WinException(_gameInfoRepository.GetGameInfo());
            return room;
        }
    }
}