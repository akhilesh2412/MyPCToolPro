# Changelog

## [2.0.0] — 2026-11-10
### Added
- Professional antivirus-style dark UI (C# / .NET 8 WinForms)
- **KILL ALL PROCESSES** — graceful close first, then force-kill with process tree; RAM-freed report
- Smart protection whitelist for Windows system & security processes
- Processes page: live list (App / Background / System-Protected), Kill Selected
- Cleaner page: Temp, Windows Temp, Thumbnail Cache, Logs, Recent (scan → clean)
- Runs as Administrator via app manifest (UAC)
- NSIS installer: Program Files, Desktop + Start Menu shortcuts, Add/Remove Programs entry, uninstaller
- Self-contained single-file EXE — no runtime installation needed
- GitHub Actions workflow: builds EXE + installer and auto-publishes a Release on version tags

## [1.0.0] — 2026-11-10
- Initial Python/Tkinter prototype (cleaner, optimizer, file tools, theme customization)
