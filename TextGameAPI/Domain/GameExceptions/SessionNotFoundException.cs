using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class SessionNotFoundException : GameException
    {
        public SessionNotFoundException() : base(ExceptionsLabels.GameSessionNotFoundCode, ExceptionsLabels.GameSessionNotFoundMessage) { }
    }
}