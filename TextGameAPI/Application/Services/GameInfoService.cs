using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Presentation.DTO.GameObjectsDTO;
using TextGame.Presentation.Mappers;

namespace TextGame.Application.Services
{
    public class GameInfoService : IGameInfoService
    {
        private readonly IStateService _stateService;
        public GameInfoService(IStateService stateService)
        {
            _stateService = stateService;
        }
        public GameInfoDTO GetGameInfo()
        {
            var roomDTO = (RoomDTOBase)_stateService.CurrentRoom.ToDTO();
            var weaponDTO = _stateService.Weapon.ToDTO();
            var helmDTO = _stateService.Helm != null ? (ArmorDTO)_stateService.Helm.ToDTO() : null;
            var chestplateDTO = _stateService.Chestplate != null ? (ArmorDTO)_stateService.Chestplate.ToDTO() : null;
            var inventoryItems = _stateService.Inventory.ToDTO();
            return new GameInfoDTO(roomDTO, weaponDTO, helmDTO, chestplateDTO, _stateService.MaxHealth, _stateService.CurrentHealth, _stateService.Coins, _stateService.Keys, inventoryItems, _stateService.IsGameStarted, ToWireResult(_stateService.Result));
        }

        // На провод уходит строка, а не enum: без StringEnumConverter Newtonsoft отдал бы 0/1/2.
        private static string? ToWireResult(GameResult result) => result switch
        {
            GameResult.Win => "win",
            GameResult.Lose => "lose",
            _ => null
        };
    }
}
