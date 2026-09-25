using Azure.AI.OpenAI;
using Azure.Communication.Email;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Infrastructure.Persistence;
using LaneReady.Infrastructure.Persistence.Interceptors;
using LaneReady.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LaneReady.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(maxRetryCount: 3);
                });

            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // Azure clients using DefaultAzureCredential — no secrets in code
        var credential = new DefaultAzureCredential();

        var storageUri = new Uri(configuration["Azure:StorageAccountUri"]
            ?? throw new InvalidOperationException("Azure:StorageAccountUri is not configured"));
        services.AddSingleton(new BlobServiceClient(storageUri, credential));

        var queueUri = new Uri(configuration["Azure:QueueServiceUri"]
            ?? throw new InvalidOperationException("Azure:QueueServiceUri is not configured"));
        services.AddSingleton(new QueueServiceClient(queueUri, credential));

        var openAiEndpoint = new Uri(configuration["Azure:OpenAI:Endpoint"]
            ?? throw new InvalidOperationException("Azure:OpenAI:Endpoint is not configured"));
        services.AddSingleton(new AzureOpenAIClient(openAiEndpoint, credential));

        var acsEndpoint = configuration["Azure:CommunicationServices:Endpoint"]
            ?? throw new InvalidOperationException("Azure:CommunicationServices:Endpoint is not configured");
        services.AddSingleton(new EmailClient(new Uri(acsEndpoint), credential));

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<IAiService, AzureOpenAiService>();
        services.AddScoped<IEmailService, AzureCommunicationEmailService>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IQueueService, QueueService>();
        services.AddScoped<IRulesEngineService, RulesEngineService>();

        return services;
    }
}
