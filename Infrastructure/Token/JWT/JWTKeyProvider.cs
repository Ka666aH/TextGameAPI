using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TextGame.Infrastructure.Token.JWT
{
    public static class JwtKeyProvider
    {
        private static SymmetricSecurityKey? _instance;
        public static SymmetricSecurityKey Instance =>
            _instance ?? throw new InvalidOperationException("JWT key not initialized");

        public static void Initialize(string secret)
        {
            _instance = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        }
    }
}
