namespace TextGame.Domain.Entities
{
    /// <summary>
    /// Исход партии. None — игра ещё идёт, а также состояние, созданное до 1.2.0
    /// (в старом JSON поля нет, десериализатор оставит значение по умолчанию).
    /// </summary>
    public enum GameResult
    {
        None,
        Win,
        Lose
    }
}
