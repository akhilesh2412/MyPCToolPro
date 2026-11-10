<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=gradient&height=200&section=header&text=MyPC%20Tool%20Pro&fontSize=64&fontAlignY=35&desc=Cleaner%20%7C%20Optimizer%20%7C%20Process%20Killer%20%7C%20RAM%20Booster&descAlignY=62" alt="MyPC Tool Pro" width="100%"/>

# 🛡️ MyPC Tool Pro

### *One click. Every useless process — gone.*

**A professional Windows PC optimizer in the style of antivirus & game-booster tools.**
**Clean junk, inspect every running process, and free RAM with one huge button.**

<br/>

![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Version](https://img.shields.io/badge/Version-2.0.0-E11D48?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-22C55E?style=for-the-badge)

<br/>

[![⬇️ Download Latest Release](https://img.shields.io/badge/⬇️_Download-Latest_Release-3B82F6?style=for-the-badge&logo=github&logoColor=white)](https://github.com/akhilesh2412/MyPCToolPro/releases/latest)
[![⭐ Star this repo](https://img.shields.io/badge/⭐_Star-this_repo-F59E0B?style=for-the-badge)](https://github.com/akhilesh2412/MyPCToolPro)

</div>

---

## ⚡ The Killer Feature — **KILL ALL PROCESSES**

<div align="center">

> ### `⚔ KILL ALL PROCESSES`
> *One big red button on the Dashboard. That's all it takes.*

</div>

1. **Click** the button → confirm
2. Open apps close **gracefully first** (like clicking ✕ — unsaved work gets its chance to prompt)
3. Anything still alive, plus every non-essential background process, is **force-killed with its whole process tree**
4. You get a report: **apps closed · processes killed · RAM freed (MB)**

Launchers, updaters, chat apps, browser helpers eating your RAM in the background? **Gone in seconds.** Perfect before gaming, editing, or when your PC just feels slow.

---

## ✨ Features

| | Feature | What it does |
| :---: | --- | --- |
| 🏠 | **Live Dashboard** | Running processes, closable processes, RAM in use & PC status — auto-refreshing every 3 seconds |
| ⚔️ | **Kill All Processes** | One-click boost: graceful close → force kill → RAM freed report |
| 📋 | **Process Manager** | Every process with PID, RAM & type — **🟢 App · 🟡 Background · ⚪ System (Protected)** — kill individually or all at once |
| 🧹 | **Junk Cleaner** | Temp files, Windows Temp, Thumbnail Cache, Logs & Recent Files — **scan first, clean second**, locked files skipped safely |
| 🛡️ | **Runs as Administrator** | UAC prompt on launch, just like real antivirus / optimizer tools |
| 📦 | **Single-File App** | No .NET, no Python, nothing else to install — everything is inside the EXE |
| 💿 | **Proper Installer** | Installs to *Program Files*, Desktop + Start Menu shortcuts, listed in Windows Settings → Apps with a working uninstaller |

---

## 🛡️ Safety First — Why It Won't Crash Your PC

Killing the *wrong* process blue-screens Windows. That's why **MyPC Tool Pro** ships with a hard-coded protection whitelist — these are **never** killed, exactly like professional boosters do:

<div align="center">

| 🔒 Protected Category | Examples |
| --- | --- |
| **Core Windows** | `System` · `Registry` · `smss` · `csrss` · `wininit` · `winlogon` · `lsass` · `services` · `svchost` · `dwm` |
| **Security** | Windows Defender (`MsMpEng`) · `SecurityHealthService` |
| **Shell & Search** | `sihost` · `taskhostw` · `SearchHost` · Start Menu & Shell hosts |
| **Itself** | `MyPCToolPro` — it won't kill itself mid-boost 😄 |

</div>

> 🖥️ **Windows Explorer** is also kept by default. Want it included? There's an opt-in checkbox — and the app restarts Explorer for you right after.

---

## ⬇️ Download & Install

### Option 1 — Installer *(recommended)*

1. Download **`MyPCToolPro-Setup.exe`** from the [**Latest Release**](https://github.com/akhilesh2412/MyPCToolPro/releases/latest)
2. Run it → **Next → Install → Finish** *(tick “Launch MyPC Tool Pro now”)*
3. Accept the **Administrator (UAC)** prompt — and the app opens. Done. ✅

### Option 2 — Portable

Download **`MyPCToolPro.exe`** from the same release page and just double-click it. No installation at all.

> **Requirements:** Windows 10 / 11 (64-bit). Nothing else.

---

## 🖥️ Inside the App

- **🏠 Dashboard** — Live stats cards + the big red **KILL ALL PROCESSES** button + a real-time activity log
- **⚔️ Processes** — Full process list with search-by-eye RAM usage, Kill Selected & Kill All Non-System
- **🧹 Cleaner** — Tick what to clean, hit Scan, see the junk size, then Clean
- **ℹ️ About** — How the booster works & safety notes

---

## 🔨 Build From Source

```powershell
# Prerequisites: .NET 8 SDK
git clone https://github.com/akhilesh2412/MyPCToolPro.git
cd MyPCToolPro
pip install pillow            # optional: generates the app icon
python scripts/make_assets.py
cd src
dotnet publish -c Release -o ..\dist
# → dist\MyPCToolPro.exe  (self-contained, single-file, win-x64)
```

**Build the installer** (optional — requires [NSIS](https://nsis.sourceforge.io/)):

```powershell
copy dist\MyPCToolPro.exe installer\
makensis installer\installer.nsi
# → installer\MyPCToolPro-Setup.exe
```

> 🤖 **GitHub Actions:** pushing a tag like `v2.1.0` automatically builds the EXE **and** the installer on a Windows runner and publishes a Release — see [`.github/workflows/build.yml`](.github/workflows/build.yml).

---

## 🗂️ Project Structure

```
MyPCToolPro/
├── 📄 README.md                  ← you are here
├── 📁 scripts/
│   └── 🐍 make_assets.py         ← generates the app icon + banner (pip install pillow)
├── 📁 src/
│   ├── 🎯 Program.cs             ← entry point
│   ├── 🖼️ MainForm.cs            ← antivirus-style dark UI (Dashboard / Processes / Cleaner / About)
│   ├── ⚔️ ProcessKiller.cs       ← smart kill engine + protection whitelist
│   ├── 🧹 CleanerService.cs      ← junk scan & clean
│   ├── 📜 app.manifest           ← requireAdministrator (UAC)
│   └── ⚙️ MyPCToolPro.csproj     ← .NET 8 WinForms, single-file publish
├── 📁 installer/
│   └── 📜 installer.nsi          ← NSIS installer script
└── 📁 .github/workflows/
    └── ⚙️ build.yml              ← CI: build EXE + installer, auto-release on tags
```

---

## 🗺️ Roadmap

- [ ] 🎮 **Game Booster mode** — boost + restore, with a per-game profile
- [ ] 📊 **Disk Analyzer** — visual chart of what's eating your storage
- [ ] 🚀 **Startup Manager** — enable / disable programs that run at login
- [ ] 🌡️ **CPU / RAM live graph** on the Dashboard
- [ ] 🔔 **Tray icon** with right-click *Boost Now*

---

## 🤝 Contributing

Issues and pull requests are welcome! Found a background process that should (or shouldn't) be killed? Open an issue with the process name — the whitelist gets smarter with every report.

## 📄 License

Released under the **MIT License** — see [`LICENSE`](LICENSE). Use it, fork it, make it yours.

---

<div align="center">

### Made with ❤️ by [@akhilesh2412](https://github.com/akhilesh2412)

**If MyPC Tool Pro freed your RAM, give it a ⭐ — it really helps!**

🛡️

</div>
