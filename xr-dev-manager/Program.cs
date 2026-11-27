using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using Microsoft.Win32;

return await Run(args);

static async Task<int> Run(string[] args)
{
    if (args.Length == 0 || args is ["help" or "--help" or "-h"])
    {
        Console.WriteLine("""
            XR Dev Manager
              doctor                        Show tool and OpenXR runtime availability
              devices                       List connected ADB devices and authorization state
              pair <ip:port>                Pair using the headset's wireless debugging code
              connect <ip:port>             Connect to an authorized wireless debugging endpoint
              apps <serial>                 List third-party package IDs
              install <serial> <file.apk>   Install or update a local APK
              logs <serial>                 Stream headset logs until Ctrl+C
              mirror <serial>               Open a read-only headset preview
              record <serial> <file.mp4>    Mirror and record to a new local file
              pcvr <launcher.exe>           Start an installed PC VR launcher
            """);
        return 0;
    }

    using var cancellation = new CancellationTokenSource();
    ConsoleCancelEventHandler cancel = (_, e) =>
    {
        e.Cancel = true;
        cancellation.Cancel();
    };
    Console.CancelKeyPress += cancel;
    try
    {
        return args switch
        {
            ["doctor"] => Doctor(),
            ["devices"] => await RunTool("adb", ["devices", "-l"], cancellation.Token),
            ["pair", var endpoint] => await RunTool("adb", ["pair", Endpoint(endpoint)], cancellation.Token),
            ["connect", var endpoint] => await RunTool("adb", ["connect", Endpoint(endpoint)], cancellation.Token),
            ["apps", var serial] => await RunTool("adb",
                ["-s", Serial(serial), "shell", "pm", "list", "packages", "-3"], cancellation.Token),
            ["install", var serial, var apk] => await RunTool("adb",
                ["-s", Serial(serial), "install", "-r", InputFile(apk, ".apk")], cancellation.Token),
            ["logs", var serial] => await RunTool("adb",
                ["-s", Serial(serial), "logcat", "-v", "threadtime"], cancellation.Token),
            ["mirror", var serial] => await RunTool("scrcpy",
                ["--serial=" + Serial(serial), "--no-control", "--no-audio"], cancellation.Token),
            ["record", var serial, var output] => await RunTool("scrcpy",
                ["--serial=" + Serial(serial), "--no-control", "--no-audio", "--record=" + NewRecording(output)],
                cancellation.Token),
            ["pcvr", var launcher] => Launch(launcher),
            _ => throw new ArgumentException("Invalid command or arguments. Run with --help.")
        };
    }
    catch (OperationCanceledException)
    {
        return 130;
    }
    catch (Exception error) when (error is ArgumentException or IOException or Win32Exception
        or UnauthorizedAccessException or InvalidOperationException)
    {
        Console.Error.WriteLine(error.Message);
        return 2;
    }
    finally
    {
        Console.CancelKeyPress -= cancel;
    }
}

static string Serial(string value)
{
    if (string.IsNullOrWhiteSpace(value) || value.StartsWith('-') || value.Any(char.IsWhiteSpace))
        throw new ArgumentException("Use a device serial from the devices command.");
    return value;
}

static string Endpoint(string value)
{
    if (!IPEndPoint.TryParse(value, out var endpoint) || endpoint.Port == 0)
        throw new ArgumentException("Use an IP address and port from the wireless debugging screen.");
    return endpoint.ToString();
}

static string InputFile(string path, string extension)
{
    var fullPath = Path.GetFullPath(path);
    if (!Path.GetExtension(fullPath).Equals(extension, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        throw new ArgumentException($"Choose an existing {extension} file.");
    return fullPath;
}

static string NewRecording(string path)
{
    var fullPath = Path.GetFullPath(path);
    if (!Path.GetExtension(fullPath).Equals(".mp4", StringComparison.OrdinalIgnoreCase)
        || !Directory.Exists(Path.GetDirectoryName(fullPath)) || Path.Exists(fullPath))
        throw new ArgumentException("Choose an unused .mp4 filename in an existing folder.");
    return fullPath;
}

static string? FindTool(string name)
{
    var configured = Environment.GetEnvironmentVariable("XRDEV_" + name.ToUpperInvariant());
    if (!string.IsNullOrWhiteSpace(configured))
        return InputFile(configured, OperatingSystem.IsWindows() ? ".exe" : "");
    var executable = OperatingSystem.IsWindows() ? name + ".exe" : name;
    foreach (var entry in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
    {
        var directory = entry.Trim('"');
        if (!Path.IsPathFullyQualified(directory))
            continue;
        var candidate = Path.Combine(directory, executable);
        if (File.Exists(candidate))
            return candidate;
    }
    return null;
}

static async Task<int> RunTool(string name, string[] arguments, CancellationToken cancellation)
{
    var executable = FindTool(name) ?? throw new FileNotFoundException(
        $"Install {name} with the XR tools profile or set XRDEV_{name.ToUpperInvariant()} to its full path.");
    var start = new ProcessStartInfo(executable) { UseShellExecute = false };
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    if (name == "scrcpy" && FindTool("adb") is { } adb)
        start.Environment["ADB"] = adb;

    cancellation.ThrowIfCancellationRequested();
    using var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not start {name}.");
    try
    {
        await process.WaitForExitAsync(cancellation);
    }
    catch (OperationCanceledException)
    {
        // Console children also receive Ctrl+C. Let scrcpy finalize recordings before forcing exit.
        using var grace = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        try
        {
            await process.WaitForExitAsync(grace.Token);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(); }
            catch (InvalidOperationException) when (process.HasExited) { }
            await process.WaitForExitAsync();
        }
        throw;
    }
    return process.ExitCode;
}

static int Launch(string launcher)
{
    if (!OperatingSystem.IsWindows())
        throw new InvalidOperationException("PC VR launcher integration requires Windows.");
    var executable = InputFile(launcher, ".exe");
    using var process = Process.Start(new ProcessStartInfo(executable)
    {
        UseShellExecute = false,
        WorkingDirectory = Path.GetDirectoryName(executable)!
    }) ?? throw new InvalidOperationException("Could not start the PC VR launcher.");
    return 0;
}

static int Doctor()
{
    foreach (var name in new[] { "adb", "scrcpy" })
    {
        try { Console.WriteLine($"{name}: {FindTool(name) ?? "missing"}"); }
        catch (ArgumentException error) { Console.WriteLine($"{name}: {error.Message}"); }
    }
    var overridePath = Environment.GetEnvironmentVariable("XR_RUNTIME_JSON");
    if (!string.IsNullOrWhiteSpace(overridePath))
        Console.WriteLine($"XR_RUNTIME_JSON: {overridePath} ({ManifestStatus(overridePath)})");

    if (OperatingSystem.IsWindows())
    {
        using var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
            Environment.Is64BitProcess ? RegistryView.Registry64 : RegistryView.Registry32);
        using var openXr = machine.OpenSubKey(@"SOFTWARE\Khronos\OpenXR\1");
        var path = openXr?.GetValue("ActiveRuntime") as string;
        Console.WriteLine(path is null
            ? "Windows OpenXR registration: missing"
            : $"Windows OpenXR registration: {path} ({ManifestStatus(path)})");
    }
    return 0;
}

static string ManifestStatus(string path) => File.Exists(path) ? "file exists" : "file missing";
