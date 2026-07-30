using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
using TextGame.Domain.Entities;
using TextGame.Domain.GameText;

namespace TextGame.Application.Factories
{
    public class SaveFactory : ISaveFactory
    {
        private readonly IMapGenerator _mapGenerator;

        public SaveFactory(IMapGenerator mapGenerator)
        {
            _mapGenerator = mapGenerator;
        }
        public Save CreateInitial(Guid sessionId)
        {
            State state = new();
            state.Rooms = _mapGenerator.Generate();
            state.CurrentRoomId = 0;
            return new(sessionId, SaveType.Initial, GeneralLabels.SaveInitialDefaultName, state);
        }

        public Save CreateManual(Guid sessionId, string? name, State state) =>
            new(
                sessionId,
                SaveType.Manual,
                name ?? string.Format(GeneralLabels.SaveManualDefaultName, DateTime.UtcNow),
                state);

        public Save CreateAuto(Guid sessionId, State state) =>
            new(
                sessionId,
                SaveType.Auto,
                string.Format(GeneralLabels.SaveAutoDefaultName, DateTime.UtcNow),
                state);
    }
}
