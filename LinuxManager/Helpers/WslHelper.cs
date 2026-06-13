using System.ComponentModel;
using Community.Wsl.Sdk;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using LinuxManager.Exceptions;
using LinuxManager.Messages;

namespace LinuxManager.Helpers;

public static class WslHelper
{
    private static readonly WslApi _wslApi = new();

    public static bool CheckWsl()
    {
        return _wslApi.IsWslSupported() && _wslApi.IsInstalled;
    }

    public static bool CheckHypervisor()
    {
        var process = ProcessFactory.Create(ProcessType.ReadOutputAndError, "powershell.exe",
            "/c (Get-WmiObject -Class \"Win32_ComputerSystem\" -ComputerName \"localhost\").HypervisorPresent");
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        var virtualizationEnabled = bool.Parse(output);

        return virtualizationEnabled;
    }
    /**
     * Check if WSL is installed from the microsoft store
     */

    public static async Task<bool> CheckWslMicrosoftStore()
    {
        var process = ProcessFactory.Create(ProcessType.ReadOutputAndError, "powershell.exe",
            "/c  winget ls  -q 'WindowsSubsystemForLinux'");
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();

        return output.Contains("Linux");
    }

    public static async void InstallWslFromMicrosoftStore()
    {
        var process = ProcessFactory.Create(ProcessType.Elevated, "powershell.exe",
            "/c  winget install 'Windows Subsystem for Linux'");
        process.Start();
        await process.WaitForExitAsync();
    }

    /**
     * Used to create snapshots by exporting the file system to an archive file
     */
    public static async Task ExportDistribution(string distroName, string destPath)
    {
        var process = ProcessFactory.Create(ProcessType.ReadOutput, "cmd.exe",
            $"/c wsl --export {distroName} {destPath}");
        process.Start();

        await process.WaitForExitAsync();
    }

    public static async Task ImportDistribution(string distroName, string installDir, string tarLocation)
    {
        
        Log.Information("Importing distribution ...");
        WeakReferenceMessenger.Default.Send(new DistroProgressBarMessage("Importing your distribution ..."));
        try
        {
            var process = ProcessFactory.Create(ProcessType.ReadOutputAndError, "cmd.exe",
                $"/c md {installDir} & wsl --import {distroName} {installDir} {tarLocation}");

            process.Start();
            await process.WaitForExitAsync();

            var isDistroImported = await CheckExistingDistribution(distroName);

            if (!isDistroImported)
            {
                throw new ImportDistributionException("Failed to import distribution");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to import distribution - Caused by exception {ex}");
            throw;
        }
    }
    /**
     * Import distribution from vhdx based snapshot 
     */
    public static async Task ImportInPlaceDistribution(string distroName, string installDir, string vhdxFilePath)
    {
        
        Log.Information("Importing distribution ...");
        WeakReferenceMessenger.Default.Send(new DistroProgressBarMessage("Importing your distribution ..."));
        try
        {
            var process = ProcessFactory.Create(ProcessType.ReadOutputAndError, "cmd.exe",
                $"/c md {installDir} & wsl --import-in-place {distroName} {vhdxFilePath}");

            process.Start();
            await process.WaitForExitAsync();

            var isDistroImported = await CheckExistingDistribution(distroName);

            if (!isDistroImported)
            {
                throw new ImportDistributionException("Failed to import distribution");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to import distribution - Caused by exception {ex}");
            throw;
        }
    }

    public static async Task<bool> CheckRunningDistribution(string distroName)
    {
        Log.Information($"Check running distribution for {distroName}");
        try
        {
            var process = ProcessFactory.Create(ProcessType.ReadOutput, "cmd.exe",
                "/c wsl --list --running --quiet");
            process.Start();

            var output = process.StandardOutput.ReadToEndAsync().GetAwaiter().GetResult();
            await process.WaitForExitAsync();
            var sanitizedOutput = output.Replace("\0", "").Replace("\r", "");  // remove special character
            var runningDistros = sanitizedOutput.Split("\n");

            return runningDistros.Contains(distroName);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to start process to check running distribution - Caused by exception : {ex}");
            return false;
        }
    }

    public static async Task<bool> CheckExistingDistribution(string distroName)
    {
        Log.Information($"Check existing distribution for {distroName}");
        try
        {
            var process = ProcessFactory.Create(ProcessType.ReadOutput, "cmd.exe",
                "/c wsl --list --quiet");
            process.Start();

            var output = process.StandardOutput.ReadToEndAsync().GetAwaiter().GetResult();
            await process.WaitForExitAsync();
            var sanitizedOutput = output.Replace("\0", "").Replace("\r", "");  // remove special character
            var existingDistros = sanitizedOutput.Split("\n");

            return existingDistros.Contains(distroName);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to start process to check existing distribution - Caused by exception : {ex}");
            return false;
        }
    }

    public static async Task TerminateDistribution(string distroName)
    {
        Log.Information($"Terminating distribution {distroName} ...");

        try
        {
            var process = ProcessFactory.Create(ProcessType.Background, "cmd.exe",
                $"/c wsl --terminate {distroName}");
            process.Start();
            await process.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to start process to terminate distribution - Caused by exception {ex}");
        }
    }

    public static async Task ShutdownWsl()
    {
        Log.Information($"Shutdown WSL ...");

        try
        {
            var process = ProcessFactory.Create(ProcessType.Background, "cmd.exe",
                "/c wsl --shutdown");
            process.Start();
            await process.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to shutdown WSL - Caused by exception {ex}");
        }
    }
}
