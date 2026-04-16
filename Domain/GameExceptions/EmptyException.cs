using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class EmptyException : GameException
    {
        public EmptyException() : base(ExceptionLabels.EmptyCode, ExceptionLabels.EmptyText) { }
    }
}