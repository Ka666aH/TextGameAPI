using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class GetRoomService : IGetRoomService
    {
        private readonly IGameSessionService _gameSessionService;
        private readonly IGameInfoService _gameInfoService;
        public GetRoomService(
            IGameSessionService gameSessionService,
            IGameInfoService gameInfoService
            )
        {
            _gameSessionService = gameSessionService;
            _gameInfoService = gameInfoService;
        }
        public Room GetRoom(int roomId)
        {
            /*//Старый вариант
            Room? room = Rooms.FirstOrDefault(r => r.Number == roomId);
            if (room == null) throw new NullIdException("ROOM_NOT_FOUND", "Комната с таким номером не найдена.");*/
            if (!_gameSessionService.IsGameStarted && _gameSessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            if (_gameSessionService.IsInBattle) throw new InBattleException();

            if (roomId < 0 || roomId > _gameSessionService.Rooms.Count) throw new NullRoomIdException();
            Room room = _gameSessionService.Rooms[roomId];
            if (!room.IsDiscovered) throw new UndiscoveredRoomException();
            _gameSessionService.SetCurrentRoom(room);
            if (_gameSessionService.CurrentRoom is EndRoom) throw new WinException(_gameInfoService.GetGameInfo());
            return room;
        }
    }
}