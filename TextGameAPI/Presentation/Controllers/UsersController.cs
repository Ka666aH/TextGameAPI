using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.Attributes;
using TextGame.Presentation.DTO;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Options;

namespace TextGame.Presentation.Controllers
{
    /// <summary>
    /// Регистрация создаёт ресурс пользователя и сразу выдаёт токены (куки) —
    /// «создал аккаунт» и «зашёл» в одном запросе, как и было раньше.
    /// </summary>
    [ApiController]
    [BypassRefresh]
    [EnableRateLimiting(RateLimiter.AuthPolicyName)]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody] AuthRequest authRequest, CancellationToken ct)
        {
            await _authService.RegisterAsync(authRequest.Login, authRequest.Password, ct);

            string fingerprint = HttpContext.GetFingerprint();
            var authResult = await _authService.LogInAsync(authRequest.Login, authRequest.Password, fingerprint, ct);
            CookieHelper.SetAuthCookies(authResult, HttpContext);

            return StatusCode(StatusCodes.Status201Created);
        }
    }
}
