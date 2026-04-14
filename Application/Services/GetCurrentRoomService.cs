using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class GetCurrentRoomService : IGetCurrentRoomService
    {
        private readonly IGameSessionService _sessionService;
        public GetCurrentRoomService(IGameSessionService sessionService)
        {
            _sessionService = sessionService;
        }
        public Room GetCurrentRoom()
        {
            if (!_sessionService.IsGameStarted && _sessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            return _sessionService.CurrentRoom!;
        }
    }
}