using TextGame.Domain.Entities.GameObjects.Items;

namespace TextGame.Application.Interfaces.Services
{
    public interface ICheckItemService
    {
        void CheckItem(Item item);
    }
}