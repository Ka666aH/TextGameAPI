namespace TextGame.Domain.GameObjects.Items.Equipments.Armors.Chestplates
{
    public class IronCuirass : Chestplate
    {
        public IronCuirass(int itemId, int roomId, bool fromShop)
            : base("ЖЕЛЕЗНАЯ КИРАСА",
                  "Тяжёлая и прочная. Имеет небольшой оттиск \"304\" на внутренней части.",
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.IronCuirassBaseDurability,
                  GameBalance.IronCuirassBaseDamageBlock)
        { }
    }
}