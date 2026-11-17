using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class CorruptStateException : GameException
    {
        public CorruptStateException() : base(ExceptionsLabels.CorruptStateCode, ExceptionsLabels.CorruptStateMessage) { }
        public CorruptStateException(Exception innerException)
            : base(ExceptionsLabels.CorruptStateCode, ExceptionsLabels.CorruptStateMessage, innerException) { }
    }
}
