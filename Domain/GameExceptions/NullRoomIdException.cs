using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class NullRoomIdException : GameException
    {
        public NullRoomIdException() : base(ExceptionsLabels.NullRoomIdCode, ExceptionsLabels.NullRoomIdText) { }
    }
}