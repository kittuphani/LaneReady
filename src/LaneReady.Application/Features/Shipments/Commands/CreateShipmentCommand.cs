using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Entities;
using LaneReady.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.Shipments.Commands;

public sealed record CreateShipmentCommand(
    DateOnly ShipmentDate,
    ConsigneeType ConsigneeType,
    List<ShipmentLineInput> Lines,
    string? CarrierName = null,
    string? SourceOrderId = null) : IRequest<CreateShipmentResult>;

public record ShipmentLineInput(Guid ProductId, int Quantity);

public record CreateShipmentResult(Guid ShipmentId, LaneDecision LaneDecision, string? LaneDecisionReason);

public sealed class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one shipment line is required");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}

public sealed class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, CreateShipmentResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IRulesEngineService _rulesEngine;

    public CreateShipmentCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IRulesEngineService rulesEngine)
    {
        _db = db;
        _currentUser = currentUser;
        _rulesEngine = rulesEngine;
    }

    public async Task<CreateShipmentResult> Handle(
        CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganisationId
            ?? throw new ForbiddenException("No organisation associated with this user.");

        // Load products and build lines
        var productIds = request.Lines.Select(l => l.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
            throw new NotFoundException("One or more products not found");

        // Get active rule set info (snapshot)
        var (ruleSetVersion, ruleSetId) = await _rulesEngine.GetActiveRuleSetInfoAsync(cancellationToken);

        var shipment = Shipment.Create(
            orgId, request.ShipmentDate, request.ConsigneeType,
            ruleSetVersion, ruleSetId, request.CarrierName, request.SourceOrderId);

        foreach (var lineInput in request.Lines)
        {
            var product = products.First(p => p.Id == lineInput.ProductId);
            var line = ShipmentLine.CreateFromProduct(orgId, shipment.Id, product, lineInput.Quantity);
            shipment.AddLine(line);
        }

        // Evaluate lane decision
        var totalValue = shipment.Lines.Sum(l => l.LineValue.Amount);
        var allHaveCodes = !shipment.HasIncompleteLines;

        var decision = await _rulesEngine.EvaluateLaneAsync(
            orgId, request.ConsigneeType, totalValue, allHaveCodes, cancellationToken);

        var reason = decision switch
        {
            LaneDecision.Green => "All UKIMS conditions met",
            LaneDecision.Orange => "Manual review required",
            LaneDecision.Red => "Does not qualify for green lane",
            _ => null
        };

        shipment.SetLaneDecision(decision, reason);

        _db.Shipments.Add(shipment);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateShipmentResult(shipment.Id, decision, reason);
    }
}
