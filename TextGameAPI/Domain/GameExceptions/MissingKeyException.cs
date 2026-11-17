using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class MissingKeyException : GameException
    {
        public MissingKeyException() : base(ExceptionsLabels.MissingKeyCode, ExceptionsLabels.MissingKeyText) { }
    }
}
