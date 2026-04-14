namespace TextGame.Domain.GameExceptions
{
    public class UnsearchedRoomException : GameException
    {
        public UnsearchedRoomException() : base("ROOM_NOT_SEARCHED", "Комната ещё не обыскана!") { }
    }
}