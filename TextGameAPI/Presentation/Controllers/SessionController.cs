using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Presentation.Attributes;
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

            return Created($"/sessions/{session.Id}", new { sessionId = session.Id, seed = session.Seed });
        }
        /// <summary>Сделать сессию активной.</summary>
        [RequireSessionOwnership]
        [HttpPut("current")]
        public async Task<IActionResult> LoadAsync([FromBody] SetCurrentRequest request, CancellationToken ct)
        {
            Guid userId = GetUserId();
            string newAccessToken = await _sessionService.LoadAsync(userId, request.Id, ct);
            CookieHelper.SetAccessCookie(HttpContext.Response, newAccessToken);
            return Ok(new { sessionId = request.Id });
        }
        [RequireSessionOwnership]
        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> DeleteAsync(Guid sessionId, CancellationToken ct)
        {
            await _sessionService.DeleteAsync(sessionId, ct);
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
