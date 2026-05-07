using TextGame.Application.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task RegisterAsync(string login, string password, CancellationToken ct = default);
        Task<AuthResult> LogInAsync(string login, string password, string fingerprint, CancellationToken ct = default);
        Task<AuthResult> RefreshAsync(string refreshToken, string fingerprint, Guid? gameSessionId, CancellationToken ct = default);
        Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    }
}
