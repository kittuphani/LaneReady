using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using MediatR;

namespace LaneReady.Application.Features.Users.Commands;

public sealed record InviteUserCommand(string Email, UserRole Role) : IRequest;

public sealed class InviteUserCommandValidator : AbstractValidator<InviteUserCommand>
{
    public InviteUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Role).IsInEnum()
            .Must(r => r != UserRole.Admin).WithMessage("Admin role cannot be assigned via invite");
    }
}

public sealed class InviteUserCommandHandler : IRequestHandler<InviteUserCommand>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _email;

    public InviteUserCommandHandler(ICurrentUserService currentUser, IEmailService email)
    {
        _currentUser = currentUser;
        _email = email;
    }

    public async Task Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole(UserRole.Owner))
            throw new ForbiddenException("Only owners can invite team members.");

        // TODO: persist invite token (Sprint 3 — US-204)
        var body = $"""
            <p>You have been invited to join a LaneReady organisation.</p>
            <p>Please sign in at <a href="https://app.laneready.co.uk">app.laneready.co.uk</a> to accept.</p>
            """;

        await _email.SendAsync(
            request.Email, request.Email,
            "You've been invited to LaneReady",
            body,
            cancellationToken);
    }
}
