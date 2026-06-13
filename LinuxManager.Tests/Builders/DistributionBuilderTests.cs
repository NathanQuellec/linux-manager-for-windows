using System.Collections.ObjectModel;
using System.Diagnostics;
using LinuxManager.Models;

namespace LinuxManager.Tests.Builders;

public class DistributionBuilderTests
{
    [Fact]
    public void Build_WithNoConfiguration_ReturnsDistributionWithDefaults()
    {
        // Arrange
        var builder = new DistributionBuilder();

        // Act
        var distro = builder.Build();

        // Assert
        distro.Should().NotBeNull();
        distro.Users.Should().NotBeNull().And.BeEmpty();
        distro.Snapshots.Should().NotBeNull().And.BeEmpty();
        distro.RunningProcesses.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Build_WithAllPropertiesConfigured_ReturnsFullyPopulatedDistribution()
    {
        // Arrange
        var id = Guid.NewGuid();
        var diskUsage = new DiskUsageInfo { Size = "100G" };
        var users = new List<string> { "root", "nathan" };
        var snapshots = new ObservableCollection<Snapshot> { new() { Name = "snap1" } };
        var processes = new List<Process>();

        // Act
        var distro = new DistributionBuilder()
            .WithId(id)
            .WithName("Ubuntu")
            .WithPath(@"C:\distros\ubuntu")
            .WithWslVersion(2)
            .WithOsName("Ubuntu")
            .WithOsVersion("22.04")
            .WithDiskUsageInfo(diskUsage)
            .WithUsers(users)
            .WithSnapshots(snapshots)
            .WithRunningProcesses(processes)
            .Build();

        // Assert
        distro.Id.Should().Be(id);
        distro.Name.Should().Be("Ubuntu");
        distro.Path.Should().Be(@"C:\distros\ubuntu");
        distro.WslVersion.Should().Be(2);
        distro.OsName.Should().Be("Ubuntu");
        distro.OsVersion.Should().Be("22.04");
        distro.DiskUsageInfo.Should().BeSameAs(diskUsage);
        distro.Users.Should().BeSameAs(users);
        distro.Snapshots.Should().BeSameAs(snapshots);
        distro.RunningProcesses.Should().BeSameAs(processes);
    }

    [Fact]
    public void WithMethods_WhenCalled_ReturnSameBuilderForChaining()
    {
        // Arrange
        var builder = new DistributionBuilder();

        // Act / Assert
        builder.WithId(Guid.NewGuid()).Should().BeSameAs(builder);
        builder.WithName("a").Should().BeSameAs(builder);
        builder.WithPath("p").Should().BeSameAs(builder);
        builder.WithWslVersion(1).Should().BeSameAs(builder);
        builder.WithOsName("os").Should().BeSameAs(builder);
        builder.WithOsVersion("1.0").Should().BeSameAs(builder);
        builder.WithDiskUsageInfo(new DiskUsageInfo()).Should().BeSameAs(builder);
        builder.WithUsers(new List<string>()).Should().BeSameAs(builder);
        builder.WithSnapshots(new ObservableCollection<Snapshot>()).Should().BeSameAs(builder);
        builder.WithRunningProcesses(new List<Process>()).Should().BeSameAs(builder);
    }

    [Fact]
    public void Build_WhenCalledTwice_ReturnsSameUnderlyingInstance()
    {
        // Arrange
        var builder = new DistributionBuilder().WithName("Ubuntu");

        // Act
        var first = builder.Build();
        var second = builder.Build();

        // Assert
        first.Should().BeSameAs(second);
    }
}
