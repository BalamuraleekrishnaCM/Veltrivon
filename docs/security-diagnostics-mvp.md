# Security and diagnostics MVP

## Implemented on this branch
A Windows-only .NET 10 diagnostic provider and console frontend. Ten read-only probes: Defender state, registered antivirus names, firewall profiles, fixed drive capacity, physical disk health, OS and memory, CPU, listening-port counts, startup count, installed registry application count. Reports are JSON on stdout; no upload, telemetry, repair, installation or automatic remediation. Installed app count is a registry entry count, not a deduplicated inventory. Probe status means data availability, not security health. Security Center registration does not prove active protection.

Run:
```powershell
dotnet run --project src/Veltrivon.Cli -- scan
dotnet run --project tests/Veltrivon.Diagnostics.Tests
dotnet publish src/Veltrivon.Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o artifacts/diagnostics
.\artifacts\diagnostics\Veltrivon.exe scan > report.json
```
Exit codes: 0 all probes returned records; 2 invalid usage or unsupported OS; 3 one or more probes unavailable; 130 cancelled. Exit 0 does not mean that findings are absent. A 30 second timeout applies per probe. Reports may reveal installed security products and network ports; review before sharing.

## Integration backlog (not implemented)
- WinUI 3 MVVM dashboard, scan navigation, cancellation and report view.
- Defender supported scan/update actions, approval flow and least-privilege broker; no disabling antivirus.
- ClamAV on-demand adapter and YARA-X custom rules; no independent real-time AV claims.
- Portmaster/simplewall optional detection and official link; avoid overlapping firewalls or automatic network changes.
- Bitwarden/KeePassXC, Sandboxie Plus, Gitleaks, Trivy, Kopia optional tool detection and launch.
- DISM/SFC/CHKDSK preview-first repair plans; no automatic destructive actions.
- WinGet exact-ID package integration with explicit consent.
- SQLite audit, signed binary, SBOM, dependencies/license review.

## Acceptance
Windows CI builds the provider and CLI, runs synthetic regression cases, publishes a self-contained unsigned console EXE and smoke-tests help. Live probes must be exercised separately on Windows 11 with Defender active, third-party antivirus active, limited user and offline conditions. Verify cancellation, timeout, missing providers and reports; no system changes allowed. No claim of equivalent protection to commercial security suites.
