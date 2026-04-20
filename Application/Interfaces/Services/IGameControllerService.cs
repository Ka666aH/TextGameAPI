using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Presentation.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGameControllerService : IInventoryService, IGameInfoService
    {
        void Start();
        IEnumerable<Item> GetInventory();
        int GetCoins();
        int GetKeys();
        List<MapRoomDTO> GetMap();
        void UseInventoryItem(int itemId);

        Room GetCurrentRoom();

    }
}