using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;
using TextGame.Domain.Entities.GameObjects.Items.Heals;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameText;
using TextGame.Presentation.DTO;

namespace TextGame.Application.Services
{
    public class GameControllerService : IGameControllerService
    {
        private readonly IGameSessionStateService _gameSessionService;
        private readonly IInventoryService _inventoryRepository;
        private readonly IGameInfoService _gameInfoService;
        private readonly IGetItemService _getItemService;

        public GameControllerService(
            IGameSessionStateService gameSessionService,
            IInventoryService inventoryRepository,
            IGameInfoService gameInfoService,
            IGetItemService getItemService)
        {
            _gameSessionService = gameSessionService;
            _inventoryRepository = inventoryRepository;
            _gameInfoService = gameInfoService;
            _getItemService = getItemService;
        }
        public async Task<Room> GetCurrentRoomAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();

            return _gameSessionService.CurrentRoom;
        }
        //public void Start() => _gameSessionService.StartGame();
        public async Task<IEnumerable<Item>> GetInventoryAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();

            return _gameSessionService.Inventory;
        }
        public async Task<int> GetCoinsAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();

            return _gameSessionService.Coins;
        }
        public async Task<int> GetKeysAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();

            return _gameSessionService.Keys;
        }
        public async Task<List<MapRoomDTO>> GetMapAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();

            if (!_gameSessionService.Inventory.OfType<Map>().Any()) throw new NoMapException();
            return _gameSessionService.Rooms.Select(r => new MapRoomDTO(r.Id, r.Name ?? GeneralLabeles.GameObjectDefaultName)).ToList();
        }
        public async Task UseInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();

            Item item = _getItemService.GetItem(itemId, _gameSessionService.Inventory);

            if (item is not Heal heal) throw new InvalidIdException(ExceptionsLabels.NotHealCode, ExceptionsLabels.NotHealText);

            _gameSessionService.RemoveItemFromInventory(heal);
            var (maxHealthBoost, currentHealthBoost) = heal.Use();
            _gameSessionService.AddMaxHealth(maxHealthBoost);
            _gameSessionService.AddCurrentHealth(currentHealthBoost);
            if (_gameSessionService.CurrentHealth <= 0) 
                throw new DefeatException(
                    string.Format(ExceptionsLabels.PlayerPoisoned, heal.Name),
                    _gameInfoService.GetGameInfo());

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }

        public async Task<Item> GetInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            return _getItemService.GetItem(itemId, _gameSessionService.Inventory);
        }
        public async Task<List<Equipment>> GetEquipmentAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            return _inventoryRepository.GetEquipment();
        }
        public async Task EquipInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            Item item = _getItemService.GetItem(itemId, _gameSessionService.Inventory);
            if (item is not Equipment equip) throw new InvalidIdException(ExceptionsLabels.NotEqiipmentCode, ExceptionsLabels.NotEqiipmentText);
            _inventoryRepository.EquipInventoryItem(equip);

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }
        public async Task UnequipWeaponAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            _inventoryRepository.UnequipWeapon();

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }
        public async Task UnequipHelmAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            _inventoryRepository.UnequipHelm();

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }
        public async Task UnequipChestplateAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            _inventoryRepository.UnequipChestplate();

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }
        public async Task SellInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            //RequireNotInBattle();
            RequireShop();

            Item item = _getItemService.GetItem(itemId, _gameSessionService.Inventory);
            _inventoryRepository.SellInventoryItem(item);

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }
        public async Task<GameInfoDTO> GetGameInfoAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);

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