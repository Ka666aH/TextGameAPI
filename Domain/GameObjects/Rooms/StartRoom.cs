using TextGame.Domain.GameText;
namespace TextGame.Domain.GameObjects.Rooms
{
    public class StartRoom : Room
    {
        public StartRoom()
            : base(RoomsLabeles.StartRoomName, RoomsLabeles.StartRoomDescription, 0)
        {
            IsDiscovered = true;
        }
    }
}
