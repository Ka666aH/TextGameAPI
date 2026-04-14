using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain;
using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Generators
{
    public class RoomContentGenerator : IRoomContentGenerator
    {
        private readonly IItemFactory _itemFactory;
        private readonly IEnemyFactory _enemyFactory;
        public RoomContentGenerator(IItemFactory itemFactory, IEnemyFactory enemyFactory)
        {
            _itemFactory = itemFactory;
            _enemyFactory = enemyFactory;
        }
        public void GenerateContent(Room room, IGameSessionService sessionService)
        {
            switch (room)
            {
                case SmallRoom smallRoom: GenerateSmallRoomContent(smallRoom, sessionService); break;
                case BigRoom bigRoom: GenerateBigRoomContent(bigRoom, sessionService); break;
                case Shop shop: GenerateShopContent(shop, sessionService); break;
            }
            if (room is not Shop && room is not StartRoom && room is not EndRoom) GenerateEnemy(room, sessionService);
        }
        private void GenerateSmallRoomContent(SmallRoom room, IGameSessionService sessionService)
        {
            for (int i = 0; i < GameBalance.SmallRoomItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateRoomItem(sessionService);
                if (item != null) room.AddItem(item);
            }
        }
        private void GenerateBigRoomContent(BigRoom room, IGameSessionService sessionService)
        {
            for (int i = 0; i < GameBalance.BigRoomItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateRoomItem(sessionService);
                if (item != null) room.AddItem(item);
            }
        }
        private void GenerateShopContent(Shop room, IGameSessionService sessionService)
        {
            for (int i = 0; i < GameBalance.ShopItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateShopItem(sessionService);
                if (item == null) continue;
                item.AddStoreMargin();
                room.AddItem(item);
            }
        }
        private void GenerateEnemy(Room room, IGameSessionService sessionService)
        {
            //Логика создания врагов
            //Формирование списка взвешенного выбора
            var options = new List<(int Weight, Func<Enemy?> Create)>
            {
                (GameBalance.CalculateNoneWeight(room.Number),            () => null),
                (GameBalance.CalculateSkeletorWeight(room.Number),        () => _enemyFactory.CreateSkeletor(sessionService)),
                (GameBalance.CalculateSkeletorArcherWeight(room.Number),  () => _enemyFactory.CreateSkeletorArcher(sessionService)),
                (GameBalance.CalculateDeadmanWeight(room.Number),         () => _enemyFactory.CreateDeadman(sessionService)),
                (GameBalance.CalculateGhostWeight(room.Number),           () => _enemyFactory.CreateGhost(sessionService)),
                (GameBalance.CalculateLichWeight(room.Number),            () => _enemyFactory.CreateLich(sessionService)),
            };
            //Выбор
            int weightsSum = options.Sum(x => x.Weight);
            int roll = Random.Shared.Next(weightsSum);
            int accumulated = 0;

            foreach (var option in options)
            {
                if (roll < accumulated + option.Weight)
                {
                    var enemy = option.Create();
                    if (enemy is not null) room.AddEnemy(enemy);
                    break;
                }
                accumulated += option.Weight;
            }
        }
    }
}