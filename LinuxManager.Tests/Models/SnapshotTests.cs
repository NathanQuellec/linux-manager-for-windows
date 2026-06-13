using LinuxManager.Contracts.Models;
using LinuxManager.Enums;
using LinuxManager.Models;

namespace LinuxManager.Tests.Models;

public class SnapshotTests
{
    [Fact]
    public void NewSnapshot_DefaultsTypeToArchive()
    {
        // Arrange / Act
        var snapshot = new Snapshot();

        // Assert
        snapshot.Type.Should().Be(SnapshotType.Archive.ToString());
    }

    [Fact]
    public void Snapshot_ImplementsIBaseModel()
    {
        // Arrange / Act
        var snapshot = new Snapshot();

        // Assert
        snapshot.Should().BeAssignableTo<IBaseModel>();
    }

    [Fact]
    public void Properties_WhenAssigned_ReturnAssignedValues()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var snapshot = new Snapshot
        {
            Id = id,
            Name = "Before upgrade",
            Description = "snapshot description",
            Type = SnapshotType.Vhdx.ToString(),
            CreationDate = "2026-06-13",
            Size = "1.2",
            DistroSize = "3.4",
            Path = @"C:\snapshots\snap.tar",
        };

        // Assert
        snapshot.Id.Should().Be(id);
        snapshot.Name.Should().Be("Before upgrade");
        snapshot.Description.Should().Be("snapshot description");
        snapshot.Type.Should().Be("Vhdx");
        snapshot.CreationDate.Should().Be("2026-06-13");
        snapshot.Size.Should().Be("1.2");
        snapshot.DistroSize.Should().Be("3.4");
        snapshot.Path.Should().Be(@"C:\snapshots\snap.tar");
    }
}
