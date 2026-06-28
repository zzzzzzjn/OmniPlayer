using System.Text.Json;

namespace OmniphonyLauncher;

public sealed class LauncherSettings
{
    public string MpvPath { get; set; } = "";
    public string StudioPath { get; set; } = "";
    public string OrenderPath { get; set; } = "";
    public string BridgePath { get; set; } = "";
    public string ConfigPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "omniphony", "config.yaml");
    public string MediaPath { get; set; } = "";
    public bool StartStudio { get; set; } = true;
    public bool EnableOsc { get; set; } = true;
    public bool ShowMpvOverlay { get; set; } = false;
    public string AudioDevice { get; set; } = "auto";
    public int OscPort { get; set; } = 9000;
    public string ExtraArguments { get; set; } = "";

    private static readonly string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniphonyLauncher", "settings.json");

    public static LauncherSettings Load()
    {
        try { return File.Exists(FilePath) ? JsonSerializer.Deserialize<LauncherSettings>(File.ReadAllText(FilePath)) ?? new() : new(); }
        catch { return new(); }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var temp = FilePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temp, FilePath, true);
    }
}
