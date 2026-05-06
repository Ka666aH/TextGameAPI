using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NoKeyException : GameException
    {
        public NoKeyException() : base(ExceptionsLabels.NoKeyCode, ExceptionsLabels.NoKeyText) { }
    }
}