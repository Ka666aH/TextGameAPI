using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Presentation.DTO;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Mappers;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Authorize]
    [Route("sessions")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        /// <summary>Создать игру. seed можно передать (?seed=) — иначе сгенерируется и вернётся в ответе.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromQuery] string? sessionName, [FromQuery] long? seed, CancellationToken ct)
        {
            Guid userId = GetUserId();
            Session session = await _sessionService.CreateAsync(userId, sessionName, seed, ct);
            string newAccessToken = await _sessionService.LoadAsync(userId, session.Id, ct);

            CookieHelper.SetAccessCookie(HttpContext.Response, newAccessToken);

            return Created($"/sessions/{session.Id}", new { sessionId = session.Id, sessionName = session.Name, seed = session.Seed });
        }
        /// <summary>Сделать сессию активной.</summary>
        [HttpPut("current")]
        public async Task<IActionResult> LoadAsync([FromBody] SetCurrentRequest request, CancellationToken ct)
        {
            Guid userId = GetUserId();
            // Владельца проверяет ISessionService.LoadAsync. Guard с claim здесь нельзя:
            // после логина активной игры ещё нет, и выбрать её было бы невозможно.
            string newAccessToken = await _sessionService.LoadAsync(userId, request.Id, ct);
            CookieHelper.SetAccessCookie(HttpContext.Response, newAccessToken);
            return Ok(new { sessionId = request.Id });
        }
        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> DeleteAsync(Guid sessionId, CancellationToken ct)
        {
            Guid userId = GetUserId();
            // Активную игру определяем до удаления: claim — единственный её источник.
            bool wasActive = User.TryGetSessionId(out Guid activeSessionId) && activeSessionId == sessionId;

            await _sessionService.DeleteAsync(userId, sessionId, ct);

            // Иначе claim остался бы на удалённую игру и следующий /state вернул бы 404.
            if (wasActive)
                CookieHelper.SetAccessCookie(HttpContext.Response, _sessionService.CreateAccessToken(userId));

            return NoContent();
        }
        [HttpGet]
        public async Task<IActionResult> GetSessionsAsync(CancellationToken ct)
        {
            Guid userId = GetUserId();
            var sessions = await _sessionService.GetListAsync(userId, ct);
            return Ok(sessions.ToDTO());
        }

        private Guid GetUserId()
        {
            // Раньше результат TryGetUserId игнорировался и сессия создавалась для Guid.Empty.
            if (!User.TryGetUserId(out Guid userId)) throw new MissingUserIdClaimException();

            return userId;
        }
    }
}
