using TextGame.Domain.DTO;
using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Wands
{
    public class RandomWand : Wand
    {
        public RandomWand(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.RandomWandName,
                  ItemsLabeles.RandomWandDescription, 
                  itemId, 
                  roomId, 
                  fromShop, 
                  GameBalance.RandomWandBaseDamage) { }
        public override WeaponAttackResult Attack(int roomId)
        {
            int damage = (int)(Damage * GameBalance.CalculateGain(_roomId));
            return new WeaponAttackResult(Random.Shared.Next(damage + 1));
        }
    }
}