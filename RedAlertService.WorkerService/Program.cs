using RedAlertService.WorkerService;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "RedAlertService";
});

builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
host.Run();




//using RedAlertService.WorkerService;

//IHost host = Host.CreateDefaultBuilder(args)
//    .ConfigureServices((hostContext, services) =>
//    {
//        IConfiguration configuration = hostContext.Configuration;
//        services.AddSingleton(configuration);
//        services.AddHostedService<Worker>();
//    })
//    .Build();

//await host.RunAsync();
