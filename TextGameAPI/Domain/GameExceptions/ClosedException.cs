using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class ClosedException : GameException
    {
        public ClosedException() : base(ExceptionsLabels.ClosedCode, ExceptionsLabels.ClosedText) { }
    }
}