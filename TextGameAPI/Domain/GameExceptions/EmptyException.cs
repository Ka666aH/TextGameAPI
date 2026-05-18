using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class EmptyException : GameException
    {
        public EmptyException() : base(ExceptionsLabels.EmptyCode, ExceptionsLabels.EmptyText) { }
    }
}