using System.Security.Claims;
using TextGame.Infrastructure.Token;

namespace TextGame.Presentation.Helpers
{
    public static class ClaimsHelper
    {
        public static bool TryGetUserId(this ClaimsPrincipal user, out Guid userId)
        {
            userId = default;
            var claimValue = user.FindFirst(AccessClaims.UserId)?.Value;
            return claimValue != null && Guid.TryParse(claimValue, out userId);
        }
        public static bool TryGetSessionId(this ClaimsPrincipal user, out Guid sessionId)
        {
            sessionId = default;
            var claimValue = user.FindFirst(AccessClaims.SessionId)?.Value;
            return claimValue != null && Guid.TryParse(claimValue, out sessionId);
        }
    }
}