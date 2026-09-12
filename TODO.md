# CreamInstaller — Stato lavori e TODO

Nota di memoria per riprendere il progetto da una sessione nuova.
Repo: `gmy77/CreamInstaller` · branch principale: `main`.

## ✅ Fatto (giugno–settembre 2026)

Tutto già in `main`:

- **Fix LAG / performance (C#)**: `Thread.Sleep` → `await Task.Delay`, fix deadlock in
  `SteamCMD.Dispose()`, leak handle GDL icona (`IconGrabber`), disposal `HttpResponseMessage`.
- **Fix giochi disinstallati**: la scansione libreria ora richiede `Directory.Exists(gameDirectory)`
  → non mostra più giochi con `appmanifest_*.acf` orfano (`SteamLibrary.cs`).
- **Retry HTTP con backoff esponenziale** in `HttpClientManager.EnsureGet()` (errori transitori/5xx/timeout).
- **Font base UI leggermente più grande** (Segoe UI 9.75pt via `Application.SetDefaultFont`, `Program.cs`).
- **Warning analyzer risolti** senza soppressione (CA1860 `.Any()`→`.Count`/`.Length`/`.IsEmpty`,
  CA1822 static, CA1852 sealed, CA1872 `Convert.ToHexString`).
- **Avvio non bloccante**: il check aggiornamenti GitHub non blocca più la comparsa della lista giochi
  (`MainForm.cs`).
- **Icona applicazione** aggiornata (`Resources\ini.ico`, usata sia dall'exe che dalle finestre).
- **Upgrade a .NET 10 LTS** (`net9.0-windows` → `net10.0-windows`, supporto fino a nov 2028):
  - `global.json` SDK 10.0.0; workflow CI (build/test/release) su `.NET 10`.
  - Bump pacchetti: HtmlAgilityPack 1.13.0, Newtonsoft.Json 13.0.4, Onova 2.6.13.
  - Rimosso `System.Reflection.Metadata` (ridondante su .NET 10, warning NU1510).
  - Build + CI verdi, testato su Windows.

### Note operative note (non bug)
- **Crash "parte ~15s poi sparisce"** = falso positivo antivirus (Windows Defender). CreamInstaller è
  un DLC unlocker → flaggato per categoria. Soluzione: esclusione Defender sulla cartella. Non è un bug del codice.

## 📋 TODO SICURO — Aggiornare le DLL unlocker (opzionale, a parte)

CreamInstaller installa nei giochi delle DLL di terze parti (di **acidicoala**), tenute embedded
sotto `CreamInstaller\Resources\...` (vedi `EmbeddedResource` nel csproj):

| Unlocker | Sblocca | File | Repo upstream |
|---|---|---|---|
| SmokeAPI | DLC Steam | `steam_api.dll`, `steam_api64.dll` | github.com/acidicoala/SmokeAPI |
| ScreamAPI | DLC Epic/EOS | `EOSSDK-Win32/64-Shipping.dll` | github.com/acidicoala/ScreamAPI |
| Koaloader | loader/proxy | `version.dll`, `dxgi.dll`, `winmm.dll`, … | github.com/acidicoala/Koaloader |
| Uplay R1 | DLC Ubisoft R1 | `uplay_r1_loader.dll` | github.com/acidicoala/UplayR1Unlocker |
| Uplay R2 | DLC Ubisoft R2 | `upc_r2_loader.dll` | github.com/acidicoala/UplayR2Unlocker |

Alcune sono datate (in `Resources/Resources.cs` ci sono hash etichettati "Koaloader v2.0.0").

**Passi per aggiornarle:**
1. Scaricare le ultime release dai repo di acidicoala.
2. Sostituire le DLL sotto `CreamInstaller\Resources\...`.
3. **Aggiornare gli hash MD5** di verifica in `CreamInstaller/Resources/Resources.cs`
   (l'app li controlla: DLL con hash non riconosciuto vengono rifiutate).
4. Ricompilare e **testare su giochi reali** (la CI non prova il runtime).

**Perché è "sicuro ma a parte":** sono binari di terze parti + hash da riallineare + test manuale.
Non urgente — l'app funziona bene com'è.

## ▶️ Come riprendere da una sessione nuova
- `main` è la fonte di verità (tutto il lavoro sopra è mergiato).
- Vincolo fisso del progetto: **NIENTE codice offuscato** (niente PyArmor ecc.).
- CI su Windows (`windows-latest`, .NET 10). Il progetto è `net10.0-windows` → compilabile solo su Windows.
