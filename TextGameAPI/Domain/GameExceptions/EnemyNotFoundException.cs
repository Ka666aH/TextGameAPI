using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class EnemyNotFoundException : GameException
    {
        public EnemyNotFoundException() : base(ExceptionsLabels.EnemyNotFoundCode, ExceptionsLabels.EnemyNotFoundMessage) { }
    }
}
