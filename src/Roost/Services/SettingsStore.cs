using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Roost.Models;

namespace Roost.Services;

/// <summary>Loads and saves <see cref="AppSettings"/> as JSON under %APPDATA%\Roost.</summary>
public static class SettingsStore
{
    public static string RoostDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Roost");

    public static string SettingsPath => Path.Combine(RoostDir, "settings.json");
    public static string IconCacheDir => Path.Combine(RoostDir, "icons");
    public static string ToolsDir => Path.Combine(RoostDir, "platform-tools");

    private static readonly JsonSerializerOptions Opts = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var s = JsonSerializer.Deserialize<AppSettings>(json, Opts);
                if (s is not null) return s;
            }
        }
        catch { /* fall through to defaults */ }
        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(RoostDir);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, Opts));
        }
        catch { /* best effort */ }
    }

    public static void EnsureDirs()
    {
        try
        {
            Directory.CreateDirectory(RoostDir);
            Directory.CreateDirectory(IconCacheDir);
            Directory.CreateDirectory(ToolsDir);
        }
        catch { /* ignore */ }
    }
}
