using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
using TextGame.Domain;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;

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
        public void GenerateContent(Room room, Random random)
        {
            switch (room)
            {
                case SmallRoom smallRoom: GenerateSmallRoomContent(smallRoom, random); break;
                case BigRoom bigRoom: GenerateBigRoomContent(bigRoom, random); break;
                case Shop shop: GenerateShopContent(shop, random); break;
            }
            if (room is not Shop && room is not StartRoom && room is not EndRoom) GenerateEnemy(room, random);
        }
        private void GenerateSmallRoomContent(SmallRoom room, Random random)
        {
            for (int i = 0; i < GameBalance.SmallRoomItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateRoomItem(random);
                if (item != null) room.AddItem(item);
                //if (item is Chest chest && chest.Mimic != null) room.AddEnemy(chest.Mimic);
            }
        }
        private void GenerateBigRoomContent(BigRoom room, Random random)
        {
            for (int i = 0; i < GameBalance.BigRoomItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateRoomItem(random);
                if (item != null) room.AddItem(item);
                //if (item is Chest chest && chest.Mimic != null) room.AddEnemy(chest.Mimic);
            }
        }
        private void GenerateShopContent(Shop room, Random random)
        {
            for (int i = 0; i < GameBalance.ShopItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateShopItem(random);
                if (item == null) continue;
                item.AddStoreMargin();
                room.AddItem(item);
            }
        }
        private void GenerateEnemy(Room room, Random random)
        {
            //Логика создания врагов
            //Формирование списка взвешенного выбора
            var options = new List<(int Weight, Func<Enemy?> Create)>
            {
                (GameBalance.CalculateNoneWeight(room.Id),            () => null),
                (GameBalance.CalculateSkeletorWeight(room.Id),        () => _enemyFactory.CreateSkeletor(random)),
                (GameBalance.CalculateSkeletorArcherWeight(room.Id),  () => _enemyFactory.CreateSkeletorArcher(random)),
                (GameBalance.CalculateDeadmanWeight(room.Id),         () => _enemyFactory.CreateDeadman(random)),
                (GameBalance.CalculateGhostWeight(room.Id),           () => _enemyFactory.CreateGhost(random)),
                (GameBalance.CalculateLichWeight(room.Id),            () => _enemyFactory.CreateLich(random)),
            };
            //Выбор
            int weightsSum = options.Sum(x => x.Weight);
            int roll = random.Next(weightsSum);
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
