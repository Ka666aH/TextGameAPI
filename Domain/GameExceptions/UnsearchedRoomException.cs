using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class UnsearchedRoomException : GameException
    {
        public UnsearchedRoomException() : base(ExceptionsLabels.UnsearchedRoomCode, ExceptionsLabels.UnsearchedRoomText) { }
    }
}