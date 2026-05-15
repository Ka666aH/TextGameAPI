using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Generators;
using TextGame.Domain.Entities;

namespace TextGame.Application.Factories
{
    public class GameSessionFactory : IGameSessionFactory
    {
        private readonly IMapGenerator _mapGenerator;

        public GameSessionFactory(IMapGenerator mapGenerator)
        {
            _mapGenerator = mapGenerator;
        }

        public GameSession CreateGameSession(Guid userId, string? name = null)
        {
            GameSession gameSession = 
                name != null ? 
                new(userId, name) : 
                new(userId);
            var rooms = _mapGenerator.Generate();
            gameSession.Rooms = rooms;
            gameSession.CurrentRoom = rooms[0];
            return gameSession;
        }
    }
}
