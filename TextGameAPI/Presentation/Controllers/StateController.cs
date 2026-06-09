using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Orchestrators;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Presentation.Attributes;
using TextGame.Presentation.Mappers;
using TextGame.Presentation.Options;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Authorize(Policy = Policies.RequireSession)]
    [RequireSessionOwnership]
    [RequireStateLoaded]
    [Route("state")]
    public class StateController : ControllerBase
    {
        private readonly IStateOrchestrator _stateOrchestrator;

        public StateController(IStateOrchestrator stateOrchestrator)
        {
            _stateOrchestrator = stateOrchestrator;
        }

        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            return Ok(_stateOrchestrator.GetGameInfo());
        }

        [HttpGet("map")]
        public IActionResult GetMap()
        {
            return Ok(_stateOrchestrator.GetMap());
        }

        [HttpGet("coins")]
        public IActionResult GetCoins()
        {
            return Ok(_stateOrchestrator.GetCoins());
        }

        [HttpGet("keys")]
        public IActionResult GetKeys()
        {
            return Ok(_stateOrchestrator.GetKeys());
        }

        [HttpGet("inventory")]
        public IActionResult GetInventory()
        {
            var items = _stateOrchestrator.GetInventory();
            return Ok(items.ToDTO());
        }

        [HttpGet("inventory/{itemId}")]
        public IActionResult GetInventoryItem(int itemId)
        {
            var item = _stateOrchestrator.GetInventoryItem(itemId);
            return Ok(item.ToDTO());
        }

        [HttpPost("inventory/{itemId}/sell")]
        [AutoCache]
        public IActionResult SellInventoryItem(int itemId)
        {
            _stateOrchestrator.SellInventoryItem(itemId);
            return GetInfo();
        }

        [HttpPost("inventory/{itemId}/use")]
        [AutoCache]
        public IActionResult UseInventoryItem(int itemId)
        {
            _stateOrchestrator.UseInventoryItem(itemId);
            return GetInfo();
        }

        [HttpGet("equipment")]
        public IActionResult GetEquipment()
        {
            var equip = _stateOrchestrator.GetEquipment();
            return Ok(equip.ToDTO());
        }

        [HttpPost("inventory/{itemId}/equip")]
        [AutoCache]
        public IActionResult EquipInventoryItem(int itemId)
        {
            _stateOrchestrator.EquipInventoryItem(itemId);
            return GetEquipment();
        }

        [HttpPost("equipment/weapon/unequip")]
        [AutoCache]
        public IActionResult UnequipWeapon()
        {
            _stateOrchestrator.UnequipWeapon();
            return GetEquipment();
        }

        [HttpPost("equipment/helm/unequip")]
        [AutoCache]
        public IActionResult UnequipHelm()
        {
            _stateOrchestrator.UnequipHelm();
            return GetEquipment();
        }

        [HttpPost("equipment/chestplate/unequip")]
        [AutoCache]
        public IActionResult UnequipChestplate()
        {
            _stateOrchestrator.UnequipChestplate();
            return GetEquipment();
        }

        [HttpPost("rooms/next")]
        [AutoCache]
        public IActionResult GoNextRoom()
        {
            var room = _stateOrchestrator.GoNextRoom();
            return Ok(room.ToDTO());
        }

        [HttpPost("rooms/{roomId}")]
        [AutoCache]
        public IActionResult GoRoom(int roomId)
        {
            var room = _stateOrchestrator.GoToRoom(roomId);
            return Ok(room.ToDTO());
        }

        [HttpGet("rooms/current")]
        public IActionResult GetCurrentRoom()
        {
            var room = _stateOrchestrator.GetCurrentRoom();
            return Ok(room.ToDTO());
        }

        [HttpPost("rooms/current/items")]
        [AutoCache]
        public IActionResult Search()
        {
            var items = _stateOrchestrator.Search();
            return Ok(items.ToDTO());
        }

        [HttpPost("rooms/current/items/{itemId}/take")]
        [AutoCache]
        public IActionResult TakeItem(int itemId)
        {
            _stateOrchestrator.TakeItem(itemId);
            var info = _stateOrchestrator.GetGameInfo();
            return Ok(info);
        }

        [HttpPost("rooms/current/items/takeall")]
        [AutoCache]
        public IActionResult TakeAllItems()
        {
            _stateOrchestrator.TakeAllItems();
            var info = _stateOrchestrator.GetGameInfo();
            return Ok(info);
        }

        [HttpPost("rooms/current/items/{itemId}/buy")]
        [AutoCache]
        public IActionResult BuyItem(int itemId)
        {
            _stateOrchestrator.BuyItem(itemId);
            var info = _stateOrchestrator.GetGameInfo();
            return Ok(info);
        }

        #region CHEST

        [HttpPost("rooms/current/items/{chestId}/chest/hit")]
        [AutoCache]
        public IActionResult HitChest(int chestId)
        {
            return Ok(_stateOrchestrator.HitChest(chestId));
        }

        [HttpPost("rooms/current/items/{chestId}/chest/open")]
        [AutoCache]
        public IActionResult OpenChest(int chestId)
        {
            _stateOrchestrator.OpenChest(chestId);
            var items = _stateOrchestrator.SearchChest(chestId);
            return Ok(items.ToDTO());
        }

        [HttpPost("rooms/current/items/{chestId}/chest/unlock")]
        [AutoCache]
        public IActionResult UnlockChest(int chestId)
        {
            var chest = _stateOrchestrator.UnlockChest(chestId);
            return Ok(chest.ToDTO());
        }

        [HttpPost("rooms/current/items/{chestId}/chest/items")]
        public IActionResult SearchChest(int chestId)
        {
            var items = _stateOrchestrator.SearchChest(chestId);
            return Ok(items.ToDTO());
        }

        [HttpPost("rooms/current/items/{chestId}/chest/items/{itemId}/take")]
        [AutoCache]
        public IActionResult TakeItemFromChest(int chestId, int itemId)
        {
            _stateOrchestrator.TakeItemFromChest(chestId, itemId);
            var info = _stateOrchestrator.GetGameInfo();
            return Ok(info);
        }

        [HttpPost("rooms/current/items/{chestId}/chest/items/takeall")]
        [AutoCache]
        public IActionResult TakeAllItemsFromChest(int chestId)
        {
            _stateOrchestrator.TakeAllItemsFromChest(chestId);
            var info = _stateOrchestrator.GetGameInfo();
            return Ok(info);
        }

        #endregion
        #region ENEMIES

        [HttpGet("rooms/current/enemy")]
        public IActionResult GetEnemy()
        {
            Enemy enemy = _stateOrchestrator.GetEnemy();
            return Ok(enemy.ToDTO());
        }

        [HttpPost("rooms/current/enemy/attack")]
        [AutoCache]
        public IActionResult AttackEnemy()
        {
            List<BattleLog> battleLogs = [
                _stateOrchestrator.DealDamage(),
                _stateOrchestrator.GetDamage()];
            return Ok(battleLogs);
        }

        #endregion
    }
}
