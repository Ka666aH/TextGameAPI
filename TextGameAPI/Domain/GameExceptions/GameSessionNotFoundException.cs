using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class GameSessionNotFoundException : GameException
    {
        public GameSessionNotFoundException() : base(ExceptionsLabels.GameSessionNotFoundCode, ExceptionsLabels.GameSessionNotFoundMessage) { }
    }
}