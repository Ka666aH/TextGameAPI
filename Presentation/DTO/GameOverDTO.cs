namespace TextGame.Presentation.DTO
{
    //public record GameOvernInfoDTO(int roomNumber, Weapon Weapon, Helm? Helm, Chestplate? Chestplate, int MaxHealth, int CurrentHealth, int coins, int keys, List<object> Inventory);
    public record GameOverDTO(string message, GameInfoDTO gameInfo);
}