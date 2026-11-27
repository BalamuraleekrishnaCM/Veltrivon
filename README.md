# Veltrivon — Windows Control Suite
Diagnose. Optimize. Protect. Develop.

Local-first, modular Windows workstation suite for PC maintenance, security posture, system diagnostics and development. Status: product specification and installer prototype (v0.1.0), not a finished desktop suite.

## Start here
- [Architecture](docs/architecture.md)
- [Feature inventory and roadmap](docs/features.md)
- [UI layout](docs/layout.md)
- [Installer catalog](installer/catalog.json)

## Utility installer
Run `installer/Veltrivon-Install.bat` on Windows 11 with App Installer/WinGet and internet. Or build the standalone console EXE:

```powershell
cd installer
go build -trimpath -ldflags="-s -w" -o Veltrivon-Install.exe .
```

Choose search-only first, or choose essential, extended, XR/media, or all three installation profiles. Optional/conflicting and manual tools are listed separately with official download links. Package IDs are candidates checked at runtime; missing packages are reported, never replaced with a fuzzy match. Some installers need user interaction, elevation, additional license acceptance, or a restart. No third-party executables are bundled. No cleaning, security configuration or system tuning is performed. The search-only mode updates/query WinGet sources but installs no applications. Log files are stored under `%LOCALAPPDATA%\Veltrivon\Installer` (or temporary directory). WinGet and network access are required; other package-provider adapters are a planned product feature, not implemented in this installer.

## Status and safety
The standalone installer is unsigned and has not been run on Windows. Verify catalog IDs and publisher information before installing. The desktop app is not implemented. Trademark rights are not implied. Third-party applications retain their own licenses; integration does not grant redistribution rights.
