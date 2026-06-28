using Microsoft.Win32;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace OmniphonyLauncher;

public partial class MainWindow : Window
{
    private LauncherSettings _settings = LauncherSettings.Load();
    private sealed record AudioDeviceItem(string Id, string DisplayName);

    public MainWindow()
    {
        App.SetLanguage(_settings.Language);
        InitializeComponent();
        PathDiscovery.FillMissing(_settings);
        ToUi();
        Loaded += async (_, _) => { await RefreshAudioDevicesAsync(); await DiagnoseAsync(false); };
    }

    private string T(string key) => Application.Current.TryFindResource(key)?.ToString() ?? key;
    private bool IsEnglish => _settings.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase);
    private string L(string chinese, string english) => IsEnglish ? english : chinese;

    private async void Language_Click(object sender, RoutedEventArgs e)
    {
        FromUi();
        _settings.Language = _settings.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "zh-CN" : "en-US";
        _settings.Save();
        App.SetLanguage(_settings.Language);
        await RefreshAudioDevicesAsync();
        await DiagnoseAsync(false);
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var previousFocus = Keyboard.FocusedElement;
        new AboutWindow { Owner = this }.ShowDialog();
        previousFocus?.Focus();
    }

    private void ToUi()
    {
        MpvPathBox.Text = _settings.MpvPath; StudioPathBox.Text = _settings.StudioPath;
        OrenderPathBox.Text = _settings.OrenderPath; BridgePathBox.Text = _settings.BridgePath;
        ConfigPathBox.Text = _settings.ConfigPath; MediaPathBox.Text = _settings.MediaPath;
        StartStudioCheck.IsChecked = _settings.StartStudio; OscCheck.IsChecked = _settings.EnableOsc;
        ShowOverlayCheck.IsChecked = _settings.ShowMpvOverlay;
        OscPortBox.Text = _settings.OscPort.ToString(); ExtraArgsBox.Text = _settings.ExtraArguments;
    }

    private void FromUi()
    {
        _settings.MpvPath = MpvPathBox.Text.Trim(); _settings.StudioPath = StudioPathBox.Text.Trim();
        _settings.OrenderPath = OrenderPathBox.Text.Trim(); _settings.BridgePath = BridgePathBox.Text.Trim();
        _settings.ConfigPath = ConfigPathBox.Text.Trim(); _settings.MediaPath = MediaPathBox.Text.Trim();
        _settings.StartStudio = StartStudioCheck.IsChecked == true; _settings.EnableOsc = OscCheck.IsChecked == true;
        _settings.ShowMpvOverlay = ShowOverlayCheck.IsChecked == true;
        _settings.AudioDevice = AudioDeviceBox.SelectedValue?.ToString() ?? _settings.AudioDevice;
        _settings.OscPort = int.TryParse(OscPortBox.Text, out var port) ? port : 9000;
        _settings.ExtraArguments = ExtraArgsBox.Text.Trim();
    }

    private void Save() { FromUi(); _settings.Save(); }
    private void Log(string message) { LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}"); LogBox.ScrollToEnd(); }

    private void PickMedia_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = T("MediaFilter") };
        if (dialog.ShowDialog() == true) MediaPathBox.Text = dialog.FileName;
    }

    private void BrowseExecutable_Click(object sender, RoutedEventArgs e)
    {
        var button = (FrameworkElement)sender; var tag = button.Tag?.ToString();
        var isConfig = tag == "config"; var isBridge = tag == "bridge";
        var allFiles = T("AllFiles");
        var dialog = new OpenFileDialog { Filter = isConfig ? $"YAML|*.yaml;*.yml|{allFiles}" : isBridge ? $"DLL|*.dll|{allFiles}" : $"Executable|*.exe|{allFiles}" };
        if (dialog.ShowDialog() != true) return;
        if (tag == "mpv") MpvPathBox.Text = dialog.FileName; else if (tag == "studio") StudioPathBox.Text = dialog.FileName;
        else if (tag == "orender") OrenderPathBox.Text = dialog.FileName; else if (tag == "bridge") BridgePathBox.Text = dialog.FileName; else ConfigPathBox.Text = dialog.FileName;
    }

    private void AutoDiscover_Click(object sender, RoutedEventArgs e) { FromUi(); PathDiscovery.FillMissing(_settings); ToUi(); Log(L("已完成路径自动发现", "Component paths discovered")); }

    private void SaveConfig_Click(object sender, RoutedEventArgs e)
    {
        try { Save(); RequireFile(_settings.BridgePath, "bridge DLL"); var result = ConfigEditor.ApplyBridgePath(_settings.ConfigPath, _settings.BridgePath); Log(result); MessageBox.Show(result, "Omniphony Launcher"); }
        catch (Exception ex) { Fail(ex); }
    }

    private void Launch_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Save(); RequireFile(_settings.MpvPath, "mpv-omniphony"); RequireFile(_settings.BridgePath, "bridge DLL"); RequireFile(_settings.MediaPath, L("片源", "media file"));
            ConfigEditor.ApplyBridgePath(_settings.ConfigPath, _settings.BridgePath);
            if (_settings.StartStudio && File.Exists(_settings.StudioPath) && !Process.GetProcessesByName("omniphony-studio").Any()) ProcessRunner.Start(_settings.StudioPath, []);
            var args = new List<string> { "--ad=orender", $"--ad-orender-config={_settings.ConfigPath}", $"--ad-orender-bridge-path={_settings.BridgePath}" };
            if (_settings.EnableOsc) { args.Add("--ad-orender-osc"); args.Add($"--ad-orender-osc-rx-port={_settings.OscPort}"); }
            if (!_settings.ShowMpvOverlay) args.Add($"--script={EnsureOverlayDisableScript()}");
            if (!string.IsNullOrWhiteSpace(_settings.AudioDevice) && _settings.AudioDevice != "auto") args.Add($"--audio-device={_settings.AudioDevice}");
            args.AddRange(SplitArguments(_settings.ExtraArguments)); args.Add(_settings.MediaPath);
            ProcessRunner.Start(_settings.MpvPath, args); Log(L("已启动 mpv：", "Started mpv: ") + string.Join(" ", args.Select(QuoteForLog)));
        }
        catch (Exception ex) { Fail(ex); }
    }

    private void LaunchStudio_Click(object sender, RoutedEventArgs e) { try { Save(); RequireFile(_settings.StudioPath, "Studio"); ProcessRunner.Start(_settings.StudioPath, []); Log(L("已启动 Studio", "Started Studio")); } catch (Exception ex) { Fail(ex); } }

    private void OpenConfigFolder_Click(object sender, RoutedEventArgs e)
    {
        Save(); var folder = Path.GetDirectoryName(_settings.ConfigPath); if (folder is not null) Process.Start(new ProcessStartInfo("explorer.exe", folder) { UseShellExecute = true });
    }

    private async void Diagnose_Click(object sender, RoutedEventArgs e) => await DiagnoseAsync(true);

    private async void RefreshAudioDevices_Click(object sender, RoutedEventArgs e) => await RefreshAudioDevicesAsync(true);

    private async Task RefreshAudioDevicesAsync(bool writeLog = false)
    {
        FromUi();
        var selected = _settings.AudioDevice;
        var devices = new List<AudioDeviceItem> { new("auto", T("AutoDevice")) };
        if (File.Exists(_settings.MpvPath))
        {
            var output = await ProcessRunner.ProbeAsync(_settings.MpvPath, new[] { "--no-config", "--audio-device=help", "--idle=no" }, 7000);
            foreach (var line in output.Split('\n'))
            {
                var match = Regex.Match(line.Trim(), "^'([^']+)'\\s+\\((.*)\\)$");
                if (match.Success && match.Groups[1].Value != "auto")
                    devices.Add(new(match.Groups[1].Value, $"{match.Groups[2].Value}  [{match.Groups[1].Value.Split('/')[0].ToUpperInvariant()}]"));
            }
        }
        AudioDeviceBox.ItemsSource = devices;
        AudioDeviceBox.SelectedValue = devices.Any(x => x.Id == selected) ? selected : "auto";
        if (writeLog) Log(IsEnglish
            ? $"Found {devices.Count - 1} audio outputs; Dante ASIO {(devices.Any(x => x.Id.StartsWith("asio/Dante", StringComparison.OrdinalIgnoreCase)) ? "available" : "not found")}"
            : $"已枚举 {devices.Count - 1} 个音频输出；Dante ASIO {(devices.Any(x => x.Id.StartsWith("asio/Dante", StringComparison.OrdinalIgnoreCase)) ? "可用" : "未发现")}");
    }
    private async Task DiagnoseAsync(bool verbose)
    {
        Save(); var checks = new[] { ("mpv-omniphony", _settings.MpvPath), ("Studio", _settings.StudioPath), ("orender CLI", _settings.OrenderPath), ("bridge DLL", _settings.BridgePath), ("config.yaml", _settings.ConfigPath) };
        var requiredOk = File.Exists(_settings.MpvPath) && File.Exists(_settings.BridgePath) && File.Exists(_settings.ConfigPath);
        if (verbose) foreach (var (name, path) in checks) Log($"{(File.Exists(path) ? "OK" : L("缺失", "MISS")),-4} {name}: {path}");
        if (verbose && File.Exists(_settings.MpvPath))
        {
            var version = await ProcessRunner.ProbeAsync(_settings.MpvPath, "--version");
            Log(L("mpv 版本: ", "mpv version: ") + version.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim());
            var options = await ProcessRunner.ProbeAsync(_settings.MpvPath, "--list-options", 7000);
            Log(options.Contains("ad-orender-bridge-path", StringComparison.OrdinalIgnoreCase) ? L("OK   检测到 ad_orender 参数", "OK   ad_orender option detected") : L("失败 当前 mpv 似乎不是 mpv-omniphony 构建", "FAIL Current mpv does not appear to be an mpv-omniphony build"));
            Log(AudioDeviceBox.Items.OfType<AudioDeviceItem>().Any(x => x.Id.StartsWith("asio/Dante", StringComparison.OrdinalIgnoreCase)) ? L("OK   检测到 Dante Virtual Soundcard ASIO 输出", "OK   Dante Virtual Soundcard ASIO output detected") : L("提示 未检测到 Dante ASIO 输出，请先启动并配置 DVS", "NOTE Dante ASIO output was not detected; start and configure DVS first"));
        }
        OverallStatus.Text = requiredOk ? T("StatusReady") : T("StatusNeedsAttention");
        OverallStatus.Foreground = requiredOk ? (System.Windows.Media.Brush)FindResource("TextPrimary") : System.Windows.Media.Brushes.Orange;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0) MediaPathBox.Text = files[0];
    }

    private void ClearLog_Click(object sender, RoutedEventArgs e) => LogBox.Clear();
    private void Fail(Exception ex) { Log(L("错误: ", "Error: ") + ex.Message); MessageBox.Show(ex.Message, L("启动失败", "Launch failed"), MessageBoxButton.OK, MessageBoxImage.Error); }
    private void RequireFile(string path, string label) { if (!File.Exists(path)) throw new FileNotFoundException(IsEnglish ? $"Cannot find {label}: {path}" : $"找不到{label}：{path}"); }
    private static string QuoteForLog(string arg) => arg.Contains(' ') ? $"\"{arg}\"" : arg;

    private static string EnsureOverlayDisableScript()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniphonyLauncher");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "disable-omniphony-overlay.lua");
        const string script = "mp.register_event('file-loaded', function() mp.commandv('script-message', 'omniphony-overlay', 'disable') end)\n";
        if (!File.Exists(path) || File.ReadAllText(path) != script) File.WriteAllText(path, script, new UTF8Encoding(false));
        return path;
    }

    private IEnumerable<string> SplitArguments(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine)) yield break;
        var current = new StringBuilder(); var quoted = false;
        for (var i = 0; i < commandLine.Length; i++)
        {
            var c = commandLine[i];
            if (c == '"') { quoted = !quoted; continue; }
            if (char.IsWhiteSpace(c) && !quoted) { if (current.Length > 0) { yield return current.ToString(); current.Clear(); } }
            else current.Append(c);
        }
        if (quoted) throw new FormatException(L("附加参数中有未闭合的双引号", "Additional arguments contain an unclosed quotation mark"));
        if (current.Length > 0) yield return current.ToString();
    }
}
