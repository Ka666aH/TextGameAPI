namespace TextGame.Domain.GameObjects.Rooms
{
    public class EmptyRoom : Room
    {
        public EmptyRoom(int number) : base("ПУСТАЯ КОМНАТА", "Ничего интересного.", number) { }
    }
}
