using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class LockedException : GameException
    {
        public LockedException() : base(ExceptionLabels.LockedCode, ExceptionLabels.LockedText) { }
    }
}