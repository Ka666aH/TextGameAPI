namespace TextGame.Domain.GameObjects.Rooms
{
    public class StartRoom : Room
    {
        public StartRoom(int number) 
            : base("СТАРТОВАЯ КОМАНТА", "В потолке дыра, через которую Вы сюда провалились.", number)
        {
            IsDiscovered = true;
        }
    }
}
