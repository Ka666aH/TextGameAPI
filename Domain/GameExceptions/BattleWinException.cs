using TextGame.Domain.DTO;
using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class BattleWinException : GameException
    {
        public BattleLog BattleLog { get; }
        public BattleWinException(string message, BattleLog battleLog) : base(ExceptionLabels.BattleWinCode, message)
        {
            BattleLog = battleLog;
        }
    }
}