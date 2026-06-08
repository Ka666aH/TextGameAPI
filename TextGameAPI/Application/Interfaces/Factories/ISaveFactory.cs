using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Factories
{
    public interface ISaveFactory
    {
        Save CreateInitialGameSessionSave(Guid gameSessionId);
        Save CreateManualGameSessionSave(Guid gameSessionId, string? name, State state);
        Save CreateAutoGameSessionSave(Guid gameSessionId, State state);
    }
}