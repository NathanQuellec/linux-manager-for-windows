using LinuxManager.Core.Contracts.Services;
using LinuxManager.Core.Services;

namespace LinuxManager.Tests.Core;

/// <summary>
/// Exercises <see cref="FileService"/> against a real, isolated temp directory.
/// </summary>
public class FileServiceTests : IDisposable
{
    private readonly string _root;
    private readonly FileService _sut;

    public FileServiceTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "lm-fileservice-" + Guid.NewGuid().ToString("N"));
        _sut = new FileService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private sealed class Sample
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public void FileService_ImplementsIFileService()
    {
        // Assert
        _sut.Should().BeAssignableTo<IFileService>();
    }

    [Fact]
    public void Save_WhenFolderDoesNotExist_CreatesFolderAndWritesFile()
    {
        // Arrange
        var content = new Sample { Name = "ubuntu", Value = 42 };

        // Act
        _sut.Save(_root, "data.json", content);

        // Assert
        Directory.Exists(_root).Should().BeTrue();
        File.Exists(Path.Combine(_root, "data.json")).Should().BeTrue();
    }

    [Fact]
    public void SaveThenRead_RoundTripsContent()
    {
        // Arrange
        var content = new Sample { Name = "debian", Value = 7 };
        _sut.Save(_root, "roundtrip.json", content);

        // Act
        var result = _sut.Read<Sample>(_root, "roundtrip.json");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("debian");
        result.Value.Should().Be(7);
    }

    [Fact]
    public void Read_WhenFileDoesNotExist_ReturnsDefaultForReferenceType()
    {
        // Act
        var result = _sut.Read<Sample>(_root, "nope.json");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Read_WhenFileDoesNotExist_ReturnsDefaultForValueType()
    {
        // Act
        var result = _sut.Read<int>(_root, "nope.json");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Delete_WhenFileExists_RemovesFile()
    {
        // Arrange
        _sut.Save(_root, "todelete.json", new Sample());
        var path = Path.Combine(_root, "todelete.json");
        File.Exists(path).Should().BeTrue();

        // Act
        _sut.Delete(_root, "todelete.json");

        // Assert
        File.Exists(path).Should().BeFalse();
    }

    [Fact]
    public void Delete_WhenFileNameIsNull_DoesNotThrow()
    {
        // Act
        Action act = () => _sut.Delete(_root, null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Delete_WhenFileDoesNotExist_DoesNotThrow()
    {
        // Act
        Action act = () => _sut.Delete(_root, "ghost.json");

        // Assert
        act.Should().NotThrow();
    }
}
