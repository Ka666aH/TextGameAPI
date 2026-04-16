using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Rooms
{
    public class Shop : Room
    {
        public Shop(int number)
            : base(RoomsLabeles.ShopName, RoomsLabeles.ShopDescription, number) { }
    }
}
