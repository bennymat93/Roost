using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Roost.Models;
using Roost.Services;
using Roost.ViewModels;
using Roost.Views;

namespace Roost;

public partial class App : Application
{
    private static MainViewModel? _main;
    private static IClassicDesktopStyleApplicationLifetime? _desktop;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var settings = SettingsStore.Load();
        SettingsStore.EnsureDirs();
        Localizer.Instance.SetLanguage(settings.Language);
        ApplyTheme(settings.Theme);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
            _main = new MainViewModel(settings);
            desktop.MainWindow = BuildWindow(_main);

            // auto-detect engines on launch
            _ = _main.RefreshEnginesCommand.ExecuteAsync(null);

            // preload an APK passed on the command line
            if (!string.IsNullOrEmpty(Program.PendingInstallFile))
            {
                _main.Navigate(1);
                _ = _main.Install.LoadFileAsync(Program.PendingInstallFile!);
            }
        }
        base.OnFrameworkInitializationCompleted();
    }

    private static MainWindow BuildWindow(MainViewModel vm)
    {
        var w = new MainWindow { DataContext = vm };
        w.FlowDirection = Localizer.Instance.Flow;
        return w;
    }

    public static void ApplyTheme(ThemeMode mode)
    {
        if (Current is null) return;
        Current.RequestedThemeVariant = mode switch
        {
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }

    /// <summary>Re-apply theme + language and rebuild the window (used after Settings save).</summary>
    public static void ApplyAndRestart(AppSettings settings)
    {
        Localizer.Instance.SetLanguage(settings.Language);
        ApplyTheme(settings.Theme);
        if (_desktop is not null && _main is not null)
        {
            var old = _desktop.MainWindow;
            _desktop.MainWindow = BuildWindow(_main);
            _desktop.MainWindow.Show();
            old?.Close();
        }
    }
}
