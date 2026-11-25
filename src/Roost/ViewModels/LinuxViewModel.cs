using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Roost.Models;
using Roost.Services;

namespace Roost.ViewModels;

public partial class LinuxViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    public LinuxViewModel(MainViewModel main) => _main = main;

    public ObservableCollection<WslDistro> Distros { get; } = new();
    public ObservableCollection<LinuxApp> Apps { get; } = new();

    [ObservableProperty] private WslDistro? _selectedDistro;
    [ObservableProperty] private bool _wslInstalled = true;
    [ObservableProperty] private bool _loading;
    [ObservableProperty] private string _status = "";
    [ObservableProperty] private string _packageName = "";
    [ObservableProperty] private string _installLog = "";
    [ObservableProperty] private bool _installing;

    public bool NoWsl => !WslInstalled;

    partial void OnWslInstalledChanged(bool value) => OnPropertyChanged(nameof(NoWsl));

    public async Task RefreshAsync()
    {
        Loading = true;
        Status = Loc["Common_Working"];
        try
        {
            WslInstalled = await WslService.IsInstalledAsync();
            Distros.Clear();
            if (WslInstalled)
            {
                foreach (var d in await WslService.ListDistrosAsync()) Distros.Add(d);
                SelectedDistro = Distros.FirstOrDefault(d => d.IsDefault) ?? Distros.FirstOrDefault();
            }
            Status = WslInstalled
                ? (Distros.Count == 0 ? Loc["Lnx_NoDistro"] : $"{Distros.Count} {Loc["Lnx_Distros"]}")
                : Loc["Lnx_NoWsl"];
        }
        catch (System.Exception ex) { Status = ex.Message; }
        finally { Loading = false; }
    }

    partial void OnSelectedDistroChanged(WslDistro? value) => _ = LoadAppsAsync();

    private int _appsToken;

    public async Task LoadAppsAsync()
    {
        var token = ++_appsToken;
        if (SelectedDistro is null) { Apps.Clear(); return; }
        Loading = true;
        try
        {
            var list = await WslService.ListAppsAsync(SelectedDistro.Name);
            if (token != _appsToken) return;     // superseded by a newer load
            Apps.Clear();
            foreach (var a in list) Apps.Add(a);
            if (Apps.Count == 0) Status = Loc["Lnx_NoApps"];
        }
        finally { if (token == _appsToken) Loading = false; }
    }

    [RelayCommand] private Task Refresh() => RefreshAsync();

    [RelayCommand]
    private async Task Launch(LinuxApp? app)
    {
        if (app is null) return;
        await WslService.LaunchAsync(app.Distro, app.Exec);
    }

    [RelayCommand]
    private void Shortcut(LinuxApp? app)
    {
        if (app is null) return;
        var ok = ShortcutMaker.ForLinuxApp(_main.Settings, app.Distro, app.Name, app.Exec);
        Status = ok ? Loc["Common_Shortcut"] + " ✓" : "!";
    }

    [RelayCommand]
    private async Task InstallPackage()
    {
        if (SelectedDistro is null || string.IsNullOrWhiteSpace(PackageName)) return;
        Installing = true;
        InstallLog = $"> apt-get install {PackageName}";
        try
        {
            var r = await WslService.InstallPackageAsync(SelectedDistro.Name, PackageName);
            InstallLog += "\n" + r.All.Trim();
            await LoadAppsAsync();
        }
        finally { Installing = false; }
    }
}
