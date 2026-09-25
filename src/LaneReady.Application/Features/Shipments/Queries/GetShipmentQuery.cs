using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.Shipments.Queries;

public sealed record GetShipmentQuery(Guid ShipmentId) : IRequest<ShipmentDetailDto>;

public record ShipmentLineDetailDto(
    Guid Id,
    string ProductSku,
    string ProductDescription,
    string? CommodityCode,
    int Quantity,
    decimal LineValueAmount,
    string LineValueCurrency);

public record EvidenceDocumentDto(
    Guid Id,
    string FileName,
    EvidenceDocumentType DocumentType,
    DateTime CreatedAt);

public record ShipmentDetailDto(
    Guid Id,
    DateOnly ShipmentDate,
    ConsigneeType ConsigneeType,
    ShipmentStatus Status,
    LaneDecision LaneDecision,
    string? CarrierName,
    string? SourceOrderId,
    string RuleSetVersion,
    string? ApprovalOverrideReason,
    DateTime CreatedAt,
    List<ShipmentLineDetailDto> Lines,
    List<EvidenceDocumentDto> Evidence);

public sealed class GetShipmentQueryHandler : IRequestHandler<GetShipmentQuery, ShipmentDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetShipmentQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ShipmentDetailDto> Handle(
        GetShipmentQuery request, CancellationToken cancellationToken)
    {
        var s = await _db.Shipments
            .AsNoTracking()
            .Include(s => s.Lines)
            .Include(s => s.Evidence)
            .SingleOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken)
            ?? throw new NotFoundException("Shipment", request.ShipmentId);

        return new ShipmentDetailDto(
            s.Id, s.ShipmentDate, s.ConsigneeType, s.Status, s.LaneDecision,
            s.CarrierName, s.SourceOrderId, s.RuleSetVersion, s.ApprovalOverrideReason,
            s.CreatedAt,
            s.Lines.Select(l => new ShipmentLineDetailDto(
                l.Id, l.ProductSku, l.ProductDescription, l.CommodityCode,
                l.Quantity, l.LineValue.Amount, l.LineValue.Currency)).ToList(),
            s.Evidence.Select(e => new EvidenceDocumentDto(
                e.Id, e.FileName, e.DocumentType, e.CreatedAt)).ToList());
    }
}
