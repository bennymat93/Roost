using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Roost.Models;

namespace Roost.Services;

/// <summary>Builds native Windows shortcuts that re-launch an installed Android app through Roost.</summary>
public static class ShortcutMaker
{
    public static bool ForApp(AppSettings settings, AndroidEngine engine,
                              string package, string label, string? iconPng)
    {
        if (!OperatingSystem.IsWindows()) return false;

        var exe = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exe)) return false;

        var display = string.IsNullOrWhiteSpace(label) ? package : label;
        var fileName = Sanitize(display);
        var args = $"--launch \"{engine.Id}\" \"{package}\"";
        var ico = ShortcutService.PngToIco(iconPng, SettingsStore.IconCacheDir) ?? exe;
        var description = $"{display} — via Roost";

        var dirs = new List<string>();
        if (settings.ShortcutLocation is ShortcutLocation.StartMenu or ShortcutLocation.Both)
            dirs.Add(ShortcutService.StartMenuDir);
        if (settings.ShortcutLocation is ShortcutLocation.Desktop or ShortcutLocation.Both)
            dirs.Add(ShortcutService.DesktopDir);

        var ok = false;
        foreach (var dir in dirs)
        {
            var lnk = Path.Combine(dir, fileName + ".lnk");
            ok |= ShortcutService.Create(lnk, exe!, args, ico, description);
        }
        return ok;
    }

    public static bool ForLinuxApp(AppSettings settings, string distro, string appName, string exec)
    {
        if (!OperatingSystem.IsWindows()) return false;
        var exe = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exe)) return false;

        var fileName = Sanitize(appName);
        var args = $"--launch-linux \"{distro}\" \"{exec}\"";
        var description = $"{appName} — via Roost (Linux · {distro})";

        var dirs = new List<string>();
        if (settings.ShortcutLocation is ShortcutLocation.StartMenu or ShortcutLocation.Both)
            dirs.Add(ShortcutService.StartMenuDir);
        if (settings.ShortcutLocation is ShortcutLocation.Desktop or ShortcutLocation.Both)
            dirs.Add(ShortcutService.DesktopDir);

        var ok = false;
        foreach (var dir in dirs)
        {
            var lnk = Path.Combine(dir, fileName + ".lnk");
            ok |= ShortcutService.Create(lnk, exe!, args, exe, description);
        }
        return ok;
    }

    private static string Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Where(c => !invalid.Contains(c)).ToArray()).Trim();
        if (cleaned.Length == 0) cleaned = "App";
        return cleaned.Length > 80 ? cleaned[..80] : cleaned;
    }
}
