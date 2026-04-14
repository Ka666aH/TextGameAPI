namespace TextGame.Domain.GameExceptions
{
    public class NoMoneyException : GameException
    {
        public NoMoneyException() : base("NO_MONEY", "Недостаточно средств!") { }
    }
}