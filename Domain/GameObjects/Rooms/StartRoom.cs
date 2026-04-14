namespace TextGame.Domain.GameObjects.Rooms
{
    public class StartRoom : Room
    {
        public StartRoom(string name, string description, int number) : base(name, description, number)
        {
            IsDiscovered = true;
        }
    }
}
