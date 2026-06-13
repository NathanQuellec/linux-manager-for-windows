using LinuxManager.Enums;

namespace LinuxManager.Tests.Enums;

public class SnapshotTypeTests
{
    [Fact]
    public void SnapshotType_DeclaresExpectedMembers()
    {
        // Arrange / Act
        var names = Enum.GetNames<SnapshotType>();

        // Assert
        names.Should().BeEquivalentTo("Vhdx", "Archive");
    }

    [Theory]
    [InlineData(SnapshotType.Vhdx, "Vhdx")]
    [InlineData(SnapshotType.Archive, "Archive")]
    public void ToString_ReturnsMemberName(SnapshotType type, string expected)
    {
        // Arrange / Act
        var result = type.ToString();

        // Assert
        result.Should().Be(expected);
    }
}
