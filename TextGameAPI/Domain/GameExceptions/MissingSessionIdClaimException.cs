using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class MissingSessionIdClaimException : GameException
    {
        public MissingSessionIdClaimException() : base(ExceptionsLabels.MissingGameSessionIdClaimCode, ExceptionsLabels.MissingGameSessionIdClaimMessage) { }
    }
}