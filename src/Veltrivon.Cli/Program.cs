using System.Text.Json;
using System.Text.Json.Serialization;
using Veltrivon.Windows;

if (args.Length != 1 || args[0] != "scan")
{
    Console.WriteLine("Veltrivon diagnostics MVP\nUsage: Veltrivon scan\nRead-only local scan; JSON report on stdout. No installation or repairs.");
    return args.Length == 0 ? 0 : 2;
}
if (!OperatingSystem.IsWindows()) { Console.Error.WriteLine("Windows required."); return 2; }
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
try
{
    var report = await new Diagnostics().ScanAsync(cts.Token);
    var options = new JsonSerializerOptions { WriteIndented = true };
    options.Converters.Add(new JsonStringEnumConverter());
    Console.WriteLine(JsonSerializer.Serialize(report, options));
    return report.Probes.Any(p => p.State != "available") ? 3 : 0;
}
catch (OperationCanceledException) { Console.Error.WriteLine("Scan cancelled."); return 130; }
