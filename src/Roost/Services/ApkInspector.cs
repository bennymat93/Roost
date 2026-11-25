using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Roost.Models;

namespace Roost.Services;

/// <summary>Reads metadata and an icon from an .apk, or a split bundle (.xapk/.apks/.apkm).
/// aapt2 is optional: when it is missing the file can still be installed, just without rich metadata.</summary>
public static class ApkInspector
{
    private static readonly string[] BundleExts = { ".xapk", ".apks", ".apkm", ".zip" };

    private static string ExtractRoot => Path.Combine(Path.GetTempPath(), "Roost", "extract");

    /// <summary>Delete leftover split-bundle extractions from previous runs.</summary>
    public static void CleanupTemp()
    {
        try { if (Directory.Exists(ExtractRoot)) Directory.Delete(ExtractRoot, recursive: true); }
        catch { /* best effort */ }
    }

    public static async Task<ApkInfo> InspectAsync(string filePath, string? aapt2Path)
    {
        var info = new ApkInfo { FilePath = filePath };
        var ext = Path.GetExtension(filePath).ToLowerInvariant();

        try
        {
            string baseApk;
            if (BundleExts.Contains(ext))
            {
                info.IsSplit = true;
                var dir = Path.Combine(ExtractRoot, Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(dir);
                ZipFile.ExtractToDirectory(filePath, dir, overwriteFiles: true);
                var apks = Directory.GetFiles(dir, "*.apk", SearchOption.AllDirectories);
                if (apks.Length == 0)
                {
                    info.ParseError = "No .apk found inside the bundle.";
                    return info;
                }
                info.SplitFiles.AddRange(apks);
                baseApk = ChooseBaseApk(apks);
            }
            else
            {
                info.SplitFiles.Add(filePath);
                baseApk = filePath;
            }

            info.Label = Path.GetFileNameWithoutExtension(filePath);

            if (!string.IsNullOrEmpty(aapt2Path) && File.Exists(aapt2Path))
                await ReadBadgingAsync(aapt2Path!, baseApk, info);

            TryExtractIcon(baseApk, info);
            info.Parsed = !string.IsNullOrEmpty(info.PackageName);
        }
        catch (Exception ex)
        {
            info.ParseError = $"{ex.GetType().Name}: {ex.Message}";
        }
        return info;
    }

    private static string ChooseBaseApk(string[] apks)
    {
        // prefer an explicit base.apk, else the apk without a "config"/"split" marker, else the largest file
        var baseNamed = apks.FirstOrDefault(a =>
            Path.GetFileName(a).Equals("base.apk", StringComparison.OrdinalIgnoreCase));
        if (baseNamed is not null) return baseNamed;

        var nonSplit = apks.Where(a =>
        {
            var n = Path.GetFileName(a).ToLowerInvariant();
            return !n.Contains("config.") && !n.StartsWith("split");
        }).ToList();
        var pool = nonSplit.Count > 0 ? nonSplit : apks.ToList();
        return pool.OrderByDescending(a => new FileInfo(a).Length).First();
    }

    private static async Task ReadBadgingAsync(string aapt2, string apk, ApkInfo info)
    {
        var r = await ProcessRunner.RunAsync(aapt2, new[] { "dump", "badging", apk }, 60000);
        if (!r.Ok && string.IsNullOrEmpty(r.StdOut)) return;
        var text = r.StdOut;

        var pkg = Regex.Match(text, @"package:\s*name='([^']+)'");
        if (pkg.Success) info.PackageName = pkg.Groups[1].Value;
        var vn = Regex.Match(text, @"versionName='([^']*)'");
        if (vn.Success) info.VersionName = vn.Groups[1].Value;
        var vc = Regex.Match(text, @"versionCode='([^']*)'");
        if (vc.Success && long.TryParse(vc.Groups[1].Value, out var vcode)) info.VersionCode = vcode;
        var sdk = Regex.Match(text, @"sdkVersion:'([^']*)'");
        if (sdk.Success && int.TryParse(sdk.Groups[1].Value, out var ms)) info.MinSdk = ms;

        var label = Regex.Match(text, @"application-label(?:-\w+)?:'([^']*)'");
        if (label.Success && label.Groups[1].Value.Length > 0) info.Label = label.Groups[1].Value;
        var act = Regex.Match(text, @"launchable-activity:\s*name='([^']+)'");
        if (act.Success) info.LaunchableActivity = act.Groups[1].Value;

        foreach (Match m in Regex.Matches(text, @"uses-permission:\s*name='([^']+)'"))
            info.Permissions.Add(m.Groups[1].Value);
    }

    private static void TryExtractIcon(string apk, ApkInfo info)
    {
        try
        {
            using var z = ZipFile.OpenRead(apk);
            // prefer a PNG mipmap/drawable launcher icon at the highest density
            var candidates = z.Entries
                .Where(e => e.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                            && (e.FullName.Contains("mipmap") || e.FullName.Contains("drawable"))
                            && (e.FullName.Contains("ic_launcher") || e.FullName.Contains("icon")))
                .OrderByDescending(e => DensityRank(e.FullName))
                .ThenByDescending(e => e.Length)
                .ToList();
            var entry = candidates.FirstOrDefault();
            if (entry is null) return;

            SettingsStore.EnsureDirs();
            var safePkg = string.IsNullOrEmpty(info.PackageName)
                ? Path.GetFileNameWithoutExtension(apk) : info.PackageName;
            var outPath = Path.Combine(SettingsStore.IconCacheDir, safePkg + ".png");
            using (var src = entry.Open())
            using (var dst = File.Create(outPath))
                src.CopyTo(dst);
            info.IconPath = outPath;
        }
        catch { /* icon is optional */ }
    }

    private static int DensityRank(string path)
    {
        var p = path.ToLowerInvariant();
        if (p.Contains("xxxhdpi")) return 6;
        if (p.Contains("xxhdpi")) return 5;
        if (p.Contains("xhdpi")) return 4;
        if (p.Contains("hdpi")) return 3;
        if (p.Contains("mdpi")) return 2;
        if (p.Contains("ldpi")) return 1;
        return 0;
    }
}
