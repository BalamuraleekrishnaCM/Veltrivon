# XR Dev Manager

MIT-licensed Windows command-line application for developing and testing on Quest and other authorized Android XR devices. Use ADB for device management and deployment, scrcpy for previews and recordings, and an installed PC VR launcher for streaming and runtime setup.

## Set up

1. Run Veltrivon's installer and choose **8** for developer essentials and **9** for open-source-first XR tools. Open a new terminal so it sees the updated PATH.
2. Enable developer mode on the headset, connect USB, and accept its USB debugging prompt. Use the model-specific vendor setup for required USB drivers.
3. Build XR Dev Manager with the .NET 10 SDK.

```powershell
dotnet publish ./xr-dev-manager/XrDevManager.csproj -c Release -o ./xr-dev-manager/bin/publish
$xr = './xr-dev-manager/bin/publish/xr-dev-manager.exe'
& $xr doctor
& $xr devices
```

If an executable is outside PATH, set `XRDEV_ADB` or `XRDEV_SCRCPY` to its absolute path. ADB handles device authorization. The devices command displays unauthorized and offline devices alongside ready devices.

## Workflows

| Command | What it does |
| --- | --- |
| `devices` | List USB devices and devices already connected through ADB over Wi-Fi. |
| `pair IP:PORT` | Pair with the endpoint shown in the headset's wireless debugging pairing dialog. ADB prompts for the code. |
| `connect IP:PORT` | Connect using the wireless debugging connection endpoint, which may have a different port. |
| `apps SERIAL` | List third-party packages on the chosen device. |
| `install SERIAL "C:\Builds\Demo.apk"` | Install a local APK or update an existing install with matching signing. |
| `logs SERIAL` | Stream Logcat to the terminal until Ctrl+C. |
| `mirror SERIAL` | Open a read-only scrcpy preview. |
| `record SERIAL "C:\Captures\Demo.mp4"` | Preview and record video. Close the preview window to finalize the file. |
| `doctor` | Show executable availability and inspect the Windows OpenXR registration and process override. |
| `pcvr "C:\Tools\ALVR\ALVR Dashboard.exe"` | Open an installed launcher using its actual executable path. |

Preview and recording depend on the headset firmware and which display ADB exposes. Protected or compositor-only surfaces may be unavailable. Logcat can contain application and device data.

## PC VR

[ALVR](https://github.com/alvr-org/ALVR) supplies the streaming software and has its own headset and PC setup. XR Dev Manager can open its launcher and report the presence of an OpenXR manifest. The launcher also accepts a locally installed vendor executable. Device pairing, streaming configuration, runtime selection, and the video transport are handled in the chosen PC VR software.

## Architecture

The command layer accepts local paths and explicit device serials. It invokes [ADB](https://developer.android.com/tools/adb) and [scrcpy](https://github.com/Genymobile/scrcpy) with individual process arguments and propagates tool exit codes. It reads the Windows runtime registration without elevation. The project builds independently of the Veltrivon desktop app and uses the shared installer catalog for tooling setup.

Wireless pairing requires headset firmware that exposes Android wireless debugging. Future work includes a graphical workspace, multi-APK deployment, curated app discovery, and frame timing overlays.
