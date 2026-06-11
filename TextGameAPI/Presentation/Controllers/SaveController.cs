using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Presentation.Attributes;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Mappers;
using TextGame.Presentation.Options;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Authorize(Policy = Policies.RequireSession)]
    [RequireSessionOwnership]
    [Route("saves")]
    public class SaveController : ControllerBase
    {
        private readonly ISaveService _saveService;

        public SaveController(ISaveService saveService)
        {
            _saveService = saveService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromQuery] string? saveName, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            Guid newSaveId = await _saveService.CreateAsync(sessionId, SaveType.Manual, saveName, ct);
            await _saveService.LoadAsync(sessionId, newSaveId, ct);
            return Created($"/saves/{newSaveId}", new { saveId = newSaveId });
        }
        [HttpPost("{saveId}")]
        public async Task<IActionResult> LoadAsync(Guid saveId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _saveService.LoadAsync(sessionId, saveId, ct);
            return Ok(new { saveId });
        }
        [HttpDelete("{saveId}")]
        public async Task<IActionResult> DeleteAsync(Guid saveId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _saveService.DeleteAsync(sessionId, saveId, ct);
            return NoContent();
        }
        [HttpGet]
        public async Task<IActionResult> GetAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            var saves = await _saveService.GetListAsync(sessionId, ct);
            return Ok(saves.ToDTO());
        }
    }
}