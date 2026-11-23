using System.Globalization;
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
            // long в JSON-числе теряет точность в JS за 2^53 — отдаём строкой (notes/backend-requests.md п. 15).
            session.Seed.ToString(CultureInfo.InvariantCulture),
            session.CreatedAt);
        public static List<SessionDTO> ToDTO(this IEnumerable<Session> sessions) =>
            [.. sessions.Select(ToDTO)];
    }
}
