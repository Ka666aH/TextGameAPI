using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
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
        public void GenerateContent(Room room)
        {
            switch (room)
            {
                case SmallRoom smallRoom: GenerateSmallRoomContent(smallRoom); break;
                case BigRoom bigRoom: GenerateBigRoomContent(bigRoom); break;
                case Shop shop: GenerateShopContent(shop); break;
            }
            if (room is not Shop && room is not StartRoom && room is not EndRoom) GenerateEnemy(room);
        }
        private void GenerateSmallRoomContent(SmallRoom room)
        {
            for (int i = 0; i < GameBalance.SmallRoomItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateRoomItem();
                if (item != null) room.AddItem(item);
            }
        }
        private void GenerateBigRoomContent(BigRoom room)
        {
            for (int i = 0; i < GameBalance.BigRoomItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateRoomItem();
                if (item != null) room.AddItem(item);
            }
        }
        private void GenerateShopContent(Shop room)
        {
            for (int i = 0; i < GameBalance.ShopItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateShopItem();
                if (item == null) continue;
                item.AddStoreMargin();
                room.AddItem(item);
            }
        }
        private void GenerateEnemy(Room room)
        {
            //Логика создания врагов
            //Формирование списка взвешенного выбора
            var options = new List<(int Weight, Func<Enemy?> Create)>
            {
                (GameBalance.CalculateNoneWeight(room.Number),            () => null),
                (GameBalance.CalculateSkeletorWeight(room.Number),        _enemyFactory.CreateSkeletor),
                (GameBalance.CalculateSkeletorArcherWeight(room.Number),  _enemyFactory.CreateSkeletorArcher),
                (GameBalance.CalculateDeadmanWeight(room.Number),         _enemyFactory.CreateDeadman),
                (GameBalance.CalculateGhostWeight(room.Number),           _enemyFactory.CreateGhost),
                (GameBalance.CalculateLichWeight(room.Number),            _enemyFactory.CreateLich),
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