using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Other;
using TextGame.Presentation.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface IRoomControllerService :
        IGetCurrentRoomService, IGetRoomService, IGameInfoService, IGetEnemyService, ICombatService
    {
        void GoNextRoom();
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