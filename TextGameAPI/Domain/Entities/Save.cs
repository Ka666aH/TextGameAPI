using System.Security.Cryptography;
using System.Text;
using TextGame.Infrastructure.JSON;

namespace TextGame.Domain.Entities
{
    public class Save
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid SessionId { get; private set; }
        // Навигация: заполняется EF Core при материализации, вручную не присваивается.
        public Session Session { get; private set; } = null!;
        public SaveType Type { get; private set; }
        // init + null! вместо required — см. комментарий в User.cs.
        public string Name { get; init; } = null!;
        public State State { get; private set; } = new();
        public string? StateHash { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public Save(Guid sessionId, SaveType saveType, string name, State state)
        {
            SessionId = sessionId;
            Type = saveType;
            Name = name;
            State = state;
            StateHash = ComputeHash(state);
        }
        // EF Core материализует сущность через этот конструктор и заполняет свойства из БД
        // рефлексией. Важно: StateHash здесь не вычисляется — пересчёт по десериализованному
        // State мог бы не совпасть с сохранённым и сломать сравнение автосейва.
        private Save() { }
        private static string ComputeHash(State state)
        {
            var json = StateSerializer.Serialize(state);
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        }
    }
}
