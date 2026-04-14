using TextGame.Domain.GameExceptions;

namespace TextGame.Presentation.ResponseTemplates
{
    public record ErrorResponse(string? code, string? message)
    {
        public ErrorResponse(GameException ex) : this(ex.Code, ex.Message) { }        
    }
}
