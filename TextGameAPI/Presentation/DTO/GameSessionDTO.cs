namespace TextGame.Presentation.DTO
{
    public record GameSessionDTO(Guid Id, string Name, int? CurrentRoomId, DateTime LastSavedAt, DateTime CreatedAt);
}
