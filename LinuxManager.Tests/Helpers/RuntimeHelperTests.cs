using LinuxManager.Helpers;

namespace LinuxManager.Tests.Helpers;

public class RuntimeHelperTests
{
    [Fact]
    public void IsMSIX_WhenRunningUnpackaged_ReturnsFalse()
    {
        // Arrange / Act - the xUnit test host is never a packaged (MSIX) application,
        // so GetCurrentPackageFullName reports APPMODEL_ERROR_NO_PACKAGE.
        var result = RuntimeHelper.IsMSIX;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMSIX_WhenAccessed_DoesNotThrow()
    {
        // Arrange / Act
        Action act = () => _ = RuntimeHelper.IsMSIX;

        // Assert
        act.Should().NotThrow();
    }
}
