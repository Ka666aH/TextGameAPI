using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class UncarryableException : GameException
    {
        public UncarryableException() : base(ExceptionsLabels.UncarryableCode, ExceptionsLabels.UncarryableText) { }
    }
}