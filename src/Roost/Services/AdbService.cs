using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roost.Services;

/// <summary>Thin async wrapper over the adb command-line tool. Backend-agnostic:
/// talks to WSA or any emulator that exposes an ADB endpoint.</summary>
public sealed class AdbService
{
    public string AdbPath { get; }
    public bool Available => !string.IsNullOrEmpty(AdbPath);

    public AdbService(string? adbPath) => AdbPath = adbPath ?? "";

    private Task<ProcResult> Run(IEnumerable<string> args, int timeout = 120000)
        => Available
            ? ProcessRunner.RunAsync(AdbPath, args, timeout)
            : Task.FromResult(new ProcResult(-1, "", "adb not found", false));

    private static IEnumerable<string> ForSerial(string serial, params string[] rest)
    {
        yield return "-s";
        yield return serial;
        foreach (var r in rest) yield return r;
    }

    public Task<ProcResult> StartServerAsync() => Run(new[] { "start-server" }, 20000);
    public Task<ProcResult> KillServerAsync() => Run(new[] { "kill-server" }, 10000);

    /// <summary>adb connect host:port. Returns true if the endpoint accepted the connection.</summary>
    public async Task<bool> ConnectAsync(string host, int port)
    {
        var r = await Run(new[] { "connect", $"{host}:{port}" }, 15000);
        var text = r.All.ToLowerInvariant();
        return text.Contains("connected to") || text.Contains("already connected");
    }

    public Task<ProcResult> DisconnectAsync(string serial) => Run(new[] { "disconnect", serial }, 10000);

    /// <summary>Serials currently in the "device" (ready) state.</summary>
    public async Task<List<string>> DevicesAsync()
    {
        var list = new List<string>();
        var r = await Run(new[] { "devices" }, 15000);
        foreach (var raw in r.StdOut.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("List of devices")) continue;
            var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && parts[1].Trim() == "device")
                list.Add(parts[0].Trim());
        }
        return list;
    }

    public Task<ProcResult> InstallAsync(string serial, string apkPath, bool reinstall)
    {
        var args = new List<string>(ForSerial(serial, "install"));
        if (reinstall) args.Add("-r");
        args.Add("-g");               // grant runtime permissions
        args.Add(apkPath);
        return Run(args, 600000);
    }

    public Task<ProcResult> InstallMultipleAsync(string serial, IEnumerable<string> apks, bool reinstall)
    {
        var args = new List<string>(ForSerial(serial, "install-multiple"));
        if (reinstall) args.Add("-r");
        args.Add("-g");
        args.AddRange(apks);
        return Run(args, 900000);
    }

    public Task<ProcResult> UninstallAsync(string serial, string package)
        => Run(ForSerial(serial, "uninstall", package), 120000);

    /// <summary>Third-party package names (pm list packages -3).</summary>
    public async Task<List<string>> ListPackagesAsync(string serial)
    {
        var r = await Run(ForSerial(serial, "shell", "pm", "list", "packages", "-3"), 60000);
        return r.StdOut.Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.StartsWith("package:"))
            .Select(l => l["package:".Length..].Trim())
            .Where(l => l.Length > 0)
            .OrderBy(l => l, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<string?> VersionNameAsync(string serial, string package)
    {
        var r = await Run(ForSerial(serial, "shell", "dumpsys", "package", package), 30000);
        foreach (var line in r.StdOut.Split('\n'))
        {
            var t = line.Trim();
            if (t.StartsWith("versionName="))
                return t["versionName=".Length..].Trim();
        }
        return null;
    }

    /// <summary>Launch a package by its launcher intent (works across all backends).</summary>
    public Task<ProcResult> LaunchAsync(string serial, string package)
        => Run(ForSerial(serial, "shell", "monkey", "-p", package, "-c",
               "android.intent.category.LAUNCHER", "1"), 30000);

    /// <summary>Best-effort launchable activity "pkg/activity", or null.</summary>
    public async Task<string?> ResolveLaunchActivityAsync(string serial, string package)
    {
        var r = await Run(ForSerial(serial, "shell", "cmd", "package", "resolve-activity",
                          "--brief", "-c", "android.intent.category.LAUNCHER", package), 30000);
        var last = r.StdOut.Split('\n').Select(l => l.Trim())
                    .LastOrDefault(l => l.Contains('/'));
        return string.IsNullOrEmpty(last) ? null : last;
    }
}
