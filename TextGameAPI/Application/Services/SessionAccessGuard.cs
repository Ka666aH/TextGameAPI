using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Presentation.Helpers;

namespace TextGame.Application.Services
{
    public class SessionAccessGuard : ISessionAccessGuard
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionAccessGuard(ISessionRepository sessionRepository, IHttpContextAccessor httpContextAccessor)
        {
            _sessionRepository = sessionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task EnsureOwnershipAsync(Guid sessionId, CancellationToken ct)
        {
            if (!_httpContextAccessor.HttpContext!.User.TryGetUserId(out Guid userId)) throw new MissingUserIdClaimException();

            Session session = await _sessionRepository.GetAsync(sessionId, ct)
                ?? throw new SessionNotFoundException();
            if (session.UserId != userId) throw new NotSessionOwnerException();
        }
    }
}