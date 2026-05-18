namespace TextGame.Domain.DTO
{
    public record WeaponAttackResult(int Damage, int SelfDamage = 0, bool IsWeaponBrokenDown = false);
}
