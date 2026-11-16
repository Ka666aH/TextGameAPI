using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Factories
{
    public interface ISaveFactory
    {
        Save CreateInitial(Guid sessionId, long seed);
        Save CreateManual(Guid sessionId, string? name, State state);
        Save CreateAuto(Guid sessionId, State state);
    }
}