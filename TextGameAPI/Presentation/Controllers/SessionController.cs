using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameText;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Mappers;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Authorize]
    [Route("sessions")]
    public class SessionController : ControllerBase
    {
        private readonly IGameSessionService _gameSessionService;
        private readonly ISaveService _saveService;

        public SessionController(IGameSessionService gameSessionService, ISaveService saveService)
        {
            _gameSessionService = gameSessionService;
            _saveService = saveService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromQuery] string? gameSessionName, CancellationToken ct)
        {
            User.TryGetUserId(out Guid userId);
            Guid newGameSessionId = await _gameSessionService.CreateAsync(userId, gameSessionName, ct);
            string newAccessToken = await _gameSessionService.LoadAsync(userId, newGameSessionId, ct);

            Guid initSaveId = await _saveService.CreateAsync(newGameSessionId, SaveType.Initial, null, ct);
            await _saveService.LoadAsync(newGameSessionId, initSaveId, ct);
            CookieHelper.SetAccessCookie(HttpContext.Response, newAccessToken);

            return Ok(); //here
        }
        [HttpGet("{gameSessionId}")]
        public async Task<IActionResult> LoadAsync(Guid gameSessionId, CancellationToken ct)
        {
            User.TryGetUserId(out Guid userId);
            string newAccessToken = await _gameSessionService.LoadAsync(userId, gameSessionId, ct);
            CookieHelper.SetAccessCookie(HttpContext.Response, newAccessToken);
            return Ok();
        }
        [HttpDelete("{gameSessionId}")]
        public async Task<IActionResult> DeleteAsync(Guid gameSessionId, CancellationToken ct)
        {
            await _gameSessionService.DeleteAsync(gameSessionId, ct);
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> GetGameSessionsAsync(CancellationToken ct)
        {
            User.TryGetUserId(out Guid userId);
            List<GameSession> gameSessions = await _gameSessionService.GetListAsync(userId, ct);
            return Ok(gameSessions.ToDTO());
        }
    }
}