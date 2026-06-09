using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class SaveNotFoundException : GameException
    {
        public SaveNotFoundException() : base(ExceptionsLabels.SaveNotFoundCode, ExceptionsLabels.SaveNotFoundMessage) { }
    }
}