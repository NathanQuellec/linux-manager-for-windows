using LinuxManager.Models;

namespace LinuxManager.Contracts.Services.Factories;

public abstract class AbstractDistributionFactory
{
    public abstract Task<Distribution?> CreateDistribution(string distroName, string resourceOrigin, string targetFolder);
}