using System.Collections.Generic;
using Avalonia.Media;
using Roost.Models;

namespace Roost.Services;

/// <summary>Minimal English/Hebrew string table. The app rebuilds its main window on
/// language change, so bindings simply read the current value.</summary>
public sealed class Localizer
{
    public static Localizer Instance { get; } = new();

    public AppLanguage Language { get; private set; } = AppLanguage.English;
    public bool IsRtl => Language == AppLanguage.Hebrew;
    public FlowDirection Flow => IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    public void SetLanguage(AppLanguage lang)
    {
        if (lang == AppLanguage.Auto)
        {
            var ui = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            lang = ui == "he" ? AppLanguage.Hebrew : AppLanguage.English;
        }
        Language = lang;
    }

    public string this[string key] =>
        Table.TryGetValue(key, out var pair)
            ? (Language == AppLanguage.Hebrew ? pair.he : pair.en)
            : key;

    private static readonly Dictionary<string, (string en, string he)> Table = new()
    {
        // App / nav
        ["App_Name"] = ("Roost", "Roost"),
        ["App_Tagline"] = ("Android & Linux apps, at home on Windows", "אפליקציות אנדרואיד ו-Linux, בבית ב-Windows"),
        ["Nav_Dashboard"] = ("Dashboard", "סקירה"),
        ["Nav_Install"] = ("Install app", "התקנת אפליקציה"),
        ["Nav_Apps"] = ("My apps", "האפליקציות שלי"),
        ["Nav_Engines"] = ("Engines", "מנועים"),
        ["Nav_Linux"] = ("Linux apps", "אפליקציות Linux"),
        ["Nav_Settings"] = ("Settings", "הגדרות"),
        ["Nav_About"] = ("About", "אודות"),
        ["Nav_Help"] = ("Help / Guide", "עזרה / מדריך"),

        // Common
        ["Common_Refresh"] = ("Refresh", "רענון"),
        ["Common_Install"] = ("Install", "התקנה"),
        ["Common_Uninstall"] = ("Uninstall", "הסרה"),
        ["Common_Launch"] = ("Launch", "הפעלה"),
        ["Common_Shortcut"] = ("Create shortcut", "יצירת קיצור דרך"),
        ["Common_Browse"] = ("Browse…", "עיון…"),
        ["Common_Connect"] = ("Connect", "התחברות"),
        ["Common_Disconnect"] = ("Disconnect", "ניתוק"),
        ["Common_Cancel"] = ("Cancel", "ביטול"),
        ["Common_Save"] = ("Save", "שמירה"),
        ["Common_Package"] = ("Package", "חבילה"),
        ["Common_Version"] = ("Version", "גרסה"),
        ["Common_Status"] = ("Status", "מצב"),
        ["Common_Connected"] = ("Connected", "מחובר"),
        ["Common_Working"] = ("Working…", "עובד…"),
        ["Common_None"] = ("None", "אין"),

        // Dashboard
        ["Dash_Welcome"] = ("Welcome to Roost", "ברוכים הבאים ל-Roost"),
        ["Dash_Intro"] = ("Install Android apps and run them on Windows 11. Roost talks to a Windows Subsystem for Android build or any emulator over ADB.",
                          "התקנת אפליקציות אנדרואיד והרצתן ב-Windows 11. Roost מתחבר ל-WSA או לכל אמולטור דרך ADB."),
        ["Dash_Engines"] = ("Detected engines", "מנועים שזוהו"),
        ["Dash_NoEngines"] = ("No Android engine detected yet. Open Engines to set one up.",
                             "לא זוהה מנוע אנדרואיד. פתח 'מנועים' כדי להגדיר אחד."),
        ["Dash_QuickInstall"] = ("Quick install", "התקנה מהירה"),
        ["Dash_Drop"] = ("Drop an APK / XAPK here, or click to choose",
                        "גרור לכאן קובץ APK / XAPK, או לחץ לבחירה"),

        // Install
        ["Install_Title"] = ("Install an Android app", "התקנת אפליקציית אנדרואיד"),
        ["Install_Pick"] = ("Choose an .apk, .xapk, .apks or .apkm file.",
                           "בחר קובץ ‎.apk‎, ‎.xapk‎, ‎.apks‎ או ‎.apkm‎."),
        ["Install_Parsing"] = ("Reading package…", "קורא את החבילה…"),
        ["Install_Target"] = ("Install to", "התקנה אל"),
        ["Install_Reinstall"] = ("Keep app data if already installed", "שמור נתונים אם כבר מותקן"),
        ["Install_MakeShortcut"] = ("Create a shortcut after install", "צור קיצור דרך לאחר ההתקנה"),
        ["Install_Go"] = ("Install now", "התקן עכשיו"),
        ["Install_NoEngine"] = ("Connect an engine first (see Engines).", "התחבר למנוע תחילה (ראה 'מנועים')."),
        ["Install_Success"] = ("Installed successfully.", "הותקן בהצלחה."),
        ["Install_Failed"] = ("Installation failed. See the log below.", "ההתקנה נכשלה. ראה יומן למטה."),
        ["Install_Permissions"] = ("Permissions", "הרשאות"),
        ["Install_Split"] = ("Split bundle", "חבילה מפוצלת"),
        ["Install_NoMeta"] = ("Metadata needs aapt2 (optional). The file can still be installed.",
                             "קריאת פרטים דורשת aapt2 (רשות). עדיין ניתן להתקין."),

        // Apps
        ["Apps_Title"] = ("Installed apps", "אפליקציות מותקנות"),
        ["Apps_On"] = ("on", "על"),
        ["Apps_None"] = ("No third-party apps found on this engine.", "לא נמצאו אפליקציות על המנוע הזה."),
        ["Apps_ConfirmUninstall"] = ("Uninstall this app?", "להסיר את האפליקציה?"),

        // Engines
        ["Eng_Title"] = ("Android engines", "מנועי אנדרואיד"),
        ["Eng_Detected"] = ("Connected now", "מחוברים כעת"),
        ["Eng_Known"] = ("Supported backends", "מנועים נתמכים"),
        ["Eng_Custom"] = ("Add a custom ADB endpoint", "הוספת כתובת ADB מותאמת"),
        ["Eng_Host"] = ("Host", "שרת"),
        ["Eng_Port"] = ("Port", "פורט"),
        ["Eng_Add"] = ("Add & connect", "הוסף והתחבר"),
        ["Eng_Setup"] = ("Readiness check", "בדיקת מוכנות"),
        ["Eng_Hypervisor"] = ("Hypervisor running", "מנהל וירטואליזציה פעיל"),
        ["Eng_Firmware"] = ("CPU virtualization (firmware)", "וירטואליזציה במעבד (BIOS)"),
        ["Eng_VmPlatform"] = ("Virtual Machine Platform feature", "רכיב Virtual Machine Platform"),
        ["Eng_CheckNow"] = ("Run checks", "הרץ בדיקות"),
        ["Eng_Corp"] = ("On a managed work PC these are often disabled by IT policy and can't be turned on without admin rights.",
                       "במחשב עבודה מנוהל, רכיבים אלה חסומים לרוב ע\"י מדיניות IT ולא ניתן להפעילם ללא הרשאות מנהל."),
        ["Eng_AdbMissing"] = ("adb was not found. Set its path in Settings or place platform-tools next to Roost.",
                             "‏adb לא נמצא. הגדר נתיב ב'הגדרות' או הנח platform-tools ליד Roost."),
        ["Eng_DownloadAdb"] = ("Download adb automatically", "הורד adb אוטומטית"),

        // Settings
        ["Set_Title"] = ("Settings", "הגדרות"),
        ["Set_Theme"] = ("Theme", "ערכת נושא"),
        ["Set_Theme_System"] = ("System", "מערכת"),
        ["Set_Theme_Light"] = ("Light", "בהיר"),
        ["Set_Theme_Dark"] = ("Dark", "כהה"),
        ["Set_Language"] = ("Language", "שפה"),
        ["Set_Lang_Auto"] = ("Automatic", "אוטומטי"),
        ["Set_Lang_En"] = ("English", "English"),
        ["Set_Lang_He"] = ("עברית", "עברית"),
        ["Set_Adb"] = ("adb.exe path", "נתיב adb.exe"),
        ["Set_Aapt2"] = ("aapt2.exe path (optional)", "נתיב aapt2.exe (רשות)"),
        ["Set_Shortcut"] = ("Create shortcuts in", "יצירת קיצורי דרך ב"),
        ["Set_Sc_StartMenu"] = ("Start Menu", "תפריט התחל"),
        ["Set_Sc_Desktop"] = ("Desktop", "שולחן עבודה"),
        ["Set_Sc_Both"] = ("Both", "שניהם"),
        ["Set_AutoSuspend"] = ("Suspend the engine when idle (saves RAM)", "השהה את המנוע במנוחה (חוסך זיכרון)"),
        ["Set_StartWin"] = ("Start Roost with Windows", "הפעל את Roost עם Windows"),
        ["Set_Tools"] = ("Android tools", "כלי אנדרואיד"),
        ["Set_ToolsFound"] = ("Found", "נמצא"),
        ["Set_ToolsMissing"] = ("Not found", "לא נמצא"),

        // Linux (WSL)
        ["Lnx_Title"] = ("Linux apps (WSL)", "אפליקציות Linux (WSL)"),
        ["Lnx_Intro"] = ("Run Linux GUI software on Windows 11 through WSL 2 + WSLg. Install packages, then launch them or pin them to the Start Menu.",
                        "הרצת תוכנות Linux עם ממשק גרפי ב-Windows 11 דרך WSL 2 + WSLg. התקן חבילות, ואז הפעל אותן או הצמד לתפריט התחל."),
        ["Lnx_NoWsl"] = ("WSL is not installed or not reachable.", "‏WSL אינו מותקן או לא נגיש."),
        ["Lnx_WslHint"] = ("Open PowerShell as administrator and run:  wsl --install  then restart. On Windows 11 this also enables WSLg for GUI apps.",
                          "פתח PowerShell כמנהל והרץ:  wsl --install  ואז הפעל מחדש. ב-Windows 11 זה גם מפעיל את WSLg לאפליקציות גרפיות."),
        ["Lnx_Distro"] = ("Distribution", "הפצה"),
        ["Lnx_Distros"] = ("distributions", "הפצות"),
        ["Lnx_NoDistro"] = ("No WSL distribution found. Install one, e.g.  wsl --install -d Ubuntu",
                           "לא נמצאה הפצת WSL. התקן אחת, למשל  wsl --install -d Ubuntu"),
        ["Lnx_Apps"] = ("Installed GUI apps", "אפליקציות גרפיות מותקנות"),
        ["Lnx_NoApps"] = ("No GUI apps found in this distribution yet.", "לא נמצאו אפליקציות גרפיות בהפצה הזו."),
        ["Lnx_InstallPkg"] = ("Install a package (apt)", "התקנת חבילה (apt)"),
        ["Lnx_PkgWatermark"] = ("package name, e.g. gimp", "שם חבילה, למשל gimp"),

        // About
        ["About_Version"] = ("Version", "גרסה"),
        ["About_Desc"] = ("Roost installs Android apps and runs them on Windows 11 by driving a Windows Subsystem for Android build or an emulator over ADB — then gives each app a native Start-Menu shortcut.",
                         "‏Roost מתקין אפליקציות אנדרואיד ומריץ אותן ב-Windows 11 באמצעות WSA או אמולטור דרך ADB — ונותן לכל אפליקציה קיצור דרך מקורי בתפריט התחל."),
        ["About_Corp"] = ("Note: the Android engine needs hardware virtualization. On a locked corporate laptop this may be blocked by IT.",
                         "הערה: המנוע דורש וירטואליזציה חומרתית. במחשב עבודה נעול ייתכן שה-IT חוסם זאת."),
        ["About_License"] = ("MIT License · open source", "רישיון MIT · קוד פתוח"),
    };
}
