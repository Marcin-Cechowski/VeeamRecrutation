using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace FolderSync
{
    public class SyncWorker : BackgroundService
    {
        private readonly ISyncService _syncService;
        private readonly CliOptions _options;
        private readonly ILogger<SyncWorker> _logger;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

        public SyncWorker(ISyncService syncService, CliOptions options, ILogger<SyncWorker> logger)
        {
            _syncService = syncService;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SyncWorker started. Interval: {Interval}", _options.Interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunOnceAsync(stoppingToken);

                try
                {
                    await Task.Delay(_options.Interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("SyncWorker stopped.");
        }

        private async Task RunOnceAsync(CancellationToken token)
        {
            if (!await _gate.WaitAsync(0, token))
            {
                _logger.LogWarning("Previous synchronization still running. Skipping tick.");
                return;
            }

            try
            {
                await _syncService.SynchronizeAsync(token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Synchronization failed.");
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
