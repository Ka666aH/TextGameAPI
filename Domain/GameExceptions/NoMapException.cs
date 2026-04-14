namespace TextGame.Domain.GameExceptions
{
    public class NoMapException : GameException
    {
        public NoMapException() : base("NO_MAP_ERROR", "Нет карты!") { }
    }
}