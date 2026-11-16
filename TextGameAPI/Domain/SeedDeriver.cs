namespace TextGame.Domain
{
    /// <summary>
    /// Стабильное выведение int-сида из (seed, соль). Одинаковая пара всегда даёт
    /// одинаковое значение независимо от машины и запуска — на этом держится
    /// воспроизводимость генерации по сиду сессии.
    /// </summary>
    public static class SeedDeriver
    {
        public static int DeriveInt(long seed, int salt)
        {
            ulong x = (ulong)(seed ^ ((long)salt << 32 | (uint)salt));
            x ^= x >> 33;
            x *= 0xFF51AFD7ED558CCDUL;
            x ^= x >> 33;
            x *= 0xC4CEB9FE1A85EC53UL;
            x ^= x >> 33;
            return (int)(x >> 32) & 0x7FFFFFFF;
        }
    }
}
