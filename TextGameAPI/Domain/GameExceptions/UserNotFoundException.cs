using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class UserNotFoundException : GameException
    {
        public UserNotFoundException() : base(ExceptionsLabels.UserNotFoundCode, ExceptionsLabels.UserNotFoundMessage) { }
    }
}