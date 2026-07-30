using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NotSaveOwnerException : GameException
    {
        public NotSaveOwnerException() : base(ExceptionsLabels.NotSaveOwnerCode, ExceptionsLabels.NotSaveOwnerMessage) { }
    }
}