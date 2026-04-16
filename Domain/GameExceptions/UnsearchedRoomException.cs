using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class UnsearchedRoomException : GameException
    {
        public UnsearchedRoomException() : base(ExceptionLabels.UnsearchedRoomCode, ExceptionLabels.UnsearchedRoomText) { }
    }
}