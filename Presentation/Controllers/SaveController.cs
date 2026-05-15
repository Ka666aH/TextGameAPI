using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Mappers;
using TextGame.Presentation.Options;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Authorize]
    [Route("saves")]
    public class SaveController : ControllerBase
    {
        private readonly ISaveService _saveService;

        public SaveController(ISaveService saveService)
        {
            _saveService = saveService;
        }
        [HttpPost]
        public async Task<IActionResult> StartNewGameSessionAsync(CancellationToken ct)
        {
            User.TryGetUserId(out Guid userId);
            Guid gameSessionId = await _saveService.CreateGameSessionAsync(userId, ct);
            string newAccessToken = await _saveService.LoadGameSessionAsync(userId, gameSessionId, ct);
            CookieHelper.SetAccessCookie(HttpContext.Response, newAccessToken);
            return Ok();
        }
        [HttpGet("{gameSessionId}")]
        public async Task<IActionResult> LoadGameSessionAsync(Guid gameSessionId, CancellationToken ct)
        {
            User.TryGetUserId(out Guid userId);
            string newAccessToken = await _saveService.LoadGameSessionAsync(userId, gameSessionId, ct);
            CookieHelper.SetAccessCookie(HttpContext.Response, newAccessToken);
            return Ok();
        }
        [Authorize(Policy = Policies.RequireGameSession)]
        [HttpPut]
        public async Task<IActionResult> SaveGameSessionAsync(CancellationToken ct)
        {
            User.TryGetUserId(out Guid userId);
            User.TryGetGameSessionId(out Guid gameSessionId);
            await _saveService.SaveGameSessionAsync(userId, gameSessionId, ct);
            return Ok();
        }
        [HttpDelete("{gameSessionId}")]
        public async Task<IActionResult> DeleteGameSessionAsync(Guid gameSessionId, CancellationToken ct)
        {
            User.TryGetUserId(out Guid userId);
            await _saveService.DeleteGameSessionAsync(userId, gameSessionId, ct);
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> GetGameSessionsAsync(CancellationToken ct)
        {
            User.TryGetUserId(out Guid userId);
            List<GameSession> gameSessions = await _saveService.GetGameSessionsAsync(userId, ct);
            return Ok(gameSessions.ToDTO());
        }
    }
}