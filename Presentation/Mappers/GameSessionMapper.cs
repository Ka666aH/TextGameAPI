using TextGame.Domain.Entities;
using TextGame.Presentation.DTO;

namespace TextGame.Presentation.Mappers
{
    public static class GameSessionMapper
    {
        public static GameSessionDTO ToDTO(this GameSession gameSession) 
            => new(
            gameSession.Id,
            gameSession.Name,
            gameSession.CurrentRoomId,
            gameSession.LastSavedAt,
            gameSession.CreatedAt);
        public static List<GameSessionDTO> ToDTO(this List<GameSession> gameSessions) =>
            [.. gameSessions.Select(ToDTO)];
    }
}
