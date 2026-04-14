namespace TextGame.Domain
{
    public record WeaponAttackResult(int Damage, int SelfDamage = 0, bool IsWeaponBrokenDown = false);
}
