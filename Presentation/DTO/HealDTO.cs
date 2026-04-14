namespace TextGame.Presentation.DTO
{
    public record HealDTO(int? Id, string Name, string Description, int? cost, int? MaxHealthBoost, int? CurrentHealthBoost);
}