using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class MissingUserIdClaimException : GameException
    {
        public MissingUserIdClaimException() : base(ExceptionsLabels.MissingUserIdClaimCode, ExceptionsLabels.MissingUserIdClaimMessage) { }
    }
}