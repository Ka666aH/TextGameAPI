namespace TextGame.Presentation.DTO
{
    public record GameSessionSaveDTO(Guid Id, string Name, int Room, DateTime CreatedAt);
}
