using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;

// ╔══════════════════════════════════════════════════════════════════╗
// ║                     KONFIGURACJA — EDYTUJ TU                    ║
// ╚══════════════════════════════════════════════════════════════════╝
static class CFG
{
    public const int UPDATE_MS  = 4000;
    public const int TOPMOST_MS = 33;
	public const int REPOSITION_MS = 500;
    public const int OFFSET_FROM_TRAY = -15;

    public const int   PILL_W    = 59;
    public const int   PILL_H    = 24;
    public const int   NUB_W     = 3;
    public const int   NUB_H     = 10;
    public const float OUTLINE_W = 3f;

    public const string FONT_FAMILY = "Segoe UI";
    public const float  FONT_SIZE   = 13f;
    public const bool   FONT_BOLD   = false;

    // ── Język domyślny ("pl" lub "en") ───────────────────────────────
    public const string LANG_DEFAULT = "pl";

    public static readonly Color BG           = Color.FromArgb(1, 1, 1);
    public static readonly Color PILL_BG      = Hex("#111111");
    public static readonly Color OUTLINE      = Hex("#ffffff");
    public static readonly Color COLOR_HIGH   = Hex("#0b993f");
    public static readonly Color COLOR_MED    = Hex("#eba834");
    public static readonly Color COLOR_LOW    = Hex("#eba834");
    public static readonly Color COLOR_CRIT   = Hex("#ef4444");
    public static readonly Color COLOR_CHARGE = Hex("#38bdf8");

    public const int THRESH_HIGH = 31;
    public const int THRESH_MED  = 30;
    public const int THRESH_LOW  = 15;

    public static readonly Color TEXT_COLOR       = Color.White;
    public static readonly Color TEXT_COLOR_EMPTY = Color.White;
    public const int TEXT_MIN_FILL_PX = 20;

    public const int PLUG_OFFSET_X = 10;
    public static readonly Color PLUG_COLOR = Color.White;
    public const int PLUG_PRONG_W   = 2;
    public const int PLUG_PRONG_H   = 5;
    public const int PLUG_PRONG_GAP = 3;
    public const int PLUG_BODY_W    = 9;
    public const int PLUG_BODY_H    = 7;
    public const int PLUG_CABLE_W   = 2;
    public const int PLUG_CABLE_H   = 4;
    public const int PLUG_OFFSET_Y  = 0;

    public static Color BattColor(int pct)
    {
        if (pct > THRESH_HIGH) return COLOR_HIGH;
        if (pct > THRESH_MED)  return COLOR_MED;
        if (pct > THRESH_LOW)  return COLOR_LOW;
        return COLOR_CRIT;
    }

    public static Color Hex(string hex)
    {
        hex = hex.TrimStart('#');
        return Color.FromArgb(
            Convert.ToInt32(hex[0..2], 16),
            Convert.ToInt32(hex[2..4], 16),
            Convert.ToInt32(hex[4..6], 16));
    }
}
// ═══════════════════════════════════════════════════════════════════

static class WinApi
{
    [DllImport("user32.dll")]   public static extern IntPtr FindWindow(string cls, string? win);
    [DllImport("user32.dll")]   public static extern IntPtr FindWindowEx(IntPtr p, IntPtr a, string cls, string? win);
    [DllImport("user32.dll")]   public static extern bool   GetWindowRect(IntPtr hwnd, out RECT r);
    [DllImport("user32.dll")]   public static extern bool   SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int w, int h, uint f);
    [DllImport("user32.dll")]   public static extern int    SetWindowLong(IntPtr hwnd, int idx, int val);
    [DllImport("user32.dll")]   public static extern int    GetWindowLong(IntPtr hwnd, int idx);
    [DllImport("user32.dll")]   public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]   public static extern IntPtr SetWinEventHook(uint eMin, uint eMax, IntPtr hmod, WinEventDelegate fn, uint pid, uint tid, uint flags);
    [DllImport("user32.dll")]   public static extern bool   UnhookWinEvent(IntPtr hook);
    [DllImport("user32.dll")]   public static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll")]   public static extern int    GetSystemMetrics(int n);
    [DllImport("kernel32.dll")] public static extern bool   GetSystemPowerStatus(out POWER_STATUS s);
    [DllImport("powrprof.dll")] public static extern uint   PowerGetActualOverlayScheme(out Guid guid);
    [DllImport("powrprof.dll")] public static extern uint   PowerSetActiveOverlayScheme(ref Guid guid);
    [DllImport("gdi32.dll")]    public static extern int    AddFontResourceEx(string path, uint fl, IntPtr pdv);

    public delegate void WinEventDelegate(IntPtr hHook, uint evType, IntPtr hwnd, int idObj, int idChild, uint tid, uint time);

    public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
    public const uint EVENT_OBJECT_REORDER    = 0x8004;
    public const uint WINEVENT_OUTOFCONTEXT   = 0x0000;
    public const int  GWL_EXSTYLE      = -20;
    public const int  WS_EX_TOPMOST    = 0x00000008;
    public const int  WS_EX_TOOLWINDOW = 0x00000080;
    public const int  WS_EX_NOACTIVATE = 0x08000000;
    public const uint SWP_NOMOVE       = 0x0002;
    public const uint SWP_NOSIZE       = 0x0001;
    public const uint SWP_NOACTIVATE   = 0x0010;
    public const uint SWP_SHOWWINDOW   = 0x0040;
    public static readonly IntPtr HWND_TOPMOST = new(-1);
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT { public int Left, Top, Right, Bottom; }

[StructLayout(LayoutKind.Sequential)]
public struct POWER_STATUS
{
    public byte ACLineStatus, BatteryFlag, BatteryLifePercent, SystemStatusFlag;
    public uint BatteryLifeTime, BatteryFullLifeTime;
}

static class PowerPlans
{
    public static readonly (string Key, Guid Guid)[] Plans =
    [
        ("high_perf", new Guid("ded574b5-45a0-4f42-8737-46345c09c238")),
        ("balanced",  new Guid("00000000-0000-0000-0000-000000000000")),
        ("saver",     new Guid("961cc777-2547-4f9d-8174-7d86181b8a7a")),
    ];

    public static Guid? GetActive()
    {
        if (WinApi.PowerGetActualOverlayScheme(out var g) == 0) return g;
        return null;
    }

    public static void Set(Guid g) => WinApi.PowerSetActiveOverlayScheme(ref g);
}

// ── Ustawienia — rejestr HKCU\SOFTWARE\BatteryBar ────────────────────────────
static class Settings
{
    const string REG_KEY = @"SOFTWARE\BatteryBar";

    public static string GetLanguage()
    {
        try
        {
            using var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REG_KEY);
            return k?.GetValue("Language") as string ?? CFG.LANG_DEFAULT;
        }
        catch { return CFG.LANG_DEFAULT; }
    }

    public static void SetLanguage(string lang)
    {
        try
        {
            using var k = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(REG_KEY);
            k.SetValue("Language", lang);
        }
        catch { }
    }
}

static class Lang
{
    // wczytaj z rejestru przy starcie (fallback do LANG_DEFAULT z CFG)
    public static string Current = Settings.GetLanguage();

    static readonly Dictionary<string, Dictionary<string, string>> S = new()
    {
        ["pl"] = new() {
            ["high_perf"]   = "Wysoka wydajność",
            ["balanced"]    = "Zrównoważony",
            ["saver"]       = "Oszczędzanie energii",
            ["quit"]        = "Zamknij",
            ["show_hide"]   = "Pokaż / Ukryj",
            ["lang_switch"] = "Switch to English",
            ["charging"]    = "(ładuje)",
        },
        ["en"] = new() {
            ["high_perf"]   = "High performance",
            ["balanced"]    = "Balanced",
            ["saver"]       = "Power saver",
            ["quit"]        = "Quit",
            ["show_hide"]   = "Show / Hide",
            ["lang_switch"] = "Przełącz na polski",
            ["charging"]    = "(charging)",
        },
    };

    public static string T(string key) => S[Current].GetValueOrDefault(key, key);

    public static void Toggle()
    {
        Current = Current == "pl" ? "en" : "pl";
        Settings.SetLanguage(Current);
    }
}

static class BI
{
    public const string HIGH_PERF = "\uf48e";
    public const string BALANCED  = "\uf5a1";
    public const string SAVER     = "\uf46e";
    public const string QUIT_ICON = "\uf62a";
    public const string CHECK     = "\uf26b";

    public static Font? IconFont;
    public static bool  Loaded;

    public static void Load()
    {
        try
        {
            // Environment.ProcessPath = rzeczywista lokalizacja exe, nawet przy single-file publish
            var exeDir = Path.GetDirectoryName(Environment.ProcessPath) ?? ".";
            var path   = Path.Combine(exeDir, "bootstrap-icons.ttf");
            if (!File.Exists(path))
            {
                using var client = new System.Net.Http.HttpClient();
                var data = client.GetByteArrayAsync(
                    "https://github.com/twbs/icons/raw/v1.11.3/font/fonts/bootstrap-icons.ttf").Result;
                File.WriteAllBytes(path, data);
            }
            WinApi.AddFontResourceEx(path, 0x10, IntPtr.Zero);
            var pfc = new PrivateFontCollection();
            pfc.AddFontFile(path);
            IconFont = new Font(pfc.Families[0], 13f, FontStyle.Regular, GraphicsUnit.Point);
            Loaded   = true;
        }
        catch { }
    }
}

// ── Menu ──────────────────────────────────────────────────────────────────────
class MenuWindow : Form
{
    static readonly Color BG_M  = CFG.Hex("#18181b");
    static readonly Color HOV   = CFG.Hex("#27272a");
    static readonly Color FG_M  = Color.White;
    static readonly Color ACC   = CFG.Hex("#38bdf8");
    static readonly Color SEP   = CFG.Hex("#3f3f46");
    static readonly Font  FONT_T = new("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

    bool _closeReady = false;
    readonly FlowLayoutPanel _inner;

    public MenuWindow()
    {
        FormBorderStyle = FormBorderStyle.None;
        BackColor       = BG_M;
        ShowInTaskbar   = false;
        TopMost         = true;
        StartPosition   = FormStartPosition.Manual;
        Padding         = new Padding(1);

        _inner = new FlowLayoutPanel {
            FlowDirection = FlowDirection.TopDown,
            WrapContents  = false,
            AutoSize      = true,
            AutoSizeMode  = AutoSizeMode.GrowAndShrink,
            BackColor     = BG_M,
            Padding       = new Padding(0, 4, 0, 4),
            Margin        = Padding.Empty,
        };
        Controls.Add(_inner);

        var activeGuid = PowerPlans.GetActive();
        var planIcons  = BI.Loaded ? new[] { BI.HIGH_PERF, BI.BALANCED, BI.SAVER } : new[] { "", "", "" };

        for (int i = 0; i < PowerPlans.Plans.Length; i++)
        {
            var (key, guid) = PowerPlans.Plans[i];
            bool active     = activeGuid.HasValue &&
                              activeGuid.Value.ToString().Equals(guid.ToString(), StringComparison.OrdinalIgnoreCase);
            var g = guid;
            AddRow(planIcons[i], Lang.T(key), active, () => { PowerPlans.Set(g); Close(); });
        }

        AddSep();
        AddRow(BI.Loaded ? BI.QUIT_ICON : "", Lang.T("quit"), false, () => Application.Exit());

        Shown += (_, _) =>
        {
            var t = new System.Windows.Forms.Timer { Interval = 150 };
            t.Tick += (_, _) => { t.Stop(); _closeReady = true; };
            t.Start();
        };
        Deactivate += (_, _) => { if (_closeReady) Close(); };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(SEP, 1);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    public void ShowAt(Point anchor)
    {
        Location = new Point(-9999, -9999);
        Show();
        ClientSize = _inner.PreferredSize + new Size(2, 2);
        Location   = new Point(anchor.X, anchor.Y - Height - 4);
        Activate();
    }

    void AddRow(string icon, string text, bool active, Action onClick)
    {
        bool showIcon = BI.Loaded && icon.Length > 0;
        int cols = showIcon ? (active ? 3 : 2) : 1;
        var row  = new TableLayoutPanel {
            ColumnCount  = cols,
            RowCount     = 1,
            AutoSize     = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor    = BG_M,
            Cursor       = Cursors.Hand,
            Margin       = Padding.Empty,
        };

        int col = 0;
        if (showIcon)
        {
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
            row.Controls.Add(MkLabel(icon, BI.IconFont ?? FONT_T, active ? ACC : FG_M,
                new Size(32, 30), new Padding(8,0,0,0), true), col++, 0);
        }

        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        row.Controls.Add(MkLabel(text, FONT_T, active ? ACC : FG_M,
            Size.Empty, new Padding(showIcon ? 4 : 12, 6, 14, 6)), col++, 0);

        if (active && showIcon)
        {
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28));
            row.Controls.Add(MkLabel(BI.CHECK, BI.IconFont ?? FONT_T, ACC,
                new Size(28, 30), Padding.Empty, true), col, 0);
        }

        void Enter(object? s, EventArgs e) { row.BackColor = HOV; foreach (Control c in row.Controls) c.BackColor = HOV; }
        void Leave(object? s, EventArgs e) { row.BackColor = BG_M; foreach (Control c in row.Controls) c.BackColor = BG_M; }
        void Click(object? s, EventArgs e) => onClick();

        row.MouseEnter += Enter; row.MouseLeave += Leave; row.Click += Click;
        foreach (Control c in row.Controls) { c.MouseEnter += Enter; c.MouseLeave += Leave; c.Click += Click; }

        _inner.Controls.Add(row);
    }

    Label MkLabel(string text, Font font, Color fg, Size size, Padding pad, bool center = false)
    {
        var l = new Label {
            Text      = text,
            Font      = font,
            ForeColor = fg,
            BackColor = BG_M,
            AutoSize  = size == Size.Empty,
            Padding   = pad,
            Margin    = Padding.Empty,
            TextAlign = center ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft,
        };
        if (size != Size.Empty) l.Size = size;
        return l;
    }

    void AddSep() =>
        _inner.Controls.Add(new Panel { BackColor = SEP, Size = new Size(180, 1), Margin = new Padding(10, 2, 10, 2) });
}

// ── Główny widget ─────────────────────────────────────────────────────────────
class BatteryWidget : Form
{
    int _ww, _wh;
    IntPtr _hook1, _hook2;
    WinApi.WinEventDelegate? _hookDelegate;
    bool _widgetVisible = true;

    int  _pct      = 0;
    bool _charging = false;
    int  _secsLeft = -1;

    NotifyIcon? _tray;
    MenuWindow? _menu;

    public BatteryWidget()
    {
        _ww = CFG.PILL_W + CFG.NUB_W + CFG.PLUG_OFFSET_X + 18;
        var (x, y, _, h) = CalcGeom();
        _wh = h;

        FormBorderStyle = FormBorderStyle.None;
        BackColor       = CFG.BG;
        TransparencyKey = CFG.BG;
        Size            = new Size(_ww, _wh);
        ShowInTaskbar   = false;
        TopMost         = true;
        StartPosition   = FormStartPosition.Manual;
        Location        = new Point(x, y);
        DoubleBuffered  = true;
        Cursor          = Cursors.Hand;

        var pill = new Panel {
            Bounds    = new Rectangle(0, 0, CFG.PILL_W + CFG.NUB_W + 4, _wh),
            BackColor = Color.Transparent,
        };
        pill.Click += (_, _) => ShowMenu();
        Controls.Add(pill);

        Click      += (_, _) => ShowMenu();
        Load       += OnLoad;
        FormClosed += (_, _) => Cleanup();
    }

    (int x, int y, int w, int h) CalcGeom()
    {
        var tbHwnd   = WinApi.FindWindow("Shell_TrayWnd", null);
        var trayHwnd = WinApi.FindWindowEx(tbHwnd, IntPtr.Zero, "TrayNotifyWnd", null);
        WinApi.GetWindowRect(tbHwnd,   out RECT tb);
        WinApi.GetWindowRect(trayHwnd, out RECT tray);
        int h = tb.Bottom - tb.Top;
        int w = CFG.PILL_W + CFG.NUB_W + CFG.PLUG_OFFSET_X + 18;
        int x = tray.Left - w - CFG.OFFSET_FROM_TRAY;
        int y = tb.Top;
        return (x, y, w, h);
    }

    void Reposition()
    {
        var (x, y, w, h) = CalcGeom();
        if (w != _ww || h != _wh) { _ww = w; _wh = h; Size = new Size(w, h); }
        WinApi.SetWindowPos(Handle, WinApi.HWND_TOPMOST, x, y, w, h, WinApi.SWP_NOACTIVATE);
    }

    void ReadBattery()
    {
        if (!WinApi.GetSystemPowerStatus(out var s)) return;
        if (s.BatteryLifePercent == 255) return;
        _pct      = s.BatteryLifePercent;
        _charging = s.ACLineStatus == 1;
        _secsLeft = s.BatteryLifeTime == 0xFFFFFFFF ? -1 : (int)s.BatteryLifeTime;
    }

    string GetLabel()
    {
        if (_charging) return $"{_pct}%";
        if (_secsLeft > 0 && _secsLeft < 86400)
        {
            int h = _secsLeft / 3600;
            int m = (_secsLeft % 3600) / 60;
            return $"{h:D2}:{m:D2}";
        }
        return $"{_pct}%";
    }

    void StartTray()
    {
        _tray = new NotifyIcon { Visible = true, Text = $"{_pct}%", Icon = DrawTrayIcon() };
        RebuildTrayMenu();
    }

    // odbuduj menu tray (potrzebne po zmianie języka)
    void RebuildTrayMenu()
    {
        if (_tray == null) return;
        var cm = new ContextMenuStrip();
        cm.Items.Add(Lang.T("show_hide"), null, (_, _) => ToggleVisible());
        cm.Items.Add("-");
        cm.Items.Add(Lang.T("lang_switch"), null, (_, _) =>
        {
            Lang.Toggle();
            RebuildTrayMenu();   // odśwież etykiety po zmianie języka
        });
        cm.Items.Add("-");
        cm.Items.Add(Lang.T("quit"), null, (_, _) => Application.Exit());
        _tray.ContextMenuStrip = cm;
    }

    [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr handle);

    Icon DrawTrayIcon()
    {
        var bmp = new Bitmap(64, 64);
        using var g = Graphics.FromImage(bmp);
        var col = CFG.BattColor(_pct);
        g.Clear(Color.Transparent);
        using (var pen = new Pen(col, 4)) g.DrawRectangle(pen, 4, 16, 52, 32);
        using var br = new SolidBrush(col);
        g.FillRectangle(br, 56, 24, 6, 16);
        int fw = (int)(46 * _pct / 100.0);
        if (fw > 0) g.FillRectangle(br, 8, 20, fw, 24);
        if (_charging)
            using (var wbr = new SolidBrush(Color.White))
                g.FillPolygon(wbr, new Point[] { new(32,18),new(24,34),new(31,34),new(28,46),new(40,30),new(33,30) });

        // GetHicon tworzy uchwyt GDI — musi być zwolniony przez DestroyIcon
        var hicon = bmp.GetHicon();
        var icon  = (Icon)Icon.FromHandle(hicon).Clone(); // klonuj zanim zniszczymy uchwyt
        DestroyIcon(hicon);
        bmp.Dispose();
        return icon;
    }

    void UpdateTray()
    {
        if (_tray == null) return;
        _tray.Text = $"{(_pct)}% " + (_charging ? Lang.T("charging") : "");
        try { _tray.Icon?.Dispose(); } catch { }
        _tray.Icon = DrawTrayIcon();
    }

    void ToggleVisible()
    {
        if (_widgetVisible) { Hide(); _widgetVisible = false; }
        else                { Show(); DoRaise(); _widgetVisible = true; }
    }


    bool IsFullscreen()
    {
        var fg = WinApi.GetForegroundWindow();
        if (fg == IntPtr.Zero || fg == Handle) return false;
        WinApi.GetWindowRect(fg, out RECT r);
        int sw = WinApi.GetSystemMetrics(0);
        int sh = WinApi.GetSystemMetrics(1);
        return r.Left <= 0 && r.Top <= 0 && r.Right >= sw && r.Bottom >= sh;
    }

    void CheckFullscreen()
    {
        bool fs = IsFullscreen();
        if (fs && _widgetVisible)
        {
            BeginInvoke(Hide);
            _widgetVisible = false;
        }
        else if (!fs && !_widgetVisible)
        {
            BeginInvoke(() => { Show(); DoRaise(); });
            _widgetVisible = true;
        }
    }

    void DoRaise() =>
        WinApi.SetWindowPos(Handle, WinApi.HWND_TOPMOST, 0, 0, 0, 0,
                            WinApi.SWP_NOMOVE | WinApi.SWP_NOSIZE | WinApi.SWP_NOACTIVATE);

    void OnLoad(object? s, EventArgs e)
    {
        int ex = WinApi.GetWindowLong(Handle, WinApi.GWL_EXSTYLE);
        ex |= WinApi.WS_EX_NOACTIVATE | WinApi.WS_EX_TOOLWINDOW | WinApi.WS_EX_TOPMOST;
        WinApi.SetWindowLong(Handle, WinApi.GWL_EXSTYLE, ex);
        Reposition();

        _hookDelegate = (_, _, _, _, _, _, _) => BeginInvoke(() => { if (_widgetVisible) DoRaise(); });
        _hook1 = WinApi.SetWinEventHook(WinApi.EVENT_SYSTEM_FOREGROUND, WinApi.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _hookDelegate, 0, 0, WinApi.WINEVENT_OUTOFCONTEXT);
        _hook2 = WinApi.SetWinEventHook(WinApi.EVENT_OBJECT_REORDER,    WinApi.EVENT_OBJECT_REORDER,    IntPtr.Zero, _hookDelegate, 0, 0, WinApi.WINEVENT_OUTOFCONTEXT);

        // timer z-order: tylko podnosi widget, nie sprawdza fullscreen
        var timerZ = new System.Windows.Forms.Timer { Interval = CFG.TOPMOST_MS };
        timerZ.Tick += (_, _) => { if (_widgetVisible) DoRaise(); };
        timerZ.Start();

        // timer fullscreen: wolniejszy, z debounce
        var timerFS = new System.Windows.Forms.Timer { Interval = 200 };
        timerFS.Tick += (_, _) => CheckFullscreen();
        timerFS.Start();

        ReadBattery();
        Invalidate();
        StartTray();

        var timerR = new System.Windows.Forms.Timer { Interval = CFG.REPOSITION_MS };
        timerR.Tick += (_, _) => Reposition();
        timerR.Start();

        var timerB = new System.Windows.Forms.Timer { Interval = CFG.UPDATE_MS };
        timerB.Tick += (_, _) => { ReadBattery(); Invalidate(); UpdateTray(); };
        timerB.Start();
    }

    void Cleanup()
    {
        if (_hook1 != IntPtr.Zero) WinApi.UnhookWinEvent(_hook1);
        if (_hook2 != IntPtr.Zero) WinApi.UnhookWinEvent(_hook2);
        if (_tray  != null) { _tray.Visible = false; _tray.Dispose(); }
    }

    void ShowMenu()
    {
        if (_menu != null && !_menu.IsDisposed) { _menu.Close(); return; }
        _menu = new MenuWindow();
        _menu.ShowAt(new Point(Left, Top));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.Clear(CFG.BG);

        var col   = CFG.BattColor(_pct);
        int x0    = 2, x1 = 2 + CFG.PILL_W;
        int y0    = (_wh - CFG.PILL_H) / 2;
        int fillW = Math.Max(0, CFG.PILL_W * _pct / 100);

        using (var br = new SolidBrush(CFG.PILL_BG))
            g.FillRectangle(br, x0, y0, CFG.PILL_W, CFG.PILL_H);

        if (fillW > 0)
            using (var br = new SolidBrush(col))
                g.FillRectangle(br, x0, y0, fillW, CFG.PILL_H);

        int nubY = (_wh - CFG.NUB_H) / 2;
        using (var br = new SolidBrush(CFG.OUTLINE))
            g.FillRectangle(br, x1, nubY, CFG.NUB_W, CFG.NUB_H);

        // Center alignment = linia wyśrodkowana na granicy jak Python's create_rectangle
        // half = połowa grubości pióra żeby nie wychodziło poza prostokąt po lewej/górze
        float half = CFG.OUTLINE_W / 2f;
        using (var pen = new Pen(CFG.OUTLINE, CFG.OUTLINE_W))
            g.DrawRectangle(pen, x0 + half, y0 + half, CFG.PILL_W - CFG.OUTLINE_W, CFG.PILL_H - CFG.OUTLINE_W);

        var   textCol = fillW > CFG.TEXT_MIN_FILL_PX ? CFG.TEXT_COLOR : CFG.TEXT_COLOR_EMPTY;
        var   fstyle  = CFG.FONT_BOLD ? FontStyle.Bold : FontStyle.Regular;
        using var font = new Font(CFG.FONT_FAMILY, CFG.FONT_SIZE, fstyle, GraphicsUnit.Point);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var tbr = new SolidBrush(textCol);
        g.DrawString(GetLabel(), font, tbr, new RectangleF(x0, y0, CFG.PILL_W, CFG.PILL_H), sf);

        if (_charging) DrawPlug(g, x1 + CFG.NUB_W + CFG.PLUG_OFFSET_X, _wh / 2);
    }

    void DrawPlug(Graphics g, int cx, int cy)
    {
        using var br = new SolidBrush(CFG.PLUG_COLOR);
        int top = cy - CFG.PLUG_PRONG_H - CFG.PLUG_BODY_H / 2 + CFG.PLUG_OFFSET_Y;
        int hg  = CFG.PLUG_PRONG_GAP / 2;
        int lx  = cx - hg - CFG.PLUG_PRONG_W;
        int rx  = cx + hg;
        int py1 = top + CFG.PLUG_PRONG_H;
        g.FillRectangle(br, lx, top, CFG.PLUG_PRONG_W, CFG.PLUG_PRONG_H);
        g.FillRectangle(br, rx, top, CFG.PLUG_PRONG_W, CFG.PLUG_PRONG_H);
        int hw = CFG.PLUG_BODY_W / 2;
        int bx0 = cx-hw, bx1 = cx+hw, by0 = py1, by1 = by0+CFG.PLUG_BODY_H, c = 2;
        g.FillPolygon(br, new Point[] {
            new(bx0,by0),new(bx1,by0),new(bx1,by1-c),new(bx1-c,by1),new(bx0+c,by1),new(bx0,by1-c)
        });
        int cw2 = CFG.PLUG_CABLE_W / 2;
        g.FillRectangle(br, cx-cw2, by1, CFG.PLUG_CABLE_W, CFG.PLUG_CABLE_H);
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        // loguj wszystkie nieobsłużone wyjątki do pliku obok exe
        var logPath = Path.Combine(
            Path.GetDirectoryName(Environment.ProcessPath) ?? ".", "BatteryBar_crash.log");

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject?.ToString() ?? "unknown";
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CRASH:\n{ex}\n\n");
        };

        Application.ThreadException += (_, e) =>
        {
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] THREAD:\n{e.Exception}\n\n");
        };

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        try
        {
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; Application.Exit(); };
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Task.Run(BI.Load);
            Application.Run(new BatteryWidget());
        }
        catch (Exception ex)
        {
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] MAIN:\n{ex}\n\n");
            MessageBox.Show(
                $"Błąd:\n\n{ex.Message}\n\nSzczegóły zapisane w:\n{logPath}",
                "BatteryBar — błąd",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}