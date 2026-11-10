using System.Diagnostics;

namespace MyPCToolPro;

/// <summary>
/// Smart process killer — the "antivirus / game booster" style kill.
/// Kills user apps + non-essential background processes, but NEVER touches
/// Windows system / security processes (killing those would crash or
/// blue-screen Windows, so every professional tool protects them).
/// </summary>
public static class ProcessKiller
{
    // NEVER kill these — Windows will crash / BSOD / log out if they die.
    private static readonly HashSet<string> Protected = new(StringComparer.OrdinalIgnoreCase)
    {
        "Idle", "System", "Registry", "Secure System", "Memory Compression",
        "smss", "csrss", "wininit", "winlogon", "services", "lsass", "lsaiso",
        "svchost", "fontdrvhost", "dwm", "spoolsv", "WUDFHost", "dasHost",
        "sihost", "taskhostw", "ctfmon", "conhost", // conhost protected: killing it kills its console apps abruptly
        "StartMenuExperienceHost", "SearchHost", "SearchIndexer", "ShellExperienceHost",
        "RuntimeBroker", "ApplicationFrameHost", "TextInputHost", "backgroundTaskHost",
        "MsMpEng", "NisSrv", "SecurityHealthService", "SecurityHealthSystray", "MpDefenderCoreService",
        "audiodg", "WmiPrvSE", "dllhost", "wermgr", "WerFault", "TrustedInstaller", "TiWorker",
        "MoUsoCoreWorker", "MyPCToolPro", "MyPCToolPro.exe",
    };

    // Well-known background updaters / launchers that are safe + useful to close when boosting.
    private static readonly HashSet<string> KnownBackground = new(StringComparer.OrdinalIgnoreCase)
    {
        "OneDrive", "Teams", "ms-teams", "Discord", "Spotify", "Steam", "steamwebhelper",
        "EpicGamesLauncher", "AdobeUpdateService", "CCXProcess", "Creative Cloud",
        "GoogleUpdate", "MicrosoftEdgeUpdate", "Skype", "Zoom", "Slack", "WhatsApp",
        "Telegram", "iTunes", "Dropbox", "Adobe Acrobat Update", "jusched", "jucheck",
    };

    public enum ProcKind { App, Background, Protected }

    public record ProcInfo(int Id, string Name, long RamMB, ProcKind Kind, string Title);

    public static List<ProcInfo> ListProcesses()
    {
        var list = new List<ProcInfo>();
        int myPid = Environment.ProcessId;
        int mySession = Process.GetCurrentProcess().SessionId;
        foreach (var p in Process.GetProcesses())
        {
            try
            {
                if (p.Id == myPid || p.Id == 0) { list.Add(new(p.Id, p.ProcessName, 0, ProcKind.Protected, "")); continue; }
                long ram = 0;
                try { ram = p.WorkingSet64 / (1024 * 1024); } catch { }
                string title = "";
                bool hasWindow = false;
                try { hasWindow = p.MainWindowHandle != IntPtr.Zero; title = p.MainWindowTitle ?? ""; } catch { }

                ProcKind kind;
                if (Protected.Contains(p.ProcessName)) kind = ProcKind.Protected;
                else if (p.SessionId != mySession) kind = ProcKind.Protected; // other session / pure system
                else if (hasWindow) kind = ProcKind.App;
                else if (KnownBackground.Contains(p.ProcessName)) kind = ProcKind.Background;
                else kind = ProcKind.Background; // same-session, not protected -> boostable background
                list.Add(new(p.Id, p.ProcessName, ram, kind, title));
            }
            catch { /* process exited while enumerating */ }
            finally { try { p.Dispose(); } catch { } }
        }
        return list.OrderByDescending(x => x.RamMB).ToList();
    }

    public record KillResult(int AppsClosed, int ProcessesKilled, long RamBeforeMB, long RamAfterMB, List<string> Log)
    {
        public long FreedMB => Math.Max(0, RamBeforeMB - RamAfterMB);
    }

    /// <summary>
    /// Kill everything non-protected. Visible apps get a graceful close first
    /// (WM_CLOSE, like clicking X) so unsaved work can prompt, then a force-kill
    /// if they are still alive after a short wait.
    /// </summary>
    public static async Task<KillResult> KillAllAsync(bool gracefulFirst, bool includeExplorer, IProgress<string>? progress = null)
    {
        var log = new List<string>();
        long ramBefore = UsedRamMB();
        int appsClosed = 0, killed = 0;

        var targets = ListProcesses().Where(x => x.Kind != ProcKind.Protected).ToList();
        if (!includeExplorer)
            targets = targets.Where(x => !x.Name.Equals("explorer", StringComparison.OrdinalIgnoreCase)).ToList();
        else
            targets = targets.Where(x => true).ToList(); // explorer is not in Protected set, opt-in via checkbox

        // 1) Graceful close for visible apps
        var graceful = new List<Process>();
        if (gracefulFirst)
        {
            foreach (var t in targets.Where(x => x.Kind == ProcKind.App))
            {
                try
                {
                    var p = Process.GetProcessById(t.Id);
                    if (p.MainWindowHandle != IntPtr.Zero && p.CloseMainWindow())
                    {
                        graceful.Add(p);
                        progress?.Report($"Closing {t.Name}…");
                    }
                    else p.Dispose();
                }
                catch { }
            }
            if (graceful.Count > 0) await Task.Delay(1800);
        }

        // 2) Force kill whatever is still alive
        var stillFromGraceful = new HashSet<int>();
        foreach (var p in graceful)
        {
            try { if (!p.HasExited) stillFromGraceful.Add(p.Id); else { appsClosed++; log.Add($"Closed app: {p.ProcessName}"); } } catch { }
        }
        foreach (var t in targets)
        {
            try
            {
                using var p = Process.GetProcessById(t.Id);
                if (p.HasExited) { if (t.Kind == ProcKind.App && !stillFromGraceful.Contains(t.Id)) { /* already gone */ } continue; }
                p.Kill(entireProcessTree: true);
                p.WaitForExit(3000);
                killed++;
                log.Add($"Killed: {t.Name} ({t.RamMB} MB)");
                progress?.Report($"Killed {t.Name}");
            }
            catch { /* access denied / already exited — skip safely */ }
        }
        foreach (var p in graceful) try { p.Dispose(); } catch { }

        await Task.Delay(600);
        long ramAfter = UsedRamMB();
        return new KillResult(appsClosed, killed, ramBefore, ramAfter, log);
    }

    public static bool KillOne(int pid, out string name)
    {
        name = "";
        try
        {
            using var p = Process.GetProcessById(pid);
            name = p.ProcessName;
            if (Protected.Contains(name)) return false;
            try { if (p.MainWindowHandle != IntPtr.Zero) p.CloseMainWindow(); } catch { }
            p.Kill(entireProcessTree: true);
            return true;
        }
        catch { return false; }
    }

    private static long UsedRamMB()
    {
        // Sum of working sets of the current session — fast and permission-safe.
        long total = 0;
        foreach (var p in Process.GetProcesses())
        {
            try { total += p.WorkingSet64; } catch { }
            finally { try { p.Dispose(); } catch { } }
        }
        return total / (1024 * 1024);
    }
}
