using System.ComponentModel;
using LinuxManager.Contracts.Models;
using LinuxManager.Models;

namespace LinuxManager.Tests.Models;

public class DistributionTests
{
    [Fact]
    public void NewDistribution_HasInitializedCollectionsAndDefaults()
    {
        // Arrange / Act
        var distro = new Distribution();

        // Assert
        distro.Users.Should().NotBeNull().And.BeEmpty();
        distro.Snapshots.Should().NotBeNull().And.BeEmpty();
        distro.RunningProcesses.Should().NotBeNull().And.BeEmpty();
        distro.SnapshotsTotalSize.Should().Be("0.0");
    }

    [Fact]
    public void Distribution_ImplementsExpectedContracts()
    {
        // Arrange / Act
        var distro = new Distribution();

        // Assert
        distro.Should().BeAssignableTo<INotifyPropertyChanged>();
        distro.Should().BeAssignableTo<IBaseModel>();
    }

    [Fact]
    public void Name_WhenSet_RaisesPropertyChangedWithPropertyName()
    {
        // Arrange
        var distro = new Distribution();
        var raised = new List<string?>();
        distro.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        // Act
        distro.Name = "Ubuntu";

        // Assert
        distro.Name.Should().Be("Ubuntu");
        raised.Should().ContainSingle().Which.Should().Be(nameof(Distribution.Name));
    }

    [Fact]
    public void SnapshotsTotalSize_WhenSet_RaisesPropertyChangedWithPropertyName()
    {
        // Arrange
        var distro = new Distribution();
        string? changed = null;
        distro.PropertyChanged += (_, e) => changed = e.PropertyName;

        // Act
        distro.SnapshotsTotalSize = "12.5";

        // Assert
        distro.SnapshotsTotalSize.Should().Be("12.5");
        changed.Should().Be(nameof(Distribution.SnapshotsTotalSize));
    }

    [Fact]
    public void Properties_WhenAssigned_ReturnAssignedValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var diskUsage = new DiskUsageInfo { Used = "10G" };

        // Act
        var distro = new Distribution
        {
            Id = id,
            Path = @"C:\wsl\ubuntu",
            WslVersion = 2,
            OsName = "Ubuntu",
            OsVersion = "22.04",
            DiskUsageInfo = diskUsage,
        };

        // Assert
        distro.Id.Should().Be(id);
        distro.Path.Should().Be(@"C:\wsl\ubuntu");
        distro.WslVersion.Should().Be(2);
        distro.OsName.Should().Be("Ubuntu");
        distro.OsVersion.Should().Be("22.04");
        distro.DiskUsageInfo.Should().BeSameAs(diskUsage);
    }
}
