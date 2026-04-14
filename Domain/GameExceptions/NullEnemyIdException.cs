namespace TextGame.Domain.GameExceptions
{
    public class NullEnemyIdException : GameException
    {
        public NullEnemyIdException() : base("ENEMY_NOT_FOUND", "Противник не найден.") { }
    }
}