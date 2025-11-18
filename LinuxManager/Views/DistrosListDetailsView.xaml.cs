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

namespace LinuxManager.Views;
public sealed partial class DistrosListDetailsView : Page
{
    private Button _distroStopButton = new();
    private Button _distroStartButton = new();

    private StackPanel _distroRunningStatusPanel = new();
    private StackPanel _distroStoppedStatusPanel = new();

    public DistrosListDetailsVM ViewModel { get; }

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
            ResolveDistroControls(distro.Name);
            _distroStartButton.Visibility = Visibility.Collapsed;
            _distroStopButton.Visibility = Visibility.Visible;
            _distroRunningStatusPanel.Visibility = Visibility.Visible;
            _distroStoppedStatusPanel.Visibility = Visibility.Collapsed;
        });

        WeakReferenceMessenger.Default.Register<HideDistroStopButtonMessage>(this, (recipient, message) =>
        {
            Log.Information("[PUB/SUB] Message received to hide distribution 'stop' button");
            var distro = message.Distribution;
            ResolveDistroControls(distro.Name);
            _distroStopButton.Visibility = Visibility.Collapsed;
            _distroStartButton.Visibility = Visibility.Visible;
            _distroRunningStatusPanel.Visibility = Visibility.Collapsed;
            _distroStoppedStatusPanel.Visibility = Visibility.Visible;
        });

        WeakReferenceMessenger.Default.Register<CloseInfoBarMessage>(this, (recipient, message) =>
        {
            Log.Information("[PUB/SUB] Message received to close infobar");
            var infoBar = message.InfoBar;
            DispatcherQueue.TryEnqueue(() => infoBar.IsOpen = false);
        });
    }

    private void ResolveDistroControls(string distroName)
    {
        var resolvers = new Dictionary<string, Action<FrameworkElement>>
        {
            [$"STOP_{distroName}"] = fe => _distroStopButton = (Button)fe,
            [$"START_{distroName}"] = fe => _distroStartButton = (Button)fe,
            [$"RUNNING_{distroName}"] = fe => _distroRunningStatusPanel = (StackPanel)fe,
            [$"STOPPED_{distroName}"] = fe => _distroStoppedStatusPanel = (StackPanel)fe
        };

        TraverseVisualTree(this, resolvers);
    }

    private static void TraverseVisualTree(DependencyObject root, IDictionary<string, Action<FrameworkElement>> resolvers)
    {
        if (resolvers.Count == 0)
        {
            return;
        }

        var stack = new Stack<DependencyObject>();
        stack.Push(root);

        while (stack.Count > 0 && resolvers.Count > 0)
        {
            var current = stack.Pop();
            var childCount = VisualTreeHelper.GetChildrenCount(current);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(current, i);
                if (child is FrameworkElement { Tag: string tag } fe && resolvers.TryGetValue(tag, out var apply))
                {
                    apply(fe);
                    resolvers.Remove(tag);
                    if (resolvers.Count == 0)
                    {
                        return; // all found
                    }
                }
                stack.Push(child);
            }
        }
    }
}
