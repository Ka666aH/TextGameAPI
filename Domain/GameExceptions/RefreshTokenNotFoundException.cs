using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class RefreshTokenNotFoundException : GameException
    {
        public RefreshTokenNotFoundException() : base(ExceptionsLabels.RefreshTokenNotFoundCode, ExceptionsLabels.RefreshTokenNotFoundMessage) { }
    }
}