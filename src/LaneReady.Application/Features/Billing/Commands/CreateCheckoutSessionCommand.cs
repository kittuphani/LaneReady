using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using MediatR;

namespace LaneReady.Application.Features.Billing.Commands;

public sealed record CreateCheckoutSessionCommand(
    PlanTier Plan,
    string SuccessUrl,
    string CancelUrl) : IRequest<string>;

public sealed class CreateCheckoutSessionCommandValidator
    : AbstractValidator<CreateCheckoutSessionCommand>
{
    public CreateCheckoutSessionCommandValidator()
    {
        RuleFor(x => x.Plan).IsInEnum()
            .Must(p => p != PlanTier.Trial).WithMessage("Cannot check out for Trial plan");
        RuleFor(x => x.SuccessUrl).NotEmpty();
        RuleFor(x => x.CancelUrl).NotEmpty();
    }
}

public sealed class CreateCheckoutSessionCommandHandler
    : IRequestHandler<CreateCheckoutSessionCommand, string>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IStripeService _stripe;

    public CreateCheckoutSessionCommandHandler(ICurrentUserService currentUser, IStripeService stripe)
    {
        _currentUser = currentUser;
        _stripe = stripe;
    }

    public async Task<string> Handle(
        CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganisationId
            ?? throw new ForbiddenException("No organisation associated with this user.");

        if (!_currentUser.IsInRole(UserRole.Owner))
            throw new ForbiddenException("Only owners can manage billing.");

        return await _stripe.CreateCheckoutSessionAsync(
            orgId.ToString(), request.Plan,
            request.SuccessUrl, request.CancelUrl,
            cancellationToken);
    }
}
