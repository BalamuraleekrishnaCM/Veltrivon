# Feature map and delivery plan

| ID | Module | Reference tools | Functions | Phase |
|---|---|---|---|---|
| M01 | Dashboard | Kudu, PC Manager, ASC | inventory, metrics, findings, job status | 1 |
| M02 | Smart Scan | Kudu, Glary | read-only checks, findings, severity, report | 1 |
| M03 | Cleaner | BleachBit, PrivaZer, ASC | temp files, caches, preview, exclusions, quarantine | 2 |
| M04 | Apps | BCUninstaller | installed inventory, uninstall queue, leftover review | 2 |
| M05 | Packages | UniGetUI | exact-ID search, install, updates, profiles, logs | 1 |
| M06 | Repair | Sysinternals, ASC | DISM, SFC, CHKDSK scan, Windows Update checks | 2 |
| M07 | Performance | System Informer, Process Lasso | process stats, CPU, RAM, GPU, priorities, affinity | 3 |
| M08 | Startup | Autoruns, Kudu | startup entries, tasks, services, backup/review | 2 |
| M09 | Hardware | HWiNFO, CPU-Z, GPU-Z, OCCT, Fan Control | inventory, sensors, stress tests, cooling status | 3 |
| M10 | Storage | Everything, WizTree, CrystalDiskInfo, Czkawka, CrystalDiskMark | tree sizes, search, SMART, duplicate hashes, benchmarks | 2–3 |
| M11 | Security | Defender, Bitdefender, Malwarebytes, AdwCleaner | security posture, signatures, supported scans, firewall status | 2 |
| M12 | Privacy | BleachBit, PrivaZer, Bitwarden | data cleanup, permissions review, password-manager launch | 3 |
| M13 | Network | TCPView, Wireshark, Portmaster, Simplewall | ports, DNS, connectivity, per-app visibility, firewall review | 3 |
| M14 | Drivers | Device Manager, Windows Update | device IDs, versions, signature status, updates review | 3 |
| M15 | Backup | FreeFileSync, Syncthing, Kopia, VeraCrypt | backup profiles, dry run, restore verification, encrypted-store launch | 3 |
| M16 | Dev toolbox | DevToys, PowerToys, ILSpy, dnSpyEx, HxD, WinMerge, Meld, Bruno, DBeaver | JSON/XML, regex, hashing, UUID, diff, API tools, external launchers | 2 |
| M17 | SDK | WinGet, Terminal, PowerShell | SDK inventory, versions, PATH diagnostics, install plans | 2 |
| M18 | Git | Git | repo discovery, status, branches, worktrees, explicit actions | 2 |
| M19 | Unity | Unity Hub | project discovery, editor/SDK compatibility, cache preview | 3 |
| M20 | XR | MQDH, ADB, scrcpy, Apktool, OVR Metrics | devices, APK deploy, logs, mirroring, metrics | 3 |
| M21 | GPU | PIX, Nsight, RenderDoc, GPUView, WPT | profiler launch, traces, metrics, workload profiles | 4 |
| M22 | Media | FFmpeg, OBS, MediaInfo | inspect, convert, capture/stream launch | 4 |
| M23 | Automation | AutoHotkey, Task Scheduler | workflows, consent, scheduling, job journal | 4 |
| M24 | Reports | WinDbg, Sysinternals, ShareX | export evidence, error history, redaction | 1 |
| M25 | Toolbox/settings | Flow Launcher, Files, 7-Zip, Double Commander, Sandboxie | tool launch, themes, preferences, isolation utility launch | 1–4 |

These are planned capabilities, not claims that each reference tool implements every feature or that all modules currently exist. Vendor-specific functionality is integrated only through documented interfaces and compatible licenses.

## Milestones / acceptance
- M0: architecture, manifest, installer prototype, Git history. Windows installer smoke test pending.
- M1: build WinUI shell + core contracts + read-only scan + exact package search + report. No writes by default; test cancellation and offline mode.
- M2: preview-first cleaner, app inventory, repair, startup, SDK and developer tools. Test path exclusions, stale plan rejection, UAC denial and exit codes.
- M3: hardware, network, storage, security, backup, Unity and XR. Test unsupported devices, sensors and failed operations; confirm thermal and destructive actions.
- M4: optional integrations, automation, signed distribution and accessibility. CI: Windows x64 build, unit/integration tests, signed artifacts, license scan, SBOM, security review.
