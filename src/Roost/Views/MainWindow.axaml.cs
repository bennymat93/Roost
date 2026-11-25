using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Roost.Views;

public partial class MainWindow : Window
{
    public MainWindow() => AvaloniaXamlLoader.Load(this);
}
