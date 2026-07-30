using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Factories
{
    public interface ISessionFactory
    {
        Session CreateSession(Guid userId, string? name = null);
    }
}
