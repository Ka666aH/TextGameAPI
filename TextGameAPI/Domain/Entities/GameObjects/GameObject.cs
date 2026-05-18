using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects
{
    public abstract class GameObject
    {
        public int Id { get; init; }
        public string Name { get; protected set; } = GeneralLabeles.GameObjectDefaultName;
        public string Description { get; protected set; } = GeneralLabeles.GameObjectDefaultDescription;
        protected GameObject(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
        protected GameObject() { }
    }
}