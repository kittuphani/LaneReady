using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Entities;
using LaneReady.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.Products.Commands;

public sealed record UpsertProductCommand(
    string Sku,
    string Title,
    string Description,
    decimal GrossWeightKg,
    decimal NetWeightKg,
    decimal UnitValueAmount,
    string CountryOfOrigin,
    string? CommodityCode = null) : IRequest<Guid>;

public sealed class UpsertProductCommandValidator : AbstractValidator<UpsertProductCommand>
{
    public UpsertProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.GrossWeightKg).GreaterThan(0);
        RuleFor(x => x.UnitValueAmount).GreaterThan(0);
        RuleFor(x => x.CountryOfOrigin).NotEmpty().Length(2, 3);
        RuleFor(x => x.CommodityCode).Matches(@"^\d{8,10}$")
            .When(x => !string.IsNullOrEmpty(x.CommodityCode))
            .WithMessage("Commodity code must be 8 or 10 digits");
    }
}

public sealed class UpsertProductCommandHandler : IRequestHandler<UpsertProductCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpsertProductCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(UpsertProductCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganisationId
            ?? throw new ForbiddenException("No organisation associated with this user.");

        var existing = await _db.Products
            .FirstOrDefaultAsync(p => p.Sku == request.Sku, cancellationToken);

        if (existing is null)
        {
            var product = Product.Create(
                orgId, request.Sku, request.Title, request.Description,
                request.GrossWeightKg, request.NetWeightKg,
                new Money(request.UnitValueAmount),
                request.CountryOfOrigin);

            if (!string.IsNullOrEmpty(request.CommodityCode))
                product.OverrideCommodityCode(
                    new CommodityCode(request.CommodityCode),
                    _currentUser.UserId ?? "system");

            _db.Products.Add(product);
            await _db.SaveChangesAsync(cancellationToken);
            return product.Id;
        }

        existing.UpdateDetails(
            request.Title, request.Description,
            request.GrossWeightKg, request.NetWeightKg,
            new Money(request.UnitValueAmount),
            request.CountryOfOrigin);

        if (!string.IsNullOrEmpty(request.CommodityCode))
            existing.OverrideCommodityCode(
                new CommodityCode(request.CommodityCode),
                _currentUser.UserId ?? "system");

        await _db.SaveChangesAsync(cancellationToken);
        return existing.Id;
    }
}
