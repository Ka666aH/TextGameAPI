using TextGame.Domain.DTO;

namespace TextGame.Presentation.DTO
{
    public record BattleWinDTO(string Message, BattleLog BattleLog);
}