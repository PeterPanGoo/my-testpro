using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class BaseForm : Form
{
	private bool dark;

	private TAMode mode;

	internal ThemeConfig? themeConfig;

	private FormBorderStyle formBorderStyle = FormBorderStyle.Sizable;

	public bool IsFull;

	private float? dpi;

	private bool isload;

	internal bool is_resizable;

	internal Action? ONESC;

	/// <summary>
	/// 深色模式
	/// </summary>
	[Description("深色模式")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Dark
	{
		get
		{
			return dark;
		}
		set
		{
			if (dark != value)
			{
				dark = value;
				mode = ((!dark) ? TAMode.Light : TAMode.Dark);
				if (IsHandleCreated && Win32.WindowTheme(this, value))
				{
					SetThemeOK(value);
				}
			}
		}
	}

	/// <summary>
	/// 色彩模式
	/// </summary>
	[Description("色彩模式")]
	[Category("Appearance")]
	[DefaultValue(TAMode.Auto)]
	public TAMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			if (mode != value)
			{
				mode = value;
				if (mode == TAMode.Dark || mode == TAMode.Auto || Config.Mode == TMode.Dark)
				{
					Dark = true;
				}
				else
				{
					Dark = false;
				}
			}
		}
	}

	/// <summary>
	/// 是否禁用主题
	/// </summary>
	[Description("是否禁用主题")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool DisableTheme { get; set; }

	[Description("指示窗体的边框和标题栏的外观和行为")]
	[Category("Behavior")]
	[DefaultValue(FormBorderStyle.Sizable)]
	public new FormBorderStyle FormBorderStyle
	{
		get
		{
			return formBorderStyle;
		}
		set
		{
			if (formBorderStyle != value)
			{
				base.FormBorderStyle = (formBorderStyle = value);
			}
		}
	}

	public virtual bool IsMax => WindowState == FormWindowState.Maximized;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public virtual bool AutoHandDpi { get; set; } = true;

	public float Dpi
	{
		get
		{
			if (Config._dpi_custom.HasValue)
			{
				return Config._dpi_custom.Value;
			}
			if (dpi.HasValue)
			{
				return dpi.Value;
			}
			float screenDpi = Helper.GetScreenDpi(this);
			dpi = screenDpi;
			return screenDpi;
		}
	}

	/// <summary>
	/// 鼠标拖拽大小使能
	/// </summary>
	[Description("鼠标拖拽大小使能")]
	[Category("Action")]
	[DefaultValue(true)]
	public bool EnableHitTest { get; set; } = true;

	public BaseForm()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		UpdateStyles();
	}

	public void SetCursor(bool val)
	{
		if (InvokeRequired)
		{
			Invoke(() =>
			{
				SetCursor(val);
			});
		}
		else
		{
			Cursor = (val ? Cursors.Hand : DefaultCursor);
		}
	}

	public ThemeConfig Theme()
	{
		themeConfig = new ThemeConfig(this);
		if (Config.ThemeConfig != null)
		{
			themeConfig.SetConfig(Config.ThemeConfig);
		}
		return themeConfig;
	}

	public void ThemeClear()
	{
		themeConfig = null;
	}

	internal void SetTheme()
	{
		if (DisableTheme)
		{
			return;
		}
		if (mode == TAMode.Auto)
		{
			if (themeConfig == null)
			{
				if (Config.ThemeConfig != null)
				{
					themeConfig = new ThemeConfig(this);
					themeConfig.SetConfig(Config.ThemeConfig);
					SetTheme(themeConfig);
				}
			}
			else
			{
				SetTheme(themeConfig);
			}
		}
		bool themeOK = mode == TAMode.Dark || (mode == TAMode.Auto && Config.Mode == TMode.Dark);
		if (Win32.WindowTheme(this, themeOK, one: true))
		{
			SetThemeOK(themeOK);
		}
	}

	protected virtual void SetThemeOK(bool dark)
	{
	}

	internal void SetTheme(ThemeConfig themeConfig)
	{
		if (themeConfig.pageheader == null && (themeConfig.headerLight.HasValue || themeConfig.headerDark.HasValue))
		{
			themeConfig.pageheader = GetPageHeader(Controls);
		}
		dark = Config.IsDark;
		themeConfig.Change(dark);
		this.Add();
	}

	private PageHeader? GetPageHeader(Control.ControlCollection controls)
	{
		foreach (Control control in controls)
		{
			if (control is PageHeader result)
			{
				return result;
			}
			if (control.Controls.Count > 0)
			{
				PageHeader pageHeader = GetPageHeader(control.Controls);
				if (pageHeader != null)
				{
					return pageHeader;
				}
			}
		}
		return null;
	}

	public virtual void RefreshDWM()
	{
	}

	/// <summary>
	/// 最小化
	/// </summary>
	public virtual void Min()
	{
		WindowState = FormWindowState.Minimized;
	}

	/// <summary>
	/// 最大化/还原
	/// </summary>
	public virtual bool MaxRestore()
	{
		if (IsFull)
		{
			base.FormBorderStyle = formBorderStyle;
			IsFull = false;
		}
		if (WindowState == FormWindowState.Maximized)
		{
			WindowState = FormWindowState.Normal;
			RefreshDWM();
			return false;
		}
		WindowState = FormWindowState.Maximized;
		RefreshDWM();
		return true;
	}

	/// <summary>
	/// 最大化
	/// </summary>
	public virtual void Max()
	{
		if (IsFull)
		{
			base.FormBorderStyle = formBorderStyle;
			IsFull = false;
		}
		WindowState = FormWindowState.Maximized;
		RefreshDWM();
	}

	/// <summary>
	/// 全屏/还原
	/// </summary>
	public virtual bool FullRestore()
	{
		if (WindowState == FormWindowState.Maximized)
		{
			NoFull();
			return false;
		}
		Full();
		return true;
	}

	/// <summary>
	/// 全屏
	/// </summary>
	public virtual void Full()
	{
		if (!IsFull)
		{
			IsFull = true;
			base.FormBorderStyle = FormBorderStyle.None;
			if (WindowState == FormWindowState.Maximized)
			{
				WindowState = FormWindowState.Normal;
			}
			WindowState = FormWindowState.Maximized;
			RefreshDWM();
		}
	}

	public virtual void NoFull()
	{
		if (IsFull)
		{
			IsFull = false;
			base.FormBorderStyle = formBorderStyle;
			WindowState = FormWindowState.Normal;
			RefreshDWM();
		}
		else if (IsMax)
		{
			MaxRestore();
		}
	}

	public void AutoDpi(Control control)
	{
		AutoDpi(Dpi, control);
	}

	public void AutoDpi(float dpi, Control control)
	{
		Helper.DpiAuto(dpi, control);
	}

	protected override void WndProc(ref System.Windows.Forms.Message m)
	{
		if (m.Msg == 736)
		{
			float old = Dpi;
			int dpiX = (int)(m.WParam.ToInt64() & 0xFFFF);
			int dpiY = (int)(m.WParam.ToInt64() >> 16);
			float _dpi = Helper.GetDpi(dpiX, dpiY);
			dpi = _dpi;
			if (AutoHandDpi)
			{
				BeginInvoke(() =>
				{
					Helper.DpiChangeAuto(_dpi, old, this);
				});
			}
		}
		try
		{
			base.WndProc(ref m);
		}
		catch
		{
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		if (!isload && !DesignMode)
		{
			OnCreated();
			SetTheme();
			if (AutoHandDpi)
			{
				AutoDpi(Dpi, this);
			}
			isload = true;
		}
	}

	public virtual void OnCreated()
	{
	}

	/// <summary>
	/// 拖动窗口（鼠标按下）
	/// </summary>
	public virtual void DraggableMouseDown()
	{
		if (!IsFull)
		{
			Win32.User32.ReleaseCapture();
			Win32.User32.SendMessage(Handle, 274, 61458, IntPtr.Zero);
		}
	}

	/// <summary>
	/// 调整窗口大小（鼠标移动）
	/// </summary>
	/// <returns>可以调整</returns>
	public virtual bool ResizableMouseMove()
	{
		if (WindowState == FormWindowState.Normal)
		{
			Win32.User32.HitTestValues hitTestValues = HitTest(PointToClient(Control.MousePosition));
			if (hitTestValues != Win32.User32.HitTestValues.HTNOWHERE)
			{
				Win32.User32.HitTestValues hitTestValues2 = hitTestValues;
				if (hitTestValues2 != Win32.User32.HitTestValues.HTCLIENT)
				{
					SetCursorHit(hitTestValues2);
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 调整窗口大小（鼠标移动）
	/// </summary>
	/// <param name="point">客户端坐标</param>
	/// <returns>可以调整</returns>
	public virtual bool ResizableMouseMove(Point point)
	{
		if (WindowState == FormWindowState.Normal)
		{
			Win32.User32.HitTestValues hitTestValues = HitTest(point);
			if (hitTestValues != Win32.User32.HitTestValues.HTNOWHERE)
			{
				Win32.User32.HitTestValues hitTestValues2 = hitTestValues;
				if (hitTestValues2 != Win32.User32.HitTestValues.HTCLIENT)
				{
					SetCursorHit(hitTestValues2);
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 整窗口大小（鼠标按下）
	/// </summary>
	/// <returns>可以调整</returns>
	public virtual bool ResizableMouseDown()
	{
		Point mousePosition = Control.MousePosition;
		Win32.User32.HitTestValues hitTestValues = HitTest(PointToClient(mousePosition));
		if (hitTestValues != Win32.User32.HitTestValues.HTCLIENT)
		{
			is_resizable = true;
			SetCursorHit(hitTestValues);
			Win32.User32.ReleaseCapture();
			Win32.User32.SendMessage(Handle, Win32.User32.WindowMessage.WM_NCLBUTTONDOWN, hitTestValues, Win32.Macros.MAKELPARAM(mousePosition.X, mousePosition.Y));
			is_resizable = false;
			return true;
		}
		return false;
	}

	internal Win32.User32.HitTestValues HitTest(Point point)
	{
		if (Window.CanHandMessage && EnableHitTest)
		{
			float num = 8f * Dpi;
			float num2 = num * 2f;
			Win32.User32.GetWindowRect(Handle, out var lpRect);
			Rectangle rectangle = new Rectangle(Point.Empty, lpRect.Size);
			Win32.User32.HitTestValues result = Win32.User32.HitTestValues.HTCLIENT;
			int num3 = point.X;
			int num4 = point.Y;
			if ((float)num3 < (float)rectangle.Left + num2 && (float)num4 < (float)rectangle.Top + num2)
			{
				result = Win32.User32.HitTestValues.HTTOPLEFT;
			}
			else if ((float)num3 >= (float)rectangle.Left + num2 && (float)num3 <= (float)rectangle.Right - num2 && (float)num4 <= (float)rectangle.Top + num)
			{
				result = Win32.User32.HitTestValues.HTTOP;
			}
			else if ((float)num3 > (float)rectangle.Right - num2 && (float)num4 <= (float)rectangle.Top + num2)
			{
				result = Win32.User32.HitTestValues.HTTOPRIGHT;
			}
			else if ((float)num3 <= (float)rectangle.Left + num && (float)num4 >= (float)rectangle.Top + num2 && (float)num4 <= (float)rectangle.Bottom - num2)
			{
				result = Win32.User32.HitTestValues.HTLEFT;
			}
			else if ((float)num3 >= (float)rectangle.Right - num && (float)num4 >= (float)(rectangle.Top * 2) + num && (float)num4 <= (float)rectangle.Bottom - num2)
			{
				result = Win32.User32.HitTestValues.HTRIGHT;
			}
			else if ((float)num3 <= (float)rectangle.Left + num2 && (float)num4 >= (float)rectangle.Bottom - num2)
			{
				result = Win32.User32.HitTestValues.HTBOTTOMLEFT;
			}
			else if ((float)num3 > (float)rectangle.Left + num2 && (float)num3 < (float)rectangle.Right - num2 && (float)num4 >= (float)rectangle.Bottom - num)
			{
				result = Win32.User32.HitTestValues.HTBOTTOM;
			}
			else if ((float)num3 >= (float)rectangle.Right - num2 && (float)num4 >= (float)rectangle.Bottom - num2)
			{
				result = Win32.User32.HitTestValues.HTBOTTOMRIGHT;
			}
			return result;
		}
		return Win32.User32.HitTestValues.HTCLIENT;
	}

	internal void SetCursorHit(Win32.User32.HitTestValues mode)
	{
		switch (mode)
		{
		case Win32.User32.HitTestValues.HTTOP:
		case Win32.User32.HitTestValues.HTBOTTOM:
			LoadCursors(32645);
			break;
		case Win32.User32.HitTestValues.HTLEFT:
		case Win32.User32.HitTestValues.HTRIGHT:
			LoadCursors(32644);
			break;
		case Win32.User32.HitTestValues.HTTOPLEFT:
		case Win32.User32.HitTestValues.HTBOTTOMRIGHT:
			LoadCursors(32642);
			break;
		case Win32.User32.HitTestValues.HTTOPRIGHT:
		case Win32.User32.HitTestValues.HTBOTTOMLEFT:
			LoadCursors(32643);
			break;
		}
	}

	internal void LoadCursors(int id)
	{
		IntPtr intPtr = Win32.User32.LoadCursor(IntPtr.Zero, id);
		if (!(intPtr == IntPtr.Zero))
		{
			Win32.User32.SetCursor(intPtr);
		}
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (ONESC == null)
		{
			return base.ProcessDialogKey(keyData);
		}
		if ((keyData & (Keys.Control | Keys.Alt)) == 0 && (keyData & Keys.KeyCode) == Keys.Escape)
		{
			ONESC();
			return true;
		}
		return base.ProcessDialogKey(keyData);
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public IAsyncResult BeginInvoke(Action method)
	{
		return BeginInvoke(method, null);
	}

	public void Invoke(Action method)
	{
		Invoke(method, null);
	}

	public T Invoke<T>(Func<T> method)
	{
		return (T)Invoke(method, null);
	}
}
