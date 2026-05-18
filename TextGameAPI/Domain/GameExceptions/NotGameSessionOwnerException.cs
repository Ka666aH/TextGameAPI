using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NotGameSessionOwnerException : GameException
    {
        public NotGameSessionOwnerException() : base(ExceptionsLabels.NotGameSessionOwnerCode, ExceptionsLabels.NotGameSessionOwnerMessage) { }
    }
}