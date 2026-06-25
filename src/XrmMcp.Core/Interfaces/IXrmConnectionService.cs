using XrmMcp.Core.Models;

namespace XrmMcp.Core.Interfaces;

public interface IXrmConnectionService
{
    Task<XrmConnectionResult> TestConnectionAsync(XrmConnectionOptions options, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<XrmEntitySummary>> GetEntitiesAsync(XrmConnectionOptions options, CancellationToken cancellationToken = default);

    Task<XrmEntityDetails> GetEntityDetailsAsync(XrmConnectionOptions options, string logicalName, CancellationToken cancellationToken = default);
}
