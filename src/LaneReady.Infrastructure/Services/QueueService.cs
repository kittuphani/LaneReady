using System.Text;
using System.Text.Json;
using Azure.Storage.Queues;
using LaneReady.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace LaneReady.Infrastructure.Services;

public class QueueService : IQueueService
{
    private readonly QueueServiceClient _queueServiceClient;
    private readonly ILogger<QueueService> _logger;

    public QueueService(QueueServiceClient queueServiceClient, ILogger<QueueService> logger)
    {
        _queueServiceClient = queueServiceClient;
        _logger = logger;
    }

    public async Task EnqueueAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
        where T : class
    {
        var client = _queueServiceClient.GetQueueClient(queueName);
        await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(message);
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        await client.SendMessageAsync(encoded, cancellationToken: cancellationToken);
        _logger.LogDebug("Enqueued message of type {MessageType} to queue {QueueName}", typeof(T).Name, queueName);
    }
}
