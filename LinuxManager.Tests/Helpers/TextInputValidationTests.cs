using System.Text.RegularExpressions;
using LinuxManager.Helpers;

namespace LinuxManager.Tests.Helpers;

public class TextInputValidationTests
{
    [Fact]
    public void NotNullOrWhiteSpace_WithNonEmptyText_ReturnsSameInstance()
    {
        // Arrange
        var sut = new TextInputValidation("Ubuntu");

        // Act
        var result = sut.NotNullOrWhiteSpace();

        // Assert
        result.Should().BeSameAs(sut);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotNullOrWhiteSpace_WithNullOrWhiteSpace_ThrowsArgumentException(string? input)
    {
        // Arrange
        var sut = new TextInputValidation(input!);

        // Act
        Action act = () => sut.NotNullOrWhiteSpace();

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*cannot be empty*");
    }

    [Fact]
    public void IncludeWhiteSpaceChar_WhenTextContainsWhiteSpace_ThrowsArgumentException()
    {
        // Arrange
        var sut = new TextInputValidation("my distro");

        // Act
        Action act = () => sut.IncludeWhiteSpaceChar();

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*cannot include white spaces*");
    }

    [Fact]
    public void IncludeWhiteSpaceChar_WhenTextHasNoWhiteSpace_ReturnsSameInstance()
    {
        // Arrange
        var sut = new TextInputValidation("mydistro");

        // Act
        var result = sut.IncludeWhiteSpaceChar();

        // Assert
        result.Should().BeSameAs(sut);
    }

    [Theory]
    [InlineData("abc", 3)]   // exactly minimum length (boundary)
    [InlineData("abcd", 3)]  // above minimum
    public void MinimumLength_WhenLengthMeetsMinimum_ReturnsSameInstance(string input, int minLength)
    {
        // Arrange
        var sut = new TextInputValidation(input);

        // Act
        var result = sut.MinimumLength(minLength);

        // Assert
        result.Should().BeSameAs(sut);
    }

    [Fact]
    public void MinimumLength_WhenShorterThanMinimum_ThrowsArgumentException()
    {
        // Arrange
        var sut = new TextInputValidation("ab");

        // Act
        Action act = () => sut.MinimumLength(3);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*at least 3 characters*");
    }

    [Fact]
    public void InvalidCharacters_WhenInputMatchesAllowedPattern_ReturnsSameInstance()
    {
        // Arrange
        var sut = new TextInputValidation("Ubuntu22");
        var regex = new Regex("^[a-zA-Z0-9]+$");

        // Act
        var result = sut.InvalidCharacters(regex, "special characters");

        // Assert
        result.Should().BeSameAs(sut);
    }

    [Fact]
    public void InvalidCharacters_WhenInputDoesNotMatchAllowedPattern_ThrowsArgumentException()
    {
        // Arrange
        var sut = new TextInputValidation("Ubuntu@22");
        var regex = new Regex("^[a-zA-Z0-9]+$");

        // Act
        Action act = () => sut.InvalidCharacters(regex, "special characters");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*cannot include special characters*");
    }

    [Fact]
    public void DataAlreadyExist_WhenCollectionContainsValueCaseInsensitive_ThrowsArgumentException()
    {
        // Arrange
        var sut = new TextInputValidation("ubuntu");
        var existing = new List<string> { "Ubuntu", "Debian" };

        // Act
        Action act = () => sut.DataAlreadyExist(existing);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*already exists*");
    }

    [Fact]
    public void DataAlreadyExist_WhenCollectionDoesNotContainValue_ReturnsSameInstance()
    {
        // Arrange
        var sut = new TextInputValidation("Fedora");
        var existing = new List<string> { "Ubuntu", "Debian" };

        // Act
        var result = sut.DataAlreadyExist(existing);

        // Assert
        result.Should().BeSameAs(sut);
    }

    [Fact]
    public void Validators_WhenChainedOnValidInput_AllPassAndReturnSameInstance()
    {
        // Arrange
        var sut = new TextInputValidation("Ubuntu");
        var existing = new List<string> { "Debian" };

        // Act
        var result = sut
            .NotNullOrWhiteSpace()
            .IncludeWhiteSpaceChar()
            .MinimumLength(2)
            .InvalidCharacters(new Regex("^[a-zA-Z0-9]+$"), "special characters")
            .DataAlreadyExist(existing);

        // Assert
        result.Should().BeSameAs(sut);
    }
}
