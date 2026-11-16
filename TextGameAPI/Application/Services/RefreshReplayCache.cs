using System.Collections.Concurrent;
using TextGame.Application.DTO;

namespace TextGame.Application.Services
{
    /// <summary>
    /// notes/AGENTS.md §2.1: rotation deletes the old refresh token, so when a double submit
    /// races past the middleware, the losing request finds no row and is logged out. The winner
    /// parks its result here and the loser replays it instead of failing.
    ///
    /// Single process only: a replay that lands on another instance still sees the exception.
    /// The window is short and the fingerprint is re-verified, so a stolen token cannot be
    /// replayed from a different browser.
    /// </summary>
    public class RefreshReplayCache
    {
        public static readonly TimeSpan Grace = TimeSpan.FromSeconds(30);

        private readonly ConcurrentDictionary<string, Entry> _entries = [];

        public void Store(string refreshToken, string hashedFingerprint, AuthResult result)
        {
            _entries[refreshToken] = new Entry(hashedFingerprint, result, DateTime.UtcNow.Add(Grace));
            PurgeExpired();
        }

        public bool TryReplay(string refreshToken, string hashedFingerprint, out AuthResult? result)
        {
            result = null;
            if (!_entries.TryGetValue(refreshToken, out Entry? entry)) return false;
            if (entry.ExpiresAt <= DateTime.UtcNow)
            {
                _entries.TryRemove(refreshToken, out _);
                return false;
            }
            if (entry.HashedFingerprint != hashedFingerprint) return false;

            result = entry.Result;
            return true;
        }

        private void PurgeExpired()
        {
            DateTime now = DateTime.UtcNow;
            foreach (var pair in _entries)
            {
                if (pair.Value.ExpiresAt <= now) _entries.TryRemove(pair.Key, out _);
            }
        }

        private sealed record Entry(string HashedFingerprint, AuthResult Result, DateTime ExpiresAt);
    }
}
