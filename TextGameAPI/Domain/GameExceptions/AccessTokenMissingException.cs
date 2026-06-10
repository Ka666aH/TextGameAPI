using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class AccessTokenMissingException : GameException
    {
        public AccessTokenMissingException() : base(ExceptionsLabels.AccessTokenMissingCode, ExceptionsLabels.AccessTokenMissingMessage) { }
    }
}
