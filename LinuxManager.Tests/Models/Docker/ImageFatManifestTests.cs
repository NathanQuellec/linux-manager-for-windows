using LinuxManager.Models.Docker.Manifests;
using Newtonsoft.Json;

namespace LinuxManager.Tests.Models.Docker;

public class ImageFatManifestTests
{
    private static ImageFatManifest BuildSample() => new()
    {
        SchemaVersion = 2,
        MediaType = "application/vnd.docker.distribution.manifest.list.v2+json",
        Manifests = new List<Manifest>
        {
            new() { Digest = "sha256:amd", Platform = new Platform { Architecture = "amd64", Os = "linux" } },
            new() { Digest = "sha256:arm", Platform = new Platform { Architecture = "arm64", Os = "linux" } },
        },
    };

    [Fact]
    public void GetLayers_ReturnsDigestOfEveryManifest()
    {
        // Arrange
        var sut = BuildSample();

        // Act
        var layers = sut.GetLayers();

        // Assert
        layers.Should().Equal("sha256:amd", "sha256:arm");
    }

    [Fact]
    public void GetManifestByArchitecture_WhenArchitectureExists_ReturnsMatchingDigest()
    {
        // Arrange
        var sut = BuildSample();

        // Act
        var digest = sut.GetManifestByArchitecture("amd64");

        // Assert
        digest.Should().Be("sha256:amd");
    }

    [Fact]
    public void GetManifestByArchitecture_WhenArchitectureMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = BuildSample();

        // Act
        Action act = () => sut.GetManifestByArchitecture("ppc64le");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deserialize_MapsJsonPropertiesViaAttributes()
    {
        // Arrange
        const string json = """
        {
            "schemaVersion": 2,
            "mediaType": "application/vnd.docker.distribution.manifest.list.v2+json",
            "manifests": [
                { "digest": "sha256:abc", "mediaType": "m", "size": 525,
                  "platform": { "architecture": "amd64", "os": "linux", "variant": "v8" } }
            ]
        }
        """;

        // Act
        var manifest = JsonConvert.DeserializeObject<ImageFatManifest>(json);

        // Assert
        manifest.Should().NotBeNull();
        manifest!.SchemaVersion.Should().Be(2);
        manifest.MediaType.Should().Be("application/vnd.docker.distribution.manifest.list.v2+json");
        manifest.Manifests.Should().ContainSingle();
        manifest.Manifests[0].Digest.Should().Be("sha256:abc");
        manifest.Manifests[0].Size.Should().Be(525);
        manifest.Manifests[0].Platform.Architecture.Should().Be("amd64");
        manifest.Manifests[0].Platform.Os.Should().Be("linux");
        manifest.Manifests[0].Platform.Variant.Should().Be("v8");
    }
}
