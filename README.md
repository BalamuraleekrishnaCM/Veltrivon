# Veltrivon — Windows Control Suite
Diagnose. Optimize. Protect. Develop.

Local-first, modular Windows workstation suite for PC maintenance, security posture, system diagnostics and development. The first desktop build provides a tool catalog, package installation, app launching, and read-only diagnostics.

## Start here
- [Architecture](docs/architecture.md)
- [Feature inventory and roadmap](docs/features.md)
- [UI layout](docs/layout.md)
- [Installer catalog](installer/catalog.json)
- [XR Dev Manager](xr-dev-manager/README.md)

## Windows desktop app

Open `artifacts\Veltrivon.App.exe` on Windows 11 x64. Use **Tool catalog** to filter by profile, select tools, check WinGet availability, and review an installation before accepting package agreements. Use **Run tool** to open an installed app. If its name differs from the catalog or it has no Start menu entry, choose its executable. ADB device inventory, Git version, and FFmpeg version display their results in **Activity**.

Optional and manual profiles provide publisher download links. **Diagnostics** shows workstation inventory and runs read-only checks for disk health, recent system errors, and startup entries. **Run smart scan** runs all checks with a cancel option. **Activity** can export a summary with check names and statuses. Full results remain available in Activity and under `%LOCALAPPDATA%\Veltrivon\Jobs`, where logs can contain Windows event details and local paths.

To publish the desktop app from source, use the .NET 10 SDK on Windows:

```powershell
dotnet publish src/Veltrivon.App/Veltrivon.App.csproj -c Release -p:DebugType=embedded -o artifacts
```

Keep the published files together in the artifacts folder. The desktop executable and third-party installers are separate. Microsoft App Installer supplies WinGet, and package operations use the WinGet community source.

## Utility installer
Run `installer/Veltrivon-Install.bat` on Windows 11 with App Installer/WinGet and internet. Or build the standalone console EXE:

```powershell
cd installer
go build -trimpath -ldflags="-s -w" -o Veltrivon-Install.exe .
```

Choose **8** for developer essentials or **9** for the open-source-first XR toolchain. Developer essentials include Git, VSCodium, Go, .NET, Node.js LTS, Python, Temurin JDK, CMake, Ninja, LLVM, GitHub CLI, ripgrep, jq, PowerShell, and Windows Terminal. The XR toolchain adds Android Platform Tools, scrcpy, RenderDoc, OBS Studio, Godot, and Blender. Choose **5** for all installation profiles or **6** for manual and optional tools, including ALVR.

The installer resolves exact package IDs against the WinGet community source and reports unavailable packages with publisher links. Individual installers may require elevation, interaction, or a restart. Search-only mode updates and queries the WinGet source. Logs are stored under `%LOCALAPPDATA%\Veltrivon\Installer` or the temporary directory.

## Status and safety
The standalone installer is unsigned. Both installer menus passed an exit-only smoke test on Windows. Live package installation and publisher verification are pending. Third-party applications retain their own licenses.
