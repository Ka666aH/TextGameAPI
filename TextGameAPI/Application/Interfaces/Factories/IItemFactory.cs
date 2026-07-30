using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities.GameObjects.Items;

namespace TextGame.Application.Interfaces.Factories
{
    public interface IItemFactory
    {
        public Item? CreateRoomItem();
        public Item? CreateChestItem();
        public Item? CreateShopItem();
    }
}