using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/// <summary>
/// 全局热键截图工具：按下 Alt + Shift + A 截取全屏（所有显示器）并保存为 PNG。
/// 以隐藏窗口 + 托盘图标方式常驻后台。
/// </summary>
static class HotkeyScreenshot
{
    [DllImport("user32.dll")]
    static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    const int WM_HOTKEY = 0x0312;
    const int HOTKEY_ID = 0xA5A5;
    const uint MOD_ALT = 0x0001;
    const uint MOD_SHIFT = 0x0004;
    const uint VK_A = 0x41;

    static NotifyIcon _tray;
    static string _saveDir;

    [STAThread]
    static void Main()
    {
        _saveDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "截图");
        try { Directory.CreateDirectory(_saveDir); }
        catch { _saveDir = AppDomain.CurrentDomain.BaseDirectory; }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using (var form = new HotkeyForm())
        {
            if (!RegisterHotKey(form.Handle, HOTKEY_ID, MOD_ALT | MOD_SHIFT, VK_A))
            {
                MessageBox.Show(
                    "注册 Alt + Shift + A 失败：该热键可能已被其他程序占用。",
                    "截图热键", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            InitTray();
            Application.Run();
        }

        UnregisterHotKey(IntPtr.Zero, HOTKEY_ID);
        if (_tray != null) { _tray.Visible = false; _tray.Dispose(); }
    }

    static void InitTray()
    {
        _tray = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "截图工具 (Alt+Shift+A)",
            Visible = true
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("截图 (Alt+Shift+A)", null, (s, e) => Capture());
        menu.Items.Add("打开截图文件夹", null, (s, e) =>
        {
            try { System.Diagnostics.Process.Start(_saveDir); } catch { }
        });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (s, e) => Application.Exit());

        _tray.ContextMenuStrip = menu;
        _tray.DoubleClick += (s, e) => Capture();
    }

    static void Capture()
    {
        try
        {
            // 截取所有显示器组成的虚拟屏幕
            Rectangle bounds = SystemInformation.VirtualScreen;
            string file = Path.Combine(
                _saveDir,
                "screenshot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png");

            using (var bmp = new Bitmap(bounds.Width, bounds.Height))
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                bmp.Save(file, ImageFormat.Png);
            }

            _tray.ShowBalloonTip(1500, "截图已保存", file, ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            _tray.ShowBalloonTip(2000, "截图失败", ex.Message, ToolTipIcon.Error);
        }
    }

    class HotkeyForm : Form
    {
        protected override void SetVisibleCore(bool value)
        {
            // 始终隐藏，不显示窗口
            base.SetVisibleCore(false);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80; // WS_EX_TOOLWINDOW：不在任务栏显示
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                Capture();
            }
            base.WndProc(ref m);
        }
    }
}
