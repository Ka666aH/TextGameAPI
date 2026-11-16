using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms
{
    public class WoodenBucket : Helm
    {

        public WoodenBucket(int id, int roomId, bool fromShop, Random? random = null)
            : base(id,
                  ItemsLabels.WoodenBucketName,
                  ItemsLabels.WoodenBucketDescription,
                  roomId,
                  fromShop,
                  GameBalance.WoodenBucketBaseDurability,
                  GameBalance.WoodenBucketBaseDamageBlock, random)
        { }
        private WoodenBucket() { }
    }
}