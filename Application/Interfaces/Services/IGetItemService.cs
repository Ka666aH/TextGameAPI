using TextGame.Domain.Entities.GameObjects.Items;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGetItemService
    {
        Item GetItem(int itemId, IEnumerable<Item> items);
    }
}