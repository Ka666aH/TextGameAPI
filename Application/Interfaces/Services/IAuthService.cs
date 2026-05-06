using TextGame.Application.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string login, string password, string deviceName, CancellationToken ct = default);
        Task<AuthResult> LogInAsync(string login, string password, string deviceName, CancellationToken ct = default);
        Task<AuthResult> RefreshAsync(CancellationToken ct = default);
        Task LogOutAsync(CancellationToken ct = default);
    }
}
