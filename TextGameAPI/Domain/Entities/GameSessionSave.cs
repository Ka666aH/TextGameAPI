using System.Security.Cryptography;
using System.Text;
using TextGame.Infrastructure.JSON;

namespace TextGame.Domain.Entities
{
    public class GameSessionSave
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid GameSessionId { get; private set; }
        public GameSession GameSession { get; private set; }
        public SaveType Type { get; private set; }
        public string Name { get; private set; }
        public GameSessionState State { get; private set; } = new();
        public string? StateHash { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public GameSessionSave(Guid gameSessionId, SaveType saveType, string name, GameSessionState state)
        {
            GameSessionId = gameSessionId;
            Type = saveType;
            Name = name;
            State = state;
            StateHash = ComputeHash(state);
        }
        private GameSessionSave() { }
        private static string ComputeHash(GameSessionState state)
        {
            var json = GameSessionStateSerializer.Serialize(state);
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        }
    }
}