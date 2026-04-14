using TextGame.Domain.GameObjects.Items;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGetItemService
    {
        Item GetItem(int itemId, IEnumerable<Item> items);
    }
}