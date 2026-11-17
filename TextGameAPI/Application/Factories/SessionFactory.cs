using TextGame.Application.Interfaces.Factories;
using TextGame.Domain.Entities;

namespace TextGame.Application.Factories
{
    public class SessionFactory : ISessionFactory
    {        
        public Session CreateSession(Guid userId, string? name = null, long? seed = null) => new(userId, name, seed);
    }
}