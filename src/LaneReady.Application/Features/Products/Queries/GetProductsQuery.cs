using LaneReady.Application.Common.Interfaces;
using LaneReady.Application.Common.Models;
using LaneReady.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.Products.Queries;

public sealed record GetProductsQuery(
    ProductStatus? StatusFilter = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedResult<ProductSummaryDto>>;

public record ProductSummaryDto(
    Guid Id,
    string Sku,
    string Title,
    string Description,
    ProductStatus Status,
    string? CommodityCode,
    string CountryOfOrigin,
    decimal UnitValueAmount,
    string UnitValueCurrency,
    bool HasAiSuggestion,
    bool AiConfirmed);

public sealed class GetProductsQueryHandler
    : IRequestHandler<GetProductsQuery, PagedResult<ProductSummaryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetProductsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<ProductSummaryDto>> Handle(
        GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Products.AsNoTracking();

        if (request.StatusFilter.HasValue)
            query = query.Where(p => p.Status == request.StatusFilter.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p =>
                p.Sku.Contains(request.Search) ||
                p.Title.Contains(request.Search) ||
                p.Description.Contains(request.Search));

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Status)
            .ThenBy(p => p.Sku)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductSummaryDto(
                p.Id, p.Sku, p.Title, p.Description, p.Status,
                p.ConfirmedCommodityCode != null ? p.ConfirmedCommodityCode.Value : null,
                p.CountryOfOrigin, p.UnitValue.Amount, p.UnitValue.Currency,
                p.AiSuggestion != null, p.AiSuggestionConfirmed))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductSummaryDto>(items, total, request.Page, request.PageSize);
    }
}
