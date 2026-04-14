using TextGame.Application.Interfaces.Services;

namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons
{
    public class Fists : Weapon
    {
        public static readonly Fists DefaultFists = new Fists();

        public Fists() : base("КУЛАКИ", "То, что есть (почти) у каждого. Базовое оружие самозащиты. Может быть больно.", null, null, GameBalance.FistsBaseDamage, 0, false) { }
        public override int Attack(IGameSessionService sessionService)
        {
            var (min, max) = GameBalance.CalculateSpread(GameBalance.FistsBaseDamage, sessionService.CurrentRoom!.Number);
            int damage = Random.Shared.Next(min, max + 1);
            if (Random.Shared.Next(GameBalance.FistSelfHarmProbabilityDivider) == 0)
                sessionService.AddCurrentHealth(-(int)(damage / GameBalance.FistSelfHarmDivider));
            return damage;
        }
    }
}