# WinAdapterSwitcher

**English** | [简体中文](docs/README.zh-CN.md)

A fast network adapter switcher for Windows 10 / 11. It stays in the system tray and lets you switch between wired and wireless networks with one right-click, or enable/disable any adapter individually.

- Single-file portable app: `WinAdapterSwitcher.exe`, no installation required
- Target platform: Windows 10 / Windows 11
- Runtime: the built-in .NET Framework 4.8, no extra installation
- No network access, no telemetry, no ads

## Why I built this

My ISP regularly causes DNS poisoning, blocks connections, or resets them outright. When that happens, I need to switch to a mobile hotspot quickly. But manually disabling and enabling adapters in Windows means going through Settings → Network & Internet → Advanced network settings → More network adapter options → right-click the adapter → Disable/Enable — far too many steps when I just want to get back online.

I defined the requirements and workflow, then used AI to help implement this tool. The result puts wired/wireless switching and adapter control into a tray menu, so common cases take one click.

## Features

- Tray icon shows the current wired/wireless adapter state (enabled, disabled, or unknown)
- One-click modes:
  - Wired network only
  - Wireless network only
  - Enable all networks
  - Disable all networks (confirmation enabled by default)
- Enable or disable any adapter individually
- Pick the wired and wireless adapters in Settings; first run detects and saves them automatically
- UI available in Simplified Chinese, Traditional Chinese, English, Japanese, Korean, German, French, and Spanish; follows the system language on first run and can be changed manually
- Start with Windows via a scheduled task that runs with highest privileges at sign-in
- Optional logging and optional auto-start task cleanup on exit
- Command-line arguments for shortcuts, scheduled tasks, and scripts
- Single-instance behavior: launching it again wakes the existing instance

## Usage

1. Download and extract the release zip.
2. Run `WinAdapterSwitcher.exe`. The app declares `requireAdministrator`, so Windows shows a UAC prompt.
3. The app appears in the system tray. Right-click the tray icon to open the menu.

> Enabling/disabling network adapters requires administrator privileges on Windows. The app requests elevation at startup through `app.manifest`.

### Tray menu

| Item | Description |
| --- | --- |
| Status line | Current state of the configured wired/wireless adapters |
| Wired network only | Enable wired, disable wireless |
| Wireless network only | Enable wireless, disable wired |
| Enable all networks | Enable all configured adapters |
| Disable all networks | Disable all configured adapters (confirmation dialog by default) |
| Enable/disable individual | List all adapters and toggle them individually |
| Refresh status | Re-read adapter state immediately |
| Restart as administrator | Shown only when the app is not elevated |
| Settings... | Choose adapters, auto-start, logging, and more |
| Exit | Quit the app |

Double-click the tray icon to open Settings.

### Settings

- **Wired adapter / Wireless adapter**: choose from the adapters on the system; the name and description are saved
- **UI language**: Automatic (follow the system) or one of the 8 supported languages
- **Confirm before “Disable all networks”**: enabled by default
- **Enable log file**: disabled by default; writes `WinAdapterSwitcher.log` next to the exe
- **Remove the auto-start task when exiting**: disabled by default
- **Start with Windows**: creates a per-user logon scheduled task that runs with highest privileges

### Configuration

Configuration is stored in **`config.ini` next to the exe**. Moving the whole program folder keeps the configuration working. Example:

```ini
; WinAdapterSwitcher configuration
; This file is stored next to WinAdapterSwitcher.exe.

[Adapters]
Wired=Ethernet
WiredDescription=Realtek PCIe GbE Family Controller
Wireless=Wi-Fi
WirelessDescription=Intel(R) Wi-Fi 6 AX201 160MHz

[Options]
Language=auto
ConfirmDisableAll=1
EnableLog=0
CleanAutoStartOnExit=0
```

`Language` accepts `auto`, `zh-CN`, `zh-TW`, `en-US`, `ja-JP`, `ko-KR`, `de-DE`, `fr-FR`, and `es-ES`. `auto` follows the system UI language and falls back to English when the language is not recognized.

If a configured adapter name no longer exists on the system, the tray status shows “Not found” and any action prompts you to reconfigure it. The app does not crash.

## Command line

Command-line arguments are intended for shortcuts, scheduled tasks, and scripts. If the app is already running, a new instance forwards the command to the existing instance. Because this is a GUI app, standalone command results are shown in a dialog.

```text
WinAdapterSwitcher.exe                 Start the tray app
WinAdapterSwitcher.exe --wired         Wired network only
WinAdapterSwitcher.exe --wifi          Wireless network only
WinAdapterSwitcher.exe --enable NAME   Enable the specified adapter
WinAdapterSwitcher.exe --disable NAME  Disable the specified adapter
WinAdapterSwitcher.exe --all-enable    Enable all networks
WinAdapterSwitcher.exe --all-disable   Disable all networks
WinAdapterSwitcher.exe --settings      Open Settings
WinAdapterSwitcher.exe --menu          Open the tray menu
WinAdapterSwitcher.exe --help          Show help
```

## Building

Building **does not require Visual Studio, the .NET SDK, Rust, Go, Node.js, or any package manager**, and does not require an internet connection. Everything is built locally in one step, and this project **does not use any CI/CD**.

### C# language constraint

The code must stay compatible with **C# 5**, because the `csc.exe` included with Windows only supports C# 5. Do not use:

- String interpolation `$""`
- Null-conditional operator `?.`
- `nameof`
- Expression-bodied members (`=>` methods/properties)
- Tuples, `using static`, `out var`, pattern matching
- Any other C# 6+ syntax

The build script enforces this with `/langversion:5`. Source files are UTF-8, and the script passes `/codepage:65001`.

### Windows 10 / 11

Requirement: the `csc.exe` from the built-in .NET Framework 4.x.

```bat
build.bat
```

The script looks for:

```text
%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
```

If no compiler is found, it prints instructions for installing or enabling .NET Framework 4.8 instead of crashing.

### Compiler options

`build.bat` uses these options:

```text
/target:winexe
/out:WinAdapterSwitcher.exe
/win32manifest:app.manifest
/win32icon:app.ico
/reference:System.dll
/reference:System.Drawing.dll
/reference:System.Windows.Forms.dll
/reference:System.Core.dll
/reference:System.Management.dll
/reference:System.Xml.dll
/langversion:5
```

> Adapter enumeration and status reads use `System.Management` to query `MSFT_NetAdapter`. Enable/disable operations use the built-in `netsh interface set interface`. If WMI is unavailable, the app falls back to parsing `netsh interface show interface`.

A successful Windows build produces a single `WinAdapterSwitcher.exe` in the repository root, normally around 150 KB, with no DLLs beside it.

## Releases

Releases are published only on GitHub Releases and are built locally by the maintainer:

1. Run `build.bat` on Windows 10 / 11.
2. Perform a basic functional check on Windows 10 / 11.
3. Package `WinAdapterSwitcher.exe` as a zip.
4. Compute the SHA256 checksum:

   ```powershell
   Get-FileHash .\WinAdapterSwitcher.exe -Algorithm SHA256
   ```

5. Create a GitHub Release, upload the zip, and include the SHA256, version, changelog, and supported Windows versions.

The repository contains no CI configuration files.

## Auto-start and uninstall

Auto-start uses a Windows scheduled task named `WinAdapterSwitcher`:

- Trigger: when the current user signs in (about a 10-second delay)
- Privileges: highest, so Windows does not block a `requireAdministrator` app at sign-in
- Action: launch the full path to `WinAdapterSwitcher.exe`
- Does not write `HKCU\...\Run`, does not write HKLM, and does not register a service

Enabling or disabling auto-start requires administrator privileges (the app is already elevated). A legacy `HKCU\...\Run\WinAdapterSwitcher` entry from older versions is migrated to the scheduled task and deleted at startup.

If you delete the program folder without disabling auto-start, the task will fail at the next sign-in. Running the app again detects the missing path and removes the task. You can also delete `WinAdapterSwitcher` in Task Scheduler, or run the app and turn off auto-start in Settings.

Apart from the exe, `config.ini`, the optional `WinAdapterSwitcher.log`, and the scheduled task, the app creates no other files. Removing the program folder and the scheduled task completes uninstallation.

## Project layout

```text
Program.cs                 Entry point, single-instance handling, CLI entry
TrayApplicationContext.cs  Tray icon, context menu, status refresh
AdapterService.cs          Adapter enumeration, state reads, enable/disable (WMI + netsh fallback)
AdapterDetector.cs         Detects wired/wireless adapters on first run
SwitchEngine.cs            Mode switching and result aggregation
SettingsForm.cs            Settings window
Localization*.cs           UI strings for 8 languages
Config.cs                  config.ini read/write
AutoStart.cs               Windows scheduled task auto-start
Ipc.cs                     Single-instance command forwarding (named pipe)
CommandLine.cs             Command-line parsing
TrayIcons.cs               Runtime tray state icons
SecurityUtil.cs            Administrator checks and elevated restart
Logger.cs                  Optional logging
app.manifest               Administrator privileges, compatibility, DPI
app.ico                    Application icon
build.bat                  One-step build script
docs/README.zh-CN.md       Simplified Chinese README
```

## License

[MIT License](LICENSE)
