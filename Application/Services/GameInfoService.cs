using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.DTO;
using TextGame.Presentation.Mappers;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class GameInfoService : IGameInfoService
    {
        private readonly IGameSessionService _sessionService;
        public GameInfoService(IGameSessionService sessionService)
        {
            _sessionService = sessionService;
        }
        public GameInfoDTO GetGameInfo()
        {
            if (!_sessionService.IsGameStarted && _sessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            //RoomDTO roomDTO = new RoomDTO(Session.CurrentRoom!.Number, Session.CurrentRoom!.Name!, Session.CurrentRoom!.Description!, Session.CurrentRoom!.Enemies);
            var roomDTO = GameObjectMapper.ToDTO(_sessionService.CurrentRoom!);
            WeaponDTO weaponDTO = (WeaponDTO)GameObjectMapper.ToDTO(_sessionService.Weapon);
            ArmorDTO? helmDTO = _sessionService.Helm != null ? (ArmorDTO)GameObjectMapper.ToDTO(_sessionService.Helm) : null;
            ArmorDTO? chestplateDTO = _sessionService.Chestplate != null ? (ArmorDTO)GameObjectMapper.ToDTO(_sessionService.Chestplate) : null;
            return new GameInfoDTO(roomDTO, weaponDTO, helmDTO, chestplateDTO, _sessionService.MaxHealth, _sessionService.CurrentHealth, _sessionService.Coins, _sessionService.Keys, GameObjectMapper.ToDTO(_sessionService.Inventory));
        }
    }
}