using LinuxManager.Exceptions;

namespace LinuxManager.Tests.Exceptions;

public class CustomExceptionsTests
{
    [Fact]
    public void GzFileExtractionException_DefaultConstructor_CreatesException()
    {
        // Arrange / Act
        var ex = new GzFileExtractionException();

        // Assert
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void GzFileExtractionException_WithMessage_SetsMessage()
    {
        // Arrange / Act
        var ex = new GzFileExtractionException("extraction failed");

        // Assert
        ex.Message.Should().Be("extraction failed");
    }

    [Fact]
    public void FileCompressionException_WithMessage_SetsMessage()
    {
        // Arrange / Act
        var ex = new FileCompressionException("compression failed");

        // Assert
        ex.Should().BeAssignableTo<Exception>();
        ex.Message.Should().Be("compression failed");
    }

    [Fact]
    public void ImportDistributionException_WithMessage_SetsMessage()
    {
        // Arrange / Act
        var ex = new ImportDistributionException("import failed");

        // Assert
        ex.Should().BeAssignableTo<Exception>();
        ex.Message.Should().Be("import failed");
    }

    [Fact]
    public void Exceptions_DefaultConstructors_DoNotThrow()
    {
        // Arrange / Act
        Action act = () =>
        {
            _ = new GzFileExtractionException();
            _ = new FileCompressionException();
            _ = new ImportDistributionException();
        };

        // Assert
        act.Should().NotThrow();
    }
}
