using LinuxManager.Models.Docker.Manifests;
using Newtonsoft.Json;

namespace LinuxManager.Tests.Models.Docker;

public class DockerImageManifestTests
{
    [Fact]
    public void GetLayers_ReturnsDigestOfEveryLayer()
    {
        // Arrange
        var sut = new DockerImageManifest
        {
            Layers = new List<Config>
            {
                new() { Digest = "sha256:l1" },
                new() { Digest = "sha256:l2" },
            },
        };

        // Act
        var layers = sut.GetLayers();

        // Assert
        layers.Should().Equal("sha256:l1", "sha256:l2");
    }

    [Fact]
    public void GetLayers_WhenNoLayers_ReturnsEmptyList()
    {
        // Arrange
        var sut = new DockerImageManifest { Layers = new List<Config>() };

        // Act
        var layers = sut.GetLayers();

        // Assert
        layers.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_MapsJsonPropertiesViaAttributes()
    {
        // Arrange
        const string json = """
        {
            "schemaVersion": 2,
            "mediaType": "application/vnd.docker.distribution.manifest.v2+json",
            "config": { "mediaType": "cfg", "size": 7023, "digest": "sha256:config" },
            "layers": [
                { "mediaType": "lay", "size": 2812, "digest": "sha256:layer1" }
            ]
        }
        """;

        // Act
        var manifest = JsonConvert.DeserializeObject<DockerImageManifest>(json);

        // Assert
        manifest.Should().NotBeNull();
        manifest!.SchemaVersion.Should().Be(2);
        manifest.MediaType.Should().Be("application/vnd.docker.distribution.manifest.v2+json");
        manifest.Config.Digest.Should().Be("sha256:config");
        manifest.Config.Size.Should().Be(7023);
        manifest.Layers.Should().ContainSingle();
        manifest.Layers[0].Digest.Should().Be("sha256:layer1");
        manifest.GetLayers().Should().Equal("sha256:layer1");
    }
}
