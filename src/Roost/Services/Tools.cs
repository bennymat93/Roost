using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Roost.Models;

namespace Roost.Services;

/// <summary>Known Android backends and the ADB endpoints they typically expose.</summary>
public static class EngineCatalog
{
    public sealed record Known(EngineKind Kind, string Name, string Host, int[] Ports, string Hint);

    public static readonly Known[] All =
    {
        new(EngineKind.Wsa,        "Windows Subsystem for Android", "127.0.0.1", new[]{58526},        "Community WSA build; apps feel native."),
        new(EngineKind.BlueStacks, "BlueStacks",                    "127.0.0.1", new[]{5555,5565,5575}, "Enable Android Debug Bridge in BlueStacks settings."),
        new(EngineKind.LDPlayer,   "LDPlayer",                      "127.0.0.1", new[]{5555,5554},     "ADB is on by default."),
        new(EngineKind.MuMu,       "MuMu Player",                   "127.0.0.1", new[]{7555,16384},    "Enable ADB in MuMu settings."),
        new(EngineKind.Nox,        "Nox Player",                    "127.0.0.1", new[]{62001,62025},   "ADB is on by default."),
        new(EngineKind.Generic,    "Generic emulator",              "127.0.0.1", new[]{5555},          "Any ADB-reachable Android device."),
    };

    public static IEnumerable<(EngineKind Kind, string Name, string Host, int Port, string Hint)> ProbeTargets()
    {
        foreach (var k in All)
            foreach (var p in k.Ports)
                yield return (k.Kind, k.Name, k.Host, p, k.Hint);
    }

    public static string EngineId(string host, int port) => $"{host}:{port}";
}

/// <summary>Locates bundled or system adb.exe / aapt2.exe.</summary>
public static class ToolLocator
{
    private static string? ExeDir =>
        Path.GetDirectoryName(Environment.ProcessPath ?? AppContext.BaseDirectory);

    public static string? FindAdb(AppSettings s) => Find("adb", s.AdbPath, SettingsStore.ToolsDir);
    public static string? FindAapt2(AppSettings s) => Find("aapt2", s.Aapt2Path, SettingsStore.ToolsDir);

    private static string? Find(string baseName, string? configured, string cacheDir)
    {
        var exe = OperatingSystem.IsWindows() ? baseName + ".exe" : baseName;

        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
            return configured;

        var candidates = new List<string>();
        void Add(string? dir) { if (!string.IsNullOrEmpty(dir)) candidates.Add(Path.Combine(dir!, exe)); }

        Add(cacheDir);
        Add(ExeDir);
        if (ExeDir is not null) Add(Path.Combine(ExeDir, "platform-tools"));

        // PATH
        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var dir in path.Split(Path.PathSeparator)) Add(dir);

        return candidates.FirstOrDefault(File.Exists);
    }
}

/// <summary>Best-effort Windows system checks for the setup wizard. All calls are guarded.</summary>
public static class SystemChecks
{
    private static async Task<string?> PsAsync(string command)
    {
        if (!OperatingSystem.IsWindows()) return null;
        try
        {
            var r = await ProcessRunner.RunAsync("powershell",
                new[] { "-NoProfile", "-NonInteractive", "-Command", command }, 15000);
            return r.Ok ? r.StdOut.Trim() : null;
        }
        catch { return null; }
    }

    /// <summary>True/false/null(unknown): is a hypervisor currently running?</summary>
    public static async Task<bool?> HypervisorPresentAsync()
    {
        var s = await PsAsync("[bool](Get-CimInstance Win32_ComputerSystem).HypervisorPresent");
        return ParseBool(s);
    }

    /// <summary>True/false/null: is CPU virtualization enabled in firmware?</summary>
    public static async Task<bool?> FirmwareVirtualizationAsync()
    {
        var s = await PsAsync("[bool](Get-CimInstance Win32_Processor | Select-Object -First 1 -Expand VirtualizationFirmwareEnabled)");
        return ParseBool(s);
    }

    /// <summary>State of an optional Windows feature (e.g. VirtualMachinePlatform, Microsoft-Windows-Subsystem-Linux).</summary>
    public static async Task<bool?> OptionalFeatureEnabledAsync(string feature)
    {
        var s = await PsAsync($"(Get-WindowsOptionalFeature -Online -FeatureName {feature} -ErrorAction SilentlyContinue).State -eq 'Enabled'");
        return ParseBool(s);
    }

    private static bool? ParseBool(string? s)
        => string.IsNullOrWhiteSpace(s) ? null
         : s.Trim().Equals("True", StringComparison.OrdinalIgnoreCase) ? true
         : s.Trim().Equals("False", StringComparison.OrdinalIgnoreCase) ? false
         : (bool?)null;
}
