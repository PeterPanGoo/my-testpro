using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Panel 面板
/// </summary>
/// <remarks>内容区域。</remarks>
[Description("Panel 面板")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
[Designer(typeof(IControlDesigner))]
public class Panel : IControl, ShadowConfig, IMessageFilter, IEventListener
{
	internal class PanelCore : ScrollableControl
	{
		public Action LoadScroll;

		public PanelCore(Action loadScroll)
		{
			LoadScroll = loadScroll;
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			UpdateStyles();
		}

		protected override void WndProc(ref System.Windows.Forms.Message m)
		{
			switch ((Win32.User32.WindowMessage)m.Msg)
			{
			case Win32.User32.WindowMessage.WM_SIZE:
			case Win32.User32.WindowMessage.WM_PAINT:
			case Win32.User32.WindowMessage.WM_MOUSEWHEEL:
				Win32.User32.HideScrollBar(Handle);
				LoadScroll();
				break;
			case Win32.User32.WindowMessage.WM_NCCALCSIZE:
				Win32.User32.HideScrollBar(Handle);
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
	}

	private int radius = 6;

	private TAlignRound radiusAlign = TAlignRound.ALL;

	private Padding _padding = new Padding(0);

	private int shadow;

	private Color? shadowColor;

	private int shadowOffsetX;

	private int shadowOffsetY;

	private float shadowOpacity = 0.1f;

	private bool shadowOpacityAnimation;

	private float shadowOpacityHover = 0.3f;

	private TAlignMini shadowAlign;

	private Color? back;

	private string? backExtend;

	private Image? backImage;

	private TFit backFit;

	private int arrwoSize = 8;

	private TAlign arrowAlign;

	private float borderWidth;

	private Color? borderColor;

	private DashStyle borderStyle;

	private AsyncShadow shadow_temp;

	private float AnimationHoverValue = 0.1f;

	private bool AnimationHover;

	private bool _mouseHover;

	private AnimationTask? ThreadHover;

	private bool autoscroll;

	private PanelCore? IPanel;

	public XScrollBar? XScroll;

	public YScrollBar? YScroll;

	/// <summary>
	/// 圆角
	/// </summary>
	[Description("圆角")]
	[Category("Appearance")]
	[DefaultValue(6)]
	public int Radius
	{
		get
		{
			return radius;
		}
		set
		{
			if (radius != value)
			{
				radius = value;
				if (XScroll != null)
				{
					XScroll.Radius = value;
				}
				if (YScroll != null)
				{
					YScroll.Radius = value;
				}
				Invalidate();
				OnPropertyChanged("Radius");
			}
		}
	}

	/// <summary>
	/// 圆角方向
	/// </summary>
	[Description("圆角方向")]
	[Category("Appearance")]
	[DefaultValue(TAlignRound.ALL)]
	public TAlignRound RadiusAlign
	{
		get
		{
			return radiusAlign;
		}
		set
		{
			if (radiusAlign != value)
			{
				radiusAlign = value;
				Invalidate();
				OnPropertyChanged("RadiusAlign");
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("use InnerPadding")]
	[Description("内边距")]
	[Category("Appearance")]
	[DefaultValue(typeof(Padding), "0, 0, 0, 0")]
	public Padding padding
	{
		get
		{
			return InnerPadding;
		}
		set
		{
			InnerPadding = value;
		}
	}

	/// <summary>
	/// 内边距
	/// </summary>
	[Description("内边距")]
	[Category("Appearance")]
	[DefaultValue(typeof(Padding), "0, 0, 0, 0")]
	public Padding InnerPadding
	{
		get
		{
			return _padding;
		}
		set
		{
			if (!(_padding == value))
			{
				_padding = value;
				shadow_temp.Change();
				IOnSizeChanged();
				OnPropertyChanged("InnerPadding");
			}
		}
	}

	/// <summary>
	/// 阴影大小
	/// </summary>
	[Description("阴影")]
	[Category("Appearance")]
	[DefaultValue(0)]
	public int Shadow
	{
		get
		{
			return shadow;
		}
		set
		{
			if (shadow != value)
			{
				shadow = value;
				shadow_temp.Change();
				IOnSizeChanged();
				OnPropertyChanged("Shadow");
			}
		}
	}

	/// <summary>
	/// 阴影颜色
	/// </summary>
	[Description("阴影颜色")]
	[Category("阴影")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ShadowColor
	{
		get
		{
			return shadowColor;
		}
		set
		{
			if (!(shadowColor == value))
			{
				shadowColor = value;
				shadow_temp.Change();
				Invalidate();
				OnPropertyChanged("ShadowColor");
			}
		}
	}

	/// <summary>
	/// 阴影偏移X
	/// </summary>
	[Description("阴影偏移X")]
	[Category("阴影")]
	[DefaultValue(0)]
	public int ShadowOffsetX
	{
		get
		{
			return shadowOffsetX;
		}
		set
		{
			if (shadowOffsetX != value)
			{
				shadowOffsetX = value;
				shadow_temp.Change();
				IOnSizeChanged();
				OnPropertyChanged("ShadowOffsetX");
			}
		}
	}

	/// <summary>
	/// 阴影偏移Y
	/// </summary>
	[Description("阴影偏移Y")]
	[Category("阴影")]
	[DefaultValue(0)]
	public int ShadowOffsetY
	{
		get
		{
			return shadowOffsetY;
		}
		set
		{
			if (shadowOffsetY != value)
			{
				shadowOffsetY = value;
				shadow_temp.Change();
				IOnSizeChanged();
				OnPropertyChanged("ShadowOffsetY");
			}
		}
	}

	/// <summary>
	/// 阴影透明度
	/// </summary>
	[Description("阴影透明度")]
	[Category("阴影")]
	[DefaultValue(0.1f)]
	public float ShadowOpacity
	{
		get
		{
			return shadowOpacity;
		}
		set
		{
			if (shadowOpacity != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				else if (value > 1f)
				{
					value = 1f;
				}
				shadowOpacity = value;
				AnimationHoverValue = shadowOpacity;
				Invalidate();
				OnPropertyChanged("ShadowOpacity");
			}
		}
	}

	/// <summary>
	/// 阴影透明度动画使能
	/// </summary>
	[Description("阴影透明度动画使能")]
	[Category("阴影")]
	[DefaultValue(false)]
	public bool ShadowOpacityAnimation
	{
		get
		{
			return shadowOpacityAnimation;
		}
		set
		{
			if (shadowOpacityAnimation == value)
			{
				return;
			}
			shadowOpacityAnimation = value;
			if (shadowOpacityAnimation)
			{
				if (IsHandleCreated)
				{
					Application.AddMessageFilter(this);
				}
			}
			else
			{
				Application.RemoveMessageFilter(this);
			}
		}
	}

	/// <summary>
	/// 悬停阴影后透明度
	/// </summary>
	[Description("悬停阴影后透明度")]
	[Category("阴影")]
	[DefaultValue(0.3f)]
	public float ShadowOpacityHover
	{
		get
		{
			return shadowOpacityHover;
		}
		set
		{
			if (shadowOpacityHover != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				else if (value > 1f)
				{
					value = 1f;
				}
				shadowOpacityHover = value;
				Invalidate();
				OnPropertyChanged("ShadowOpacityHover");
			}
		}
	}

	/// <summary>
	/// 阴影方向
	/// </summary>
	[Description("阴影方向")]
	[Category("阴影")]
	[DefaultValue(TAlignMini.None)]
	public TAlignMini ShadowAlign
	{
		get
		{
			return shadowAlign;
		}
		set
		{
			if (shadowAlign != value)
			{
				shadowAlign = value;
				shadow_temp.Change();
				IOnSizeChanged();
				OnPropertyChanged("ShadowAlign");
			}
		}
	}

	/// <summary>
	/// 阴影延迟毫秒
	/// </summary>
	[Description("阴影延迟毫秒")]
	[Category("阴影")]
	[DefaultValue(100)]
	public int ShadowDelay { get; set; } = 100;

	/// <summary>
	/// 背景颜色
	/// </summary>
	[Description("背景颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Back
	{
		get
		{
			return back;
		}
		set
		{
			if (!(back == value))
			{
				back = value;
				Invalidate();
				OnPropertyChanged("Back");
			}
		}
	}

	/// <summary>
	/// 背景渐变色
	/// </summary>
	[Description("背景渐变色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? BackExtend
	{
		get
		{
			return backExtend;
		}
		set
		{
			if (!(backExtend == value))
			{
				backExtend = value;
				Invalidate();
				OnPropertyChanged("BackExtend");
			}
		}
	}

	/// <summary>
	/// 背景图片
	/// </summary>
	[Description("背景图片")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public new Image? BackgroundImage
	{
		get
		{
			return backImage;
		}
		set
		{
			if (backImage != value)
			{
				backImage = value;
				Invalidate();
				OnPropertyChanged("BackgroundImage");
			}
		}
	}

	/// <summary>
	/// 背景图片布局
	/// </summary>
	[Description("背景图片布局")]
	[Category("Appearance")]
	[DefaultValue(TFit.Fill)]
	public new TFit BackgroundImageLayout
	{
		get
		{
			return backFit;
		}
		set
		{
			if (backFit != value)
			{
				backFit = value;
				Invalidate();
				OnPropertyChanged("BackgroundImageLayout");
			}
		}
	}

	/// <summary>
	/// 箭头大小
	/// </summary>
	[Description("箭头大小")]
	[Category("箭头")]
	[DefaultValue(8)]
	public int ArrowSize
	{
		get
		{
			return arrwoSize;
		}
		set
		{
			if (arrwoSize != value)
			{
				arrwoSize = value;
				Invalidate();
				OnPropertyChanged("ArrowSize");
			}
		}
	}

	/// <summary>
	/// 箭头方向
	/// </summary>
	[Description("箭头方向")]
	[Category("箭头")]
	[DefaultValue(TAlign.None)]
	public TAlign ArrowAlign
	{
		get
		{
			return arrowAlign;
		}
		set
		{
			if (arrowAlign != value)
			{
				arrowAlign = value;
				Invalidate();
				OnPropertyChanged("ArrowAlign");
			}
		}
	}

	/// <summary>
	/// 边框宽度
	/// </summary>
	[Description("边框宽度")]
	[Category("边框")]
	[DefaultValue(0f)]
	public float BorderWidth
	{
		get
		{
			return borderWidth;
		}
		set
		{
			if (borderWidth != value)
			{
				borderWidth = value;
				IOnSizeChanged();
				OnPropertyChanged("BorderWidth");
			}
		}
	}

	/// <summary>
	/// 边框颜色
	/// </summary>
	[Description("边框颜色")]
	[Category("边框")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			if (!(borderColor == value))
			{
				borderColor = value;
				if (borderWidth > 0f)
				{
					Invalidate();
				}
				OnPropertyChanged("BorderColor");
			}
		}
	}

	/// <summary>
	/// 边框样式
	/// </summary>
	[Description("边框样式")]
	[Category("边框")]
	[DefaultValue(DashStyle.Solid)]
	public DashStyle BorderStyle
	{
		get
		{
			return borderStyle;
		}
		set
		{
			if (borderStyle != value)
			{
				borderStyle = value;
				if (borderWidth > 0f)
				{
					Invalidate();
				}
				OnPropertyChanged("BorderStyle");
			}
		}
	}

	public override Rectangle DisplayRectangle => ClientRectangle.DeflateRect(Padding, this, shadowAlign, borderWidth);

	public override Rectangle ReadRectangle => ClientRectangle.DeflateRect(_padding, Dpi).PaddingRect(this, shadowAlign, borderWidth / 2f);

	public override GraphicsPath RenderRegion => ReadRectangle.RoundPath((float)radius * Dpi);

	private bool ExtraMouseHover
	{
		get
		{
			return _mouseHover;
		}
		set
		{
			if (_mouseHover == value || !Enabled || !ShadowOpacityAnimation || shadow <= 0 || !(shadowOpacityHover > 0f) || !(shadowOpacityHover > shadowOpacity))
			{
				return;
			}
			_mouseHover = value;
			if (Config.HasAnimation("Panel", Name))
			{
				ThreadHover?.Dispose();
				AnimationHover = true;
				float num = shadowOpacityHover / 12f;
				if (value)
				{
					ThreadHover = new AnimationTask(new AnimationLinearFConfig(this, (float i) =>
					{
						AnimationHoverValue = i;
						Invalidate();
						return true;
					}, 20).SetAdd(num).SetMax(shadowOpacityHover).SetValue(AnimationHoverValue)
						.SetEnd(() =>
						{
							AnimationHover = false;
							Invalidate();
						}));
				}
				else
				{
					ThreadHover = new AnimationTask(new AnimationLinearFConfig(this, (float i) =>
					{
						AnimationHoverValue = i;
						Invalidate();
						return true;
					}, 20).SetAdd(0f - num).SetMax(shadowOpacity).SetValue(AnimationHoverValue)
						.SetEnd(() =>
						{
							AnimationHover = false;
							Invalidate();
						}));
				}
			}
			else
			{
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 内部容器背景透明
	/// </summary>
	[Description("内部容器背景透明")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool AutoContainerBgTransparent { get; set; }

	/// <summary>
	/// 是否显示滚动条
	/// </summary>
	[Description("是否显示滚动条")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool AutoScroll
	{
		get
		{
			return autoscroll;
		}
		set
		{
			if (autoscroll != value)
			{
				autoscroll = value;
				if (IsHandleCreated)
				{
					InitScroll();
				}
				OnPropertyChanged("AutoScroll");
			}
		}
	}

	[Browsable(false)]
	public new ControlCollection Controls
	{
		get
		{
			if (IPanel == null)
			{
				return base.Controls;
			}
			return IPanel.Controls;
		}
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		Canvas canvas = e.Canvas;
		Rectangle readRectangle = ReadRectangle;
		if (readRectangle.Width > 0 && readRectangle.Height > 0)
		{
			float num = (float)radius * Dpi;
			Color color = back ?? Colour.BgContainer.Get(ColorScheme, "Panel", Name);
			using (GraphicsPath path = DrawShadow(canvas, num, e.Rect, readRectangle))
			{
				using (Brush brush = backExtend.BrushEx(readRectangle, color))
				{
					canvas.Fill(brush, path);
				}
				if (backImage != null)
				{
					canvas.Image(readRectangle, backImage, backFit, num, round: false);
				}
				if (borderWidth > 0f)
				{
					canvas.Draw(borderColor ?? Colour.BorderColor.Get(ColorScheme, "Panel", Name), borderWidth * Dpi, borderStyle, path);
				}
			}
			if (ArrowAlign != TAlign.None)
			{
				canvas.FillPolygon(color, ArrowAlign.AlignLines(ArrowSize, e.Rect, readRectangle));
			}
		}
		base.OnDraw(e);
	}

	public Panel()
	{
		shadow_temp = new AsyncShadow(this, (Rectangle rect) => rect.RoundPath(radius, shadowAlign, radiusAlign));
	}

	/// <summary>
	/// 绘制阴影
	/// </summary>
	/// <param name="g">GDI</param>
	/// <param name="rect_client">客户区域</param>
	/// <param name="rect_read">真实区域</param>
	private GraphicsPath DrawShadow(Canvas g, float radius, Rectangle rect_client, Rectangle rect_read)
	{
		GraphicsPath result = rect_read.RoundPath(radius, shadowAlign, radiusAlign);
		if (shadow > 0)
		{
			float opacity;
			if (AnimationHover)
			{
				opacity = AnimationHoverValue;
			}
			else
			{
				opacity = ((!ExtraMouseHover) ? shadowOpacity : shadowOpacityHover);
			}
			shadow_temp.Draw(g, rect_client, rect_read, Dpi, shadow, ShadowOffsetX, ShadowOffsetY, shadowColor ?? Colour.TextBase.Get(ColorScheme, "Panel", Name), opacity);
		}
		return result;
	}

	public void SetMouseHover(bool val)
	{
		ExtraMouseHover = val;
	}

	protected override void Dispose(bool disposing)
	{
		ThreadHover?.Dispose();
		ThreadHover = null;
		ShadowOpacityAnimation = false;
		shadow_temp.Dispose();
		base.Dispose(disposing);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		ExtraMouseHover = true;
	}

	public bool PreFilterMessage(ref System.Windows.Forms.Message m)
	{
		if (m.Msg == 673 || m.Msg == 675)
		{
			ExtraMouseHover = ClientRectangle.Contains(PointToClient(Control.MousePosition));
			return false;
		}
		return false;
	}

	protected override void OnLeave(EventArgs e)
	{
		base.OnLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		this.AddListener();
		if (ShadowOpacityAnimation)
		{
			Application.AddMessageFilter(this);
		}
		if (autoscroll)
		{
			InitScroll();
		}
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.THEME)
		{
			shadow_temp.Change();
			if (IPanel != null)
			{
				InitScrollBg(IPanel);
			}
			if (XScroll != null)
			{
				InitScrollBg(XScroll);
			}
			if (YScroll != null)
			{
				InitScrollBg(YScroll);
			}
		}
	}

	private void InitScroll()
	{
		if (autoscroll)
		{
			if (DesignMode)
			{
				return;
			}
			if (IPanel == null)
			{
				IPanel = new PanelCore(ScrollInfo)
				{
					Name = "__IN__",
					Dock = DockStyle.Fill,
					AutoScroll = true,
					Bounds = DisplayRectangle
				};
				InitScrollBg(IPanel);
				List<Control> list = new List<Control>(IPanel.Controls.Count);
				foreach (Control control in base.Controls)
				{
					if (!(control.Name == "__IN__") && !(control.Name == "__BARX__") && !(control.Name == "__BARY__"))
					{
						list.Add(control);
					}
				}
				if (list.Count > 0)
				{
					IPanel.Controls.AddRange(list.ToArray());
				}
				base.Controls.Add(IPanel);
			}
			if (XScroll == null)
			{
				int horizontalScrollBarHeight = SystemInformation.HorizontalScrollBarHeight;
				int num = IPanel.Width;
				XScroll = new XScrollBar
				{
					Name = "__BARX__",
					Dock = DockStyle.Bottom,
					MinimumSize = new Size(0, horizontalScrollBarHeight),
					Size = new Size(num, horizontalScrollBarHeight),
					Radius = Radius
				};
				InitScrollBg(XScroll);
				XScroll.ValueChanged += ScrollX_ValueChanged;
				base.Controls.Add(XScroll);
			}
			if (YScroll == null)
			{
				int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
				int num2 = IPanel.Height;
				YScroll = new YScrollBar
				{
					Name = "__BARY__",
					Dock = DockStyle.Right,
					MinimumSize = new Size(verticalScrollBarWidth, 0),
					Size = new Size(verticalScrollBarWidth, num2),
					Radius = Radius
				};
				InitScrollBg(YScroll);
				YScroll.ShowChanged += ScrollY_ShowChanged;
				YScroll.ValueChanged += ScrollY_ValueChanged;
				base.Controls.Add(YScroll);
			}
			return;
		}
		if (IPanel != null)
		{
			if (IPanel.Controls.Count > 0)
			{
				List<Control> list2 = new List<Control>(IPanel.Controls.Count);
				foreach (Control control2 in IPanel.Controls)
				{
					list2.Add(control2);
				}
				base.Controls.AddRange(list2.ToArray());
			}
			IPanel?.Dispose();
			IPanel = null;
		}
		if (YScroll != null)
		{
			YScroll.ValueChanged -= ScrollY_ValueChanged;
			YScroll.ShowChanged -= ScrollY_ShowChanged;
			YScroll.Dispose();
			YScroll = null;
		}
		if (XScroll != null)
		{
			XScroll.ValueChanged -= ScrollX_ValueChanged;
			XScroll.Dispose();
			XScroll = null;
		}
	}

	private void InitScrollBg(Control control)
	{
		if (AutoContainerBgTransparent)
		{
			control.BackColor = Color.Transparent;
		}
		else
		{
			control.BackColor = back ?? Colour.BgContainer.Get(ColorScheme, "Panel", Name);
		}
	}

	private void ScrollY_ShowChanged(object? sender, BoolEventArgs e)
	{
		if (XScroll != null && YScroll != null)
		{
			XScroll.RadiusRB = !e.Value;
		}
	}

	private void ScrollX_ValueChanged(object? sender, IntEventArgs e)
	{
		if (IPanel != null && XScroll != null)
		{
			XScroll.SetValue(IPanel.HorizontalScroll);
		}
	}

	private void ScrollY_ValueChanged(object? sender, IntEventArgs e)
	{
		if (IPanel != null && YScroll != null)
		{
			YScroll.SetValue(IPanel.VerticalScroll);
		}
	}

	private void ScrollInfo()
	{
		if (IPanel != null && YScroll != null && XScroll != null)
		{
			YScroll.LoadValue(IPanel.VerticalScroll);
			XScroll.LoadValue(IPanel.HorizontalScroll);
		}
	}

	protected override void OnControlAdded(ControlEventArgs e)
	{
		if (e.Control != null)
		{
			if (IPanel == null || e.Control.Name == "__IN__" || e.Control.Name == "__BARX__" || e.Control.Name == "__BARY__")
			{
				base.OnControlAdded(e);
			}
			else if (DesignMode)
			{
				base.OnControlAdded(e);
			}
			else
			{
				IPanel.Controls.Add(e.Control);
			}
		}
	}

	public void Add(Control control)
	{
		Controls.Add(control);
	}

	public void ScrollControlIntoView(Control? activeControl)
	{
		IPanel?.ScrollControlIntoView(activeControl);
	}

	protected override void OnContextMenuStripChanged(EventArgs e)
	{
		base.OnContextMenuStripChanged(e);
		if (IPanel != null)
		{
			IPanel.ContextMenuStrip = ContextMenuStrip;
		}
	}
}
