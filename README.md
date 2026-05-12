### CreamInstaller: Automatic DLC Unlocker Installer & Configuration Generator

[![Version](https://img.shields.io/github/v/release/gmy77/CreamInstaller?label=release)](https://github.com/gmy77/CreamInstaller/releases/latest)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License](https://img.shields.io/github/license/gmy77/CreamInstaller)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-0078D6?logo=windows)](https://github.com/gmy77/CreamInstaller/releases/latest)

![Program Preview Image](https://raw.githubusercontent.com/gmy77/CreamInstaller/main/preview.png)

###### The program utilizes the latest versions of [Koaloader](https://github.com/acidicoala/Koaloader), [SmokeAPI](https://github.com/acidicoala/SmokeAPI), [ScreamAPI](https://github.com/acidicoala/ScreamAPI), [Uplay R1 Unlocker](https://github.com/acidicoala/UplayR1Unlocker) and [Uplay R2 Unlocker](https://github.com/acidicoala/UplayR2Unlocker), all by the wonderful [acidicoala](https://github.com/acidicoala), and all downloaded from the posts above and embedded into the program itself; no further downloads necessary on your part!

---
#### What's new in v5.0:
* Migrated to **.NET 9** (was .NET 7, which is end-of-life).
* Replaced blocking `Thread.Sleep` / `.Wait()` calls with `async`/`await` (`Task.Delay`) in `SteamCMD`, `SteamStore`, `InstallForm`, `SelectForm` — eliminates UI freezes and potential deadlocks.
* Async file I/O (`File.ReadAllTextAsync`) in `SteamLibrary` for faster startup with large libraries.
* Fixed unmanaged handle leak in `IconGrabber` (`GetHicon()`).
* `HttpResponseMessage` lifetime properly scoped in `EpicStore` (`using`).
* SmokeAPI config: `extra_inventory_items` corrected from object to array.
* Dark theme UI rendering fixes.
* Path validation, race condition and other security/correctness fixes.

---
#### Description:
Automatically finds all installed Steam, Epic and Ubisoft games with their respective DLC-related DLL locations on the user's computer,
parses SteamCMD, Steam Store and Epic Games Store for user-selected games' DLCs, then provides a very simple graphical interface
utilizing the gathered information for the maintenance of DLC unlockers.

The primary function of the program is to **automatically generate and install DLC unlockers** for whichever
games and DLCs the user selects; however, through the use of **right-click context menus** the user can also:
* automatically repair the Paradox Launcher
* open parsed Steam and/or Epic Games appinfo in Notepad(++)
* refresh parsed Steam and/or Epic Games appinfo
* open root game directories and important DLL directories in Explorer
* open SteamDB, ScreamDB, Steam Store, Epic Games Store, Steam Community, Ubisoft Store, and official game website links (where applicable) in the default browser

---
#### Features:
* Automatic download and installation of SteamCMD as necessary whenever a Steam game is chosen. *For gathering appinfo such as name, buildid, listofdlc, depots, etc.*
* Automatic gathering and caching of information for all selected Steam and Epic games and **ALL** of their DLCs.
* Automatic DLL installation and configuration generation for Koaloader, SmokeAPI, ScreamAPI, Uplay R1 Unlocker and Uplay R2 Unlocker.
* Automatic uninstallation of DLLs and configurations for Koaloader, CreamAPI, SmokeAPI, ScreamAPI, Uplay R1 Unlocker and Uplay R2 Unlocker.
* Automatic reparation of the Paradox Launcher (and manually via the right-click context menu "Repair" option). *For when the launcher updates whilst you have CreamAPI, SmokeAPI or ScreamAPI installed to it.*

---
#### Installation:
1. Click [here](https://github.com/gmy77/CreamInstaller/releases/latest) to download the latest release from [GitHub](https://github.com/gmy77/CreamInstaller).
2. Extract the executable to anywhere on your computer you want. *It's completely self-contained.*

If the program doesn't seem to launch, try downloading and installing the [.NET Desktop Runtime 9.0](https://dotnet.microsoft.com/download/dotnet/9.0/runtime).

---
#### **NOTE:** This program does not automatically download nor install actual DLC files for you; as the title of the program states, this program is only a *DLC Unlocker* installer. Should the game you wish to unlock DLC for not already come with the DLCs installed, as is the case with a good majority of games, you must find, download and install those to the game yourself.

#### **ALSO NOTE:** Assuming the program functioned as it was supposed to by installing the DLC unlocker(s) correctly, your game not working or its DLCs not unlocking is not an issue I can do anything about and it's entirely up to you to seek the appropriate resources to fix it yourself (hint: [cs.rin.ru](https://cs.rin.ru/forum/viewforum.php?f=10)).

#### Preferably, should you encounter any issue outside of the program itself, you should be referring said issues to the proper [cs.rin.ru](https://cs.rin.ru/forum/viewforum.php?f=10) post for the game you're tinkering with; 99% of the time you will find the answers to your problems there (you will probably also find the DLC files you're looking for there *wink wink*).

---
#### Usage:
1. Start the program executable. *Read above under Installation if it doesn't launch.*
2. Choose which programs and/or games the program should scan for DLC. *The program automatically gathers all installed games from Steam, Epic and Ubisoft directories.*
3. Wait for the program to download and install SteamCMD (if you chose a Steam game). *Very fast, depends on internet speed.*
4. Wait for the program to gather and cache the chosen games' information & DLCs. *May take a good amount of time on the first run, depends on how many games you chose and how many DLCs they have.*
5. **CAREFULLY** select which games' DLCs you wish to unlock. *Obviously none of the DLC unlockers are tested for every single game!*
6. Choose whether or not to install with Koaloader, and if so then also pick the proxy DLL to use. *If the default version.dll doesn't work, then see [here](https://cs.rin.ru/forum/viewtopic.php?p=2552172#p2552172) to find one that does.*
7. Click the **Generate and Install** button.
8. Click the **OK** button to close the program.
9. If any of the DLC unlockers cause problems with any of the games you installed them on, simply go back to step 5 and select what games you wish you **revert** changes to, and instead click the **Uninstall** button this time.

---
#### Building from source:
Requirements:
* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (the pinned version is declared in `global.json`)
* Windows (the project targets `net9.0-windows` and uses Windows Forms)
* Visual Studio 2022 (17.12+) or the .NET CLI

Clone and build a single-file, self-contained Release binary:
```powershell
git clone https://github.com/gmy77/CreamInstaller.git
cd CreamInstaller
dotnet publish CreamInstaller/CreamInstaller.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
The output binary will be in `CreamInstaller/bin/Release/net9.0-windows/win-x64/publish/`.

---
##### Bugs/Crashes/Issues:
For reliable and quick assistance, all bugs, crashes and other issues should be referred to the [GitHub Issues](https://github.com/gmy77/CreamInstaller/issues) page!

---
##### More Information:
* SteamCMD installation and appinfo cache can be found at **C:\ProgramData\CreamInstaller**.
* The program automatically and very quickly updates from [GitHub](https://github.com/gmy77/CreamInstaller) using [Onova](https://github.com/Tyrrrz/Onova). *updates can be ignored*
* The program source and other information can be found on [GitHub](https://github.com/gmy77/CreamInstaller). Originally forked from [pointfeev/CreamInstaller](https://github.com/pointfeev/CreamInstaller).


## ⚠️ Antivirus False Positives

Some antivirus software (including Windows Defender) may flag CreamInstaller or its embedded DLLs as threats. **These are false positives.**

### Why it happens
CreamInstaller embeds proxy DLLs (Koaloader, SmokeAPI, ScreamAPI, UplayR1/R2) that replace game DRM libraries. This behavior — replacing a system DLL — triggers heuristic detection in most AV engines, even though no malicious activity takes place.

### What was verified
- All embedded DLLs were statically analyzed (no execution)
- SHA-256 hashes match the official builds by [@acidicoala](https://github.com/acidicoala)
- No process injection, no keyloggers, no C2 servers, no persistence mechanisms
- The only outbound URL is `raw.githubusercontent.com/acidicoala/public-entitlements` (DLC entitlement list)
- Full source code is available and auditable in this repository

### AV classification
These tools are correctly classified as **HackTool / Riskware** — they bypass DRM protection. They are **not** viruses, worms, or trojans and pose no risk to your system.

### What to do
Add the installation folder to your antivirus exclusion list before running.
**Windows Defender (PowerShell):**
```powershell
Add-MpPreference -ExclusionPath "C:\Path\To\CreamInstaller"
```
