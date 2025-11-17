using System.Globalization;
using System.Text.RegularExpressions;
using LinuxManager.Contracts.Services;
using LinuxManager.Helpers;
using LinuxManager.Models;
using Serilog;

namespace LinuxManager.Services;

/** <summary>
 * Fetch informations about distributions (os, users, ...)
 * </summary>
 */
public class DistributionInfosService : IDistributionInfosService
{
    private const string WSL_UNC_PATH = @"\\wsl$";

    /*  To get distributions infos, we try at first to read the image "ext4.vhdx" and open the file /etc/os-release.
        If we cannot read ext4.dhdx, that means the distribution is runningand we can get os-release file from the 
        file system located at \\wsl$\distroname\...
    */
    public string GetOsInfos(string distroName, string distroPath, string field)
    {
        Log.Information($"Fetching OS information of distribution {distroName} ...");

        var osInfosPattern = $@"(\b{field}="")(.*?)""";
        var osInfosFile = Path.Combine("etc", "os-release");
        var osInfosFileFallBack = Path.Combine("usr", "lib", "os-release");
        string osInfos;

        try
        {
            osInfos = GetOsInfosFromVhdx(distroPath, osInfosFile, osInfosPattern);
        }
        catch (FileNotFoundException ex)
        {
            // fallback following os-release specs : https://www.freedesktop.org/software/systemd/man/os-release.html

            Log.Error($"Didn't find /etc/os-release, retry with fallback file - Caused by exception : {ex}");
            osInfos = GetOsInfosFromVhdx(distroPath, osInfosFileFallBack, osInfosPattern);
        }
        catch (IOException ex)
        {
            /*  if we cannot read ext4.dhdx, that means the distribution is running
                and we can get os-release file from the file system located at \\wsl$\distroname\...
             */

            Log.Error($"Another process is already reading ext4.vhdx - Caused by exception : {ex}");
            osInfos = GetOsInfosFromFileSystem(distroName, osInfosPattern);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed fetch OS information - Caused by exception {ex}");
            osInfos = "Unknown";
        }

        return string.IsNullOrEmpty(osInfos) ? "Unknown" : osInfos;
    }

    public DiskUsageInfo GetDistributionDiskUsageInfo(string distroName)
    {
        Log.Information($"Fetching {distroName} disk usage information ...");

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
            Log.Error($"Failed to fetch distribution disk usage information - Caused by exception : {ex}");
            return new DiskUsageInfo();
        }
    }

    private static DiskUsageInfo ParseDiskUsageOutput(string output)
    {
        var diskUsageInfo = new DiskUsageInfo();

        if (string.IsNullOrWhiteSpace(output))
        {
            Log.Warning("Disk usage output is empty");
            return diskUsageInfo;
        }

        try
        {
            var lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
            {
                Log.Warning("Disk usage output has unexpected format (less than 2 lines)");
                return diskUsageInfo;
            }

            // Skip the header line and parse the data line
            var dataLine = lines[1].Trim();
            
            // Split by whitespace while preserving the mount point which might contain spaces
            var parts = dataLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 6)
            {
                Log.Warning($"Disk usage data line has unexpected format: {dataLine}");
                return diskUsageInfo;
            }

            // Parse each field from the df output
            diskUsageInfo.Size = UnitHelper.ParseBytesToGigaBytesStr(long.Parse(parts[1]));
            diskUsageInfo.Used = UnitHelper.ParseBytesToGigaBytesStr(long.Parse(parts[2]));
            diskUsageInfo.Available = UnitHelper.ParseBytesToGigaBytesStr(long.Parse(parts[3]));
            diskUsageInfo.UsePercentage = UnitHelper.CalculateAndParsePercentage(diskUsageInfo.Used, diskUsageInfo.Size);

            Log.Information($"Successfully parsed disk usage info: " +
                $"Size={diskUsageInfo.Size}, " +
                $"Used={diskUsageInfo.Used}, " +
                $"Available={diskUsageInfo.Available}, " +
                $"UsePercentage={diskUsageInfo.UsePercentage}, ");

            return diskUsageInfo;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to parse disk usage output - Caused by exception: {ex}");
            return diskUsageInfo;
        }
    }

    private static string GetOsInfosFromVhdx(string distroPath, string osInfosFilePath, string osInfosPattern)
    {
        Log.Information($"Fetching OS information from WSL vhdx image ...");

        var wslImagePath = Path.Combine(distroPath, "ext4.vhdx");

        try
        {

            var wslImageHelper = new WslImageHelper(wslImagePath);
            var fileContent = wslImageHelper.ReadFile(osInfosFilePath);
            var osInfos = Regex.Match(fileContent, osInfosPattern)
                .Groups[2].Value;

            return osInfos;
        }
        catch (FileNotFoundException ex)
        {
            Log.Error($"Didn't find /usr/lib/os-release - Caused by exception : {ex}");
            throw;
        }
        catch (IOException ex)
        {
            Log.Error($"Another process is already reading ext4.vhdx - Caused by exception : {ex}");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed fetch OS information from WSL vhdx image - Caused by exception {ex}");
            throw;
        }
    }

    private static string GetOsInfosFromFileSystem(string distroName, string osInfosPattern)
    {
        Log.Information($"Fetching OS information from os-release file of {distroName} FS ...");

        var osInfosFilePath = Path.Combine(WSL_UNC_PATH, distroName, "etc", "os-release");

        try
        {
            var osInfosFile = new FileInfo(osInfosFilePath);
            if (osInfosFile.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                // we cannot read a symlink, so we use the fallback os-release file located at /usr/lib/os-release
                Log.Warning("/etc/os-release is a symbolic link to /usr/lib/os-release");
                osInfosFilePath = Path.Combine(WSL_UNC_PATH, distroName, "usr", "lib", "os-release");
            }

            // using var streamReader = new StreamReader(osInfosFilePath);
            var content = File.ReadAllText(osInfosFilePath);
            var osInfos = Regex.Match(content, osInfosPattern).Groups[2].Value;

            return (string.IsNullOrEmpty(osInfos) ? "Unknown" : osInfos);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to fetch os infos from os-release file - Caused by exception : {ex} ");
            return "Unknown";
        }
    }

    public string GetSize(string distroPath)
    {
        Log.Information("Getting distribution size from wsl vhdx image ...");

        try
        {
            var diskLocation = Path.Combine(distroPath, "ext4.vhdx");
            var diskFile = new FileInfo(diskLocation);
            var sizeInGB = (decimal)diskFile.Length / 1024 / 1024 / 1024;

            return Math.Round(sizeInGB, 2).ToString(CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to get distribution size from wsl vhdx image - Caused by exception : {ex} ");
            return "0";
        }
    }

    public List<string> GetDistributionUsers(string distroName, string distroPath)
    {
        Log.Information("Getting distribution's users list ...");

        const string userShellPattern = @"/bin/(.*?)sh$";
        var usersList = new List<string>();

        try
        {
            usersList = GetUsersFromExt4(distroPath, userShellPattern);

        }
        catch (IOException ex)
        {
            Log.Error($"Failed to get distro users from ext4.vhdx image file - Caused by exception : {ex}");
            usersList = GetUsersFromFileSystem(distroName, userShellPattern);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to get distro users from file system - Caused by exception : {ex}");
            usersList.Add("Unknown");
        }

        return usersList;
    }

    private static List<string> GetUsersFromExt4(string distroPath, string userShellPattern)
    {
        Log.Information("Getting distribution's users list from wsl vhdx image ...");

        var passwdFilePath = Path.Combine("etc", "passwd");
        var wslImagePath = Path.Combine(distroPath, "ext4.vhdx");

        try
        {
            var wslImageHelper = new WslImageHelper(wslImagePath);
            var fileContent = wslImageHelper.ReadFile(passwdFilePath);
            var lines = fileContent.Split('\n');

            var users = lines
                .Where(line => Regex.Match(line, userShellPattern).Success)
                .Select(line => line.Split(':')[0])
                .ToList();

            return users;
        }
        catch (IOException ex)
        {
            Log.Error($"Cannot read ext4.vhdx image file - Caused by exception : {ex}");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error($"Cannot get list of users from /etc/passwd file - Caused by exception : {ex}");
            return new List<string>() { "Unknown" };
        }
    }

    private static List<string> GetUsersFromFileSystem(string distroName, string userShellPattern)
    {
        Log.Information("Getting distribution's users list from distro FS ...");

        var passwdFilePath = Path.Combine(WSL_UNC_PATH, distroName, "etc", "passwd");

        try
        {
            using var streamReader = new StreamReader(passwdFilePath);
            var users = streamReader.ReadToEnd()
                .Split("\n")
                .Where(line => Regex.Match(line, userShellPattern).Success)
                .Select(line => line.Split(':')[0])
                .ToList();

            return users;
        }
        catch (Exception ex)
        {
            Log.Error($"Cannot get list of users from /etc/passwd file : {ex}");
            return new List<string>() { "Unknown" };
        }
    }
}