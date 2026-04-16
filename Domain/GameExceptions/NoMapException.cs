using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NoMapException : GameException
    {
        public NoMapException() : base(ExceptionLabels.NoMapCode, ExceptionLabels.NoMapText) { }
    }
}