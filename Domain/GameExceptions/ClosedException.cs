namespace TextGame.Domain.GameExceptions
{
    public class ClosedException : GameException
    {
        public ClosedException() : base("CLOSED", "Сундук закрыт!") { }
    }
}