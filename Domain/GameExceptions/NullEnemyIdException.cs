using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NullEnemyIdException : GameException
    {
        public NullEnemyIdException() : base(ExceptionLabels.NullEnemyIdCode, ExceptionLabels.NullEnemyIdText) { }
    }
}