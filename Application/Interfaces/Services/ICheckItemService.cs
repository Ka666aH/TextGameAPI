using TextGame.Domain.GameObjects.Items;

namespace TextGame.Application.Interfaces.Services
{
    public interface ICheckItemService
    {
        void CheckItem(Item item, IGameSessionService sessionService);
    }
}