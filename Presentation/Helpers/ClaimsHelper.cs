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
        public static bool TryGetGameSessionId(this ClaimsPrincipal user, out Guid gameSessionId)
        {
            gameSessionId = default;
            var claimValue = user.FindFirst(AccessClaims.GameSessionId)?.Value;
            return claimValue != null && Guid.TryParse(claimValue, out gameSessionId);
        }
    }
}
