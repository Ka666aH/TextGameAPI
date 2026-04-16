using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class UnsellableItemException : GameException
    {
        public UnsellableItemException() : base(ExceptionLabels.UnsellableItemCode, ExceptionLabels.UnsellableItemText) { }
    }
}