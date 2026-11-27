# Architecture — Veltrivon v0.1

## Technology decision
Windows 11 x64 initially; .NET 10 LTS and WinUI 3 / Windows App SDK for desktop UI, MVVM, dependency injection, async streams, SQLite for local history, JSON for settings. Windows 10/ARM64 only after compatibility verification. Use supported Windows APIs first; optional CLI adapters for external tools. The Go installer is independent of the future desktop application. No permanent elevated process by default.

```mermaid
flowchart TD
 UI[WinUI desktop / MVVM] --> APP[Application use cases]
 APP --> CORE[Domain models and policies]
 APP --> JOB[Job coordinator and cancellation]
 JOB --> PORT[Provider interfaces]
 PORT --> WIN[Windows APIs / WMI / ETW]
 PORT --> PKG[WinGet / Scoop / Chocolatey adapters]
 PORT --> DEV[Git / ADB / Unity / FFmpeg adapters]
 PORT --> SEC[Defender and firewall status]
 APP --> DATA[SQLite / JSON / logs]
 JOB --> BROKER[On-demand elevated broker]
 BROKER --> WIN
```

## Boundaries
- `Veltrivon.App`: navigation, settings, accessibility, views and view models; unelevated.
- `Veltrivon.Core`: immutable findings, action plans, risk levels, typed module contracts; no Windows dependencies.
- `Veltrivon.Application`: scan orchestration, permissions, action approval and audit.
- `Veltrivon.Windows`: signed Windows provider adapters, structured queries, command runners with argument lists, timeouts and exit codes.
- `Veltrivon.Broker`: on-demand administrator process, authenticated per-user IPC; validate allowlisted operation IDs and typed arguments; no arbitrary shell execution.
- `Veltrivon.Infrastructure`: local SQLite repository, migrations, logs, redaction, settings and export.
- `Veltrivon.Modules.*`: built-in feature packages; optional signed plugins later, isolated out-of-process when appropriate.
- `installer`: independent, embedded manifest CLI installer with WinGet source and exact-ID search.

## Operation flow
Discover -> scan -> finding -> proposed action -> preview/impact -> consent -> privilege check -> execute -> verify -> audit -> rollback where supported. Read-only scan by default. Revalidate stale plans immediately before writes. Every operation supports cancellation when the underlying tool does; do not terminate DISM or updates blindly. Store errors, exit codes, duration and redacted output. Reboots require explicit approval.

## Provider contracts
`IScanner.ScanAsync(context, cancellationToken)` returns findings. `IActionProvider.PlanAsync(finding)` returns typed action plan with targets, risk, privileges, dependencies, expected effect and reversibility. `ExecuteAsync(approvedPlan)` returns a structured result. `VerifyAsync` checks postconditions; `RollbackAsync` is optional and must not promise reversibility for arbitrary operations. `IPackageProvider` exposes search, inventory, install and update using exact identities. `ITelemetryProvider` streams bounded samples. `IReportStore` saves redacted local history.

## Safety and security
Protect Unity Library, SDKs, Gradle, Git, NuGet, user documents and downloads from default cleanup; never recursively follow junctions/reparse points. No generic registry cleaner, disabling security features, RAM purges, silent driver updates, automatic startup changes, or unsupported antivirus claims. Use Defender/Windows Security APIs and supported commands. No credential capture, no default cloud telemetry. Respect permissions, validate paths, use least privilege, sign releases, publish SHA-256/SBOM; check dependency licenses (especially GPL) before redistribution or linking. Restore points are optional and not guaranteed to undo file deletion. Use a reviewable quarantine for eligible files. Do not run two antivirus engines. Hardware stress tests require explicit confirmation and thermal stop thresholds.
