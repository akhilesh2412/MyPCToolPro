using System.Diagnostics;

namespace MyPCToolPro;

public class MainForm : Form
{
    // Professional dark palette (antivirus style)
    private static readonly Color Bg = Color.FromArgb(11, 18, 32);
    private static readonly Color SidebarBg = Color.FromArgb(15, 23, 42);
    private static readonly Color Card = Color.FromArgb(30, 41, 59);
    private static readonly Color CardBorder = Color.FromArgb(51, 65, 85);
    private static readonly Color Accent = Color.FromArgb(59, 130, 246);
    private static readonly Color Danger = Color.FromArgb(225, 29, 72);
    private static readonly Color Success = Color.FromArgb(34, 197, 94);
    private static readonly Color Txt = Color.FromArgb(241, 245, 249);
    private static readonly Color Muted = Color.FromArgb(148, 163, 184);

    private Panel _content = null!;
    private Panel _pageDashboard = null!, _pageProcesses = null!, _pageCleaner = null!, _pageAbout = null!;
    private readonly List<Button> _navButtons = new();
    private Label _statProcesses = null!, _statKillable = null!, _statRam = null!, _statStatus = null!;
    private Label _dashStatus = null!;
    private Button _btnKillAll = null!;
    private CheckBox _chkGraceful = null!, _chkExplorer = null!;
    private TextBox _logBox = null!;
    private ListView _procList = null!;
    private Label _cleanResult = null!;
    private CheckedListBox _cleanChecks = null!;
    private System.Windows.Forms.Timer _timer = null!;
    private bool _busy;

    public MainForm()
    {
        Text = "MyPC Tool Pro — PC Optimizer";
        Width = 1120; Height = 720; MinimumSize = new Size(980, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Bg; ForeColor = Txt;
        Font = new Font("Segoe UI", 10f);
        BuildUi();
        RefreshDashboard();
        _timer = new System.Windows.Forms.Timer { Interval = 3000 };
        _timer.Tick += (_, _) => { if (!_busy && _pageDashboard.Visible) RefreshDashboardStatsOnly(); };
        _timer.Start();
    }

    // ---------------- UI construction ----------------
    private void BuildUi()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 228, BackColor = SidebarBg };
        Controls.Add(sidebar);

        var logo = new Label { Text = "🛡️", Font = new Font("Segoe UI", 34f), Location = new Point(18, 16), AutoSize = true, ForeColor = Accent };
        var appName = new Label { Text = "MyPC Tool", Font = new Font("Segoe UI", 19f, FontStyle.Bold), Location = new Point(78, 22), AutoSize = true, ForeColor = Txt };
        var pro = new Label { Text = "PRO  •  v2.0", Font = new Font("Segoe UI", 9f, FontStyle.Bold), Location = new Point(80, 54), AutoSize = true, ForeColor = Accent };
        var tag = new Label { Text = "Cleaner  •  Optimizer  •  Booster", Font = new Font("Segoe UI", 8f), Location = new Point(20, 96), AutoSize = true, ForeColor = Muted };
        sidebar.Controls.AddRange(new Control[] { logo, appName, pro, tag });

        int y = 140;
        AddNav(sidebar, "🏠   Dashboard", y, () => ShowPage(_pageDashboard)); y += 52;
        AddNav(sidebar, "⚔   Processes", y, () => { ShowPage(_pageProcesses); RefreshProcesses(); }); y += 52;
        AddNav(sidebar, "🧹   Cleaner", y, () => ShowPage(_pageCleaner)); y += 52;
        AddNav(sidebar, "ℹ   About", y, () => ShowPage(_pageAbout));

        var adminLbl = new Label { Text = "✓ Running as Administrator", Font = new Font("Segoe UI", 8.5f), ForeColor = Success, Location = new Point(20, Height - 130), Anchor = AnchorStyles.Left | AnchorStyles.Bottom, AutoSize = true };
        var copy = new Label { Text = "MyPC Tool Pro © 2026", Font = new Font("Segoe UI", 8f), ForeColor = Muted, Location = new Point(20, Height - 108), Anchor = AnchorStyles.Left | AnchorStyles.Bottom, AutoSize = true };
        sidebar.Controls.AddRange(new Control[] { adminLbl, copy });

        _content = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Padding = new Padding(24, 18, 24, 18) };
        Controls.Add(_content);
        _content.BringToFront();

        BuildDashboard(); BuildProcesses(); BuildCleaner(); BuildAbout();
        ShowPage(_pageDashboard);
    }

    private void AddNav(Panel sidebar, string text, int y, Action onClick)
    {
        var b = new Button { Text = text, Location = new Point(12, y), Size = new Size(204, 44), FlatStyle = FlatStyle.Flat, BackColor = SidebarBg, ForeColor = Txt, Font = new Font("Segoe UI", 11f), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
        b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = Card;
        b.Click += (_, _) => { foreach (var x in _navButtons) x.BackColor = SidebarBg; b.BackColor = Accent; onClick(); };
        _navButtons.Add(b); sidebar.Controls.Add(b);
        if (_navButtons.Count == 1) b.BackColor = Accent;
    }

    private Panel NewPage()
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Visible = false };
        _content.Controls.Add(p); return p;
    }

    private void ShowPage(Panel page)
    {
        foreach (var p in new[] { _pageDashboard, _pageProcesses, _pageCleaner, _pageAbout }) p.Visible = false;
        page.Visible = true; page.BringToFront();
    }

    private Label Title(Panel p, string text, string sub)
    {
        var t = new Label { Text = text, Font = new Font("Segoe UI", 21f, FontStyle.Bold), ForeColor = Txt, Location = new Point(0, 0), AutoSize = true };
        var s = new Label { Text = sub, Font = new Font("Segoe UI", 10f), ForeColor = Muted, Location = new Point(2, 44), AutoSize = true };
        p.Controls.AddRange(new Control[] { t, s }); return t;
    }

    private (Panel card, Label value) StatCard(Panel parent, int x, string caption)
    {
        var card = new Panel { Location = new Point(x, 84), Size = new Size(196, 92), BackColor = Card };
        card.Paint += (_, e) => { using var pen = new Pen(CardBorder); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
        var val = new Label { Text = "—", Font = new Font("Segoe UI", 19f, FontStyle.Bold), ForeColor = Txt, Location = new Point(14, 10), AutoSize = true };
        var cap = new Label { Text = caption, Font = new Font("Segoe UI", 9f), ForeColor = Muted, Location = new Point(14, 56), AutoSize = true };
        card.Controls.AddRange(new Control[] { val, cap }); parent.Controls.Add(card);
        return (card, val);
    }

    // ---------------- Dashboard ----------------
    private void BuildDashboard()
    {
        _pageDashboard = NewPage();
        Title(_pageDashboard, "Dashboard", "Your PC at a glance — one click to close everything and free RAM");
        _statProcesses = StatCard(_pageDashboard, 0, "Running processes").value;
        _statKillable = StatCard(_pageDashboard, 212, "Can be closed").value;
        _statRam = StatCard(_pageDashboard, 424, "RAM in use").value;
        _statStatus = StatCard(_pageDashboard, 636, "Status").value;
        _statStatus.ForeColor = Success; _statStatus.Font = new Font("Segoe UI", 15f, FontStyle.Bold);

        _dashStatus = new Label { Text = "System ready.", Font = new Font("Segoe UI", 10.5f), ForeColor = Muted, Location = new Point(2, 196), AutoSize = true };
        _pageDashboard.Controls.Add(_dashStatus);

        _btnKillAll = new Button
        {
            Text = "⚔   KILL  ALL  PROCESSES",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Color.White, BackColor = Danger,
            FlatStyle = FlatStyle.Flat, Location = new Point(0, 232), Size = new Size(832, 68), Cursor = Cursors.Hand
        };
        _btnKillAll.FlatAppearance.BorderSize = 0;
        _btnKillAll.Click += async (_, _) => await DoKillAllAsync();
        _pageDashboard.Controls.Add(_btnKillAll);

        var hint = new Label { Text = "Closes all open apps + non-essential background processes and frees RAM — like an antivirus / game booster.\nWindows system & security processes are automatically protected, so your PC will NOT crash.", Font = new Font("Segoe UI", 9.5f), ForeColor = Muted, Location = new Point(2, 312), AutoSize = true };
        _pageDashboard.Controls.Add(hint);

        _chkGraceful = new CheckBox { Text = "Close apps gracefully first (asks apps to close, like clicking ✕ — safer for unsaved work)", Checked = true, ForeColor = Txt, Location = new Point(2, 362), AutoSize = true, Font = new Font("Segoe UI", 10f) };
        _chkExplorer = new CheckBox { Text = "Also restart Windows Explorer (taskbar / desktop will flicker for a second)", Checked = false, ForeColor = Txt, Location = new Point(2, 390), AutoSize = true, Font = new Font("Segoe UI", 10f) };
        _pageDashboard.Controls.AddRange(new Control[] { _chkGraceful, _chkExplorer });

        _logBox = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Card, ForeColor = Txt, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 9.5f), Location = new Point(0, 428), Size = new Size(832, 190), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top };
        _pageDashboard.Controls.Add(_logBox);
    }

    private void Log(string msg)
    {
        if (InvokeRequired) { BeginInvoke(() => Log(msg)); return; }
        _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
    }

    private void RefreshDashboard() => RefreshDashboardStatsOnly();

    private void RefreshDashboardStatsOnly()
    {
        try
        {
            var all = ProcessKiller.ListProcesses();
            int killable = all.Count(x => x.Kind != ProcessKiller.ProcKind.Protected && !x.Name.Equals("explorer", StringComparison.OrdinalIgnoreCase));
            long ramMB = all.Sum(x => x.RamMB);
            _statProcesses.Text = all.Count.ToString();
            _statKillable.Text = killable.ToString();
            _statRam.Text = $"{ramMB / 1024.0:0.0} GB";
            _statStatus.Text = killable > 25 ? "Needs Boost" : "Optimized";
            _statStatus.ForeColor = killable > 25 ? Color.FromArgb(245, 158, 11) : Success;
        }
        catch { }
    }

    private async Task DoKillAllAsync()
    {
        if (_busy) return;
        var confirm = MessageBox.Show(
            "Close ALL open apps and non-essential background processes?\n\n" +
            "• Open apps will close (graceful close first, if enabled)\n" +
            "• Windows system & security processes stay protected\n" +
            "• Unsaved work in apps that ignore the close request may be lost",
            "Kill All Processes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        _busy = true; _btnKillAll.Enabled = false; _btnKillAll.Text = "⏳   WORKING…";
        _dashStatus.Text = "Closing processes…"; Log("Kill All started.");
        var progress = new Progress<string>(s => _dashStatus.Text = s);
        try
        {
            bool includeExplorer = _chkExplorer.Checked;
            var result = await ProcessKiller.KillAllAsync(_chkGraceful.Checked, includeExplorer, progress);
            foreach (var line in result.Log) Log(line);
            Log($"DONE — Apps closed: {result.AppsClosed}, Processes killed: {result.ProcessesKilled}, RAM freed: ~{result.FreedMB} MB");
            _dashStatus.Text = "Boost complete ✓";
            if (includeExplorer)
            {
                try { Process.Start("explorer.exe"); Log("Explorer restarted."); } catch { }
            }
            MessageBox.Show(
                $"✅ Boost Complete!\n\nApps closed: {result.AppsClosed}\nProcesses killed: {result.ProcessesKilled}\nRAM freed: ~{result.FreedMB} MB",
                "MyPC Tool Pro", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { Log("Error: " + ex.Message); }
        finally
        {
            _busy = false; _btnKillAll.Enabled = true; _btnKillAll.Text = "⚔   KILL  ALL  PROCESSES";
            RefreshDashboardStatsOnly();
        }
    }

    // ---------------- Processes page ----------------
    private void BuildProcesses()
    {
        _pageProcesses = NewPage();
        Title(_pageProcesses, "Processes", "Green = app, Yellow = background, Grey = protected system process (cannot be killed)");
        var btnRefresh = new Button { Text = "🔄 Refresh", Location = new Point(0, 76), Size = new Size(130, 36), FlatStyle = FlatStyle.Flat, BackColor = Card, ForeColor = Txt };
        btnRefresh.FlatAppearance.BorderSize = 0; btnRefresh.Click += (_, _) => RefreshProcesses();
        var btnKillSel = new Button { Text = "Kill Selected", Location = new Point(142, 76), Size = new Size(150, 36), FlatStyle = FlatStyle.Flat, BackColor = Card, ForeColor = Txt };
        btnKillSel.FlatAppearance.BorderSize = 0; btnKillSel.Click += (_, _) => KillSelected();
        var btnKillAll2 = new Button { Text = "⚔ Kill All Non-System", Location = new Point(304, 76), Size = new Size(210, 36), FlatStyle = FlatStyle.Flat, BackColor = Danger, ForeColor = Color.White, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        btnKillAll2.FlatAppearance.BorderSize = 0; btnKillAll2.Click += async (_, _) => { ShowPage(_pageDashboard); await DoKillAllAsync(); };
        _pageProcesses.Controls.AddRange(new Control[] { btnRefresh, btnKillSel, btnKillAll2 });

        _procList = new ListView
        {
            View = View.Details, FullRowSelect = true, GridLines = false, BackColor = Card, ForeColor = Txt,
            BorderStyle = BorderStyle.FixedSingle, Location = new Point(0, 124), Size = new Size(832, 494),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom, Font = new Font("Segoe UI", 10f)
        };
        _procList.Columns.Add("Process", 220); _procList.Columns.Add("PID", 80);
        _procList.Columns.Add("RAM", 110); _procList.Columns.Add("Type", 150); _procList.Columns.Add("Window Title", 260);
        _pageProcesses.Controls.Add(_procList);
    }

    private void RefreshProcesses()
    {
        _procList.Items.Clear();
        foreach (var p in ProcessKiller.ListProcesses())
        {
            string kind = p.Kind switch { ProcessKiller.ProcKind.App => "🟢 App", ProcessKiller.ProcKind.Background => "🟡 Background", _ => "⚪ System (Protected)" };
            var item = new ListViewItem(new[] { p.Name, p.Id.ToString(), $"{p.RamMB} MB", kind, p.Title });
            item.Tag = p.Id;
            if (p.Kind == ProcessKiller.ProcKind.Protected) item.ForeColor = Muted;
            _procList.Items.Add(item);
        }
    }

    private void KillSelected()
    {
        foreach (ListViewItem item in _procList.SelectedItems)
        {
            int pid = (int)item.Tag!;
            if (ProcessKiller.KillOne(pid, out string name)) Log($"Killed {name} from Processes page.");
            else MessageBox.Show("That process is protected or could not be killed.", "MyPC Tool Pro");
        }
        RefreshProcesses();
    }

    // ---------------- Cleaner page ----------------
    private void BuildCleaner()
    {
        _pageCleaner = NewPage();
        Title(_pageCleaner, "Cleaner", "Scan first — nothing is deleted until you press Clean. Personal files are never touched.");
        _cleanChecks = new CheckedListBox { BackColor = Card, ForeColor = Txt, BorderStyle = BorderStyle.FixedSingle, Location = new Point(0, 84), Size = new Size(832, 170), Font = new Font("Segoe UI", 11f), CheckOnClick = true };
        foreach (var (label, path) in CleanerService.Targets()) { _cleanChecks.Items.Add($"{label}   —   {path}", true); }
        _pageCleaner.Controls.Add(_cleanChecks);

        var btnScan = new Button { Text = "🔍  Scan", Location = new Point(0, 270), Size = new Size(150, 42), FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = Color.White, Font = new Font("Segoe UI", 11f, FontStyle.Bold) };
        btnScan.FlatAppearance.BorderSize = 0;
        var btnClean = new Button { Text = "🧹  Clean Now", Location = new Point(162, 270), Size = new Size(170, 42), FlatStyle = FlatStyle.Flat, BackColor = Card, ForeColor = Txt, Font = new Font("Segoe UI", 11f) };
        btnClean.FlatAppearance.BorderSize = 0;
        _pageCleaner.Controls.AddRange(new Control[] { btnScan, btnClean });

        _cleanResult = new Label { Text = "", Font = new Font("Segoe UI", 11f), ForeColor = Txt, Location = new Point(2, 330), AutoSize = true, MaximumSize = new Size(830, 0) };
        _pageCleaner.Controls.Add(_cleanResult);

        btnScan.Click += (_, _) =>
        {
            long total = 0; var lines = new List<string>();
            var targets = CleanerService.Targets();
            for (int i = 0; i < targets.Count; i++)
            {
                if (!_cleanChecks.GetItemChecked(i)) continue;
                string pat = targets[i].Label.Contains("Thumbnail") ? "thumbcache_*.db" : "*";
                var (bytes, count) = CleanerService.Scan(targets[i].Path, pat);
                total += bytes; lines.Add($"• {targets[i].Label}: {count} files — {CleanerService.Human(bytes)}");
            }
            _cleanResult.Text = string.Join("\n", lines) + $"\n\n✅ Total junk found: {CleanerService.Human(total)}";
        };
        btnClean.Click += (_, _) =>
        {
            if (MessageBox.Show("Delete the selected junk files? Locked files will be skipped safely.", "Confirm Clean", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            long freed = 0; int removed = 0, skipped = 0;
            var targets = CleanerService.Targets();
            for (int i = 0; i < targets.Count; i++)
            {
                if (!_cleanChecks.GetItemChecked(i)) continue;
                string pat = targets[i].Label.Contains("Thumbnail") ? "thumbcache_*.db" : "*";
                var (f, r, s) = CleanerService.Clean(targets[i].Path, pat);
                freed += f; removed += r; skipped += s;
            }
            _cleanResult.Text = $"✅ Freed: {CleanerService.Human(freed)}  •  Removed: {removed} files  •  Skipped (in use): {skipped}";
        };
    }

    // ---------------- About ----------------
    private void BuildAbout()
    {
        _pageAbout = NewPage();
        Title(_pageAbout, "About", "");
        var lbl = new Label
        {
            Text = "🛡️  MyPC Tool Pro  v2.0\n\n" +
                   "A professional PC optimizer in the style of antivirus / booster tools.\n\n" +
                   "• KILL ALL PROCESSES — closes every open app + non-essential background\n" +
                   "   process in one click and frees RAM. System & security processes are\n" +
                   "   protected by a built-in whitelist, so Windows stays stable.\n" +
                   "• Runs as Administrator (UAC prompt on launch, like real optimizers).\n" +
                   "• Cleaner for Temp, Logs, Thumbnail cache and Recent files.\n" +
                   "• Single-file app — no installation of .NET or anything else needed.\n\n" +
                   "Tip: Pin it to your taskbar, or create a desktop shortcut, for one-click boosting.",
            Font = new Font("Segoe UI", 11f), ForeColor = Txt, Location = new Point(2, 76), AutoSize = true
        };
        _pageAbout.Controls.Add(lbl);
    }
}
