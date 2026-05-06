using FluentValidation;
using System.Diagnostics;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameText;

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
            var validationResult = _registerValidator.Validate(new RegisterCommand(login, password, deviceName));
            if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

            string hashedPassword = _passwordHasher.Hash(password);
            User user = new(login, hashedPassword);

            bool userExist = await _userRepository.GetAsync(login, ct) != null;
            if (userExist) throw new ValidationException([new("Login", ValidatorsText.LoginAlreadyExist)]);

            await _userRepository.CreateAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return await LogInAsync(login, password, deviceName, ct);
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
