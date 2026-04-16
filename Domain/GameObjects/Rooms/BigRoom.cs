using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Rooms
{
    public class BigRoom : Room
    {
        public BigRoom(int number)
            : base(RoomsLabeles.BigRoomName, RoomsLabeles.BigRoomDescription, number) { }
    }
}