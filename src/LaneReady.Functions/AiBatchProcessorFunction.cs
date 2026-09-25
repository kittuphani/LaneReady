using LaneReady.Application.Common.Interfaces;
using LaneReady.Application.Common.QueueMessages;
using LaneReady.Application.Features.Products.Commands;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LaneReady.Functions;

public class AiBatchProcessorFunction
{
    private readonly IMediator _mediator;
    private readonly IFunctionCurrentUserService _functionUser;
    private readonly ILogger<AiBatchProcessorFunction> _logger;

    public AiBatchProcessorFunction(
        IMediator mediator,
        IFunctionCurrentUserService functionUser,
        ILogger<AiBatchProcessorFunction> logger)
    {
        _mediator = mediator;
        _functionUser = functionUser;
        _logger = logger;
    }

    [Function("AiBatchProcessor")]
    public async Task Run(
        [QueueTrigger("ai-processing", Connection = "AzureWebJobsStorage")] AiProcessingMessage message,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing AI suggestion for product {ProductId}, org {OrgId}",
            message.ProductId, message.OrganisationId);

        _functionUser.SetOrganisationId(message.OrganisationId);

        await _mediator.Send(
            new GenerateAiSuggestionCommand(message.ProductId),
            cancellationToken);
    }
}
