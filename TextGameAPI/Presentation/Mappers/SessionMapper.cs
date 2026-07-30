using TextGame.Domain.Entities;
using TextGame.Presentation.DTO;

namespace TextGame.Presentation.Mappers
{
    public static class SessionMapper
    {
        public static SessionDTO ToDTO(this Session session) 
            => new(
            session.Id,
            session.Name,
            session.CreatedAt);
        public static List<SessionDTO> ToDTO(this IEnumerable<Session> sessions) =>
            [.. sessions.Select(ToDTO)];
    }
}
