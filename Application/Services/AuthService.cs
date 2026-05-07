using FluentValidation;
using System.Diagnostics;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameText;
using TextGame.Infrastructure.Token;

namespace TextGame.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IValidator<RegisterCommand> _registerValidator;

        private readonly IPasswordHasher _passwordHasher;

        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IValidator<RegisterCommand> registerValidator, IPasswordHasher passwordHasher, IUserRepository userRepository, ITokenRepository tokenRepository, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
        {
            _registerValidator = registerValidator;
            _passwordHasher = passwordHasher;
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task RegisterAsync(string login, string password, string deviceName, CancellationToken ct = default)
        {
            var validationResult = await _registerValidator.ValidateAsync(new RegisterCommand(login, password, deviceName), ct);
            if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

            string hashedPassword = _passwordHasher.Hash(password);
            User user = new(login, hashedPassword);

            bool userExist = await _userRepository.GetAsync(login, ct) != null;
            if (userExist) throw new ValidationException([new("Login", ValidatorsText.LoginAlreadyExist)]);
            await _userRepository.CreateAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            //return await GenerateTokens(user.Id, deviceName, null, ct);
        }
        public async Task<AuthResult> LogInAsync(string login, string password, string deviceName, CancellationToken ct = default)
        {
            User user = await _userRepository.GetAsync(login, ct) 
                ?? throw new UserNotFoundException();
            bool passwordCorrect = _passwordHasher.Verify(password, user.HashedPass);
            if (!passwordCorrect) throw new IncorrectPasswordException();
            return await GenerateTokens(user.Id, deviceName, null, ct);
        }
        public async Task<AuthResult> RefreshAsync(string refreshToken, string deviceName, Guid? gameSessionId, CancellationToken ct = default)
        {
            RefreshToken? token = await _refreshTokenRepository.GetAsync(refreshToken, ct) 
                ?? throw new RefreshTokenNotFoundException();

            if (token.IsRevoked || token.DeviceName != deviceName)
            {
                await _refreshTokenRepository.RevokeAllAsync(token.UserId, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                throw new RefreshTokenCompromisedException();
            }
            if (token.ExpiresUTC < TokenParameters.GetExpiredThreshold()) 
                throw new RefreshTokenExpiredException();

            await _refreshTokenRepository.RevokeAsync(token, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return await GenerateTokens(token.UserId, token.DeviceName, gameSessionId, ct);
        }
        public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            RefreshToken? token = await _refreshTokenRepository.GetAsync(refreshToken, ct)
                ?? throw new RefreshTokenNotFoundException();
            await _refreshTokenRepository.RevokeAsync(token, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        private async Task<AuthResult> GenerateTokens(Guid userId, string deviceName, Guid? gameSessionId, CancellationToken ct = default)
        {
            RefreshToken refreshToken = _tokenRepository.GenerateRefreshToken(userId, deviceName);
            await _refreshTokenRepository.CreateAsync(refreshToken, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            string accessToken = _tokenRepository.GenerateAccessToken(userId, gameSessionId);
            return new AuthResult(refreshToken.Token, accessToken);
        }
    }
}
