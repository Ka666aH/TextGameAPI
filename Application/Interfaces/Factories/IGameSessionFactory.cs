using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Factories
{
    public interface IGameSessionFactory
    {
        GameSession CreateGameSession(Guid userId, string? name = null);
    }
}
