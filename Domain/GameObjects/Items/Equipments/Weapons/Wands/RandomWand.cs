using TextGame.Application.Interfaces.Services;

namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons.Wands
{
    public class RandomWand : Wand
    {
        public RandomWand(int itemId, int roomId, bool fromShop)
            : base("ЖЕЗЛ СЛУЧАЙНОСТЕЙ", "Странное магическое оружие. Становится сильнее со временем.", itemId, roomId, fromShop, GameBalance.RandomWandBaseDamage) { }
        public override int Attack(IGameSessionService sessionService)
        {
            int damage = (int)(Damage * GameBalance.ApplyGain(_roomId));
            return Random.Shared.Next(damage + 1);
        }
    }
}