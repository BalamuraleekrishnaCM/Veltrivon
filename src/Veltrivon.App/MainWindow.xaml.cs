using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;

namespace Veltrivon.App;

public sealed partial class MainWindow : Window
{
    private readonly CatalogTool[] catalog = ToolOperations.LoadCatalog();
    private bool ready;
    private bool busy;
    private CancellationTokenSource? scanCancellation;
    private string? latestScanReport;

    public MainWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1120, 780));
        AddDiagnostics();
        Profile.SelectedIndex = 0;
        Navigation.SelectedItem = ToolNav;
        ready = true;
        ApplyFilter();
    }

    private void Navigate(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var view = (args.SelectedItem as NavigationViewItem)?.Tag as string ?? "tools";
        CatalogView.Visibility = view == "tools" ? Visibility.Visible : Visibility.Collapsed;
        DiagnosticsView.Visibility = view == "diagnostics" ? Visibility.Visible : Visibility.Collapsed;
        ActivityView.Visibility = view == "activity" ? Visibility.Visible : Visibility.Collapsed;
        (Heading.Text, Description.Text) = view switch
        {
            "diagnostics" => ("Diagnostics", "Inspect workstation inventory, disk health, system errors, and startup entries."),
            "activity" => ("Activity", "View operation results and open saved logs."),
            _ => ("Tool catalog", "Find, install, and open workstation, developer, and XR tools.")
        };
    }

    private void FilterChanged(object sender, RoutedEventArgs args)
    {
        if (ready) ApplyFilter();
    }

    private void ApplyFilter()
    {
        var profile = (Profile.SelectedItem as ComboBoxItem)?.Tag as string;
        var query = Search.Text.Trim();
        Tools.ItemsSource = catalog.Where(tool =>
            (string.IsNullOrEmpty(profile) || tool.InProfile(profile)) &&
            (tool.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (tool.Id?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))).ToArray();
        UpdateActions();
    }

    private void ToolSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (ready) UpdateActions();
    }

    private CatalogTool[] SelectedTools() => Tools.SelectedItems.OfType<CatalogTool>().ToArray();

    private void UpdateActions()
    {
        var selected = SelectedTools();
        Selection.Text = $"{Tools.Items.Count} shown · {selected.Length} selected";
        if (selected.Length == 1 && !selected[0].CanInstall)
            Selection.Text += " · Publisher download";
        SelectVisible.IsEnabled = !busy && Tools.Items.Count > 0;
        Check.IsEnabled = !busy && selected.Any(tool => !string.IsNullOrWhiteSpace(tool.Id));
        Install.IsEnabled = !busy && selected.Any(tool => tool.CanInstall);
        Launch.IsEnabled = !busy && selected.Length == 1;
        Website.IsEnabled = !busy && selected.Length == 1;
        foreach (var card in DiagnosticCards.Children.OfType<StackPanel>())
            foreach (var button in card.Children.OfType<Button>())
                button.IsEnabled = !busy;
    }

    private void SelectTools(object sender, RoutedEventArgs args) => Tools.SelectAll();

    private async void CheckPackages(object sender, RoutedEventArgs args)
    {
        var selected = SelectedTools().Where(tool => !string.IsNullOrWhiteSpace(tool.Id)).ToArray();
        await RunPackagesAsync(selected, false);
    }

    private async void InstallPackages(object sender, RoutedEventArgs args)
    {
        var selected = SelectedTools().Where(tool => tool.CanInstall).ToArray();
        if (selected.Length == 0 || busy) return;

        var dialog = new ContentDialog
        {
            XamlRoot = Root.XamlRoot,
            Title = "Review installation",
            Content = new ScrollViewer
            {
                MaxHeight = 310,
                Content = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = "Continue to accept the WinGet source and package agreements for these tools. Some installers may show their own prompts.\n\n" +
                        string.Join("\n", selected.Select(tool => $"{tool.Name} ({tool.Id})"))
                }
            },
            PrimaryButtonText = "Accept and install",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            await RunPackagesAsync(selected, true);
    }

    private Task RunPackagesAsync(CatalogTool[] selected, bool install) => RunJobAsync(
        install ? "Installing tools" : "Checking WinGet", async output =>
        {
            var successful = 0;
            foreach (var tool in selected)
            {
                Status.Text = $"{(install ? "Installing" : "Checking")} {tool.Name}…";
                output($"{tool.Name} ({tool.Id})");
                if (install)
                {
                    var installed = await ToolOperations.QueryPackageAsync("list", tool.Id!, output);
                    if (installed.Succeeded)
                    {
                        output("Already installed.");
                        successful++;
                        continue;
                    }

                    // WinGet's no-installed-package result permits proceeding to install.
                    if (installed.ExitCode != unchecked((int)0x8A150014))
                    {
                        output($"Installed status unavailable. {installed.Summary}");
                        continue;
                    }
                }

                var result = await ToolOperations.QueryPackageAsync(install ? "install" : "search", tool.Id!, output);
                if (result.Succeeded) successful++;
                output(result.Summary);
            }
            return $"{successful} of {selected.Length} completed successfully.";
        });

    private async void LaunchTool(object sender, RoutedEventArgs args)
    {
        if (SelectedTools() is not [var tool]) return;
        var command = tool.Id switch
        {
            "Google.PlatformTools" => (File: "adb.exe", Argument: "devices"),
            "Gyan.FFmpeg" => (File: "ffmpeg.exe", Argument: "-version"),
            "Git.Git" => (File: "git.exe", Argument: "--version"),
            _ => (File: "", Argument: "")
        };
        try
        {
            SetBusy(true);
            if (command.File.Length > 0)
            {
                var links = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft", "WinGet", "Links", command.File);
                var executable = File.Exists(links) ? links : await PickToolAsync(command.File);
                if (executable is null)
                {
                    Status.Text = "Launch canceled.";
                    return;
                }
                SetBusy(false);
                await RunJobAsync(tool.Name, async output =>
                {
                    var result = await ToolOperations.RunAsync(executable, [command.Argument], output);
                    return result.Summary;
                });
                return;
            }
            Status.Text = $"Finding {tool.Name}…";
            var apps = await ToolOperations.StartAppsAsync();
            var matches = apps.Where(app => string.Equals(app.Name, tool.Name, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matches is [var match])
            {
                ToolOperations.Open(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"),
                    "shell:AppsFolder\\" + match.AppID);
                Status.Text = $"Opened {tool.Name}.";
                return;
            }

            Status.Text = $"Choose the installed executable for {tool.Name}.";
            var executablePath = await PickToolAsync();
            if (executablePath is not null) ToolOperations.Open(executablePath);
            Status.Text = executablePath is null ? "Launch canceled." : $"Opened {Path.GetFileName(executablePath)}.";
        }
        catch (Exception error) when (error is Win32Exception or IOException or InvalidOperationException
            or System.Runtime.InteropServices.COMException or System.Text.Json.JsonException)
        {
            Status.Text = error.Message;
        }
        finally { SetBusy(false); }
    }

    private async Task<string?> PickToolAsync(string? expectedName = null)
    {
        if (expectedName is not null) Status.Text = $"Choose the installed {expectedName}.";
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            CommitButtonText = "Run selected tool"
        };
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
        picker.FileTypeFilter.Add(".exe");
        var file = await picker.PickSingleFileAsync();
        if (file is not null && expectedName is not null &&
            !string.Equals(file.Name, expectedName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Select {expectedName} to run this action.");
        return file?.Path;
    }

    private void OpenWebsite(object sender, RoutedEventArgs args)
    {
        if (SelectedTools() is [var tool] && Uri.TryCreate(tool.Url, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps)
            OpenExternal(uri.AbsoluteUri);
    }

    private void OpenLogs(object sender, RoutedEventArgs args)
    {
        try
        {
            Directory.CreateDirectory(ToolOperations.LogFolder);
            OpenExternal(ToolOperations.LogFolder);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Status.Text = error.Message;
        }
    }

    private void OpenExternal(string path)
    {
        try { ToolOperations.Open(path); }
        catch (Win32Exception error) { Status.Text = error.Message; }
    }

    private void AddDiagnostics()
    {
        var scanPanel = new StackPanel { Spacing = 6 };
        var scanButton = new Button { Content = "Run smart scan" };
        scanButton.Click += async (_, _) => await RunDiagnosticsAsync("Smart scan", ToolOperations.Diagnostics);
        scanPanel.Children.Add(scanButton);
        DiagnosticCards.Children.Add(scanPanel);

        foreach (var diagnostic in ToolOperations.Diagnostics)
        {
            var panel = new StackPanel { Spacing = 6 };
            panel.Children.Add(new TextBlock { Text = diagnostic.Name, FontSize = 20 });
            panel.Children.Add(new TextBlock { Text = diagnostic.Description, TextWrapping = TextWrapping.Wrap });
            var button = new Button { Content = $"Run {diagnostic.Name.ToLowerInvariant()}" };
            button.Click += async (_, _) => await RunDiagnosticsAsync(diagnostic.Name, [diagnostic]);
            panel.Children.Add(button);
            DiagnosticCards.Children.Add(panel);
        }
    }

    private async Task RunDiagnosticsAsync(string title, Diagnostic[] diagnostics)
    {
        if (busy) return;
        using var cancellation = new CancellationTokenSource();
        scanCancellation = cancellation;
        CancelJob.IsEnabled = true;
        CancelJob.Visibility = Visibility.Visible;
        var report = new List<string> { $"Veltrivon · {title}", $"Started: {DateTimeOffset.Now:O}" };
        try
        {
            await RunJobAsync(title, async output =>
            {
                var completed = 0;
                foreach (var diagnostic in diagnostics)
                {
                    cancellation.Token.ThrowIfCancellationRequested();
                    Status.Text = $"Checking {diagnostic.Name.ToLowerInvariant()}…";
                    output(diagnostic.Name);
                    var result = await ToolOperations.RunAsync(ToolOperations.PowerShell,
                        ToolOperations.PowerShellArguments(diagnostic.Command), output, cancellation.Token);
                    if (result.Succeeded) completed++;
                    output(result.Summary);
                    report.Add($"{diagnostic.Name}: {result.Summary}");
                }
                var summary = $"{completed} of {diagnostics.Length} checks completed successfully.";
                report.Add(summary);
                return summary;
            }, report);
        }
        finally
        {
            scanCancellation = null;
            CancelJob.Visibility = Visibility.Collapsed;
        }
    }

    private void CancelScan(object sender, RoutedEventArgs args)
    {
        CancelJob.IsEnabled = false;
        scanCancellation?.Cancel();
    }

    private async void SaveReport(object sender, RoutedEventArgs args)
    {
        if (busy || latestScanReport is not { } report) return;
        SetBusy(true);
        try
        {
            var picker = new FileSavePicker { SuggestedFileName = "Veltrivon diagnostic report" };
            picker.FileTypeChoices.Add("Text report", [".txt"]);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file = await picker.PickSaveFileAsync();
            if (file is not null)
            {
                await Windows.Storage.FileIO.WriteTextAsync(file, report);
                Status.Text = $"Report saved to {file.Name}.";
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException
            or System.Runtime.InteropServices.COMException)
        {
            Status.Text = error.Message;
        }
        finally { SetBusy(false); }
    }

    private async Task RunJobAsync(string title, Func<Action<string>, Task<string>> work, List<string>? scanReport = null)
    {
        if (busy) return;
        SetBusy(true);
        Navigation.SelectedItem = Navigation.MenuItems[2];
        Activity.Text = title + Environment.NewLine;
        Status.Text = title + "…";
        ScrollViewer.SetVerticalScrollBarVisibility(Activity, ScrollBarVisibility.Auto);
        string? logPath = null;
        var output = new List<string> { $"{DateTimeOffset.Now:O} {title}" };
        void Append(string line)
        {
            lock (output) output.Add(line);
        }

        try
        {
            logPath = Path.Combine(ToolOperations.LogFolder, $"job-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.log");
            Status.Text = await work(Append);
        }
        catch (OperationCanceledException) when (scanReport is not null)
        {
            Status.Text = "Scan canceled. Output includes checks that ran before cancellation.";
            scanReport.Add("Scan canceled.");
        }
        catch (Exception error) when (error is Win32Exception or IOException or UnauthorizedAccessException
            or InvalidOperationException)
        {
            Status.Text = error.Message;
            scanReport?.Add("Scan stopped because a check could not run. See the local job log for details.");
        }
        finally
        {
            string[] lines;
            lock (output)
            {
                output.Add(Status.Text);
                lines = output.ToArray();
            }
            Activity.Text = string.Join(Environment.NewLine, lines);
            if (scanReport is not null)
            {
                scanReport.Add($"Finished: {DateTimeOffset.Now:O}");
                latestScanReport = string.Join(Environment.NewLine, scanReport);
            }
            try
            {
                if (logPath is not null)
                {
                    Directory.CreateDirectory(ToolOperations.LogFolder);
                    await File.WriteAllLinesAsync(logPath, lines);
                }
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                Status.Text += " Log could not be saved. " + error.Message;
            }
            SetBusy(false);
        }
    }

    private void SetBusy(bool value)
    {
        busy = value;
        Progress.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        ExportReport.IsEnabled = !value && latestScanReport is not null;
        UpdateActions();
        if (FrameworkElementAutomationPeer.FromElement(Status) is { } peer)
            peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
    }
}
