using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class UndiscoveredRoomException : GameException
    {
        public UndiscoveredRoomException() : base(ExceptionsLabels.UndiscoveredRoomCode, ExceptionsLabels.UndiscoveredRoomText) { }
    }
}