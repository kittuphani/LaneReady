namespace LaneReady.Application.Common.Interfaces;

public interface IQueueService
{
    Task EnqueueAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
        where T : class;
}
