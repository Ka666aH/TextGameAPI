using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Factories
{
    public interface IRoomFactory
    {
        StartRoom CreateStartRoom();
        EmptyRoom CreateEmptyRoom(int roomNumber);
        SmallRoom CreateSmallRoom(int roomNumber);
        BigRoom CreateBigRoom(int roomNumber);
        Shop CreateShopRoom(int roomNumber);
        EndRoom CreateEndRoom(int roomNumber);
    }
}