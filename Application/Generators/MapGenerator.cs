using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain;
using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Generators
{
    public class MapGenerator : IMapGenerator
    {
        private readonly IRoomFactory _roomFactory;
        private readonly IRoomContentGenerator _roomContentGenerator;
        public MapGenerator(IRoomFactory roomFactory, IRoomContentGenerator roomContentGenerator) 
        {
            _roomFactory = roomFactory;
            _roomContentGenerator = roomContentGenerator;
        }
        public List<Room> Generate(IGameSessionService sessionService)
        {
            var rooms = new List<Room> { _roomFactory.CreateStartRoom() };
            var options = new (int Weight, Func<Room> Create)[]
            {
            (GameBalance.EmptyRoomWeight, () => _roomFactory.CreateEmptyRoom(sessionService.NextRoomNumber())),
            (GameBalance.SmallRoomWeight, () => _roomFactory.CreateSmallRoom(sessionService.NextRoomNumber())),
            (GameBalance.BigRoomWeight, () => _roomFactory.CreateBigRoom(sessionService.NextRoomNumber())),
            (GameBalance.ShopWeight, () => _roomFactory.CreateShopRoom(sessionService.NextRoomNumber()))
            };
            int baseWeightSum = options.Sum(x => x.Weight);

            while (rooms.Last() is not EndRoom)
            {
                int endRoomWeight = sessionService.RoomCounter;
                int totalWeight = baseWeightSum + endRoomWeight;

                int roll = Random.Shared.Next(totalWeight);

                if (roll >= baseWeightSum)
                {
                    Room room = (_roomFactory.CreateEndRoom(sessionService.NextRoomNumber()));
                    _roomContentGenerator.GenerateContent(room,sessionService);
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
                            _roomContentGenerator.GenerateContent(room, sessionService);
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