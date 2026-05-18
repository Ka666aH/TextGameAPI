using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Application.Generators
{
    public class MapGenerator : IMapGenerator
    {
        private readonly IRoomFactory _roomFactory;
        private readonly IRoomContentGenerator _roomContentGenerator;
        private readonly IRoomIdService _roomIdService;
        public MapGenerator(IRoomFactory roomFactory, IRoomContentGenerator roomContentGenerator, IRoomIdService roomIdService)
        {
            _roomFactory = roomFactory;
            _roomContentGenerator = roomContentGenerator;
            _roomIdService = roomIdService;
        }
        public List<Room> Generate()
        {
            var rooms = new List<Room> { _roomFactory.CreateStartRoom() };
            var options = new (int Weight, Func<Room> Create)[]
            {
            (GameBalance.EmptyRoomWeight, _roomFactory.CreateEmptyRoom),
            (GameBalance.SmallRoomWeight, _roomFactory.CreateSmallRoom),
            (GameBalance.BigRoomWeight, _roomFactory.CreateBigRoom),
            (GameBalance.ShopWeight, _roomFactory.CreateShopRoom)
            };
            int baseWeightSum = options.Sum(x => x.Weight);

            while (rooms.Last() is not EndRoom)
            {
                int endRoomWeight = _roomIdService.Current();
                int totalWeight = baseWeightSum + endRoomWeight;

                int roll = Random.Shared.Next(totalWeight);

                if (roll >= baseWeightSum)
                {
                    Room room = _roomFactory.CreateEndRoom();
                    _roomContentGenerator.GenerateContent(room);
                    rooms.Add(room);
                }
                else
                {
                    int acc = 0;
                    foreach (var (weight, create) in options)
                    {
                        if (roll < acc + weight)
                        {
                            Room room = (create());
                            _roomContentGenerator.GenerateContent(room);
                            rooms.Add(room);
                            break;
                        }
                        acc += weight;
                    }
                }
            }
            return rooms;
        }
    }
}