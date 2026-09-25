using LaneReady.Application;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Functions.Services;
using LaneReady.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);

        // Override web CurrentUserService with function-scoped implementation
        services.AddSingleton<FunctionCurrentUserService>();
        services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<FunctionCurrentUserService>());
        services.AddSingleton<IFunctionCurrentUserService>(sp => sp.GetRequiredService<FunctionCurrentUserService>());
    })
    .Build();

await host.RunAsync();
