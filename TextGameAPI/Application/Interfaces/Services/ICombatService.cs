using TextGame.Application.Enums;
using TextGame.Domain.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface ICombatService
    {
        DealDamageOutcome DealDamage(out BattleLog battleLog);
        GetDamageOutcome GetDamage(out BattleLog battleLog);
    }
}