using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NullRoomIdException : GameException
    {
        public NullRoomIdException() : base(ExceptionLabels.NullRoomIdCode, ExceptionLabels.NullRoomIdText) { }
    }
}