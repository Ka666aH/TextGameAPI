using TextGame.Application.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string login, string password, string deviceName, CancellationToken ct = default);
        Task<AuthResult> LogInAsync(string login, string password, string deviceName, CancellationToken ct = default);
        Task<AuthResult> RefreshAsync(string refreshToken, string deviceName, Guid? gameSessionId, CancellationToken ct = default);
        Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    }
}
