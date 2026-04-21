using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.DTO;
using TextGame.Presentation.DTO.GameObjectsDTO;
using TextGame.Presentation.Mappers;

namespace TextGame.Application.Services
{
    public class GameInfoService : IGameInfoService
    {
        private readonly IGameSessionService _gameSessionService;
        public GameInfoService(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }
        public GameInfoDTO GetGameInfo()
        {
            //if (!_gameSessionService.IsGameStarted && _gameSessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            //RoomDTO roomDTO = new RoomDTO(Session.CurrentRoom!.Number, Session.CurrentRoom!.Name!, Session.CurrentRoom!.Description!, Session.CurrentRoom!.Enemies);
            var roomDTO = (RoomDTOBase)GameObjectMapper.ToDTO(_gameSessionService.CurrentRoom!);
            WeaponDTO weaponDTO = (WeaponDTO)GameObjectMapper.ToDTO(_gameSessionService.Weapon);
            ArmorDTO? helmDTO = _gameSessionService.Helm != null ? (ArmorDTO)GameObjectMapper.ToDTO(_gameSessionService.Helm) : null;
            ArmorDTO? chestplateDTO = _gameSessionService.Chestplate != null ? (ArmorDTO)GameObjectMapper.ToDTO(_gameSessionService.Chestplate) : null;
            var inventoryItems = GameObjectMapper.ToDTO(_gameSessionService.Inventory).Cast<ItemDTO>();
            return new GameInfoDTO(roomDTO, weaponDTO, helmDTO, chestplateDTO, _gameSessionService.MaxHealth, _gameSessionService.CurrentHealth, _gameSessionService.Coins, _gameSessionService.Keys, inventoryItems);
        }
    }
}