namespace TextGame.Domain.GameExceptions
{
    public class UnsellableItemException : GameException
    {
        public UnsellableItemException() : base("UNSELLABLE_ERROR", "Невозможно продать этот предмет!") { }
    }
}