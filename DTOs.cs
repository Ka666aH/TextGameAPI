namespace TextGame
{
    public record CurrentRoomDTO(int number, string name, string description);
    public record MapRoomDTO(int number, string name);
    public record ChestDTO(string name, string description, bool isLocked, bool isClosed);
    public record GameStatsDTO(int coins, int keys, List<Item> Inventory);
    public record GameOverStatsDTO(int roomNumber, int coins, int keys, List<Item> Inventory);
    public record GameOverDTO(string message, GameOverStatsDTO gameOverStats);
    public record EquipmentListDTO(Weapon weapon, Armor helm, Armor chestplate);

    
}
