using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public class BigRoom : Room
    {
        public BigRoom(int id)
            : base(id, RoomsLabeles.BigRoomName, RoomsLabeles.BigRoomDescription) { }
        private BigRoom() { }
    }
}