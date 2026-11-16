namespace TextGame.Application.DTO
{
    /// <summary>
    /// Кто пришёл и в какой игре он сейчас: <c>SessionId == null</c> — залогинен, но активной игры нет.
    /// <c>SessionName</c> идёт из той же строки сессии, которая уже читается для проверки claim,
    /// поэтому фронт получает название игры без отдельного <c>GET /sessions</c>.
    /// </summary>
    public record MeResult(Guid UserId, string Login, Guid? SessionId, string? SessionName);
}
