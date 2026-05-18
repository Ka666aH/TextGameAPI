using TextGame.Domain.GameText;
using TextGame.Presentation.DTO;

namespace TextGame.Domain.GameExceptions
{
    public class WinException : EndExeption
    {
        public WinException(GameInfoDTO gameInfo) : base(ExceptionsLabels.WinCode, ExceptionsLabels.WinText, gameInfo) { }
    }
}