using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class ImpossibleStealException : GameException
    {
        public ImpossibleStealException() : base(ExceptionLabels.ImpossibleStealCode, ExceptionLabels.ImpossibleStealText) { }
    }
}