using LinuxManager.Models;

namespace LinuxManager.Contracts.Services;

public interface IDistributionService
{
    /// <summary>Initialize internal distribution list from system registry.</summary>
    void InitDistributionsList();
    /// <summary>Return all known distributions.</summary>
    IEnumerable<Distribution> GetAllDistributions();
    /// <summary>Create a new distribution given a name, creation mode and resource path.</summary>
    Task<Distribution?> CreateDistribution(string distroName, string creationMode, string resourceOrigin);
    /// <summary>Unregister and remove the distribution from disk.</summary>
    Task RemoveDistribution(Distribution distribution);
    /// <summary>Rename an existing distribution (obsolete feature kept for backward compatibility).</summary>
    Task<bool> RenameDistribution(Distribution distribution, string newDistroName);
    /// <summary>Launch the default shell inside the distribution.</summary>
    void LaunchDistribution(Distribution distribution);
    /// <summary>Terminate processes started for this distribution.</summary>
    void StopDistribution(Distribution distribution);
    /// <summary>Open the distribution file system (\\wsl$ UNC path) in Explorer.</summary>
    void OpenDistributionFileSystem(Distribution distribution);
    /// <summary>Open VS Code inside the distribution.</summary>
    void OpenDistributionWithVsCode(Distribution distribution);
    /// <summary>Open Windows Terminal inside the distribution.</summary>
    void OpenDistroWithWinTerm(Distribution distribution);
    /// <summary>Open the installation folder containing the distribution's data.</summary>
    void OpenDistroInstallationLocation(Distribution distribution);
}
