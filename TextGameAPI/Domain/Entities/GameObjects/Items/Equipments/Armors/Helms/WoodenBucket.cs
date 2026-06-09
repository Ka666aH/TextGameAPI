using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms
{
    public class WoodenBucket : Helm
    {

        public WoodenBucket(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabels.WoodenBucketName,
                  ItemsLabels.WoodenBucketDescription,
                  roomId,
                  fromShop,
                  GameBalance.WoodenBucketBaseDurability,
                  GameBalance.WoodenBucketBaseDamageBlock)
        { }
        private WoodenBucket() { }
    }
}