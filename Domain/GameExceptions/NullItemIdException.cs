namespace TextGame.Domain.GameExceptions
{
    public class NullItemIdException : GameException
    {
        public NullItemIdException() : base("ITEM_NOT_FOUND", "Предмет не найден.") { }
    }
}