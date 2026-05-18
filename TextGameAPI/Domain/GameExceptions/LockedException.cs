using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class LockedException : GameException
    {
        public LockedException() : base(ExceptionsLabels.LockedCode, ExceptionsLabels.LockedText) { }
    }
}