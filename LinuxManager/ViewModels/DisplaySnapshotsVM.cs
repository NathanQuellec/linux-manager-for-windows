using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using LinuxManager.Contracts.Services;
using LinuxManager.Models;
using CommunityToolkit.WinUI.UI;

namespace LinuxManager.ViewModels;

/// <summary>
/// ViewModel for snapshot display and related actions.
/// </summary>
public class DisplaySnapshotsVM : ObservableObject
{
    private readonly ISnapshotService _snapshotService;
    private readonly DistrosListDetailsVM _distrosViewModel;

    public RelayCommand<Snapshot> DeleteSnapshotCommand { get; set; }

    public DisplaySnapshotsVM(ISnapshotService snapshotService)
    {
        _snapshotService = snapshotService;
        _distrosViewModel = App.GetService<DistrosListDetailsVM>();
        DeleteSnapshotCommand = new RelayCommand<Snapshot>(DeleteSnapshotViewModel);
    }

    public void DeleteSnapshotViewModel(Snapshot snapshot)
    {
        try
        {
            Log.Information($"Deleting snapshot metadata for {snapshot.Name}");
            _snapshotService.DeleteSnapshotInfosRecord(snapshot);
            Log.Information("Deleting snapshot file");
            File.Delete(snapshot.Path);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed deleting snapshot {snapshot.Name}: {ex}");
        }
    }

    /// <summary>
    /// Create a new distribution from a snapshot (dialog primary action).
    /// </summary>
    public async void CreateDistroFromSnapshot(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        try
        {
            App.IsDistributionProcessing = true;
            var distroNameInput = (sender.Content as StackPanel)?.FindChild("DistroNameInput") as TextBox;
            var snapshot = sender.DataContext as Snapshot;
            _distrosViewModel.ValidateDistributionName(sender, args);
            await _distrosViewModel.CreateDistributionViewModel(distroNameInput!.Text, snapshot!.Type, snapshot!.Path);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed creating distro from snapshot: {ex}");
            args.Cancel = true;
        }
        finally
        {
            App.IsDistributionProcessing = false;
        }
    }
}