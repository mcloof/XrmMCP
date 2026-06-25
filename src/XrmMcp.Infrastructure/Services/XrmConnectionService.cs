using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using XrmMcp.Core.Interfaces;
using XrmMcp.Core.Models;

namespace XrmMcp.Infrastructure.Services;

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

    public Task<IReadOnlyCollection<XrmEntitySummary>> GetEntitiesAsync(XrmConnectionOptions options, CancellationToken cancellationToken = default)
    {
        var client = CreateReadyClient(options);

        using (client)
        {
            var request = new RetrieveAllEntitiesRequest
            {
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveAllEntitiesResponse)client.Execute(request);

            var entities = response.EntityMetadata
                .OrderBy(e => e.LogicalName)
                .Select(e => new XrmEntitySummary(
                    e.LogicalName,
                    string.IsNullOrWhiteSpace(e.DisplayName?.UserLocalizedLabel?.Label)
                        ? null
                        : e.DisplayName.UserLocalizedLabel.Label,
                    e.PrimaryIdAttribute,
                    e.PrimaryNameAttribute))
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<XrmEntitySummary>>(entities);
        }
    }

    public Task<XrmEntityDetails> GetEntityDetailsAsync(XrmConnectionOptions options, string logicalName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(logicalName))
        {
            throw new ArgumentException("Entity logical name is required.", nameof(logicalName));
        }

        var client = CreateReadyClient(options);

        using (client)
        {
            var request = new RetrieveEntityRequest
            {
                LogicalName = logicalName,
                EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)client.Execute(request);
            var entity = response.EntityMetadata;

            var attributes = (entity.Attributes ?? Array.Empty<AttributeMetadata>())
                .OrderBy(a => a.LogicalName)
                .Select(a => new XrmAttributeSummary(
                    a.LogicalName ?? string.Empty,
                    a.DisplayName?.UserLocalizedLabel?.Label,
                    a.AttributeTypeName?.Value ?? a.AttributeType?.ToString() ?? "Unknown",
                    a.IsCustomAttribute ?? false,
                    string.Equals(a.LogicalName, entity.PrimaryIdAttribute, StringComparison.OrdinalIgnoreCase),
                    string.Equals(a.LogicalName, entity.PrimaryNameAttribute, StringComparison.OrdinalIgnoreCase),
                    a.IsValidForCreate ?? false,
                    a.IsValidForUpdate ?? false,
                    a.IsValidForRead ?? false))
                .ToArray();

            var oneToMany = (entity.OneToManyRelationships ?? Array.Empty<OneToManyRelationshipMetadata>())
                .Select(r => new XrmRelationshipSummary(
                    r.SchemaName ?? string.Empty,
                    "OneToMany",
                    r.ReferencedEntity ?? string.Empty,
                    r.ReferencingEntity ?? string.Empty,
                    r.ReferencingAttribute,
                    r.IsCustomRelationship ?? false));

            var manyToOne = (entity.ManyToOneRelationships ?? Array.Empty<OneToManyRelationshipMetadata>())
                .Select(r => new XrmRelationshipSummary(
                    r.SchemaName ?? string.Empty,
                    "ManyToOne",
                    r.ReferencedEntity ?? string.Empty,
                    r.ReferencingEntity ?? string.Empty,
                    r.ReferencingAttribute,
                    r.IsCustomRelationship ?? false));

            var manyToMany = (entity.ManyToManyRelationships ?? Array.Empty<ManyToManyRelationshipMetadata>())
                .Select(r => new XrmRelationshipSummary(
                    r.SchemaName ?? string.Empty,
                    "ManyToMany",
                    r.Entity1LogicalName ?? string.Empty,
                    r.Entity2LogicalName ?? string.Empty,
                    null,
                    r.IsCustomRelationship ?? false));

            var relationships = oneToMany
                .Concat(manyToOne)
                .Concat(manyToMany)
                .OrderBy(r => r.SchemaName)
                .ToArray();

            var details = new XrmEntityDetails(
                entity.LogicalName ?? logicalName,
                entity.DisplayName?.UserLocalizedLabel?.Label,
                entity.Description?.UserLocalizedLabel?.Label,
                entity.PrimaryIdAttribute,
                entity.PrimaryNameAttribute,
                entity.IsCustomEntity ?? false,
                attributes,
                relationships);

            return Task.FromResult(details);
        }
    }

    private static ServiceClient CreateReadyClient(XrmConnectionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(options));
        }

        var client = new ServiceClient(options.ConnectionString);
        if (!client.IsReady)
        {
            var error = client.LastError ?? "Unable to connect to XRM.";
            client.Dispose();
            throw new InvalidOperationException(error);
        }

        return client;
    }
}
