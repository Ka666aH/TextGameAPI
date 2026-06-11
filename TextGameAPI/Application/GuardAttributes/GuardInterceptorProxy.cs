using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
using TextGame.Application.Interfaces.Services;

namespace TextGame.Application.GuardAttributes
{
    public class GuardInterceptorProxy : DispatchProxy
    {
        private object _inner = null!;
        private IStateService _stateService = null!;
        private static readonly ConcurrentDictionary<(Type, string), GuardAttribute[]> _guardCache = new();

        public void Initialize(object inner, IStateService stateService)
        {
            _inner = inner;
            _stateService = stateService;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var key = (_inner.GetType(), targetMethod!.Name);
            var guards = _guardCache.GetOrAdd(key, k =>
            {
                var paramTypes = targetMethod.GetParameters().Select(p => p.ParameterType).ToArray();
                var implMethod = k.Item1.GetMethod(k.Item2, paramTypes);
                return implMethod?.GetCustomAttributes<GuardAttribute>().ToArray() ?? [];
            });

            foreach (var guard in guards)
            {
                guard.Validate(_stateService);
            }

            try
            {
                return targetMethod.Invoke(_inner, args);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }

    public static class GuardProxyFactory
    {
        public static TInterface Create<TInterface>(TInterface inner, IStateService stateService) where TInterface : class
        {
            var proxy = DispatchProxy.Create<TInterface, GuardInterceptorProxy>();
            var interceptor = (GuardInterceptorProxy)(object)proxy!;
            interceptor.Initialize(inner, stateService);
            return proxy;
        }
    }
}
