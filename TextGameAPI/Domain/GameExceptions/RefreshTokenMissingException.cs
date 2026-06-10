using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class RefreshTokenMissingException : GameException
    {
        public RefreshTokenMissingException() : base(ExceptionsLabels.RefreshTokenMissingCode, ExceptionsLabels.RefreshTokenMissingMessage) { }
    }
}
