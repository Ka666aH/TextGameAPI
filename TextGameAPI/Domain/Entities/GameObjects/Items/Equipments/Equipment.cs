namespace TextGame.Domain.Entities.GameObjects.Items.Equipments
{
    public abstract class Equipment : Item
    {
        protected readonly int _roomId;
        protected readonly bool _fromShop;

        public int? Durability { get; protected set; }
        public Equipment(int id, string name, string description, int? durability, int roomId, bool fromShop)
            : base(id, name, description, roomId)
        {
            _roomId = roomId;
            _fromShop = fromShop;

            Durability = durability;
        }
        protected Equipment() { }
    }
}
