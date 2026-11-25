using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Roost.Models;

namespace Roost.Services;

public sealed class HelpStep
{
    public int Number { get; init; }
    public string Text { get; init; } = "";
}

public sealed class HelpSection
{
    public string Title { get; init; } = "";
    public string Intro { get; init; } = "";
    public Bitmap? Image { get; init; }
    public List<HelpStep> Steps { get; init; } = new();
    public bool HasImage => Image is not null;
    public bool HasSteps => Steps.Count > 0;
}

/// <summary>Builds the in-app user guide (English / Hebrew) with annotated screenshots.</summary>
public static class HelpContent
{
    public static List<HelpSection> Build(AppLanguage lang)
    {
        bool he = lang == AppLanguage.Hebrew;
        string code = he ? "he" : "en";
        string S(string en, string heb) => he ? heb : en;

        Bitmap? Img(string baseName)
        {
            try
            {
                var uri = new Uri($"avares://Roost/Assets/help/{baseName}_{code}.png");
                return new Bitmap(AssetLoader.Open(uri));
            }
            catch { return null; }
        }

        HelpStep St(int n, string en, string heb) => new() { Number = n, Text = S(en, heb) };

        return new List<HelpSection>
        {
            new()
            {
                Title = S("Getting started", "תחילת העבודה"),
                Intro = S("Roost installs Android apps and Linux software and runs them on Windows 11. It does not emulate them itself — it drives an Android engine (a WSA build or an emulator) or WSL over the background, and gives each app a native shortcut. Use the sidebar to move between pages.",
                          "‏Roost מתקין אפליקציות אנדרואיד ותוכנות Linux ומריץ אותן ב-Windows 11. הוא אינו מדמה אותן בעצמו — הוא מתזמר מנוע אנדרואיד (WSA או אמולטור) או WSL ברקע, ונותן לכל אפליקציה קיצור דרך מקורי. השתמש בסרגל הצד למעבר בין עמודים."),
                Image = Img("dashboard"),
                Steps =
                {
                    St(1, "The sidebar — switch between Dashboard, Install, My apps, Engines, Linux, Settings, About and Help.",
                          "סרגל הצד — מעבר בין סקירה, התקנה, האפליקציות שלי, מנועים, Linux, הגדרות, אודות ועזרה."),
                    St(2, "Quick install — jump straight to installing an app file.",
                          "התקנה מהירה — מעבר ישיר להתקנת קובץ אפליקציה."),
                    St(3, "Refresh — re-scan for connected Android engines.",
                          "רענון — סריקה מחדש של מנועי אנדרואיד מחוברים."),
                    St(4, "Detected engines — every Android backend currently reachable.",
                          "מנועים שזוהו — כל מנוע אנדרואיד שזמין כרגע."),
                    St(5, "Status bar — shows what Roost is doing right now.",
                          "שורת מצב — מציגה מה Roost עושה כרגע."),
                },
            },
            new()
            {
                Title = S("1 · Set up an Android engine", "‏1 · הגדרת מנוע אנדרואיד"),
                Intro = S("Roost needs an Android engine to install into. It works with a community WSA build or any emulator (BlueStacks, LDPlayer, MuMu, Nox). An Android engine requires hardware virtualization — on a locked work PC this may be blocked by IT.",
                          "‏Roost צריך מנוע אנדרואיד להתקין אליו. הוא עובד עם WSA קהילתי או כל אמולטור (BlueStacks, LDPlayer, MuMu, Nox). מנוע אנדרואיד דורש וירטואליזציה חומרתית — במחשב עבודה נעול ייתכן שה-IT חוסם זאת."),
                Image = Img("engines"),
                Steps =
                {
                    St(1, "Download adb automatically — if adb is missing, Roost fetches Google's platform-tools for you.",
                          "הורד adb אוטומטית — אם adb חסר, Roost מוריד עבורך את platform-tools של גוגל."),
                    St(2, "Refresh — probe the known ADB ports and list what is connected.",
                          "רענון — בדיקת פורטי ADB ידועים והצגת מה שמחובר."),
                    St(3, "Connected now — the engines Roost found; pick one as the target.",
                          "מחוברים כעת — המנועים ש-Roost מצא; בחר אחד כיעד."),
                    St(4, "Add a custom ADB endpoint — type a host and port for any other emulator/device.",
                          "הוספת כתובת ADB מותאמת — הקלד שרת ופורט לכל אמולטור/מכשיר אחר."),
                    St(5, "Run checks — see whether virtualization and the Virtual Machine Platform are ready.",
                          "הרץ בדיקות — בדוק אם הווירטואליזציה ורכיב Virtual Machine Platform מוכנים."),
                },
            },
            new()
            {
                Title = S("2 · Install an app", "‏2 · התקנת אפליקציה"),
                Intro = S("Install an .apk, or a split bundle (.xapk / .apks / .apkm). After the file loads you will see its name, version and permissions, choose a target engine, and install. Tick 'Create a shortcut' to get a Start-Menu icon.",
                          "התקן ‎.apk‎, או חבילה מפוצלת (‎.xapk / .apks / .apkm‎). לאחר טעינת הקובץ יוצגו שמו, גרסתו והרשאותיו, תבחר מנוע יעד, ותתקין. סמן 'צור קיצור דרך' כדי לקבל אייקון בתפריט התחל."),
                Image = Img("install"),
                Steps =
                {
                    St(1, "Drop zone — drag an app file anywhere here.",
                          "אזור גרירה — גרור לכאן קובץ אפליקציה."),
                    St(2, "Browse — or pick the file from a dialog. Then choose the engine and press Install now.",
                          "עיון — או בחר את הקובץ מחלון. לאחר מכן בחר מנוע ולחץ 'התקן עכשיו'."),
                },
            },
            new()
            {
                Title = S("3 · Manage installed apps", "‏3 · ניהול אפליקציות מותקנות"),
                Intro = S("The My apps page lists the third-party apps on the selected engine. For each one you can launch it, create a native shortcut, or uninstall it.",
                          "עמוד 'האפליקציות שלי' מציג את האפליקציות על המנוע הנבחר. לכל אחת אפשר להפעיל, ליצור קיצור דרך מקורי, או להסיר."),
                Image = Img("apps"),
                Steps =
                {
                    St(1, "Engine selector — choose which engine's apps to show.",
                          "בחירת מנוע — בחר את האפליקציות של איזה מנוע להציג."),
                    St(2, "Refresh — reload the list of installed apps.",
                          "רענון — טען מחדש את רשימת האפליקציות."),
                },
            },
            new()
            {
                Title = S("4 · Linux apps (WSL)", "‏4 · אפליקציות Linux"),
                Intro = S("Run Linux GUI software on Windows 11 through WSL 2 + WSLg. Pick a distribution, install packages with apt, then launch the GUI apps or pin them to the Start Menu. If WSL is missing, Roost tells you how to install it (wsl --install).",
                          "הרצת תוכנות Linux גרפיות ב-Windows 11 דרך WSL 2 + WSLg. בחר הפצה, התקן חבילות עם apt, ואז הפעל את האפליקציות או הצמד לתפריט התחל. אם WSL חסר, Roost מסביר כיצד להתקין (‎wsl --install‎)."),
                Image = Img("linux"),
                Steps =
                {
                    St(1, "Distribution — choose which WSL distro to work with.",
                          "הפצה — בחר באיזו הפצת WSL לעבוד."),
                    St(2, "Package name — type a package to install, e.g. gimp.",
                          "שם חבילה — הקלד חבילה להתקנה, למשל gimp."),
                    St(3, "Install — runs apt inside the distribution.",
                          "התקן — מריץ apt בתוך ההפצה."),
                    St(4, "Launch — opens the Linux app; WSLg draws its window on Windows.",
                          "הפעל — פותח את אפליקציית ה-Linux; WSLg מצייר את החלון ב-Windows."),
                    St(5, "Create shortcut — pin the Linux app to your Start Menu.",
                          "צור קיצור דרך — הצמד את אפליקציית ה-Linux לתפריט התחל."),
                },
            },
            new()
            {
                Title = S("5 · Settings", "‏5 · הגדרות"),
                Intro = S("Tune Roost to your taste. Changes to theme and language apply immediately.",
                          "התאם את Roost. שינויי ערכת נושא ושפה חלים מיד."),
                Image = Img("settings"),
                Steps =
                {
                    St(1, "Theme — System, Light or Dark.", "ערכת נושא — מערכת, בהיר או כהה."),
                    St(2, "Language — English or Hebrew (full right-to-left).", "שפה — אנגלית או עברית (ימין-לשמאל מלא)."),
                    St(3, "Create shortcuts in — Start Menu, Desktop, or both.", "יצירת קיצורים ב — תפריט התחל, שולחן עבודה, או שניהם."),
                    St(4, "Start with Windows — launch Roost when you sign in.", "הפעלה עם Windows — הפעל את Roost בכניסה."),
                    St(5, "adb / aapt2 paths — point Roost at the tools, or use the auto-download.", "נתיבי adb / aapt2 — הכוון את Roost לכלים, או השתמש בהורדה האוטומטית."),
                    St(6, "Save — store your settings.", "שמירה — שמור את ההגדרות."),
                },
            },
            new()
            {
                Title = S("Troubleshooting", "פתרון תקלות"),
                Intro = S("Common issues and what to do about them.", "תקלות נפוצות ומה לעשות."),
                Image = null,
                Steps =
                {
                    St(1, "\"adb not found\" — go to Engines and click Download adb automatically, or set the path in Settings.",
                          "\"adb לא נמצא\" — עבור ל'מנועים' ולחץ 'הורד adb אוטומטית', או הגדר נתיב ב'הגדרות'."),
                    St(2, "No engine detected — make sure WSA or your emulator is running, then press Refresh.",
                          "לא זוהה מנוע — ודא ש-WSA או האמולטור פועלים, ואז לחץ 'רענון'."),
                    St(3, "Readiness check shows ✗ — virtualization or the Virtual Machine Platform is off. On a personal PC enable it; on a managed work PC IT may block it.",
                          "בדיקת המוכנות מציגה ✗ — הווירטואליזציה או רכיב VM Platform כבויים. במחשב אישי הפעל; במחשב עבודה ייתכן שה-IT חוסם."),
                    St(4, "Windows SmartScreen warning — the build is not code-signed. Choose More info → Run anyway.",
                          "אזהרת SmartScreen — הקובץ אינו חתום דיגיטלית. בחר More info ← Run anyway."),
                    St(5, "Linux apps won't launch — confirm WSL is installed (wsl --install) and the distro is running; WSLg needs Windows 11.",
                          "אפליקציות Linux לא נפתחות — ודא ש-WSL מותקן (‎wsl --install‎) וההפצה פועלת; WSLg דורש Windows 11."),
                },
            },
        };
    }
}
