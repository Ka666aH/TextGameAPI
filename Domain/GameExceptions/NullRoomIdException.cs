namespace TextGame.Domain.GameExceptions
{
    public class NullRoomIdException : GameException
    {
        public NullRoomIdException() : base("ROOM_NOT_FOUND", "Комната не найдена.") { }
    }
}