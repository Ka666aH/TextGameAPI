using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class MissingGameSessionIdClaimException : GameException
    {
        public MissingGameSessionIdClaimException() : base(ExceptionsLabels.MissingGameSessionIdClaimCode, ExceptionsLabels.MissingGameSessionIdClaimMessage) { }
    }
}