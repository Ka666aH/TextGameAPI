using TextGame.Domain.DTO;

namespace TextGame.Presentation.DTO
{
    public record BattleWinDTO(string message, BattleLog BattleLog);
}