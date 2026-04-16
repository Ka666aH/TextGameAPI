using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class UnstartedGameException : GameException
    {
        public UnstartedGameException() : base(ExceptionLabels.UnstartedGameCode, ExceptionLabels.UnstartedGameText) { }
    }
}