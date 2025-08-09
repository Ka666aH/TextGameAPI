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
        public InvalidIdException(string code, string message) : base(code, message) { }
    }
    public class NullIdException : GameException
    {
        public NullIdException(string code, string message) : base(code, message) { }
    }
    public class UnstartedGameException : GameException
    {
        public UnstartedGameException() : base("NOT_STARTED", "Игра ещё не начата!") { }
    }
    public class UndiscoveredRoomException : GameException
    {
        public UndiscoveredRoomException() : base("UNDISCOVERED_ROOM_ERROR", "Комната ещё не открыта.") { }
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
    public class NoMapException : GameException
    {
        public NoMapException() : base("NO_MAP_ERROR", "Нет карты!") { }
    }
    public class ClosedException : GameException
    {
        public ClosedException() : base("CLOSED", "Сундук закрыт!") { }
    }
    public class EndExeption : GameException
    {
        public GameOverStatsDTO GameOverStats { get; }
        public EndExeption(string code, string message, GameOverStatsDTO gameOverStats) : base(code, message)
        {
            this.GameOverStats = gameOverStats;
        }
    }
    public class DefeatException : EndExeption
    {
        public DefeatException(string message, GameOverStatsDTO gameOverStats) : base("DEFEAT", message, gameOverStats) { }
    }
    public class WinException : EndExeption
    {
        public WinException(GameOverStatsDTO gameOverStats) : base("WIN", "Вы нашли выход и выбрались наружу.", gameOverStats) { }
    }
}