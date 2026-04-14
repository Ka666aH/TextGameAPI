using TextGame.Presentation.DTO;

namespace TextGame.Domain.GameExceptions
{
    public class BattleWinException : GameException
    {
        public BattleLog BattleLog { get; }
        public BattleWinException(string message, BattleLog battleLog) : base("YOU_WIN_IN_BATTLE", message)
        {
            BattleLog = battleLog;
        }
    }
}