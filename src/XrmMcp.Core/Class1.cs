namespace XrmMcp.Core;

public enum XrmAuthType
{
    OAuth,
    ActiveDirectory
}

public sealed record XrmConnectionOptions(
    string Name,
    XrmAuthType AuthType,
    string ConnectionString);

public sealed record XrmConnectionResult(
    string Name,
    bool IsSuccess,
    bool IsReady,
    string? ConnectedOrganization,
    string? ErrorMessage);

public interface IXrmConnectionService
{
    Task<XrmConnectionResult> TestConnectionAsync(XrmConnectionOptions options, CancellationToken cancellationToken = default);
}
