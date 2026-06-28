using OmniphonyLauncher;

var root = Path.Combine(Path.GetTempPath(), "OmniphonyLauncher.Tests", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    var config = Path.Combine(root, "config.yaml");
    var bridge = @"C:\Program Files\Omniphony Studio\harletty_bridge.dll";
    File.WriteAllText(config, "render:\r\n  osc: true\r\nother:\r\n  value: keep\r\n");
    ConfigEditor.ApplyBridgePath(config, bridge);
    var first = File.ReadAllText(config);
    Assert(first.Contains("render:\r\n  bridge_path:"), "bridge_path 没有插入 render 下方");
    Assert(first.Contains("other:\r\n  value: keep"), "无关配置被损坏");
    ConfigEditor.ApplyBridgePath(config, @"D:\Audio\new_bridge.dll");
    var second = File.ReadAllText(config);
    Assert(second.Contains(@"D:\\Audio\\new_bridge.dll"), "已有 bridge_path 没有被替换");
    Assert(Directory.GetFiles(root, "config.yaml.launcher-backup-*").Length >= 2, "没有生成配置备份");
    Console.WriteLine("PASS ConfigEditor preserves YAML and creates backups");
}
finally { Directory.Delete(root, true); }

static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
