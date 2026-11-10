namespace MyPCToolPro;

public static class CleanerService
{
    public static List<(string Label, string Path)> Targets() => new()
    {
        ("User Temp", Path.GetTempPath()),
        ("Windows Temp", @"C:\Windows\Temp"),
        ("Thumbnail Cache", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Explorer")),
        ("Windows Logs", @"C:\Windows\Logs"),
        ("Recent Files", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Recent")),
    };

    public static (long bytes, int count) Scan(string folder, string pattern = "*")
    {
        long bytes = 0; int count = 0;
        try
        {
            if (!Directory.Exists(folder)) return (0, 0);
            foreach (var f in Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories))
            {
                try { bytes += new FileInfo(f).Length; count++; } catch { }
            }
        }
        catch { }
        return (bytes, count);
    }

    public static (long freed, int removed, int skipped) Clean(string folder, string pattern = "*")
    {
        long freed = 0; int removed = 0, skipped = 0;
        try
        {
            if (!Directory.Exists(folder)) return (0, 0, 0);
            foreach (var f in Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories).ToList())
            {
                try { var len = new FileInfo(f).Length; File.Delete(f); freed += len; removed++; }
                catch { skipped++; }
            }
        }
        catch { }
        return (freed, removed, skipped);
    }

    public static string Human(long bytes)
    {
        string[] u = { "B", "KB", "MB", "GB", "TB" };
        double s = bytes; int i = 0;
        while (s >= 1024 && i < u.Length - 1) { s /= 1024; i++; }
        return $"{s:0.0} {u[i]}";
    }
}
