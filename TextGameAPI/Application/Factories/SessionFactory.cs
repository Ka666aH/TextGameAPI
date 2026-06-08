using TextGame.Application.Interfaces.Factories;
using TextGame.Domain.Entities;

namespace TextGame.Application.Factories
{
    public class SessionFactory : ISessionFactory
    {        
        public Session CreateGameSession(Guid userId, string? name = null)
        {
            Session gameSession = 
                name != null ? 
                new(userId, name) : 
                new(userId);
            return gameSession;
        }
    }
}