using Avalonia;
using Avalonia.Headless;
using Avalonia.Threading;
using Roost;
using Roost.Models;
using Roost.Services;
using Roost.ViewModels;
using Roost.Views;

var outDir = args.Length > 0 ? args[0] : "/tmp/roost-shots";
System.IO.Directory.CreateDirectory(outDir);

AppBuilder.Configure<App>()
    .UseSkia()
    .WithInterFont()
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
    .SetupWithoutStarting();

var lang = args.Length > 1 ? args[1] : "en";
Localizer.Instance.SetLanguage(lang == "he" ? AppLanguage.Hebrew : AppLanguage.English);

var settings = new AppSettings();
var vm = new MainViewModel(settings);

// Seed a couple of fake engines/apps so pages have content to render.
vm.Engines.Add(new AndroidEngine { Id = "127.0.0.1:58526", Kind = EngineKind.Wsa, DisplayName = "Windows Subsystem for Android", Host = "127.0.0.1", Port = 58526, Serial = "127.0.0.1:58526", IsConnected = true, Status = "Connected" });
vm.Engines.Add(new AndroidEngine { Id = "127.0.0.1:5555", Kind = EngineKind.BlueStacks, DisplayName = "BlueStacks", Host = "127.0.0.1", Port = 5555, Serial = "127.0.0.1:5555", IsConnected = true, Status = "Connected" });
vm.SelectedEngine = vm.Engines[0];
vm.StatusText = "2 Connected now";

vm.Apps.Apps.Add(new InstalledApp { PackageName = "com.whatsapp", Label = "WhatsApp", EngineId = "127.0.0.1:58526" });
vm.Apps.Apps.Add(new InstalledApp { PackageName = "com.spotify.music", Label = "Spotify", EngineId = "127.0.0.1:58526" });
vm.Apps.Apps.Add(new InstalledApp { PackageName = "com.instagram.android", Label = "Instagram", EngineId = "127.0.0.1:58526" });

var window = new MainWindow { DataContext = vm };
window.FlowDirection = Localizer.Instance.Flow;
window.Show();

string[] pages = { "dashboard", "install", "apps", "engines", "linux", "settings", "about", "help" };
for (int i = 0; i < pages.Length; i++)
{
    vm.Navigate(i);
    Dispatcher.UIThread.RunJobs();

    if (pages[i] == "linux")
    {
        // seed after navigation (navigation would otherwise run a real WSL probe)
        vm.Linux.WslInstalled = true;
        vm.Linux.Distros.Clear();
        vm.Linux.Distros.Add(new WslDistro { Name = "Ubuntu-22.04", State = "Running", Version = 2, IsDefault = true });
        vm.Linux.Distros.Add(new WslDistro { Name = "Debian", State = "Stopped", Version = 2 });
        vm.Linux.Apps.Clear();
        vm.Linux.Apps.Add(new LinuxApp { Name = "GIMP", Exec = "gimp", Distro = "Ubuntu-22.04" });
        vm.Linux.Apps.Add(new LinuxApp { Name = "LibreOffice Writer", Exec = "libreoffice --writer", Distro = "Ubuntu-22.04" });
        vm.Linux.Apps.Add(new LinuxApp { Name = "Inkscape", Exec = "inkscape", Distro = "Ubuntu-22.04" });
    }
    // give layout a couple of passes
    for (int k = 0; k < 3; k++) Dispatcher.UIThread.RunJobs();
    var frame = window.CaptureRenderedFrame();
    var path = System.IO.Path.Combine(outDir, $"{i}_{pages[i]}_{lang}.png");
    frame?.Save(path);
    System.Console.WriteLine("wrote " + path);
}
