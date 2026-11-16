using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class MissingMapException : GameException
    {
        public MissingMapException() : base(ExceptionsLabels.MissingMapCode, ExceptionsLabels.MissingMapText) { }
    }
}
