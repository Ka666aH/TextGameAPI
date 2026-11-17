namespace TextGame.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        // Навигация: заполняется EF Core при материализации, вручную не присваивается.
        public User User { get; private set; } = null!;
        // В БД хранится только SHA-256 сырого токена (RefreshTokenHash); сырой уходит исключительно в cookie.
        // init + null! вместо required — см. комментарий в User.cs.
        public string Token { get; init; } = null!;
        public DateTime ExpiresUTC { get; private set; }
        public string HashedFingerprint { get; init; } = null!;
        public RefreshToken(Guid userId, string token, DateTime expiresUTC, string hashedFingerprint)
        {
            UserId = userId;
            Token = token;
            ExpiresUTC = expiresUTC;
            HashedFingerprint = hashedFingerprint;
        }
        // EF Core материализует сущность через этот конструктор и заполняет свойства из БД рефлексией.
        private RefreshToken() { }
    }
}
