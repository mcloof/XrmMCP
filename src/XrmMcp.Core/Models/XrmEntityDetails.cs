namespace XrmMcp.Core.Models;

public sealed record XrmEntityDetails(
    string LogicalName,
    string? DisplayName,
    string? Description,
    string? PrimaryIdAttribute,
    string? PrimaryNameAttribute,
    bool IsCustomEntity,
    IReadOnlyCollection<XrmAttributeSummary> Attributes,
    IReadOnlyCollection<XrmRelationshipSummary> Relationships);
