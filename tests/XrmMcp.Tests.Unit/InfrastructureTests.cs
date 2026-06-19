using FluentAssertions;

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
        // Arrange
        var expected = 42;

        // Act
        var actual = 42;

        // Assert
        actual.Should().Be(expected, "xUnit + FluentAssertions should work correctly");
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 5, 10)]
    [InlineData(-1, 1, 0)]
    public void Theory_Tests_Should_Work(int a, int b, int expected)
    {
        // Act
        var result = a + b;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task Async_Tests_Should_Work()
    {
        // Arrange
        var value = "test";

        // Act
        var result = await Task.FromResult(value);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("test");
    }
}
