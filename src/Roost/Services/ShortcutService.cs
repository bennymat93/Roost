using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Roost.Services;

/// <summary>Creates native Windows .lnk shortcuts (Start Menu / Desktop) that launch an
/// installed Android app through Roost, with the app's own icon.</summary>
public static class ShortcutService
{
    public static string StartMenuDir
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Roost");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string DesktopDir =>
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    /// <summary>Create a .lnk at <paramref name="lnkPath"/>.</summary>
    public static bool Create(string lnkPath, string targetExe, string arguments,
                              string? iconPath, string description)
    {
        if (!OperatingSystem.IsWindows()) return false;
        object? com = null;
        try
        {
            com = new ShellLink();
            var link = (IShellLinkW)com;
            link.SetPath(targetExe);
            link.SetArguments(arguments);
            link.SetDescription(description.Length > 255 ? description[..255] : description);
            link.SetWorkingDirectory(Path.GetDirectoryName(targetExe) ?? "");
            if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                link.SetIconLocation(iconPath, 0);
            ((IPersistFile)link).Save(lnkPath, true);
            return true;
        }
        catch { return false; }
        finally { if (com is not null) Marshal.FinalReleaseComObject(com); }
    }

    /// <summary>Wrap a PNG into a single-image .ico (PNG-in-ICO, supported on Windows Vista+)
    /// so a shortcut can use the app's launcher icon. Returns the .ico path, or the PNG path on failure.</summary>
    public static string? PngToIco(string? pngPath, string outDir)
    {
        if (string.IsNullOrEmpty(pngPath) || !File.Exists(pngPath)) return null;
        try
        {
            var png = File.ReadAllBytes(pngPath);
            if (png.Length < 24) return pngPath;
            // PNG IHDR width/height are big-endian at offsets 16 and 20
            int w = (png[16] << 24) | (png[17] << 16) | (png[18] << 8) | png[19];
            int h = (png[20] << 24) | (png[21] << 16) | (png[22] << 8) | png[23];
            byte bw = (byte)(w >= 256 ? 0 : w);
            byte bh = (byte)(h >= 256 ? 0 : h);

            Directory.CreateDirectory(outDir);
            var icoPath = Path.Combine(outDir, Path.GetFileNameWithoutExtension(pngPath) + ".ico");
            using var fs = File.Create(icoPath);
            using var bw2 = new BinaryWriter(fs);
            // ICONDIR
            bw2.Write((ushort)0);     // reserved
            bw2.Write((ushort)1);     // type = icon
            bw2.Write((ushort)1);     // count
            // ICONDIRENTRY
            bw2.Write(bw);            // width
            bw2.Write(bh);            // height
            bw2.Write((byte)0);       // palette
            bw2.Write((byte)0);       // reserved
            bw2.Write((ushort)1);     // planes
            bw2.Write((ushort)32);    // bpp
            bw2.Write((uint)png.Length);
            bw2.Write((uint)22);      // offset = 6 + 16
            bw2.Write(png);
            return icoPath;
        }
        catch { return pngPath; }
    }

    // ---- COM interop for IShellLink ----
    [ComImport, Guid("00021401-0000-0000-C000-000000000046")]
    private class ShellLink { }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszFile,
                     int cchMaxPath, IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszIconPath,
                             int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("0000010b-0000-0000-C000-000000000046")]
    private interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        [PreserveSig] int IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, int dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                  [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder ppszFileName);
    }
}
