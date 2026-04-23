using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public class SmallRoom : Room
    {
        public SmallRoom(int number) 
            : base(RoomsLabeles.SmallRoomName, RoomsLabeles.SmallRoomDescription, number) { }
        private SmallRoom() { }
    }
}
