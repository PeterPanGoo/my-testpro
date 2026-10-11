# HotkeyScreenshot.ps1 — 全局热键截图：Alt + Shift + A 截取全屏保存为 PNG
$ErrorActionPreference = 'Stop'
$log = Join-Path $PSScriptRoot 'log.txt'
function L([string]$msg) { try { Add-Content -Path $log -Value ("{0}  {1}" -f (Get-Date -Format 'HH:mm:ss.fff'), $msg) } catch {} }

L "=== start ==="
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
L ("assemblies loaded, apartment=" + [System.Threading.Thread]::CurrentThread.ApartmentState)

$refs = [System.AppDomain]::CurrentDomain.GetAssemblies() |
    Where-Object { $_.Location -and (Test-Path $_.Location) } |
    Select-Object -ExpandProperty Location
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class HotkeyForm : Form
{
    [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    const int WM_HOTKEY = 0x0312;
    const int HOTKEY_ID = 0xA5A5;
    const uint MOD_ALT = 0x0001;
    const uint MOD_SHIFT = 0x0004;
    const uint VK_A = 0x41;

    public bool Registered;
    NotifyIcon tray;
    string saveDir;

    public HotkeyForm()
    {
        saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "截图");
        try { Directory.CreateDirectory(saveDir); } catch { saveDir = AppDomain.CurrentDomain.BaseDirectory; }

        Registered = RegisterHotKey(this.Handle, HOTKEY_ID, MOD_ALT | MOD_SHIFT, VK_A);
        if (!Registered) return;

        tray = new NotifyIcon { Icon = SystemIcons.Application, Text = "截图工具 (Alt+Shift+A)", Visible = true };
        var menu = new ContextMenuStrip();
        menu.Items.Add("截图 (Alt+Shift+A)", null, (s, e) => TakeScreenshot());
        menu.Items.Add("打开截图文件夹", null, (s, e) => { try { System.Diagnostics.Process.Start(saveDir); } catch { } });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (s, e) => Application.Exit());
        tray.ContextMenuStrip = menu;
        tray.DoubleClick += (s, e) => TakeScreenshot();
    }

    protected override void SetVisibleCore(bool value) { base.SetVisibleCore(false); }
    protected override CreateParams CreateParams
    {
        get { var cp = base.CreateParams; cp.ExStyle |= 0x80; return cp; }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID) TakeScreenshot();
        base.WndProc(ref m);
    }

    void TakeScreenshot()
    {
        try
        {
            Rectangle b = SystemInformation.VirtualScreen;
            string file = Path.Combine(saveDir, "screenshot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png");
            using (var bmp = new Bitmap(b.Width, b.Height))
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(b.Location, Point.Empty, b.Size);
                bmp.Save(file, ImageFormat.Png);
            }
            try { tray.ShowBalloonTip(1500, "截图已保存", file, ToolTipIcon.Info); } catch { }
        }
        catch (Exception ex)
        {
            try { tray.ShowBalloonTip(2000, "截图失败", ex.Message, ToolTipIcon.Error); } catch { }
        }
    }

    protected override void Dispose(bool disposing)
    {
        try { UnregisterHotKey(IntPtr.Zero, HOTKEY_ID); } catch { }
        if (tray != null) { tray.Visible = false; tray.Dispose(); }
        base.Dispose(disposing);
    }
}
'@
L "addtype compiled"

$form = New-Object HotkeyForm
L ("form created, registered=" + $form.Registered)

if (-not $form.Registered) {
    L "hotkey registration FAILED"
    [System.Windows.Forms.MessageBox]::Show('注册 Alt+Shift+A 失败：该热键可能已被其他程序占用。', '截图热键')
    return
}

L "entering message loop"
[System.Windows.Forms.Application]::Run()
L "message loop exited"
