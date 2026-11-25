using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Roost.Services;

/// <summary>Downloads Google's official platform-tools (adb) into %APPDATA%\Roost so the
/// tool works out of the box. aapt2 lives in build-tools and stays optional.</summary>
public static class ToolDownloader
{
    private const string Url = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";

    public static async Task<string?> DownloadAdbAsync(Action<string>? progress = null)
    {
        try
        {
            SettingsStore.EnsureDirs();
            var tmpZip = Path.Combine(Path.GetTempPath(), $"roost-ptools-{Guid.NewGuid():N}.zip");

            progress?.Invoke("Downloading platform-tools…");
            using (var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) })
            using (var resp = await http.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead))
            {
                resp.EnsureSuccessStatusCode();
                await using var src = await resp.Content.ReadAsStreamAsync();
                await using var dst = File.Create(tmpZip);
                await src.CopyToAsync(dst);
            }

            progress?.Invoke("Extracting…");
            var dest = SettingsStore.ToolsDir;
            // clean any previous extraction of the top-level folder
            var ptDir = Path.Combine(dest, "platform-tools");
            if (Directory.Exists(ptDir)) Directory.Delete(ptDir, true);
            ZipFile.ExtractToDirectory(tmpZip, dest, overwriteFiles: true);
            try { File.Delete(tmpZip); } catch { }

            var adb = Directory.GetFiles(dest, "adb.exe", SearchOption.AllDirectories).FirstOrDefault();
            progress?.Invoke(adb is null ? "adb.exe not found after extract." : "Ready.");
            return adb;
        }
        catch (Exception ex)
        {
            progress?.Invoke("Failed: " + ex.Message);
            return null;
        }
    }
}
