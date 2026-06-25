namespace XrmMcp.Core.Models;

public sealed record XrmEntitySummary(
    string LogicalName,
    string? DisplayName,
    string? PrimaryIdAttribute,
    string? PrimaryNameAttribute);
