using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public class EndRoom : Room
    {
        public EndRoom(int id) 
            : base(id, RoomsLabeles.EndRoomName, RoomsLabeles.EndRoomDescription) { }
        private EndRoom() { }
    }
}
