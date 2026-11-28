# Veltrivon Windows preview

This preview adds a workstation overview, read-only checks for Defender, Firewall, BitLocker, and Secure Boot, and a WinGet view for installed software and available updates. **Test selected** verifies tool installations and checks versions for supported command-line tools. The developer profile adds Neovim, fzf, uv, fd, bat, and Bun.

| Download | Contents |
| --- | --- |
| `Veltrivon-Setup-win-x64.exe` | Per-user setup with Start menu entries and an uninstaller. Includes the desktop app, XR Dev Manager, and the developer tool installer. |
| `Veltrivon-Portable-win-x64.zip` | Extract the full folder and run the desktop app or bundled tools. |
| `XR-Dev-Manager-win-x64.exe` | Self-contained XR Dev Manager command-line executable. Run it with `--help`. |
| `Veltrivon-Tools-win-x64.exe` | Standalone WinGet tool installer. Choose option 8 for developer essentials and 9 for XR tools. |
| `Veltrivon-Batch-Installer.zip` | Batch installer and its package catalog. |
| `SHA256SUMS.txt` | SHA-256 checksums for the downloads. |

GitHub also provides source ZIP and tar.gz archives. The preview targets Windows 11 x64. The executables are unsigned. Installing third-party tools requires WinGet, and XR hardware commands use separately installed ADB and scrcpy.
