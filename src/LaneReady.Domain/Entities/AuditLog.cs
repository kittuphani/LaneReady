using LaneReady.Domain.Common;
using LaneReady.Domain.Enums;

namespace LaneReady.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    public Guid OrganisationId { get; private set; }
    public string ActorUserId { get; private set; } = default!;
    public string ActorEmail { get; private set; } = default!;
    public AuditAction Action { get; private set; }
    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public string? BeforeJson { get; private set; }
    public string? AfterJson { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? IpAddress { get; private set; }
    public string? AdditionalInfo { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid organisationId,
        string actorUserId,
        string actorEmail,
        AuditAction action,
        string entityType,
        Guid entityId,
        string? beforeJson = null,
        string? afterJson = null,
        string? ipAddress = null,
        string? additionalInfo = null)
    {
        return new AuditLog
        {
            OrganisationId = organisationId,
            ActorUserId = actorUserId,
            ActorEmail = actorEmail,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            BeforeJson = beforeJson,
            AfterJson = afterJson,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress,
            AdditionalInfo = additionalInfo
        };
    }
}
