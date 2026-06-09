using TextGame.Application.Interfaces.Repositories;

namespace TextGame.Infrastructure.PasswordHasher
{
    public class BCryptRepository : IHasher
    {
        public string Hash(string password) =>
            BCrypt.Net.BCrypt.EnhancedHashPassword(password, 11);

        public bool Verify(string password, string hashedPassword) =>
            BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
    }
}