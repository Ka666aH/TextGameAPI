using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Wands;
using TextGame.Domain.Entities.GameObjects.Items.Heals;
using TextGame.Domain.Entities.GameObjects.Items;

namespace TextGame.Application.Factories
{
    public class ItemFactory : IItemFactory
    {
        private readonly IItemIdService _itemIdService;
        private readonly IEnemyFactory _enemyFactory;
        private readonly IRoomIdService _roomIdService;

        public ItemFactory(IEnemyFactory enemyFactory, IRoomIdService roomIdService, IItemIdService itemIdService)
        {
            _enemyFactory = enemyFactory;
            _roomIdService = roomIdService;
            _itemIdService = itemIdService;
        }

        private Item? SelectRandom(List<(int Weight, Func<Item?> Creator)> options, Random random)
        {
            // Суммируем все веса
            int totalWeight = options.Sum(option => option.Weight);

            // Защита от пустого списка или нулевых весов
            if (totalWeight <= 0)
                return null;

            // Генерируем случайное число в диапазоне [0, totalWeight)
            int roll = random.Next(totalWeight);
            int accumulated = 0;

            // Ищем предмет, в чей диапазон попало число
            foreach (var (weight, creator) in options)
            {
                if (roll < accumulated + weight)
                    return creator(); // Создаём предмет только когда он выбран

                accumulated += weight;
            }

            // На случай ошибок округления (очень редко)
            return options[^1].Creator();
        }
        private void AddWeightedGroup(
    List<(int Weight, Func<Item?> Creator)> options,
    int roomId,
    double groupWeightWeight,
    params (Func<int, double> WeightCalc, Func<Item?> Creator)[] items)
        {
            // 1. Считаем "сырые" веса для предметов в группе (без отрицательных значений)
            var rawWeights = items.Select(item => Math.Max(0, item.WeightCalc(roomId))).ToArray();

            // 2. Сумма всех весов в группе
            double groupSum = rawWeights.Sum();

            // 3. Если группа пустая - пропускаем
            if (groupSum <= 0)
                return;

            // 4. Добавляем каждый предмет с нормализованным весом
            for (int i = 0; i < items.Length; i++)
            {
                double rawWeight = rawWeights[i];
                if (rawWeight <= 0)
                    continue; // Пропускаем предметы с нулевым весом

                // Абсолютный вес = (вес_группы_в_процентах) * (вес_предмета / сумма_весов_группы)
                int absoluteWeight = (int)(groupWeightWeight * (rawWeight / groupSum));

                if (absoluteWeight > 0)
                    options.Add((absoluteWeight, items[i].Creator));
            }
        }
        public Item? CreateRoomItem(Random random)
        {
            int roomId = _roomIdService.Current();
            var options = new List<(int Weight, Func<Item?> Creator)>();

            // Группа "Прочее"
            AddWeightedGroup(options, roomId, GameBalance.RoomOtherWeight,
                // Относительные веса внутри группы (не в процентах)
                (_ => GameBalance.CalculateNoneRoomWeight(), () => null),
                (_ => GameBalance.CalculateKeyRoomWeight(), () => new Key(_itemIdService.Next(), roomId)),
                (_ => GameBalance.CalculateBagOfCoinsRoomWeight(), () => new BagOfCoins(_itemIdService.Next(), roomId, random)),
                (_ => GameBalance.CalculateChestRoomWeight(), () =>
                {
                    //create items
                    var itemsInChest = random.Next(GameBalance.MinChestItemsAmount, GameBalance.MaxChestItemsAmount + 1);
                    var items = new List<Item>(itemsInChest);
                    for (int i = 0; i < itemsInChest; i++)
                    {
                        Item? item = CreateChestItem(random);
                        if (item != null) items.Add(item);
                    }
                    //create mimic
                    var mimic = random.Next(GameBalance.ChestDivider) < GameBalance.MimicProbabilityDenominator ? _enemyFactory.CreateMimic(random) : null;

                    return new Chest(_itemIdService.Next(), roomId, items, mimic, random);
                }
            )
            );

            // Группа "Оружие" 
            AddWeightedGroup(options, roomId, GameBalance.RoomWeaponWeight,
                (r => GameBalance.CalculateRustSwordRoomWeight(r), () => new RustSword(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateIronSwordRoomWeight(r), () => new IronSword(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateSilverSwordRoomWeight(r), () => new SilverSword(_itemIdService.Next(), roomId, false, random)),
                (_ => GameBalance.CalculateGlassSwordRoomWeight(), () => new GlassSword(_itemIdService.Next(), roomId, false, random)),

                (r => GameBalance.CalculateMagicWandRoomWeight(r), () => new MagicWand(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateRandomWandRoomWeight(r), () => new RandomWand(_itemIdService.Next(), roomId, false, random))
            );

            // Группа "Броня"
            AddWeightedGroup(options, roomId, GameBalance.RoomArmorWeight,
                (r => GameBalance.CalculateWoodenBucketRoomWeight(r), () => new WoodenBucket(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateLeatherHelmRoomWeight(r), () => new LeatherHelm(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateIronHelmRoomWeight(r), () => new IronHelm(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateLeatherVestRoomWeight(r), () => new LeatherVest(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateIronCuirassRoomWeight(r), () => new IronCuirass(_itemIdService.Next(), roomId, false, random))
            );

            // Группа "Зелья"
            AddWeightedGroup(options, roomId, GameBalance.RoomHealWeight,
                (r => GameBalance.CalculateBandageRoomWeight(r), () => new Bandage(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateRegenPotionRoomWeight(r), () => new RegenPotion(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculatePowerPotionRoomWeight(r), () => new PowerPotion(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateRandomPotionRoomWeight(r), () => new RandomPotion(_itemIdService.Next(), roomId, false, random))
            );

            return SelectRandom(options, random);
        }
        public Item? CreateChestItem(Random random)
        {
            int roomId = _roomIdService.Current();
            var options = new List<(int Weight, Func<Item?> Creator)>();

            AddWeightedGroup(options, roomId, GameBalance.ChestOtherWeight,
                (_ => GameBalance.CalculateKeyChestWeight(), () => new Key(_itemIdService.Next(), roomId)),
                (_ => GameBalance.CalculateBagOfCoinsChestWeight(), () => new BagOfCoins(_itemIdService.Next(), roomId, random)),
                (_ => GameBalance.CalculateMapChestWeight(), () => new Map(_itemIdService.Next(), roomId))
            );
            AddWeightedGroup(options, roomId, GameBalance.ChestWeaponWeight,
                (r => GameBalance.CalculateRustSwordChestWeight(r), () => new RustSword(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateIronSwordChestWeight(r), () => new IronSword(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateSilverSwordChestWeight(r), () => new SilverSword(_itemIdService.Next(), roomId, false, random)),
                (_ => GameBalance.CalculateGlassSwordChestWeight(), () => new GlassSword(_itemIdService.Next(), roomId, false, random)),

                (r => GameBalance.CalculateMagicWandChestWeight(r), () => new MagicWand(_itemIdService.Next(), roomId, false, random)),
                (_ => GameBalance.CalculateRandomWandChestWeight(), () => new RandomWand(_itemIdService.Next(), roomId, false, random))
            );
            AddWeightedGroup(options, roomId, GameBalance.ChestArmorWeight,
                (r => GameBalance.CalculateLeatherHelmChestWeight(r), () => new LeatherHelm(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateIronHelmChestWeight(r), () => new IronHelm(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateLeatherVestChestWeight(r), () => new LeatherVest(_itemIdService.Next(), roomId, false, random)),
                (r => GameBalance.CalculateIronCuirassChestWeight(r), () => new IronCuirass(_itemIdService.Next(), roomId, false, random))
            );
            AddWeightedGroup(options, roomId, GameBalance.ChestHealWeight,
                (_ => GameBalance.CalculateRegenPotionChestWeight(), () => new RegenPotion(_itemIdService.Next(), roomId, false, random)),
                (_ => GameBalance.CalculatePowerPotionChestWeight(), () => new PowerPotion(_itemIdService.Next(), roomId, false, random)),
                (_ => GameBalance.CalculateRandomPotionChestWeight(), () => new RandomPotion(_itemIdService.Next(), roomId, false, random))
            );
            return SelectRandom(options, random);
        }
        public Item? CreateShopItem(Random random)
        {
            int roomId = _roomIdService.Current();
            var options = new List<(int Weight, Func<Item?> Creator)>();

            AddWeightedGroup(options, roomId, GameBalance.ShopOtherWeight,
                (_ => GameBalance.CalculateKeyShopWeight(), () => new Key(_itemIdService.Next(), roomId)),
                (_ => GameBalance.CalculateMapShopWeight(), () => new Map(_itemIdService.Next(), roomId))
            );
            AddWeightedGroup(options, roomId, GameBalance.ShopWeaponWeight,
                (r => GameBalance.CalculateRustSwordShopWeight(r), () => new RustSword(_itemIdService.Next(), roomId, true, random)),
                (r => GameBalance.CalculateIronSwordShopWeight(r), () => new IronSword(_itemIdService.Next(), roomId, true, random)),
                (r => GameBalance.CalculateSilverSwordShopWeight(r), () => new SilverSword(_itemIdService.Next(), roomId, true, random)),

                (_ => GameBalance.CalculateMagicWandShopWeight(), () => new MagicWand(_itemIdService.Next(), roomId, true, random)),
                (_ => GameBalance.CalculateRandomWandShopWeight(), () => new RandomWand(_itemIdService.Next(), roomId, true, random))
            );
            AddWeightedGroup(options, roomId, GameBalance.ShopArmorWeight,
                (r => GameBalance.CalculateWoodenBucketShopWeight(r), () => new WoodenBucket(_itemIdService.Next(), roomId, true, random)),
                (r => GameBalance.CalculateLeatherHelmShopWeight(r), () => new LeatherHelm(_itemIdService.Next(), roomId, true, random)),
                (r => GameBalance.CalculateIronHelmShopWeight(r), () => new IronHelm(_itemIdService.Next(), roomId, true, random)),
                (r => GameBalance.CalculateLeatherVestShopWeight(r), () => new LeatherVest(_itemIdService.Next(), roomId, true, random)),
                (r => GameBalance.CalculateIronCuirassShopWeight(r), () => new IronCuirass(_itemIdService.Next(), roomId, true, random))
            );
            AddWeightedGroup(options, roomId, GameBalance.ShopHealWeight,
                (r => GameBalance.CalculateBandageShopWeight(r), () => new Bandage(_itemIdService.Next(), roomId, true, random)),
                (r => GameBalance.CalculateRegenPotionShopWeight(r), () => new RegenPotion(_itemIdService.Next(), roomId, true, random)),
                (r => GameBalance.CalculatePowerPotionShopWeight(r), () => new PowerPotion(_itemIdService.Next(), roomId, true, random)),
                (_ => GameBalance.CalculateRandomPotionShopWeight(), () => new RandomPotion(_itemIdService.Next(), roomId, true, random))
            );
            return SelectRandom(options, random);
        }
    }
}