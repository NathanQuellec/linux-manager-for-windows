using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Serilog;
using LinuxManager.Contracts.Services;
using LinuxManager.Contracts.Services.UserInterface;
using LinuxManager.Helpers;
using LinuxManager.Messages;
using LinuxManager.Models;
using LinuxManager.Views.Dialogs;

namespace LinuxManager.ViewModels;

public class DistrosListDetailsVM : ObservableObject
{
    private readonly IDistributionService _distributionService;
    private readonly ISnapshotService _snapshotService;
    private readonly IInfoBarService _infoBarService;

    #region RelayCommand
    public AsyncRelayCommand<Distribution> RemoveDistroCommand { get; set; }
    public AsyncRelayCommand<Distribution> RenameDistroCommand { get; set; }
    public RelayCommand<Distribution> LaunchDistroCommand { get; set; }
    public RelayCommand<Distribution> StopDistroCommand { get; set; }
    public RelayCommand<Distribution> OpenDistroWithFileExplorerCommand { get; set; }
    public RelayCommand<Distribution> OpenDistroWithVsCodeCommand { get; set; }
    public RelayCommand<Distribution> OpenDistroWithWinTermCommand { get; set; }
    public AsyncRelayCommand CreateDistroCommand { get; set; }
    public AsyncRelayCommand<Distribution> CreateSnapshotCommand { get; set; }
    public RelayCommand<Distribution> OpenDistroInstallationLocationCommand { get; set; }
    #endregion

    public ObservableCollection<Distribution> Distros { get; set; } = new();

    public DistrosListDetailsVM(IDistributionService distributionService, ISnapshotService snapshotService, IInfoBarService infoBarService)
    {
        _distributionService = distributionService;
        _snapshotService = snapshotService;
        _infoBarService = infoBarService;

        RemoveDistroCommand = new AsyncRelayCommand<Distribution>(RemoveDistributionDialog);
        RenameDistroCommand = new AsyncRelayCommand<Distribution>(RenameDistributionDialog);
        LaunchDistroCommand = new RelayCommand<Distribution>(LaunchDistributionViewModel);
        StopDistroCommand = new RelayCommand<Distribution>(StopDistributionViewModel);
        OpenDistroWithFileExplorerCommand = new RelayCommand<Distribution>(OpenDistributionWithFileExplorerViewModel);
        OpenDistroWithVsCodeCommand = new RelayCommand<Distribution>(OpenDistributionWithVsCodeViewModel);
        OpenDistroWithWinTermCommand = new RelayCommand<Distribution>(OpenDistroWithWinTermViewModel);
        CreateDistroCommand = new AsyncRelayCommand(CreateDistributionDialog);
        CreateSnapshotCommand = new AsyncRelayCommand<Distribution>(CreateSnapshotDialog);
        OpenDistroInstallationLocationCommand = new RelayCommand<Distribution>(OpenDistroInstallationLocationViewModel);

        _distributionService.InitDistributionsList();
        PopulateDistributionsCollection();
    }

    private void PopulateDistributionsCollection()
    {
        Log.Information("Populating distributions collection");
        try
        {
            Distros.Clear();
            foreach (var distro in _distributionService.GetAllDistributions())
            {
                Distros.Add(distro);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to populate distributions: {ex.Message}");
        }
    }

    private async Task RemoveDistributionDialog(Distribution distribution)
    {
        Log.Information($"Confirm removal for {distribution.Name}");
        try
        {
            var dialog = new ContentDialog()
            {
                Title = $"Are you sure to remove \"{distribution.Name}\" ?",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var buttonClicked = await dialog.ShowAsync();
            if (buttonClicked == ContentDialogResult.Primary)
            {
                RemoveDistributionViewModel(distribution);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to open remove dialog: {ex}");
        }
    }

    private void RemoveDistributionViewModel(Distribution distribution)
    {
        Log.Information($"Removing {distribution.Name}");
        _distributionService.RemoveDistribution(distribution);
        Distros.Remove(distribution);

        if (!Distros.Contains(distribution))
        {
            var infoBar = _infoBarService.FindInfoBar("RemoveDistroInfoSuccess");
            _infoBarService.OpenInfoBar(infoBar, 2000);
            Log.Information($"Removed {distribution.Name}");
        }
        else
        {
            Log.Warning($"Removal failed: {distribution.Name} still present");
        }
    }

    private async Task RenameDistributionDialog(Distribution distribution)
    {
        Log.Information($"Rename dialog for {distribution.Name}");
        try
        {
            var dialog = new RenameDistributionView()
            {
                Title = $"Rename \"{distribution.Name}\" :",
                XamlRoot = App.MainWindow.Content.XamlRoot,
            };

            dialog.PrimaryButtonClick += ValidateRenameDistribution;
            var buttonClicked = await dialog.ShowAsync();
            if (buttonClicked == ContentDialogResult.Primary)
            {
                var newDistroNameInput = (dialog.Content as StackPanel)!.FindChild("DistroNameInput") as TextBox;
                RenameDistributionViewModel(distribution, newDistroNameInput!.Text);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to open rename dialog: {ex}");
        }
    }

    private void ValidateRenameDistribution(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        try
        {
            ValidateDistributionName(sender, args);
            Log.Information("Rename validation succeeded");
        }
        catch (Exception ex)
        {
            Log.Error($"Rename validation failed: {ex}");
        }
    }

    internal void ValidateDistributionName(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var distroNameInput = sender.FindChild("DistroNameInput") as TextBox;
        distroNameInput!.ClearValue(Control.BorderBrushProperty);
        var errorInfoBar = sender.FindChild("DistroNameErrorInfoBar") as InfoBar;
        errorInfoBar!.IsOpen = false;

        var distroNamesList = Distros.Select(distro => distro.Name).ToList();
        var regex = new Regex("^[a-zA-Z0-9-_ ]*$");
        const int minLength = 2;
        try
        {
            var validator = new TextInputValidation(distroNameInput.Text);
            validator.NotNullOrWhiteSpace().IncludeWhiteSpaceChar().MinimumLength(minLength).InvalidCharacters(regex, "special characters").DataAlreadyExist(distroNamesList);
        }
        catch (ArgumentException e)
        {
            args.Cancel = true;
            errorInfoBar.Message = e.Message;
            errorInfoBar.IsOpen = true;
            distroNameInput.BorderBrush = new SolidColorBrush(Colors.DarkRed);
            throw;
        }
    }

    private async Task RenameDistributionViewModel(Distribution distribution, string newDistroName)
    {
        Log.Information($"Renaming {distribution.Name} -> {newDistroName}");
        try
        {
            var isRenamed = await _distributionService.RenameDistribution(distribution, newDistroName);
            if (!isRenamed) return;
            var index = Distros.ToList().FindIndex(d => d.Name == distribution.Name);
            if (index != -1)
            {
                Distros.ElementAt(index).Name = newDistroName;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Rename failed: {ex}");
        }
    }

    private void LaunchDistributionViewModel(Distribution distribution)
    {
        Log.Information($"Launching {distribution!.Name}");
        _distributionService.LaunchDistribution(distribution);
        WeakReferenceMessenger.Default.Send(new ShowDistroStopButtonMessage(distribution));
    }

    private void StopDistributionViewModel(Distribution distribution)
    {
        Log.Information($"Stopping {distribution!.Name}");
        _distributionService.StopDistribution(distribution);
        WeakReferenceMessenger.Default.Send(new HideDistroStopButtonMessage(distribution));
    }

    private void OpenDistributionWithFileExplorerViewModel(Distribution distribution)
    {
        Log.Information($"Opening WSL file system for {distribution!.Name}");
        _distributionService.OpenDistributionFileSystem(distribution);
    }

    private void OpenDistributionWithVsCodeViewModel(Distribution distribution)
    {
        Log.Information($"Opening VS Code in {distribution.Name}");
        _distributionService.OpenDistributionWithVsCode(distribution);
    }

    private void OpenDistroWithWinTermViewModel(Distribution distribution)
    {
        Log.Information($"Opening Windows Terminal in {distribution.Name}");
        _distributionService.OpenDistroWithWinTerm(distribution);
    }

    private void OpenDistroInstallationLocationViewModel(Distribution distribution)
    {
        Log.Information($"Opening installation folder for {distribution.Name}");
        _distributionService.OpenDistroInstallationLocation(distribution);
    }

    private static Tuple<string, string, string>? GetDistroCreationFormInfos(ContentDialog dialog)
    {
        Log.Information("Extracting creation form values");
        try
        {
            var distroNameInput = dialog.FindChild("DistroNameInput") as TextBox;
            var distroName = distroNameInput!.Text;
            var creationModeComboBox = dialog.FindChild("DistroCreationMode") as ComboBox;
            var creationMode = creationModeComboBox!.SelectedItem.ToString();

            TextBox? resourceOriginTextBox;
            var resourceOrigin = "";
            switch (creationMode)
            {
                case "Dockerfile":
                    resourceOriginTextBox = dialog.FindChild("DockerfileInput") as TextBox;
                    resourceOrigin = resourceOriginTextBox.Text;
                    break;
                case "Docker Hub":
                    resourceOriginTextBox = dialog.FindChild("DockerHubInput") as TextBox;
                    resourceOrigin = resourceOriginTextBox.Text;
                    break;
                case "Archive":
                    resourceOriginTextBox = dialog.FindChild("ArchiveInput") as TextBox;
                    resourceOrigin = resourceOriginTextBox.Text;
                    break;
                case "Vhdx":
                    resourceOriginTextBox = dialog.FindChild("VhdxInput") as TextBox;
                    resourceOrigin = resourceOriginTextBox.Text;
                    break;
            }
            return Tuple.Create(distroName, creationMode, resourceOrigin);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to parse creation form: {ex}");
            return null;
        }
    }

    private async Task CreateDistributionDialog()
    {
        Log.Information("Create distro dialog");
        try
        {
            var dialog = new CreateDistributionView { XamlRoot = App.MainWindow.Content.XamlRoot };
            dialog.PrimaryButtonClick += ValidateCreateDistribution;

            if (App.IsDistributionProcessing)
            {
                App.ShowIsProcessingDialog();
                return;
            }

            var buttonClicked = await dialog.ShowAsync();
            if (buttonClicked == ContentDialogResult.Primary)
            {
                var (distroName, creationMode, resourceOrigin) = GetDistroCreationFormInfos(dialog);
                await CreateDistributionViewModel(distroName, creationMode, resourceOrigin);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to open create dialog: {ex}");
        }
    }

    private void ValidateCreateDistribution(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        Log.Information("Validating create form");
        try
        {
            ValidateDistributionName(sender, args);
            ValidateCreationMode(sender, args);
        }
        catch (Exception ex)
        {
            Log.Error($"Create validation failed: {ex}");
        }
    }

    private static void ValidateCreationMode(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        Log.Information("Validating creation mode");
        try
        {
            var creationMode = sender.FindChild("DistroCreationMode") as ComboBox;
            creationMode!.ClearValue(Control.BorderBrushProperty);
            var errorInfoBar = sender.FindChild("CreationModeErrorInfoBar") as InfoBar;
            errorInfoBar!.IsOpen = false;
            if (creationMode.SelectedItem == null)
            {
                args.Cancel = true;
                errorInfoBar.IsOpen = true;
                creationMode.BorderBrush = new SolidColorBrush(Colors.DarkRed);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Creation mode validation failed: {ex}");
        }
    }

    internal async Task CreateDistributionViewModel(string distroName, string creationMode, string resourceOrigin)
    {
        Log.Information("Creating distribution");
        App.IsDistributionProcessing = true;
        var progressBarInfo = _infoBarService.FindInfoBar("CreateDistroInfoProgress");
        WeakReferenceMessenger.Default.Send(new DistroProgressBarMessage("Linux Manager creates your distribution ..."));
        _infoBarService.OpenInfoBar(progressBarInfo);
        try
        {
            var newDistro = await _distributionService.CreateDistribution(distroName, creationMode, resourceOrigin);
            _infoBarService.CloseInfoBar(progressBarInfo);
            var successInfo = _infoBarService.FindInfoBar("CreateDistroInfoSuccess");
            _infoBarService.OpenInfoBar(successInfo, 2000);
            Distros.Add(newDistro);
            App.IsDistributionProcessing = false;
        }
        catch (Exception ex)
        {
            Log.Error($"Create failed: {ex} ");
            _infoBarService.CloseInfoBar(progressBarInfo);
            var errorInfo = _infoBarService.FindInfoBar("CreateDistroInfoError");
            _infoBarService.OpenInfoBar(errorInfo, ex.Message, 5000);
            App.IsDistributionProcessing = false;
        }
    }

    private async Task CreateSnapshotDialog(Distribution distribution)
    {
        Log.Information($"Snapshot dialog for {distribution.Name}");
        try
        {
            var dialog = new CreateSnapshotView { XamlRoot = App.MainWindow.Content.XamlRoot };
            dialog.PrimaryButtonClick += ValidateSnapshotName;
            var buttonClicked = await dialog.ShowAsync();
            if (buttonClicked == ContentDialogResult.Primary)
            {
                WeakReferenceMessenger.Default.Send(new HideDistroStopButtonMessage(distribution));
                var snapshotName = (dialog.FindChild("SnapshotNameInput") as TextBox)!.Text;
                var snapshotDescr = (dialog.FindChild("SnapshotDescrInput") as TextBox)!.Text.Replace(';', ' ').Replace('\n', ' ').Replace('\r', ' ');
                var isFastSnapshot = (dialog.FindChild("IsFastSnapshot") as ToggleSwitch)!.IsOn;
                await CreateSnapshotViewModel(distribution, snapshotName, snapshotDescr, isFastSnapshot);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to open snapshot dialog: {ex}");
        }
    }

    private static void ValidateSnapshotName(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        Log.Information("Validating snapshot name");
        var snapshotNameInput = sender.FindChild("SnapshotNameInput") as TextBox;
        snapshotNameInput!.ClearValue(Control.BorderBrushProperty);
        var errorInfoBar = sender.FindChild("SnapshotNameErrorInfoBar") as InfoBar;
        errorInfoBar!.IsOpen = false;
        var regex = new Regex("^[a-zA-Z0-9-_ ]*$");
        const int minLength = 2;
        try
        {
            var validator = new TextInputValidation(snapshotNameInput.Text);
            validator.NotNullOrWhiteSpace().IncludeWhiteSpaceChar().MinimumLength(minLength).InvalidCharacters(regex, "special characters");
        }
        catch (ArgumentException e)
        {
            Log.Warning("Snapshot name validation failed");
            args.Cancel = true;
            errorInfoBar.Message = e.Message;
            errorInfoBar.IsOpen = true;
            snapshotNameInput.BorderBrush = new SolidColorBrush(Colors.DarkRed);
        }
    }

    private async Task CreateSnapshotViewModel(Distribution distribution, string snapshotName, string snapshotDescr, bool isFastSnapshot)
    {
        Log.Information($"Creating snapshot {snapshotName} for {distribution.Name}");
        WeakReferenceMessenger.Default.Send(new SnapshotProgressBarMessage("Linux Manager creates your snapshot ..."));
        try
        {
            var progressInfo = _infoBarService.FindInfoBar("CreateSnapshotInfoProgress");
            _infoBarService.OpenInfoBar(progressInfo);
            var isCreated = await _snapshotService.CreateSnapshot(distribution, snapshotName, snapshotDescr, isFastSnapshot);
            _infoBarService.CloseInfoBar(progressInfo);
            if (isCreated)
            {
                var successInfo = _infoBarService.FindInfoBar("CreateSnapshotInfoSuccess");
                _infoBarService.OpenInfoBar(successInfo, 2000);
            }
            else
            {
                var errorInfo = _infoBarService.FindInfoBar("CreateSnapshotInfoError");
                _infoBarService.OpenInfoBar(errorInfo, 5000);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Snapshot creation failed: {ex}");
        }
    }
}
