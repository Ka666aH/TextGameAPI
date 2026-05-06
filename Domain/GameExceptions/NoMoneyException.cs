using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NoMoneyException : GameException
    {
        public NoMoneyException() : base(ExceptionsLabels.NoMoneyCode, ExceptionsLabels.NoMoneyText) { }
    }
}