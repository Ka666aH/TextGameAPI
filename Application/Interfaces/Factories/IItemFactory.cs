using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Items;

namespace TextGame.Application.Interfaces.Factories
{
    public interface IItemFactory
    {
        public Item? CreateRoomItem(IGameSessionService sessionService);
        public Item? CreateChestItem(IGameSessionService sessionService);
        public Item? CreateShopItem(IGameSessionService sessionService);
    }
}