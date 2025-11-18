// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.


using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Serilog;
using LinuxManager.Helpers;
using LinuxManager.Messages;
using LinuxManager.ViewModels;
using System.Collections.Specialized;
using LinuxManager.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LinuxManager.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DistrosListDetailsView : Page
{
    private Button _distroStopButton = new();
    private Button _distroStartButton = new();

    private StackPanel _distroRunningStatusPanel = new();
    private StackPanel _distroStoppedStatusPanel = new();

    // Track current distribution and snapshot collection handler
    private Distribution? _currentDistributionForSnapshots;
    private NotifyCollectionChangedEventHandler? _snapshotsChangedHandler;

    public DistrosListDetailsVM ViewModel
    {
        get;
    }

    public DistrosListDetailsView()
    {
        this.InitializeComponent();

        this.ViewModel = App.GetService<DistrosListDetailsVM>();
        App.MainWindow.SetTitleBar(AppTitleBar);
        TitleBarHelper.UpdateTitleBar(ElementTheme.Default);

        // Subscribe to selection changed so we can control the snapshot area visibility
        MasterListView.SelectionChanged += MasterListView_SelectionChanged;

        WeakReferenceMessenger.Default.Register<ShowDistroStopButtonMessage>(this, (recipient, message) =>
        {
            Log.Information("[PUB/SUB] Message received to show distribution 'stop' button");
            var distro = message.Distribution;
            FindDistroButton(this, distro.Name);
            _distroStartButton.Visibility = Visibility.Collapsed;
            _distroStopButton.Visibility = Visibility.Visible;

            FindDistroStatusPanel(this, distro.Name);
            _distroRunningStatusPanel.Visibility = Visibility.Visible;
            _distroStoppedStatusPanel.Visibility = Visibility.Collapsed;
        });

        WeakReferenceMessenger.Default.Register<HideDistroStopButtonMessage>(this, (recipient, message) =>
        {
            Log.Information("[PUB/SUB] Message received to hide distribution 'stop' button");
            var distro = message.Distribution;
            FindDistroButton(this, distro.Name);
            _distroStopButton.Visibility = Visibility.Collapsed;
            _distroStartButton.Visibility = Visibility.Visible;

            FindDistroStatusPanel(this, distro.Name);
            _distroRunningStatusPanel.Visibility = Visibility.Collapsed;
            _distroStoppedStatusPanel.Visibility = Visibility.Visible;
        });

        //Close InfoBar after timer set in DistroListDetailsViewModel.cs
        WeakReferenceMessenger.Default.Register<CloseInfoBarMessage>(this, (recipient, message) =>
        {
            Log.Information("[PUB/SUB] Message received to close infobar");
            var infoBar = message.InfoBar;
            DispatcherQueue.TryEnqueue(() => infoBar.IsOpen = false);
        });
    }

    private void MasterListView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var distribution = MasterListView.SelectedItem as Distribution;
        UpdateSnapshotsForDistribution(distribution);
    }

    private void UpdateSnapshotsForDistribution(Distribution? distribution)
    {
        // Unsubscribe previous handler
        if (_currentDistributionForSnapshots != null && _snapshotsChangedHandler != null)
        {
            _currentDistributionForSnapshots.Snapshots.CollectionChanged -= _snapshotsChangedHandler;
            _snapshotsChangedHandler = null;
        }

        _currentDistributionForSnapshots = distribution;

        if (distribution != null)
        {
            // subscribe to changes to update UI when snapshots are added/removed
            _snapshotsChangedHandler = (s, args) => DispatcherQueue.TryEnqueue(() => UpdateSnapshotsVisibility());
            distribution.Snapshots.CollectionChanged += _snapshotsChangedHandler;
        }

        // Ensure UI is updated immediately
        DispatcherQueue.TryEnqueue(() => UpdateSnapshotsVisibility());
    }

    private void UpdateSnapshotsVisibility()
    {
        // Root of the instantiated template content
        var root = DetailContent.ContentTemplateRoot ?? (DependencyObject)DetailContent;

        var snapshotsList = FindChildByName<ListView>(root, "SnapshotsListView");
        var emptyPanel = FindChildByName<StackPanel>(root, "SnapshotsEmptyStatePanel");

        if (snapshotsList == null || emptyPanel == null)
        {
            return;
        }

        var count = _currentDistributionForSnapshots?.Snapshots?.Count ?? 0;

        snapshotsList.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        emptyPanel.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    // Recursive helper to find named elements in the visual tree
    private T? FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        if (parent == null) return null;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is FrameworkElement fe)
            {
                if (fe.Name == name && fe is T matched)
                {
                    return matched;
                }
            }

            var result = FindChildByName<T>(child, name);
            if (result != null) return result;
        }

        return null;
    }

    /*
     * We need to go through the Visual Tree recursively to find the Tag that matches the Distro Name received,
     * as we cannot set a dynamic x:Name property for the Stop button.
     */
    private void FindDistroButton(DependencyObject parent, string searchDistroName)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var currentChild = VisualTreeHelper.GetChild(parent, i);
            if (currentChild != null && currentChild is Button stopButton && 
                (string)stopButton.Tag == $"STOP_{searchDistroName}")
            {
                _distroStopButton = stopButton;
            }
            else if (currentChild != null && currentChild is Button startButton &&
                     (string)startButton.Tag == $"START_{searchDistroName}")
            {
                _distroStartButton = startButton;
            } 
            FindDistroButton(currentChild, searchDistroName);
        }
    }


    private void FindDistroStatusPanel(DependencyObject parent, string searchDistroName)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var currentChild = VisualTreeHelper.GetChild(parent, i);
            if (currentChild != null && currentChild is StackPanel runningPanel &&
                (string)runningPanel.Tag == $"RUNNING_{searchDistroName}")
            {
                _distroRunningStatusPanel = runningPanel;
            }
            else if (currentChild != null && currentChild is StackPanel stoppedPanel &&
                     (string)stoppedPanel.Tag == $"STOPPED_{searchDistroName}")
            {
                _distroStoppedStatusPanel = stoppedPanel;
            }
            FindDistroStatusPanel(currentChild, searchDistroName);
        }
    }
}
