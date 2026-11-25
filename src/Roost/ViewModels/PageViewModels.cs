using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Roost.Models;
using Roost.Services;

namespace Roost.ViewModels;

// ---------------- Dashboard ----------------
public partial class DashboardViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    public DashboardViewModel(MainViewModel main) => _main = main;

    public ObservableCollection<AndroidEngine> Engines => _main.Engines;
    public bool HasEngines => _main.Engines.Count > 0;

    [RelayCommand] private void GoInstall() => _main.Navigate(1);
    [RelayCommand] private void GoEngines() => _main.Navigate(3);
    [RelayCommand] private Task Refresh() => _main.RefreshEnginesCommand.ExecuteAsync(null);
}

// ---------------- My apps ----------------
public partial class AppsViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    public AppsViewModel(MainViewModel main) => _main = main;

    public MainViewModel Main => _main;
    public ObservableCollection<InstalledApp> Apps { get; } = new();

    [ObservableProperty] private bool _loading;
    [ObservableProperty] private string _message = "";

    private int _loadToken;   // guards against overlapping reloads

    public AndroidEngine? Engine => _main.SelectedEngine;

    [RelayCommand] private Task Refresh() => LoadAsync();

    public async Task LoadAsync()
    {
        var token = ++_loadToken;
        var engine = _main.SelectedEngine;
        if (engine?.Serial is null || !_main.Adb.Available)
        {
            Apps.Clear();
            Message = Loc["Dash_NoEngines"];
            return;
        }
        Loading = true;
        Message = Loc["Common_Working"];
        try
        {
            var pkgs = await _main.Adb.ListPackagesAsync(engine.Serial);
            if (token != _loadToken) return;     // a newer load superseded this one
            Apps.Clear();
            foreach (var p in pkgs)
            {
                var iconPath = Path.Combine(SettingsStore.IconCacheDir, p + ".png");
                Apps.Add(new InstalledApp
                {
                    PackageName = p,
                    Label = p,
                    EngineId = engine.Id,
                    IconPath = File.Exists(iconPath) ? iconPath : null,
                });
            }
            Message = Apps.Count == 0 ? Loc["Apps_None"] : "";
        }
        catch (Exception ex) { if (token == _loadToken) Message = ex.Message; }
        finally { if (token == _loadToken) Loading = false; }
    }

    [RelayCommand]
    private async Task Launch(InstalledApp? app)
    {
        var engine = _main.SelectedEngine;
        if (app is null || engine?.Serial is null) return;
        await _main.Adb.LaunchAsync(engine.Serial, app.PackageName);
    }

    [RelayCommand]
    private async Task Uninstall(InstalledApp? app)
    {
        var engine = _main.SelectedEngine;
        if (app is null || engine?.Serial is null) return;

        var ok = await Dialogs.ConfirmAsync(
            Loc["Common_Uninstall"],
            Loc["Apps_ConfirmUninstall"] + "\n\n" + (string.IsNullOrEmpty(app.Label) ? app.PackageName : app.Label),
            Loc["Common_Uninstall"], Loc["Common_Cancel"]);
        if (!ok) return;

        Loading = true;
        try
        {
            var r = await _main.Adb.UninstallAsync(engine.Serial, app.PackageName);
            if (r.StdOut.Contains("Success", System.StringComparison.OrdinalIgnoreCase)) Apps.Remove(app);
            else Message = r.All;
        }
        finally { Loading = false; }
    }

    [RelayCommand]
    private void Shortcut(InstalledApp? app)
    {
        var engine = _main.SelectedEngine;
        if (app is null || engine is null) return;
        ShortcutMaker.ForApp(_main.Settings, engine, app.PackageName, app.Label, app.IconPath);
        Message = Loc["Common_Shortcut"] + " ✓";
    }
}

// ---------------- About ----------------
public partial class AboutViewModel : ViewModelBase
{
    public AboutViewModel(MainViewModel main) { }

    public string VersionText => "1.3.0";

    [RelayCommand]
    private void OpenUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { /* ignore */ }
    }
}
