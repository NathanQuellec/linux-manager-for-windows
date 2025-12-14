using System.Globalization;
using Community.Wsl.Sdk;
using Microsoft.Win32;
using Serilog;
using LinuxManager.Contracts.Services;
using LinuxManager.Contracts.Services.Factories;
using LinuxManager.Helpers;
using LinuxManager.Models;
using LinuxManager.Services.Factories;


namespace LinuxManager.Services;

public class DistributionService : IDistributionService
{
    private const string WSL_UNC_PATH = @"\\wsl$"; // Root UNC path for WSL file systems

    private readonly IList<Distribution> _distros;
    private readonly WslApi _wslApi;
    private readonly IDistributionInfosService _distroInfosService;
    private readonly ISnapshotService _snapshotService;

    public DistributionService(ISnapshotService snapshotService, IDistributionInfosService distroInfosService)
    {
        _distros = new List<Distribution>();
        _wslApi = new WslApi();
        _distroInfosService = distroInfosService;
        _snapshotService = snapshotService;
    }

    public void InitDistributionsList()
    {
        Log.Information("Loading distributions from registry");
        try
        {
            var lxssRegPath = Path.Combine("SOFTWARE", "Microsoft", "Windows", "CurrentVersion", "Lxss");
            var lxssSubKeys = Registry.CurrentUser.OpenSubKey(lxssRegPath);

            foreach (var subKey in lxssSubKeys.GetSubKeyNames())
            {
                // Skip non-guid keys
                if (!subKey.StartsWith('{') || !subKey.EndsWith('}'))
                {
                    continue;
                }

                var distroRegPath = Path.Combine(lxssRegPath, subKey);
                var distroSubkeys = Registry.CurrentUser.OpenSubKey(distroRegPath);
                var distroName = (string)distroSubkeys.GetValue("DistributionName");

                // Filter Docker internal distros
                if (distroName != "docker-desktop" && distroName != "docker-desktop-data")
                {
                    var distroPath = (string)distroSubkeys.GetValue("BasePath");
                    var wslVersion = (int)distroSubkeys.GetValue("Version");

                    var distro = new DistributionBuilder()
                        .WithId(Guid.Parse(subKey))
                        .WithName(distroName)
                        .WithPath(distroPath)
                        .WithWslVersion(wslVersion)
                        .WithOsName(_distroInfosService.GetOsInfos(distroName, distroPath, "NAME"))
                        .WithOsVersion(_distroInfosService.GetOsInfos(distroName, distroPath, "VERSION"))
                        .WithDiskUsageInfo(_distroInfosService.GetDistributionDiskUsageInfo(distroName))
                        .WithUsers(_distroInfosService.GetDistributionUsers(distroName, distroPath))
                        .WithSnapshots(_snapshotService.GetDistributionSnapshots(distroPath))
                        .Build();

                    distro.SnapshotsTotalSize = distro.Snapshots
                        .Sum(snapshot => decimal.Parse(snapshot.Size, CultureInfo.InvariantCulture))
                        .ToString(CultureInfo.InvariantCulture);

                    _distros.Add(distro);
                }
                distroSubkeys.Close();
            }
            lxssSubKeys.Close();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to load distributions from registry: {ex}");
        }
    }

    public IEnumerable<Distribution> GetAllDistributions() => _distros;

    public async Task<Distribution?> CreateDistribution(string distroName, string creationMode, string resourceOrigin)
    {
        var distroFolder = Path.Combine(App.DistroDirPath, distroName);
        try
        {
            AbstractDistributionFactory factory = creationMode switch
            {
                "Dockerfile" => new DockerfileDistributionFactory(),
                "Archive" => new ArchiveDistributionFactory(),
                "Docker Hub" => new DockerHubDistributionFactory(),
                "Vhdx" => new VhdxDistributionFactory(),
                _ => throw new NullReferenceException("Unsupported creation mode"),
            };

            var newDistro = await factory.CreateDistribution(distroName, resourceOrigin, distroFolder);

            // Populate additional metadata via WSL API
            var distro = _wslApi
                .GetDistributionList()
                .FirstOrDefault(d => d.DistroName == newDistro.Name);

            await WslHelper.TerminateDistribution(distroName); // required to access ext4.vhdx
            newDistro.Id = distro.DistroId;
            newDistro.Path = distro.BasePath;
            newDistro.WslVersion = distro.WslVersion;
            newDistro.OsName = _distroInfosService.GetOsInfos(newDistro.Name, newDistro.Path, "NAME");
            newDistro.OsVersion = _distroInfosService.GetOsInfos(newDistro.Name, newDistro.Path, "VERSION");
            newDistro.DiskUsageInfo = _distroInfosService.GetDistributionDiskUsageInfo(newDistro.Name);
            newDistro.Users = _distroInfosService.GetDistributionUsers(newDistro.Name, newDistro.Path);

            _distros.Add(newDistro);
            return newDistro;
        }
        catch (Exception ex)
        {
            Log.Error($"Error creating distribution: {ex}");
            throw;
        }
    }

    public async Task RemoveDistribution(Distribution distribution)
    {
        var process = new ProcessBuilder("cmd.exe")
            .SetArguments($"/c wsl --unregister {distribution.Name}")
            .SetCreateNoWindow(true)
            .Build();
        process.Start();
        await process.WaitForExitAsync();

        if (process.HasExited)
        {
            _distros.Remove(distribution);
            RemoveDistributionFolder(distribution);
            Log.Information($"Distribution removed: {distribution.Name}");
        }
    }

    private static void RemoveDistributionFolder(Distribution distribution)
    {
        var distroPath = Directory.GetParent(distribution.Path).FullName;
        if (Directory.Exists(distroPath))
        {
            Directory.Delete(distroPath, true);
        }
    }

    [Obsolete("Feature removed")] // Kept for backward compatibility
    public async Task<bool> RenameDistribution(Distribution distribution, string newDistroName)
    {
        Log.Information($"Renaming distribution in registry: {distribution.Name}");
        var lxssRegPath = Path.Combine("SOFTWARE", "Microsoft", "Windows", "CurrentVersion", "Lxss");
        try
        {
            using var lxsSubKeys = Registry.CurrentUser.OpenSubKey(lxssRegPath);
            foreach (var subKey in lxsSubKeys.GetSubKeyNames())
            {
                if (subKey != $"{{{distribution.Id}}}")
                {
                    continue;
                }

                var distroRegPath = Path.Combine(lxssRegPath, subKey);
                var distroSubkeys = Registry.CurrentUser.OpenSubKey(distroRegPath, true);

                var oldDistroName = distribution.Name;
                await WslHelper.TerminateDistribution(newDistroName);
                var isFolderRenamed = RenameDistributionFolder(oldDistroName, newDistroName);

                if (isFolderRenamed)
                {
                    var newDistroPath = distribution.Path.Replace(oldDistroName, newDistroName);
                    distroSubkeys.SetValue("DistributionName", newDistroName);
                    distroSubkeys.SetValue("BasePath", newDistroPath);
                    distribution.Name = newDistroName;
                    distribution.Path = newDistroPath;
                }

                distroSubkeys.Close();
                return true;
            }
            lxsSubKeys.Close();
            return false;
        }
        catch (Exception ex)
        {
            Log.Error("Failed to rename distribution in registry");
            return false;
        }
    }

    [Obsolete("Feature removed")] // Folder rename helper
    private static bool RenameDistributionFolder(string oldDistroName, string newDistroName)
    {
        var oldDistroPath = Path.Combine(App.AppDirPath, oldDistroName);
        var newDistroPath = Path.Combine(App.AppDirPath, newDistroName);
        try
        {
            if (!Directory.Exists(oldDistroPath))
            {
                Log.Information("Source directory does not exist.");
                throw new DirectoryNotFoundException();
            }
            File.Copy(oldDistroPath, newDistroPath); // ensure content copied before rename
            Directory.Move(oldDistroPath, newDistroPath);
            Log.Information("Directory renamed successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Error renaming directory: " + ex.Message);
            return false;
        }
    }

    public void LaunchDistribution(Distribution distribution)
    {
        try
        {
            var process = new ProcessBuilder("cmd.exe")
                .SetArguments($"/c wsl ~ -d {distribution?.Name}")
                .SetRedirectStandardOutput(false)
                .SetUseShellExecute(true)
                .SetCreateNoWindow(true)
                .Build();
            process.Start();
            Log.Information($"Launch process started (PID={process.Id}) for {distribution.Name}");
            distribution?.RunningProcesses.Add(process);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to launch distribution {distribution.Name}: {ex}");
        }
    }

    // Start distribution silently to guarantee file system availability (WSL issue #5307 workaround)
    private static void BackgroundLaunchDistribution(Distribution distribution)
    {
        Log.Information($"Background start for {distribution.Name}");
        try
        {
            var process = new ProcessBuilder("cmd.exe")
                .SetArguments($"/c wsl -d {distribution?.Name}")
                .SetCreateNoWindow(true)
                .SetUseShellExecute(false)
                .Build();
            process.Start();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed background start: {ex}");
        }
    }

    public async void StopDistribution(Distribution distribution)
    {
        if (distribution.RunningProcesses.Count == 0)
        {
            Log.Warning($"No processes to stop for {distribution.Name}");
            return;
        }

        foreach (var process in distribution.RunningProcesses)
        {
            process.CloseMainWindow();
            await process.WaitForExitAsync();
            if (process.HasExited)
            {
                Log.Information($"Process exited (PID={process.Id})");
            }
            else
            {
                process.Kill();
            }
        }
        distribution.RunningProcesses.Clear();
    }

    public async void OpenDistributionFileSystem(Distribution distribution)
    {
        var distroFileSystem = Path.Combine(WSL_UNC_PATH, distribution.Name);
        try
        {
            var distroIsRunning = await WslHelper.CheckRunningDistribution(distribution.Name);
            if (!distroIsRunning)
            {
                BackgroundLaunchDistribution(distribution);
            }
            var processBuilder = new ProcessBuilder("explorer.exe")
                .SetArguments(distroFileSystem)
                .Build();
            processBuilder.Start();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to open file system for {distribution.Name}: {ex}");
        }
    }

    public void OpenDistributionWithVsCode(Distribution distribution)
    {
        var process = new ProcessBuilder("cmd.exe")
            .SetArguments($"/c wsl ~ -d {distribution.Name} code .")
            .Build();
        process.Start();
    }

    public void OpenDistroWithWinTerm(Distribution distribution)
    {
        try
        {
            var process = new ProcessBuilder("cmd.exe")
                .SetArguments($"/c wt wsl ~ -d {distribution?.Name}")
                .SetRedirectStandardOutput(false)
                .SetUseShellExecute(false)
                .SetCreateNoWindow(true)
                .Build();
            process.Start();
            Log.Information($"Windows Terminal started (PID={process.Id}) for {distribution.Name}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to open Windows Terminal for {distribution.Name}: {ex}");
        }
    }

    public void OpenDistroInstallationLocation(Distribution distribution)
    {
        try
        {
            var target = distribution.Path;
            if (File.Exists(target))
            {
                target = Directory.GetParent(target)?.FullName ?? target; // Show containing folder if path is a file
            }
            if (!Directory.Exists(target))
            {
                Log.Warning($"Installation location not found for {distribution.Name}: {target}");
                return;
            }
            var processBuilder = new ProcessBuilder("explorer.exe")
                .SetArguments(target)
                .Build();
            processBuilder.Start();
            Log.Information($"Opening installation location: {target}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to open installation location for {distribution.Name}: {ex}");
        }
    }
}