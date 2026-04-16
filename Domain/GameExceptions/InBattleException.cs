using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class InBattleException : GameException
    {
        public InBattleException() : base(ExceptionLabels.InBattleCode, ExceptionLabels.InBattleText) { }
    }
}