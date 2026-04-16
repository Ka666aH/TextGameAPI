using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Rooms
{
    public class EmptyRoom : Room
    {
        public EmptyRoom(int number) : base(RoomsLabeles.EmptyRoomName, RoomsLabeles.EmptyRoomDescription, number) { }
    }
}
