using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;

namespace Roost.Services;

/// <summary>Small modal confirm dialog (Avalonia has no built-in message box).</summary>
public static class Dialogs
{
    public static async Task<bool> ConfirmAsync(string title, string message, string yes, string no)
    {
        var owner = (Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        var tcs = new TaskCompletionSource<bool>();

        var text = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };
        var yesBtn = new Button { Content = yes, MinWidth = 90 };
        var noBtn = new Button { Content = no, MinWidth = 90, IsDefault = true };

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
        };
        buttons.Children.Add(noBtn);
        buttons.Children.Add(yesBtn);

        var root = new StackPanel { Margin = new Thickness(20), Spacing = 18 };
        root.Children.Add(text);
        root.Children.Add(buttons);

        var win = new Window
        {
            Title = title,
            Width = 400,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
            FlowDirection = Localizer.Instance.Flow,
            Content = root,
        };

        yesBtn.Click += (_, _) => { tcs.TrySetResult(true); win.Close(); };
        noBtn.Click += (_, _) => { tcs.TrySetResult(false); win.Close(); };
        win.Closed += (_, _) => tcs.TrySetResult(false);

        if (owner is not null) await win.ShowDialog(owner);
        else win.Show();

        return await tcs.Task;
    }
}
