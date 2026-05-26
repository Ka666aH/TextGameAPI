using TextGame.Application.DTO;

namespace TextGame.Domain.GameExceptions
{
    public class EndExeption : GameException
    {
        public GameInfoDTO GameInfo { get; }
        public EndExeption(string code, string message, GameInfoDTO gameInfo) : base(code, message)
        {
            GameInfo = gameInfo;
        }
    }
}