namespace TextGame.Domain.GameExceptions
{
    public class LockedException : GameException
    {
        public LockedException() : base("LOCKED", "Сундук заперт!") { }
    }
}