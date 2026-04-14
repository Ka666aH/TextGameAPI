namespace TextGame.Domain.GameExceptions
{
    public class EmptyException : GameException
    {
        public EmptyException() : base("EMPTY_ERROR", "Тут ничего нет!") { }
    }
}