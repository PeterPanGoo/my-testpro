using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Tag 标签
/// </summary>
/// <remarks>进行标记和分类的小标签。</remarks>
[Description("Tag 标签")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
public class Tag : IControl, IEventListener
{
	private Color? fore;

	private Color? back;

	private Image? backImage;

	private TFit backFit;

	internal float borderWidth = 1f;

	private int radius = 6;

	internal TTypeMini type;

	private bool closeIcon;

	private string? text;

	private FormatFlags sf = FormatFlags.Center | FormatFlags.NoWrap;

	private ContentAlignment textAlign = ContentAlignment.MiddleCenter;

	private bool autoEllipsis;

	private bool textMultiLine;

	private Image? image;

	private string? imageSvg;

	private ITaskOpacity hover_close;

	private RectangleF rect_close;

	private TAutoSize autoSize;

	/// <summary>
	/// 原装背景颜色
	/// </summary>
	[Description("原装背景颜色")]
	[Category("Appearance")]
	[DefaultValue(typeof(Color), "Transparent")]
	public Color OriginalBackColor
	{
		get
		{
			return base.BackColor;
		}
		set
		{
			base.BackColor = value;
		}
	}

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
	/// 背景颜色
	/// </summary>
	[Description("背景颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public new Color? BackColor
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
				OnPropertyChanged("BackColor");
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
	/// 边框宽度
	/// </summary>
	[Description("边框宽度")]
	[Category("边框")]
	[DefaultValue(1f)]
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
				Invalidate();
				OnPropertyChanged("BorderWidth");
			}
		}
	}

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
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("Radius");
			}
		}
	}

	/// <summary>
	/// 类型
	/// </summary>
	[Description("类型")]
	[Category("Appearance")]
	[DefaultValue(TTypeMini.Default)]
	public TTypeMini Type
	{
		get
		{
			return type;
		}
		set
		{
			if (type != value)
			{
				type = value;
				Invalidate();
				OnPropertyChanged("Type");
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
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("CloseIcon");
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
	[DefaultValue(ContentAlignment.MiddleCenter)]
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
				OnPropertyChanged("AutoEllipsis");
			}
		}
	}

	/// <summary>
	/// 是否多行
	/// </summary>
	[Description("是否多行")]
	[Category("Behavior")]
	[DefaultValue(false)]
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
	/// 图像
	/// </summary>
	[Description("图像")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Image? Image
	{
		get
		{
			return image;
		}
		set
		{
			if (image != value)
			{
				image = value;
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("Image");
			}
		}
	}

	[Description("图像SVG")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? ImageSvg
	{
		get
		{
			return imageSvg;
		}
		set
		{
			if (!(imageSvg == value))
			{
				imageSvg = value;
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("ImageSvg");
			}
		}
	}

	/// <summary>
	/// 是否包含图片
	/// </summary>
	public bool HasImage
	{
		get
		{
			if (imageSvg == null)
			{
				return image != null;
			}
			return true;
		}
	}

	/// <summary>
	/// 图像大小
	/// </summary>
	[Description("图像大小")]
	[Category("Appearance")]
	[DefaultValue(typeof(Size), "0, 0")]
	public Size ImageSize { get; set; }

	public override Rectangle ReadRectangle => ClientRectangle.PaddingRect(Padding, Dpi, borderWidth / 2f);

	public override GraphicsPath RenderRegion
	{
		get
		{
			Rectangle readRectangle = ReadRectangle;
			float num = (float)radius * Dpi;
			return readRectangle.RoundPath(num);
		}
	}

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

	public virtual Size PSize => this.GDI((Canvas g) =>
	{
		Size size = g.MeasureText(Text ?? "龍Qq", Font);
		int num = 0;
		if (HasImage)
		{
			num++;
		}
		if (closeIcon)
		{
			num++;
		}
		return new Size(size.Width + (int)(14f * Dpi) + size.Height * num, size.Height + (int)(8f * Dpi));
	});

	/// <summary>
	/// Close时发生
	/// </summary>
	[Description("Close时发生")]
	[Category("Behavior")]
	public event RBoolEventHandler? CloseChanged;

	protected virtual bool OnCloseChanged()
	{
		return CloseChanged?.Invoke(this, EventArgs.Empty) ?? true;
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		Canvas canvas = e.Canvas;
		Rectangle readRectangle = ReadRectangle;
		if (backImage != null)
		{
			canvas.Image(readRectangle, backImage, backFit, radius, round: false);
		}
		float num = (float)radius * Dpi;
		Color brush;
		Color color;
		Color color2;
		switch (type)
		{
		case TTypeMini.Default:
			brush = Colour.TagDefaultBg.Get(ColorScheme, "Tag", Name);
			color = Colour.TagDefaultColor.Get(ColorScheme, "Tag", Name);
			color2 = Colour.DefaultBorder.Get(ColorScheme, "Tag", Name);
			break;
		case TTypeMini.Error:
			brush = Colour.ErrorBg.Get(ColorScheme, "Tag", Name);
			color = Colour.Error.Get(ColorScheme, "Tag", Name);
			color2 = Colour.ErrorBorder.Get(ColorScheme, "Tag", Name);
			break;
		case TTypeMini.Success:
			brush = Colour.SuccessBg.Get(ColorScheme, "Tag", Name);
			color = Colour.Success.Get(ColorScheme, "Tag", Name);
			color2 = Colour.SuccessBorder.Get(ColorScheme, "Tag", Name);
			break;
		case TTypeMini.Info:
			brush = Colour.InfoBg.Get(ColorScheme, "Tag", Name);
			color = Colour.Info.Get(ColorScheme, "Tag", Name);
			color2 = Colour.InfoBorder.Get(ColorScheme, "Tag", Name);
			break;
		case TTypeMini.Warn:
			brush = Colour.WarningBg.Get(ColorScheme, "Tag", Name);
			color = Colour.Warning.Get(ColorScheme, "Tag", Name);
			color2 = Colour.WarningBorder.Get(ColorScheme, "Tag", Name);
			break;
		default:
			brush = Colour.PrimaryBg.Get(ColorScheme, "Tag", Name);
			color = Colour.Primary.Get(ColorScheme, "Tag", Name);
			color2 = Colour.Primary.Get(ColorScheme, "Tag", Name);
			break;
		}
		if (fore.HasValue)
		{
			color = fore.Value;
		}
		if (back.HasValue)
		{
			brush = back.Value;
		}
		if (backImage != null)
		{
			canvas.Image(readRectangle, backImage, backFit, num, round: false);
		}
		using (GraphicsPath path = readRectangle.RoundPath(num))
		{
			canvas.Fill(brush, path);
			if (borderWidth > 0f)
			{
				canvas.Draw(color2, borderWidth * Dpi, path);
			}
			PaintText(canvas, Text, color, readRectangle);
		}
		base.OnDraw(e);
	}

	internal void PaintText(Canvas g, string? text, Color color, Rectangle rect_read)
	{
		Size font_size = g.MeasureText(text ?? "龍Qq", Font);
		if (text == null)
		{
			if (PaintImageNoText(g, color, font_size, rect_read) && closeIcon)
			{
				int num = (int)((float)rect_read.Height * 0.4f);
				Rectangle rectangle = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				rect_close = rectangle;
				if (hover_close.Animation)
				{
					g.Svg(SvgDb.IcoErrorGhost, rectangle, Helper.ToColor(hover_close.Value + Colour.TextQuaternary.Get(ColorScheme, "Tag", Name).A, Colour.Text.Get(ColorScheme, "Tag", Name)));
				}
				else if (hover_close.Switch)
				{
					g.Svg(SvgDb.IcoErrorGhost, rectangle, Colour.Text.Get(ColorScheme, "Tag", Name));
				}
				else
				{
					g.Svg(SvgDb.IcoErrorGhost, rectangle, Colour.TextQuaternary.Get(ColorScheme, "Tag", Name));
				}
			}
			return;
		}
		bool right = RightToLeft == RightToLeft.Yes;
		RectTextLR rectTextLR = ReadRectangle.IconRect(font_size.Height, HasImage, closeIcon, right, muit: false);
		rect_close = rectTextLR.r;
		if (closeIcon)
		{
			if (hover_close.Animation)
			{
				g.PaintIconCore(rectTextLR.r, SvgDb.IcoErrorGhost, Helper.ToColor(hover_close.Value + Colour.TextQuaternary.Get(ColorScheme, "Tag", Name).A, Colour.Text.Get(ColorScheme, "Tag", Name)), 0.8f);
			}
			else if (hover_close.Switch)
			{
				g.PaintIconCore(rectTextLR.r, SvgDb.IcoErrorGhost, Colour.Text.Get(ColorScheme, "Tag", Name), 0.8f);
			}
			else
			{
				g.PaintIconCore(rectTextLR.r, SvgDb.IcoErrorGhost, Colour.TextQuaternary.Get(ColorScheme, "Tag", Name), 0.8f);
			}
		}
		PaintImage(g, color, rectTextLR.l);
		using SolidBrush brush = new SolidBrush(color);
		g.DrawText(text, Font, brush, rectTextLR.text, sf);
	}

	/// <summary>
	/// 渲染图片（没有文字）
	/// </summary>
	/// <param name="g">GDI</param>
	/// <param name="color">颜色</param>
	/// <param name="font_size">字体大小</param>
	/// <param name="rect_read">客户区域</param>
	private bool PaintImageNoText(Canvas g, Color? color, Size font_size, Rectangle rect_read)
	{
		if (imageSvg != null)
		{
			g.Svg(imageSvg, GetImageRectCenter(font_size, rect_read), color);
			return false;
		}
		if (image != null)
		{
			g.Image(image, GetImageRectCenter(font_size, rect_read));
			return false;
		}
		return true;
	}

	/// <summary>
	/// 居中的图片绘制区域
	/// </summary>
	/// <param name="font_size">字体大小</param>
	/// <param name="rect_read">客户区域</param>
	private Rectangle GetImageRectCenter(Size font_size, Rectangle rect_read)
	{
		if (ImageSize.Width > 0 && ImageSize.Height > 0)
		{
			int num = (int)((float)ImageSize.Width * Dpi);
			int num2 = (int)((float)ImageSize.Height * Dpi);
			return new Rectangle(rect_read.X + (rect_read.Width - num) / 2, rect_read.Y + (rect_read.Height - num2) / 2, num, num2);
		}
		int num3 = (int)((float)font_size.Height * 0.8f);
		return new Rectangle(rect_read.X + (rect_read.Width - num3) / 2, rect_read.Y + (rect_read.Height - num3) / 2, num3, num3);
	}

	/// <summary>
	/// 渲染图片
	/// </summary>
	/// <param name="g">GDI</param>
	/// <param name="color">颜色</param>
	/// <param name="rectl">图标区域</param>
	private void PaintImage(Canvas g, Color? color, Rectangle rectl)
	{
		if (imageSvg != null)
		{
			g.Svg(imageSvg, GetImageRect(rectl), color);
		}
		else if (image != null)
		{
			g.Image(image, GetImageRect(rectl));
		}
	}

	/// <summary>
	/// 图片绘制区域
	/// </summary>
	/// <param name="rectl">图标区域</param>
	private Rectangle GetImageRect(Rectangle rectl)
	{
		if (ImageSize.Width > 0 && ImageSize.Height > 0)
		{
			int num = (int)((float)ImageSize.Width * Dpi);
			int num2 = (int)((float)ImageSize.Height * Dpi);
			return new Rectangle(rectl.X + (rectl.Width - num) / 2, rectl.Y + (rectl.Height - num2) / 2, num, num2);
		}
		return rectl;
	}

	public Tag()
	{
		base.BackColor = Color.Transparent;
		hover_close = new ITaskOpacity("Tag", this);
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
			hover_close.MaxValue = Colour.Text.Get(ColorScheme, "Tag", Name).A - Colour.TextQuaternary.Get(ColorScheme, "Tag", Name).A;
			hover_close.Switch = rect_close.Contains(e.X, e.Y);
			SetCursor(hover_close.Switch);
		}
		else
		{
			SetCursor(val: false);
		}
		base.OnMouseMove(e);
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
