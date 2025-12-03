using Microsoft.AspNetCore.Routing.Constraints;
using System.Reflection.Metadata.Ecma335;
using static System.Net.Mime.MediaTypeNames;

namespace TextGame
{
    public class GameBalance
    {
        public const double SpreadFloor = 0.8;
        public const double SpreadCeiling = 1.2;
        public const double ShopMultiplier = 1.2;
        private const double _gain = 0.01;

        public const double StoreMargin = 2;

        public static double ApplyGain(int roomId) => 1 + roomId * _gain;
        public static (int min, int max) ApplySpread(int baseValue, int roomId)
        {
            int minValue = (int)(baseValue * ApplyGain(roomId) * SpreadFloor);
            int maxValue = (int)(baseValue * ApplyGain(roomId) * SpreadCeiling);
            return (minValue, maxValue);
        }
        public static int ApplyShopMultiplier(int baseValue) => (int)(baseValue * ShopMultiplier);

        //Игрок
        public const int DefaultMaxHealth = 100;

        //Распределение комнат
        public const int EmptyRoomWeight = 3000;
        public const int SmallRoomWeight = 4000;
        public const int BigRoomWeight = 2000;
        public const int ShopWeight = 1000;

        public const int SmallRoomItemsAmount = 1;
        public const int BigRoomItemsAmount = 3;
        public const int ShopItemsAmount = 5;

        //Расчёт весов (вероятности) появления врагов в комнате
        public static int CalculateNoneWeight(int roomId) => roomId < 100 ? (int)(-0.4 * roomId + 60) : 20;
        public static int CalculateSkeletorWeight(int roomId) => roomId < 50 ? (int)-0.48 * roomId + 24 : 0;
        public static int CalculateSkeletorArcherWeight(int roomId) => roomId switch
        {
            < 10 => (int)(0.3 * roomId + 12),
            >= 10 and < 65 => (int)(-0.27778 * (roomId - 10) + 15),
            _ => 0
        };
        public static int CalculateDeadmanWeight(int roomId) => roomId switch
        {
            <= 100 => (int)(0.2 * roomId + 4),
            > 100 and <= 200 => (int)(-0.08 * (roomId - 100) + 24),
            _ => 20
        };
        public static int CalculateGhostWeight(int roomId) => roomId switch
        {
            <= 100 => (int)(0.32 * roomId),
            > 100 and <= 200 => (int)(-0.08 * (roomId - 100) + 32),
            _ => 30
        };
        public static int CalculateLichWeight(int roomId) => roomId switch
        {
            <= 10 => 0,
            > 10 and <= 200 => (int)(0.21053 * (roomId - 10)),
            _ => 50
        };

        //Item

        //Cost
        public const int KeyBaseCost = 35;
        public const int BagOfCoinsBaseCost = 11;
        public const int MapBaseCost = 100;


        //Chest
        public const int MinChestItemsAmount = 1;
        public const int MaxChestItemsAmount = 3;
        public const int LockedProbabilityDenominator = 75;
        public const int MimicProbabilityDenominator = 50;
        public const int ChestDivider = 100;

        //Heal
        public const double MaxHealthCostMultiplier = 2;
        public const double CurrentHealthCostMultiplier = 1;
        //Bandage
        public const int BandageBaseMaxHealthBoost = 0;
        public const int BandageBaseCurrentHealthBoost = 20;
        //Regen
        public const int RegenPotionBaseMaxHealthBoost = 0;
        public const int RegenPotionBaseCurrentHealthBoost = 60;
        //Regen
        public const int PowerPotionBaseMaxHealthBoost = 10;
        public const int PowerPotionBaseCurrentHealthBoost = 30;
        //Random
        public const int RandomPotionBaseMaxHealthBoost = 25;
        public const int RandomPotionBaseCurrentHealthBoost = 50;

        //Equip

        //Weapon

        //Fists
        public const int FistsBaseDamage = 10;
        public const int FistSelfHarmProbabilityDivider = 2;
        public const double FistSelfHarmDivider = 2;

        //Sword
        public static int CalculateSwordCost(int durability, int damage) => 1 + (durability * damage / 10);
        //Rust
        public const int RustSwordBaseDurability = 8;
        public const int RustSwordBaseDamage = 15;
        //Iron
        public const int IronSwordBaseDurability = 18;
        public const int IronSwordBaseDamage = 26;
        //Silver
        public const int SilverSwordBaseDurability = 13;
        public const int SilverSwordBaseDamage = 53;
        //Glass
        public const int GlassSwordBaseDurability = 1;
        public const int GlassSwordBaseDamage = 115;

        //Wand
        public static int CalculateWandCost(int damage) => damage * 3;
        //Magic
        public const int MagicWandBaseDamage = 21;
        //Random
        public const int RandomWandBaseDamage = 41;

        //Armor
        public static int CalculateArmorCost(int durability, int damageBlock) => 1 + (durability * damageBlock / 10);
        //Bucket
        public const int WoodenBucketBaseDurability = 3;
        public const int WoodenBucketBaseDamageBlock = 2;
        //Leather helm
        public const int LeatherHelmBaseDurability = 12;
        public const int LeatherHelmBaseDamageBlock = 4;
        //Iron helm
        public const int IronHelmBaseDurability = 18;
        public const int IronHelmBaseDamageBlock = 6;
        //Leather vest
        public const int LeatherVestBaseDurability = 24;
        public const int LeatherVestBaseDamageBlock = 8;
        //Iron cuirass
        public const int IronCuirassBaseDurability = 36;
        public const int IronCuirassBaseDamageBlock = 12;

        //Веса групп предметов
        public const double RoomOtherWeight = 70;
        public const double RoomWeaponWeight = 10;
        public const double RoomArmorWeight = 5;
        public const double RoomHealWeight = 15;

        public const double ChestOtherWeight = 45;
        public const double ChestWeaponWeight = 20;
        public const double ChestArmorWeight = 10;
        public const double ChestHealWeight = 25;

        public const double ShopOtherWeight = 18;
        public const double ShopWeaponWeight = 28;
        public const double ShopArmorWeight = 27;
        public const double ShopHealWeight = 27;

        //Веса внутри групп
        //Room
        //RoomOther
        public static int CalculateNoneRoomWeight() => 30;
        public static int CalculateKeyRoomWeight() => 6;
        public static int CalculateBagOfCoinsRoomWeight() => 14;
        public static int CalculateChestRoomWeight() => 20;
        //RoomWeapon
        public static int CalculateRustSwordRoomWeight(int r) => r < 100 ? (int)(-0.89 * r + 89) : 0;
        public static int CalculateIronSwordRoomWeight(int r) => (int)(0.25 * r + 5);
        public static int CalculateSilverSwordRoomWeight(int r) => (int)(0.15 * r);
        public static int CalculateGlassSwordRoomWeight() => 1;
        public static int CalculateMagicWandRoomWeight(int r) => (int)(0.25 * r + 5);
        public static int CalculateRandomWandRoomWeight(int r) => (int)(0.04 * r);
        //RoomArmor
        public static int CalculateWoodenBucketRoomWeight(int r) => (int)(r < 50 ? -1.7 * r + 85 : 0);
        public static int CalculateLeatherHelmRoomWeight(int r) => (int)(0.35 * r + 10);
        public static int CalculateIronHelmRoomWeight(int r) => (int)(0.2 * r);
        public static int CalculateLeatherVestRoomWeight(int r) => (int)(0.2 * r + 5);
        public static int CalculateIronCuirassRoomWeight(int r) => (int)(0.1 * r);
        //RoomHeal
        public static int CalculateBandageRoomWeight(int r) => (int)(r < 100 ? -0.34 * r + 67 : 0);
        public static int CalculateRegenPotionRoomWeight(int r) => (int)(0.22 * r + 22);
        public static int CalculatePowerPotionRoomWeight(int r) => (int)(0.11 * r + 10);
        public static int CalculateRandomPotionRoomWeight(int r) => (int)(0.01 * r + 1);

        //Chest
        //ChestOther
        public static int CalculateKeyChestWeight() => 10;
        public static int CalculateBagOfCoinsChestWeight() => 30;
        public static int CalculateMapChestWeight() => 5;
        //ChestWeapon
        public static int CalculateRustSwordChestWeight(int r) => (int)(r < 50 ? -0.8 * r + 40 : 0);
        public static int CalculateIronSwordChestWeight(int r) => (int)(0.25 * r + 15);
        public static int CalculateSilverSwordChestWeight(int r) => (int)(0.1 * r + 5);
        public static int CalculateGlassSwordChestWeight() => 5;
        public static int CalculateMagicWandChestWeight(int r) => (int)(0.05 * r + 25);
        public static int CalculateRandomWandChestWeight() => 10;
        //ChestArmor
        public static int CalculateLeatherHelmChestWeight(int r) => (int)(-0.15 * r + 55);
        public static int CalculateIronHelmChestWeight(int r) => (int)(0.15 * r + 25);
        public static int CalculateLeatherVestChestWeight(int r) => (int)(-0.04 * r + 14);
        public static int CalculateIronCuirassChestWeight(int r) => (int)(0.04 * r + 6);
        //ChestHeal
        public static int CalculateRegenPotionChestWeight() => 60;
        public static int CalculatePowerPotionChestWeight() => 30;
        public static int CalculateRandomPotionChestWeight() => 10;

        //Shop
        //ShopOther
        public static int CalculateKeyShopWeight() => 14;
        public static int CalculateMapShopWeight() => 4;
        //ShopWeapon
        public static int CalculateRustSwordShopWeight(int r) => (int)(r < 30 ? -r + 30 : 0);
        public static int CalculateIronSwordShopWeight(int r) => (int)(0.15 * r + 20);
        public static int CalculateSilverSwordShopWeight(int r) => (int)(0.15 * r + 5);
        public static int CalculateMagicWandShopWeight() => 30;
        public static int CalculateRandomWandShopWeight() => 15;
        //ShopArmor
        public static int CalculateWoodenBucketShopWeight(int r) => (int)(r < 30 ? -1.67 * r + 50 : 0);
        public static int CalculateLeatherHelmShopWeight(int r) => (int)(0.025 * r + 27.5);
        public static int CalculateIronHelmShopWeight(int r) => (int)(0.375 * r + 12.5);
        public static int CalculateLeatherVestShopWeight(int r) => (int)(0.01 * r + 7);
        public static int CalculateIronCuirassShopWeight(int r) => (int)(0.09 * r + 3);
        //ShopHeal
        public static int CalculateBandageShopWeight(int r) => (int)(r < 200 ? -0.15 * r + 30 : 0);
        public static int CalculateRegenPotionShopWeight(int r) => (int)(0.1 * r + 40);
        public static int CalculatePowerPotionShopWeight(int r) => (int)(0.05 * r + 20);
        public static int CalculateRandomPotionShopWeight() => 10;

        //Enemy
        //Skeletor
        public const int SkeletorBaseHealth = 20;
        public const int SkeletorBaseDamage = 10;
        public const int SkeletorBaseDamageBlock = 2;

        //SkeletorArcher
        public const int SkeletorArcherBaseHealth = 10;
        public const int SkeletorArcherBaseDamage = 20;
        public const int SkeletorArcherBaseDamageBlock = 1;

        //Deadman
        public const int DeadmanBaseHealth = 50;
        public const int DeadmanBaseDamage = 5;
        public const int DeadmanBaseDamageBlock = 3;

        //Ghost
        public const int GhostBaseHealth = 15;
        public const int GhostBaseDamage = 15;
        public const int GhostBaseDamageBlock = 0;
        public const int GhostHitDivider = 2;
        //Lich
        public const int LichBaseHealth = 40;
        public const int LichBaseDamage = 30;
        public const int LichBaseDamageBlock = 8;

        //Mimic
        public const int MimicBaseHealth = 15;
        public const int MimicBaseDamage = 10;
        public const int MimicBaseDamageBlock = 5;
    }
}