using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public class EmptyRoom : Room
    {
        public EmptyRoom(int id) 
            : base(id, RoomsLabeles.EmptyRoomName, RoomsLabeles.EmptyRoomDescription) { }
        private EmptyRoom() { }
    }
}
