using FluentAssertions;
using XrmMcp.Core.Models;
using XrmMcp.Infrastructure.Services;

namespace XrmMcp.Tests.Unit;

/// <summary>
/// Tests de validation de l'infrastructure de test
/// Phase 0 - Setup validation
/// </summary>
public class InfrastructureTests
{
    [Fact]
    public void Test_Framework_Should_Work()
    {
        var expected = 42;
        var actual = 42;

        actual.Should().Be(expected, "xUnit + FluentAssertions should work correctly");
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 5, 10)]
    [InlineData(-1, 1, 0)]
    public void Theory_Tests_Should_Work(int a, int b, int expected)
    {
        var result = a + b;

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Async_Tests_Should_Work()
    {
        var value = "test";
        var result = await Task.FromResult(value);

        result.Should().NotBeNull();
        result.Should().Be("test");
    }

    [Fact]
    public async Task XrmConnectionService_Should_Return_Error_When_ConnectionString_Is_Empty()
    {
        var service = new XrmConnectionService();
        var options = new XrmConnectionOptions("online", XrmAuthType.OAuth, string.Empty);

        var result = await service.TestConnectionAsync(options);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }
}
