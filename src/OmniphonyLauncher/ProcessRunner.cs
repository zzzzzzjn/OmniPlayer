using System.Diagnostics;

namespace OmniphonyLauncher;

public static class ProcessRunner
{
    public static Process Start(string executable, IEnumerable<string> arguments)
    {
        var info = new ProcessStartInfo(executable) { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(executable)! };
        foreach (var arg in arguments) info.ArgumentList.Add(arg);
        return Process.Start(info) ?? throw new InvalidOperationException($"无法启动 {executable}");
    }

    public static async Task<string> ProbeAsync(string executable, string argument, int timeoutMs = 3500)
        => await ProbeAsync(executable, new[] { argument }, timeoutMs);

    public static async Task<string> ProbeAsync(string executable, IEnumerable<string> arguments, int timeoutMs = 3500)
    {
        var info = new ProcessStartInfo(executable) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true, WorkingDirectory = Path.GetDirectoryName(executable)! };
        foreach (var argument in arguments) info.ArgumentList.Add(argument);
        using var process = Process.Start(info) ?? throw new InvalidOperationException("进程启动失败");
        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            var output = await process.StandardOutput.ReadToEndAsync(cts.Token);
            var error = await process.StandardError.ReadToEndAsync(cts.Token);
            await process.WaitForExitAsync(cts.Token);
            return (output + Environment.NewLine + error).Trim();
        }
        catch (OperationCanceledException) { try { process.Kill(true); } catch { } return "探测超时"; }
    }
}
