using LinuxManager.Views.Functions;

namespace LinuxManager.Tests.Views;

public class InvertBooleanTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Invert_ReturnsLogicalNegationOfInput(bool input, bool expected)
    {
        // Arrange / Act
        var result = InvertBoolean.Invert(input);

        // Assert
        result.Should().Be(expected);
    }
}
