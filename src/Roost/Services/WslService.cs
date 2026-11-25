using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Roost.Models;

namespace Roost.Services;

/// <summary>Runs Linux GUI software on Windows 11 through WSL 2 + WSLg.
/// Lists distros, enumerates .desktop apps, installs packages, launches apps.</summary>
public static class WslService
{
    private const string Wsl = "wsl.exe";

    public static bool PlatformSupported => OperatingSystem.IsWindows();

    // wsl.exe emits UTF-16LE for its own listing commands.
    private static Task<ProcResult> RunWslList(IEnumerable<string> args, int timeout = 20000)
        => ProcessRunner.RunAsync(Wsl, args, timeout, outputEncoding: Encoding.Unicode);

    // Commands executed *inside* a distro produce normal UTF-8.
    private static Task<ProcResult> RunInDistro(string distro, string bash, int timeout = 60000)
        => ProcessRunner.RunAsync(Wsl, new[] { "-d", distro, "--", "bash", "-lc", bash }, timeout);

    public static async Task<bool> IsInstalledAsync()
    {
        if (!PlatformSupported) return false;
        try
        {
            var r = await RunWslList(new[] { "--status" }, 12000);
            return r.ExitCode == 0 || !string.IsNullOrWhiteSpace(r.StdOut);
        }
        catch { return false; }
    }

    public static async Task<List<WslDistro>> ListDistrosAsync()
    {
        var list = new List<WslDistro>();
        if (!PlatformSupported) return list;
        try
        {
            var r = await RunWslList(new[] { "-l", "-v" }, 15000);
            foreach (var raw in r.StdOut.Replace("\0", "").Split('\n'))
            {
                var line = raw.TrimEnd();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var trimmed = line.TrimStart();
                if (trimmed.StartsWith("NAME", StringComparison.OrdinalIgnoreCase)) continue;

                var isDefault = trimmed.StartsWith("*");
                if (isDefault) trimmed = trimmed[1..].TrimStart();

                var cols = Regex.Split(trimmed, @"\s{2,}")
                                .Where(c => c.Length > 0).ToArray();
                if (cols.Length == 0) continue;

                var d = new WslDistro { Name = cols[0].Trim(), IsDefault = isDefault };
                if (cols.Length >= 2) d.State = cols[1].Trim();
                if (cols.Length >= 3 && int.TryParse(cols[2].Trim(), out var v)) d.Version = v;
                list.Add(d);
            }
        }
        catch { /* ignore */ }
        return list;
    }

    public static async Task<List<LinuxApp>> ListAppsAsync(string distro)
    {
        var apps = new List<LinuxApp>();
        if (!PlatformSupported || string.IsNullOrEmpty(distro)) return apps;

        // Emit "Name|||Exec" for each visible .desktop entry.
        const string script =
            "for d in /usr/share/applications ~/.local/share/applications /var/lib/flatpak/exports/share/applications; do " +
            "[ -d \"$d\" ] || continue; for f in \"$d\"/*.desktop; do [ -e \"$f\" ] || continue; " +
            "n=$(grep -m1 '^Name=' \"$f\" | cut -d= -f2-); " +
            "e=$(grep -m1 '^Exec=' \"$f\" | cut -d= -f2-); " +
            "nd=$(grep -m1 '^NoDisplay=' \"$f\" | cut -d= -f2-); " +
            "t=$(grep -m1 '^Type=' \"$f\" | cut -d= -f2-); " +
            "[ -n \"$n\" ] && [ -n \"$e\" ] && [ \"$nd\" != \"true\" ] && [ \"$t\" = \"Application\" ] && echo \"$n|||$e\"; " +
            "done; done | sort -u";

        try
        {
            var r = await RunInDistro(distro, script, 40000);
            foreach (var line in r.StdOut.Split('\n'))
            {
                var parts = line.Split("|||", 2, StringSplitOptions.None);
                if (parts.Length != 2) continue;
                var name = parts[0].Trim();
                var exec = CleanExec(parts[1].Trim());
                if (name.Length == 0 || exec.Length == 0) continue;
                apps.Add(new LinuxApp { Name = name, Exec = exec, Distro = distro });
            }
        }
        catch { /* ignore */ }

        return apps
            .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Strip .desktop Exec field codes like %U %F %i %c %k.</summary>
    public static string CleanExec(string exec)
    {
        var cleaned = Regex.Replace(exec, @"%[a-zA-Z]", "").Trim();
        return Regex.Replace(cleaned, @"\s{2,}", " ");
    }

    public static Task<ProcResult> LaunchAsync(string distro, string exec)
    {
        var bash = $"nohup {exec} >/dev/null 2>&1 &";
        return ProcessRunner.RunAsync(Wsl, new[] { "-d", distro, "--", "bash", "-lc", bash }, 15000);
    }

    public static Task<ProcResult> InstallPackageAsync(string distro, string package)
    {
        var safe = Regex.Replace(package.Trim(), @"[^a-zA-Z0-9\.\-\+_ ]", "");
        // sudo -n fails fast (instead of hanging on a password prompt) when the distro's
        // default user does not have passwordless sudo.
        var bash = $"sudo -n sh -c 'apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y {safe}'";
        return RunInDistro(distro, bash, 1800000);
    }
}
