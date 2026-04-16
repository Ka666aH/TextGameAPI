using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Rooms
{
    public class EndRoom : Room
    {
        public EndRoom(int number) : base(RoomsLabeles.EndRoomName, RoomsLabeles.EndRoomDescription, number) { }
    }
}
