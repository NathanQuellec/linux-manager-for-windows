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
