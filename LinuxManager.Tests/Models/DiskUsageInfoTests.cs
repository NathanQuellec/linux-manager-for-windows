using LinuxManager.Models;

namespace LinuxManager.Tests.Models;

public class DiskUsageInfoTests
{
    [Fact]
    public void NewDiskUsageInfo_DefaultsAllPropertiesToEmptyString()
    {
        // Arrange / Act
        var info = new DiskUsageInfo();

        // Assert
        info.Size.Should().BeEmpty();
        info.Used.Should().BeEmpty();
        info.Available.Should().BeEmpty();
        info.UsePercentage.Should().BeEmpty();
    }

    [Fact]
    public void Properties_WhenAssigned_ReturnAssignedValues()
    {
        // Arrange / Act
        var info = new DiskUsageInfo
        {
            Size = "100G",
            Used = "50G",
            Available = "50G",
            UsePercentage = "50%",
        };

        // Assert
        info.Size.Should().Be("100G");
        info.Used.Should().Be("50G");
        info.Available.Should().Be("50G");
        info.UsePercentage.Should().Be("50%");
    }
}
