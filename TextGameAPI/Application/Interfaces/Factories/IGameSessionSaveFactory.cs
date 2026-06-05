using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Factories
{
    public interface IGameSessionSaveFactory
    {
        GameSessionSave CreateInitialGameSessionSave(Guid gameSessionId);
        GameSessionSave CreateManualGameSessionSave(Guid gameSessionId, string? name, GameSessionState state);
        GameSessionSave CreateAutoGameSessionSave(Guid gameSessionId, GameSessionState state);
    }
}