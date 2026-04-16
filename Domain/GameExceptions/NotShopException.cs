using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NotShopException : GameException
    {
        public NotShopException() : base(ExceptionLabels.NotShopCode, ExceptionLabels.NotShopText) { }
    }
}