using Microsoft.AspNetCore.Mvc;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Presentation.Helpers;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("regiter")]
        public async Task<IActionResult> RegisterAsync(string login, string password, CancellationToken ct)
        {
            await _authService.RegisterAsync(login, password, ct);
            return await LogInAsync(login, password, ct);
        }
        [HttpPost("login")]
        public async Task<IActionResult> LogInAsync(string login, string password, CancellationToken ct)
        {
            string fingerprint = GetFingerprint();

            var authResult = await _authService.LogInAsync(login, password, fingerprint, ct);
            SetAuthCookies(authResult);
            return Ok();
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync(CancellationToken ct)
        {
            string refreshToken = GetRefreshToken();
            string fingerprint = GetFingerprint();
            string accessToken = GetAccessToken();

            var authResult = await _authService.RefreshAsync(refreshToken, accessToken, fingerprint, ct);
            SetAuthCookies(authResult);
            return Ok();
        }
        [HttpPost("logout")]
        public async Task<IActionResult> LogOut(CancellationToken ct)
        {
            string refreshToken = GetRefreshToken();
            await _authService.RevokeRefreshTokenAsync(refreshToken, ct);
            HttpContext.Response.DeleteAuthCookies();
            return Ok();
        }
        private string GetFingerprint()
        {
            string deviceName = HttpContext.Request.Headers.UserAgent.ToString();
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            return deviceName + ip;
        }
        private string GetRefreshToken()
        {
            var request = HttpContext.Request;
            bool refreshTokenExist = request.TryGetRefreshToken(out string refreshToken);
            if (!refreshTokenExist) throw new RefreshTokenNotFoundException();
            return refreshToken;
        }
        private string GetAccessToken()
        {
            var request = HttpContext.Request;
            bool accessTokenExist = request.TryGetAccessToken(out string accessToken);
            if (!accessTokenExist) throw new AccessTokenNotFoundException();
            return accessToken;
        }
        private void SetAuthCookies(AuthResult authResult)
        {
            var response = HttpContext.Response;
            response.SetRefreshCookie(authResult.RefreshToken);
            response.SetAccessCookie(authResult.AccessToken);
        }
    }
}
