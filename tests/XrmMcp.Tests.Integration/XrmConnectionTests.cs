namespace XrmMcp.Tests.Integration;

/// <summary>
/// Tests d'intégration - Placeholder pour Phase 1+
/// Ces tests nécessiteront une vraie connexion XRM
/// </summary>
public class XrmConnectionTests
{
    [Fact(Skip = "Requires XRM connection - will be implemented in Phase 1")]
    public async Task Connect_To_Online_Should_Succeed()
    {
        // TODO: Implement in Phase 1
        // This test will validate OAuth connection to Dynamics 365 Online
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires XRM connection - will be implemented in Phase 1")]
    public async Task Connect_To_OnPrem_Should_Succeed()
    {
        // TODO: Implement in Phase 1
        // This test will validate Windows Auth connection to OnPrem
        await Task.CompletedTask;
    }
}
