using TextGame.Domain.GameText;
namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public class StartRoom : Room
    {
        public StartRoom()
            : base(0, RoomsLabeles.StartRoomName, RoomsLabeles.StartRoomDescription)
        {
            IsDiscovered = true;
        }
        //private StartRoom() { }
    }
}
