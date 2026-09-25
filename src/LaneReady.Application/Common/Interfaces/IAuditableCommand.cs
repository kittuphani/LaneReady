using LaneReady.Domain.Enums;

namespace LaneReady.Application.Common.Interfaces;

public interface IAuditableCommand
{
    AuditAction AuditAction { get; }
    string EntityType { get; }
    Guid EntityId { get; }
    string? BeforeJson { get; }
    string? AfterJson { get; }
}
