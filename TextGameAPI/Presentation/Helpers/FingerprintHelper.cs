namespace TextGame.Presentation.Helpers
{
    /// <summary>
    /// Fingerprint (см. notes/AGENTS.md, «Термины») — User-Agent + IP. Считается в одном месте:
    /// логин и middleware обновления токенов обязаны получать одинаковую строку, иначе после
    /// ротации refresh-токен не пройдёт проверку отпечатка и игрока выкинет из аккаунта.
    /// </summary>
    public static class FingerprintHelper
    {
        public static string GetFingerprint(this HttpContext context)
        {
            string deviceName = context.Request.Headers.UserAgent.ToString();
            string ip = context.Connection.RemoteIpAddress?.ToString() ?? "";
            return deviceName + " | " + ip;
        }
    }
}
