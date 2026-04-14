namespace TextGame.Domain.GameExceptions
{
    public class UndiscoveredRoomException : GameException
    {
        public UndiscoveredRoomException() : base("UNDISCOVERED_ROOM_ERROR", "Комната ещё не открыта.") { }
    }
}