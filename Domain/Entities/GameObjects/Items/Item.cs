using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Domain.Entities.GameObjects.Items
{
    public abstract class Item : GameObject
    {
        public int? RoomId { get; protected set; }
        public Room? Room { get; protected set; }
        public int? Cost { get; protected set; }
        public bool IsCarryable { get; init; }
        public Item(int id, string name, string description, int roomId, bool isCarryable = true) : base(id, name, description)
        {
            RoomId = roomId;
            IsCarryable = isCarryable;
        }
        public virtual void AddStoreMargin() => Cost = (int)(Cost! * GameBalance.StoreMargin);
        protected Item() { }
    }
}