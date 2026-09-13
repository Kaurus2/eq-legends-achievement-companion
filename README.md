# EQ Legends Achievement Companion

Private source repository for the Classic Windows desktop Companion.

## For guildmates
Download the ZIP from **Releases**, extract the whole folder, and run the executable.
Start with `docs/START HERE - Quick Guide.txt` or the illustrated PDF in `docs`.
Select your own achievement export and optional combat log in Configuration.
Access to this private repository is required to download its releases.

## Features
- Compact achievement tiles and a single-column tracker.
- Click-to-open research panel; narrow windows expand and shrink back.
- Multi-select filters and editable alternative farming locations.
- Independent live tracking and popup switches.
- Compact progress popups, completion tune/fireworks, and recent-progress sorting.
- Whole-window opacity from 15% to 100% in Configuration.
- Export refresh, personal notes, copy, CSV export and local backups.

## Build on Windows
Requires Windows with .NET Framework 4.x WPF and its C# compiler installed.
From the repository root in PowerShell:

```powershell
.\src\build.ps1
```

The executable is generated in the repository root. Keep `data` beside it.
No NuGet packages are required. Build output is ignored by Git.

## Test
Build first, then run the executable with `--test`. It writes `test-results.txt`
or `error.log`. Tests use a synthetic achievement fixture, not a player's export.
Tests create temporary integration files beneath the ignored `test-output` folder.

## Data and privacy
`data/research.json` is a distributable research baseline, with personal notes and
preferred selections cleared. Runtime settings, cached exports and backups are
ignored. Never commit combat logs, character exports, tokens or personal notes.
Use a separate extracted release folder for actual play; running this source
checkout can modify its tracked research baseline.

Location research contains explicitly marked community leads and uncertainties.
A location entry does not guarantee current spawn, faction safety or kill credit.
Live estimates do not replace the manually generated achievement export counters.

## Releases
Release ZIPs contain the executable, shared data and user documentation only.
Source remains in this repository; binaries are distributed as release assets.
No license has been selected; this private repository does not grant public reuse.
