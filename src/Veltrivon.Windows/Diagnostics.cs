using System.Diagnostics;
using System.Text.Json;
using Veltrivon.Core;

namespace Veltrivon.Windows;

public sealed record Probe(string Name, string State, JsonElement? Data, string? Error);
public sealed record ScanReport(DateTimeOffset Timestamp, IReadOnlyList<Probe> Probes, IReadOnlyList<Finding> Findings);
public sealed class Diagnostics
{
    // All scripts are constants. No user-supplied command text or elevation.
    private static readonly (string Name, string Script)[] Checks =
    [
        ("defender", "Get-MpComputerStatus | Select-Object AMRunningMode,AntivirusEnabled,RealTimeProtectionEnabled,AntivirusSignatureAge,AntivirusSignatureLastUpdated,IsTamperProtected"),
        ("firewall", "Get-NetFirewallProfile | Select-Object Name,Enabled"),
        ("antivirus-providers", "Get-CimInstance -Namespace root/SecurityCenter2 -ClassName AntiVirusProduct | Select-Object displayName"),
        ("storage", "Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3' | Select-Object DeviceID,Size,FreeSpace"),
        ("physical-disk", "Get-PhysicalDisk | Select-Object MediaType,HealthStatus,OperationalStatus,Size"),
        ("system", "Get-CimInstance Win32_OperatingSystem | Select-Object Caption,Version,BuildNumber,TotalVisibleMemorySize,FreePhysicalMemory"),
        ("cpu", "Get-CimInstance Win32_Processor | Select-Object Name,NumberOfCores,NumberOfLogicalProcessors,LoadPercentage"),
        ("network", "Get-NetTCPConnection -State Listen | Group-Object LocalPort | Select-Object @{n='Port';e={[int]$_.Name}},Count"),
        ("startup", "Get-CimInstance Win32_StartupCommand | Measure-Object | Select-Object Count"),
        ("apps", "Get-ItemProperty 'HKLM:\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*','HKLM:\\Software\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*','HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*' -ErrorAction SilentlyContinue | Where-Object DisplayName | Measure-Object | Select-Object Count")
    ];
    public async Task<ScanReport> ScanAsync(CancellationToken ct)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Windows only.");
        var probes = new List<Probe>();
        foreach (var check in Checks)
        {
            ct.ThrowIfCancellationRequested();
            probes.Add(await RunAsync(check.Name, check.Script, ct));
        }
        return new(DateTimeOffset.UtcNow, probes, Evaluate(probes));
    }
    private static async Task<Probe> RunAsync(string name, string script, CancellationToken ct)
    {
        var exe = Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");
        var psi = new ProcessStartInfo(exe) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var arg in new[] { "-NoLogo", "-NoProfile", "-NonInteractive", "-Command",
            "$ErrorActionPreference='Stop'; [Console]::OutputEncoding=[Text.Encoding]::UTF8; $r=@(" + script + "); ConvertTo-Json -InputObject $r -Depth 5 -Compress" })
            psi.ArgumentList.Add(arg);
        psi.StandardOutputEncoding = System.Text.Encoding.UTF8;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        using var p = new Process { StartInfo = psi };
        try
        {
            if (!p.Start()) return new(name, "unavailable", null, "Could not start provider.");
            var output = p.StandardOutput.ReadToEndAsync();
            var error = p.StandardError.ReadToEndAsync();
            await p.WaitForExitAsync(timeout.Token);
            await error;
            var json = await output;
            if (p.ExitCode != 0) return new(name, "unavailable", null, $"Provider exit {p.ExitCode}; access or feature may be unavailable.");
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
                return new(name, "unavailable", null, "No records returned.");
            return new(name, "available", doc.RootElement.Clone(), null);
        }
        catch (OperationCanceledException)
        {
            try { if (!p.HasExited) p.Kill(entireProcessTree: true); } catch (InvalidOperationException) {} catch (System.ComponentModel.Win32Exception) {}
            ct.ThrowIfCancellationRequested();
            return new(name, "unavailable", null, "Provider timed out.");
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or JsonException or IOException or InvalidOperationException)
        { return new(name, "unavailable", null, "Provider failed: " + ex.GetType().Name); }
    }
    public static IReadOnlyList<Finding> Evaluate(IEnumerable<Probe> probes)
    {
        var result = new List<Finding>();
        foreach (var probe in probes)
        {
            if (probe.State != "available" || probe.Data is not JsonElement data || data.ValueKind != JsonValueKind.Array)
            {
                result.Add(new(probe.Name + "-unknown", probe.Name, "Status unavailable; verify manually.", RiskLevel.Informational));
                continue;
            }
            foreach (var item in data.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                if (probe.Name == "defender" && item.TryGetProperty("AMRunningMode", out var mode) &&
                    mode.ValueKind == JsonValueKind.String && mode.GetString() == "Normal" &&
                    item.TryGetProperty("RealTimeProtectionEnabled", out var rt) && rt.ValueKind == JsonValueKind.False)
                    result.Add(new("defender-realtime-off", "security", "Defender reports real-time protection off in normal mode; review active antivirus.", RiskLevel.High));
                if (probe.Name == "firewall" && item.TryGetProperty("Enabled", out var en) &&
                    (en.ValueKind == JsonValueKind.False || (en.ValueKind == JsonValueKind.Number && en.TryGetInt32(out var n) && n == 0)))
                    result.Add(new("firewall-disabled", "security", "A firewall profile is disabled; review configuration.", RiskLevel.High));
                if (probe.Name == "storage" && item.TryGetProperty("Size", out var sz) && item.TryGetProperty("FreeSpace", out var fr) &&
                    sz.ValueKind == JsonValueKind.Number && fr.ValueKind == JsonValueKind.Number && sz.TryGetDouble(out var total) && fr.TryGetDouble(out var free) && total > 0 && free / total < 0.1)
                    result.Add(new("low-space", "storage", "A fixed drive has less than 10% free space. Review storage; no files were deleted.", RiskLevel.Moderate));
            }
        }
        return result;
    }
}
