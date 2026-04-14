namespace TextGame.Presentation.DTO
{
    public record BattleLog(string target, int damageToTarget, int? targetHealthBeforeAttack, int? targetHealthAfterAttack, string attacker, int damageToAttacker, int? healthBeforeAttack, int? healthAfterAttack);
}