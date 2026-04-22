namespace TextGame.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public User User { get; private set; }
        public string Token { get; private set; }
        public DateTime ExpiresUTC { get; private set; }
        public bool IsRevoked { get; private set; } = false;
        public string DeviceName { get; private set; }
        private RefreshToken() { }
        public RefreshToken(Guid userId, string token, DateTime expiresUTC, string deviceName)
        {
            UserId = userId;
            Token = token;
            ExpiresUTC = expiresUTC;
            DeviceName = deviceName;
        }
    }
}