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
            var rooms = _mapGenerator.Generate();
            state.Rooms = rooms;
            state.CurrentRoomId = 0;
            return new(sessionId, SaveType.Initial, GeneralLabeles.SaveInitialDefaultName, state);
        }

        public Save CreateManual(Guid sessionId, string? name, State state) =>
            new(
                sessionId,
                SaveType.Manual,
                name ?? string.Format(GeneralLabeles.SaveManualDefaultName, DateTime.UtcNow),
                state);

        public Save CreateAuto(Guid sessionId, State state) =>
            new(
                sessionId,
                SaveType.Auto,
                string.Format(GeneralLabeles.SaveAutoDefaultName, DateTime.UtcNow),
                state);
    }
}
