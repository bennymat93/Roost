using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Roost.ViewModels;

namespace Roost.Views;

public partial class InstallView : UserControl
{
    public InstallView()
    {
        AvaloniaXamlLoader.Load(this);
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    private async void OnBrowse(object? sender, RoutedEventArgs e)
    {
        var top = TopLevel.GetTopLevel(this);
        if (top is null || DataContext is not InstallViewModel vm) return;

        var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Choose an Android app",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Android app")
                {
                    Patterns = new[] { "*.apk", "*.xapk", "*.apks", "*.apkm" }
                }
            }
        });

        var path = files.FirstOrDefault()?.TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
            await vm.LoadFileAsync(path!);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not InstallViewModel vm) return;
        var file = e.Data.GetFiles()?.FirstOrDefault();
        var path = file?.TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
            await vm.LoadFileAsync(path!);
    }
}
