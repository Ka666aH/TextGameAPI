using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NullItemIdException : GameException
    {
        public NullItemIdException() : base(ExceptionLabels.NullItemIdCode, ExceptionLabels.NullItemIdText) { }
    }
}