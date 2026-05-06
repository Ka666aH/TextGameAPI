using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class RefreshTokenExpiredException : GameException
    {
        public RefreshTokenExpiredException() : base(ExceptionsLabels.RefreshTokenExpiredCode, ExceptionsLabels.RefreshTokenExpiredMessage) { }
    }
}