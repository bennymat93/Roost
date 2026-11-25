using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Roost.Services;

public readonly record struct ProcResult(int ExitCode, string StdOut, string StdErr, bool TimedOut)
{
    public bool Ok => !TimedOut && ExitCode == 0;
    public string All => string.IsNullOrEmpty(StdErr) ? StdOut : StdOut + "\n" + StdErr;
}

/// <summary>Runs an external executable and captures its output. No shell, args passed safely.</summary>
public static class ProcessRunner
{
    public static async Task<ProcResult> RunAsync(
        string exe, IEnumerable<string> args, int timeoutMs = 30000,
        string? workingDir = null, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = exe,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        if (!string.IsNullOrEmpty(workingDir)) psi.WorkingDirectory = workingDir;
        foreach (var a in args) psi.ArgumentList.Add(a);

        using var p = new Process { StartInfo = psi };
        var sb = new StringBuilder();
        var eb = new StringBuilder();
        var outDone = new TaskCompletionSource();
        var errDone = new TaskCompletionSource();
        p.OutputDataReceived += (_, e) => { if (e.Data is null) outDone.TrySetResult(); else sb.AppendLine(e.Data); };
        p.ErrorDataReceived  += (_, e) => { if (e.Data is null) errDone.TrySetResult(); else eb.AppendLine(e.Data); };

        try
        {
            if (!p.Start())
                return new ProcResult(-1, "", $"Failed to start: {exe}", false);
        }
        catch (Exception ex)
        {
            return new ProcResult(-1, "", $"{ex.GetType().Name}: {ex.Message}", false);
        }

        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var waited = Task.Run(() => p.WaitForExit(timeoutMs));
        var finished = await waited.ConfigureAwait(false);
        if (!finished)
        {
            try { p.Kill(entireProcessTree: true); } catch { /* ignore */ }
            return new ProcResult(-1, sb.ToString().TrimEnd(), eb.ToString().TrimEnd(), true);
        }

        // let the async readers flush
        await Task.WhenAny(Task.WhenAll(outDone.Task, errDone.Task), Task.Delay(500)).ConfigureAwait(false);
        return new ProcResult(p.ExitCode, sb.ToString().TrimEnd(), eb.ToString().TrimEnd(), false);
    }
}
