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
        AddChecks(DiagnosticCards, "Run all diagnostics", "Diagnostics", ToolOperations.Diagnostics);
        AddChecks(SecurityCards, "Run security checks", "Security checks", ToolOperations.SecurityChecks);
        Profile.SelectedIndex = 0;
        Navigation.SelectedItem = OverviewNav;
        ready = true;
        ApplyFilter();
    }

    private void Navigate(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var view = (args.SelectedItem as NavigationViewItem)?.Tag as string ?? "overview";
        OverviewView.Visibility = view == "overview" ? Visibility.Visible : Visibility.Collapsed;
        CatalogView.Visibility = view == "tools" ? Visibility.Visible : Visibility.Collapsed;
        UpdatesView.Visibility = view == "updates" ? Visibility.Visible : Visibility.Collapsed;
        SecurityView.Visibility = view == "security" ? Visibility.Visible : Visibility.Collapsed;
        DiagnosticsView.Visibility = view == "diagnostics" ? Visibility.Visible : Visibility.Collapsed;
        ActivityView.Visibility = view == "activity" ? Visibility.Visible : Visibility.Collapsed;
        (Heading.Text, Description.Text) = view switch
        {
            "overview" => ("Overview", "Manage and check this Windows workstation."),
            "updates" => ("Software updates", "Check available updates or review your installed apps."),
            "security" => ("Security checks", "Inspect Windows protection, firewall, drive encryption, and Secure Boot."),
            "diagnostics" => ("Diagnostics", "Inspect workstation inventory, disk health, system errors, and startup entries."),
            "activity" => ("Activity", "View operation results and open saved logs."),
            _ => ("Tool catalog", "Find, install, and open workstation, developer, and XR tools.")
        };
    }

    private void GoToTools(object sender, RoutedEventArgs args) => Navigation.SelectedItem = ToolNav;

    private void GoToUpdates(object sender, RoutedEventArgs args) => Navigation.SelectedItem = UpdatesNav;

    private async void CheckWorkstation(object sender, RoutedEventArgs args)
    {
        if (busy) return;
        await RunDiagnosticsAsync("Workstation check", [.. ToolOperations.SecurityChecks, .. ToolOperations.Diagnostics], checkUpdates: true);
        LastOverviewScan.Text = $"Last workstation check attempt · {DateTime.Now:g}";
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
        Test.IsEnabled = Check.IsEnabled;
        Install.IsEnabled = !busy && selected.Any(tool => tool.CanInstall);
        Launch.IsEnabled = !busy && selected.Length == 1;
        Website.IsEnabled = !busy && selected.Length == 1;
        OverviewScan.IsEnabled = CheckUpdates.IsEnabled = ListSoftware.IsEnabled = RefreshSources.IsEnabled = !busy;
        foreach (var card in DiagnosticCards.Children.Concat(SecurityCards.Children).OfType<StackPanel>())
            foreach (var button in card.Children.OfType<Button>())
                button.IsEnabled = !busy;
    }

    private void SelectTools(object sender, RoutedEventArgs args) => Tools.SelectAll();

    private async void CheckPackages(object sender, RoutedEventArgs args)
    {
        var selected = SelectedTools().Where(tool => !string.IsNullOrWhiteSpace(tool.Id)).ToArray();
        await RunPackagesAsync(selected, false);
    }

    private async void TestPackages(object sender, RoutedEventArgs args)
    {
        var selected = SelectedTools().Where(tool => !string.IsNullOrWhiteSpace(tool.Id)).ToArray();
        await RunJobAsync("Testing tool installations", async output =>
        {
            var found = 0;
            var commandTests = 0;
            var passedTests = 0;
            foreach (var tool in selected)
            {
                Status.Text = $"Checking the installation for {tool.Name}…";
                output(tool.Name);
                var result = await ToolOperations.QueryPackageAsync("list", tool.Id!, output);
                if (result.Succeeded) found++;
                output(result.ExitCode == ToolOperations.NoInstalledPackage ? "No matching installation found." : result.Summary);
                if (!result.Succeeded || ToolOperations.FindToolTest(tool.Id!) is not { } test) continue;
                commandTests++;
                try
                {
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                    var tested = await ToolOperations.RunAsync(test.Path, [test.Argument], output, timeout.Token);
                    if (tested.Succeeded) passedTests++;
                    output($"Executable test: {tested.Summary}");
                }
                catch (OperationCanceledException)
                {
                    output("Executable test timed out.");
                }
                catch (Win32Exception error)
                {
                    output($"Executable test could not run. {error.Message}");
                }
            }
            var summary = $"WinGet identified {found} of {selected.Length} selected installations.";
            return commandTests == 0 ? summary : $"{summary} {passedTests} of {commandTests} executable tests passed.";
        });
    }

    private async void CheckSoftwareUpdates(object sender, RoutedEventArgs args) =>
        await RunWinGetJobAsync("Checking software updates", ["list", "--upgrade-available"]);

    private async void ListInstalledSoftware(object sender, RoutedEventArgs args) =>
        await RunWinGetJobAsync("Listing installed software", ["list"]);

    private async void RefreshPackageSources(object sender, RoutedEventArgs args) =>
        await RunWinGetJobAsync("Refreshing WinGet sources", ["source", "update"], acceptSourceAgreements: false);

    private Task RunWinGetJobAsync(string title, string[] arguments, bool acceptSourceAgreements = true) => RunJobAsync(title, async output =>
    {
        var flags = acceptSourceAgreements
            ? new[] { "--accept-source-agreements", "--disable-interactivity" }
            : new[] { "--disable-interactivity" };
        var result = await ToolOperations.RunWinGetAsync([.. arguments, .. flags], output);
        return WinGetSummary(result.ExitCode, result.Summary, arguments.Contains("--upgrade-available"));
    });

    private static string WinGetSummary(int? exitCode, string summary, bool checkingUpdates) =>
        checkingUpdates && exitCode == ToolOperations.NoInstalledPackage ? "WinGet reported no available updates." : summary;

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
                    if (installed.ExitCode != ToolOperations.NoInstalledPackage)
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

    private void AddChecks(StackPanel cards, string buttonTitle, string jobTitle, Diagnostic[] checks)
    {
        var scanPanel = new StackPanel { Spacing = 6 };
        var scanButton = new Button { Content = buttonTitle };
        scanButton.Click += async (_, _) => await RunDiagnosticsAsync(jobTitle, checks);
        scanPanel.Children.Add(scanButton);
        cards.Children.Add(scanPanel);

        foreach (var diagnostic in checks)
        {
            var panel = new StackPanel { Spacing = 6 };
            panel.Children.Add(new TextBlock { Text = diagnostic.Name, FontSize = 20 });
            panel.Children.Add(new TextBlock { Text = diagnostic.Description, TextWrapping = TextWrapping.Wrap });
            var button = new Button { Content = $"Run {diagnostic.Name.ToLowerInvariant()}" };
            button.Click += async (_, _) => await RunDiagnosticsAsync(diagnostic.Name, [diagnostic]);
            panel.Children.Add(button);
            cards.Children.Add(panel);
        }
    }

    private async Task RunDiagnosticsAsync(string title, Diagnostic[] diagnostics, bool checkUpdates = false)
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
                if (checkUpdates)
                {
                    Status.Text = "Checking software updates…";
                    output("Software updates");
                    try
                    {
                        var result = await ToolOperations.RunWinGetAsync(
                            ["list", "--upgrade-available", "--accept-source-agreements", "--disable-interactivity"],
                            output, cancellation.Token);
                        if (result.Succeeded || result.ExitCode == ToolOperations.NoInstalledPackage) completed++;
                        report.Add($"Software updates: {WinGetSummary(result.ExitCode, result.Summary, true)}");
                    }
                    catch (Exception error) when (error is Win32Exception or InvalidOperationException)
                    {
                        report.Add($"Software updates: {error.Message}");
                        output(error.Message);
                    }
                }
                var count = diagnostics.Length + (checkUpdates ? 1 : 0);
                var summary = $"{completed} of {count} checks ran. Review the findings in Activity.";
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
        Navigation.SelectedItem = ActivityNav;
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
