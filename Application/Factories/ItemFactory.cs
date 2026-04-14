using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.GameObjects.Items.Equipments.Weapons.Swords;
using TextGame.Domain.GameObjects.Items.Equipments.Weapons.Wands;
using TextGame.Domain.GameObjects.Items.Heal;

namespace TextGame.Application.Factories
{
    public class ItemFactory : IItemFactory
    {
        private readonly IEnemyFactory _enemyFactory;
        public ItemFactory(IEnemyFactory enemyFactory)
        {

            _enemyFactory = enemyFactory;
        }

        private Item? SelectRandom(List<(int Weight, Func<Item?> Creator)> options)
        {
            // Суммируем все веса
            int totalWeight = options.Sum(option => option.Weight);

            // Защита от пустого списка или нулевых весов
            if (totalWeight <= 0)
                return null;

            // Генерируем случайное число в диапазоне [0, totalWeight)
            int roll = Random.Shared.Next(totalWeight);
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
        public Item? CreateRoomItem(IGameSessionService sessionService)
        {
            int roomId = sessionService.RoomCounter;

            var options = new List<(int Weight, Func<Item?> Creator)>();

            // Группа "Прочее"
            AddWeightedGroup(options, roomId, GameBalance.RoomOtherWeight,
                // Относительные веса внутри группы (не в процентах)
                (_ => GameBalance.CalculateNoneRoomWeight(), () => null),
                (_ => GameBalance.CalculateKeyRoomWeight(), () => new Key(sessionService.NextItemId(), roomId)),
                (_ => GameBalance.CalculateBagOfCoinsRoomWeight(), () => new BagOfCoins(sessionService.NextItemId(), roomId)),
                (_ => GameBalance.CalculateChestRoomWeight(), () => new Chest(this, _enemyFactory, sessionService))
            );

            // Группа "Оружие" 
            AddWeightedGroup(options, roomId, GameBalance.RoomWeaponWeight,
                (r => GameBalance.CalculateRustSwordRoomWeight(r), () => new RustSword(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronSwordRoomWeight(r), () => new IronSword(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateSilverSwordRoomWeight(r), () => new SilverSword(sessionService.NextItemId(), roomId, false)),
                (_ => GameBalance.CalculateGlassSwordRoomWeight(), () => new GlassSword(sessionService.NextItemId(), roomId, false)),

                (r => GameBalance.CalculateMagicWandRoomWeight(r), () => new MagicWand(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateRandomWandRoomWeight(r), () => new RandomWand(sessionService.NextItemId(), roomId, false))
            );

            // Группа "Броня"
            AddWeightedGroup(options, roomId, GameBalance.RoomArmorWeight,
                (r => GameBalance.CalculateWoodenBucketRoomWeight(r), () => new WoodenBucket(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateLeatherHelmRoomWeight(r), () => new LeatherHelm(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronHelmRoomWeight(r), () => new IronHelm(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateLeatherVestRoomWeight(r), () => new LeatherVest(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronCuirassRoomWeight(r), () => new IronCuirass(sessionService.NextItemId(), roomId, false))
            );

            // Группа "Зелья"
            AddWeightedGroup(options, roomId, GameBalance.RoomHealWeight,
                (r => GameBalance.CalculateBandageRoomWeight(r), () => new Bandage(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateRegenPotionRoomWeight(r), () => new RegenPotion(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculatePowerPotionRoomWeight(r), () => new PowerPotion(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateRandomPotionRoomWeight(r), () => new RandomPotion(sessionService.NextItemId(), roomId, false))
            );

            return SelectRandom(options);
        }
        public Item? CreateChestItem(IGameSessionService sessionService)
        {
            int roomId = sessionService.RoomCounter;

            var options = new List<(int Weight, Func<Item?> Creator)>();

            AddWeightedGroup(options, roomId, GameBalance.ChestOtherWeight,
                (_ => GameBalance.CalculateKeyChestWeight(), () => new Key(sessionService.NextItemId(), roomId)),
                (_ => GameBalance.CalculateBagOfCoinsChestWeight(), () => new BagOfCoins(sessionService.NextItemId(), roomId)),
                (_ => GameBalance.CalculateMapChestWeight(), () => new Map(sessionService.NextItemId()))
            );
            AddWeightedGroup(options, roomId, GameBalance.ChestWeaponWeight,
                (r => GameBalance.CalculateRustSwordChestWeight(r), () => new RustSword(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronSwordChestWeight(r), () => new IronSword(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateSilverSwordChestWeight(r), () => new SilverSword(sessionService.NextItemId(), roomId, false)),
                (_ => GameBalance.CalculateGlassSwordChestWeight(), () => new GlassSword(sessionService.NextItemId(), roomId, false)),

                (r => GameBalance.CalculateMagicWandChestWeight(r), () => new MagicWand(sessionService.NextItemId(), roomId, false)),
                (_ => GameBalance.CalculateRandomWandChestWeight(), () => new RandomWand(sessionService.NextItemId(), roomId, false))
            );
            AddWeightedGroup(options, roomId, GameBalance.ChestArmorWeight,
                (r => GameBalance.CalculateLeatherHelmChestWeight(r), () => new LeatherHelm(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronHelmChestWeight(r), () => new IronHelm(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateLeatherVestChestWeight(r), () => new LeatherVest(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronCuirassChestWeight(r), () => new IronCuirass(sessionService.NextItemId(), roomId, false))
            );
            AddWeightedGroup(options, roomId, GameBalance.ChestHealWeight,
                (_ => GameBalance.CalculateRegenPotionChestWeight(), () => new RegenPotion(sessionService.NextItemId(), roomId, false)),
                (_ => GameBalance.CalculatePowerPotionChestWeight(), () => new PowerPotion(sessionService.NextItemId(), roomId, false)),
                (_ => GameBalance.CalculateRandomPotionChestWeight(), () => new RandomPotion(sessionService.NextItemId(), roomId, false))
            );
            return SelectRandom(options);
        }
        public Item? CreateShopItem(IGameSessionService sessionService)
        {
            int roomId = sessionService.RoomCounter;

            var options = new List<(int Weight, Func<Item?> Creator)>();

            AddWeightedGroup(options, roomId, GameBalance.ShopOtherWeight,
                (_ => GameBalance.CalculateKeyShopWeight(), () => new Key(sessionService.NextItemId(), roomId)),
                (_ => GameBalance.CalculateMapShopWeight(), () => new Map(sessionService.NextItemId()))
            );
            AddWeightedGroup(options, roomId, GameBalance.ShopWeaponWeight,
                (r => GameBalance.CalculateRustSwordShopWeight(r), () => new RustSword(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateIronSwordShopWeight(r), () => new IronSword(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateSilverSwordShopWeight(r), () => new SilverSword(sessionService.NextItemId(), roomId, true)),

                (_ => GameBalance.CalculateMagicWandShopWeight(), () => new MagicWand(sessionService.NextItemId(), roomId, true)),
                (_ => GameBalance.CalculateRandomWandShopWeight(), () => new RandomWand(sessionService.NextItemId(), roomId, true))
            );
            AddWeightedGroup(options, roomId, GameBalance.ShopArmorWeight,
                (r => GameBalance.CalculateWoodenBucketShopWeight(r), () => new WoodenBucket(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateLeatherHelmShopWeight(r), () => new LeatherHelm(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateIronHelmShopWeight(r), () => new IronHelm(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateLeatherVestShopWeight(r), () => new LeatherVest(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateIronCuirassShopWeight(r), () => new IronCuirass(sessionService.NextItemId(), roomId, true))
            );
            AddWeightedGroup(options, roomId, GameBalance.ShopHealWeight,
                (r => GameBalance.CalculateBandageShopWeight(r), () => new Bandage(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateRegenPotionShopWeight(r), () => new RegenPotion(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculatePowerPotionShopWeight(r), () => new PowerPotion(sessionService.NextItemId(), roomId, true)),
                (_ => GameBalance.CalculateRandomPotionShopWeight(), () => new RandomPotion(sessionService.NextItemId(), roomId, true))
            );
            return SelectRandom(options);
        }
    }
}