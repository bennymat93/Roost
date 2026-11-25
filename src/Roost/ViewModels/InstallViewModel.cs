using System;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Roost.Models;
using Roost.Services;

namespace Roost.ViewModels;

public partial class InstallViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    public InstallViewModel(MainViewModel main) => _main = main;

    public MainViewModel Main => _main;

    [ObservableProperty] private ApkInfo? _info;
    [ObservableProperty] private bool _reinstall = true;
    [ObservableProperty] private bool _makeShortcut = true;
    [ObservableProperty] private bool _installing;
    [ObservableProperty] private string _log = "";
    [ObservableProperty] private string _resultMessage = "";
    [ObservableProperty] private bool _parsing;

    public bool HasInfo => Info is not null;
    public bool CanInstall => Info is not null && !Installing && _main.SelectedEngine is not null;

    partial void OnInfoChanged(ApkInfo? value)
    {
        OnPropertyChanged(nameof(HasInfo));
        OnPropertyChanged(nameof(CanInstall));
    }

    partial void OnInstallingChanged(bool value) => OnPropertyChanged(nameof(CanInstall));

    /// <summary>Called when the selected engine changes elsewhere, so the Install button re-evaluates.</summary>
    public void NotifyCanInstall() => OnPropertyChanged(nameof(CanInstall));

    public async Task LoadFileAsync(string path)
    {
        ResultMessage = "";
        Log = "";
        Parsing = true;
        try
        {
            var aapt2 = ToolLocator.FindAapt2(_main.Settings);
            Info = await ApkInspector.InspectAsync(path, aapt2);
            if (Info.ParseError is not null)
                AppendLog("! " + Info.ParseError);
            if (string.IsNullOrEmpty(aapt2))
                AppendLog("i " + Loc["Install_NoMeta"]);
        }
        finally { Parsing = false; }
    }

    private void AppendLog(string line)
    {
        Log = string.IsNullOrEmpty(Log) ? line : Log + "\n" + line;
    }

    [RelayCommand]
    private async Task Install()
    {
        var engine = _main.SelectedEngine;
        var info = Info;
        if (info is null) return;
        if (engine?.Serial is null) { ResultMessage = Loc["Install_NoEngine"]; return; }
        if (info.SplitFiles.Count == 0)
        {
            AppendLog("! no installable .apk found in the selected file");
            ResultMessage = Loc["Install_Failed"];
            return;
        }

        Installing = true;
        ResultMessage = "";
        AppendLog($"> adb install → {engine.DisplayName} ({engine.Serial})");
        try
        {
            ProcResult r;
            if (info.IsSplit && info.SplitFiles.Count > 1)
                r = await _main.Adb.InstallMultipleAsync(engine.Serial, info.SplitFiles, Reinstall);
            else
                r = await _main.Adb.InstallAsync(engine.Serial, info.SplitFiles[0], Reinstall);

            if (!string.IsNullOrWhiteSpace(r.All)) AppendLog(r.All.Trim());

            // adb prints "Success" on success and "Failure [...]" otherwise, on stdout
            // or stderr depending on the build — so check combined output.
            bool success = r.All.Contains("Success", StringComparison.OrdinalIgnoreCase)
                           && !r.All.Contains("Failure", StringComparison.OrdinalIgnoreCase);
            if (success)
            {
                ResultMessage = Loc["Install_Success"];
                if (MakeShortcut && !string.IsNullOrEmpty(info.PackageName))
                {
                    var made = ShortcutMaker.ForApp(_main.Settings, engine,
                        info.PackageName, info.Label, info.IconPath);
                    AppendLog(made ? "✓ " + Loc["Common_Shortcut"] : "! shortcut skipped");
                }
                _ = _main.Apps.LoadAsync();
            }
            else
            {
                ResultMessage = Loc["Install_Failed"];
            }
        }
        catch (Exception ex)
        {
            AppendLog("! " + ex.Message);
            ResultMessage = Loc["Install_Failed"];
        }
        finally { Installing = false; }
    }
}
