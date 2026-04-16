using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects
{
    public class GameObject
    {
        public string Name { get; protected set; } = GeneralLabeles.GameObjectDefaultName;
        public string Description { get; protected set; } = GeneralLabeles.GameObjectDefaultDescription;
        protected GameObject(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}