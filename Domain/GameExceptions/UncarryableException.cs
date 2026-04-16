using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class UncarryableException : GameException
    {
        public UncarryableException() : base(ExceptionLabels.UncarryableCode, ExceptionLabels.UncarryableText) { }
    }
}