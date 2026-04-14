namespace TextGame.Domain.GameObjects.Items
{
    public class Item : GameObject
    {
        public int? Id { get; init; }
        public int? Cost { get; protected set; }
        public bool IsCarryable { get; init; }
        public Item(string name, string description, int? id, bool isCarryable = true) : base(name, description)
        {
            Id = id;
            IsCarryable = isCarryable;
        }
        public virtual void AddStoreMargin() => Cost = (int)(Cost! * GameBalance.StoreMargin);
    }
}