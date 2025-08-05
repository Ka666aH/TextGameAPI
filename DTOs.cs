namespace TextGame
{
    public record CurrentRoomDTO(int Number, string Name, string Description);
    public record ChestDTO(string Name, string Description, bool isLocked, bool isClosed);
}
