using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AntdUI;

[ToolboxItem(false)]
[Localizable(true)]
public class IControl : Control, BadgeConfig
{
	/// <summary>
	/// Bool 类型事件
	/// </summary>
	public delegate void DragEventHandler(object sender, StringsEventArgs e);

	private TAMode colorScheme;

	private string? badge;

	private string? badgeSvg;

	private TAlign badgeAlign = TAlign.TR;

	private float badgeSize = 0.6f;

	private bool badgeMode;

	private Color? badgefore;

	private Color? badgeback;

	private Color? badgeBorderColor;

	private float? badgeBorderWidth;

	private int badgeOffsetX = 1;

	private int badgeOffsetY = 1;

	private float? dpi;

	private static bool disableDataBinding;

	private CursorType oldcursor;

	private bool mdown;

	private int mdownd;

	private int oldX;

	private int oldY;

	private int mouseX;

	private int mouseY;

	private int oldMY;

	private AnimationTask? taskTouch;

	private AnimationTask? taskHover;

	private TimeSpan timeHover;

	private Stopwatch hoverStopwatch = new Stopwatch();

	private int oldx = -1;

	private int oldy = -1;

	private bool tmpHover;

	internal FileDropHandler? fileDrop;

	internal Func<string[], string[]?>? ONDRAG;

	/// <summary>
	/// 确定该控件是可见的还是隐藏的
	/// </summary>
	[Description("确定该控件是可见的还是隐藏的")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public new bool Visible
	{
		get
		{
			return base.Visible;
		}
		set
		{
			if (InvokeRequired)
			{
				Invoke(() => base.Visible = value);
			}
			else
			{
				base.Visible = value;
			}
		}
	}

	/// <summary>
	/// 指示是否已启用该控件
	/// </summary>
	[Description("指示是否已启用该控件")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public new bool Enabled
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			if (InvokeRequired)
			{
				Invoke(() => base.Enabled = value);
			}
			else
			{
				base.Enabled = value;
			}
		}
	}

	/// <summary>
	/// 色彩模式
	/// </summary>
	[Description("色彩模式")]
	[Category("Appearance")]
	[DefaultValue(TAMode.Auto)]
	public TAMode ColorScheme
	{
		get
		{
			return colorScheme;
		}
		set
		{
			if (colorScheme != value)
			{
				colorScheme = value;
				OnColorSchemeChanged(EventArgs.Empty);
				if (IsHandleCreated)
				{
					Invalidate();
				}
				OnPropertyChanged("ColorScheme");
			}
		}
	}

	/// <summary>
	/// 徽标内容
	/// </summary>
	[Description("徽标内容")]
	[Category("徽标")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? Badge
	{
		get
		{
			return badge;
		}
		set
		{
			if (!(badge == value))
			{
				badge = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 徽标SVG
	/// </summary>
	[Description("徽标SVG")]
	[Category("徽标")]
	[DefaultValue(null)]
	public string? BadgeSvg
	{
		get
		{
			return badgeSvg;
		}
		set
		{
			if (!(badgeSvg == value))
			{
				badgeSvg = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 徽标方向
	/// </summary>
	[Description("徽标方向")]
	[Category("徽标")]
	[DefaultValue(TAlign.TR)]
	public TAlign BadgeAlign
	{
		get
		{
			return badgeAlign;
		}
		set
		{
			if (badgeAlign != value)
			{
				badgeAlign = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标比例
	/// </summary>
	[Description("徽标比例")]
	[Category("徽标")]
	[DefaultValue(0.6f)]
	public float BadgeSize
	{
		get
		{
			return badgeSize;
		}
		set
		{
			if (badgeSize != value)
			{
				badgeSize = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标模式（镂空）
	/// </summary>
	[Description("徽标模式（镂空）")]
	[Category("徽标")]
	[DefaultValue(false)]
	public bool BadgeMode
	{
		get
		{
			return badgeMode;
		}
		set
		{
			if (badgeMode != value)
			{
				badgeMode = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标前景颜色
	/// </summary>
	[Description("徽标前景颜色")]
	[Category("徽标")]
	[DefaultValue(null)]
	public Color? BadgeFore
	{
		get
		{
			return badgefore;
		}
		set
		{
			if (!(badgefore == value))
			{
				badgefore = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标背景颜色
	/// </summary>
	[Description("徽标背景颜色")]
	[Category("徽标")]
	[DefaultValue(null)]
	public Color? BadgeBack
	{
		get
		{
			return badgeback;
		}
		set
		{
			if (!(badgeback == value))
			{
				badgeback = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标边框颜色
	/// </summary>
	[Description("徽标边框颜色")]
	[Category("徽标")]
	[DefaultValue(null)]
	public Color? BadgeBorderColor
	{
		get
		{
			return badgeBorderColor;
		}
		set
		{
			if (!(badgeBorderColor == value))
			{
				badgeBorderColor = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标边框宽度
	/// </summary>
	[Description("徽标边框宽度")]
	[Category("徽标")]
	[DefaultValue(null)]
	public float? BadgeBorderWidth
	{
		get
		{
			return badgeBorderWidth;
		}
		set
		{
			if (badgeBorderWidth != value)
			{
				badgeBorderWidth = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标偏移X
	/// </summary>
	[Description("徽标偏移X")]
	[Category("徽标")]
	[DefaultValue(1)]
	public int BadgeOffsetX
	{
		get
		{
			return badgeOffsetX;
		}
		set
		{
			if (badgeOffsetX != value)
			{
				badgeOffsetX = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标偏移Y
	/// </summary>
	[Description("徽标偏移Y")]
	[Category("徽标")]
	[DefaultValue(1)]
	public int BadgeOffsetY
	{
		get
		{
			return badgeOffsetY;
		}
		set
		{
			if (badgeOffsetY != value)
			{
				badgeOffsetY = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

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
			Form form = this.FindPARENT();
			if (form is BaseForm { Dpi: var num })
			{
				dpi = num;
				return num;
			}
			if (form is ILayeredForm { Dpi: var num2 })
			{
				dpi = num2;
				return num2;
			}
			float screenDpi = Helper.GetScreenDpi(this);
			dpi = screenDpi;
			return screenDpi;
		}
	}

	[Browsable(false)]
	public virtual GraphicsPath RenderRegion
	{
		get
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddRectangle(ClientRectangle);
			return graphicsPath;
		}
	}

	/// <summary>
	/// 真实区域
	/// </summary>
	[Browsable(false)]
	public virtual Rectangle ReadRectangle => ClientRectangle.PaddingRect(Padding);

	[Description("悬停光标")]
	[Category("光标")]
	[DefaultValue(typeof(Cursor), "Hand")]
	public virtual Cursor HandCursor { get; set; } = Cursors.Hand;

	protected virtual bool CanMouseMove { get; set; }

	/// <summary>
	/// 拖拽文件夹处理
	/// </summary>
	[Description("拖拽文件夹处理")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool HandDragFolder { get; set; } = true;

	/// <summary>
	/// 文件拖拽后时发生
	/// </summary>
	[Description("文件拖拽后时发生")]
	[Category("Behavior")]
	public event DragEventHandler? DragChanged;

	/// <summary>
	/// 渲染 时发生
	/// </summary>
	[Description("渲染 时发生")]
	[Category("Behavior")]
	public event DrawEventHandler? Draw;

	/// <summary>
	/// 渲染背景 时发生
	/// </summary>
	[Description("渲染背景 时发生")]
	[Category("Behavior")]
	public event DrawEventHandler? DrawBg;

	public IControl(ControlType ctype = ControlType.Default)
	{
		switch (ctype)
		{
		case ControlType.Default:
			SetStyle(ControlStyles.ContainerControl | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
			break;
		case ControlType.Select:
			SetStyle(ControlStyles.ContainerControl | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			break;
		case ControlType.Button:
			SetStyle(ControlStyles.ContainerControl | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, value: false);
			break;
		}
		UpdateStyles();
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	protected virtual void OnColorSchemeChanged(EventArgs e)
	{
	}

	/// <summary>
	/// Spin 加载中
	/// </summary>
	/// <param name="action">需要等待的委托</param>
	/// <param name="end">运行结束后的回调</param>
	public Task Spin(Action<Spin.Config> action, Action? end = null)
	{
		return Spin(new Spin.Config(), action, end);
	}

	/// <summary>
	/// Spin 加载中
	/// </summary>
	/// <param name="text">加载文本</param>
	/// <param name="action">需要等待的委托</param>
	/// <param name="end">运行结束后的回调</param>
	public Task Spin(string text, Action<Spin.Config> action, Action? end = null)
	{
		return Spin(new Spin.Config(text), action, end);
	}

	/// <summary>
	/// Spin 加载中
	/// </summary>
	/// <param name="action">需要等待的委托</param>
	/// <param name="token">取消令牌</param>
	/// <param name="end">运行结束后的回调</param>
	public Task Spin(Action<Spin.Config> action, CancellationTokenSource? token, Action? end = null)
	{
		return Spin(new Spin.Config().SetCancel(token), action, end);
	}

	/// <summary>
	/// Spin 加载中
	/// </summary>
	/// <param name="text">加载文本</param>
	/// <param name="action">需要等待的委托</param>
	/// <param name="token">取消令牌</param>
	/// <param name="end">运行结束后的回调</param>
	public Task Spin(string text, Action<Spin.Config> action, CancellationTokenSource? token, Action? end = null)
	{
		return Spin(new Spin.Config(text).SetCancel(token), action, end);
	}

	/// <summary>
	/// Spin 加载中
	/// </summary>
	/// <param name="config">自定义配置</param>
	/// <param name="action">需要等待的委托</param>
	/// <param name="end">运行结束后的回调</param>
	public Task Spin(Spin.Config config, Action<Spin.Config> action, Action? end = null)
	{
		return AntdUI.Spin.open(this, config, action, end);
	}

	/// <summary>
	/// Spin 加载中
	/// </summary>
	/// <param name="action">需要等待的委托</param>
	/// <param name="end">运行结束后的回调</param>
	public Task Spin(Func<Spin.Config, Task> action, Action? end = null)
	{
		return Spin(new Spin.Config(), action, end);
	}

	/// <summary>
	/// Spin 加载中
	/// </summary>
	/// <param name="text">加载文本</param>
	/// <param name="action">需要等待的委托</param>
	/// <param name="end">运行结束后的回调</param>
	public Task Spin(string text, Func<Spin.Config, Task> action, Action? end = null)
	{
		return Spin(new Spin.Config(text), action, end);
	}

	/// <summary>
	/// Spin 加载中
	/// </summary>
	/// <param name="action">需要等待的委托</param>
	/// <param name="token">取消令牌</param>
	/// <param name="end">运行结束后的回调</param>
	public Task Spin(Func<Spin.Config, Task> action, CancellationTokenSource? token, Action? end = null)
	{
		return Spin(new Spin.Config().SetCancel(token), action, end);
	}

	/// <summary>
	/// Spin 加载中
	/// </summary>
	/// <param name="text">加载文本</param>
	/// <param name="action">需要等待的委托</param>
	/// <param name="token">取消令牌</param>
	/// <param name="end">运行结束后的回调</param>
	public Task Spin(string text, Func<Spin.Config, Task> action, CancellationTokenSource? token, Action? end = null)
	{
		return Spin(new Spin.Config(text).SetCancel(token), action, end);
	}

	/// <summary>
	/// Spin 加载中
	/// </summary>
	/// <param name="config">自定义配置</param>
	/// <param name="action">需要等待的委托</param>
	/// <param name="end">运行结束后的回调</param>
	public Task Spin(Spin.Config config, Func<Spin.Config, Task> action, Action? end = null)
	{
		return AntdUI.Spin.open(this, config, action, end);
	}

	internal void IOnSizeChanged()
	{
		if (InvokeRequired)
		{
			Invoke(IOnSizeChanged);
		}
		else
		{
			OnSizeChanged(EventArgs.Empty);
		}
	}

	internal void IOnLayout(LayoutEventArgs levent)
	{
		if (InvokeRequired)
		{
			Invoke(() =>
			{
				IOnLayout(levent);
			});
		}
		else
		{
			OnLayout(levent);
		}
	}

	public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		if (disableDataBinding)
		{
			return;
		}
		try
		{
			foreach (Binding dataBinding in DataBindings)
			{
				if (dataBinding.PropertyName == propertyName)
				{
					dataBinding.WriteValue();
					break;
				}
			}
		}
		catch (NotSupportedException)
		{
			disableDataBinding = true;
		}
	}

	public void SetCursor(bool val)
	{
		SetCursor(val ? CursorType.Hand : CursorType.Default);
	}

	public void SetCursor(CursorType cursor = CursorType.Default)
	{
		if (oldcursor != cursor)
		{
			oldcursor = cursor;
			bool canHandMessage = true;
			switch (cursor)
			{
			case CursorType.Hand:
				SetCursor(HandCursor);
				break;
			case CursorType.IBeam:
				SetCursor(Cursors.IBeam);
				break;
			case CursorType.No:
				SetCursor(Cursors.No);
				break;
			case CursorType.SizeAll:
				canHandMessage = false;
				SetCursor(Cursors.SizeAll);
				break;
			case CursorType.VSplit:
				canHandMessage = false;
				SetCursor(Cursors.VSplit);
				break;
			case CursorType.HSplit:
				canHandMessage = false;
				SetCursor(Cursors.HSplit);
				break;
			default:
				SetCursor(DefaultCursor);
				break;
			}
			Window.CanHandMessage = canHandMessage;
		}
	}

	private void SetCursor(Cursor cursor)
	{
		if (InvokeRequired)
		{
			Invoke(() =>
			{
				SetCursor(cursor);
			});
		}
		else
		{
			Cursor = cursor;
		}
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

	internal bool CanProcessMnemonicBefore(char charCode, string? text)
	{
		if (Enabled && Visible && Control.IsMnemonic(charCode, text))
		{
			return true;
		}
		return false;
	}

	internal bool CanProcessMnemonicAfter()
	{
		Form form = FindForm();
		if (form == null)
		{
			return true;
		}
		return CanProcessMnemonicActiveControl(form.ActiveControl);
	}

	internal static bool CanProcessMnemonicActiveControl(Control? control)
	{
		if (control is Input input)
		{
			return input.hasAlt;
		}
		if (control is UserControl userControl)
		{
			return CanProcessMnemonicActiveControl(userControl.ActiveControl);
		}
		return true;
	}

	protected virtual void OnTouchDown(int x, int y)
	{
		oldMY = 0;
		oldX = (mouseX = x);
		oldY = (mouseY = y);
		if (Config.TouchEnabled)
		{
			taskTouch?.Dispose();
			taskTouch = null;
			mdownd = 0;
			mdown = true;
		}
	}

	protected virtual bool OnTouchMove(int x, int y)
	{
		if (mdown)
		{
			int num = oldX - x;
			int num2 = oldY - y;
			int num3 = Math.Abs(num);
			int num4 = Math.Abs(num2);
			int num5 = (int)((float)Config.TouchThreshold * Dpi);
			if (mdownd > 0 || num3 > num5 || num4 > num5)
			{
				oldMY = num2;
				if (mdownd > 0)
				{
					if (mdownd == 1)
					{
						OnTouchScrollY(mouseX, mouseY, -num2);
					}
					else
					{
						OnTouchScrollX(mouseX, mouseY, -num);
					}
					oldX = x;
					oldY = y;
					return false;
				}
				if (num4 > num3)
				{
					mdownd = 1;
				}
				else
				{
					mdownd = 2;
				}
				oldX = x;
				oldY = y;
				return false;
			}
		}
		return true;
	}

	protected virtual bool OnTouchUp()
	{
		taskTouch?.Dispose();
		taskTouch = null;
		mdown = false;
		if (mdownd > 0)
		{
			if (mdownd == 1)
			{
				int num = oldMY;
				int moveYa = Math.Abs(num);
				int num2 = (int)((float)Config.TouchThreshold * Dpi);
				if (moveYa > num2)
				{
					int duration = (int)Math.Ceiling((float)moveYa * 0.1f);
					int incremental = moveYa / 2;
					int interval = 20;
					if (num > 0)
					{
						taskTouch = new AnimationTask(new AnimationLoopConfig(this, () =>
						{
							if (moveYa > 0 && OnTouchScrollY(mouseX, mouseY, -incremental))
							{
								moveYa -= duration;
								return true;
							}
							return false;
						}, interval).SetPriority());
					}
					else
					{
						taskTouch = new AnimationTask(new AnimationLoopConfig(this, () =>
						{
							if (moveYa > 0 && OnTouchScrollY(mouseX, mouseY, incremental))
							{
								moveYa -= duration;
								return true;
							}
							return false;
						}, interval).SetPriority());
					}
				}
			}
			return false;
		}
		return true;
	}

	protected void OnTouchCancel()
	{
		taskTouch?.Dispose();
		taskTouch = null;
		mdown = false;
	}

	protected virtual bool OnTouchScrollX(int x, int y, int value)
	{
		return false;
	}

	protected virtual bool OnTouchScrollY(int x, int y, int value)
	{
		return false;
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		taskTouch?.Dispose();
		taskTouch = null;
		base.OnMouseWheel(e);
	}

	protected override void WndProc(ref System.Windows.Forms.Message m)
	{
		try
		{
			if (OS.Win7OrLower && m.Msg == 513)
			{
				Select();
				base.WndProc(ref m);
				return;
			}
			base.WndProc(ref m);
			if (Config.TouchClickEnabled)
			{
				switch ((Win32.User32.WindowMessage)m.Msg)
				{
				case Win32.User32.WindowMessage.WM_POINTERDOWN:
					Win32.User32.PostMessage(m.HWnd, 513u, m.WParam, m.LParam);
					break;
				case Win32.User32.WindowMessage.WM_POINTERUP:
					Win32.User32.PostMessage(m.HWnd, 514u, m.WParam, m.LParam);
					break;
				}
			}
		}
		catch
		{
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (!CanMouseMove || (oldx == e.X && oldy == e.Y))
		{
			return;
		}
		oldx = e.X;
		oldy = e.Y;
		if (tmpHover)
		{
			tmpHover = OnMouseHover(oldx, oldy);
		}
		hoverStopwatch.Restart();
		timeHover = hoverStopwatch.Elapsed + TimeSpan.FromMilliseconds(Config.MouseHoverDelay);
		if (taskHover != null)
		{
			return;
		}
		taskHover = new AnimationTask(new AnimationLoopConfig(this, () =>
		{
			if (hoverStopwatch.Elapsed < timeHover)
			{
				return true;
			}
			tmpHover = Invoke(() => OnMouseHover(oldx, oldy));
			return false;
		}, Config.MouseHoverDelay).SetEnd(() =>
		{
			taskHover = null;
		}).SetSleep(Config.MouseHoverDelay).SetPriority());
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		taskHover?.Dispose();
		taskHover = null;
		hoverStopwatch.Reset();
		oldx = -1;
		oldy = -1;
	}

	protected virtual bool OnMouseHover(int x, int y)
	{
		return false;
	}

	protected virtual void OnDragEnter()
	{
	}

	protected virtual void OnDragLeave()
	{
	}

	/// <summary>
	/// 使用管理员权限拖拽上传
	/// </summary>
	public void UseAdmin()
	{
		if (fileDrop == null)
		{
			fileDrop = new FileDropHandler(this);
		}
	}

	protected override void OnDragEnter(DragEventArgs e)
	{
		base.OnDragEnter(e);
		if (AllowDrop)
		{
			OnDragEnter();
			if (DragState(e.Data))
			{
				e.Effect = DragDropEffects.All;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}
	}

	protected override void OnDragLeave(EventArgs e)
	{
		base.OnDragLeave(e);
		OnDragLeave();
	}

	protected override void OnDragDrop(DragEventArgs e)
	{
		base.OnDragDrop(e);
		if (DragData(e.Data, out string[] files))
		{
			if (ONDRAG == null)
			{
				OnDragChanged(files);
			}
			else
			{
				string[] array = ONDRAG(files);
				if (array != null)
				{
					OnDragChanged(array);
				}
			}
		}
		OnDragLeave();
	}

	private bool DragState(IDataObject? Data)
	{
		if (DragData(Data, out string[] files))
		{
			if (ONDRAG == null)
			{
				return true;
			}
			if (ONDRAG(files) == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private bool DragData(IDataObject? Data, out string[] files)
	{
		if (Data == null)
		{
			files = new string[0];
			return false;
		}
		string[] formats = Data.GetFormats();
		foreach (string format in formats)
		{
			if (!(Data.GetData(format) is string[] array) || array.Length == 0)
			{
				continue;
			}
			if (HandDragFolder)
			{
				List<string> list = new List<string>(array.Length);
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (File.Exists(text))
					{
						list.Add(text);
					}
					else
					{
						list.AddRange(DragDataDirTree(text));
					}
				}
				files = list.ToArray();
			}
			else
			{
				files = array;
			}
			return true;
		}
		files = new string[0];
		return false;
	}

	private static List<string> DragDataDirTree(string dir)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(dir);
		FileInfo[] files = directoryInfo.GetFiles();
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		List<string> list = new List<string>(files.Length + directories.Length);
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			list.Add(fileInfo.FullName);
		}
		DirectoryInfo[] array2 = directories;
		foreach (DirectoryInfo directoryInfo2 in array2)
		{
			list.AddRange(DragDataDirTree(directoryInfo2.FullName));
		}
		return list;
	}

	protected virtual void OnDragChanged(string[] files)
	{
		DragChanged?.Invoke(this, new StringsEventArgs(files));
	}

	protected override void Dispose(bool disposing)
	{
		Window.CanHandMessage = true;
		fileDrop?.Dispose();
		base.Dispose(disposing);
	}

	protected virtual void OnDrawBg(DrawEventArgs e)
	{
		DrawBg?.Invoke(this, e);
	}

	protected virtual void OnDraw(DrawEventArgs e)
	{
		Draw?.Invoke(this, e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Rectangle clientRectangle = ClientRectangle;
		if (clientRectangle.Width != 0 && clientRectangle.Height != 0)
		{
			base.OnPaint(e);
			Canvas canvas = e.Graphics.High(Dpi);
			try
			{
				DrawEventArgs e2 = new DrawEventArgs(canvas, clientRectangle);
				OnDrawBg(e2);
				OnDraw(e2);
			}
			catch
			{
			}
			this.PaintBadge(canvas);
		}
	}

	public Bitmap? DrawBitmap()
	{
		Rectangle clientRectangle = ClientRectangle;
		if (clientRectangle.Width == 0 || clientRectangle.Height == 0)
		{
			return null;
		}
		Bitmap bitmap = new Bitmap(clientRectangle.Width, clientRectangle.Height);
		using Canvas canvas = Graphics.FromImage(bitmap).High(Dpi);
		try
		{
			DrawEventArgs e = new DrawEventArgs(canvas, clientRectangle);
			OnDrawBg(e);
			OnDraw(e);
		}
		catch
		{
		}
		this.PaintBadge(canvas);
		return bitmap;
	}
}
