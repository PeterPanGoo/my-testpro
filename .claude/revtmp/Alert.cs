using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

/// <summary>
/// Alert 警告提示
/// </summary>
/// <remarks>警告提示，展现需要关注的信息。</remarks>
[Description("Alert 警告提示")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
[Designer(typeof(IControlDesigner))]
public class Alert : IControl, IEventListener
{
	private int radius = 6;

	private float borWidth;

	private string? text;

	private ContentAlignment textAlign = ContentAlignment.MiddleLeft;

	private string? textTitle;

	private Color? textColor;

	private Color? textTitleColor;

	private TType icon;

	private string? iconSvg;

	private Image? iconCustom;

	private bool closeIcon;

	private float? iconratio;

	private float? icongap;

	private bool loop;

	private bool loopState;

	private int loopSpeed = 10;

	private AnimationTask? task;

	private int val;

	private Size? font_size;

	private ITaskOpacity hover_close;

	private RectangleF rect_close;

	private readonly FormatFlags sEllipsis = FormatFlags.Top | FormatFlags.Left | FormatFlags.EllipsisCharacter;

	private FormatFlags sc = FormatFlags.Center | FormatFlags.NoWrap;

	private FormatFlags sf = FormatFlags.NoWrapEllipsis | FormatFlags.VerticalCenter | FormatFlags.Left;

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
				Invalidate();
				OnPropertyChanged("Radius");
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
			return borWidth;
		}
		set
		{
			if (borWidth != value)
			{
				borWidth = value;
				Invalidate();
				OnPropertyChanged("BorderWidth");
			}
		}
	}

	/// <summary>
	/// 文本
	/// </summary>
	[Description("文本")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public override string? Text
	{
		get
		{
			return this.GetLangI(LocalizationText, text);
		}
		set
		{
			if (!(text == value))
			{
				if (loop && string.IsNullOrEmpty(value))
				{
					task?.Dispose();
					task = null;
				}
				text = value;
				ClearFont();
				OnTextChanged(EventArgs.Empty);
				OnPropertyChanged("Text");
				Invalidate();
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	/// <summary>
	/// 文本位置
	/// </summary>
	[Description("文本位置")]
	[Category("Appearance")]
	[DefaultValue(ContentAlignment.MiddleLeft)]
	public ContentAlignment TextAlign
	{
		get
		{
			return textAlign;
		}
		set
		{
			if (textAlign != value)
			{
				textAlign = value;
				sf = textAlign.SetAlignment(sf);
				sc = Helper.SetVerticalAlignment(sf, sc);
				Invalidate();
				OnPropertyChanged("TextAlign");
			}
		}
	}

	/// <summary>
	/// 标题
	/// </summary>
	[Description("标题")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? TextTitle
	{
		get
		{
			return this.GetLangI(LocalizationTextTitle, textTitle);
		}
		set
		{
			if (!(textTitle == value))
			{
				textTitle = value;
				Invalidate();
				OnPropertyChanged("TextTitle");
			}
		}
	}

	[Description("标题")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationTextTitle { get; set; }

	/// <summary>
	/// 文本的自定义颜色
	/// </summary>
	[Description("文本的自定义颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? TextColor
	{
		get
		{
			return textColor;
		}
		set
		{
			if (!(textColor == value))
			{
				textColor = value;
				Invalidate();
				OnPropertyChanged("TextColor");
			}
		}
	}

	/// <summary>
	/// 标题的自定义颜色
	/// </summary>
	[Description("标题的自定义颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? TextTitleColor
	{
		get
		{
			return textTitleColor;
		}
		set
		{
			if (!(textTitleColor == value))
			{
				textTitleColor = value;
				Invalidate();
				OnPropertyChanged("TextTitleColor");
			}
		}
	}

	/// <summary>
	/// 样式
	/// </summary>
	[Description("样式")]
	[Category("Appearance")]
	[DefaultValue(TType.None)]
	public TType Icon
	{
		get
		{
			return icon;
		}
		set
		{
			if (icon != value)
			{
				icon = value;
				Invalidate();
				OnPropertyChanged("Icon");
			}
		}
	}

	/// <summary>
	/// 自定义图标SVG
	/// </summary>
	[Description("自定义图标SVG")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? IconSvg
	{
		get
		{
			return iconSvg;
		}
		set
		{
			if (!(iconSvg == value))
			{
				iconSvg = value;
				Invalidate();
				OnPropertyChanged("IconSvg");
			}
		}
	}

	/// <summary>
	/// 自定义图标
	/// </summary>
	[Description("自定义图标")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Image? IconCustom
	{
		get
		{
			return iconCustom;
		}
		set
		{
			if (iconCustom != value)
			{
				iconCustom = value;
				Invalidate();
				OnPropertyChanged("IconCustom");
			}
		}
	}

	/// <summary>
	/// 是否显示关闭图标
	/// </summary>
	[Description("是否显示关闭图标")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool CloseIcon
	{
		get
		{
			return closeIcon;
		}
		set
		{
			if (closeIcon != value)
			{
				closeIcon = value;
				Invalidate();
				OnPropertyChanged("CloseIcon");
			}
		}
	}

	/// <summary>
	/// 图标比例
	/// </summary>
	[Description("图标比例")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public float? IconRatio
	{
		get
		{
			return iconratio;
		}
		set
		{
			if (iconratio != value)
			{
				iconratio = value;
				Invalidate();
				OnPropertyChanged("IconRatio");
			}
		}
	}

	/// <summary>
	/// 图标与文字间距比例
	/// </summary>
	[Description("图标与文字间距比例")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public float? IconGap
	{
		get
		{
			return icongap;
		}
		set
		{
			if (icongap != value)
			{
				icongap = value;
				Invalidate();
				OnPropertyChanged("IconGap");
			}
		}
	}

	/// <summary>
	/// 是否包含自定义图标
	/// </summary>
	public bool HasIcon
	{
		get
		{
			if (iconSvg == null)
			{
				return iconCustom != null;
			}
			return true;
		}
	}

	/// <summary>
	/// 文本轮播
	/// </summary>
	[Description("文本轮播")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Loop
	{
		get
		{
			return loop;
		}
		set
		{
			if (loop != value)
			{
				loop = value;
				if (IsHandleCreated)
				{
					StartTask();
				}
				OnPropertyChanged("Loop");
			}
		}
	}

	/// <summary>
	/// 溢出轮播
	/// </summary>
	[Description("溢出轮播")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool LoopOverflow { get; set; }

	/// <summary>
	/// 轮播文本无尽
	/// </summary>
	[Description("轮播文本无尽")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool LoopInfinite { get; set; } = true;

	/// <summary>
	/// 鼠标移入时暂停轮播
	/// </summary>
	[Description("鼠标移入时暂停轮播")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool LoopPauseOnMouseEnter { get; set; } = true;

	/// <summary>
	/// 文本轮播速率
	/// </summary>
	[Description("文本轮播速率")]
	[Category("Appearance")]
	[DefaultValue(10)]
	public int LoopSpeed
	{
		get
		{
			return loopSpeed;
		}
		set
		{
			if (value < 1)
			{
				value = 1;
			}
			loopSpeed = value;
		}
	}

	/// <summary>
	/// 显示区域（容器）
	/// </summary>
	public override Rectangle DisplayRectangle => ClientRectangle.PaddingRect(Padding, Dpi, borWidth / 2f);

	public override Rectangle ReadRectangle => DisplayRectangle;

	public override GraphicsPath RenderRegion => DisplayRectangle.RoundPath((float)radius * Dpi);

	/// <summary>
	/// Close时发生
	/// </summary>
	[Description("Close时发生")]
	[Category("Behavior")]
	public event RBoolEventHandler? CloseChanged;

	public Alert()
	{
		hover_close = new ITaskOpacity("Alert", this);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		this.AddListener();
		val = ((icon != TType.None) ? (-Height) : 0);
		if (loop)
		{
			StartTask();
		}
	}

	protected override void OnFontChanged(EventArgs e)
	{
		ClearFont();
		base.OnFontChanged(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		ClearFont();
		base.OnSizeChanged(e);
	}

	private void ClearFont()
	{
		font_size = null;
		if (IsHandleCreated && loop && task == null)
		{
			StartTask();
		}
	}

	private void StartTask()
	{
		task?.Dispose();
		if (loop)
		{
			task = new AnimationTask(new AnimationLoopConfig(this, TextAnimation, LoopSpeed).SetEnd(EndTask));
		}
		else
		{
			Invalidate();
		}
	}

	private void EndTask()
	{
		task = null;
		Invalidate();
	}

	private bool TextAnimation()
	{
		if (font_size.HasValue && font_size.Value.Width > 0)
		{
			if (LoopOverflow && font_size.Value.Width < DisplayRectangle.Width)
			{
				loopState = true;
				val = 0;
				return false;
			}
			loopState = false;
			val++;
			if (val > font_size.Value.Width)
			{
				if (LoopInfinite)
				{
					if (Width > font_size.Value.Width)
					{
						val = 0;
					}
					else
					{
						val = -(Width - Padding.Horizontal);
					}
				}
				else
				{
					val = -(Width - Padding.Horizontal);
				}
			}
			Invalidate();
		}
		else
		{
			Thread.Sleep(1000);
		}
		return loop;
	}

	protected virtual bool OnCloseChanged()
	{
		if (CloseChanged != null)
		{
			return CloseChanged(this, EventArgs.Empty);
		}
		return true;
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && closeIcon && rect_close.Contains(e.X, e.Y))
		{
			if (OnCloseChanged())
			{
				Dispose();
			}
		}
		else
		{
			base.OnMouseClick(e);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (closeIcon)
		{
			hover_close.MaxValue = Colour.Text.Get(ColorScheme, "Alert", Name).A - Colour.TextQuaternary.Get(ColorScheme, "Alert", Name).A;
			hover_close.Switch = rect_close.Contains(e.X, e.Y);
			SetCursor(hover_close.Switch);
		}
		else
		{
			SetCursor(val: false);
		}
		base.OnMouseMove(e);
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		Rectangle displayRectangle = DisplayRectangle;
		Canvas canvas = e.Canvas;
		bool flag = string.IsNullOrEmpty(Text);
		if (icon == TType.None && !HasIcon)
		{
			if (loop)
			{
				if (!font_size.HasValue && !flag)
				{
					font_size = canvas.MeasureText(Text, Font);
				}
				if (font_size.HasValue)
				{
					canvas.SetClip(displayRectangle);
					PaintText(canvas, displayRectangle, font_size.Value);
					canvas.ResetClip();
				}
			}
			else if (string.IsNullOrEmpty(TextTitle))
			{
				if (!flag)
				{
					Size value = canvas.MeasureText(Text, Font);
					font_size = value;
					int num = (int)((float)(int)((float)value.Height * 0.86f) * 0.4f);
					canvas.DrawText(rect: new Rectangle(displayRectangle.X + num, displayRectangle.Y, displayRectangle.Width - num * 2, displayRectangle.Height), text: Text, font: Font, color: textColor ?? ForeColor, format: sf);
				}
			}
			else
			{
				using Font font = new Font(Font.FontFamily, Font.Size * 1.14f, Font.Style);
				Size size = canvas.MeasureText(TextTitle, font);
				int num2 = (int)((float)size.Height * 1.2f);
				int num3 = (int)((float)num2 * 0.5f);
				using SolidBrush brush = new SolidBrush(textColor ?? ForeColor);
				using SolidBrush brush2 = new SolidBrush(textTitleColor ?? ForeColor);
				Rectangle rect = new Rectangle(displayRectangle.X + num3, displayRectangle.Y + num3, displayRectangle.Width - num3 * 2, size.Height);
				canvas.DrawText(TextTitle, font, brush2, rect, sf);
				int num4 = rect.Bottom + (int)((float)num2 * 0.33f);
				canvas.DrawText(rect: new Rectangle(rect.X, num4, rect.Width, displayRectangle.Height - (num4 + num3)), text: Text, font: Font, brush: brush, format: sEllipsis);
			}
		}
		else
		{
			float num5 = (float)radius * Dpi;
			Color color = Colour.Text.Get(ColorScheme, "Alert", Name);
			Color color2;
			Color color3;
			switch (icon)
			{
			case TType.Success:
				color2 = Colour.SuccessBg.Get(ColorScheme, "Alert", Name);
				color3 = Colour.SuccessBorder.Get(ColorScheme, "Alert", Name);
				break;
			case TType.Info:
				color2 = Colour.InfoBg.Get(ColorScheme, "Alert", Name);
				color3 = Colour.InfoBorder.Get(ColorScheme, "Alert", Name);
				break;
			case TType.Warn:
				color2 = Colour.WarningBg.Get(ColorScheme, "Alert", Name);
				color3 = Colour.WarningBorder.Get(ColorScheme, "Alert", Name);
				break;
			case TType.Error:
				color2 = Colour.ErrorBg.Get(ColorScheme, "Alert", Name);
				color3 = Colour.ErrorBorder.Get(ColorScheme, "Alert", Name);
				break;
			default:
				color2 = Colour.SuccessBg.Get(ColorScheme, "Alert", Name);
				color3 = Colour.SuccessBorder.Get(ColorScheme, "Alert", Name);
				break;
			}
			using GraphicsPath path = displayRectangle.RoundPath(num5);
			Size size2 = canvas.MeasureString("龍Qq", Font);
			canvas.Fill(color2, path);
			if (loop)
			{
				if (!font_size.HasValue && !flag)
				{
					font_size = canvas.MeasureText(Text, Font);
				}
				if (font_size.HasValue)
				{
					int num6 = (int)((float)size2.Height * (iconratio ?? 0.86f));
					int num7 = (int)((float)num6 * (icongap ?? 0.4f));
					Rectangle rectangle = new Rectangle(num7, displayRectangle.Y + (displayRectangle.Height - num6) / 2, num6, num6);
					PaintText(canvas, displayRectangle, rectangle, font_size.Value, color, color2, num5);
					canvas.ResetClip();
					if (iconCustom == null)
					{
						canvas.PaintIcons(icon, iconSvg, rectangle, Colour.BgBase, "Alert", ColorScheme, Name);
					}
					else
					{
						canvas.Image(iconCustom, rectangle);
					}
				}
			}
			else
			{
				if (!flag)
				{
					Size value2 = canvas.MeasureText(Text, Font);
					font_size = value2;
				}
				if (string.IsNullOrEmpty(TextTitle))
				{
					int num8 = (int)((float)size2.Height * (iconratio ?? 0.86f));
					int num9 = (int)((float)num8 * (icongap ?? 0.4f));
					Rectangle rect2 = new Rectangle(displayRectangle.X + num9, displayRectangle.Y + (displayRectangle.Height - num8) / 2, num8, num8);
					if (iconCustom == null)
					{
						canvas.PaintIcons(icon, iconSvg, rect2, Colour.BgBase, "Alert", ColorScheme, Name);
					}
					else
					{
						canvas.Image(iconCustom, rect2);
					}
					Rectangle rect3 = new Rectangle(rect2.X + rect2.Width + num9, displayRectangle.Y, displayRectangle.Width - (rect2.Width + num9 * 3), displayRectangle.Height);
					canvas.DrawText(Text, Font, textColor ?? color, rect3, sf);
					PaintCloseIcon(canvas, rect3, 0.4f);
				}
				else
				{
					using Font font2 = new Font(Font.FontFamily, Font.Size * 1.14f, Font.Style);
					int num10 = (int)((float)size2.Height * (iconratio ?? 1.2f));
					int num11 = (int)((float)num10 * (icongap ?? 0.5f));
					Rectangle rect4 = new Rectangle(displayRectangle.X + num11, displayRectangle.Y + num11, num10, num10);
					if (iconCustom == null)
					{
						canvas.PaintIcons(icon, iconSvg, rect4, Colour.BgBase, "Alert", ColorScheme, Name);
					}
					else
					{
						canvas.Image(iconCustom, rect4);
					}
					using SolidBrush brush3 = new SolidBrush(textColor ?? color);
					using SolidBrush brush4 = new SolidBrush(textTitleColor ?? color);
					Rectangle rect5 = new Rectangle(rect4.X + rect4.Width + num10 / 2, rect4.Y, displayRectangle.Width - (rect4.Width + num11 * 3), rect4.Height);
					canvas.DrawText(TextTitle, font2, brush4, rect5, sf);
					int num12 = rect5.Bottom + (int)((float)num10 * 0.2f);
					canvas.DrawText(rect: new Rectangle(rect5.X, num12, rect5.Width, displayRectangle.Height - (num12 + num11)), text: Text, font: Font, brush: brush3, format: sEllipsis);
					PaintCloseIcon(canvas, rect5, 0.7f);
				}
			}
			if (borWidth > 0f)
			{
				canvas.Draw(color3, borWidth * Dpi, path);
			}
		}
		base.OnDraw(e);
	}

	/// <summary>
	/// 渲染文字
	/// </summary>
	/// <param name="g">GDI</param>
	/// <param name="rect">区域</param>
	/// <param name="size">文字大小</param>
	/// <param name="fore">文字颜色</param>
	private void PaintText(Canvas g, Rectangle rect, Size size)
	{
		using SolidBrush brush = new SolidBrush(textColor ?? ForeColor);
		using SolidBrush brush2 = new SolidBrush(textTitleColor ?? ForeColor);
		if (string.IsNullOrEmpty(TextTitle))
		{
			bool flag = rect.Width > size.Width;
			if (loopState & flag)
			{
				g.DrawText(Text, Font, brush, rect, sf);
				return;
			}
			Rectangle rect2 = new Rectangle(rect.X - val, rect.Y, size.Width, rect.Height);
			g.DrawText(Text, Font, brush, rect2, sc);
			if (LoopInfinite & flag)
			{
				g.DrawText(Text, Font, brush, rect2, sc);
				int num = rect.Width + rect2.Width / 2;
				Rectangle rect3 = new Rectangle(rect2.Right, rect2.Y, rect2.Width, rect2.Height);
				while (rect3.X < num)
				{
					g.DrawText(Text, Font, brush, rect3, sc);
					rect3.X = rect3.Right;
				}
			}
			return;
		}
		Size size2 = g.MeasureText(TextTitle, Font);
		Rectangle rect4 = new Rectangle(rect.X + size2.Width - val, rect.Y, size.Width, rect.Height);
		g.DrawText(Text, Font, brush, rect4, sc);
		if (LoopInfinite && rect.Width > size.Width)
		{
			int num2 = rect.Width + rect4.Width / 2;
			Rectangle rect5 = new Rectangle(rect4.Right, rect4.Y, rect4.Width, rect4.Height);
			while (rect5.X < num2)
			{
				g.DrawText(Text, Font, brush, rect5, sc);
				rect5.X = rect5.Right;
			}
		}
		Rectangle rect6 = new Rectangle(rect.X, rect.Y, (size.Height + size2.Width) * 2, rect.Height);
		using (LinearGradientBrush brush3 = new LinearGradientBrush(rect6, BackColor, Color.Transparent, 0f))
		{
			g.Fill(brush3, rect6);
			g.Fill(brush3, rect6);
		}
		g.DrawText(TextTitle, Font, brush2, new Rectangle(rect.X, rect.Y, size2.Width, rect.Height), sc);
	}

	/// <summary>
	/// 渲染文字（渐变遮罩）
	/// </summary>
	/// <param name="g">GDI</param>
	/// <param name="rect">区域</param>
	/// <param name="size">文字大小</param>
	/// <param name="fore">文字颜色</param>
	/// <param name="back">背景颜色</param>
	private void PaintText(Canvas g, Rectangle rect, Rectangle rect_icon, Size size, Color fore, Color back, float radius)
	{
		using SolidBrush brush = new SolidBrush(textColor ?? fore);
		using SolidBrush brush2 = new SolidBrush(textTitleColor ?? fore);
		Rectangle rect2 = new Rectangle(rect.X - val, rect.Y, size.Width, rect.Height);
		g.SetClip(new Rectangle(rect.X, rect2.Y + (rect.Height - size.Height) / 2, rect.Width, size.Height));
		g.DrawText(Text, Font, brush, rect2, sc);
		if (LoopInfinite && rect.Width > size.Width)
		{
			int num = rect.Width + rect2.Width / 2;
			Rectangle rect3 = new Rectangle(rect2.Right, rect2.Y, rect2.Width, rect2.Height);
			while (rect3.X < num)
			{
				g.DrawText(Text, Font, brush, rect3, sc);
				rect3.X = rect3.Right;
			}
		}
		Rectangle rect4;
		if (string.IsNullOrEmpty(TextTitle))
		{
			rect4 = new Rectangle(rect.X, rect.Y, size.Height * 2, rect.Height);
			using LinearGradientBrush brush3 = new LinearGradientBrush(rect4, back, Color.Transparent, 0f);
			rect4.Width--;
			g.Fill(brush3, rect4);
			g.Fill(brush3, rect4);
		}
		else
		{
			Size size2 = g.MeasureText(TextTitle, Font);
			rect4 = new Rectangle(rect.X, rect.Y, (size.Height + size2.Width) * 2, rect.Height);
			using (LinearGradientBrush brush4 = new LinearGradientBrush(rect4, back, Color.Transparent, 0f))
			{
				g.Fill(brush4, rect4);
				g.Fill(brush4, rect4);
			}
			g.DrawText(TextTitle, Font, brush2, new Rectangle(rect_icon.Right, rect.Y, size2.Width, rect.Height), sc);
		}
		Rectangle rect5 = new Rectangle(rect.Right - rect4.Width, rect4.Y, rect4.Width, rect4.Height);
		using LinearGradientBrush brush5 = new LinearGradientBrush(rect5, Color.Transparent, back, 0f);
		rect5.X++;
		rect5.Width--;
		g.Fill(brush5, rect5);
		g.Fill(brush5, rect5);
	}

	private void PaintCloseIcon(Canvas g, Rectangle rect, float ratio)
	{
		if (closeIcon)
		{
			int num = (int)((float)rect.Height * ratio);
			Rectangle rectangle = new Rectangle(rect.Right - num, rect.Y + (rect.Height - num) / 2, num, num);
			rect_close = rectangle;
			if (hover_close.Animation)
			{
				g.Svg(SvgDb.IcoErrorGhost, rectangle, Helper.ToColor(hover_close.Value + Colour.TextQuaternary.Get(ColorScheme, "Alert", Name).A, Colour.Text.Get(ColorScheme, "Alert", Name)));
			}
			else if (hover_close.Switch)
			{
				g.Svg(SvgDb.IcoErrorGhost, rectangle, Colour.Text.Get(ColorScheme, "Alert", Name));
			}
			else
			{
				g.Svg(SvgDb.IcoErrorGhost, rectangle, Colour.TextQuaternary.Get(ColorScheme, "Alert", Name));
			}
		}
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		if (LoopPauseOnMouseEnter)
		{
			task?.Dispose();
		}
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		if (LoopPauseOnMouseEnter && loop)
		{
			StartTask();
		}
		base.OnMouseLeave(e);
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG)
		{
			ClearFont();
		}
	}
}
