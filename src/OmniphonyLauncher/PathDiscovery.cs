namespace OmniphonyLauncher;

public static class PathDiscovery
{
    public static void FillMissing(LauncherSettings s)
    {
        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        var app = AppContext.BaseDirectory;
        var bundledMpv = Path.Combine(app, "runtime", "mpv", "mpv.exe");
        var bundledStudio = Path.Combine(app, "runtime", "studio", "omniphony-studio.exe");
        var bundledOrender = Path.Combine(app, "runtime", "studio", "orender.exe");
        var bundledBridge = Path.Combine(app, "runtime", "studio", "harletty_bridge.dll");

        s.StudioPath = FirstExisting(bundledStudio, s.StudioPath, Path.Combine(pf, "Omniphony Studio", "omniphony-studio.exe"));
        s.OrenderPath = FirstExisting(bundledOrender, s.OrenderPath, Path.Combine(pf, "Omniphony Studio", "orender.exe"));
        s.BridgePath = FirstExisting(bundledBridge, s.BridgePath, Path.Combine(pf, "Omniphony Studio", "harletty_bridge.dll"));
        if (!File.Exists(s.BridgePath)) s.BridgePath = FirstExisting(Find(desktop, "harletty_bridge.dll"), Find(downloads, "harletty_bridge.dll"));
        s.MpvPath = FirstExisting(bundledMpv, s.MpvPath);
        if (!File.Exists(s.MpvPath)) s.MpvPath = FirstExisting(Find(desktop, "mpv.exe", "mpv-omniphony"), Find(downloads, "mpv.exe", "mpv-omniphony"));
        ReadBridgeFromConfig(s);
    }

    private static void ReadBridgeFromConfig(LauncherSettings s)
    {
        if (File.Exists(s.BridgePath) || !File.Exists(s.ConfigPath)) return;
        foreach (var line in File.ReadLines(s.ConfigPath))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("bridge_path:", StringComparison.OrdinalIgnoreCase)) continue;
            var value = trimmed[(trimmed.IndexOf(':') + 1)..].Trim().Trim('"', '\'');
            if (File.Exists(value)) s.BridgePath = value;
            return;
        }
    }

    private static string Find(string root, string name, string? requiredParent = null)
    {
        if (!Directory.Exists(root)) return "";
        try
        {
            return Directory.EnumerateFiles(root, name, SearchOption.AllDirectories)
                .FirstOrDefault(p => requiredParent is null || p.Contains(requiredParent, StringComparison.OrdinalIgnoreCase)) ?? "";
        }
        catch { return ""; }
    }

    private static string FirstExisting(params string[] paths) => paths.FirstOrDefault(File.Exists) ?? paths.FirstOrDefault() ?? "";
}
