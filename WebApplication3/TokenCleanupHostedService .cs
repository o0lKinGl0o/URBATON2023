namespace WebApplication3
{
    public class TokenCleanupHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public TokenCleanupHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Запускаем таймер каждый час
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            // Используем IServiceScopeFactory, чтобы создать новый экземпляр области видимости и получить экземпляр вашего сервиса
            using (var scope = _serviceProvider.CreateScope())
            {
                var tokenCleanupService = scope.ServiceProvider.GetRequiredService<TokenCleanupService>();
                tokenCleanupService.RemoveExpiredTokens();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
