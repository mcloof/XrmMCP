namespace XrmMcp.Core.Models;

public sealed record XrmConnectionResult(
    string Name,
    bool IsSuccess,
    bool IsReady,
    string? ConnectedOrganization,
    string? ErrorMessage);
