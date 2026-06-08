using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Factories
{
    public interface ISessionFactory
    {
        Session CreateGameSession(Guid userId, string? name = null);
    }
}
