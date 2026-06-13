using System.IO.Compression;
using System.Text;
using LinuxManager.Helpers;

namespace LinuxManager.Tests.Helpers;

/// <summary>
/// Exercises <see cref="ArchiveHelper"/> against a real, isolated temp directory.
/// </summary>
public class ArchiveHelperTests : IDisposable
{
    private readonly string _root;

    public ArchiveHelperTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "lm-archive-" + Guid.NewGuid().ToString("N"));
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
    public async Task DecompressArchive_WithValidGzip_DecompressesAndRemovesSource()
    {
        // Arrange
        const string original = "archive payload contents";
        var sourcePath = Path.Combine(_root, "image.tar.gz");
        await using (var fs = File.Create(sourcePath))
        await using (var gz = new GZipStream(fs, CompressionMode.Compress))
        {
            var bytes = Encoding.UTF8.GetBytes(original);
            gz.Write(bytes, 0, bytes.Length);
        }

        // Act
        var resultPath = await ArchiveHelper.DecompressArchive(sourcePath);

        // Assert
        resultPath.Should().Be(Path.Combine(_root, "image.tar"));
        File.Exists(resultPath).Should().BeTrue();
        File.Exists(sourcePath).Should().BeFalse("the source .gz is deleted after decompression");
        (await File.ReadAllTextAsync(resultPath!)).Should().Be(original);
    }

    [Fact]
    public async Task DecompressArchive_WhenFileMissing_ReturnsNull()
    {
        // Arrange
        var missing = Path.Combine(_root, "missing.tar.gz");

        // Act
        var result = await ArchiveHelper.DecompressArchive(missing);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task MergeArchive_WithMultipleTars_StripsEofMarkerFromAllButLast()
    {
        // Arrange - two 2048-byte "tar" files; the EOF marker (1024 bytes) is stripped from all but the last
        var tar0 = Path.Combine(_root, "layer0.tar");
        var tar1 = Path.Combine(_root, "layer1.tar");
        File.WriteAllBytes(tar0, new byte[2048]);
        File.WriteAllBytes(tar1, new byte[2048]);
        var dest = Path.Combine(_root, "merged.tar");

        // Act
        await ArchiveHelper.MergeArchive(new List<string> { tar0, tar1 }, dest);

        // Assert - (2048 - 1024) + 2048
        new FileInfo(dest).Length.Should().Be(3072);
    }

    [Fact]
    public async Task MergeArchive_WithSingleTar_CopiesItWhole()
    {
        // Arrange
        var tar = Path.Combine(_root, "only.tar");
        File.WriteAllBytes(tar, new byte[2048]);
        var dest = Path.Combine(_root, "merged-single.tar");

        // Act
        await ArchiveHelper.MergeArchive(new List<string> { tar }, dest);

        // Assert - the single (last) file is copied in full, no marker stripped
        new FileInfo(dest).Length.Should().Be(2048);
    }
}
