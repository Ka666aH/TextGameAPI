using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class CheckItemService : ICheckItemService
    {
        public void CheckItem(Item item, IGameSessionService sessionService)
        {
            if (!item.IsCarryable) throw new UncarryableException();
            if (item is BagOfCoins bagOfCoins) sessionService.AddCoins((int)bagOfCoins.Cost!);
            else if (item is Key) sessionService.AddKeys(1);
            else sessionService.AddItemToInventory(item);
        }
    }
}