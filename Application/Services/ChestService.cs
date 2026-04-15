using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Items;
using TextGame.Presentation.DTO;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameObjects.Items.Other;

namespace TextGame.Application.Services
{
    public class ChestService : IChestService
    {
        private readonly IGameSessionService _sessionService;

        private IGameInfoService _gameInfoRepository;
        private IGetItemService _getItemByIdRepository;
        private ICombatService _combatRepository;

        private ICheckItemService _checkItemService;
        public ChestService(
            IGameSessionService sessionService,
            IGameInfoService gameInfoRepository,
            IGetItemService getItemByIdRepository,
            ICombatService combatRepository,
            ICheckItemService checkItemService
            )
        {
            _sessionService = sessionService;
            _gameInfoRepository = gameInfoRepository;
            _getItemByIdRepository = getItemByIdRepository;
            _combatRepository = combatRepository;
            _checkItemService = checkItemService;
        }
        public ChestStateDTO ReturnChestDTO(Chest chest)
        {
            return new ChestStateDTO(chest.Name!, chest.Description!, chest.IsLocked, chest.IsClosed);
        }
        public ChestStateDTO ReturnChestDTO(int chestId)
        {
            Chest chest = GetChestById(chestId);
            return new ChestStateDTO(chest.Name!, chest.Description!, chest.IsLocked, chest.IsClosed);
        }
        public Chest GetChestById(int chestId)
        {
            //Room room = GetRoomByIdRepository.GetRoomById(roomId);
            Item item = _getItemByIdRepository.GetItem(chestId, _sessionService.CurrentRoom!.Items);
            if (item is not Chest) throw new InvalidIdException("NOT_CHEST", "Это не сундук.");
            return (Chest)item;
        }
        public BattleLog HitChest(int chestId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            //Room room = GetRoomByIdRepository.GetRoomById(roomId);

            BattleLog battleLog;
            if (chest.Mimic is not null)
            {
                _sessionService.SetCurrentMimicChest(chest);
                _sessionService.RemoveItemFromCurrentRoom(chest);
                _sessionService.AddEnemyToCurrentRoom(chest.Mimic);
                _sessionService.StartBattle();
                battleLog = _combatRepository.DealDamage();
            }
            else
            {
                int playerHealthBeforeAttack = _sessionService.CurrentHealth;
                //attack
                var attackResult = _sessionService.Weapon.Attack(_sessionService.CurrentRoom!.Number);
                if (attackResult.SelfDamage != 0) _sessionService.AddCurrentHealth(-attackResult.SelfDamage);
                if (attackResult.IsWeaponBrokenDown) _sessionService.RemoveWeapon();

                int playerHealthAfterAttack = playerHealthBeforeAttack - _sessionService.CurrentHealth;
                battleLog = new BattleLog("СУНДУК", attackResult.Damage, null, null, "ИГРОК", playerHealthAfterAttack, playerHealthBeforeAttack, _sessionService.CurrentHealth);
            }
            return battleLog;
        }
        public void OpenChest(int chestId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.Mimic is not null)
            {
                _sessionService.EndGame();
                throw new DefeatException("НА ВАС НАПАЛ МИМИК! ВЫ БЫЛИ ПРОГЛОЧЕНЫ И ПЕРЕВАРЕНЫ!", _gameInfoRepository.GetGameInfo());
            }
            chest.Open();
        }
        public void UnlockChest(int chestId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (_sessionService.Keys > 0) _sessionService.AddKeys(-1);
            else throw new NoKeyException();
            chest.Unlock();
        }
        public IEnumerable<Item> SearchChest(int chestId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsClosed) throw new ClosedException();
            return chest.Search();
        }
        public void TakeItemFromChest(int chestId, int itemId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            Item item = _getItemByIdRepository.GetItem(itemId, chest.Items);
            _checkItemService.CheckItem(item, _sessionService);
            chest.RemoveItem(item);
        }
        public void TakeAllItemsFromChest(int chestId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            List<Item> carryableItems = chest.Items.Where(i => i.IsCarryable == true).ToList();
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                _checkItemService.CheckItem(item, _sessionService);
                chest.RemoveItem(item);
            }
            //chest.Items.RemoveAll(x => x.IsCarryable);
        }
    }
}