using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Roost.Models;
using Roost.Services;

namespace Roost.ViewModels;

public sealed class BackendItem
{
    public string Name { get; init; } = "";
    public string Hint { get; init; } = "";
    public string Ports { get; init; } = "";
}

public partial class EnginesViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public EnginesViewModel(MainViewModel main)
    {
        _main = main;
        foreach (var k in EngineCatalog.All)
            KnownBackends.Add(new BackendItem
            {
                Name = k.Name,
                Hint = k.Hint,
                Ports = string.Join(", ", k.Ports),
            });
    }

    public MainViewModel Main => _main;
    public ObservableCollection<AndroidEngine> Engines => _main.Engines;
    public ObservableCollection<BackendItem> KnownBackends { get; } = new();

    public bool AdbMissing => !_main.Adb.Available;

    [ObservableProperty] private string _customHost = "127.0.0.1";
    [ObservableProperty] private string _customPort = "5555";

    [ObservableProperty] private string _hypervisorText = "—";
    [ObservableProperty] private string _firmwareText = "—";
    [ObservableProperty] private string _vmPlatformText = "—";
    [ObservableProperty] private bool _checking;
    [ObservableProperty] private string _downloadStatus = "";
    [ObservableProperty] private bool _downloading;

    [RelayCommand]
    private async Task DownloadTools()
    {
        Downloading = true;
        try
        {
            var adb = await ToolDownloader.DownloadAdbAsync(s => DownloadStatus = s);
            if (!string.IsNullOrEmpty(adb))
            {
                _main.Settings.AdbPath = adb;
                SettingsStore.Save(_main.Settings);
                _main.ReloadAdb();
                OnPropertyChanged(nameof(AdbMissing));
                await _main.RefreshEnginesCommand.ExecuteAsync(null);
            }
        }
        finally { Downloading = false; }
    }

    [RelayCommand]
    private Task Refresh() => _main.RefreshEnginesCommand.ExecuteAsync(null);

    [RelayCommand]
    private async Task AddCustom()
    {
        if (!int.TryParse(CustomPort, out var port) || port < 1 || port > 65535) return;
        if (!_main.Adb.Available) return;

        await _main.Adb.ConnectAsync(CustomHost, port);
        var id = EngineCatalog.EngineId(CustomHost, port);
        if (!_main.Settings.CustomEngines.Exists(e => e.Endpoint == id))
        {
            _main.Settings.CustomEngines.Add(new AndroidEngine
            {
                Id = id, Kind = EngineKind.Custom, DisplayName = $"Custom {id}",
                Host = CustomHost, Port = port,
            });
            SettingsStore.Save(_main.Settings);
        }
        await _main.RefreshEnginesCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task RunChecks()
    {
        Checking = true;
        HypervisorText = FirmwareText = VmPlatformText = Loc["Common_Working"];
        try
        {
            HypervisorText = Fmt(await SystemChecks.HypervisorPresentAsync());
            FirmwareText = Fmt(await SystemChecks.FirmwareVirtualizationAsync());
            VmPlatformText = Fmt(await SystemChecks.OptionalFeatureEnabledAsync("VirtualMachinePlatform"));
        }
        finally { Checking = false; }
    }

    private string Fmt(bool? v) => v switch
    {
        true => "✓ " + Loc["Common_Connected"],
        false => "✗",
        _ => "?",
    };
}
