using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public class EndRoom : Room
    {
        public EndRoom(int id) 
            : base(id, RoomsLabels.EndRoomName, RoomsLabels.EndRoomDescription) { }
        private EndRoom() { }
    }
}
