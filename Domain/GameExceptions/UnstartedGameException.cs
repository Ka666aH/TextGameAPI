namespace TextGame.Domain.GameExceptions
{
    public class UnstartedGameException : GameException
    {
        public UnstartedGameException() : base("NOT_STARTED", "Игра ещё не начата!") { }
    }
}