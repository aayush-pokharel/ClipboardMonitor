using ClipboardIntercept.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClipboardIntercept.Common
{
    public sealed class ClipboardInterceptMonitor
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly IFileWriteConfigurationService _fileWriteConfigurationService;
        private readonly ILogger<ClipboardInterceptMonitor> _logger;
        private readonly CancellationToken _cancellationToken;

        public ClipboardInterceptMonitor(ITaskQueueProvider taskQueueProvider,
            ILogger<ClipboardInterceptMonitor> logger,
            IFileWriteConfigurationService fileWriteConfigurationService,
            IHostApplicationLifetime applicationLifetime)
        {
            _queue = taskQueueProvider.GetTaskQueue(typeof(ClipboardQueue));
            _logger= logger;
            _fileWriteConfigurationService= fileWriteConfigurationService;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        public void StartMonitor()
        {
            ClipboardMonitor.OnClipboardChange += ClipboardMonitor_OnClipboardChange;
            ClipboardMonitor.Start();
        }
        private async void ClipboardMonitor_OnClipboardChange(ClipboardFormat format, object data)
        {
            //If the clipboard format is text pass on the value to a singleton collection
            switch(format)
            {
                case ClipboardFormat.Text:
                case ClipboardFormat.Html:
                case ClipboardFormat.OemText:
                case ClipboardFormat.UnicodeText:
                    Console.WriteLine(data.ToString());
                    await _queue.QueueBackgroundWorkItemAsync(ct => BuildWorkItemAsync(data.ToString() ?? "", ct));
                    break;
                default:
                    break;
            }

        }
        private async ValueTask BuildWorkItemAsync(string clip, CancellationToken token)
        {
            //write the clip to a specified text file
            var writeConfig = _fileWriteConfigurationService.GetConfig();
            //check if config exists
            if (writeConfig == null)
                throw new Exception("File Write config not found");
            if (string.IsNullOrEmpty(writeConfig.FilePath))
                throw new ArgumentNullException($"{nameof(writeConfig.FilePath)} cannot be null");
            
            System.IO.FileInfo fi = new System.IO.FileInfo(writeConfig.FilePath);

            if (ReferenceEquals(fi, null))
                throw new ArgumentOutOfRangeException($"Invalid file name for {nameof(writeConfig.FilePath)}.");

            var fw = new FileWriteUtility();

            //write all new clips to to the top of a file
            await fw.InsertTextToTop(writeConfig.FilePath, clip);

            //trim the file to specified size to keep a rolling log of clips
            await fw.TrimFileBottom(writeConfig.FilePath, writeConfig.FileSizeBytes);
        }


    }
}
