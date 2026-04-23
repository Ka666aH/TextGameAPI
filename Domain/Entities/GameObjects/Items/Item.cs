namespace TextGame.Domain.Entities.GameObjects.Items
{
    public abstract class Item : GameObject
    {
        public int? Cost { get; protected set; }
        public bool IsCarryable { get; init; }
        public Item(int id, string name, string description, bool isCarryable = true) : base(id, name, description)
        {
            IsCarryable = isCarryable;
        }
        public virtual void AddStoreMargin() => Cost = (int)(Cost! * GameBalance.StoreMargin);
        protected Item() { }
    }
}