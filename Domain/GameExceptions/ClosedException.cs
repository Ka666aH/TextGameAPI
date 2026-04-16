using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class ClosedException : GameException
    {
        public ClosedException() : base(ExceptionLabels.ClosedCode, ExceptionLabels.ClosedText) { }
    }
}