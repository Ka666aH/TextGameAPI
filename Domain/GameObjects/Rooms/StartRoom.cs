namespace TextGame.Domain.GameObjects.Rooms
{
    public class StartRoom : Room
    {
        public StartRoom() 
            : base("СТАРТОВАЯ КОМАНТА", "В потолке дыра, через которую Вы сюда провалились.", 0)
        {
            IsDiscovered = true;
        }
    }
}
