using LaneReady.Application.Common.Interfaces;
using LaneReady.Application.Common.Models;
using LaneReady.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.Shipments.Queries;

public sealed record GetShipmentsQuery(
    ShipmentStatus? StatusFilter = null,
    int Page = 1,
    int PageSize = 25) : IRequest<PagedResult<ShipmentSummaryDto>>;

public record ShipmentSummaryDto(
    Guid Id,
    DateOnly ShipmentDate,
    ConsigneeType ConsigneeType,
    ShipmentStatus Status,
    LaneDecision LaneDecision,
    string? CarrierName,
    int LineCount,
    string RuleSetVersion,
    DateTime CreatedAt);

public sealed class GetShipmentsQueryHandler
    : IRequestHandler<GetShipmentsQuery, PagedResult<ShipmentSummaryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetShipmentsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<ShipmentSummaryDto>> Handle(
        GetShipmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Shipments.AsNoTracking();

        if (request.StatusFilter.HasValue)
            query = query.Where(s => s.Status == request.StatusFilter.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.ShipmentDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new ShipmentSummaryDto(
                s.Id, s.ShipmentDate, s.ConsigneeType, s.Status, s.LaneDecision,
                s.CarrierName, s.Lines.Count, s.RuleSetVersion, s.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ShipmentSummaryDto>(items, total, request.Page, request.PageSize);
    }
}
