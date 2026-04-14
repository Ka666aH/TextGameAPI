using TextGame.Domain.GameObjects.Items;

namespace TextGame.Application.Interfaces.Services
{
    public interface IRoomControllerService :
        IGetCurrentRoomService, IChestService, IGetRoomService, IGameInfoService, IGetEnemyService, ICombatService
    {
        void GoNextRoom();
        IEnumerable<Item> Search();
        void TakeItem(int itemId);
        void TakeAllItems();
        void BuyItem(int itemId);
    }
}