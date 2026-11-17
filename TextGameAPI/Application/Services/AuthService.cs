using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
        private readonly ISessionRepository _sessionRepository;
        private readonly RefreshReplayCache _replayCache;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            IValidator<RegisterCommand> registerValidator, 
            IHasher hasher, 
            IUserRepository userRepository, 
            ITokenRepository tokenRepository, 
            IRefreshTokenRepository refreshTokenRepository, 
            ISessionRepository sessionRepository, 
            RefreshReplayCache replayCache,
            IUnitOfWork unitOfWork)
        {
            _registerValidator = registerValidator;
            _hasher = hasher;
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _sessionRepository = sessionRepository;
            _replayCache = replayCache;
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

            try
            {
                await _userRepository.CreateAsync(user, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                // notes/AGENTS.md §2.5: two parallel registrations both pass the check above and
                // the database rejects the second one. That is a duplicate login, not a 500.
                throw new ValidationException([new("Login", ValidatorsText.LoginAlreadyExist)]);
            }
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
        public async Task<MeResult> GetCurrentAsync(Guid userId, Guid? sessionId, CancellationToken ct = default)
        {
            User user = await _userRepository.GetAsync(userId, ct)
                ?? throw new UserNotFoundException();

            // Claim мог пережить удаление игры: это не ошибка авторизации, просто нет активной сессии.
            string? sessionName = null;
            if (sessionId.HasValue)
            {
                Session? session = await _sessionRepository.GetAsync(sessionId.Value, ct);
                if (session == null || session.UserId != userId) sessionId = null;
                else sessionName = session.Name;
            }

            return new MeResult(userId, user.Login, sessionId, sessionName);
        }
        public async Task<AuthResult> RefreshAsync(string refreshToken, string accessToken, string fingerprint, CancellationToken ct = default)
        {
            string hashedFingerprint = _hasher.Hash(fingerprint);

            RefreshToken? token = await _refreshTokenRepository.GetAsync(RefreshTokenHash.Of(refreshToken), ct);
            if (token == null)
            {
                // notes/AGENTS.md §2.1: the losing half of a double submit arrives after the
                // winner already rotated the token. Replay the winner's result instead of
                // throwing, which the middleware would turn into "logged out".
                if (_replayCache.TryReplay(refreshToken, hashedFingerprint, out AuthResult? replayed))
                    return replayed!;

                throw new RefreshTokenNotFoundException();
            }

            bool fingerprintIsCorrect = _hasher.Verify(fingerprint, token.HashedFingerprint);
            if (!fingerprintIsCorrect)
            {
                await _refreshTokenRepository.DeleteAllAsync(token.UserId, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                throw new RefreshTokenCompromisedException();
            }
            if (token.ExpiresUTC < DateTime.UtcNow)
                throw new RefreshTokenExpiredException();

            await _refreshTokenRepository.DeleteAsync(token, ct);
            //await _unitOfWork.SaveChangesAsync(ct);

            Guid? sessionId = null;
            if (accessToken != "")
            {
                var principal = _tokenRepository.ReadTokenWithoutLifetime(accessToken);
                var claimValue = principal.FindFirst(AccessClaims.SessionId)?.Value;
                if (claimValue != null && Guid.TryParse(claimValue, out var parsed))
                {
                    var session = await _sessionRepository.GetAsync(parsed, ct);
                    if (session == null || session.UserId != token.UserId) sessionId = null;
                    else sessionId = parsed;
                }
            }
            
            AuthResult result = await GenerateTokens(token.UserId, token.HashedFingerprint, sessionId, ct);
            _replayCache.Store(refreshToken, token.HashedFingerprint, result);

            return result;
        }
        public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            // Logout получает сырой токен из cookie, в БД лежит только его хэш.
            RefreshToken? token = await _refreshTokenRepository.GetAsync(RefreshTokenHash.Of(refreshToken), ct)
                ?? throw new RefreshTokenNotFoundException();
            await _refreshTokenRepository.DeleteAsync(token, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        private async Task<AuthResult> GenerateTokens(Guid userId, string hashedFingerprint, Guid? sessionId, CancellationToken ct = default)
        {
            // Сырой refresh-токен существует только до записи в БД: наружу — в cookie, в БД — хэш.
            var (rawRefreshToken, refreshToken) = _tokenRepository.GenerateRefreshToken(userId, hashedFingerprint);
            await _refreshTokenRepository.CreateAsync(refreshToken, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            string accessToken = _tokenRepository.GenerateAccessToken(userId, sessionId);
            return new AuthResult(rawRefreshToken, accessToken);
        }
        // Postgres reports a unique violation as SQLSTATE 23505; EF wraps it in DbUpdateException.
        private static bool IsUniqueViolation(DbUpdateException exception)
        {
            for (Exception? current = exception; current != null; current = current.InnerException)
            {
                if (current.Message.Contains("23505", StringComparison.Ordinal) ||
                    current.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
                    current.Message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
