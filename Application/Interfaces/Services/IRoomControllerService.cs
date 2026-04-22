using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Services
{
    public interface IRoomControllerService :
        IGameInfoService, IGetEnemyService, ICombatService
    {
        Room GetCurrentRoom();
        Room GoNextRoom();
        Room GoToRoom(int roomId);
        List<Item> Search();
        void TakeItem(int itemId);
        void TakeAllItems();
        void BuyItem(int itemId);

        BattleLog HitChest(int chestId);
        Chest UnlockChest(int chestId);
        void OpenChest(int chestId);
        List<Item> SearchChest(int chestId);
        void TakeItemFromChest(int chestId, int itemId);
        void TakeAllItemsFromChest(int chestId);

    }
}