using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using LinuxManager.Messages;

namespace LinuxManager.Views.UserControls;
public sealed partial class WslProcessInfoBar : UserControl
{
    public WslProcessInfoBar()
    {
        this.InitializeComponent();

        Log.Information("[PUB/SUB] Message received to update progress bar advancement status");

        WeakReferenceMessenger.Default.Register<DistroProgressBarMessage>(this, (recipient, message) =>
        {
            Log.Information("");
            CreateDistroInfoProgress.Title = message.ProgressInfo;
        });

        WeakReferenceMessenger.Default.Register<SnapshotProgressBarMessage>(this, (recipient, message) =>
        {
            Log.Information("");
            CreateSnapshotInfoProgress.Title = message.ProgressInfo;
        });
    }
}
