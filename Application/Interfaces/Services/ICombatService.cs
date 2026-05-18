using TextGame.Domain.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface ICombatService
    {
        BattleLog DealDamage();
        BattleLog GetDamage();
    }
}