namespace TextGame.Domain.GameExceptions
{
    public class UncarryableException : GameException
    {
        public UncarryableException() : base("UNCARRYABLE_ERROR", "Невозможно поднять этот предмет!") { }
    }
}