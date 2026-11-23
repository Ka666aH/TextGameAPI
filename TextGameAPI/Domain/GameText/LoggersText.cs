namespace TextGame.Domain.GameText
{
    public static class LoggersText
    {
        public const string AutoSaveStarted = "Auto-save started";
        public const string AutoSaveSuccess = "Auto-saved sessions: {0}";
        public const string AutoSaveError = "Auto-save error";
        public const string AutoSaveImmediateError = "Immediate auto-save failed for session {SessionId}";

        // Единственная точка логирования HTTP-ошибок — ExceptionController: все исключения
        // конвейера приходят туда через UseExceptionHandler.
        public const string RequestFailed = "Unhandled request error: {Method} {Path} -> {StatusCode} {Code} (trace {TraceId})";
        public const string RequestRejected = "Request rejected: {Method} {Path} -> {StatusCode} {Code} (trace {TraceId})";
        public const string GameFlowCompleted = "Game event mapped to HTTP {StatusCode} {Code}: {Method} {Path} (trace {TraceId})";
    }
}
