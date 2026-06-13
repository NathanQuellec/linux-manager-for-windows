using LinuxManager.Helpers;

namespace LinuxManager.Tests.Builders;

public class ProcessBuilderTests
{
    [Fact]
    public void Constructor_SetsStartInfoFileName()
    {
        // Arrange / Act
        using var process = new ProcessBuilder("cmd.exe").Build();

        // Assert
        process.StartInfo.FileName.Should().Be("cmd.exe");
    }

    [Fact]
    public void Build_WithAllOptionsConfigured_PopulatesStartInfo()
    {
        // Arrange / Act
        using var process = new ProcessBuilder("powershell.exe")
            .SetArguments("/c echo hi")
            .SetRedirectStandardOutput(true)
            .SetRedirectStandardError(true)
            .SetUseShellExecute(false)
            .SetCreateNoWindow(true)
            .SetVerb("runas")
            .Build();

        // Assert
        process.StartInfo.FileName.Should().Be("powershell.exe");
        process.StartInfo.Arguments.Should().Be("/c echo hi");
        process.StartInfo.RedirectStandardOutput.Should().BeTrue();
        process.StartInfo.RedirectStandardError.Should().BeTrue();
        process.StartInfo.UseShellExecute.Should().BeFalse();
        process.StartInfo.CreateNoWindow.Should().BeTrue();
        process.StartInfo.Verb.Should().Be("runas");
    }

    [Fact]
    public void SetterMethods_WhenCalled_ReturnSameBuilderForChaining()
    {
        // Arrange
        var builder = new ProcessBuilder("cmd.exe");

        // Act / Assert
        builder.SetArguments("x").Should().BeSameAs(builder);
        builder.SetRedirectStandardOutput(true).Should().BeSameAs(builder);
        builder.SetRedirectStandardError(true).Should().BeSameAs(builder);
        builder.SetUseShellExecute(false).Should().BeSameAs(builder);
        builder.SetCreateNoWindow(true).Should().BeSameAs(builder);
        builder.SetVerb("open").Should().BeSameAs(builder);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetUseShellExecute_StoresProvidedValue(bool value)
    {
        // Arrange / Act
        using var process = new ProcessBuilder("cmd.exe").SetUseShellExecute(value).Build();

        // Assert
        process.StartInfo.UseShellExecute.Should().Be(value);
    }
}
