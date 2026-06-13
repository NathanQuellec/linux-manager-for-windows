using System.IO.Compression;
using System.Text;
using LinuxManager.Exceptions;
using LinuxManager.Helpers;

namespace LinuxManager.Tests.Helpers;

/// <summary>
/// Exercises <see cref="FilesHelper"/> against a real, isolated temp directory that is
/// removed after each test, so the file-system side effects stay deterministic and contained.
/// </summary>
public class FilesHelperTests : IDisposable
{
    private readonly string _root;

    public FilesHelperTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "lm-files-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void CreateDirectory_WhenDirectoryDoesNotExist_CreatesItAndReturnsPath()
    {
        // Arrange
        var expected = Path.Combine(_root, "newdir");

        // Act
        var result = FilesHelper.CreateDirectory(_root, "newdir");

        // Assert
        result.Should().Be(expected);
        Directory.Exists(expected).Should().BeTrue();
    }

    [Fact]
    public void CreateDirectory_WhenDirectoryAlreadyExists_ReturnsPathWithoutThrowing()
    {
        // Arrange
        FilesHelper.CreateDirectory(_root, "dup");

        // Act
        var result = FilesHelper.CreateDirectory(_root, "dup");

        // Assert
        result.Should().Be(Path.Combine(_root, "dup"));
    }

    [Fact]
    public void CreateDirectory_WhenNameIsInvalid_ReturnsNull()
    {
        // Arrange - a null character is an illegal path character on every platform
        var invalidName = "bad\0name";

        // Act
        var result = FilesHelper.CreateDirectory(_root, invalidName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RemoveDirectory_WhenDirectoryIsEmpty_DeletesIt()
    {
        // Arrange
        var dir = Path.Combine(_root, "to-remove");
        Directory.CreateDirectory(dir);

        // Act
        FilesHelper.RemoveDirectory(dir);

        // Assert
        Directory.Exists(dir).Should().BeFalse();
    }

    [Fact]
    public void RemoveDirectory_WhenDirectoryMissing_DoesNotThrow()
    {
        // Arrange
        var dir = Path.Combine(_root, "does-not-exist");

        // Act
        Action act = () => FilesHelper.RemoveDirectory(dir);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RemoveDirContent_RemovesFilesAndEmptySubDirectories()
    {
        // Arrange
        var dir = Path.Combine(_root, "with-content");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "file1.txt"), "data");
        Directory.CreateDirectory(Path.Combine(dir, "emptysub"));

        // Act
        FilesHelper.RemoveDirContent(dir);

        // Assert
        Directory.EnumerateFiles(dir).Should().BeEmpty();
        Directory.EnumerateDirectories(dir).Should().BeEmpty();
    }

    [Fact]
    public async Task ExtractGzFile_WithValidGzip_WritesDecompressedContent()
    {
        // Arrange
        const string original = "the quick brown fox";
        var sourceGz = Path.Combine(_root, "source.gz");
        var dest = Path.Combine(_root, "extracted.txt");
        await using (var fs = File.Create(sourceGz))
        await using (var gz = new GZipStream(fs, CompressionMode.Compress))
        {
            var bytes = Encoding.UTF8.GetBytes(original);
            gz.Write(bytes, 0, bytes.Length);
        }

        // Act
        await FilesHelper.ExtractGzFile(sourceGz, dest);

        // Assert
        File.Exists(dest).Should().BeTrue();
        (await File.ReadAllTextAsync(dest)).Should().Be(original);
    }

    [Fact]
    public async Task ExtractGzFile_WhenSourceMissing_ThrowsGzFileExtractionException()
    {
        // Arrange
        var missing = Path.Combine(_root, "missing.gz");
        var dest = Path.Combine(_root, "out.txt");

        // Act
        Func<Task> act = () => FilesHelper.ExtractGzFile(missing, dest);

        // Assert
        await act.Should().ThrowAsync<GzFileExtractionException>();
    }
}
