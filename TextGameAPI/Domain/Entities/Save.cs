using System.Security.Cryptography;
using System.Text;
using TextGame.Infrastructure.JSON;

namespace TextGame.Domain.Entities
{
    public class Save
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid SessionId { get; private set; }
        public Session Session { get; private set; }
        public SaveType Type { get; private set; }
        public string Name { get; private set; }
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
        private Save() { }
        private static string ComputeHash(State state)
        {
            var json = StateSerializer.Serialize(state);
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        }
    }
}