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

        public ClipboardInterceptMonitor(IBackgroundTaskQueue queue,
            ILogger<ClipboardInterceptMonitor> logger,
            IFileWriteConfigurationService fileWriteConfigurationService,
            IHostApplicationLifetime applicationLifetime)
        {
            _queue = queue;
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

            //write all new clips to to the top of a file
            await InsertTextToTop(writeConfig.FilePath, clip);

            //trim the file to specified size to keep a rolling log of clips
            await TrimFile(writeConfig.FilePath, writeConfig.FileSizeBytes);
        }

        private async Task InsertTextToTop(string path, string newText)
        {
            //if there is no existing file write text directly to new file
            if (!File.Exists(path))
            {
                await File.WriteAllTextAsync(path, newText);
                return;
            }
            //path validity check
            var pathDir = Path.GetDirectoryName(path);
            if(String.IsNullOrEmpty(pathDir))
                throw new Exception ($"{pathDir} does not exist");


            var tempPath = Path.Combine(pathDir, Guid.NewGuid().ToString("N"));
            //write new text to a temp file followed by end line terminator
            {
                using var stream = new FileStream(tempPath, FileMode.Create,
                    FileAccess.Write, FileShare.None, 4 * 1024 * 1024);
                using var sw = new StreamWriter(stream);
                await sw.WriteLineAsync(newText);
                sw.Flush();

                //copy the contents of the current clip file to the new temp file
                using var old = File.OpenRead(path);
                await old.CopyToAsync(sw.BaseStream);
            }

            //replace old file with the new temp one created
            File.Delete(path);
            File.Move(tempPath, path);
        }
        private async Task TrimFile(string filename, long bytes)
        {
            var fileSize = (new System.IO.FileInfo(filename)).Length;

            if (fileSize > bytes)
            {
                var text = await File.ReadAllTextAsync(filename);

                var amountToKeep = (int)(text.Length * 0.9);
                amountToKeep = text.IndexOf('\n', amountToKeep);
                var trimmedText = text.Substring(0, amountToKeep + 1);

                await File.WriteAllTextAsync(filename, trimmedText);
            }
        }

    }
}
