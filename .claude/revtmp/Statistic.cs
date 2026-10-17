using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Statistic 统计数值
/// </summary>
/// <remarks>展示统计数值。</remarks>
[Description("Statistic 统计数值")]
[ToolboxItem(true)]
public class Statistic : IControl
{
	private int radius = 6;

	private Color? back;

	private string? backExtend;

	private Image? backImage;

	private TFit backFit;

	private float borderWidth = 1f;

	private Color? borderColor;

	private DashStyle borderStyle;

	private string? _value;

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

	/// <summary>
	/// 文本高度比例
	/// </summary>
	[Description("文本高度比例")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public float? TextRatio { get; set; }

	/// <summary>
	/// 值高度比例
	/// </summary>
	[Description("值高度比例")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public float? ValueRatio { get; set; }

	/// <summary>
	/// 图表高度比例
	/// </summary>
	[Description("图表高度比例")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public float? ChartLineRatio { get; set; }

	[Description("数值内容")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? Value
	{
		get
		{
			return this.GetLangI(LocalizationValue, _value);
		}
		set
		{
			if (!(_value == value))
			{
				_value = value;
				Invalidate();
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("数值内容")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationValue { get; set; }

	[Description("图表饼图数据")]
	[Category("Data")]
	[DefaultValue(null)]
	public float? ChartPieData { get; set; }

	[Description("图表折线图数据")]
	[Category("Data")]
	[DefaultValue(null)]
	public double[]? ChartLineData { get; set; }

	/// <summary>
	/// 图表饼图背景颜色
	/// </summary>
	[Description("图表饼图背景颜色")]
	[Category("图表折线图")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ChartPieBg { get; set; }

	/// <summary>
	/// 图表饼图颜色
	/// </summary>
	[Description("图表饼图颜色")]
	[Category("图表折线图")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ChartPieColor { get; set; }

	/// <summary>
	/// 图表折线图颜色
	/// </summary>
	[Description("图表折线图颜色")]
	[Category("图表折线图")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ChartLineColor { get; set; }

	protected override void OnDraw(DrawEventArgs e)
	{
		base.OnDraw(e);
		Canvas canvas = e.Canvas;
		float num = borderWidth * Dpi;
		Rectangle rect = ClientRectangle.PaddingRectNoDpi(Padding, num / 2f);
		Size size = canvas.MeasureString("龍Qq", Font);
		int num2 = (int)((float)size.Height * 0.18f);
		int num3 = num2 * 2;
		using Font font = new Font(Font.FontFamily.Name, Font.Size * 2.2f);
		string langI = this.GetLangI(LocalizationText, Text);
		Size size2 = canvas.MeasureString(langI, Font);
		Size size3 = canvas.MeasureString(Value, font);
		if (ChartLineData == null)
		{
			float num4 = size2.Height + size3.Height + num2;
			float v_height_text = TextRatio ?? ((float)(size2.Height + num2) / num4);
			float v_height_value = ValueRatio ?? ((float)size3.Height / num4);
			PaintBase(canvas, rect, font, langI, num2, v_height_text, v_height_value, size2.Height, size3.Height, num);
			return;
		}
		int num5 = (int)((float)size.Height * (ChartLineRatio ?? 2.08f));
		float num6 = size2.Height + size3.Height + num5 + num3;
		float v_height_text2 = TextRatio ?? ((ValueRatio ?? ((float)(size2.Height + num2))) / num6);
		float v_height_value2 = ValueRatio ?? ((float)size3.Height / num6);
		float num7 = (float)num5 / num6;
		int num8 = (int)((float)rect.Height * num7);
		Rectangle rectangle = PaintBase(canvas, rect, font, langI, num2, v_height_text2, v_height_value2, size2.Height, size3.Height, num);
		Rectangle rectangle2 = new Rectangle(rectangle.X, rectangle.Bottom - num2, rectangle.Width, num8);
		using LinearGradientBrush brush = new LinearGradientBrush(rectangle2, ChartLineColor ?? Colour.Primary.Get(ColorScheme, "Statistic", Name), Color.Transparent, 90f);
		double num9 = 0.0;
		double[] chartLineData = ChartLineData;
		foreach (double num10 in chartLineData)
		{
			if (num10 > num9)
			{
				num9 = num10;
			}
		}
		float num11 = (float)rectangle2.Width * 1f / (float)ChartLineData.Length;
		float num12 = 0f;
		List<PointF> list = new List<PointF>(ChartLineData.Length + 4);
		chartLineData = ChartLineData;
		foreach (double num13 in chartLineData)
		{
			list.Add(new PointF((float)rectangle.X + num12, (float)rect.Bottom - (float)((double)rectangle2.Height * (num13 / num9))));
			num12 += num11;
		}
		list.Insert(0, new PointF(rectangle.X - num3, rect.Bottom));
		list.Insert(0, new PointF(rectangle.X - num2, list[0].Y));
		list.Add(new PointF(rectangle.Right + num2, list[list.Count - 1].Y));
		list.Add(new PointF(rectangle.Right + num3, rect.Bottom));
		canvas.SetClip(rectangle2);
		canvas.FillClosedCurve(brush, list.ToArray());
		canvas.ResetClip();
	}

	private Rectangle PaintBase(Canvas g, Rectangle rect, Font font, string? text, int gap, float v_height_text, float v_height_value, int text_h, int value_h, float bor)
	{
		float num = (float)radius * Dpi;
		using GraphicsPath path = rect.RoundPath(radius);
		using (Brush brush = backExtend.BrushEx(rect, back ?? Colour.BgContainer.Get(ColorScheme, "Statistic", Name)))
		{
			g.Fill(brush, path);
		}
		if (backImage != null)
		{
			g.Image(rect, backImage, backFit, num, round: false);
		}
		int num2 = (int)((float)rect.Height * v_height_text);
		int num3 = (int)((float)rect.Height * v_height_value);
		int num4 = num2 - text_h;
		int num5 = 0;
		if (ChartPieData.HasValue)
		{
			int num6 = num4 / 2;
			int num7 = (int)((float)text_h * 0.6f);
			int num8 = num7 / 2;
			int num9 = text_h + value_h + gap + num4;
			Rectangle rect2 = new Rectangle(rect.X + num4 + num8, rect.Y + num6 + num8, num9 - num7, num9 - num7);
			g.DrawEllipse(ChartPieBg ?? Colour.FillSecondary.Get(ColorScheme, "Statistic", Name), num7, rect2);
			if (ChartPieData.Value > 0f)
			{
				int num10 = (int)Math.Round(360f * ChartPieData.Value);
				using Pen pen = new Pen(ChartPieColor ?? Colour.Primary.Get(ColorScheme, "Statistic", Name), num7);
				g.DrawArc(pen, rect2, -90f, num10);
			}
			num5 += num9 + num4;
		}
		Rectangle rectangle = new Rectangle(rect.X + num4 + num5, rect.Y, rect.Width - num4 * 2 - num5, rect.Height);
		Rectangle rect3 = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, num2);
		Rectangle rectangle2 = new Rectangle(rectangle.X, rectangle.Y + rect3.Height + gap, rectangle.Width, num3);
		g.String(text, Font, Colour.TextTertiary.Get(ColorScheme, "Statistic", Name), rect3, sf);
		g.String(Value, font, Colour.TextBase.Get(ColorScheme, "Statistic", Name), rectangle2, sf);
		if (bor > 0f)
		{
			using Pen pen2 = new Pen(borderColor ?? Colour.BorderColor.Get(ColorScheme, "Statistic", Name), bor);
			pen2.DashStyle = borderStyle;
			if (!ChartPieData.HasValue)
			{
				g.DrawLine(pen2, rect.X, rect3.Bottom, rect.Right, rect3.Bottom);
			}
			g.Draw(pen2, path);
		}
		return rectangle2;
	}
}
