using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TextGame.Infrastructure.Token.JWT
{
    public static class JwtKeyProvider
    {
        /// <summary>
        /// HMAC-SHA256 требует ключ не короче 256 бит — 32 байта. appsettings и .env уже
        /// содержат секреты нужной длины; проверка ловит короткий/пустой секрет на старте,
        /// а не в момент первого логина.
        /// </summary>
        public const int MinimumSecretLength = 32;

        private static SymmetricSecurityKey? _instance;
        public static SymmetricSecurityKey Instance =>
            _instance ?? throw new InvalidOperationException("JWT key not initialized");

        public static void Initialize(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException(
                    $"JWT secret is not configured (JwtSettings:Secret). Set a secret of at least {MinimumSecretLength} characters.");

            if (secret.Length < MinimumSecretLength)
                throw new InvalidOperationException(
                    $"JWT secret is too short: {secret.Length} characters. HMAC-SHA256 needs at least {MinimumSecretLength} (JwtSettings:Secret).");

            _instance = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        }
    }
}
