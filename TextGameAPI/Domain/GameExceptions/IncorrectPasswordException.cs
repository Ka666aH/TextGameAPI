using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class IncorrectPasswordException : GameException
    {
        public IncorrectPasswordException() : base(ExceptionsLabels.IncorrectPasswordCode, ExceptionsLabels.IncorrectPasswordMessage) { }
    }
}