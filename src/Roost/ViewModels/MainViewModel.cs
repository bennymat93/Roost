using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Roost.Models;
using Roost.Services;

namespace Roost.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public AppSettings Settings { get; }
    public AdbService Adb { get; private set; }

    public ObservableCollection<AndroidEngine> Engines { get; } = new();

    [ObservableProperty] private AndroidEngine? _selectedEngine;
    [ObservableProperty] private object? _current;
    [ObservableProperty] private int _navIndex;
    [ObservableProperty] private string _statusText = "";
    [ObservableProperty] private bool _busy;

    public DashboardViewModel Dashboard { get; }
    public InstallViewModel Install { get; }
    public AppsViewModel Apps { get; }
    public EnginesViewModel EnginesVm { get; }
    public SettingsViewModel SettingsVm { get; }
    public AboutViewModel About { get; }

    public MainViewModel(AppSettings settings)
    {
        Settings = settings;
        Adb = new AdbService(ToolLocator.FindAdb(settings));

        Dashboard = new DashboardViewModel(this);
        Install = new InstallViewModel(this);
        Apps = new AppsViewModel(this);
        EnginesVm = new EnginesViewModel(this);
        SettingsVm = new SettingsViewModel(this);
        About = new AboutViewModel(this);

        Current = Dashboard;
    }

    public void ReloadAdb()
    {
        Adb = new AdbService(ToolLocator.FindAdb(Settings));
        OnPropertyChanged(nameof(Adb));
    }

    public bool AdbAvailable => Adb.Available;

    partial void OnNavIndexChanged(int value)
    {
        Current = value switch
        {
            0 => Dashboard,
            1 => Install,
            2 => Apps,
            3 => EnginesVm,
            4 => SettingsVm,
            5 => About,
            _ => Dashboard,
        };
        if (value == 2) _ = Apps.LoadAsync();
    }

    public void Navigate(int index) => NavIndex = index;

    [RelayCommand]
    private async Task RefreshEngines()
    {
        if (!Adb.Available) { StatusText = Loc["Eng_AdbMissing"]; return; }
        Busy = true;
        StatusText = Loc["Common_Working"];
        try
        {
            var found = await EngineDetector.DetectAsync(Adb, Settings);
            Engines.Clear();
            foreach (var e in found) Engines.Add(e);
            SelectedEngine = Engines.FirstOrDefault(e => e.Id == Settings.DefaultEngineId)
                             ?? Engines.FirstOrDefault();
            StatusText = Engines.Count > 0
                ? $"{Engines.Count} {Loc["Eng_Detected"]}"
                : Loc["Dash_NoEngines"];
        }
        finally { Busy = false; }
    }

    partial void OnSelectedEngineChanged(AndroidEngine? value)
    {
        if (value is not null) Settings.DefaultEngineId = value.Id;
        _ = Apps.LoadAsync();
    }
}
