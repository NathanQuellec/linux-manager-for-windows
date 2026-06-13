using LinuxManager.Helpers;

namespace LinuxManager.Tests.Helpers;

public class WslImageHelperTests
{
    [Fact]
    public void Constructor_WithAnyPath_DoesNotThrow()
    {
        // Arrange / Act
        Action act = () => _ = new WslImageHelper(@"C:\does\not\exist.vhdx");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ReadFile_WhenVhdxImageDoesNotExist_Throws()
    {
        // Arrange
        var missingImage = Path.Combine(Path.GetTempPath(), "missing-" + Guid.NewGuid().ToString("N") + ".vhdx");
        var sut = new WslImageHelper(missingImage);

        // Act
        Action act = () => sut.ReadFile("/etc/os-release");

        // Assert
        act.Should().Throw<Exception>();
    }
}
