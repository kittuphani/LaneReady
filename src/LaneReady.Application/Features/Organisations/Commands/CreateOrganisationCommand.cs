using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Entities;
using LaneReady.Domain.Enums;
using LaneReady.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.Organisations.Commands;

public sealed record CreateOrganisationCommand(
    string Name,
    string GbEoriNumber,
    string? XiEoriNumber,
    string? VatNumber,
    string? UkimsNumber) : IRequest<Guid>;

public sealed class CreateOrganisationCommandValidator
    : AbstractValidator<CreateOrganisationCommand>
{
    public CreateOrganisationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.GbEoriNumber)
            .NotEmpty()
            .Matches(@"^GB\d{12}$")
            .WithMessage("GB EORI must be 'GB' followed by 12 digits.");

        RuleFor(x => x.XiEoriNumber)
            .Matches(@"^XI\d{12}$")
            .When(x => !string.IsNullOrWhiteSpace(x.XiEoriNumber))
            .WithMessage("XI EORI must be 'XI' followed by 12 digits.");

        RuleFor(x => x.VatNumber)
            .Matches(@"^(GB)?(\d{9}|\d{12})$")
            .When(x => !string.IsNullOrWhiteSpace(x.VatNumber))
            .WithMessage("VAT number must be 9 or 12 digits, optionally prefixed with GB.");
    }
}

public sealed class CreateOrganisationCommandHandler
    : IRequestHandler<CreateOrganisationCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateOrganisationCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreateOrganisationCommand request,
        CancellationToken cancellationToken)
    {
        var gbEori = new EoriNumber(request.GbEoriNumber);
        EoriNumber? xiEori = request.XiEoriNumber is not null ? new EoriNumber(request.XiEoriNumber) : null;
        VatNumber? vat = request.VatNumber is not null ? new VatNumber(request.VatNumber) : null;

        var org = Organisation.Create(request.Name, gbEori, xiEori, vat);

        if (request.UkimsNumber is not null)
            org.RecordUkims(request.UkimsNumber, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));

        _db.Organisations.Add(org);

        var laneReadyUserId = _currentUser.LaneReadyUserId
            ?? throw new ForbiddenException("User is not authenticated.");

        var user = await _db.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(u => u.Id == laneReadyUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), laneReadyUserId);

        user.LinkToOrganisation(org.Id, UserRole.Owner);

        await _db.SaveChangesAsync(cancellationToken);
        return org.Id;
    }
}
