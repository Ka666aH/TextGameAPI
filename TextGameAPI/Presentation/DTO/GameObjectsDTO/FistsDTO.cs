namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record FistsDTO(string Name, string Description, int? Damage)
        : GameObjectDTO(Name, Description);
}