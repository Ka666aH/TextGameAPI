using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms
{
    public class WoodenBucket : Helm
    {

        public WoodenBucket(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.WoodenBucketName,
                  ItemsLabeles.WoodenBucketDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.WoodenBucketBaseDurability,
                  GameBalance.WoodenBucketBaseDamageBlock)
        { }
    }
}