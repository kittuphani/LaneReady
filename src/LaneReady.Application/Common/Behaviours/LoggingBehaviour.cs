using LaneReady.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaneReady.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehaviour(
        ILogger<LoggingBehaviour<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "LaneReady request: {RequestType} | User: {UserId} | Org: {OrgId}",
                typeof(TRequest).Name,
                _currentUserService.UserId ?? "anonymous",
                _currentUserService.OrganisationId?.ToString() ?? "none");
        }

        return await next(cancellationToken);
    }
}
