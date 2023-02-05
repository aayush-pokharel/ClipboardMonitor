using ClipboardIntercept;
using Microsoft.Extensions.DependencyInjection;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        //services.AddHostedService<FileCleanWorker>();
        services.Configure<ClipboardIntercept.Common.FileWriteConfiguration>(context.Configuration.GetRequiredSection("FileWriteConfiguration"));
        services.AddTransient<ClipboardIntercept.Common.Interfaces.IFileWriteConfigurationService, ClipboardIntercept.Common.FileWriteConfigurationService>();
        services.AddSingleton<ClipboardIntercept.Common.Interfaces.IBackgroundTaskQueue>(_ =>
        {
            var queueCapcity = 100;
            return new ClipboardIntercept.Common.ClipboardQueue(queueCapcity);
        });
        services.AddSingleton<ClipboardIntercept.Common.ClipboardInterceptMonitor>();
        services.AddHostedService<ClipboardIntercept.Common.ClipboardQueueService>();
    })
    .UseWindowsService(options =>
    {
        options.ServiceName = "Clipboard Monitor"; 
    })
    .Build();

    var monitor = host.Services.GetService<ClipboardIntercept.Common.ClipboardInterceptMonitor>()!;
    monitor.StartMonitor();

await host.RunAsync();
