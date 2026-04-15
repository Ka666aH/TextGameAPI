using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Factories
{
    public class RoomFactory : IRoomFactory
    {
        private readonly IRoomIdService _roomIdService;

        public RoomFactory(IRoomIdService roomIdService)
        {
            _roomIdService = roomIdService;
        }

        public StartRoom CreateStartRoom() => new StartRoom();
        public EndRoom CreateEndRoom() => new EndRoom(_roomIdService.Next());
        public EmptyRoom CreateEmptyRoom() => new EmptyRoom(_roomIdService.Next());
        public SmallRoom CreateSmallRoom() => new SmallRoom(_roomIdService.Next());
        public BigRoom CreateBigRoom() => new BigRoom(_roomIdService.Next());
        public Shop CreateShopRoom() => new Shop(_roomIdService.Next());
    }
}