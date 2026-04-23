using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public class Shop : Room
    {
        public Shop(int id)
            : base(id, RoomsLabeles.ShopName, RoomsLabeles.ShopDescription) { }
        private Shop() { }
    }
}
