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

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IRulesEngineService, RulesEngineService>();

        var azureConfigured = !string.IsNullOrEmpty(configuration["Azure:StorageAccountUri"]);

        if (azureConfigured)
        {
            // Production: Azure clients using DefaultAzureCredential — no secrets in code
            var credential = new DefaultAzureCredential();

            services.AddSingleton(new BlobServiceClient(
                new Uri(configuration["Azure:StorageAccountUri"]!), credential));
            services.AddSingleton(new QueueServiceClient(
                new Uri(configuration["Azure:QueueServiceUri"]!), credential));
            services.AddSingleton(new AzureOpenAIClient(
                new Uri(configuration["Azure:OpenAI:Endpoint"]!), credential));
            services.AddSingleton(new EmailClient(
                new Uri(configuration["Azure:CommunicationServices:Endpoint"]!), credential));

            services.AddScoped<IBlobStorageService, BlobStorageService>();
            services.AddScoped<IAiService, AzureOpenAiService>();
            services.AddScoped<IEmailService, AzureCommunicationEmailService>();
            services.AddScoped<IQueueService, QueueService>();
            services.AddScoped<IStripeService, StripeService>();
        }
        else
        {
            // Local development: stub implementations (no Azure required)
            services.AddScoped<IBlobStorageService, StubBlobStorageService>();
            services.AddScoped<IAiService, StubAiService>();
            services.AddScoped<IEmailService, StubEmailService>();
            services.AddScoped<IQueueService, StubQueueService>();
            services.AddScoped<IStripeService, StubStripeService>();
        }

        return services;
    }
}
