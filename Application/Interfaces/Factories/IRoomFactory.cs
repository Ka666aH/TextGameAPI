using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Factories
{
    public interface IRoomFactory
    {
        StartRoom CreateStartRoom();
        EmptyRoom CreateEmptyRoom();
        SmallRoom CreateSmallRoom();
        BigRoom CreateBigRoom();
        Shop CreateShopRoom();
        EndRoom CreateEndRoom();
    }
}