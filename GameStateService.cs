using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace TextGame
{
    public interface IGameStateService
    {
        GameState GetGameState();
        void SaveGameState(GameState gameState);
        void ClearGameState();
    }

    public class GameState
    {
        public Room? CurrentRoom { get; set; }
        public List<Item> Inventory { get; set; } = new List<Item>();
        public int Coins { get; set; }
        public bool IsGameStarted { get; set; }
    }

    public class GameStateService : IGameStateService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string GameStateKey = "GameState";

        public GameStateService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public GameState GetGameState()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
                return new GameState();

            var gameStateJson = session.GetString(GameStateKey);
            if (string.IsNullOrEmpty(gameStateJson))
                return new GameState();

            try
            {
                return JsonSerializer.Deserialize<GameState>(gameStateJson) ?? new GameState();
            }
            catch
            {
                return new GameState();
            }
        }

        public void SaveGameState(GameState gameState)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
                return;

            var gameStateJson = JsonSerializer.Serialize(gameState);
            session.SetString(GameStateKey, gameStateJson);
        }

        public void ClearGameState()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(GameStateKey);
        }
    }
} 