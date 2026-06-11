using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
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
            string fingerprint = GetFingerprint();

            var authResult = await _authService.LogInAsync(authRequest.Login, authRequest.Password, fingerprint, ct);
            SetAuthCookies(authResult);
            return NoContent();
        }
        [HttpPost("logout")]
        public async Task<IActionResult> LogOut(CancellationToken ct)
        {
            string refreshToken = GetRefreshToken();
            await _authService.RevokeRefreshTokenAsync(refreshToken, ct);
            HttpContext.Response.DeleteAuthCookies();
            return NoContent();
        }
        private string GetFingerprint()
        {
            string deviceName = HttpContext.Request.Headers.UserAgent.ToString();
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            return deviceName + " | " + ip;
        }
        private string GetRefreshToken()
        {
            var request = HttpContext.Request;
            bool refreshTokenExist = request.TryGetRefreshToken(out string refreshToken);
            if (!refreshTokenExist) throw new RefreshTokenNotFoundException();
            return refreshToken;
        }
        private void SetAuthCookies(AuthResult authResult)
        {
            var response = HttpContext.Response;
            response.SetRefreshCookie(authResult.RefreshToken);
            response.SetAccessCookie(authResult.AccessToken);
        }
    }
}
