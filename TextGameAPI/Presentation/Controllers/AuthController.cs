using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.Attributes;
using TextGame.Presentation.DTO;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Options;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [BypassRefresh]
    [EnableRateLimiting(RateLimiter.AuthPolicyName)]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody]AuthRequest authRequest, CancellationToken ct)
        {
            await _authService.RegisterAsync(authRequest.Login, authRequest.Password, ct);
            return await LogInAsync(authRequest, ct);
        }
        [HttpPost("login")]
        public async Task<IActionResult> LogInAsync([FromBody] AuthRequest authRequest, CancellationToken ct)
        {
            string fingerprint = HttpContext.GetFingerprint();

            var authResult = await _authService.LogInAsync(authRequest.Login, authRequest.Password, fingerprint, ct);
            CookieHelper.SetAuthCookies(authResult, HttpContext);
            return NoContent();
        }
        [HttpPost("logout")]
        public async Task<IActionResult> LogOut(CancellationToken ct)
        {
            string refreshToken = HttpContext.Request.GetRequiredRefreshToken();
            await _authService.RevokeRefreshTokenAsync(refreshToken, ct);
            HttpContext.Response.DeleteAuthCookies();
            return NoContent();
        }
    }
}
