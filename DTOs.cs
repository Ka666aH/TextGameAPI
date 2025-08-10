namespace TextGame
{
    //public record RoomDTO(int number, string name, string description);
    public record MapRoomDTO(int number, string name);
    public record ChestDTO(string name, string description, bool isLocked, bool isClosed);
    public record GameStatsDTO(int coins, int keys, List<object> Inventory);
    public record GameOverStatsDTO(int roomNumber, int coins, int keys, List<object> Inventory);
    public record GameOverDTO(string message, GameOverStatsDTO gameOverStats);
    public record EquipmentListDTO(Weapon weapon, Armor helm, Armor chestplate);


    public record GameObjectDTO(string Name, string Description);
    public record RoomDTO(int Number, string Name, string Description);
    public record ItemDTO(int? Id, string Name, string Description, bool IsCarryable);
    public record EquipmentDTO(int? Id, string Name, string Description, bool IsCarryable, int? Durability);
    public record WeaponDTO(int? Id, string Name, string Description, bool IsCarryable, int? Durability, int? Damage);
    public record ArmorDTO(int? Id, string Name, string Description, bool IsCarryable, int? Durability, int? DamageBlock);

    public static class GameObjectMapper
    {
        public static object ToDTO(GameObject gameObject)
        {
            return gameObject switch
            {
                Room room => new RoomDTO(room.Number, room.Name!, room.Description!),
                Item item =>
                item switch
                {
                    Equipment equipment =>
                    equipment switch
                    {
                        Weapon weapon => new WeaponDTO(
                    weapon.Id,
                    weapon.Name!,
                    weapon.Description!,
                    weapon.IsCarryable,
                    weapon.Durability,
                    weapon.Damage
                ),
                        Armor armor => new ArmorDTO(
                            armor.Id,
                            armor.Name!,
                            armor.Description!,
                            armor.IsCarryable,
                            armor.Durability,
                            armor.DamageBlock
                        ),
                        _ => new EquipmentDTO(equipment.Id, equipment.Name!, equipment.Description!, equipment.IsCarryable, equipment.Durability),
                    },

                    _ => new ItemDTO(item.Id, item.Name!, item.Description!, item.IsCarryable),
                },
                _ => new GameObjectDTO(gameObject.Name ?? "", gameObject.Description ?? "")
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