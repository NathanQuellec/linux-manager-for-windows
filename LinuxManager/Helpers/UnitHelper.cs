using System.Globalization;

namespace LinuxManager.Helpers;

public static class UnitHelper
{
    public static string ParseBytesToGigaBytesStr(long bytes)
    {
        var gigaBytes = bytes / (1024.0 * 1024.0 * 1024.0);
        return Math.Round(gigaBytes, 2).ToString(CultureInfo.InvariantCulture);
    }

    public static double BytesToGigaBytesStr(long bytes)
    {
        var gigaBytes = bytes / (1024.0 * 1024.0 * 1024.0);
        return Math.Round(gigaBytes, 2);
    }

    public static string CalculateAndParsePercentage(string partStr, string totalStr)
    {
        if (double.TryParse(partStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var part) &&
            double.TryParse(totalStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var total) &&
            total != 0)
        {
            var percentage = (part / total) * 100;
            return Math.Round(percentage, 2).ToString(CultureInfo.InvariantCulture);
        }
        return "0";
    }
}