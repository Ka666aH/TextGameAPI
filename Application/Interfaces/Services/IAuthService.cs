using TextGame.Application.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string login, string password, string deviceName);
        Task<AuthResult> LogInAsync(string login, string password, string deviceName);
        Task<AuthResult> RefreshAsync();
        Task LogOutAsync();
    }
}
