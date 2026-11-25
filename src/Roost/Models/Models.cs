using System.Collections.Generic;

namespace Roost.Models;

public enum EngineKind { Wsa, BlueStacks, LDPlayer, MuMu, Nox, Generic, Custom }

public enum ThemeMode { System, Light, Dark }

public enum AppLanguage { Auto, English, Hebrew }

public enum ShortcutLocation { StartMenu, Desktop, Both }

/// <summary>A known or detected Android backend reachable over ADB.</summary>
public sealed class AndroidEngine
{
    public string Id { get; set; } = "";            // stable id, e.g. "wsa" or "custom:127.0.0.1:5555"
    public EngineKind Kind { get; set; }
    public string DisplayName { get; set; } = "";
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; }
    public string? Serial { get; set; }             // adb serial once connected (host:port)
    public bool IsConnected { get; set; }
    public string Status { get; set; } = "";        // human-readable last status

    public string Endpoint => $"{Host}:{Port}";
}

/// <summary>A third-party app installed inside an engine.</summary>
public sealed class InstalledApp
{
    public string PackageName { get; set; } = "";
    public string Label { get; set; } = "";
    public string VersionName { get; set; } = "";
    public string EngineId { get; set; } = "";
    public string? IconPath { get; set; }           // extracted PNG on disk, if available
}

/// <summary>Metadata parsed from an .apk / .xapk / .apks / .apkm before install.</summary>
public sealed class ApkInfo
{
    public string FilePath { get; set; } = "";
    public string PackageName { get; set; } = "";
    public string Label { get; set; } = "";
    public string VersionName { get; set; } = "";
    public long VersionCode { get; set; }
    public int MinSdk { get; set; }
    public string? LaunchableActivity { get; set; }
    public List<string> Permissions { get; } = new();
    public string? IconPath { get; set; }           // extracted icon preview on disk
    public bool IsSplit { get; set; }               // bundle of multiple apks
    public List<string> SplitFiles { get; } = new();// absolute paths to the apks to install together
    public bool Parsed { get; set; }
    public string? ParseError { get; set; }
}

public sealed class AppSettings
{
    public ThemeMode Theme { get; set; } = ThemeMode.Dark;
    public AppLanguage Language { get; set; } = AppLanguage.Auto;
    public string? AdbPath { get; set; }            // null => auto-locate
    public string? Aapt2Path { get; set; }
    public ShortcutLocation ShortcutLocation { get; set; } = ShortcutLocation.StartMenu;
    public bool AutoSuspendIdle { get; set; } = false;
    public bool StartWithWindows { get; set; } = false;
    public string? DefaultEngineId { get; set; }
    public List<AndroidEngine> CustomEngines { get; set; } = new();
}
