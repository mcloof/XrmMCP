namespace XrmMcp.Core.Models;

public sealed record XrmRelationshipSummary(
    string SchemaName,
    string RelationshipType,
    string ReferencedEntity,
    string ReferencingEntity,
    string? ReferencingAttribute,
    bool IsCustomRelationship);
