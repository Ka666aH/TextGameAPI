namespace TextGame.Domain.GameExceptions
{
    public class NotShopException : GameException
    {
        public NotShopException() : base("NOT_IN_SHOP", "Невозможно вне магазина!") { }
    }
}