using TextGame.Domain.Entities;
using TextGame.Presentation.DTO;

namespace TextGame.Presentation.Mappers
{
    public static class GameSessinSaveMapper
    {
        public static GameSessionSaveDTO ToDTO(this GameSessionSave gameSessionSave) =>
            new(
                gameSessionSave.Id,
                gameSessionSave.Name,
                gameSessionSave.State.CurrentRoomId,
                gameSessionSave.CreatedAt
                );
        public static List<GameSessionSaveDTO> ToDTO(this List<GameSessionSave> gameSessionSaves) =>
            [.. gameSessionSaves.Select(ToDTO)];
    }
}
