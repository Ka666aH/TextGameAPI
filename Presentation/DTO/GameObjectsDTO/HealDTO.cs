namespace TextGame.Presentation.DTO
{
    public record HealDTO(int? Id, string Name, string Description, int? Cost, int? MaxHealthBoost, int? CurrentHealthBoost)
        : ItemDTO(Id, Name, Description, Cost);
}