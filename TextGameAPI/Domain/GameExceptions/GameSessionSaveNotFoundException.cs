using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class GameSessionSaveNotFoundException : GameException
    {
        public GameSessionSaveNotFoundException() : base(ExceptionsLabels.GameSessionSaveNotFoundCode, ExceptionsLabels.GameSessionSaveNotFoundMessage) { }
    }
}