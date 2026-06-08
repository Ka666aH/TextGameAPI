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
        public Save CreateInitialGameSessionSave(Guid gameSessionId)
        {
            State state = new();
            var rooms = _mapGenerator.Generate();
            state.Rooms = rooms;
            state.CurrentRoomId = 0;
            return new(gameSessionId, SaveType.Initial, GeneralLabeles.GameSessionSaveInitialDefaultName, state);
        }

        public Save CreateManualGameSessionSave(Guid gameSessionId, string? name, State state) =>
            new(
                gameSessionId,
                SaveType.Manual,
                name ?? string.Format(GeneralLabeles.GameSessionSaveManualDefaultName, DateTime.UtcNow),
                state);

        public Save CreateAutoGameSessionSave(Guid gameSessionId, State state) =>
            new(
                gameSessionId,
                SaveType.Auto,
                string.Format(GeneralLabeles.GameSessionSaveAutoDefaultName, DateTime.UtcNow),
                state);
    }
}
