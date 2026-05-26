using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.DTO.GameObjectsDTO;
using TextGame.Presentation.Mappers;

namespace TextGame.Application.Services
{
    public class GameInfoService : IGameInfoService
    {
        private readonly IGameSessionStateService _gameSessionService;
        public GameInfoService(IGameSessionStateService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }
        public GameInfoDTO GetGameInfo()
        {
            var roomDTO = (RoomDTOBase)_gameSessionService.CurrentRoom.ToDTO();
            var weaponDTO = _gameSessionService.Weapon.ToDTO();
            var helmDTO = _gameSessionService.Helm != null ? (ArmorDTO)_gameSessionService.Helm.ToDTO() : null;
            var chestplateDTO = _gameSessionService.Chestplate != null ? (ArmorDTO)_gameSessionService.Chestplate.ToDTO() : null;
            var inventoryItems = _gameSessionService.Inventory.ToDTO();
            return new GameInfoDTO(roomDTO, weaponDTO, helmDTO, chestplateDTO, _gameSessionService.MaxHealth, _gameSessionService.CurrentHealth, _gameSessionService.Coins, _gameSessionService.Keys, inventoryItems);
        }
    }
}