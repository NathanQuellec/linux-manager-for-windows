using LinuxManager.Models.Docker;
using Newtonsoft.Json;

namespace LinuxManager.Tests.Models.Docker;

public class AuthTokenTests
{
    [Fact]
    public void Properties_WhenAssigned_ReturnAssignedValues()
    {
        // Arrange / Act
        var token = new AuthToken
        {
            Token = "abc",
            AccessToken = "def",
            ExpiresIn = 300,
        };

        // Assert
        token.Token.Should().Be("abc");
        token.AccessToken.Should().Be("def");
        token.ExpiresIn.Should().Be(300);
    }

    [Fact]
    public void Deserialize_MapsSnakeCaseJsonPropertiesViaAttributes()
    {
        // Arrange
        const string json = """
        {
            "token": "header.payload.signature",
            "access_token": "access-123",
            "expires_in": 300,
            "issued_at": "2026-06-13T10:00:00Z"
        }
        """;

        // Act
        var token = JsonConvert.DeserializeObject<AuthToken>(json);

        // Assert
        token.Should().NotBeNull();
        token!.Token.Should().Be("header.payload.signature");
        token.AccessToken.Should().Be("access-123");
        token.ExpiresIn.Should().Be(300);
        token.IssuedAt.UtcDateTime.Should().Be(new DateTime(2026, 6, 13, 10, 0, 0, DateTimeKind.Utc));
    }
}
