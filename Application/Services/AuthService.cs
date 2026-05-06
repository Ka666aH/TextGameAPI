using FluentValidation;
using System.Diagnostics;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;

namespace TextGame.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPasswordHasher _passwordHasher;

        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IValidator<RegisterCommand> _registerValidator;

        public AuthService(IUnitOfWork unitOfWork, IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, IPasswordHasher passwordHasher, IValidator<RegisterCommand> registerValidator)
        {
            _unitOfWork = unitOfWork;
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _registerValidator = registerValidator;
        }

        public async Task<AuthResult> RegisterAsync(string login, string password, string deviceName, CancellationToken ct = default)
        {
            var validationResult = await _registerValidator.ValidateAsync(new RegisterCommand(login, password, deviceName), ct);
            if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);


            throw new NotImplementedException();
        }
        public async Task<AuthResult> LogInAsync(string login, string password, string deviceName, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
        public async Task<AuthResult> RefreshAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
        public async Task LogOutAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
