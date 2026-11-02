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

### Verifica aggiornamenti (24 ottobre 2026)
- **.NET 10** resta l'LTS corrente (supporto fino a nov 2028). **.NET 11** è STS: non va inseguito.
- Pacchetti già all'ultima versione, tranne:
  - HtmlAgilityPack 1.13.0 → 1.13.1: tentato nella PR #4, CI rossa non diagnosticabile dalla
    sessione cloud (log bloccati, re-run negato). Patch inutile → **PR #4 chiusa senza merge**.
  - System.ServiceModel.Primitives 4.10.0 → 10.1.0: salto major, **rimandato** (va testato a parte).
- Branch remoti già mergiati/chiusi da cancellare a mano (la sessione cloud non ha il permesso):
  `git push origin --delete claude/htmlagilitypack-1131 claude/net10-and-deps claude/creaminstaller-startup-async-w3bo22`

## 📋 TODO SICURO — Aggiornare le DLL unlocker (opzionale, a parte)

> Da fare **in autonomia**: Claude non aggiorna i binari unlocker. Restano validi i passi qui sotto.

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

## 🐍 Progetto collegato: GetDLCInfoFromSteamDB (Python)

Repo `gmy77/GetDLCInfoFromSteamDB`. **Attenzione:** su `master` c'è solo README/`dist/`; tutto il
codice sta sul branch `claude/fix-steamdb-readonly-channel-011CUfbq6BhKdSqvV61ALjAU` → **PR #3
aperta, non ancora mergiata**. È un progetto Python: nessun `.sln`, si apre la cartella
(VS Code / VS 2026 "Apri cartella"), si avvia con `run_gui.bat`, l'exe si crea con `BUILD_WINDOWS.bat`.

### ✅ Fatto (ottobre–novembre 2026), commit sulla PR #3
- `68143b8` — GUI: la ricerca non perde più giochi; niente decine di schede browser su "Select All";
  niente loop infinito di riavvio se l'installazione delle dipendenze fallisce; tolto l'anti-debug
  che chiudeva l'app senza messaggi.
- `770db16` — **URL dell'API Steam rotto** (stringa XOR "offuscata" che decodificava a spazzatura):
  ora in chiaro, rimossi `_d`/`_K`/`_hwid`. Backoff su 429/5xx (`Retry-After`), cache 24h nel client
  Pro (scrittura atomica), overrides letti una volta, rilevamento Steam da registro Windows + Flatpak.
- `2553607` — **Decisione: lo scraping della pagina `/dlc/` resta il percorso PRIMARIO** (è quello
  che ha sempre funzionato, "velocissimo"); l'API è solo fallback, SteamDB ultima risorsa. Fix Codex:
  titoli numerici, Refresh dopo un filtro, selezione da tastiera.
- `354da45` — Backup `.bak` (una volta sola) di `cream_api.ini` / `DLC.txt` esistenti prima di
  sovrascriverli; SteamDB non più troncato a 50 DLC; niente elaborazioni sovrapposte nella GUI.
- `d63dc8e` — SteamDB: nome del DLC letto dalla colonna giusta (prima poteva uscire data/AppID).

### 📋 Da fare
1. **Testare su Windows** il branch: avvio, scansione, generazione config, e doppio clic su
   "Process All" durante un'elaborazione (il secondo deve essere ignorato).
2. Se tutto ok → **mergiare la PR #3 in `master`**.
3. Opzionali (sicuri, a parte): log delle eccezioni silenziose ("nessun DLC" vs errore di rete);
   `cream_api.ini` accanto a `steam_api64.dll` invece che nella root del gioco; unificare le due
   codebase (`steam_dlc_manager.py`/`steam_dlc_gui.py` vs `steam_dlc_pro.py`, refactor grande).
- Regola fissa: **"il giocattolo non deve rompersi"** — nessun cambio al percorso dati che funziona
  senza test reale.

## ▶️ Come riprendere da una sessione nuova
- CreamInstaller: `main` è la fonte di verità (tutto il lavoro sopra è mergiato).
- GetDLCInfoFromSteamDB: il lavoro è sul branch della **PR #3** (vedi sezione sopra).
- Vincolo fisso del progetto: **NIENTE codice offuscato** (niente PyArmor ecc.).
- CI su Windows (`windows-latest`, .NET 10). Il progetto è `net10.0-windows` → compilabile solo su Windows.
