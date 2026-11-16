using TextGame.Domain.Entities.GameObjects.Items;

namespace TextGame.Application.Interfaces.Factories
{
    public interface IItemFactory
    {
        public Item? CreateRoomItem(Random random);
        public Item? CreateChestItem(Random random);
        public Item? CreateShopItem(Random random);
    }
}
