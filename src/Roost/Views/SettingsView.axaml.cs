using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Roost.ViewModels;

namespace Roost.Views;

public partial class SettingsView : UserControl
{
    public SettingsView() => AvaloniaXamlLoader.Load(this);

    private async void OnBrowseAdb(object? sender, RoutedEventArgs e) => await Pick(true);
    private async void OnBrowseAapt2(object? sender, RoutedEventArgs e) => await Pick(false);

    private async System.Threading.Tasks.Task Pick(bool adb)
    {
        var top = TopLevel.GetTopLevel(this);
        if (top is null || DataContext is not SettingsViewModel vm) return;

        var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = adb ? "Locate adb.exe" : "Locate aapt2.exe",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Executable") { Patterns = new[] { "*.exe" } }
            }
        });
        var path = files.FirstOrDefault()?.TryGetLocalPath();
        if (string.IsNullOrEmpty(path)) return;
        if (adb) vm.SetAdbPath(path!); else vm.SetAapt2Path(path!);
    }
}
