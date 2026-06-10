using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NothingFoundException : GameException
    {
        public NothingFoundException() : base(ExceptionsLabels.NothingFoundCode, ExceptionsLabels.NothingFoundMessage) { }
    }
}
