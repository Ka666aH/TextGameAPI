using TextGame.Domain.GameText;
using TextGame.Presentation.DTO;

namespace TextGame.Domain.GameExceptions
{
    public class DefeatException : EndExeption
    {
        public DefeatException(string message, GameInfoDTO gameInfo) : base(ExceptionLabels.DefeatCode, message, gameInfo) { }
    }
}