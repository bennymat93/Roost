using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Roost.Services;

namespace Roost;

internal static class Program
{
    /// <summary>When started via "--install &lt;path&gt;", the file to preload on the Install page.</summary>
    public static string? PendingInstallFile;

    [STAThread]
    public static int Main(string[] args)
    {
        // App shortcuts launch Roost headless: --launch <engineId> <package>
        var li = Array.IndexOf(args, "--launch");
        if (li >= 0 && args.Length >= li + 3)
            return LaunchHeadless(args[li + 1], args[li + 2]).GetAwaiter().GetResult();

        var ii = Array.IndexOf(args, "--install");
        if (ii >= 0 && args.Length >= ii + 2)
            PendingInstallFile = args[ii + 1];

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        return 0;
    }

    private static async Task<int> LaunchHeadless(string engineId, string package)
    {
        try
        {
            var settings = SettingsStore.Load();
            var adb = new AdbService(ToolLocator.FindAdb(settings));
            if (!adb.Available) return 2;

            var i = engineId.LastIndexOf(':');
            if (i > 0 && int.TryParse(engineId[(i + 1)..], out var port))
            {
                await adb.StartServerAsync();
                await adb.ConnectAsync(engineId[..i], port);
            }
            var r = await adb.LaunchAsync(engineId, package);
            return r.Ok ? 0 : 1;
        }
        catch { return 1; }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
