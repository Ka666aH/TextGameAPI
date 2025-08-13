namespace TextGame
{
    public record MapRoomDTO(int number, string name);
    public record ChestDTO(string name, string description, bool isLocked, bool isClosed);
    public record GameStatsDTO(Weapon Weapon, Helm? Helm, Chestplate? Chestplate, int MaxHealth, int CurrentHealth, int coins, int keys, List<object> Inventory);
    public record GameOverStatsDTO(int roomNumber, Weapon Weapon, Helm? Helm, Chestplate? Chestplate, int MaxHealth, int CurrentHealth, int coins, int keys, List<object> Inventory);
    public record GameOverDTO(string message, GameOverStatsDTO gameOverStats);
    public record BattleLog(string target, int damage, int? healthBeforeAttack, int? healthAfterAttack);


    public record GameObjectDTO(string Name, string Description);
    public record RoomDTO(int Number, string Name, string Description, List<Enemy> Enemies);
    public record ItemDTO(int? Id, string Name, string Description, bool IsCarryable);
    public record EquipmentDTO(int? Id, string Name, string Description, bool IsCarryable, int? Durability);
    public record WeaponDTO(int? Id, string Name, string Description, bool IsCarryable, int? Durability, int? Damage);
    public record ArmorDTO(int? Id, string Name, string Description, bool IsCarryable, int? Durability, int? DamageBlock);
    public record HealDTO(int? Id, string Name, string Description, bool IsCarryable, int? MaxHealthBoost, int? CurrentHealthBoost);
    public record EnemyDTO(int? Id, string Name, string Description, int Health, int Damage, int DamageBlock);

    public static class GameObjectMapper
    {
        public static object ToDTO(GameObject gameObject)
        {
            return gameObject switch
            {
                Room room => new RoomDTO(room.Number, room.Name!, room.Description!, room.Enemies),
                Enemy enemy => new EnemyDTO(enemy.Id, enemy.Name!, enemy.Description!, enemy.Health, enemy.Damage, enemy.DamageBlock),
                Item item =>
                item switch
                {
                    Heal heal => new HealDTO(heal.Id, heal.Name!, heal.Description!, heal.IsCarryable, heal.MaxHealthBoost, heal.CurrentHealthBoost),
                    Equipment equipment =>
                    equipment switch
                    {
                        Weapon weapon => new WeaponDTO(weapon.Id, weapon.Name!, weapon.Description!, weapon.IsCarryable, weapon.Durability, weapon.Damage),
                        Armor armor => new ArmorDTO(armor.Id, armor.Name!, armor.Description!, armor.IsCarryable, armor.Durability, armor.DamageBlock),

                        _ => new EquipmentDTO(equipment.Id, equipment.Name!, equipment.Description!, equipment.IsCarryable, equipment.Durability),
                    },

                    _ => new ItemDTO(item.Id, item.Name!, item.Description!, item.IsCarryable),
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