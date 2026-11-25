using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Roost.Models;
using Roost.Services;

namespace Roost.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    public SettingsViewModel(MainViewModel main)
    {
        _main = main;
        var s = main.Settings;
        _themeIndex = (int)s.Theme;
        _languageIndex = (int)s.Language;
        _shortcutIndex = (int)s.ShortcutLocation;
        _adbPath = s.AdbPath ?? "";
        _aapt2Path = s.Aapt2Path ?? "";
        _autoSuspend = s.AutoSuspendIdle;
        _startWithWindows = s.StartWithWindows;
        RefreshToolStatus();
    }

    [ObservableProperty] private int _themeIndex;
    [ObservableProperty] private int _languageIndex;
    [ObservableProperty] private int _shortcutIndex;
    [ObservableProperty] private string _adbPath = "";
    [ObservableProperty] private string _aapt2Path = "";
    [ObservableProperty] private bool _autoSuspend;
    [ObservableProperty] private bool _startWithWindows;
    [ObservableProperty] private string _adbStatus = "";
    [ObservableProperty] private string _aapt2Status = "";
    [ObservableProperty] private string _saved = "";

    public void SetAdbPath(string p) { AdbPath = p; }
    public void SetAapt2Path(string p) { Aapt2Path = p; }

    private void RefreshToolStatus()
    {
        var s = _main.Settings;
        var adb = ToolLocator.FindAdb(s);
        var aapt = ToolLocator.FindAapt2(s);
        AdbStatus = adb is null ? Loc["Set_ToolsMissing"] : $"{Loc["Set_ToolsFound"]}: {adb}";
        Aapt2Status = aapt is null ? Loc["Set_ToolsMissing"] : $"{Loc["Set_ToolsFound"]}: {aapt}";
    }

    [RelayCommand]
    private void Save()
    {
        var s = _main.Settings;
        var oldTheme = s.Theme;
        var oldLang = s.Language;

        s.Theme = (ThemeMode)ThemeIndex;
        s.Language = (AppLanguage)LanguageIndex;
        s.ShortcutLocation = (ShortcutLocation)ShortcutIndex;
        s.AdbPath = string.IsNullOrWhiteSpace(AdbPath) ? null : AdbPath.Trim();
        s.Aapt2Path = string.IsNullOrWhiteSpace(Aapt2Path) ? null : Aapt2Path.Trim();
        s.AutoSuspendIdle = AutoSuspend;
        s.StartWithWindows = StartWithWindows;
        SettingsStore.Save(s);

        _main.ReloadAdb();
        RefreshToolStatus();
        ApplyStartup(StartWithWindows);
        Saved = "✓";

        if (oldTheme != s.Theme || oldLang != s.Language)
            App.ApplyAndRestart(s);
    }

    private static void ApplyStartup(bool enable)
    {
        if (!OperatingSystem.IsWindows()) return;
        try
        {
            var startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var lnk = Path.Combine(startup, "Roost.lnk");
            var exe = Environment.ProcessPath;
            if (enable && !string.IsNullOrEmpty(exe))
                ShortcutService.Create(lnk, exe!, "", exe, "Roost");
            else if (File.Exists(lnk))
                File.Delete(lnk);
        }
        catch { /* best effort */ }
    }
}
