using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class RoomNotFoundException : GameException
    {
        public RoomNotFoundException() : base(ExceptionsLabels.RoomNotFoundCode, ExceptionsLabels.RoomNotFoundMessage) { }
    }
}
