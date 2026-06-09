namespace TextGame.Domain.Entities
{
    public class Session
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public User User { get; private set; }
        public string Name { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public Session(Guid userId, string name = GameText.GeneralLabels.SessionDefaultName)
        {
            UserId = userId;
            Name = name;
        }
        private Session() { }
    }
}