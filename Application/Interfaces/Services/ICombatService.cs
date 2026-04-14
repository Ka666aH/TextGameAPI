using TextGame.Presentation.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface ICombatService
    {
        BattleLog DealDamage();
        BattleLog GetDamage();
    }
}