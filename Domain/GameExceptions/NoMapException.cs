using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NoMapException : GameException
    {
        public NoMapException() : base(ExceptionsLabels.NoMapCode, ExceptionsLabels.NoMapText) { }
    }
}