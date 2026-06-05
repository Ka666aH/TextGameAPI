using TextGame.Application.Interfaces.Factories;
using TextGame.Domain.Entities;

namespace TextGame.Application.Factories
{
    public class GameSessionFactory : IGameSessionFactory
    {        
        public GameSession CreateGameSession(Guid userId, string? name = null)
        {
            GameSession gameSession = 
                name != null ? 
                new(userId, name) : 
                new(userId);
            return gameSession;
        }
    }
}