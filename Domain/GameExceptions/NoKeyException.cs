namespace TextGame.Domain.GameExceptions
{
    public class NoKeyException : GameException
    {
        public NoKeyException() : base("NO_KEY_ERROR", "Нет ключа!") { }
    }
}