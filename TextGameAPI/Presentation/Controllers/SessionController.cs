using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
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
        private readonly ISaveService _saveService;
        private readonly ISessionAccessGuard _sessionAccessGuard;

        public SessionController(ISessionService sessionService, ISaveService saveService, ISessionAccessGuard sessionAccessGuard)
        {
            _sessionService = sessionService;
            _saveService = saveService;
            _sessionAccessGuard = sessionAccessGuard;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromQuery] string? sessionName, CancellationToken ct)
        {
            User.TryGetUserId(out Guid userId);
            Guid newSessionId = await _sessionService.CreateAsync(userId, sessionName, ct);
            string newAccessToken = await _sessionService.LoadAsync(userId, newSessionId, ct);

            Guid initSaveId = await _saveService.CreateAsync(newSessionId, SaveType.Initial, null, ct);
            await _saveService.LoadAsync(newSessionId, initSaveId, ct);
            CookieHelper.SetAccessCookie(HttpContext.Response, newAccessToken);

            return Created($"/sessions/{newSessionId}", new { sessionId = newSessionId });
        }
        [HttpPost("{sessionId}")]
        public async Task<IActionResult> LoadAsync(Guid sessionId, CancellationToken ct)
        {
            await _sessionAccessGuard.EnsureOwnershipAsync(sessionId,ct);
            User.TryGetUserId(out Guid userId);
            string newAccessToken = await _sessionService.LoadAsync(userId, sessionId, ct);
            CookieHelper.SetAccessCookie(HttpContext.Response, newAccessToken);
            return Ok(new { sessionId });
        }
        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> DeleteAsync(Guid sessionId, CancellationToken ct)
        {
            await _sessionAccessGuard.EnsureOwnershipAsync(sessionId, ct);
            await _sessionService.DeleteAsync(sessionId, ct);
            return NoContent();
        }
        [HttpGet]
        public async Task<IActionResult> GetSessionsAsync(CancellationToken ct)
        {
            User.TryGetUserId(out Guid userId);
            var sessions = await _sessionService.GetListAsync(userId, ct);
            return Ok(sessions.ToDTO());
        }
    }
}