using LaneReady.Application.Common.Interfaces;
using LaneReady.Application.Common.QueueMessages;
using LaneReady.Application.Features.Products.Commands;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LaneReady.Functions;

public class ProductImportFunction
{
    private readonly IMediator _mediator;
    private readonly IFunctionCurrentUserService _functionUser;
    private readonly ILogger<ProductImportFunction> _logger;

    public ProductImportFunction(
        IMediator mediator,
        IFunctionCurrentUserService functionUser,
        ILogger<ProductImportFunction> logger)
    {
        _mediator = mediator;
        _functionUser = functionUser;
        _logger = logger;
    }

    [Function("ProductImport")]
    public async Task Run(
        [QueueTrigger("product-imports", Connection = "AzureWebJobsStorage")] ProductImportMessage message,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing product import {ImportJobId} for org {OrgId}",
            message.ImportJobId, message.OrganisationId);

        _functionUser.SetOrganisationId(message.OrganisationId);

        await _mediator.Send(
            new ImportProductsCommand(message.BlobPath, message.ImportJobId),
            cancellationToken);
    }
}
