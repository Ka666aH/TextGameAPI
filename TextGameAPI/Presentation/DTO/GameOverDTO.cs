using TextGame.Application.DTO;

namespace TextGame.Presentation.DTO
{
    public record GameOverDTO(string Message, GameInfoDTO GameInfo);
}