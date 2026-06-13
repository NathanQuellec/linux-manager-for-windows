using LinuxManager.Helpers;

namespace LinuxManager.Tests.Helpers;

public class UnitHelperTests
{
    [Theory]
    [InlineData(0L, "0")]
    [InlineData(1073741824L, "1")]            // exactly 1 GiB
    [InlineData(1610612736L, "1.5")]          // 1.5 GiB
    [InlineData(536870912L, "0.5")]           // 0.5 GiB
    public void ParseBytesToGigaBytesStr_ForVariousByteCounts_ReturnsRoundedInvariantString(long bytes, string expected)
    {
        // Arrange / Act
        var result = UnitHelper.ParseBytesToGigaBytesStr(bytes);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ParseBytesToGigaBytesStr_WhenValueHasManyDecimals_RoundsToTwoDecimals()
    {
        // Arrange - 1.234567 GiB worth of bytes
        var bytes = (long)(1.234567 * 1024 * 1024 * 1024);

        // Act
        var result = UnitHelper.ParseBytesToGigaBytesStr(bytes);

        // Assert
        result.Should().Be("1.23");
    }

    [Theory]
    [InlineData(0L, 0.0)]
    [InlineData(1073741824L, 1.0)]
    [InlineData(1610612736L, 1.5)]
    [InlineData(536870912L, 0.5)]
    public void BytesToGigaBytesStr_ForVariousByteCounts_ReturnsRoundedDouble(long bytes, double expected)
    {
        // Arrange / Act
        var result = UnitHelper.BytesToGigaBytesStr(bytes);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("50", "100", "50")]
    [InlineData("1", "3", "33.33")]
    [InlineData("0", "100", "0")]
    [InlineData("100", "100", "100")]
    [InlineData("1.5", "3", "50")]
    public void CalculateAndParsePercentage_WithValidNumbers_ReturnsRoundedPercentage(string part, string total, string expected)
    {
        // Arrange / Act
        var result = UnitHelper.CalculateAndParsePercentage(part, total);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("5", "0")]      // division by zero guard
    [InlineData("abc", "100")]  // non-numeric part
    [InlineData("50", "xyz")]   // non-numeric total
    [InlineData("", "")]        // empty inputs
    public void CalculateAndParsePercentage_WithInvalidOrZeroInputs_ReturnsZeroString(string part, string total)
    {
        // Arrange / Act
        var result = UnitHelper.CalculateAndParsePercentage(part, total);

        // Assert
        result.Should().Be("0");
    }
}
