using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Label 文本
/// </summary>
/// <remarks>显示一段文本。</remarks>
[Description("Label 文本")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
public class Label : IControl, ShadowConfig, IEventListener
{
	private Color? fore;

	private string? colorExtend;

	private string? text;

	private FormatFlags sf_wrap = FormatFlags.Center | FormatFlags.NoWrap;

	private FormatFlags sf = FormatFlags.VerticalCenter | FormatFlags.Left;

	private ContentAlignment textAlign = ContentAlignment.MiddleLeft;

	private bool autoEllipsis;

	private bool textMultiLine = true;

	private float iconratio = 0.7f;

	private int iconGap;

	private string? prefix;

	private Image? prefixIcon;

	private string? prefixSvg;

	private string? suffix;

	private Image? suffixIcon;

	private string? suffixSvg;

	private TRotate rotate;

	private int shadow;

	private float shadowOpacity = 0.3f;

	private int shadowOffsetX;

	private int shadowOffsetY;

	private bool ellipsis;

	private TooltipForm? tooltipForm;

	private TAutoSize autoSize;

	/// <summary>
	/// 文字颜色
	/// </summary>
	[Description("文字颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public new Color? ForeColor
	{
		get
		{
			return fore;
		}
		set
		{
			if (!(fore == value))
			{
				fore = value;
				Invalidate();
				OnPropertyChanged("ForeColor");
			}
		}
	}

	/// <summary>
	/// 文字渐变色
	/// </summary>
	[Description("文字渐变色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? ColorExtend
	{
		get
		{
			return colorExtend;
		}
		set
		{
			if (!(colorExtend == value))
			{
				colorExtend = value;
				Invalidate();
				OnPropertyChanged("ColorExtend");
			}
		}
	}

	/// <summary>
	/// 文本
	/// </summary>
	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
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
				text = value;
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnTextChanged(EventArgs.Empty);
				OnPropertyChanged("Text");
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
				Invalidate();
				OnPropertyChanged("TextAlign");
			}
		}
	}

	/// <summary>
	/// 文本超出自动处理
	/// </summary>
	[Description("文本超出自动处理")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool AutoEllipsis
	{
		get
		{
			return autoEllipsis;
		}
		set
		{
			if (autoEllipsis != value)
			{
				autoEllipsis = value;
				if (value)
				{
					sf |= FormatFlags.EllipsisCharacter;
				}
				else
				{
					sf &= ~FormatFlags.EllipsisCharacter;
				}
				Invalidate();
				OnPropertyChanged("AutoEllipsis");
			}
		}
	}

	/// <summary>
	/// 是否多行
	/// </summary>
	[Description("是否多行")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool TextMultiLine
	{
		get
		{
			return textMultiLine;
		}
		set
		{
			if (textMultiLine != value)
			{
				textMultiLine = value;
				if (value)
				{
					sf &= ~FormatFlags.NoWrap;
				}
				else
				{
					sf |= FormatFlags.NoWrap;
				}
				Invalidate();
				OnPropertyChanged("TextMultiLine");
			}
		}
	}

	/// <summary>
	/// 图标比例
	/// </summary>
	[Description("图标比例")]
	[Category("Appearance")]
	[DefaultValue(0.7f)]
	public float IconRatio
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
				IOnSizeChanged();
				Invalidate();
				OnPropertyChanged("IconRatio");
			}
		}
	}

	/// <summary>
	/// 图标与文本间隔
	/// </summary>
	[Description("图标与文本间隔")]
	[Category("Appearance")]
	[DefaultValue(0)]
	public int IconGap
	{
		get
		{
			return iconGap;
		}
		set
		{
			if (iconGap != value)
			{
				iconGap = value;
				IOnSizeChanged();
				Invalidate();
				OnPropertyChanged("IconGap");
			}
		}
	}

	/// <summary>
	/// 前缀
	/// </summary>
	[Description("前缀")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? Prefix
	{
		get
		{
			return this.GetLangI(LocalizationPrefix, prefix);
		}
		set
		{
			if (!(prefix == value))
			{
				prefix = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("Prefix");
			}
		}
	}

	[Description("前缀")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationPrefix { get; set; }

	/// <summary>
	/// 前缀图标
	/// </summary>
	[Description("前缀图标")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Image? PrefixIcon
	{
		get
		{
			return prefixIcon;
		}
		set
		{
			if (prefixIcon != value)
			{
				prefixIcon = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("PrefixIcon");
			}
		}
	}

	/// <summary>
	/// 前缀SVG
	/// </summary>
	[Description("前缀SVG")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? PrefixSvg
	{
		get
		{
			return prefixSvg;
		}
		set
		{
			if (!(prefixSvg == value))
			{
				prefixSvg = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("PrefixSvg");
			}
		}
	}

	/// <summary>
	/// 前缀颜色
	/// </summary>
	[Description("前缀颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? PrefixColor { get; set; }

	/// <summary>
	/// 前缀颜色枚举
	/// </summary>
	[Description("前缀颜色枚举")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Colour? PrefixColour { get; set; }

	/// <summary>
	/// 是否包含前缀
	/// </summary>
	public bool HasPrefix
	{
		get
		{
			if (prefixSvg == null && Prefix == null)
			{
				return prefixIcon != null;
			}
			return true;
		}
	}

	/// <summary>
	/// 后缀
	/// </summary>
	[Description("后缀")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? Suffix
	{
		get
		{
			return this.GetLangI(LocalizationSuffix, suffix);
		}
		set
		{
			if (!(suffix == value))
			{
				suffix = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("Suffix");
			}
		}
	}

	[Description("后缀")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationSuffix { get; set; }

	/// <summary>
	/// 后缀图标
	/// </summary>
	[Description("后缀图标")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Image? SuffixIcon
	{
		get
		{
			return suffixIcon;
		}
		set
		{
			if (suffixIcon != value)
			{
				suffixIcon = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("SuffixIcon");
			}
		}
	}

	/// <summary>
	/// 后缀SVG
	/// </summary>
	[Description("后缀SVG")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? SuffixSvg
	{
		get
		{
			return suffixSvg;
		}
		set
		{
			if (!(suffixSvg == value))
			{
				suffixSvg = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("SuffixSvg");
			}
		}
	}

	/// <summary>
	/// 后缀颜色
	/// </summary>
	[Description("后缀颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? SuffixColor { get; set; }

	/// <summary>
	/// 后缀颜色枚举
	/// </summary>
	[Description("后缀颜色枚举")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Colour? SuffixColour { get; set; }

	/// <summary>
	/// 缀标完全展示
	/// </summary>
	[Description("缀标完全展示")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool Highlight { get; set; } = true;

	/// <summary>
	/// 是否包含后缀
	/// </summary>
	public bool HasSuffix
	{
		get
		{
			if (suffixSvg == null && Suffix == null)
			{
				return suffixIcon != null;
			}
			return true;
		}
	}

	/// <summary>
	/// 超出文字显示 Tooltip
	/// </summary>
	[Description("超出文字显示 Tooltip")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool ShowTooltip { get; set; } = true;

	/// <summary>
	/// 超出文字提示配置
	/// </summary>
	[Browsable(false)]
	[Description("超出文字提示配置")]
	[Category("Behavior")]
	[DefaultValue(null)]
	public TooltipConfig? TooltipConfig { get; set; }

	/// <summary>
	/// 旋转
	/// </summary>
	[Description("旋转")]
	[Category("Appearance")]
	[DefaultValue(TRotate.None)]
	public TRotate Rotate
	{
		get
		{
			return rotate;
		}
		set
		{
			if (rotate != value)
			{
				rotate = value;
				IOnSizeChanged();
				if (BeforeAutoSize())
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 阴影大小
	/// </summary>
	[Description("阴影大小")]
	[Category("阴影")]
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
				Invalidate();
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
	public Color? ShadowColor { get; set; }

	/// <summary>
	/// 阴影透明度
	/// </summary>
	[Description("阴影透明度")]
	[Category("阴影")]
	[DefaultValue(0.3f)]
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
				Invalidate();
				OnPropertyChanged("ShadowOpacity");
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
				Invalidate();
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
				Invalidate();
				OnPropertyChanged("ShadowOffsetY");
			}
		}
	}

	public override Rectangle ReadRectangle => ClientRectangle.PaddingRect(Padding);

	/// <summary>
	/// 自动大小
	/// </summary>
	[Browsable(true)]
	[Description("自动大小")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public override bool AutoSize
	{
		get
		{
			return base.AutoSize;
		}
		set
		{
			if (base.AutoSize == value)
			{
				return;
			}
			base.AutoSize = value;
			if (value)
			{
				if (autoSize == TAutoSize.None)
				{
					autoSize = TAutoSize.Auto;
				}
			}
			else
			{
				autoSize = TAutoSize.None;
			}
			BeforeAutoSize();
		}
	}

	/// <summary>
	/// 自动大小模式
	/// </summary>
	[Description("自动大小模式")]
	[Category("Appearance")]
	[DefaultValue(TAutoSize.None)]
	public TAutoSize AutoSizeMode
	{
		get
		{
			return autoSize;
		}
		set
		{
			if (autoSize != value)
			{
				autoSize = value;
				base.AutoSize = autoSize != TAutoSize.None;
				BeforeAutoSize();
			}
		}
	}

	/// <summary>
	/// 自动大小填充边距（仅 MiddleCenter）
	/// </summary>
	[Description("自动大小填充边距（仅 MiddleCenter）")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool AutoSizePadding { get; set; }

	public virtual Size PSize
	{
		get
		{
			bool has_prefixText = Prefix != null;
			bool has_suffixText = Suffix != null;
			bool has_prefix = prefixSvg != null || prefixIcon != null;
			bool has_suffix = suffixSvg != null || suffixIcon != null;
			return this.GDI((Canvas g) =>
			{
				Size size = g.MeasureText(Text ?? "龍Qq", Font, (textMultiLine && autoSize == TAutoSize.Height) ? Width : 0);
				if (string.IsNullOrWhiteSpace(Text))
				{
					size.Width = 0;
				}
				if (has_prefixText | has_suffixText | has_prefix | has_suffix)
				{
					float num = 0f;
					if (has_prefix)
					{
						num += (float)(int)((float)size.Height * iconratio);
					}
					else if (has_prefixText)
					{
						int num2 = g.MeasureText(Prefix, Font).Width;
						num += (float)num2;
					}
					if (has_suffix)
					{
						num += (float)(int)((float)size.Height * iconratio);
					}
					else if (has_suffixText)
					{
						int num3 = g.MeasureText(Suffix, Font).Width;
						num += (float)num3;
					}
					if (AutoSizePadding && textAlign == ContentAlignment.MiddleCenter)
					{
						size = size.SizeEm(Font);
					}
					Size size2 = size.SizeEm(Font).DeflateSize(Padding);
					return new Size((int)Math.Ceiling((float)size2.Width + num), size2.Height);
				}
				return (AutoSizePadding && textAlign == ContentAlignment.MiddleCenter) ? size.SizeEm(Font).DeflateSize(Padding) : size.DeflateSize(Padding);
			});
		}
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		Canvas canvas = e.Canvas;
		Rectangle readRectangle = ReadRectangle;
		if (readRectangle.Width == 0 || readRectangle.Height == 0)
		{
			return;
		}
		if (rotate == TRotate.Clockwise_90)
		{
			using Matrix matrix = new Matrix();
			matrix.RotateAt(90f, new PointF(Width / 2, Height / 2));
			e.Graphics.Transform = matrix;
		}
		else if (rotate == TRotate.CounterClockwise_90)
		{
			using Matrix matrix2 = new Matrix();
			matrix2.RotateAt(-90f, new PointF(Width / 2, Height / 2));
			e.Graphics.Transform = matrix2;
		}
		Color color = Colour.DefaultColor.Get(ColorScheme, "Label", Name);
		if (fore.HasValue)
		{
			color = fore.Value;
		}
		if (shadow > 0)
		{
			using Bitmap bitmap = new Bitmap(Width, Height);
			using (Canvas g = Graphics.FromImage(bitmap).HighLay(Dpi, text: true))
			{
				PaintText(g, Text, ShadowColor ?? color, readRectangle);
			}
			Helper.Blur(bitmap, shadow);
			canvas.Image(bitmap, new Rectangle(shadowOffsetX, shadowOffsetY, bitmap.Width, bitmap.Height), shadowOpacity);
		}
		PaintText(canvas, Text, color, readRectangle);
		base.OnDraw(e);
	}

	private void PaintPrefix(Canvas g, Rectangle rect, Color color)
	{
		if (prefixIcon != null)
		{
			g.Image(prefixIcon, rect);
		}
		if (prefixSvg != null)
		{
			g.Svg(prefixSvg, rect, PrefixColor.HandColor(PrefixColour, color));
		}
	}

	private void PaintSuffix(Canvas g, Rectangle rect, Color color)
	{
		if (suffixIcon != null)
		{
			g.Image(suffixIcon, rect);
		}
		if (suffixSvg != null)
		{
			g.Svg(suffixSvg, rect, SuffixColor.HandColor(SuffixColour, color));
		}
	}

	private void PaintText(Canvas g, string? text, Color color, Rectangle rect_read)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		Size font_size = g.MeasureText(text, Font);
		bool flag = Prefix != null;
		bool flag2 = Suffix != null;
		bool flag3 = prefixSvg != null || prefixIcon != null;
		bool flag4 = suffixSvg != null || suffixIcon != null;
		Rectangle rect;
		if (flag | flag2 | flag3 | flag4)
		{
			switch (textAlign)
			{
			case ContentAlignment.TopLeft:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.BottomLeft:
				rect = PaintTextLeft(g, color, rect_read, font_size, flag, flag2, flag3, flag4);
				break;
			case ContentAlignment.TopRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.BottomRight:
				rect = PaintTextRight(g, color, rect_read, font_size, flag, flag2, flag3, flag4);
				break;
			default:
				rect = PaintTextCenter(g, color, rect_read, font_size, flag, flag2, flag3, flag4);
				break;
			}
		}
		else
		{
			rect = rect_read;
		}
		TRotate tRotate = rotate;
		if ((uint)(tRotate - 1) <= 1u)
		{
			if (autoEllipsis)
			{
				if (sf.HasFlag(FormatFlags.NoWrap))
				{
					ellipsis = rect.Height < font_size.Width;
				}
				else if (rect.Height < font_size.Width)
				{
					int num = (int)Math.Ceiling((float)font_size.Width * 1f / (float)rect.Height);
					int num2 = font_size.Height * num;
					ellipsis = rect.Width < num2;
				}
				else
				{
					ellipsis = false;
				}
			}
			else
			{
				ellipsis = false;
			}
			int num3 = (rect.Width - rect.Height) / 2;
			int num4 = rect.Width;
			int num5 = rect.X;
			rect.X = rect.Y + num3;
			rect.Width = rect.Height;
			rect.Height = num4;
			rect.Y = num5 - num3;
		}
		else if (autoEllipsis)
		{
			if (sf.HasFlag(FormatFlags.NoWrap))
			{
				ellipsis = rect.Width < font_size.Width;
			}
			else if (rect.Width < font_size.Width)
			{
				int num6 = (int)Math.Ceiling((float)font_size.Width * 1f / (float)rect.Width);
				int num7 = font_size.Height * num6;
				ellipsis = rect.Height < num7;
			}
			else
			{
				ellipsis = false;
			}
		}
		else
		{
			ellipsis = false;
		}
		using Brush brush = colorExtend.BrushEx(rect, color);
		g.DrawText(text, Font, brush, rect, sf);
	}

	private Rectangle PaintTextLeft(Canvas g, Color color, Rectangle rect_read, Size font_size, bool has_prefixText, bool has_suffixText, bool has_prefix, bool has_suffix)
	{
		int num = (int)((float)iconGap * Dpi);
		int num2 = font_size.Width;
		int num3 = rect_read.X;
		if (Highlight)
		{
			if (has_prefix)
			{
				int num4 = (int)((float)font_size.Height * iconratio);
				Rectangle rect = RecFixAuto(num3, num4, rect_read, font_size);
				PaintPrefix(g, rect, color);
				num3 += num4 + num;
			}
			else if (has_prefixText)
			{
				Size size = g.MeasureText(Prefix, Font, 0, sf_wrap);
				Rectangle rect2 = RecFixAuto(num3, size.Width, rect_read, font_size);
				g.DrawText(Prefix, Font, PrefixColor.HandColor(PrefixColour, color), rect2, sf_wrap);
				num3 += size.Width + num;
			}
			int num5 = rect_read.Width;
			if (has_suffix | has_suffixText)
			{
				num5 -= num;
			}
			if (has_suffix)
			{
				num5 -= (int)((float)font_size.Height * iconratio);
			}
			else if (has_suffixText)
			{
				num5 -= g.MeasureText(Suffix, Font, 0, sf_wrap).Width;
			}
			if (num2 > num5)
			{
				num2 = num5;
			}
			Rectangle result = new Rectangle(num3, rect_read.Y, num2, rect_read.Height);
			if (has_suffix | has_suffixText)
			{
				int num6 = num3 + num2 + num;
				if (has_suffix)
				{
					int w = (int)((float)font_size.Height * iconratio);
					Rectangle rect3 = RecFixAuto(num6, w, rect_read, font_size);
					PaintSuffix(g, rect3, color);
					return result;
				}
				if (has_suffixText)
				{
					g.DrawText(rect: RecFixAuto(num6, g.MeasureText(Suffix, Font, 0, sf_wrap).Width, rect_read, font_size), text: Suffix, font: Font, color: SuffixColor.HandColor(SuffixColour, color), format: sf_wrap);
				}
			}
			return result;
		}
		if (num2 > rect_read.Width)
		{
			num2 = rect_read.Width;
		}
		Rectangle result2 = new Rectangle(num3, rect_read.Y, num2, rect_read.Height);
		if (has_prefix)
		{
			int num7 = (int)((float)font_size.Height * iconratio);
			Rectangle rect4 = RecFixAuto(num3 - num7 - num, num7, rect_read, font_size);
			PaintPrefix(g, rect4, color);
		}
		else if (has_prefixText)
		{
			Size size2 = g.MeasureText(Prefix, Font, 0, sf_wrap);
			g.DrawText(rect: RecFixAuto(num3 - size2.Width - num, size2.Width, rect_read, font_size), text: Prefix, font: Font, color: PrefixColor.HandColor(PrefixColour, color), format: sf_wrap);
		}
		if (has_suffix)
		{
			int w2 = (int)((float)font_size.Height * iconratio);
			Rectangle rect5 = RecFixAuto(num3 + num2 + num, w2, rect_read, font_size);
			PaintSuffix(g, rect5, color);
			return result2;
		}
		if (has_suffixText)
		{
			g.DrawText(rect: RecFixAuto(num3 + num2 + num, g.MeasureText(Suffix, Font, 0, sf_wrap).Width, rect_read, font_size), text: Suffix, font: Font, color: SuffixColor.HandColor(SuffixColour, color), format: sf_wrap);
		}
		return result2;
	}

	private Rectangle PaintTextRight(Canvas g, Color color, Rectangle rect_read, Size font_size, bool has_prefixText, bool has_suffixText, bool has_prefix, bool has_suffix)
	{
		int num = (int)((float)iconGap * Dpi);
		int num2 = font_size.Width;
		int num3 = rect_read.Right;
		if (Highlight)
		{
			if (has_suffix)
			{
				int num4 = (int)((float)font_size.Height * iconratio);
				int num5 = num3 - num4;
				Rectangle rect = RecFixAuto(num5, num4, rect_read, font_size);
				PaintSuffix(g, rect, color);
				num3 -= num4 + num;
			}
			else if (has_suffixText)
			{
				string text = Suffix;
				Size size = g.MeasureText(text, Font, 0, sf_wrap);
				int num6 = num3 - size.Width;
				Rectangle rect2 = RecFixAuto(num6, size.Width, rect_read, font_size);
				g.DrawText(text, Font, SuffixColor.HandColor(SuffixColour, color), rect2, sf_wrap);
				num3 -= size.Width + num;
			}
			int num7 = num3 - rect_read.X;
			if (has_prefix | has_prefixText)
			{
				num7 -= num;
			}
			if (has_prefix)
			{
				num7 -= (int)((float)font_size.Height * iconratio);
			}
			else if (has_prefixText)
			{
				num7 -= g.MeasureText(Prefix, Font, 0, sf_wrap).Width;
			}
			if (num2 > num7)
			{
				num2 = num7;
			}
			int num8 = num3 - num2;
			Rectangle result = new Rectangle(num8, rect_read.Y, num2, rect_read.Height);
			if (has_prefix | has_prefixText)
			{
				int num9 = num8 - num;
				if (has_prefix)
				{
					int num10 = (int)((float)font_size.Height * iconratio);
					num9 -= num10;
					Rectangle rect3 = RecFixAuto(num9, num10, rect_read, font_size);
					PaintPrefix(g, rect3, color);
					return result;
				}
				if (has_prefixText)
				{
					Size size2 = g.MeasureText(Prefix, Font, 0, sf_wrap);
					num9 -= size2.Width;
					g.DrawText(rect: RecFixAuto(num9, size2.Width, rect_read, font_size), text: Prefix, font: Font, color: PrefixColor.HandColor(PrefixColour, color), format: sf_wrap);
				}
			}
			return result;
		}
		if (num2 > rect_read.Width)
		{
			num2 = rect_read.Width;
		}
		int num11 = rect_read.Right - num2;
		Rectangle result2 = new Rectangle(num11, rect_read.Y, num2, rect_read.Height);
		if (has_prefix)
		{
			int num12 = (int)((float)font_size.Height * iconratio);
			Rectangle rect4 = RecFixAuto(num11 - num12 - num, num12, rect_read, font_size);
			PaintPrefix(g, rect4, color);
		}
		else if (has_prefixText)
		{
			Size size3 = g.MeasureText(Prefix, Font, 0, sf_wrap);
			g.DrawText(rect: RecFixAuto(num11 - size3.Width - num, size3.Width, rect_read, font_size), text: Prefix, font: Font, color: PrefixColor.HandColor(PrefixColour, color), format: sf_wrap);
		}
		if (has_suffix)
		{
			int w = (int)((float)font_size.Height * iconratio);
			Rectangle rect5 = RecFixAuto(num11 + num2 + num, w, rect_read, font_size);
			PaintSuffix(g, rect5, color);
			return result2;
		}
		if (has_suffixText)
		{
			g.DrawText(rect: RecFixAuto(num11 + num2 + num, g.MeasureText(Suffix, Font, 0, sf_wrap).Width, rect_read, font_size), text: Suffix, font: Font, color: SuffixColor.HandColor(SuffixColour, color), format: sf_wrap);
		}
		return result2;
	}

	private Rectangle PaintTextCenter(Canvas g, Color color, Rectangle rect_read, Size font_size, bool has_prefixText, bool has_suffixText, bool has_prefix, bool has_suffix)
	{
		int num = (int)((float)iconGap * Dpi);
		int num2 = font_size.Width;
		if (Highlight)
		{
			int num3 = 0;
			if (has_prefix)
			{
				num3 = (int)((float)font_size.Height * iconratio) + num;
			}
			else if (has_prefixText)
			{
				num3 = g.MeasureText(Prefix, Font, 0, sf_wrap).Width + num;
			}
			int num4 = 0;
			if (has_suffix)
			{
				num4 = (int)((float)font_size.Height * iconratio) + num;
			}
			else if (has_suffixText)
			{
				num4 = g.MeasureText(Suffix, Font, 0, sf_wrap).Width + num;
			}
			int num5 = num2 + num3 + num4;
			int num6 = rect_read.X + (rect_read.Width - num5) / 2;
			if (has_prefix)
			{
				int num7 = (int)((float)font_size.Height * iconratio);
				Rectangle rect = RecFixAuto(num6, num7, rect_read, font_size);
				PaintPrefix(g, rect, color);
				num6 += num7 + num;
			}
			else if (has_prefixText)
			{
				Size size = g.MeasureText(Prefix, Font, 0, sf_wrap);
				Rectangle rect2 = RecFixAuto(num6, size.Width, rect_read, font_size);
				g.DrawText(Prefix, Font, PrefixColor.HandColor(PrefixColour, color), rect2, sf_wrap);
				num6 += size.Width + num;
			}
			int num8 = rect_read.Width - num3 - num4;
			if (num2 > num8)
			{
				num2 = num8;
			}
			Rectangle result = new Rectangle(num6, rect_read.Y, num2, rect_read.Height);
			if (has_suffix)
			{
				int w = (int)((float)font_size.Height * iconratio);
				int num9 = num6 + num2 + num;
				Rectangle rect3 = RecFixAuto(num9, w, rect_read, font_size);
				PaintSuffix(g, rect3, color);
				return result;
			}
			if (has_suffixText)
			{
				Size size2 = g.MeasureText(Suffix, Font, 0, sf_wrap);
				int num10 = num6 + num2 + num;
				g.DrawText(rect: RecFixAuto(num10, size2.Width, rect_read, font_size), text: Suffix, font: Font, color: SuffixColor.HandColor(SuffixColour, color), format: sf_wrap);
			}
			return result;
		}
		if (num2 > rect_read.Width)
		{
			num2 = rect_read.Width;
		}
		int num11 = rect_read.X + (rect_read.Width - num2) / 2;
		if (has_prefix)
		{
			int num12 = (int)((float)font_size.Height * iconratio);
			Rectangle rect4 = RecFixAuto(num11 - num12 - num, num12, rect_read, font_size);
			PaintPrefix(g, rect4, color);
		}
		else if (has_prefixText)
		{
			Size size3 = g.MeasureText(Prefix, Font, 0, sf_wrap);
			Rectangle rect5 = RecFixAuto(num11 - size3.Width - num, size3.Width, rect_read, font_size);
			g.DrawText(Prefix, Font, PrefixColor.HandColor(PrefixColour, color), rect5, sf_wrap);
		}
		if (has_suffix)
		{
			int w2 = (int)((float)font_size.Height * iconratio);
			Rectangle rect6 = RecFixAuto(num11 + num2 + num, w2, rect_read, font_size);
			PaintSuffix(g, rect6, color);
		}
		else if (has_suffixText)
		{
			Rectangle rect7 = RecFixAuto(num11 + num2 + num, g.MeasureText(Suffix, Font, 0, sf_wrap).Width, rect_read, font_size);
			g.DrawText(Suffix, Font, SuffixColor.HandColor(SuffixColour, color), rect7, sf_wrap);
		}
		return new Rectangle(num11, rect_read.Y, num2, rect_read.Height);
	}

	private Rectangle RecFixAuto(int x, int w, Rectangle rect_read, Size font_size)
	{
		switch (textAlign)
		{
		case ContentAlignment.TopLeft:
		case ContentAlignment.TopCenter:
		case ContentAlignment.TopRight:
			return RecFixT(x, w, rect_read, font_size);
		case ContentAlignment.BottomLeft:
		case ContentAlignment.BottomCenter:
		case ContentAlignment.BottomRight:
			return RecFixB(x, w, rect_read, font_size);
		default:
			return RecFix(x, w, rect_read);
		}
	}

	private Rectangle RecFix(int x, int w, Rectangle rect_read)
	{
		return new Rectangle(x, rect_read.Y, w, rect_read.Height);
	}

	private Rectangle RecFixT(int x, int w, Rectangle rect_read, Size font_size)
	{
		return new Rectangle(x, rect_read.Y, w, font_size.Height);
	}

	private Rectangle RecFixB(int x, int w, Rectangle rect_read, Size font_size)
	{
		return new Rectangle(x, rect_read.Bottom - font_size.Height, w, font_size.Height);
	}

	protected override void OnMouseHover(EventArgs e)
	{
		tooltipForm?.Close();
		tooltipForm = null;
		if (ellipsis && ShowTooltip && Text != null && tooltipForm == null)
		{
			tooltipForm = new TooltipForm(this, Text, TooltipConfig ?? new TooltipConfig
			{
				Font = Font,
				ArrowAlign = TAlign.Top
			});
			tooltipForm.Show(this);
		}
		base.OnMouseHover(e);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		BeforeAutoSize();
		base.OnFontChanged(e);
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		if (autoSize == TAutoSize.None)
		{
			return base.GetPreferredSize(proposedSize);
		}
		if (autoSize == TAutoSize.Width)
		{
			return new Size(PSize.Width, base.GetPreferredSize(proposedSize).Height);
		}
		if (autoSize == TAutoSize.Height)
		{
			return new Size(base.GetPreferredSize(proposedSize).Width, PSize.Height);
		}
		return PSize;
	}

	protected override void OnResize(EventArgs e)
	{
		BeforeAutoSize();
		base.OnResize(e);
	}

	private bool BeforeAutoSize()
	{
		if (autoSize == TAutoSize.None)
		{
			return true;
		}
		if (InvokeRequired)
		{
			return ITask.Invoke(this, BeforeAutoSize);
		}
		Size pSize = PSize;
		switch (autoSize)
		{
		case TAutoSize.Width:
			if (Width == pSize.Width)
			{
				return true;
			}
			Width = pSize.Width;
			break;
		case TAutoSize.Height:
			if (Height == pSize.Height)
			{
				return true;
			}
			Height = pSize.Height;
			break;
		default:
			if (Width == pSize.Width && Height == pSize.Height)
			{
				return true;
			}
			Size = pSize;
			break;
		}
		return false;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		this.AddListener();
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG)
		{
			BeforeAutoSize();
		}
	}
}
