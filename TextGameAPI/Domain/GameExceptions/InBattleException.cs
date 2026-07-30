using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class InBattleException : GameException
    {
        public InBattleException() : base(ExceptionsLabels.InBattleCode, ExceptionsLabels.InBattleText) { }
    }
}