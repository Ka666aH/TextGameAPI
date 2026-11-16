namespace TextGame.Domain.Entities
{
    public class Session
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        // Навигация: заполняется EF Core при материализации, вручную не присваивается.
        public User User { get; private set; } = null!;
        // init + null! вместо required — см. комментарий в User.cs.
        public string Name { get; init; } = null!;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public Session(Guid userId, string name = GameText.GeneralLabels.SessionDefaultName)
        {
            UserId = userId;
            Name = name;
        }
        // EF Core материализует сущность через этот конструктор и заполняет свойства из БД рефлексией.
        private Session() { }
    }
}
