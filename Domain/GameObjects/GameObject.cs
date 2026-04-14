namespace TextGame.Domain.GameObjects
{
    public class GameObject
    {
        public string Name { get; protected set; } = "НЕИЗВЕСТНО";
        public string Description { get; protected set; } = "НЕОПИСУЕМО!";
        protected GameObject(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}