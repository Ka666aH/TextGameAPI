using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.DTO;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameText;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;
using TextGame.Domain.Entities.GameObjects.Items.Heals;
using TextGame.Domain.Entities.GameObjects.Items.Other;

namespace TextGame.Application.Services
{
    public class GameControllerService : IGameControllerService
    {
        private readonly IGameSessionService _gameSessionService;
        private readonly IInventoryService _inventoryRepository;
        private readonly IGameInfoService _gameInfoService;
        private readonly IGetItemService _getItemService;
        public GameControllerService(
            IGameSessionService gameSessionService,
            IInventoryService inventoryRepository,
            IGameInfoService gameInfoService,
            IGetItemService getItemService)
        {
            _gameSessionService = gameSessionService;
            _inventoryRepository = inventoryRepository;
            _gameInfoService = gameInfoService;
            _getItemService = getItemService;
        }
        public Room GetCurrentRoom()
        {
            RequireGameStartedAndNotStartRoom();

            return _gameSessionService.CurrentRoom;
        }
        public void Start() => _gameSessionService.StartGame();
        public IEnumerable<Item> GetInventory()
        {
            RequireGameStartedAndNotStartRoom();

            return _gameSessionService.Inventory;
        }
        public int GetCoins()
        {
            RequireGameStartedAndNotStartRoom();

            return _gameSessionService.Coins;
        }
        public int GetKeys()
        {
            RequireGameStartedAndNotStartRoom();

            return _gameSessionService.Keys;
        }
        public List<MapRoomDTO> GetMap()
        {
            RequireGameStartedAndNotStartRoom();

            if (!_gameSessionService.Inventory.OfType<Map>().Any()) throw new NoMapException();
            return _gameSessionService.Rooms.Select(r => new MapRoomDTO(r.Number, r.Name ?? GeneralLabeles.GameObjectDefaultName)).ToList();
        }
        public void UseInventoryItem(int itemId)
        {
            RequireGameStarted();

            Item item = GetInventoryItem(itemId);

            if (item is not Heal heal) throw new InvalidIdException(ExceptionLabels.NotHealCode, ExceptionLabels.NotHealText);

            _gameSessionService.RemoveItemFromInventory(heal);
            var (maxHealthBoost, currentHealthBoost) = heal.Use();
            _gameSessionService.AddMaxHealth(maxHealthBoost);
            _gameSessionService.AddCurrentHealth(currentHealthBoost);
            if (_gameSessionService.CurrentHealth <= 0) throw new DefeatException(string.Format(ExceptionLabels.PlayerPoisoned, heal.Name), GetGameInfo());
        }

        public Item GetInventoryItem(int itemId) => _getItemService.GetItem(itemId, _gameSessionService.Inventory);
        public List<Equipment> GetEquipment() => _inventoryRepository.GetEquipment();
        public void EquipInventoryItem(int itemId)
        {
            RequireGameStarted();
            Item item = GetInventoryItem(itemId);
            if (item is not Equipment equip) throw new InvalidIdException(ExceptionLabels.NotEqiipmentCode, ExceptionLabels.NotEqiipmentText);
            _inventoryRepository.EquipInventoryItem(equip);
        }
        public void UnequipWeapon()
        {
            RequireGameStarted();
            _inventoryRepository.UnequipWeapon();
        }
        public void UnequipHelm()
        {
            RequireGameStarted();
            _inventoryRepository.UnequipHelm();
        }
        public void UnequipChestplate()
        {
            RequireGameStarted();
            _inventoryRepository.UnequipChestplate();
        }
        public void SellInventoryItem(int itemId)
        {
            RequireGameStarted();
            //RequireNotInBattle();
            RequireShop();

            Item item = GetInventoryItem(itemId);
            _inventoryRepository.SellInventoryItem(item);
        }
        public GameInfoDTO GetGameInfo()
        {
            RequireGameStartedAndNotStartRoom();
            return _gameInfoService.GetGameInfo();
        }
        private void RequireGameStarted()
        {
            if (!_gameSessionService.IsGameStarted) throw new UnstartedGameException();
        }
        private void RequireNotInBattle()
        {
            if (_gameSessionService.IsInBattle) throw new InBattleException();
        }
        private void RequireGameStartedAndNotStartRoom()
        {
            if (!_gameSessionService.IsGameStarted && _gameSessionService.Rooms.Count <= 1) throw new UnstartedGameException();
        }
        private void RequireShop()
        {
            if (_gameSessionService.CurrentRoom is not Shop) throw new NotShopException();
        }
    }
}