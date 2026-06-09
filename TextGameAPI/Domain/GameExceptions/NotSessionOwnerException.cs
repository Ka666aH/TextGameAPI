using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NotSessionOwnerException : GameException
    {
        public NotSessionOwnerException() : base(ExceptionsLabels.NotSessionOwnerCode, ExceptionsLabels.NotSessionOwnerMessage) { }
    }
}