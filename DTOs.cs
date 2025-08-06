namespace TextGame
{
    public record CurrentRoomDTO(int number, string name, string description);
    public record ChestDTO(string name, string description, bool isLocked, bool isClosed);
    public record GameOverStatsDTO(int roomNumber, int coins, List<Item> Inventory);
}
