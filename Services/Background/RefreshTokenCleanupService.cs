using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using KzBarry.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace KzBarry.Services.Background
{
    public class RefreshTokenCleanupOptions
    {
        public bool Enabled { get; set; } = true;
        public int IntervalMinutes { get; set; } = 60;
    }

    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RefreshTokenCleanupService> _logger;
        private readonly RefreshTokenCleanupOptions _options;

        public RefreshTokenCleanupService(IServiceProvider serviceProvider, IOptions<RefreshTokenCleanupOptions> options, ILogger<RefreshTokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("RefreshTokenCleanupService is disabled by config.");
                return;
            }
            _logger.LogInformation("RefreshTokenCleanupService started with interval {IntervalMinutes} minutes.", _options.IntervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var repo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                        int deleted = await repo.DeleteExpiredAsync();
                        _logger.LogInformation("Deleted {Count} expired refresh tokens.", deleted);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during refresh token cleanup.");
                }
                await Task.Delay(TimeSpan.FromMinutes(_options.IntervalMinutes), stoppingToken);
            }
        }
    }
}
