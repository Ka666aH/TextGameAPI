using TextGame.Application.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task RegisterAsync(string login, string password, CancellationToken ct = default);
        Task<AuthResult> LogInAsync(string login, string password, string fingerprint, CancellationToken ct = default);
        Task<AuthResult> RefreshAsync(string refreshToken, string accessToken, string fingerprint, CancellationToken ct = default);
        Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        /// <summary>Bootstrap фронта: логин и активная игра. Протухшая сессия в claim превращается в null.</summary>
        Task<MeResult> GetCurrentAsync(Guid userId, Guid? sessionId, CancellationToken ct = default);
    }
}
