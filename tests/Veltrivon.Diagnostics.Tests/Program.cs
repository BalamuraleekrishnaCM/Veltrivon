using System.Text.Json;
using Veltrivon.Windows;
using Veltrivon.Core;
static Probe P(string name, string json) { using var doc = JsonDocument.Parse(json); return new(name, "available", doc.RootElement.Clone(), null); }
static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
var findings = Diagnostics.Evaluate(new[] {
    P("defender", """[{"AMRunningMode":"Normal","RealTimeProtectionEnabled":false}]"""),
    P("firewall", """[{"Enabled":0}]"""),
    P("storage", """[{"Size":100,"FreeSpace":9}]"""),
    new Probe("physical-disk", "unavailable", null, "test")
});
Check(findings.Count == 4, "Expected four findings");
Check(findings.Count(x => x.Risk == RiskLevel.High) == 2, "Expected two high risk findings");
Check(findings.Any(x => x.Id == "low-space"), "Low space detection");
Check(findings.Any(x => x.Id == "physical-disk-unknown"), "Unavailable must not be classified healthy");
Check(Diagnostics.Evaluate(new[] {
    P("defender", """[{"AMRunningMode":"Passive Mode","RealTimeProtectionEnabled":false}]"""),
    P("firewall", """[{"Enabled":1}]"""),
    P("storage", """[{"Size":100,"FreeSpace":10}]""")
}).Count == 0, "Passive mode and normal storage must not trigger findings");
Check(Diagnostics.Evaluate(new[] { P("storage", """[{"Size":0,"FreeSpace":0},{"Size":null,"FreeSpace":null},null]""") }).Count == 0, "Invalid storage values");
Console.WriteLine("Diagnostics regression checks passed.");
