using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using MediatR;

namespace LaneReady.Application.Features.Shipments.Commands;

public sealed record ApproveShipmentCommand(Guid ShipmentId, string? OverrideReason = null)
    : IRequest, IAuditableCommand
{
    public AuditAction AuditAction => AuditAction.Approve;
    public string EntityType => "Shipment";
    public Guid EntityId => ShipmentId;
    public string? BeforeJson => null;
    public string? AfterJson => null;
}

public sealed class ApproveShipmentCommandValidator : AbstractValidator<ApproveShipmentCommand>
{
    public ApproveShipmentCommandValidator() =>
        RuleFor(x => x.ShipmentId).NotEmpty();
}

public sealed class ApproveShipmentCommandHandler : IRequestHandler<ApproveShipmentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ApproveShipmentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(ApproveShipmentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole(UserRole.Operations))
            throw new ForbiddenException("Insufficient permissions to approve shipments.");

        var shipment = await _db.Shipments.FindAsync([request.ShipmentId], cancellationToken)
            ?? throw new NotFoundException("Shipment", request.ShipmentId);

        shipment.Approve(
            _currentUser.UserId ?? throw new ForbiddenException("User identity required."),
            request.OverrideReason);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
