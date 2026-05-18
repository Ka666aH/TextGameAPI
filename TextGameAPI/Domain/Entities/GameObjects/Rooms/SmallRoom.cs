using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public class SmallRoom : Room
    {
        public SmallRoom(int id) 
            : base(id, RoomsLabeles.SmallRoomName, RoomsLabeles.SmallRoomDescription) { }
        private SmallRoom() { }
    }
}
