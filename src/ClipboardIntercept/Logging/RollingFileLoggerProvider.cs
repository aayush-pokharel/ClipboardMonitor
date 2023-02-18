using ClipboardIntercept.Common.Interfaces;
using ClipboardIntercept.Common.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

using System.Runtime.Versioning;


namespace ClipboardIntercept.Logging
{
    [UnsupportedOSPlatform("browser")]
    [ProviderAlias("RollingFileLogger")]
    public class RollingFileLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable? _onChangeToken;
        private RollingFileLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, RollingFileLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly ITaskQueueProvider _taskQueueProvider;

        public RollingFileLoggerProvider(
            IOptionsMonitor<RollingFileLoggerConfiguration> config,
            ITaskQueueProvider taskQueueProvider)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
            _taskQueueProvider = taskQueueProvider;
        }
        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new RollingFileLogger(name, GetCurrentConfig, GetTaskQueueProvider));

        private RollingFileLoggerConfiguration GetCurrentConfig() => _currentConfig;
        private ITaskQueueProvider GetTaskQueueProvider() => _taskQueueProvider;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken?.Dispose();
        }
    }
}
