using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AntdUI;

public class Window : BaseForm, IMessageFilter
{
	private bool resizable = true;

	private WState winState;

	private bool ReadMessage;

	private bool _isaddMessage;

	private bool rmax;

	private bool DwmEnabled = true;

	private bool eNonclient = true;

	private static IntPtr TRUE = new IntPtr(1);

	private static IntPtr FALSE = new IntPtr(0);

	private bool iszoomed;

	private int oldmargin = -1;

	public static bool CanHandMessage = true;

	private const nint SIZE_RESTORED = 0;

	private const nint SIZE_MINIMIZED = 1;

	private const nint SIZE_MAXIMIZED = 2;

	internal Size? sizeInit;

	private Size? sizeNormal;

	[Description("调整窗口大小")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool Resizable
	{
		get
		{
			return resizable;
		}
		set
		{
			if (resizable != value)
			{
				resizable = value;
				HandMessage();
			}
		}
	}

	/// <summary>
	/// 窗口边框颜色
	/// </summary>
	[Description("窗口边框颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? BorderColor { get; set; }

	private WState WinState
	{
		set
		{
			if (winState != value)
			{
				winState = value;
				if (IsHandleCreated)
				{
					HandMessage();
				}
				EventHub.Dispatch(EventType.WINDOW_STATE);
			}
		}
	}

	protected virtual bool UseMessageFilter => false;

	private bool IsAddMessage
	{
		set
		{
			if (_isaddMessage != value)
			{
				_isaddMessage = value;
				if (value)
				{
					Application.AddMessageFilter(this);
				}
				else
				{
					Application.RemoveMessageFilter(this);
				}
			}
		}
	}

	/// <summary>
	/// 确定窗体是否出现在 Windows 任务栏中
	/// </summary>
	[Description("确定窗体是否出现在 Windows 任务栏中")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public new bool ShowInTaskbar
	{
		get
		{
			return base.ShowInTaskbar;
		}
		set
		{
			if (base.ShowInTaskbar == value)
			{
				return;
			}
			if (IsHandleCreated)
			{
				Size maximumSize = MaximumSize;
				Size minimumSize = MinimumSize;
				Size maximumSize2 = (MinimumSize = base.ClientSize);
				MaximumSize = maximumSize2;
				if (InvokeRequired)
				{
					Invoke(() => base.ShowInTaskbar = value);
				}
				else
				{
					base.ShowInTaskbar = value;
				}
				MinimumSize = minimumSize;
				MaximumSize = maximumSize;
				oldmargin = 0;
				DwmArea();
			}
			else
			{
				base.ShowInTaskbar = value;
			}
		}
	}

	protected override CreateParams CreateParams
	{
		get
		{
			if (DwmEnabled)
			{
				return base.CreateParams;
			}
			CreateParams createParams = base.CreateParams;
			createParams.Style &= -13107201;
			return createParams;
		}
	}

	/// <summary>
	/// 获取或设置窗体的位置
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public new Point Location
	{
		get
		{
			if (winState == WState.Restore)
			{
				return base.Location;
			}
			return ScreenRectangle.Location;
		}
		set
		{
			sizeNormal = null;
			base.Location = value;
		}
	}

	/// <summary>
	/// 控件的顶部坐标
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new int Top
	{
		get
		{
			return Location.Y;
		}
		set
		{
			sizeNormal = null;
			base.Top = value;
		}
	}

	/// <summary>
	/// 控件的左侧坐标
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new int Left
	{
		get
		{
			return Location.X;
		}
		set
		{
			sizeNormal = null;
			base.Left = value;
		}
	}

	/// <summary>
	/// 控件的右坐标
	/// </summary>
	public new int Right => ScreenRectangle.Right;

	/// <summary>
	/// 控件的底部坐标
	/// </summary>
	public new int Bottom => ScreenRectangle.Bottom;

	/// <summary>
	/// 获取或设置窗体的大小
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new Size Size
	{
		get
		{
			if (winState == WState.Restore)
			{
				return base.Size;
			}
			return ScreenRectangle.Size;
		}
		set
		{
			sizeNormal = null;
			base.Size = value;
			sizeInit = base.ClientSize;
		}
	}

	/// <summary>
	/// 获取或设置窗体的客户端区域的大小
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	[Localizable(true)]
	public new Size ClientSize
	{
		get
		{
			return base.ClientSize;
		}
		set
		{
			sizeNormal = null;
			base.ClientSize = value;
			sizeInit = base.ClientSize;
		}
	}

	/// <summary>
	/// 控件的宽度
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new int Width
	{
		get
		{
			return Size.Width;
		}
		set
		{
			sizeNormal = null;
			base.Width = value;
			sizeInit = base.ClientSize;
		}
	}

	/// <summary>
	/// 控件的高度
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new int Height
	{
		get
		{
			return Size.Height;
		}
		set
		{
			sizeNormal = null;
			base.Height = value;
			sizeInit = base.ClientSize;
		}
	}

	/// <summary>
	/// 获取或设置窗体屏幕区域
	/// </summary>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Always)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Rectangle ScreenRectangle
	{
		get
		{
			if (winState == WState.Restore)
			{
				return new Rectangle(base.Location, base.Size);
			}
			Rectangle clientRectangle = ClientRectangle;
			return new Rectangle(RectangleToScreen(Rectangle.Empty).Location, clientRectangle.Size);
		}
		set
		{
			sizeNormal = null;
			base.Location = value.Location;
			base.Size = value.Size;
			sizeInit = base.ClientSize;
		}
	}

	public override bool IsMax => winState == WState.Maximize;

	private void HandMessage()
	{
		ReadMessage = winState == WState.Restore && resizable;
		if (UseMessageFilter)
		{
			IsAddMessage = true;
		}
		else
		{
			IsAddMessage = ReadMessage;
		}
	}

	protected override void Dispose(bool disposing)
	{
		IsAddMessage = false;
		base.Dispose(disposing);
	}

	public override void OnCreated()
	{
		if (OS.Version.Major >= 6)
		{
			DwmEnabled = Win32.IsCompositionEnabled;
		}
		else
		{
			DwmEnabled = false;
		}
		Win32.User32.DisableProcessWindowsGhosting();
		if (WindowState == FormWindowState.Maximized)
		{
			rmax = true;
		}
		if (FormBorderStyle != FormBorderStyle.None && !rmax)
		{
			sizeInit = base.ClientSize;
			SetSize(sizeInit.Value);
		}
		HandMessage();
		DwmArea();
		eNonclient = false;
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		eNonclient = true;
		base.OnHandleDestroyed(e);
	}

	private void SetSize(Size size)
	{
		Size maximumSize = MaximumSize;
		Size minimumSize = MinimumSize;
		Size size2 = (base.ClientSize = size);
		Size maximumSize2 = (MinimumSize = size2);
		MaximumSize = maximumSize2;
		MinimumSize = minimumSize;
		MaximumSize = maximumSize;
	}

	protected override void OnLoad(EventArgs e)
	{
		Win32.User32.SetWindowPos(Handle, Win32.HWND.NULL, 0, 0, 0, 0, Win32.User32.SetWindowPosFlags.SWP_DRAWFRAME | Win32.User32.SetWindowPosFlags.SWP_NOMOVE | Win32.User32.SetWindowPosFlags.SWP_NOOWNERZORDER | Win32.User32.SetWindowPosFlags.SWP_NOSIZE | Win32.User32.SetWindowPosFlags.SWP_NOZORDER);
		base.OnLoad(e);
	}

	private void InvalidateNonclient()
	{
		if (!eNonclient)
		{
			Win32.User32.UpdateWindow(Handle);
			Win32.User32.SetWindowPos(Handle, Win32.HWND.NULL, 0, 0, 0, 0, Win32.User32.SetWindowPosFlags.SWP_DRAWFRAME | Win32.User32.SetWindowPosFlags.SWP_NOACTIVATE | Win32.User32.SetWindowPosFlags.SWP_NOCOPYBITS | Win32.User32.SetWindowPosFlags.SWP_NOMOVE | Win32.User32.SetWindowPosFlags.SWP_NOOWNERZORDER | Win32.User32.SetWindowPosFlags.SWP_NOSIZE | Win32.User32.SetWindowPosFlags.SWP_NOZORDER);
		}
	}

	protected override void WndProc(ref System.Windows.Forms.Message m)
	{
		switch ((Win32.User32.WindowMessage)m.Msg)
		{
		case Win32.User32.WindowMessage.WM_DESTROY:
			oldmargin = -1;
			break;
		case Win32.User32.WindowMessage.WM_ACTIVATE:
			DwmArea();
			break;
		case Win32.User32.WindowMessage.WM_NCCALCSIZE:
			if (m.WParam != IntPtr.Zero)
			{
				if (WmNCCalcSize(ref m))
				{
					return;
				}
				break;
			}
			goto default;
		case Win32.User32.WindowMessage.WM_SIZE:
			WmSize(ref m);
			break;
		case Win32.User32.WindowMessage.WM_ACTIVATEAPP:
		case Win32.User32.WindowMessage.WM_NCACTIVATE:
			if (DwmEnabled)
			{
				InvalidateNonclient();
				break;
			}
			m.Result = TRUE;
			return;
		default:
			if (WmGhostingHandler(m))
			{
				return;
			}
			break;
		}
		try
		{
			base.WndProc(ref m);
		}
		catch
		{
		}
	}

	private bool WmGhostingHandler(System.Windows.Forms.Message m)
	{
		int msg = m.Msg;
		if ((uint)(msg - 174) <= 1u || msg == 49596)
		{
			m.Result = FALSE;
			InvalidateNonclient();
		}
		return false;
	}

	private bool ISZoomed()
	{
		bool flag = Win32.User32.IsZoomed(Handle);
		if (iszoomed == flag)
		{
			return flag;
		}
		iszoomed = flag;
		DwmArea();
		return flag;
	}

	private void DwmArea()
	{
		int num = ((!iszoomed && !IsFull) ? 1 : 0);
		if (oldmargin != num)
		{
			oldmargin = num;
			Win32.DwmApi.DwmExtendFrameIntoClientArea(Handle, new Win32.DwmApi.MARGINS(num));
		}
	}

	protected override void SetThemeOK(bool dark)
	{
		Win32.SetWindowBorderColor(Handle, BorderColor);
	}

	public override void RefreshDWM()
	{
		DwmArea();
	}

	/// <summary>
	/// 调整窗口大小（鼠标移动）
	/// </summary>
	/// <returns>可以调整</returns>
	public override bool ResizableMouseMove()
	{
		Win32.User32.HitTestValues hitTestValues = HitTest(PointToClient(Control.MousePosition));
		if (hitTestValues != Win32.User32.HitTestValues.HTNOWHERE)
		{
			Win32.User32.HitTestValues hitTestValues2 = hitTestValues;
			if (hitTestValues2 != Win32.User32.HitTestValues.HTCLIENT && winState == WState.Restore)
			{
				SetCursorHit(hitTestValues2);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// 调整窗口大小（鼠标移动）
	/// </summary>
	/// <param name="point">客户端坐标</param>
	/// <returns>可以调整</returns>
	public override bool ResizableMouseMove(Point point)
	{
		Win32.User32.HitTestValues hitTestValues = HitTest(point);
		if (hitTestValues != Win32.User32.HitTestValues.HTNOWHERE)
		{
			Win32.User32.HitTestValues hitTestValues2 = hitTestValues;
			if (hitTestValues2 != Win32.User32.HitTestValues.HTCLIENT && winState == WState.Restore)
			{
				SetCursorHit(hitTestValues2);
				return true;
			}
		}
		return false;
	}

	public bool PreFilterMessage(ref System.Windows.Forms.Message m)
	{
		if (is_resizable)
		{
			return OnPreFilterMessage(m);
		}
		if (CanHandMessage && ReadMessage)
		{
			switch (m.Msg)
			{
			case 160:
			case 512:
				if (isMe(m.HWnd) && ResizableMouseMove(PointToClient(Control.MousePosition)))
				{
					return true;
				}
				break;
			case 161:
			case 513:
				if (isMe(m.HWnd) && ResizableMouseDown())
				{
					return true;
				}
				break;
			}
		}
		return OnPreFilterMessage(m);
	}

	protected virtual bool OnPreFilterMessage(System.Windows.Forms.Message m)
	{
		return false;
	}

	private bool isMe(IntPtr intPtr)
	{
		Control control = Control.FromHandle(intPtr);
		if (control == this || GetParent(control) == this)
		{
			return true;
		}
		return false;
	}

	private static Control? GetParent(Control? control)
	{
		try
		{
			if (control != null && control.IsHandleCreated && control.Parent != null)
			{
				if (control is Form)
				{
					return control;
				}
				return GetParent(control.Parent);
			}
		}
		catch
		{
		}
		return control;
	}

	private void WmSize(ref System.Windows.Forms.Message m)
	{
		if (m.WParam == (IntPtr)1)
		{
			WinState = WState.Minimize;
		}
		else if (m.WParam == (IntPtr)2)
		{
			WinState = WState.Maximize;
			Invalidate();
			InvalidateNonclient();
		}
		else if (m.WParam == (IntPtr)0)
		{
			WinState = WState.Restore;
			InvalidateNonclient();
			Invalidate();
			sizeNormal = ClientSize;
		}
	}

	private bool WmNCCalcSize(ref System.Windows.Forms.Message m)
	{
		if (FormBorderStyle == FormBorderStyle.None)
		{
			return false;
		}
		if (ISZoomed())
		{
			Win32.RECT structure = Marshal.PtrToStructure<Win32.RECT>(m.LParam);
			Padding nonClientMetrics = GetNonClientMetrics();
			structure.top -= nonClientMetrics.Top;
			structure.top += nonClientMetrics.Bottom;
			Marshal.StructureToPtr(structure, m.LParam, fDeleteOld: false);
			m.Result = new IntPtr(1024);
			return false;
		}
		m.Result = TRUE;
		return true;
	}

	protected override void SetClientSizeCore(int x, int y)
	{
		if (DesignMode)
		{
			Size = new Size(x, y);
		}
		else
		{
			base.SetClientSizeCore(x, y);
		}
	}

	protected Padding GetNonClientMetrics()
	{
		Rectangle clientRectangle = ClientRectangle;
		clientRectangle.Offset(-Bounds.Left, -Bounds.Top);
		Win32.RECT lpRect = new Win32.RECT(clientRectangle);
		Win32.User32.AdjustWindowRectEx(ref lpRect, (Win32.User32.WindowStyles)CreateParams.Style, bMenu: false, (Win32.User32.WindowStylesEx)CreateParams.ExStyle);
		return new Padding
		{
			Top = clientRectangle.Top - lpRect.top,
			Left = clientRectangle.Left - lpRect.left,
			Bottom = lpRect.bottom - clientRectangle.Bottom,
			Right = lpRect.right - clientRectangle.Right
		};
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		if (DesignMode)
		{
			base.SetBoundsCore(x, y, width, height, specified);
		}
		else if (WindowState == FormWindowState.Normal && sizeNormal.HasValue)
		{
			if (rmax)
			{
				rmax = false;
				base.SetBoundsCore(x, y, width, height, specified);
			}
			else
			{
				base.SetBoundsCore(x, y, sizeNormal.Value.Width, sizeNormal.Value.Height, BoundsSpecified.None);
			}
		}
		else
		{
			base.SetBoundsCore(x, y, width, height, specified);
		}
	}
}
