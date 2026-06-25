using Xunit;
using XrmMcp.Core.Models;
using XrmMcp.Infrastructure.Services;
using Xunit.Sdk;

namespace XrmMcp.Tests.Integration;

/// <summary>
/// Tests d'intégration Phase 1 - Connexion XRM réelle
/// Activés uniquement si les variables d'environnement sont définies
/// </summary>
public class XrmConnectionTests
{
    private const string OnlineConnectionStringEnvVar = "XRMMCP_ONLINE_CONNECTION_STRING";
    private const string OnPremConnectionStringEnvVar = "XRMMCP_ONPREM_CONNECTION_STRING";

    [Fact]
    public async Task Connect_To_Online_Should_Succeed()
    {
        // Arrange
        var connectionString = Environment.GetEnvironmentVariable(OnlineConnectionStringEnvVar);
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw SkipException.ForSkip($"Test skipped: Environment variable '{OnlineConnectionStringEnvVar}' not set");
        }

        var service = new XrmConnectionService();
        var options = new XrmConnectionOptions(
            "OnlineTest",
            XrmAuthType.OAuth,
            connectionString);

        // Act
        var result = await service.TestConnectionAsync(options);

        // Assert
        Assert.True(result.IsSuccess, $"Connection failed: {result.ErrorMessage}");
        Assert.True(result.IsReady, "ServiceClient is not ready");
        Assert.NotNull(result.ConnectedOrganization);
        Assert.NotEmpty(result.ConnectedOrganization);
    }

    [Fact]
    public async Task Connect_To_OnPrem_Should_Succeed()
    {
        // Arrange
        var connectionString = Environment.GetEnvironmentVariable(OnPremConnectionStringEnvVar);
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw SkipException.ForSkip($"Test skipped: Environment variable '{OnPremConnectionStringEnvVar}' not set");
        }

        var service = new XrmConnectionService();
        var options = new XrmConnectionOptions(
            "OnPremTest",
            XrmAuthType.ActiveDirectory,
            connectionString);

        // Act
        var result = await service.TestConnectionAsync(options);

        // Assert
        Assert.True(result.IsSuccess, $"Connection failed: {result.ErrorMessage}");
        Assert.True(result.IsReady, "ServiceClient is not ready");
        Assert.NotNull(result.ConnectedOrganization);
        Assert.NotEmpty(result.ConnectedOrganization);
    }
}