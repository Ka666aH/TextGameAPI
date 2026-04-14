namespace TextGame.Domain.GameObjects.Rooms
{
    public class EndRoom : Room
    {
        public EndRoom(int number) : base("ВЫХОД", "Выход наружу. Свобода.", number) { }
    }
}
