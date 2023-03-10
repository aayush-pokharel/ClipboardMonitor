using ClipboardIntercept.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClipboardIntercept.Common
{
    public class ClipboardQueueService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<ClipboardQueueService> _logger;
        private ClipboardInterceptMonitor _monitor;

        public ClipboardQueueService(
            ITaskQueueProvider taskQueueProvider,
            ClipboardInterceptMonitor monitor,
            ILogger<ClipboardQueueService> logger)
        {
            _taskQueue = taskQueueProvider.GetTaskQueue(typeof(ClipboardQueue));
            _monitor = monitor;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(ClipboardQueueService)} is running.{Environment.NewLine}");


            _monitor.StartMonitor();

            return ProcessTaskQueueAsync(stoppingToken);
        }

        private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Func<CancellationToken, ValueTask>? workItem =
                        await _taskQueue.DequeueAsync(stoppingToken);

                    await workItem(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing task work item.");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(ClipboardQueueService)} is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
