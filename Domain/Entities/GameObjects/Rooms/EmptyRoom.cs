using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public class EmptyRoom : Room
    {
        public EmptyRoom(int number) 
            : base(RoomsLabeles.EmptyRoomName, RoomsLabeles.EmptyRoomDescription, number) { }
        private EmptyRoom() { }
    }
}
