using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
using TextGame.Domain.Entities;
using TextGame.Domain.GameText;

namespace TextGame.Application.Factories
{
    public class GameSessionSaveFactory : IGameSessionSaveFactory
    {
        private readonly IMapGenerator _mapGenerator;

        public GameSessionSaveFactory(IMapGenerator mapGenerator)
        {
            _mapGenerator = mapGenerator;
        }
        public GameSessionSave CreateInitialGameSessionSave(Guid gameSessionId)
        {
            GameSessionState state = new();
            var rooms = _mapGenerator.Generate();
            state.Rooms = rooms;
            state.CurrentRoomId = 0;
            return new(gameSessionId, SaveType.Initial, GeneralLabeles.GameSessionSaveInitialDefaultName, state);
        }

        public GameSessionSave CreateManualGameSessionSave(Guid gameSessionId, string? name, GameSessionState state) =>
            new(
                gameSessionId,
                SaveType.Manual,
                name ?? string.Format(GeneralLabeles.GameSessionSaveManualDefaultName, DateTime.UtcNow),
                state);

        public GameSessionSave CreateAutoGameSessionSave(Guid gameSessionId, GameSessionState state) =>
            new(
                gameSessionId,
                SaveType.Auto,
                string.Format(GeneralLabeles.GameSessionSaveAutoDefaultName, DateTime.UtcNow),
                state);
    }
}
