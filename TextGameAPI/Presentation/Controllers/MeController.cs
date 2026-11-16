using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Presentation.Helpers;

namespace TextGame.Presentation.Controllers
{
    /// <summary>
    /// Bootstrap-эндпоинт фронтенда: «кто я и в какой игре».
    /// Токены живут в httpOnly-куках, поэтому без этого запроса после перезагрузки страницы
    /// клиент не знает ни факта логина, ни логина, ни активной сессии (claim SessionId).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("auth")]
    public class MeController : ControllerBase
    {
        private readonly IAuthService _authService;

        public MeController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Атрибут [BypassRefresh] здесь намеренно отсутствует: запрос сам должен обновить
        /// просроченный access-токен по refresh-куке, иначе фронт разлогинится каждые 15 минут.
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetAsync(CancellationToken ct)
        {
            if (!User.TryGetUserId(out Guid userId)) throw new MissingUserIdClaimException();

            Guid? sessionId = User.TryGetSessionId(out Guid claimedSessionId) ? claimedSessionId : null;
            MeResult result = await _authService.GetCurrentAsync(userId, sessionId, ct);

            return Ok(result);
        }
    }
}
