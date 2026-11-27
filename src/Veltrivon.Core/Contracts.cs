namespace Veltrivon.Core;
public enum RiskLevel { Informational, Low, Moderate, High }
public sealed record Finding(string Id, string ModuleId, string Description, RiskLevel Risk);
public sealed record ActionPlan(string Id, string FindingId, string OperationId, IReadOnlyList<string> Targets, RiskLevel Risk, bool RequiresElevation, bool SupportsRollback);
public sealed record OperationResult(bool Succeeded, string Summary, int? ExitCode);
public interface IScanner { IAsyncEnumerable<Finding> ScanAsync(CancellationToken cancellationToken); }
public interface IActionPlanner { Task<ActionPlan> PlanAsync(Finding finding, CancellationToken cancellationToken); }
