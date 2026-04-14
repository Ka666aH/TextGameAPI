namespace TextGame.Domain.GameObjects.Items.Equipments.Armors.Helms
{
    public class WoodenBucket : Helm
    {

        public WoodenBucket(int itemId, int roomId, bool fromShop)
            : base("ДЕРЕВЯННОЕ ВЕДРО",
                  "Старое дырявое ведро. Кто в своём уме наденет его на голову?",
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.WoodenBucketBaseDurability,
                  GameBalance.WoodenBucketBaseDamageBlock)
        { }
    }
}