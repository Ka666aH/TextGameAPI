using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public class EmptyRoom : Room
    {
        public EmptyRoom(int id) 
            : base(id, RoomsLabels.EmptyRoomName, RoomsLabels.EmptyRoomDescription) { }
        private EmptyRoom() { }
    }
}
