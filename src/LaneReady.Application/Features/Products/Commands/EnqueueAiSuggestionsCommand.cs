using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Application.Common.QueueMessages;
using LaneReady.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.Products.Commands;

public sealed record EnqueueAiSuggestionsCommand(
    bool OnlyMissingCodes = true) : IRequest<int>;

public sealed class EnqueueAiSuggestionsCommandHandler
    : IRequestHandler<EnqueueAiSuggestionsCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IQueueService _queue;

    public EnqueueAiSuggestionsCommandHandler(
        IApplicationDbContext db, ICurrentUserService currentUser, IQueueService queue)
    {
        _db = db;
        _currentUser = currentUser;
        _queue = queue;
    }

    public async Task<int> Handle(EnqueueAiSuggestionsCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganisationId
            ?? throw new ForbiddenException("No organisation associated with this user.");

        var query = _db.Products.AsNoTracking().Where(p => p.OrganisationId == orgId);
        if (request.OnlyMissingCodes)
            query = query.Where(p => p.ConfirmedCommodityCode == null);

        var productIds = await query.Select(p => p.Id).ToListAsync(cancellationToken);

        foreach (var id in productIds)
        {
            await _queue.EnqueueAsync(
                "ai-processing",
                new AiProcessingMessage(id, orgId, 1),
                cancellationToken);
        }

        return productIds.Count;
    }
}
