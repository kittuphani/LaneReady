namespace LaneReady.Application.Common.QueueMessages;

public record AiProcessingMessage(
    Guid ProductId,
    Guid OrganisationId,
    int Priority = 0);

public record ProductImportMessage(
    Guid OrganisationId,
    string BlobPath,
    Guid ImportJobId,
    string ImportFormat);

public record ReminderMessage(
    Guid OrganisationId,
    string ReminderType);
