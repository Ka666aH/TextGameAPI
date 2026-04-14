using TextGame.Application.Interfaces.Factories;
using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Factories
{
    public class RoomFactory : IRoomFactory
    {
        public StartRoom CreateStartRoom() => new StartRoom(0);
        public EndRoom CreateEndRoom(int roomNumber) => new EndRoom(roomNumber);
        public EmptyRoom CreateEmptyRoom(int roomNumber) => new EmptyRoom(roomNumber);
        public SmallRoom CreateSmallRoom(int roomNumber) => new SmallRoom(roomNumber);
        public BigRoom CreateBigRoom(int roomNumber) => new BigRoom(roomNumber);
        public Shop CreateShopRoom(int roomNumber) => new Shop(roomNumber);
    }
}