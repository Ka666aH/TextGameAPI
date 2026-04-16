using TextGame.Domain.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface ICombatService
    {
        //here переделать в void вынести создание лога?
        BattleLog DealDamage();
        BattleLog GetDamage();
    }
}