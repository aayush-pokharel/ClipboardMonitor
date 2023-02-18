using ClipboardIntercept.Common;
using ClipboardIntercept.Common.Interfaces;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClipboardIntercept.Logging
{
    public sealed class RollingFileLogger : ILogger
    {
        private readonly string _name;
        private readonly Func<Common.Logging.RollingFileLoggerConfiguration> _getCurrentConfig;
        private readonly IBackgroundTaskQueue _loggerQueue;
        public RollingFileLogger(
            string name,
            Func<Common.Logging.RollingFileLoggerConfiguration> getCurrentConfig,
            Func<ITaskQueueProvider> taskQueueProvider) =>
        (_name, _getCurrentConfig, _loggerQueue) = (name, getCurrentConfig, taskQueueProvider().GetTaskQueue(typeof(ClipboardIntercept.Common.Logging.LoggerQueue)));
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel.ToString().Equals(_getCurrentConfig().LogLevel ?? "");
        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Don't log the entry if it's not enabled.
            if (!IsEnabled(logLevel))
                return;
            if (!_getCurrentConfig().LogFields?.Any() ?? false)
                return;

            string path = _getCurrentConfig().FileWriteConfiguration?.FilePath ?? "";

            //path validity check
            var pathDir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(pathDir))
                throw new Exception($"{pathDir} does not exist");

            //Create a json output for every log
            var threadId = Thread.CurrentThread.ManagedThreadId;


            var logRecord = new Common.Logging.LogRecord();

            foreach (var logField in _getCurrentConfig().LogFields)
            {
                switch (logField)
                {
                    case "LogLevel":
                        if (!string.IsNullOrWhiteSpace(logLevel.ToString()))
                        {
                            logRecord.LogLevel = logLevel.ToString();
                        }
                        break;
                    case "ThreadId":
                        logRecord.ThreadId = threadId;
                        break;
                    case "EventId":
                        logRecord.EventId = eventId.Id;
                        break;
                    case "EventName":
                        if (!string.IsNullOrWhiteSpace(eventId.Name))
                        {
                            logRecord.EventName = eventId.Name;
                        }
                        break;
                    case "Message":
                        if (!string.IsNullOrWhiteSpace(formatter(state, exception)))
                        {
                            logRecord.Message = formatter(state, exception);
                        }
                        break;
                    case "ExceptionMessage":
                        if (exception != null && !string.IsNullOrWhiteSpace(exception.Message))
                        {
                            logRecord.ExceptionMessage = exception?.Message;
                        }
                        break;
                    case "ExceptionStackTrace":
                        if (exception != null && !string.IsNullOrWhiteSpace(exception.StackTrace))
                        {
                            logRecord.ExceptionStackTrace = exception?.StackTrace;
                        }
                        break;
                    case "ExceptionSource":
                        if (exception != null && !string.IsNullOrWhiteSpace(exception.Source))
                        {
                            logRecord.ExceptionSource = exception?.Source;
                        }
                        break;
                }
            }

            //add log record to queue
            await _loggerQueue.QueueBackgroundWorkItemAsync(wi => WriteLogToFile(JsonSerializer.Serialize(logRecord) ?? "", wi));



        }
        private async ValueTask WriteLogToFile(string json, CancellationToken token)
        {
            //write the clip to a specified text file
            var writeConfig = _getCurrentConfig().FileWriteConfiguration;
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
            await fw.AppendTextToFile(writeConfig.FilePath, json);

            //trim the file to specified size to keep a rolling log of clips
            await fw.TrimFileTop(writeConfig.FilePath, writeConfig.FileSizeBytes);
        }
    }
}
