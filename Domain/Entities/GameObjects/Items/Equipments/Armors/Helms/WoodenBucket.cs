using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms
{
    public class WoodenBucket : Helm
    {

        public WoodenBucket(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabeles.WoodenBucketName,
                  ItemsLabeles.WoodenBucketDescription,
                  roomId,
                  fromShop,
                  GameBalance.WoodenBucketBaseDurability,
                  GameBalance.WoodenBucketBaseDamageBlock)
        { }
        private WoodenBucket() { }
    }
}