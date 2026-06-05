namespace TextGame.Domain.Entities
{
    public class GameSession
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public User User { get; private set; }
        public string Name { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public GameSession(Guid userId, string name = GameText.GeneralLabeles.GameSessionDefaultName)
        {
            UserId = userId;
            Name = name;
        }
        private GameSession() { }
    }
}