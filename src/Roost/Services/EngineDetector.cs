using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Roost.Models;

namespace Roost.Services;

/// <summary>Finds Android backends reachable over ADB (WSA and emulators).</summary>
public static class EngineDetector
{
    public static EngineKind GuessKind(int port) => port switch
    {
        58526 => EngineKind.Wsa,
        7555 or 16384 => EngineKind.MuMu,
        62001 or 62025 => EngineKind.Nox,
        _ => EngineKind.Generic,
    };

    public static string NameFor(EngineKind kind) =>
        EngineCatalog.All.FirstOrDefault(k => k.Kind == kind)?.Name
        ?? kind.ToString();

    /// <summary>Attempts to connect to all known/custom endpoints, then returns the ready devices.</summary>
    public static async Task<List<AndroidEngine>> DetectAsync(AdbService adb, AppSettings settings)
    {
        var result = new List<AndroidEngine>();
        if (!adb.Available) return result;

        await adb.StartServerAsync();

        // try every known endpoint + any custom ones the user added
        var targets = EngineCatalog.ProbeTargets()
            .Select(t => (t.Host, t.Port))
            .Concat(settings.CustomEngines.Select(c => (c.Host, c.Port)))
            .Distinct();

        foreach (var (host, port) in targets)
            await adb.ConnectAsync(host, port);

        var serials = await adb.DevicesAsync();
        foreach (var serial in serials)
        {
            var (host, port) = SplitSerial(serial);
            var custom = settings.CustomEngines.FirstOrDefault(c => c.Endpoint == serial);
            var kind = custom?.Kind ?? GuessKind(port);
            result.Add(new AndroidEngine
            {
                Id = serial,
                Kind = kind,
                DisplayName = custom?.DisplayName ?? NameFor(kind),
                Host = host,
                Port = port,
                Serial = serial,
                IsConnected = true,
                Status = "Connected",
            });
        }
        return result;
    }

    private static (string host, int port) SplitSerial(string serial)
    {
        var i = serial.LastIndexOf(':');
        if (i > 0 && int.TryParse(serial[(i + 1)..], out var p))
            return (serial[..i], p);
        return (serial, 0);
    }
}
