using TextGame.Application.DTO;
using TextGame.Domain.DTO;
using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class BattleWinException : GameException
    {
        public BattleLog BattleLog { get; }
        public GameInfoDTO GameInfo { get; }
        public BattleWinException(string message, BattleLog battleLog, GameInfoDTO gameInfo) : base(ExceptionsLabels.BattleWinCode, message)
        {
            BattleLog = battleLog;
            GameInfo = gameInfo;
        }
    }
}
