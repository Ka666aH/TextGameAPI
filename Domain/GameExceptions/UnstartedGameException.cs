using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class UnstartedGameException : GameException
    {
        public UnstartedGameException() : base(ExceptionsLabels.UnstartedGameCode, ExceptionsLabels.UnstartedGameText) { }
    }
}