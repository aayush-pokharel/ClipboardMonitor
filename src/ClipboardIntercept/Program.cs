
using Accessibility;
using ClipboardIntercept.Logging;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
    })
    .ConfigureServices((context, services) =>
    {
        //services.AddHostedService<FileCleanWorker>();

        //logging services
        services.AddSingleton<ClipboardIntercept.Common.Interfaces.IBackgroundTaskQueue>(_ =>
        {
            var queueCapcity = 500;
            return new ClipboardIntercept.Common.Logging.LoggerQueue(queueCapcity);
        });
        services.AddHostedService<ClipboardIntercept.Common.Logging.LoggerQueueService>();


        //Clipboard monitor services
        services.Configure<ClipboardIntercept.Common.FileWriteConfiguration>(
            context.Configuration.GetRequiredSection("FileWriteConfiguration"));
        services.AddTransient<ClipboardIntercept.Common.Interfaces.IFileWriteConfigurationService, 
            ClipboardIntercept.Common.FileWriteConfigurationService>();
        services.AddSingleton<ClipboardIntercept.Common.Interfaces.IBackgroundTaskQueue>(_ =>
        {
            var queueCapcity = 100;
            return new ClipboardIntercept.Common.ClipboardQueue(queueCapcity);
        });
        services.AddSingleton<ClipboardIntercept.Common.ClipboardInterceptMonitor>();
        services.AddHostedService<ClipboardIntercept.Common.ClipboardQueueService>();

        //Task queue provider
        services.AddTransient<ClipboardIntercept.Common.Interfaces.ITaskQueueProvider,
            ClipboardIntercept.Common.TaskQueueProvider>();
        
    })
    .ConfigureLogging(builder =>
        builder.AddRollingFileLogger(configuration =>
        { 
            //Add logger configurations here
        })
    )
    .UseWindowsService(options =>
    {
        options.ServiceName = "Clipboard Monitor"; 
    })
    .Build();

await host.RunAsync();
