using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using LinuxManager.Models;
using LinuxManager.ViewModels;
using CommunityToolkit.Mvvm.Input;
using LinuxManager.Helpers;
using LinuxManager.Views.Dialogs;
using System.Collections.Specialized;
using Path = System.IO.Path;

namespace LinuxManager.Views.UserControls;
public sealed partial class DisplaySnapshotsView : UserControl
{
    public DisplaySnapshotsVM ViewModel { get; set; }

    private Distribution? _currentDistribution;
    private NotifyCollectionChangedEventHandler? _snapshotsChangedHandler;

    public IAsyncRelayCommand<Distribution>? CreateSnapshotCommand
    {
        get => (IAsyncRelayCommand<Distribution>)GetValue(CreateSnapshotCommandProperty);
        set => SetValue(CreateSnapshotCommandProperty, value);
    }

    public static readonly DependencyProperty CreateSnapshotCommandProperty =
        DependencyProperty.Register(nameof(CreateSnapshotCommand), typeof(IAsyncRelayCommand<Distribution>), typeof(DisplaySnapshotsView), new PropertyMetadata(null));

    public DisplaySnapshotsView()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<DisplaySnapshotsVM>();
        Loaded += DisplaySnapshotsView_Loaded;
        DataContextChanged += DisplaySnapshotsView_DataContextChanged;
    }

    private void DisplaySnapshotsView_Loaded(object sender, RoutedEventArgs e) => UpdateSnapshotsVisibility();

    private void DisplaySnapshotsView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        // Unsubscribe previous
        if (_currentDistribution != null && _snapshotsChangedHandler != null)
        {
            _currentDistribution.Snapshots.CollectionChanged -= _snapshotsChangedHandler;
            _snapshotsChangedHandler = null;
        }

        _currentDistribution = DataContext as Distribution;

        if (_currentDistribution != null)
        {
            _snapshotsChangedHandler = (s, e) => DispatcherQueue.TryEnqueue(UpdateSnapshotsVisibility);
            _currentDistribution.Snapshots.CollectionChanged += _snapshotsChangedHandler;
        }

        UpdateSnapshotsVisibility();
    }

    private void UpdateSnapshotsVisibility()
    {
        var distribution = DataContext as Distribution;
        var emptyPanel = (StackPanel)FindName("SnapshotsEmptyStatePanel");
        var listView = (ListView)FindName("SnapshotsListView");
        if (distribution == null || distribution.Snapshots.Count == 0)
        {
            emptyPanel.Visibility = Visibility.Visible;

            listView.Visibility = Visibility.Collapsed;
        }
        else
        {
            emptyPanel.Visibility = Visibility.Collapsed;

            listView.Visibility = Visibility.Visible;
        }
    }

    private void OpenSnapshotsFolder(object sender, RoutedEventArgs e)
    {
        if (DataContext is not Distribution distribution)
        {
            return;
        }

        var snapshotsFolderPath = Path.Combine(distribution.Path, "snapshots");
        Log.Information("Opening snapshots folder ...");
        try
        {
            var process = ProcessFactory.Create(ProcessType.Default, "explorer.exe", snapshotsFolderPath);
            process.Start();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to open snapshots folder - Caused by exception {ex}");
        }
    }

    private async void OpenDeleteSnapshotDialog(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is not Snapshot snapshot)
        {
            return;
        }

        if (App.IsDistributionProcessing) { App.ShowIsProcessingDialog(); return; }

        var deleteSnapshotDialog = new ContentDialog
        {
            Title = $"Are you sure to delete snapshot \"{snapshot.Name}\" ?",
            XamlRoot = App.MainWindow.Content.XamlRoot,
            DataContext = snapshot,
            PrimaryButtonText = "Yes",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            PrimaryButtonCommand = ViewModel.DeleteSnapshotCommand
        };
        deleteSnapshotDialog.PrimaryButtonCommandParameter = deleteSnapshotDialog.DataContext;

        try
        {
            var buttonClicked = await deleteSnapshotDialog.ShowAsync();
            if (buttonClicked != ContentDialogResult.Primary)
            {
                return;
            }

            var distribution = DataContext as Distribution;
            var toRemove = distribution?.Snapshots.FirstOrDefault(s => s.Id == snapshot.Id);
            if (toRemove != null)
            {
                distribution!.Snapshots.Remove(toRemove);
            }

            UpdateSnapshotsVisibility();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to open snapshot deletion dialog of {snapshot.Name} - Caused by exception {ex}");
        }
    }

    private async void OpenCreateDistroDialog(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is not Snapshot snapshot)
        {
            return;
        }

        if (App.IsDistributionProcessing)
        {
            App.ShowIsProcessingDialog(); return;
        }

        var createDistroDialog = new CreateDistributionView
        {
            Title = $"Create distribution from snapshot \"{snapshot.Name}\":",
            DataContext = snapshot,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };
        createDistroDialog.PrimaryButtonClick += ViewModel.CreateDistroFromSnapshot;

        try
        {
            var dialogStackPanel = createDistroDialog.Content as StackPanel;
            ComboBox? creationMode = null;
            if (dialogStackPanel != null)
            {
                foreach (var child in dialogStackPanel.Children)
                {
                    if (child is not ComboBox { Name: "DistroCreationMode" } cb)
                    {
                        continue;
                    }

                    creationMode = cb;
                    break;
                }
            }
            if (creationMode != null)
            {
                creationMode.Visibility = Visibility.Collapsed;
            }

            await createDistroDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to open distribution creation dialog - Caused by exception : {ex}");
        }
    }
}
