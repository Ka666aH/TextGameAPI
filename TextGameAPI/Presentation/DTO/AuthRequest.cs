using System.ComponentModel.DataAnnotations;

namespace TextGame.Presentation.DTO
{
    public record AuthRequest(string Login, string Password);
}