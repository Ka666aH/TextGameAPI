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
    /// Пара токенов как ресурс: POST — выпустить (вход), DELETE — отозвать (выход).
    /// </summary>
    [ApiController]
    [BypassRefresh]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("tokens")]
        // Лимит только на вход: иначе игрок, упёршийся в 429, не может даже разлогиниться (п. 12).
        [EnableRateLimiting(RateLimiter.AuthPolicyName)]
        public async Task<IActionResult> LogInAsync([FromBody] AuthRequest authRequest, CancellationToken ct)
        {
            string fingerprint = HttpContext.GetFingerprint();

            var authResult = await _authService.LogInAsync(authRequest.Login, authRequest.Password, fingerprint, ct);
            CookieHelper.SetAuthCookies(authResult, HttpContext);
            return NoContent();
        }
        [HttpDelete("tokens")]
        public async Task<IActionResult> LogOut(CancellationToken ct)
        {
            string refreshToken = HttpContext.Request.GetRequiredRefreshToken();
            await _authService.RevokeRefreshTokenAsync(refreshToken, ct);
            HttpContext.Response.DeleteAuthCookies();
            return NoContent();
        }
    }
}
