@echo off
setlocal EnableExtensions DisableDelayedExpansion
 title Veltrivon Windows Control Suite - Tool Installer
set "LOGDIR=%LOCALAPPDATA%\Veltrivon\Installer"
if not defined LOCALAPPDATA set "LOGDIR=%TEMP%\Veltrivon-Installer"
if not exist "%LOGDIR%" mkdir "%LOGDIR%"
set "LOG=%LOGDIR%\install-%RANDOM%-%RANDOM%.log"
set "MODE=search"
set "FAIL=0"
set "OK=0"
where winget.exe >nul 2>&1
if errorlevel 1 (
 echo WinGet is missing. Install or update Microsoft App Installer from Microsoft Store.
 echo https://learn.microsoft.com/windows/package-manager/winget/
 pause
 exit /b 2
)
echo Veltrivon Windows Control Suite - third-party application installer
 echo 1. Search all packages only [no installations]
 echo 2. Install essential workstation tools
 echo 3. Install extended diagnostic and developer tools
 echo 4. Install XR and media tools
 echo 5. Install all installation profiles
 echo 6. Review optional or conflicting apps and official links
 echo 7. Exit
 echo 8. Install essential developer tools
 echo 9. Install open-source-first XR tools
choice /c 123456789 /n /m "Select [1-9]: "
set "PICK=%ERRORLEVEL%"
if "%PICK%"=="7" exit /b 0
if "%PICK%"=="6" goto optional
if "%PICK%"=="1" goto run
 echo Installers may request administrator rights or user input.
 echo Some tools are proprietary or may offer paid plans. Review their terms.
 echo This uses WinGet's official community source and exact IDs only.
 echo Existing apps are skipped where detected. No cleaning or tuning is performed.
choice /c YN /n /m "Proceed and accept package/source agreements? [Y/N]: "
if errorlevel 2 exit /b 0
set "MODE=install"
:run
 echo Mode=%MODE% Profile=%PICK% > "%LOG%"
 winget source update --name winget >> "%LOG%" 2>&1
if "%PICK%"=="1" call :pkg "Kudu" "AdventDevelopmentInc.Kudu" "https://usekudu.com/download"
if "%PICK%"=="2" call :pkg "Kudu" "AdventDevelopmentInc.Kudu" "https://usekudu.com/download"
if "%PICK%"=="5" call :pkg "Kudu" "AdventDevelopmentInc.Kudu" "https://usekudu.com/download"
if "%PICK%"=="1" call :pkg "BCUninstaller" "Klocman.BulkCrapUninstaller" "https://www.bcuninstaller.com/"
if "%PICK%"=="2" call :pkg "BCUninstaller" "Klocman.BulkCrapUninstaller" "https://www.bcuninstaller.com/"
if "%PICK%"=="5" call :pkg "BCUninstaller" "Klocman.BulkCrapUninstaller" "https://www.bcuninstaller.com/"
if "%PICK%"=="1" call :pkg "UniGetUI" "Devolutions.UniGetUI" "https://github.com/Devolutions/UniGetUI"
if "%PICK%"=="2" call :pkg "UniGetUI" "Devolutions.UniGetUI" "https://github.com/Devolutions/UniGetUI"
if "%PICK%"=="5" call :pkg "UniGetUI" "Devolutions.UniGetUI" "https://github.com/Devolutions/UniGetUI"
if "%PICK%"=="1" call :pkg "Sysinternals Suite" "Microsoft.Sysinternals.Suite" "https://learn.microsoft.com/sysinternals/downloads/sysinternals-suite"
if "%PICK%"=="2" call :pkg "Sysinternals Suite" "Microsoft.Sysinternals.Suite" "https://learn.microsoft.com/sysinternals/downloads/sysinternals-suite"
if "%PICK%"=="5" call :pkg "Sysinternals Suite" "Microsoft.Sysinternals.Suite" "https://learn.microsoft.com/sysinternals/downloads/sysinternals-suite"
if "%PICK%"=="1" call :pkg "System Informer" "WinsiderSS.SystemInformer" "https://systeminformer.dev/downloads"
if "%PICK%"=="2" call :pkg "System Informer" "WinsiderSS.SystemInformer" "https://systeminformer.dev/downloads"
if "%PICK%"=="5" call :pkg "System Informer" "WinsiderSS.SystemInformer" "https://systeminformer.dev/downloads"
if "%PICK%"=="1" call :pkg "HWiNFO" "REALiX.HWiNFO" "https://www.hwinfo.com/download/"
if "%PICK%"=="2" call :pkg "HWiNFO" "REALiX.HWiNFO" "https://www.hwinfo.com/download/"
if "%PICK%"=="5" call :pkg "HWiNFO" "REALiX.HWiNFO" "https://www.hwinfo.com/download/"
if "%PICK%"=="1" call :pkg "CrystalDiskInfo" "CrystalDewWorld.CrystalDiskInfo" "https://crystalmark.info/"
if "%PICK%"=="2" call :pkg "CrystalDiskInfo" "CrystalDewWorld.CrystalDiskInfo" "https://crystalmark.info/"
if "%PICK%"=="5" call :pkg "CrystalDiskInfo" "CrystalDewWorld.CrystalDiskInfo" "https://crystalmark.info/"
if "%PICK%"=="1" call :pkg "Everything" "voidtools.Everything" "https://www.voidtools.com/"
if "%PICK%"=="2" call :pkg "Everything" "voidtools.Everything" "https://www.voidtools.com/"
if "%PICK%"=="5" call :pkg "Everything" "voidtools.Everything" "https://www.voidtools.com/"
if "%PICK%"=="1" call :pkg "WizTree" "AntibodySoftware.WizTree" "https://diskanalyzer.com/"
if "%PICK%"=="2" call :pkg "WizTree" "AntibodySoftware.WizTree" "https://diskanalyzer.com/"
if "%PICK%"=="5" call :pkg "WizTree" "AntibodySoftware.WizTree" "https://diskanalyzer.com/"
if "%PICK%"=="1" call :pkg "7-Zip" "7zip.7zip" "https://www.7-zip.org/"
if "%PICK%"=="2" call :pkg "7-Zip" "7zip.7zip" "https://www.7-zip.org/"
if "%PICK%"=="5" call :pkg "7-Zip" "7zip.7zip" "https://www.7-zip.org/"
if "%PICK%"=="1" call :pkg "PowerToys" "Microsoft.PowerToys" "https://github.com/microsoft/PowerToys"
if "%PICK%"=="2" call :pkg "PowerToys" "Microsoft.PowerToys" "https://github.com/microsoft/PowerToys"
if "%PICK%"=="5" call :pkg "PowerToys" "Microsoft.PowerToys" "https://github.com/microsoft/PowerToys"
if "%PICK%"=="1" call :pkg "DevToys" "DevToys-app.DevToys" "https://devtoys.app/"
if "%PICK%"=="2" call :pkg "DevToys" "DevToys-app.DevToys" "https://devtoys.app/"
if "%PICK%"=="5" call :pkg "DevToys" "DevToys-app.DevToys" "https://devtoys.app/"
if "%PICK%"=="1" call :pkg "Windows Terminal" "Microsoft.WindowsTerminal" "https://github.com/microsoft/terminal"
if "%PICK%"=="2" call :pkg "Windows Terminal" "Microsoft.WindowsTerminal" "https://github.com/microsoft/terminal"
if "%PICK%"=="5" call :pkg "Windows Terminal" "Microsoft.WindowsTerminal" "https://github.com/microsoft/terminal"
if "%PICK%"=="8" call :pkg "Windows Terminal" "Microsoft.WindowsTerminal" "https://github.com/microsoft/terminal"
if "%PICK%"=="1" call :pkg "PowerShell 7" "Microsoft.PowerShell" "https://github.com/PowerShell/PowerShell"
if "%PICK%"=="2" call :pkg "PowerShell 7" "Microsoft.PowerShell" "https://github.com/PowerShell/PowerShell"
if "%PICK%"=="5" call :pkg "PowerShell 7" "Microsoft.PowerShell" "https://github.com/PowerShell/PowerShell"
if "%PICK%"=="8" call :pkg "PowerShell 7" "Microsoft.PowerShell" "https://github.com/PowerShell/PowerShell"
if "%PICK%"=="1" call :pkg "Bitwarden" "Bitwarden.Bitwarden" "https://bitwarden.com/download/"
if "%PICK%"=="2" call :pkg "Bitwarden" "Bitwarden.Bitwarden" "https://bitwarden.com/download/"
if "%PICK%"=="5" call :pkg "Bitwarden" "Bitwarden.Bitwarden" "https://bitwarden.com/download/"
if "%PICK%"=="1" call :pkg "FreeFileSync" "FreeFileSync.FreeFileSync" "https://freefilesync.org/"
if "%PICK%"=="3" call :pkg "FreeFileSync" "FreeFileSync.FreeFileSync" "https://freefilesync.org/"
if "%PICK%"=="5" call :pkg "FreeFileSync" "FreeFileSync.FreeFileSync" "https://freefilesync.org/"
if "%PICK%"=="1" call :pkg "OCCT" "OCBase.OCCT" "https://www.ocbase.com/"
if "%PICK%"=="3" call :pkg "OCCT" "OCBase.OCCT" "https://www.ocbase.com/"
if "%PICK%"=="5" call :pkg "OCCT" "OCBase.OCCT" "https://www.ocbase.com/"
if "%PICK%"=="1" call :pkg "CrystalDiskMark" "CrystalDewWorld.CrystalDiskMark" "https://crystalmark.info/"
if "%PICK%"=="3" call :pkg "CrystalDiskMark" "CrystalDewWorld.CrystalDiskMark" "https://crystalmark.info/"
if "%PICK%"=="5" call :pkg "CrystalDiskMark" "CrystalDewWorld.CrystalDiskMark" "https://crystalmark.info/"
if "%PICK%"=="1" call :pkg "CPU-Z" "CPUID.CPU-Z" "https://www.cpuid.com/"
if "%PICK%"=="3" call :pkg "CPU-Z" "CPUID.CPU-Z" "https://www.cpuid.com/"
if "%PICK%"=="5" call :pkg "CPU-Z" "CPUID.CPU-Z" "https://www.cpuid.com/"
if "%PICK%"=="1" call :pkg "GPU-Z" "TechPowerUp.GPU-Z" "https://www.techpowerup.com/gpuz/"
if "%PICK%"=="3" call :pkg "GPU-Z" "TechPowerUp.GPU-Z" "https://www.techpowerup.com/gpuz/"
if "%PICK%"=="5" call :pkg "GPU-Z" "TechPowerUp.GPU-Z" "https://www.techpowerup.com/gpuz/"
if "%PICK%"=="1" call :pkg "Czkawka" "qarmin.czkawka" "https://github.com/qarmin/czkawka"
if "%PICK%"=="3" call :pkg "Czkawka" "qarmin.czkawka" "https://github.com/qarmin/czkawka"
if "%PICK%"=="5" call :pkg "Czkawka" "qarmin.czkawka" "https://github.com/qarmin/czkawka"
if "%PICK%"=="1" call :pkg "BleachBit" "BleachBit.BleachBit" "https://www.bleachbit.org/"
if "%PICK%"=="3" call :pkg "BleachBit" "BleachBit.BleachBit" "https://www.bleachbit.org/"
if "%PICK%"=="5" call :pkg "BleachBit" "BleachBit.BleachBit" "https://www.bleachbit.org/"
if "%PICK%"=="1" call :pkg "Files" "FilesCommunity.Files" "https://files.community/"
if "%PICK%"=="3" call :pkg "Files" "FilesCommunity.Files" "https://files.community/"
if "%PICK%"=="5" call :pkg "Files" "FilesCommunity.Files" "https://files.community/"
if "%PICK%"=="1" call :pkg "Double Commander" "DoubleCommander.DoubleCommander" "https://doublecmd.sourceforge.io/"
if "%PICK%"=="3" call :pkg "Double Commander" "DoubleCommander.DoubleCommander" "https://doublecmd.sourceforge.io/"
if "%PICK%"=="5" call :pkg "Double Commander" "DoubleCommander.DoubleCommander" "https://doublecmd.sourceforge.io/"
if "%PICK%"=="1" call :pkg "Syncthing" "Syncthing.Syncthing" "https://syncthing.net/"
if "%PICK%"=="3" call :pkg "Syncthing" "Syncthing.Syncthing" "https://syncthing.net/"
if "%PICK%"=="5" call :pkg "Syncthing" "Syncthing.Syncthing" "https://syncthing.net/"
if "%PICK%"=="1" call :pkg "Kopia" "Kopia.KopiaUI" "https://kopia.io/"
if "%PICK%"=="3" call :pkg "Kopia" "Kopia.KopiaUI" "https://kopia.io/"
if "%PICK%"=="5" call :pkg "Kopia" "Kopia.KopiaUI" "https://kopia.io/"
if "%PICK%"=="1" call :pkg "Flow Launcher" "Flow-Launcher.Flow-Launcher" "https://www.flowlauncher.com/"
if "%PICK%"=="3" call :pkg "Flow Launcher" "Flow-Launcher.Flow-Launcher" "https://www.flowlauncher.com/"
if "%PICK%"=="5" call :pkg "Flow Launcher" "Flow-Launcher.Flow-Launcher" "https://www.flowlauncher.com/"
if "%PICK%"=="1" call :pkg "AutoHotkey" "AutoHotkey.AutoHotkey" "https://www.autohotkey.com/"
if "%PICK%"=="3" call :pkg "AutoHotkey" "AutoHotkey.AutoHotkey" "https://www.autohotkey.com/"
if "%PICK%"=="5" call :pkg "AutoHotkey" "AutoHotkey.AutoHotkey" "https://www.autohotkey.com/"
if "%PICK%"=="1" call :pkg "ShareX" "ShareX.ShareX" "https://getsharex.com/"
if "%PICK%"=="3" call :pkg "ShareX" "ShareX.ShareX" "https://getsharex.com/"
if "%PICK%"=="5" call :pkg "ShareX" "ShareX.ShareX" "https://getsharex.com/"
if "%PICK%"=="1" call :pkg "ILSpy" "icsharpcode.ILSpy" "https://github.com/icsharpcode/ILSpy"
if "%PICK%"=="3" call :pkg "ILSpy" "icsharpcode.ILSpy" "https://github.com/icsharpcode/ILSpy"
if "%PICK%"=="5" call :pkg "ILSpy" "icsharpcode.ILSpy" "https://github.com/icsharpcode/ILSpy"
if "%PICK%"=="1" call :pkg "dnSpyEx" "dnSpyEx.dnSpy" "https://github.com/dnSpyEx/dnSpy"
if "%PICK%"=="3" call :pkg "dnSpyEx" "dnSpyEx.dnSpy" "https://github.com/dnSpyEx/dnSpy"
if "%PICK%"=="5" call :pkg "dnSpyEx" "dnSpyEx.dnSpy" "https://github.com/dnSpyEx/dnSpy"
if "%PICK%"=="1" call :pkg "HxD" "MHNexus.HxD" "https://mh-nexus.de/en/hxd/"
if "%PICK%"=="3" call :pkg "HxD" "MHNexus.HxD" "https://mh-nexus.de/en/hxd/"
if "%PICK%"=="5" call :pkg "HxD" "MHNexus.HxD" "https://mh-nexus.de/en/hxd/"
if "%PICK%"=="1" call :pkg "WinMerge" "WinMerge.WinMerge" "https://winmerge.org/"
if "%PICK%"=="3" call :pkg "WinMerge" "WinMerge.WinMerge" "https://winmerge.org/"
if "%PICK%"=="5" call :pkg "WinMerge" "WinMerge.WinMerge" "https://winmerge.org/"
if "%PICK%"=="1" call :pkg "Meld" "Meld.Meld" "https://meldmerge.org/"
if "%PICK%"=="3" call :pkg "Meld" "Meld.Meld" "https://meldmerge.org/"
if "%PICK%"=="5" call :pkg "Meld" "Meld.Meld" "https://meldmerge.org/"
if "%PICK%"=="1" call :pkg "DBeaver" "dbeaver.dbeaver" "https://dbeaver.io/"
if "%PICK%"=="3" call :pkg "DBeaver" "dbeaver.dbeaver" "https://dbeaver.io/"
if "%PICK%"=="5" call :pkg "DBeaver" "dbeaver.dbeaver" "https://dbeaver.io/"
if "%PICK%"=="1" call :pkg "Bruno" "Bruno.Bruno" "https://www.usebruno.com/"
if "%PICK%"=="3" call :pkg "Bruno" "Bruno.Bruno" "https://www.usebruno.com/"
if "%PICK%"=="5" call :pkg "Bruno" "Bruno.Bruno" "https://www.usebruno.com/"
if "%PICK%"=="1" call :pkg "Wireshark" "WiresharkFoundation.Wireshark" "https://www.wireshark.org/"
if "%PICK%"=="3" call :pkg "Wireshark" "WiresharkFoundation.Wireshark" "https://www.wireshark.org/"
if "%PICK%"=="5" call :pkg "Wireshark" "WiresharkFoundation.Wireshark" "https://www.wireshark.org/"
if "%PICK%"=="1" call :pkg "WinDbg" "Microsoft.WinDbg" "https://learn.microsoft.com/windows-hardware/drivers/debugger/"
if "%PICK%"=="3" call :pkg "WinDbg" "Microsoft.WinDbg" "https://learn.microsoft.com/windows-hardware/drivers/debugger/"
if "%PICK%"=="5" call :pkg "WinDbg" "Microsoft.WinDbg" "https://learn.microsoft.com/windows-hardware/drivers/debugger/"
if "%PICK%"=="1" call :pkg "RenderDoc" "RenderDoc.RenderDoc" "https://renderdoc.org/"
if "%PICK%"=="4" call :pkg "RenderDoc" "RenderDoc.RenderDoc" "https://renderdoc.org/"
if "%PICK%"=="9" call :pkg "RenderDoc" "RenderDoc.RenderDoc" "https://renderdoc.org/"
if "%PICK%"=="5" call :pkg "RenderDoc" "RenderDoc.RenderDoc" "https://renderdoc.org/"
if "%PICK%"=="1" call :pkg "PIX" "Microsoft.PIX" "https://devblogs.microsoft.com/pix/"
if "%PICK%"=="4" call :pkg "PIX" "Microsoft.PIX" "https://devblogs.microsoft.com/pix/"
if "%PICK%"=="5" call :pkg "PIX" "Microsoft.PIX" "https://devblogs.microsoft.com/pix/"
if "%PICK%"=="1" call :pkg "ADB Platform Tools" "Google.PlatformTools" "https://developer.android.com/tools/releases/platform-tools"
if "%PICK%"=="4" call :pkg "ADB Platform Tools" "Google.PlatformTools" "https://developer.android.com/tools/releases/platform-tools"
if "%PICK%"=="5" call :pkg "ADB Platform Tools" "Google.PlatformTools" "https://developer.android.com/tools/releases/platform-tools"
if "%PICK%"=="9" call :pkg "ADB Platform Tools" "Google.PlatformTools" "https://developer.android.com/tools/releases/platform-tools"
if "%PICK%"=="1" call :pkg "scrcpy" "Genymobile.scrcpy" "https://github.com/Genymobile/scrcpy"
if "%PICK%"=="4" call :pkg "scrcpy" "Genymobile.scrcpy" "https://github.com/Genymobile/scrcpy"
if "%PICK%"=="5" call :pkg "scrcpy" "Genymobile.scrcpy" "https://github.com/Genymobile/scrcpy"
if "%PICK%"=="9" call :pkg "scrcpy" "Genymobile.scrcpy" "https://github.com/Genymobile/scrcpy"
if "%PICK%"=="1" call :pkg "FFmpeg" "Gyan.FFmpeg" "https://ffmpeg.org/"
if "%PICK%"=="4" call :pkg "FFmpeg" "Gyan.FFmpeg" "https://ffmpeg.org/"
if "%PICK%"=="5" call :pkg "FFmpeg" "Gyan.FFmpeg" "https://ffmpeg.org/"
if "%PICK%"=="1" call :pkg "MediaInfo" "MediaArea.MediaInfo.GUI" "https://mediaarea.net/en/MediaInfo"
if "%PICK%"=="4" call :pkg "MediaInfo" "MediaArea.MediaInfo.GUI" "https://mediaarea.net/en/MediaInfo"
if "%PICK%"=="5" call :pkg "MediaInfo" "MediaArea.MediaInfo.GUI" "https://mediaarea.net/en/MediaInfo"
if "%PICK%"=="1" call :pkg "OBS Studio" "OBSProject.OBSStudio" "https://obsproject.com/"
if "%PICK%"=="4" call :pkg "OBS Studio" "OBSProject.OBSStudio" "https://obsproject.com/"
if "%PICK%"=="5" call :pkg "OBS Studio" "OBSProject.OBSStudio" "https://obsproject.com/"
if "%PICK%"=="9" call :pkg "OBS Studio" "OBSProject.OBSStudio" "https://obsproject.com/"
if "%PICK%"=="1" call :pkg "Apktool" "iBotPeaches.Apktool" "https://apktool.org/"
if "%PICK%"=="4" call :pkg "Apktool" "iBotPeaches.Apktool" "https://apktool.org/"
if "%PICK%"=="5" call :pkg "Apktool" "iBotPeaches.Apktool" "https://apktool.org/"
if "%PICK%"=="1" call :pkg "Git" "Git.Git" "https://git-scm.com/"
if "%PICK%"=="4" call :pkg "Git" "Git.Git" "https://git-scm.com/"
if "%PICK%"=="5" call :pkg "Git" "Git.Git" "https://git-scm.com/"
if "%PICK%"=="8" call :pkg "Git" "Git.Git" "https://git-scm.com/"
if "%PICK%"=="9" call :pkg "Git" "Git.Git" "https://git-scm.com/"
if "%PICK%"=="1" call :pkg "Malwarebytes Free" "Malwarebytes.Malwarebytes" "https://www.malwarebytes.com/"
if "%PICK%"=="1" call :pkg "AdwCleaner" "Malwarebytes.AdwCleaner" "https://www.malwarebytes.com/adwcleaner"
if "%PICK%"=="1" call :pkg "Advanced SystemCare" "IObit.AdvancedSystemCare" "https://www.iobit.com/en/advancedsystemcarefree.php"
if "%PICK%"=="1" call :pkg "Glary Utilities" "Glarysoft.GlaryUtilities" "https://www.glarysoft.com/"
if "%PICK%"=="1" call :pkg "PrivaZer" "Goversoft.PrivaZer" "https://privazer.com/"
if "%PICK%"=="1" call :pkg "Microsoft PC Manager" "Microsoft.PCManager" "https://pcmanager.microsoft.com/"
if "%PICK%"=="1" call :pkg "Process Lasso" "Bitsum.ProcessLasso" "https://bitsum.com/"
if "%PICK%"=="1" call :pkg "Fan Control" "Rem0o.FanControl" "https://getfancontrol.com/"
if "%PICK%"=="1" call :pkg "Portmaster" "Safing.Portmaster" "https://safing.io/"
if "%PICK%"=="1" call :pkg "Simplewall" "Henry++.simplewall" "https://github.com/henrypp/simplewall"
if "%PICK%"=="1" call :pkg "VeraCrypt" "IDRIX.VeraCrypt" "https://veracrypt.fr/"
if "%PICK%"=="1" call :pkg "Sandboxie Plus" "Sandboxie.Plus" "https://sandboxie-plus.com/"
if "%PICK%"=="1" call :pkg "VSCodium" "VSCodium.VSCodium" "https://github.com/VSCodium/vscodium"
if "%PICK%"=="8" call :pkg "VSCodium" "VSCodium.VSCodium" "https://github.com/VSCodium/vscodium"
if "%PICK%"=="5" call :pkg "VSCodium" "VSCodium.VSCodium" "https://github.com/VSCodium/vscodium"
if "%PICK%"=="1" call :pkg "Go" "GoLang.Go" "https://go.dev/"
if "%PICK%"=="8" call :pkg "Go" "GoLang.Go" "https://go.dev/"
if "%PICK%"=="5" call :pkg "Go" "GoLang.Go" "https://go.dev/"
if "%PICK%"=="1" call :pkg ".NET 10 SDK" "Microsoft.DotNet.SDK.10" "https://dotnet.microsoft.com/download/dotnet/10.0"
if "%PICK%"=="8" call :pkg ".NET 10 SDK" "Microsoft.DotNet.SDK.10" "https://dotnet.microsoft.com/download/dotnet/10.0"
if "%PICK%"=="5" call :pkg ".NET 10 SDK" "Microsoft.DotNet.SDK.10" "https://dotnet.microsoft.com/download/dotnet/10.0"
if "%PICK%"=="1" call :pkg "Node.js LTS" "OpenJS.NodeJS.LTS" "https://nodejs.org/"
if "%PICK%"=="8" call :pkg "Node.js LTS" "OpenJS.NodeJS.LTS" "https://nodejs.org/"
if "%PICK%"=="5" call :pkg "Node.js LTS" "OpenJS.NodeJS.LTS" "https://nodejs.org/"
if "%PICK%"=="1" call :pkg "Python 3.14" "Python.Python.3.14" "https://www.python.org/"
if "%PICK%"=="8" call :pkg "Python 3.14" "Python.Python.3.14" "https://www.python.org/"
if "%PICK%"=="5" call :pkg "Python 3.14" "Python.Python.3.14" "https://www.python.org/"
if "%PICK%"=="1" call :pkg "Eclipse Temurin 21 JDK" "EclipseAdoptium.Temurin.21.JDK" "https://adoptium.net/"
if "%PICK%"=="8" call :pkg "Eclipse Temurin 21 JDK" "EclipseAdoptium.Temurin.21.JDK" "https://adoptium.net/"
if "%PICK%"=="5" call :pkg "Eclipse Temurin 21 JDK" "EclipseAdoptium.Temurin.21.JDK" "https://adoptium.net/"
if "%PICK%"=="1" call :pkg "CMake" "Kitware.CMake" "https://cmake.org/"
if "%PICK%"=="8" call :pkg "CMake" "Kitware.CMake" "https://cmake.org/"
if "%PICK%"=="5" call :pkg "CMake" "Kitware.CMake" "https://cmake.org/"
if "%PICK%"=="1" call :pkg "Ninja" "Ninja-build.Ninja" "https://ninja-build.org/"
if "%PICK%"=="8" call :pkg "Ninja" "Ninja-build.Ninja" "https://ninja-build.org/"
if "%PICK%"=="5" call :pkg "Ninja" "Ninja-build.Ninja" "https://ninja-build.org/"
if "%PICK%"=="1" call :pkg "LLVM" "LLVM.LLVM" "https://llvm.org/"
if "%PICK%"=="8" call :pkg "LLVM" "LLVM.LLVM" "https://llvm.org/"
if "%PICK%"=="5" call :pkg "LLVM" "LLVM.LLVM" "https://llvm.org/"
if "%PICK%"=="1" call :pkg "GitHub CLI" "GitHub.cli" "https://cli.github.com/"
if "%PICK%"=="8" call :pkg "GitHub CLI" "GitHub.cli" "https://cli.github.com/"
if "%PICK%"=="5" call :pkg "GitHub CLI" "GitHub.cli" "https://cli.github.com/"
if "%PICK%"=="1" call :pkg "ripgrep" "BurntSushi.ripgrep.MSVC" "https://github.com/BurntSushi/ripgrep"
if "%PICK%"=="8" call :pkg "ripgrep" "BurntSushi.ripgrep.MSVC" "https://github.com/BurntSushi/ripgrep"
if "%PICK%"=="5" call :pkg "ripgrep" "BurntSushi.ripgrep.MSVC" "https://github.com/BurntSushi/ripgrep"
if "%PICK%"=="1" call :pkg "jq" "jqlang.jq" "https://jqlang.org/"
if "%PICK%"=="8" call :pkg "jq" "jqlang.jq" "https://jqlang.org/"
if "%PICK%"=="5" call :pkg "jq" "jqlang.jq" "https://jqlang.org/"
if "%PICK%"=="1" call :pkg "Godot" "GodotEngine.GodotEngine" "https://godotengine.org/"
if "%PICK%"=="9" call :pkg "Godot" "GodotEngine.GodotEngine" "https://godotengine.org/"
if "%PICK%"=="5" call :pkg "Godot" "GodotEngine.GodotEngine" "https://godotengine.org/"
if "%PICK%"=="1" call :pkg "Blender" "BlenderFoundation.Blender" "https://www.blender.org/"
if "%PICK%"=="9" call :pkg "Blender" "BlenderFoundation.Blender" "https://www.blender.org/"
if "%PICK%"=="5" call :pkg "Blender" "BlenderFoundation.Blender" "https://www.blender.org/"
echo.
echo Completed. Successful or already installed: %OK% ; unresolved or failed: %FAIL%
echo Log: "%LOG%"
if "%MODE%"=="search" echo Search only - no software was installed.
if "%PICK%"=="5" echo For excluded security and vendor-specific apps rerun option 6.
pause
if not "%FAIL%"=="0" exit /b 1
exit /b 0
:optional
echo These are not automatically installed due to overlapping functions, security drivers, or vendor-specific setup.
echo Microsoft Defender - https://support.microsoft.com/windows/stay-protected-with-windows-security-2ae0363d-0ada-c064-8b56-6a39afb6a963
echo Malwarebytes Free - https://www.malwarebytes.com/
echo Bitdefender Free - https://www.bitdefender.com/en-in/consumer/free-antivirus
echo AdwCleaner - https://www.malwarebytes.com/adwcleaner
echo ESET Online Scanner - https://www.eset.com/in/home/online-scanner/
echo Advanced SystemCare - https://www.iobit.com/en/advancedsystemcarefree.php
echo Glary Utilities - https://www.glarysoft.com/
echo PrivaZer - https://privazer.com/
echo Microsoft PC Manager - https://pcmanager.microsoft.com/
echo Process Lasso - https://bitsum.com/
echo Fan Control - https://getfancontrol.com/
echo Portmaster - https://safing.io/
echo Simplewall - https://github.com/henrypp/simplewall
echo VeraCrypt - https://veracrypt.fr/
echo Sandboxie Plus - https://sandboxie-plus.com/
echo Windows Performance Toolkit - https://learn.microsoft.com/windows-hardware/test/wpt/
echo GPUView - https://learn.microsoft.com/en-us/windows-hardware/drivers/display/using-gpuview
echo NVIDIA Nsight - https://developer.nvidia.com/tools-downloads
echo Meta Quest Developer Hub - https://developers.meta.com/vr/downloads/package/oculus-developer-hub-win/
echo ALVR - https://github.com/alvr-org/ALVR
echo OVR Metrics Tool - https://developers.meta.com/vr/resources/developer-tools/
echo Microsoft Defender is built into supported Windows versions. Do not run competing real-time antivirus products.
pause
exit /b 0
:pkg
 echo.
 echo [%~1] %~2
 echo [%~1] %~2 >> "%LOG%"
 winget search --id "%~2" --exact --source winget --accept-source-agreements >> "%LOG%" 2>&1
 if errorlevel 1 goto missing
 if "%MODE%"=="search" goto success
 winget list --id "%~2" --exact --source winget --accept-source-agreements >> "%LOG%" 2>&1
 if not errorlevel 1 goto installed
 winget install --id "%~2" --exact --source winget --accept-source-agreements --accept-package-agreements >> "%LOG%" 2>&1
 if errorlevel 1 goto missing
:success
 set /a OK+=1 >nul
 echo OK
 exit /b 0
:installed
 echo Already installed - skipping.
 echo Already installed - skipping. >> "%LOG%"
 set /a OK+=1 >nul
 exit /b 0
:missing
 echo Unresolved or failed. Official download: %~3
 echo Unresolved or failed. Official download: %~3 >> "%LOG%"
 set /a FAIL+=1 >nul
 exit /b 0
