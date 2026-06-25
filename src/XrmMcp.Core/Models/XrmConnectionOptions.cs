namespace XrmMcp.Core.Models;

public sealed record XrmConnectionOptions(
    string Name,
    XrmAuthType AuthType,
    string ConnectionString);
