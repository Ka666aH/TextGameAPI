namespace TextGame.Presentation.DTO
{
    public record SessionDTO(Guid Id, string Name, long Seed, DateTime CreatedAt);
}
