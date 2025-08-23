using System.Collections.ObjectModel;
using LinuxManager.Models;

namespace LinuxManager.Contracts.Services;

public interface ISnapshotService
{
    ObservableCollection<Snapshot> GetDistributionSnapshots(string distroPath);
    Task<bool> CreateSnapshot(Distribution distribution, string snapshotName, string snapshotDescr, bool isFastSnapshot);
    void DeleteSnapshotInfosRecord(Snapshot snapshot);
}