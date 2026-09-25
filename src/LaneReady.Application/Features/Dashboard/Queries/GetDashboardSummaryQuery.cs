using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.Dashboard.Queries;

public sealed record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;

public record DashboardSummaryDto(
    int ReadinessScore,
    int TotalProducts,
    int CompleteProducts,
    int NeedsReviewProducts,
    int MissingDataProducts,
    bool HasValidUkims,
    string? UkimsStatus,
    int TotalShipments,
    int ShipmentsThisMonth,
    int EvidenceGaps,
    List<DashboardTask> OpenTasks);

public record DashboardTask(string Title, string Description, string ActionUrl, int Priority);

public sealed class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DashboardSummaryDto> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganisationId
            ?? throw new ForbiddenException("No organisation associated with this user.");

        var org = await _db.Organisations
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.Id == orgId, cancellationToken)
            ?? throw new NotFoundException("Organisation", orgId);

        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.OrganisationId == orgId)
            .Select(p => p.Status)
            .ToListAsync(cancellationToken);

        var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var shipmentsThisMonth = await _db.Shipments
            .AsNoTracking()
            .CountAsync(s => s.OrganisationId == orgId &&
                             s.CreatedAt >= thisMonth &&
                             s.Status != ShipmentStatus.Cancelled, cancellationToken);

        var totalShipments = await _db.Shipments
            .AsNoTracking()
            .CountAsync(s => s.OrganisationId == orgId &&
                             s.Status != ShipmentStatus.Cancelled, cancellationToken);

        var complete = products.Count(p => p == ProductStatus.Complete);
        var needsReview = products.Count(p => p == ProductStatus.NeedsReview);
        var missing = products.Count(p => p == ProductStatus.MissingData);
        var total = products.Count;

        var score = CalculateScore(org.HasValidUkims, complete, total, shipmentsThisMonth);

        var tasks = BuildOpenTasks(org.HasValidUkims, needsReview + missing, org.Plan);

        string ukimsStatus = org.HasValidUkims
            ? "Authorised"
            : org.UkimsNumber is not null ? "Awaiting HMRC" : "Not Applied";

        return new DashboardSummaryDto(
            ReadinessScore: score,
            TotalProducts: total,
            CompleteProducts: complete,
            NeedsReviewProducts: needsReview,
            MissingDataProducts: missing,
            HasValidUkims: org.HasValidUkims,
            UkimsStatus: ukimsStatus,
            TotalShipments: totalShipments,
            ShipmentsThisMonth: shipmentsThisMonth,
            EvidenceGaps: 0,
            OpenTasks: tasks);
    }

    private static int CalculateScore(bool hasUkims, int complete, int total, int shipmentsThisMonth)
    {
        if (total == 0 && !hasUkims) return 0;

        int score = 0;
        if (hasUkims) score += 40;
        if (total > 0)
            score += (int)(complete / (double)total * 40);
        if (shipmentsThisMonth > 0) score += 20;
        return Math.Min(100, score);
    }

    private static List<DashboardTask> BuildOpenTasks(bool hasUkims, int productsNeedingWork, PlanTier plan)
    {
        var tasks = new List<DashboardTask>();
        int priority = 0;

        if (!hasUkims)
            tasks.Add(new DashboardTask(
                "Apply for UKIMS",
                "Required for B2B green lane shipments",
                "/app/ukims",
                priority++));

        if (productsNeedingWork > 0)
            tasks.Add(new DashboardTask(
                $"Review {productsNeedingWork} product{(productsNeedingWork == 1 ? "" : "s")}",
                "Products need descriptions or commodity codes",
                "/app/products",
                priority++));

        return tasks;
    }
}
