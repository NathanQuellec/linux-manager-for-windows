using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using LinuxManager.Contracts.Services;
using LinuxManager.Models;
using LinuxManager.ViewModels; // for DistrosListDetailsVM reference
using CommunityToolkit.WinUI.UI;

namespace LinuxManager.ViewModels;

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
            Log.Information("Deleting snapshot record");
            _snapshotService.DeleteSnapshotInfosRecord(snapshot);
            Log.Information("Deleting snapshot file");
            File.Delete(snapshot.Path);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to delete snapshot {snapshot.Name} - Caused by {ex}");
        }
    }

    public async void CreateDistroFromSnapshot(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        try
        {
            App.IsDistributionProcessing = true;
            var distroNameInput = (sender.Content as StackPanel)?.FindChild("DistroNameInput") as TextBox;
            var snapshot = sender.DataContext as Snapshot;
            _distrosViewModel.ValidateDistributionName(sender, args);
            await _distrosViewModel.CreateDistributionViewModel(distroNameInput!.Text, snapshot!.Type, snapshot!.Path);
            App.IsDistributionProcessing = false;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to create distribution from snapshot - Caused by {ex}");
            App.IsDistributionProcessing = false;
            args.Cancel = true;
        }
    }
}