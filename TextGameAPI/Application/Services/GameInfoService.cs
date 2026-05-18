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
            var roomDTO = (RoomDTOBase)_gameSessionService.CurrentRoom!.ToDTO();
            WeaponDTO weaponDTO = (WeaponDTO)_gameSessionService.Weapon.ToDTO();
            ArmorDTO? helmDTO = _gameSessionService.Helm != null ? (ArmorDTO)_gameSessionService.Helm.ToDTO() : null;
            ArmorDTO? chestplateDTO = _gameSessionService.Chestplate != null ? (ArmorDTO)_gameSessionService.Chestplate.ToDTO() : null;
            var inventoryItems = _gameSessionService.Inventory.ToDTO().Cast<ItemDTO>();
            return new GameInfoDTO(roomDTO, weaponDTO, helmDTO, chestplateDTO, _gameSessionService.MaxHealth, _gameSessionService.CurrentHealth, _gameSessionService.Coins, _gameSessionService.Keys, inventoryItems);
        }
    }
}