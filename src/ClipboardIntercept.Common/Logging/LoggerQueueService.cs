using ClipboardIntercept.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace ClipboardIntercept.Common.Logging
{
    public class LoggerQueueService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<LoggerQueueService> _logger;
        public LoggerQueueService(
            ITaskQueueProvider taskQueueProvider,
            ILogger<LoggerQueueService> logger,
            IConfiguration configuration)
        {
            _taskQueue = taskQueueProvider.GetTaskQueue(typeof(LoggerQueue));
            _logger = logger;
            _logger.LogInformation($"FilePath for file write: {configuration.GetSection("FileWriteConfiguration:FilePath").Value}");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(LoggerQueueService)} is running.{Environment.NewLine}");

            return ProcessTaskQueueAsync(stoppingToken);
        }
        private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //locking here because the file is a shared resurce
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
                finally { 
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(LoggerQueueService)} is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
