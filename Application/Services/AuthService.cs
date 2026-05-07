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

        private readonly IHasher _hasher;

        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IValidator<RegisterCommand> registerValidator, IHasher hasher, IUserRepository userRepository, ITokenRepository tokenRepository, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
        {
            _registerValidator = registerValidator;
            _hasher = hasher;
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task RegisterAsync(string login, string password, CancellationToken ct = default)
        {
            var validationResult = await _registerValidator.ValidateAsync(new RegisterCommand(login, password), ct);
            if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

            string hashedPassword = _hasher.Hash(password);
            User user = new(login, hashedPassword);

            bool userExist = await _userRepository.GetAsync(login, ct) != null;
            if (userExist) throw new ValidationException([new("Login", ValidatorsText.LoginAlreadyExist)]);
            await _userRepository.CreateAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task<AuthResult> LogInAsync(string login, string password, string fingerprint, CancellationToken ct = default)
        {
            User user = await _userRepository.GetAsync(login, ct)
                ?? throw new UserNotFoundException();
            bool passwordIsCorrect = _hasher.Verify(password, user.HashedPass);
            if (!passwordIsCorrect) throw new IncorrectPasswordException();
            string hashedFingerprint = _hasher.Hash(fingerprint);
            return await GenerateTokens(user.Id, hashedFingerprint, null, ct);
        }
        public async Task<AuthResult> RefreshAsync(string refreshToken, string accessToken, string fingerprint, CancellationToken ct = default)
        {
            RefreshToken? token = await _refreshTokenRepository.GetAsync(refreshToken, ct)
                ?? throw new RefreshTokenNotFoundException();

            bool fingerprintIsCorrect = _hasher.Verify(fingerprint, token.HashedFingerprint);
            if (token.IsRevoked || !fingerprintIsCorrect)
            {
                await _refreshTokenRepository.RevokeAllAsync(token.UserId, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                throw new RefreshTokenCompromisedException();
            }
            if (token.ExpiresUTC < TokenParameters.GetExpiredThreshold())
                throw new RefreshTokenExpiredException();

            await _refreshTokenRepository.RevokeAsync(token, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var principal = _tokenRepository.ReadTokenWithoutLifetime(accessToken);
            Guid? gameSessionId = null;
            var claimValue = principal.FindFirst(AccessClaims.GameSessionId)?.Value;
            if (claimValue != null && Guid.TryParse(claimValue, out var parsed)) gameSessionId = parsed;

            return await GenerateTokens(token.UserId, token.HashedFingerprint, gameSessionId, ct);
        }
        public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            RefreshToken? token = await _refreshTokenRepository.GetAsync(refreshToken, ct)
                ?? throw new RefreshTokenNotFoundException();
            await _refreshTokenRepository.RevokeAsync(token, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        private async Task<AuthResult> GenerateTokens(Guid userId, string hashedFingerprint, Guid? gameSessionId, CancellationToken ct = default)
        {
            RefreshToken refreshToken = _tokenRepository.GenerateRefreshToken(userId, hashedFingerprint);
            await _refreshTokenRepository.CreateAsync(refreshToken, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            string accessToken = _tokenRepository.GenerateAccessToken(userId, gameSessionId);
            return new AuthResult(refreshToken.Token, accessToken);
        }
    }
}
