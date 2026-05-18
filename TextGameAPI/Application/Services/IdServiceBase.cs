using TextGame.Application.Interfaces.Services;

namespace TextGame.Application.Services
{
    public abstract class IdServiceBase : IIdService
    {
        private int _current = 0;
        public int Current() => _current;
        public int Next() => ++_current;
    }
}