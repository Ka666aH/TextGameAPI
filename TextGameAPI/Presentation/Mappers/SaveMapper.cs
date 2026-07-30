using TextGame.Domain.Entities;
using TextGame.Presentation.DTO;

namespace TextGame.Presentation.Mappers
{
    public static class SaveMapper
    {
        public static SaveDTO ToDTO(this Save save) =>
            new(
                save.Id,
                save.Name,
                save.State.CurrentRoomId,
                save.CreatedAt
                );
        public static List<SaveDTO> ToDTO(this IEnumerable<Save> saves) =>
            [.. saves.Select(ToDTO)];
    }
}
