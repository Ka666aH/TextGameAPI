using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.Entities.GameObjects.Items;

namespace TextGame.Application.Services
{
    public class GetItemService : IGetItemService
    {
        public Item GetItem(int itemId, IEnumerable<Item> items)
        {
            Item? item = items.FirstOrDefault(i => i.Id == itemId);
            return item ?? throw new NullItemIdException();
        }
    }
}