using TextGame.Domain.Entities.GameObjects.Items;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments
{
    public abstract class Equipment : Item
    {
        protected readonly int _roomId;
        protected readonly bool _fromShop;

        public int? Durability { get; protected set; }
        public Equipment(string name, string description, int? id, int? durability, int roomId, bool fromShop) : base(name, description, id, true)
        {
            _roomId = roomId;
            _fromShop = fromShop;

            Durability = durability;
        }
        protected Equipment() { }
    }
}
