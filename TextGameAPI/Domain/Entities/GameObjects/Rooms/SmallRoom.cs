using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public class SmallRoom : Room
    {
        public SmallRoom(int id) 
            : base(id, RoomsLabels.SmallRoomName, RoomsLabels.SmallRoomDescription) { }
        private SmallRoom() { }
    }
}
