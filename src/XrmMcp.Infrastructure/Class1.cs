using Microsoft.PowerPlatform.Dataverse.Client;
using XrmMcp.Core;

namespace XrmMcp.Infrastructure;

public sealed class XrmConnectionService : IXrmConnectionService
{
    public Task<XrmConnectionResult> TestConnectionAsync(XrmConnectionOptions options, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return Task.FromResult(new XrmConnectionResult(
                options.Name,
                false,
                false,
                null,
                "Connection string is required."));
        }

        try
        {
            using var client = new ServiceClient(options.ConnectionString);
            var isReady = client.IsReady;

            return Task.FromResult(new XrmConnectionResult(
                options.Name,
                isReady,
                isReady,
                isReady ? client.ConnectedOrgFriendlyName : null,
                isReady ? null : client.LastError));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new XrmConnectionResult(
                options.Name,
                false,
                false,
                null,
                ex.Message));
        }
    }
}
