namespace TextGame.Domain.GameExceptions
{
    public class InvalidIdException : GameException
    {
        public InvalidIdException(string code, string message) : base(code, message) { }
    }
}