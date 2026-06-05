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
    [Authorize(Policy = Policies.RequireGameSession)]
    [Route("saves")]
    public class SaveController : ControllerBase
    {
        private readonly ISaveService _saveService;

        public SaveController(ISaveService saveService)
        {
            _saveService = saveService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromQuery] string? gameSessionSaveName, CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            Guid newGameSessionSaveId = await _saveService.CreateAsync(gameSessionId, SaveType.Manual, gameSessionSaveName, ct);
            await _saveService.LoadAsync(gameSessionId, newGameSessionSaveId, ct);
            return Ok();
        }
        [HttpGet("{gameSessionSaveId}")]
        public async Task<IActionResult> LoadGameSessionSaveAsync(Guid gameSessionSaveId, CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            await _saveService.LoadAsync(gameSessionId, gameSessionSaveId, ct);
            return Ok();
        }
        [HttpDelete("{gameSessionSaveId}")]
        public async Task<IActionResult> DeleteGameSessionAsync(Guid gameSessionSaveId, CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            await _saveService.DeleteAsync(gameSessionId, gameSessionSaveId, ct);
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> GetGameSessionSavesAsync(CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            List<GameSessionSave> gameSessionSaves = await _saveService.GetListAsync(gameSessionId, ct);
            return Ok(gameSessionSaves.ToDTO());
        }
    }
}