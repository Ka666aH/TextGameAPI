using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;
using TextGame.Domain.Entities.GameObjects.Items.Heals;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameText;

namespace TextGame.Application.Services
{
    public class GameControllerService : IGameControllerService
    {
        private readonly IStateService _stateService;
        private readonly IInventoryService _inventoryService;
        private readonly IGameInfoService _gameInfoService;
        private readonly IGetItemService _getItemService;

        public GameControllerService(
            IStateService stateService,
            IInventoryService inventoryService,
            IGameInfoService gameInfoService,
            IGetItemService getItemService)
        {
            _stateService = stateService;
            _inventoryService = inventoryService;
            _gameInfoService = gameInfoService;
            _getItemService = getItemService;
        }
        public async Task<Room> GetCurrentRoomAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();

            return _stateService.CurrentRoom;
        }
        //public void Start() => _gameSessionService.StartGame();
        public async Task<IEnumerable<Item>> GetInventoryAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();

            return _stateService.Inventory;
        }
        public async Task<int> GetCoinsAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();

            return _stateService.Coins;
        }
        public async Task<int> GetKeysAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();

            return _stateService.Keys;
        }
        public async Task<List<MapRoomDTO>> GetMapAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();

            if (!_stateService.Inventory.OfType<Map>().Any()) throw new NoMapException();
            return _stateService.Rooms.Select(r => new MapRoomDTO(r.Id, r.Name ?? GeneralLabeles.GameObjectDefaultName)).ToList();
        }
        public async Task UseInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();

            Item item = _getItemService.GetItem(itemId, _stateService.Inventory);

            if (item is not Heal heal) throw new InvalidIdException(ExceptionsLabels.NotHealCode, ExceptionsLabels.NotHealText);

            _stateService.RemoveItemFromInventory(heal);
            var (maxHealthBoost, currentHealthBoost) = heal.Use();
            _stateService.AddMaxHealth(maxHealthBoost);
            _stateService.AddCurrentHealth(currentHealthBoost);
            if (_stateService.CurrentHealth <= 0) 
                throw new DefeatException(
                    string.Format(ExceptionsLabels.PlayerPoisoned, heal.Name),
                    _gameInfoService.GetGameInfo());

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task<Item> GetInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            return _getItemService.GetItem(itemId, _stateService.Inventory);
        }
        public async Task<List<Equipment>> GetEquipmentAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            return _inventoryService.GetEquipment();
        }
        public async Task EquipInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            Item item = _getItemService.GetItem(itemId, _stateService.Inventory);
            if (item is not Equipment equip) throw new InvalidIdException(ExceptionsLabels.NotEqiipmentCode, ExceptionsLabels.NotEqiipmentText);
            _inventoryService.EquipInventoryItem(equip);

            await _stateService.CacheAsync(gameSessionId, ct);
        }
        public async Task UnequipWeaponAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            _inventoryService.UnequipWeapon();

            await _stateService.CacheAsync(gameSessionId, ct);
        }
        public async Task UnequipHelmAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            _inventoryService.UnequipHelm();

            await _stateService.CacheAsync(gameSessionId, ct);
        }
        public async Task UnequipChestplateAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            _inventoryService.UnequipChestplate();

            await _stateService.CacheAsync(gameSessionId, ct);
        }
        public async Task SellInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            RequireNotInBattle();
            RequireShop();

            Item item = _getItemService.GetItem(itemId, _stateService.Inventory);
            _inventoryService.SellInventoryItem(item);

            await _stateService.CacheAsync(gameSessionId, ct);
        }
        public async Task<GameInfoDTO> GetGameInfoAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();
            return _gameInfoService.GetGameInfo();
        }
        private void RequireGameStarted()
        {
            if (!_stateService.IsGameStarted) throw new UnstartedGameException();
        }
        private void RequireNotInBattle()
        {
            if (_stateService.IsInBattle) throw new InBattleException();
        }
        private void RequireGameStartedAndNotStartRoom()
        {
            if (!_stateService.IsGameStarted && _stateService.Rooms.Count <= 1) throw new UnstartedGameException();
        }
        private void RequireShop()
        {
            if (_stateService.CurrentRoom is not Shop) throw new NotShopException();
        }
    }
}