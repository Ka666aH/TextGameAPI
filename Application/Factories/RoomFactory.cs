using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Application.Factories
{
    public class RoomFactory : IRoomFactory
    {
        private readonly IRoomIdService _roomIdService;

        public RoomFactory(IRoomIdService roomIdService)
        {
            _roomIdService = roomIdService;
        }

        public StartRoom CreateStartRoom() => new();
        public EndRoom CreateEndRoom() => new(_roomIdService.Next());
        public EmptyRoom CreateEmptyRoom() => new(_roomIdService.Next());
        public SmallRoom CreateSmallRoom() => new(_roomIdService.Next());
        public BigRoom CreateBigRoom() => new(_roomIdService.Next());
        public Shop CreateShopRoom() => new(_roomIdService.Next());
    }
}