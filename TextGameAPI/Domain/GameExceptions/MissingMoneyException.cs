using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class MissingMoneyException : GameException
    {
        public MissingMoneyException() : base(ExceptionsLabels.MissingMoneyCode, ExceptionsLabels.MissingMoneyText) { }
    }
}
