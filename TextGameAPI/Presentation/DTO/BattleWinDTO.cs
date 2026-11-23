using TextGame.Application.DTO;
using TextGame.Domain.DTO;

namespace TextGame.Presentation.DTO
{
    public record BattleWinDTO(string Message, BattleLog BattleLog, GameInfoDTO GameInfo);
}
