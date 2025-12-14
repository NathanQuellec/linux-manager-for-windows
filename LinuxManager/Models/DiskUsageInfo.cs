namespace LinuxManager.Models;

/// <summary>
/// Represents disk usage information for a distribution
/// </summary>
public class DiskUsageInfo
{
    /// <summary>
    /// Total size in human-readable format (e.g., "100G")
    /// </summary>
    public string Size { get; set; } = string.Empty;

    /// <summary>
    /// Used space in human-readable format (e.g., "50G")
    /// </summary>
    public string Used { get; set; } = string.Empty;

    /// <summary>
    /// Available space in human-readable format (e.g., "50G")
    /// </summary>
    public string Available { get; set; } = string.Empty;

    /// <summary>
    /// Usage percentage (e.g., "50%")
    /// </summary>
    public string UsePercentage { get; set; } = string.Empty;

}
