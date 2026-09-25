using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LaneReady.Functions;

public class ReminderEmailFunction
{
    private readonly ILogger<ReminderEmailFunction> _logger;

    public ReminderEmailFunction(ILogger<ReminderEmailFunction> logger)
    {
        _logger = logger;
    }

    [Function("ReminderEmail")]
    public async Task Run(
        [TimerTrigger("0 0 7 * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reminder email function triggered at {Timestamp}", DateTime.UtcNow);
        await Task.CompletedTask;
    }
}
