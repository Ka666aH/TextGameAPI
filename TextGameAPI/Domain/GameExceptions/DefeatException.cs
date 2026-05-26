using TextGame.Application.DTO;
using TextGame.Domain.GameText;

namespace TextGame.Domain.GameExceptions
{
    public class DefeatException : EndExeption
    {
        public DefeatException(string message, GameInfoDTO gameInfo) : base(ExceptionsLabels.DefeatCode, message, gameInfo) { }
    }
}