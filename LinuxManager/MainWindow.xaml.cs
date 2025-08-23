using LinuxManager.Helpers;

namespace LinuxManager;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        Content = null;
        ExtendsContentIntoTitleBar = true;
    }
}
