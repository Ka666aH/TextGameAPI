namespace TextGame.Domain.GameExceptions
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
}