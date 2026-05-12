using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.GameText;

namespace TextGame.Application.Services
{
    public class TokenCleaningService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public TokenCleaningService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(_interval);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var tokenRepo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                    int deleted = await tokenRepo.DeleteExpiredAsync(stoppingToken);
                    Console.WriteLine(string.Format(LoggersText.CleanupSuccess, deleted));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format(LoggersText.CleanupError, ex.Message));
                }
            }
        }
    }
}