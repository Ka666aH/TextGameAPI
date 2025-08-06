namespace TextGame
{
    public class GameException : Exception
    {
        public string Code { get; }

        protected GameException(string code, string message) : base(message)
        {
            Code = code;
        }
        protected GameException(string code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }
    }
    public class InvalidIdException : GameException
    {
        public InvalidIdException(string code,string message) : base(code, message) { }
    }
    public class NullIdException : GameException
    {
        public NullIdException(string code, string message) : base(code, message) { }
    }
    public class UnstartedGameException : GameException
    {
        public UnstartedGameException() : base("NOT_STARTED", "Игра ещё не начата!") { }
    }
    public class EmptyException : GameException
    {
        public EmptyException() : base("EMPTY_ERROR", "Тут ничего нет!") { }
    }
    public class UncarryableException : GameException
    {
        public UncarryableException() : base("UNCARRYABLE_ERROR", "Невозможно поднять этот предмет!") { }
    }
    public class LockedException : GameException
    {
        public LockedException() : base("LOCKED", "Сундук заперт!") { }
    }
    public class NoKeyException : GameException
    {
        public NoKeyException() : base("NO_KEY_ERROR", "Нет ключа!") { }
    }
    public class ClosedException : GameException
    {
        public ClosedException() : base("CLOSED", "Сундук закрыт!") { }
    }
    public class DefeatException : GameException
    {
        public GameOverStatsDTO GameOverStats { get; }
        public DefeatException(string message, GameOverStatsDTO gameOverStats) : base("DEFEAT", message) 
        {
            this.GameOverStats = gameOverStats;
        }
    }
}