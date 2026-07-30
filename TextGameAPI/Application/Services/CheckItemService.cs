using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Other;

namespace TextGame.Application.Services
{
    public class CheckItemService : ICheckItemService
    {
        private readonly IStateService _stateService;

        public CheckItemService(IStateService stateService)
        {
            _stateService = stateService;
        }

        public void CheckItem(Item item)
        {
            if (!item.IsCarryable) throw new UncarryableException();
            switch (item)
            {
                case BagOfCoins: _stateService.AddCoins((int)item.Cost!); break;
                case Key: _stateService.AddKeys(1); break;
                default: _stateService.AddItemToInventory(item); break;
            }
        }
    }
}