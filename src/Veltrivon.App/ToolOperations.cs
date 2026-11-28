using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Veltrivon.Core;

namespace Veltrivon.App;

public sealed record CatalogTool(string Name, string? Id, string Profile, string Url, [property: JsonPropertyName("also_profiles")] string[]? AlsoProfiles)
{
    public bool InProfile(string profile) => Profile == profile || AlsoProfiles?.Contains(profile) == true;

    public bool CanInstall => Id is { Length: > 0 } && Profile is "essential" or "extended" or "xr" or "developer" or "xr-open";
}

internal sealed record StartApp(string Name, string AppID);
internal sealed record Diagnostic(string Name, string Description, string Command);

internal static class ToolOperations
{
    internal const int NoInstalledPackage = unchecked((int)0x8A150014);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    internal static readonly string LogFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Veltrivon", "Jobs");

    internal static readonly Diagnostic[] Diagnostics =
    [
        new("Workstation overview", "Windows version, processor, memory, and free disk space",
            """
            Get-CimInstance Win32_OperatingSystem |
                Select-Object Caption, Version, @{Name='TotalMemoryGiB';Expression={[math]::Round($_.TotalVisibleMemorySize / 1MB, 1)}},
                @{Name='FreeMemoryGiB';Expression={[math]::Round($_.FreePhysicalMemory / 1MB, 1)}} |
                Format-List | Out-String
            Get-CimInstance Win32_Processor | Select-Object Name, NumberOfLogicalProcessors |
                Format-Table -AutoSize | Out-String
            Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3' |
                Select-Object DeviceID, @{Name='FreeGiB';Expression={[math]::Round($_.FreeSpace / 1GB, 1)}},
                @{Name='TotalGiB';Expression={[math]::Round($_.Size / 1GB, 1)}} |
                Format-Table -AutoSize | Out-String
            """),
        new("Disk health", "Physical disk health and operational status",
            "Get-PhysicalDisk | Select-Object FriendlyName, MediaType, HealthStatus, OperationalStatus | Format-Table -AutoSize | Out-String -Width 160"),
        new("System errors", "Latest 20 critical and error events from the Windows System log",
            """
            try {
                Get-WinEvent -FilterHashtable @{ LogName='System'; Level=1,2 } -MaxEvents 20 |
                    Select-Object TimeCreated, Id, ProviderName, Message | Format-List | Out-String -Width 160
            } catch {
                if ($_.FullyQualifiedErrorId -notlike 'NoMatchingEventsFound,*') { throw }
                'No critical or error events were found.'
            }
            """),
        new("Startup inventory", "Startup entry names, locations, and commands",
            "Get-CimInstance Win32_StartupCommand | Select-Object Name, Location, Command | Format-List | Out-String -Width 160")
    ];

    internal static readonly Diagnostic[] SecurityChecks =
    [
        new("Microsoft Defender", "Antivirus, real-time protection, and security intelligence status",
            """
            Get-MpComputerStatus |
                Select-Object AMRunningMode, AntivirusEnabled, RealTimeProtectionEnabled,
                    AntivirusSignatureAge, AntivirusSignatureLastUpdated |
                Format-List | Out-String
            """),
        new("Windows Firewall", "Firewall status for domain, private, and public networks",
            "Get-NetFirewallProfile -PolicyStore ActiveStore | Select-Object Name, Enabled, DefaultInboundAction | Format-Table -AutoSize | Out-String"),
        new("Drive encryption", "BitLocker protection and encryption status",
            "Get-BitLockerVolume | Select-Object MountPoint, ProtectionStatus, VolumeStatus, EncryptionPercentage | Format-Table -AutoSize | Out-String"),
        new("Secure Boot", "UEFI Secure Boot status",
            "if (Confirm-SecureBootUEFI) { 'Secure Boot is enabled.' } else { 'Secure Boot is disabled.' }")
    ];

    internal static CatalogTool[] LoadCatalog()
    {
        using var stream = typeof(ToolOperations).Assembly.GetManifestResourceStream("Veltrivon.Catalog.json")
            ?? throw new InvalidOperationException("The embedded tool catalog is missing.");
        return JsonSerializer.Deserialize<CatalogTool[]>(stream, JsonOptions)
            ?? throw new InvalidOperationException("The tool catalog is empty.");
    }

    internal static string PowerShell => Path.Combine(Environment.SystemDirectory,
        "WindowsPowerShell", "v1.0", "powershell.exe");

    internal static string[] PowerShellArguments(string command) =>
    [
        "-NoLogo", "-NoProfile", "-NonInteractive", "-Command",
        "[Console]::OutputEncoding = [Text.UTF8Encoding]::new(); $ErrorActionPreference = 'Stop'; " + command
    ];

    internal static Task<OperationResult> QueryPackageAsync(string action, string id, Action<string> output)
    {
        var arguments = new List<string>
        {
            action, "--id", id, "--exact", "--source", "winget",
            "--accept-source-agreements", "--disable-interactivity"
        };
        if (action == "install")
            arguments.Add("--accept-package-agreements");

        return RunWinGetAsync(arguments, output);
    }

    internal static Task<OperationResult> RunWinGetAsync(IEnumerable<string> arguments, Action<string> output,
        CancellationToken cancellationToken = default)
    {
        var winget = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft", "WindowsApps", "winget.exe");
        if (!File.Exists(winget))
            throw new InvalidOperationException("Enable the WinGet app execution alias or install Microsoft App Installer.");

        return RunAsync(winget, arguments, output, cancellationToken);
    }

    internal static async Task<StartApp[]> StartAppsAsync()
    {
        var json = new StringBuilder();
        var result = await RunAsync(PowerShell,
            PowerShellArguments("ConvertTo-Json -InputObject @(Get-StartApps | Select-Object Name, AppID) -Compress"),
            line => json.AppendLine(line));
        if (!result.Succeeded)
            throw new InvalidOperationException("Windows could not read the Start menu app list.");
        return JsonSerializer.Deserialize<StartApp[]>(json.ToString(), JsonOptions) ?? [];
    }

    internal static (string Path, string Argument)? FindToolTest(string id)
    {
        var (executable, argument) = id switch
        {
            "Git.Git" => ("git.exe", "--version"),
            "GoLang.Go" => ("go.exe", "version"),
            "Microsoft.DotNet.SDK.10" => ("dotnet.exe", "--list-sdks"),
            "OpenJS.NodeJS.LTS" => ("node.exe", "--version"),
            "Kitware.CMake" => ("cmake.exe", "--version"),
            "Ninja-build.Ninja" => ("ninja.exe", "--version"),
            "Google.PlatformTools" => ("adb.exe", "version"),
            "Genymobile.scrcpy" => ("scrcpy.exe", "--version"),
            "astral-sh.uv" => ("uv.exe", "--version"),
            "Oven-sh.Bun" => ("bun.exe", "--version"),
            _ => ("", "")
        };
        if (executable.Length == 0) return null;

        var links = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft", "WinGet", "Links");
        var candidates = (Environment.GetEnvironmentVariable("PATH") ?? "")
            .Split(Path.PathSeparator).Prepend(links);
        foreach (var entry in candidates)
        {
            var directory = entry.Trim('"');
            if (Path.IsPathFullyQualified(directory) && File.Exists(Path.Combine(directory, executable)))
                return (Path.Combine(directory, executable), argument);
        }
        return null;
    }

    internal static void Open(string path, params string[] arguments)
    {
        var info = new ProcessStartInfo(path) { UseShellExecute = true };
        foreach (var argument in arguments)
            info.ArgumentList.Add(argument);
        using var process = Process.Start(info);
    }

    internal static async Task<OperationResult> RunAsync(
        string executable, IEnumerable<string> arguments, Action<string> output,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var startInfo = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };
        process.OutputDataReceived += (_, args) => { if (args.Data is { } line) output(line); };
        process.ErrorDataReceived += (_, args) => { if (args.Data is { } line) output(line); };
        if (!process.Start())
            throw new InvalidOperationException("Windows could not start the requested tool.");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) when (process.HasExited) { }
            await process.WaitForExitAsync();
            throw;
        }
        var code = process.ExitCode;
        return new OperationResult(code == 0, code == 0 ? "Completed" : $"Exit code 0x{code:X8}", code);
    }
}
