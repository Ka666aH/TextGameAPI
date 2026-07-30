using TextGame.Application.DTO;
using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class WinException : EndExeption
    {
        public WinException(GameInfoDTO gameInfo) : base(ExceptionsLabels.WinCode, ExceptionsLabels.WinText, gameInfo) { }
    }
}