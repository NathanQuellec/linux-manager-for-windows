using System.Globalization;
using System.Text.RegularExpressions;
using LinuxManager.Contracts.Services;
using LinuxManager.Helpers;
using LinuxManager.Models;
using Serilog;

namespace LinuxManager.Services;

/// <summary>
/// Provides runtime information about distributions (OS metadata, users, disk usage, etc.).
/// </summary>
public class DistributionInfosService : IDistributionInfosService
{
    private const string WSL_UNC_PATH = @"\\wsl$";

    /// <summary>
    /// Retrieve a field value (e.g. NAME / VERSION) from the distribution's os-release file.
    /// Strategy:
    /// 1. Try /etc/os-release via ext4.vhdx.
    /// 2. Fallback to /usr/lib/os-release if missing.
    /// 3. If ext4.vhdx locked (running), read live UNC path.
    /// </summary>
    public string GetOsInfos(string distroName, string distroPath, string field)
    {
        Log.Information($"Fetching OS information for {distroName}");

        var osInfosPattern = $@"(\b{field}=\"")(.*?)\"""; // capture value between quotes after FIELD="..."
        var osReleasePrimary = Path.Combine("etc", "os-release");
        var osReleaseFallback = Path.Combine("usr", "lib", "os-release");
        string value;

        try
        {
            value = GetOsInfosFromVhdx(distroPath, osReleasePrimary, osInfosPattern);
        }
        catch (FileNotFoundException ex)
        {
            Log.Warning($"/etc/os-release not found; using fallback for {distroName}: {ex.Message}");
            value = GetOsInfosFromVhdx(distroPath, osReleaseFallback, osInfosPattern);
        }
        catch (IOException ex)
        {
            Log.Warning($"ext4.vhdx locked; reading live FS for {distroName}: {ex.Message}");
            value = GetOsInfosFromFileSystem(distroName, osInfosPattern);
        }
        catch (Exception ex)
        {
            Log.Error($"Error fetching OS info for {distroName}: {ex}");
            value = "Unknown";
        }

        return string.IsNullOrEmpty(value) ? "Unknown" : value;
    }

    /// <summary>Get disk usage information by invoking df inside the distribution.</summary>
    public DiskUsageInfo GetDistributionDiskUsageInfo(string distroName)
    {
        Log.Information($"Fetching disk usage for {distroName}");
        try
        {
            var process = new ProcessBuilder("powershell.exe")
                .SetArguments($"/c wsl.exe --system -d {distroName} df -B1 /mnt/wslg/distro")
                .SetCreateNoWindow(true)
                .SetUseShellExecute(false)
                .SetRedirectStandardOutput(true)
                .SetRedirectStandardError(true)
                .Build();
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var errorOutput = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(errorOutput))
            {
                Log.Error($"Error fetching disk usage for {distroName}: {errorOutput}");
                return new DiskUsageInfo();
            }
            return ParseDiskUsageOutput(output);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to fetch disk usage for {distroName}: {ex}");
            return new DiskUsageInfo();
        }
    }

    private static DiskUsageInfo ParseDiskUsageOutput(string output)
    {
        var info = new DiskUsageInfo();
        if (string.IsNullOrWhiteSpace(output))
        {
            Log.Warning("Disk usage output empty");
            return info;
        }
        try
        {
            var lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                Log.Warning("Disk usage output has fewer than 2 lines");
                return info;
            }
            var dataLine = lines[1].Trim();
            var parts = dataLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 6)
            {
                Log.Warning($"Unexpected disk usage format: {dataLine}");
                return info;
            }
            info.Size = UnitHelper.ParseBytesToGigaBytesStr(long.Parse(parts[1]));
            info.Used = UnitHelper.ParseBytesToGigaBytesStr(long.Parse(parts[2]));
            info.Available = UnitHelper.ParseBytesToGigaBytesStr(long.Parse(parts[3]));
            info.UsePercentage = UnitHelper.CalculateAndParsePercentage(info.Used, info.Size);
            Log.Information($"Parsed disk usage: Size={info.Size}, Used={info.Used}, Avail={info.Available}, Percent={info.UsePercentage}");
            return info;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed parsing disk usage output: {ex}");
            return info;
        }
    }

    private static string GetOsInfosFromVhdx(string distroPath, string osReleaseFile, string pattern)
    {
        Log.Information("Reading os-release from ext4.vhdx");
        var vhdxPath = Path.Combine(distroPath, "ext4.vhdx");
        try
        {
            var helper = new WslImageHelper(vhdxPath);
            var fileContent = helper.ReadFile(osReleaseFile);
            return Regex.Match(fileContent, pattern).Groups[2].Value;
        }
        catch (FileNotFoundException) { throw; }
        catch (IOException) { throw; }
        catch (Exception ex)
        {
            Log.Error($"Error reading os-release: {ex}");
            throw;
        }
    }

    private static string GetOsInfosFromFileSystem(string distroName, string pattern)
    {
        Log.Information("Reading os-release from live FS");
        var osReleasePath = Path.Combine(WSL_UNC_PATH, distroName, "etc", "os-release");
        try
        {
            if (File.Exists(osReleasePath) && new FileInfo(osReleasePath).Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                Log.Warning("/etc/os-release is a symlink; using fallback file");
                osReleasePath = Path.Combine(WSL_UNC_PATH, distroName, "usr", "lib", "os-release");
            }
            var content = File.ReadAllText(osReleasePath);
            var value = Regex.Match(content, pattern).Groups[2].Value;
            return string.IsNullOrEmpty(value) ? "Unknown" : value;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed reading live os-release: {ex}");
            return "Unknown";
        }
    }

    /// <summary>Get user list from /etc/passwd via vhdx or live FS fallback.</summary>
    public List<string> GetDistributionUsers(string distroName, string distroPath)
    {
        Log.Information($"Fetching users for {distroName}");
        const string shellPattern = @"/bin/(.*?)sh$";
        try { return GetUsersFromExt4(distroPath, shellPattern); }
        catch (IOException ex)
        {
            Log.Warning($"ext4.vhdx locked; reading live FS users for {distroName}: {ex.Message}");
            return GetUsersFromFileSystem(distroName, shellPattern);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to obtain users for {distroName}: {ex}");
            return new List<string> { "Unknown" };
        }
    }

    private static List<string> GetUsersFromExt4(string distroPath, string pattern)
    {
        Log.Information("Reading /etc/passwd from ext4.vhdx");
        var passwdPath = Path.Combine("etc", "passwd");
        var vhdxPath = Path.Combine(distroPath, "ext4.vhdx");
        try
        {
            var helper = new WslImageHelper(vhdxPath);
            var content = helper.ReadFile(passwdPath);
            return content.Split('\n')
                .Where(line => Regex.IsMatch(line, pattern))
                .Select(line => line.Split(':')[0])
                .ToList();
        }
        catch (IOException) { throw; }
        catch (Exception ex)
        {
            Log.Error($"Error parsing passwd from image: {ex}");
            return new List<string> { "Unknown" };
        }
    }

    private static List<string> GetUsersFromFileSystem(string distroName, string pattern)
    {
        Log.Information("Reading /etc/passwd from live FS");
        var passwdPath = Path.Combine(WSL_UNC_PATH, distroName, "etc", "passwd");
        try
        {
            var content = File.ReadAllText(passwdPath);
            return content.Split('\n')
                .Where(line => Regex.IsMatch(line, pattern))
                .Select(line => line.Split(':')[0])
                .ToList();
        }
        catch (Exception ex)
        {
            Log.Error($"Error parsing passwd from live FS: {ex}");
            return new List<string> { "Unknown" };
        }
    }
}