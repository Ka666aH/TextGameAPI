using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameObjects.Items.Other;

namespace TextGame.Application.Services
{
    public class CheckItemService : ICheckItemService
    {
        private readonly IGameSessionService _gameSessionService;

        public CheckItemService(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        public void CheckItem(Item item)
        {
            if (!item.IsCarryable) throw new UncarryableException();
            switch (item)
            {
                case BagOfCoins: _gameSessionService.AddCoins((int)item.Cost!); break;
                case Key: _gameSessionService.AddKeys(1); break;
                default: _gameSessionService.AddItemToInventory(item); break;
            }
        }
    }
}