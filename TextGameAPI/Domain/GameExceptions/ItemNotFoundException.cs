using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class ItemNotFoundException : GameException
    {
        public ItemNotFoundException() : base(ExceptionsLabels.ItemNotFoundCode, ExceptionsLabels.ItemNotFoundMessage) { }
    }
}
