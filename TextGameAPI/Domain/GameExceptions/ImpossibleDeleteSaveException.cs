using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class ImpossibleDeleteSaveException : GameException
    {
        public ImpossibleDeleteSaveException() : base(ExceptionsLabels.ImpossibleDeleteGameSessionSaveCode, ExceptionsLabels.ImpossibleDeleteGameSessionSaveMessage) { }
    }
}