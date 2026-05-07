using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class AccessTokenNotFoundException : GameException
    {
        public AccessTokenNotFoundException() : base(ExceptionsLabels.AccessTokenNotFoundCode, ExceptionsLabels.AccessTokenNotFoundMessage) { }
    }
}