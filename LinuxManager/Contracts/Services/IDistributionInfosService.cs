using LinuxManager.Models;

namespace LinuxManager.Contracts.Services;

public interface IDistributionInfosService
{
    string GetOsInfos(string distroName, string distroPath, string field);
    List<string> GetDistributionUsers(string distroName, string distroPath);
    DiskUsageInfo GetDistributionDiskUsageInfo(string distroName);
}