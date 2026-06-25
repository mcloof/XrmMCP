namespace XrmMcp.Core.Models;

public sealed record XrmAttributeSummary(
    string LogicalName,
    string? DisplayName,
    string AttributeType,
    bool IsCustomAttribute,
    bool IsPrimaryId,
    bool IsPrimaryName,
    bool IsValidForCreate,
    bool IsValidForUpdate,
    bool IsValidForRead);
