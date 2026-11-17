using System.Security.Cryptography;
using System.Text;

namespace TextGame.Infrastructure.Token
{
    /// <summary>
    /// Refresh-токен в БД хранится только как SHA-256 (notes/AGENTS.md §1b, п.8): это
    /// высокоэнтропийная случайная строка, поэтому быстрого хэша достаточно — BCrypt нужен
    /// лишь для низкоэнтропийных данных (пароль, fingerprint). Утечка дампа БД больше не
    /// даёт рабочих токенов, а детерминированный хэш сохраняет поиск по уникальному индексу.
    /// </summary>
    public static class RefreshTokenHash
    {
        public static string Of(string rawToken) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
    }
}
