namespace TextGame
{
    public record MapRoomDTO(int number, string name);
    public record ChestStateDTO(string name, string description, bool isLocked, bool isClosed);
    public record GameInfoDTO(object room, WeaponDTO Weapon, ArmorDTO? Helm, ArmorDTO? Chestplate, int MaxHealth, int CurrentHealth, int coins, int keys, List<object> Inventory);
    //public record GameOvernInfoDTO(int roomNumber, Weapon Weapon, Helm? Helm, Chestplate? Chestplate, int MaxHealth, int CurrentHealth, int coins, int keys, List<object> Inventory);
    public record GameOverDTO(string message, GameInfoDTO gameInfo);
    public record BattleLog(string target, int damage, int? targetHealthBeforeAttack, int? targetHealthAfterAttack, string attacker, int? healthBeforeAttack, int? healthAfterAttack);
    public record BattleWinDTO(string message, BattleLog BattleLog);


    public record GameObjectDTO(string Name, string Description);
    public record RoomDTO(int Number, string Name, string Description, List<Enemy> Enemies);
    public record RoomWithoutEnemiesDTO(int Number, string Name, string Description);
    public record EnemyDTO(int? Id, string Name, string Description, int Health, int Damage, int DamageBlock);
    public record ItemDTO(int? Id, string Name, string Description, int? cost);
    public record ChestDTO(int? Id, string Name, string Description);
    public record HealDTO(int? Id, string Name, string Description, int? cost, int? MaxHealthBoost, int? CurrentHealthBoost);
    public record EquipmentDTO(int? Id, string Name, string Description, int? cost, int? Durability);
    public record WeaponDTO(int? Id, string Name, string Description, int? cost, int? Durability, int? Damage);
    public record ArmorDTO(int? Id, string Name, string Description, int? cost, int? Durability, int? DamageBlock);

    public static class GameObjectMapper
    {
        public static object ToDTO(GameObject gameObject)
        {
            return gameObject switch
            {
                Room room =>
                room switch
                {
                    StartRoom or EndRoom or Shop => new RoomWithoutEnemiesDTO(room.Number, room.Name!, room.Description!),
                    _ => new RoomDTO(room.Number, room.Name!, room.Description!, room.Enemies),
                },
                Enemy enemy => new EnemyDTO(enemy.Id, enemy.Name!, enemy.Description!, enemy.Health, enemy.Damage, enemy.DamageBlock),
                Item item =>
                item switch
                {
                    Chest chest => new ChestDTO(chest.Id, chest.Name!, chest.Description!),
                    Heal heal => new HealDTO(heal.Id, heal.Name!, heal.Description!, heal.Cost, heal.MaxHealthBoost, heal.CurrentHealthBoost),
                    Equipment equipment =>
                    equipment switch
                    {
                        Weapon weapon => new WeaponDTO(weapon.Id, weapon.Name!, weapon.Description!, weapon.Cost, weapon.Durability, weapon.Damage),
                        Armor armor => new ArmorDTO(armor.Id, armor.Name!, armor.Description!, armor.Cost, armor.Durability, armor.DamageBlock),

                        _ => new EquipmentDTO(equipment.Id, equipment.Name!, equipment.Description!, equipment.Cost, equipment.Durability),
                    },

                    _ => new ItemDTO(item.Id, item.Name!, item.Description!, item.Cost),
                },
                _ => new GameObjectDTO(gameObject.Name ?? "НЕИЗВЕСТНО", gameObject.Description ?? "НЕИЗВЕСТНО")
            };
        }
        public static List<object> ToDTO<T>(List<T> gameObjects) where T : GameObject
        {
            List<object> gameObjectsDTO = new List<object>();
            foreach (T gameObject in gameObjects)
            {
                gameObjectsDTO.Add(ToDTO(gameObject));
            }
            return gameObjectsDTO;
        }
    }
}