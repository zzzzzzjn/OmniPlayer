using System.Text.RegularExpressions;

namespace OmniphonyLauncher;

public static partial class ConfigEditor
{
    [GeneratedRegex(@"(?m)^(\s*bridge_path\s*:\s*).*$", RegexOptions.IgnoreCase)]
    private static partial Regex BridgeLine();

    public static string ApplyBridgePath(string configPath, string bridgePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
        var original = File.Exists(configPath) ? File.ReadAllText(configPath) : "render:\r\n";
        var escaped = bridgePath.Replace("\\", "\\\\").Replace("\"", "\\\"");
        var replacement = "${1}\"" + escaped + "\"";
        var updated = BridgeLine().IsMatch(original)
            ? BridgeLine().Replace(original, replacement, 1)
            : InsertBridgePath(original, escaped);
        if (updated == original) return "配置无需修改";

        if (File.Exists(configPath))
        {
            var backup = configPath + ".launcher-backup-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N")[..6];
            File.Copy(configPath, backup, false);
        }
        var temp = configPath + ".tmp";
        File.WriteAllText(temp, updated);
        File.Move(temp, configPath, true);
        return "已原子写入配置，并保留时间戳备份";
    }

    private static string InsertBridgePath(string text, string escaped)
    {
        var render = Regex.Match(text, @"(?m)^render\s*:\s*(?:\r?\n|$)");
        if (!render.Success) return $"render:\r\n  bridge_path: \"{escaped}\"\r\n" + text.TrimStart();
        return text.Insert(render.Index + render.Length, $"  bridge_path: \"{escaped}\"\r\n");
    }
}
