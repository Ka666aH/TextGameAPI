using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class RefreshTokenCompromisedException : GameException
    {
        public RefreshTokenCompromisedException() : base(ExceptionsLabels.RefreshTokenCompromisedCode, ExceptionsLabels.RefreshTokenCompromisedMessage) { }
    }
}