namespace TextGame
{
    public record SuccessfulResponse(object? data);
    public record ErrorResponse(string? code, string? message)
    {
        public ErrorResponse(GameException ex) : this(ex.Code, ex.Message) { }
    }
}
