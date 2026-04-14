using TextGame.Presentation.DTO;

namespace TextGame.Domain.GameExceptions
{
    public class WinException : EndExeption
    {
        public WinException(GameInfoDTO gameInfo) : base("WIN", "Вы нашли выход и выбрались наружу.", gameInfo) { }
    }
}