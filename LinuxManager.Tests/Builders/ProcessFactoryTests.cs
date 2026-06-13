using LinuxManager.Helpers;

namespace LinuxManager.Tests.Builders;

public class ProcessFactoryTests
{
    [Fact]
    public void Create_SetsFileNameAndArguments()
    {
        // Arrange / Act
        using var process = ProcessFactory.Create(ProcessType.Background, "cmd.exe", "/c echo hi");

        // Assert
        process.StartInfo.FileName.Should().Be("cmd.exe");
        process.StartInfo.Arguments.Should().Be("/c echo hi");
    }

    [Fact]
    public void Create_WithoutArguments_LeavesArgumentsEmpty()
    {
        // Arrange / Act
        using var process = ProcessFactory.Create(ProcessType.Default, "explorer.exe");

        // Assert
        process.StartInfo.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void Create_ReadOutput_CapturesStdoutOnlyAndHidesWindow()
    {
        using var process = ProcessFactory.Create(ProcessType.ReadOutput, "cmd.exe", "/c wsl --list");

        process.StartInfo.UseShellExecute.Should().BeFalse();
        process.StartInfo.RedirectStandardOutput.Should().BeTrue();
        process.StartInfo.RedirectStandardError.Should().BeFalse();
        process.StartInfo.CreateNoWindow.Should().BeTrue();
    }

    [Fact]
    public void Create_ReadOutputAndError_CapturesBothStreamsAndHidesWindow()
    {
        using var process = ProcessFactory.Create(ProcessType.ReadOutputAndError, "powershell.exe", "/c x");

        process.StartInfo.UseShellExecute.Should().BeFalse();
        process.StartInfo.RedirectStandardOutput.Should().BeTrue();
        process.StartInfo.RedirectStandardError.Should().BeTrue();
        process.StartInfo.CreateNoWindow.Should().BeTrue();
    }

    [Fact]
    public void Create_Background_HidesWindowWithoutRedirectingStreams()
    {
        using var process = ProcessFactory.Create(ProcessType.Background, "cmd.exe", "/c wsl --shutdown");

        process.StartInfo.UseShellExecute.Should().BeFalse();
        process.StartInfo.RedirectStandardOutput.Should().BeFalse();
        process.StartInfo.RedirectStandardError.Should().BeFalse();
        process.StartInfo.CreateNoWindow.Should().BeTrue();
    }

    [Fact]
    public void Create_Interactive_UsesShellExecuteWithNoWindowFlag()
    {
        using var process = ProcessFactory.Create(ProcessType.Interactive, "cmd.exe", "/c wsl ~");

        process.StartInfo.UseShellExecute.Should().BeTrue();
        process.StartInfo.RedirectStandardOutput.Should().BeFalse();
        process.StartInfo.CreateNoWindow.Should().BeTrue();
    }

    [Fact]
    public void Create_Elevated_UsesShellExecuteWithRunAsVerb()
    {
        using var process = ProcessFactory.Create(ProcessType.Elevated, "powershell.exe", "/c winget install x");

        process.StartInfo.UseShellExecute.Should().BeTrue();
        process.StartInfo.Verb.Should().Be("runas");
    }

    [Fact]
    public void Create_Default_LeavesFrameworkDefaults()
    {
        using var process = ProcessFactory.Create(ProcessType.Default, "explorer.exe", @"\\wsl$\Ubuntu");

        process.StartInfo.UseShellExecute.Should().BeFalse();
        process.StartInfo.RedirectStandardOutput.Should().BeFalse();
        process.StartInfo.RedirectStandardError.Should().BeFalse();
        process.StartInfo.CreateNoWindow.Should().BeFalse();
        process.StartInfo.Verb.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithUnsupportedType_Throws()
    {
        // Arrange
        var invalid = (ProcessType)999;

        // Act
        var act = () => ProcessFactory.Create(invalid, "cmd.exe");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
