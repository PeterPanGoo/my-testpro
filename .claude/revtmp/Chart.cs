using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Chart 图表控件
/// </summary>
/// <remarks>用于显示各种类型的数据图表，支持多种图表类型和交互功能。</remarks>
[Description("Chart 图表控件")]
[ToolboxItem(true)]
[DefaultProperty("ChartType")]
public class Chart : IControl
{
	/// <summary>
	/// 图表配置常量
	/// </summary>
	private static class ChartConstants
	{
		/// <summary>
		/// 默认柱形图宽度除数
		/// </summary>
		public const int DefaultBarWidthDivisor = 10;

		/// <summary>
		/// 鼠标悬停检测半径（像素）
		/// </summary>
		public const int HoverDetectionRadius = 10;

		/// <summary>
		/// 默认数据点大小
		/// </summary>
		public const int DefaultPointSize = 3;

		/// <summary>
		/// 默认动画持续时间（毫秒）
		/// </summary>
		public const int DefaultAnimationDuration = 1000;
	}

	private TooltipForm? tooltipForm;

	private Point? mousePosition;

	private ChartDataPoint? selectedSegmentPoint;

	private ChartDataPoint? previousSelectedSegmentPoint;

	private int selectedSegmentIndex = -1;

	private int previousSelectedSegmentIndex = -1;

	private TChartType chartType = TChartType.Line;

	private float segmentAnimationProgress = 1f;

	private double? xMin;

	private double? xMax;

	private double? yMin;

	private double? yMax;

	private AnimationTask? animationTask;

	private AnimationTask? segmentAnimationTask;

	/// <summary>
	/// 图表类型
	/// </summary>
	[Description("图表类型")]
	[Category("图表")]
	[DefaultValue(TChartType.Line)]
	public TChartType ChartType
	{
		get
		{
			return chartType;
		}
		set
		{
			if (chartType != value)
			{
				chartType = value;
				StartAnimation();
				Invalidate();
				OnPropertyChanged("ChartType");
			}
		}
	}

	/// <summary>
	/// 数据集集合
	/// </summary>
	[Description("数据集集合")]
	[Category("Data")]
	[Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public List<ChartDataset> Datasets { get; set; } = new List<ChartDataset>();

	/// <summary>
	/// 标题
	/// </summary>
	[Description("标题")]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public string? Title { get; set; }

	/// <summary>
	/// 标题字体
	/// </summary>
	[Description("标题字体")]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public Font? TitleFont { get; set; }

	/// <summary>
	/// 标题颜色
	/// </summary>
	[Description("标题颜色")]
	[Category("Appearance")]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public Color? TitleColor { get; set; }

	/// <summary>
	/// 显示图例
	/// </summary>
	[Description("显示图例")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool ShowLegend { get; set; } = true;

	/// <summary>
	/// 图例位置
	/// </summary>
	[Description("图例位置")]
	[Category("Appearance")]
	[DefaultValue(ContentAlignment.TopRight)]
	public ContentAlignment LegendPosition { get; set; } = ContentAlignment.TopRight;

	/// <summary>
	/// 显示网格线
	/// </summary>
	[Description("显示网格线")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool ShowGrid { get; set; } = true;

	/// <summary>
	/// 网格线颜色
	/// </summary>
	[Description("网格线颜色")]
	[Category("Appearance")]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public Color? GridColor { get; set; }

	/// <summary>
	/// 显示坐标轴
	/// </summary>
	[Description("显示坐标轴")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool ShowAxes { get; set; } = true;

	/// <summary>
	/// 坐标轴颜色
	/// </summary>
	[Description("坐标轴颜色")]
	[Category("Appearance")]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public Color? AxisColor { get; set; }

	/// <summary>
	/// 显示工具提示
	/// </summary>
	[Description("显示工具提示")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool ShowTooltip { get; set; } = true;

	/// <summary>
	/// 游标模式
	/// </summary>
	[Description("游标模式")]
	[Category("Behavior")]
	[DefaultValue(TChartCursor.None)]
	public TChartCursor CursorMode { get; set; }

	/// <summary>
	/// 在游标线与坐标轴交点显示游标值
	/// </summary>
	[Description("在游标线与坐标轴交点显示游标值")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool ShowCursorAxisValue { get; set; }

	/// <summary>
	/// 启用饼图鼠标悬停值提示
	/// </summary>
	[Description("启用饼图鼠标悬停值提示")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool EnablePieHoverValue { get; set; } = true;

	/// <summary>
	/// 雷达图显示数值
	/// </summary>
	[Description("雷达图显示数值")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool ShowRadarValues { get; set; }

	/// <summary>
	/// 启用动画
	/// </summary>
	[Description("启用动画")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool EnableAnimation { get; set; } = true;

	/// <summary>
	/// 动画持续时间（毫秒）
	/// </summary>
	[Description("动画持续时间（毫秒）")]
	[Category("Behavior")]
	[DefaultValue(200)]
	public int AnimationDuration { get; set; } = 200;

	/// <summary>
	/// 内边距
	/// </summary>
	[Description("内边距")]
	[Category("Layout")]
	[DefaultValue(20)]
	public new int Padding { get; set; } = 20;

	/// <summary>
	/// 图例内边距
	/// </summary>
	[Description("图例内边距")]
	[Category("Layout")]
	[DefaultValue(10)]
	public int LegendPadding { get; set; } = 10;

	/// <summary>
	/// 图例背景颜色
	/// </summary>
	[Description("图例背景颜色")]
	[Category("Appearance")]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public Color? LegendBackColor { get; set; }

	/// <summary>
	/// 图例边框颜色
	/// </summary>
	[Description("图例边框颜色")]
	[Category("Appearance")]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public Color? LegendBorderColor { get; set; }

	/// <summary>
	/// 饼图颜色方案
	/// </summary>
	[Description("饼图颜色方案")]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public Color[]? PieColors { get; set; }

	/// <summary>
	/// 动画进度 (0-1)
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public float AnimationProgress { get; private set; } = 1f;

	/// <summary>
	/// 当前悬停的数据点
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ChartDataPoint? HoveredPoint { get; private set; }

	/// <summary>
	/// X轴最小值（null表示自动计算）
	/// </summary>
	[Description("X轴最小值")]
	[Category("坐标轴")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public double? XMin
	{
		get
		{
			return xMin;
		}
		set
		{
			if (xMin != value)
			{
				xMin = value;
				Invalidate();
				OnPropertyChanged("XMin");
			}
		}
	}

	/// <summary>
	/// X轴最大值（null表示自动计算）
	/// </summary>
	[Description("X轴最大值")]
	[Category("坐标轴")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public double? XMax
	{
		get
		{
			return xMax;
		}
		set
		{
			if (xMax != value)
			{
				xMax = value;
				Invalidate();
				OnPropertyChanged("XMax");
			}
		}
	}

	/// <summary>
	/// Y轴最小值（null表示自动计算）
	/// </summary>
	[Description("Y轴最小值")]
	[Category("坐标轴")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public double? YMin
	{
		get
		{
			return yMin;
		}
		set
		{
			if (yMin != value)
			{
				yMin = value;
				Invalidate();
				OnPropertyChanged("YMin");
			}
		}
	}

	/// <summary>
	/// Y轴最大值（null表示自动计算）
	/// </summary>
	[Description("Y轴最大值")]
	[Category("坐标轴")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public double? YMax
	{
		get
		{
			return yMax;
		}
		set
		{
			if (yMax != value)
			{
				yMax = value;
				Invalidate();
				OnPropertyChanged("YMax");
			}
		}
	}

	/// <summary>
	/// 显示X轴标签
	/// </summary>
	[Description("显示X轴标签")]
	[Category("坐标轴")]
	[DefaultValue(true)]
	public bool ShowXAxisLabels { get; set; } = true;

	/// <summary>
	/// 显示Y轴标签
	/// </summary>
	[Description("显示Y轴标签")]
	[Category("坐标轴")]
	[DefaultValue(true)]
	public bool ShowYAxisLabels { get; set; } = true;

	/// <summary>
	/// X轴标签格式
	/// </summary>
	[Description("X轴标签格式")]
	[Category("坐标轴")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public string? XAxisLabelFormat { get; set; }

	/// <summary>
	/// Y轴标签格式
	/// </summary>
	[Description("Y轴标签格式")]
	[Category("坐标轴")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public string? YAxisLabelFormat { get; set; }

	/// <summary>
	/// 数据点点击事件
	/// </summary>
	[Description("数据点点击事件")]
	[Category("Behavior")]
	public event EventHandler<ChartPointClickEventArgs>? PointClick;

	/// <summary>
	/// 图表区域点击事件
	/// </summary>
	[Description("图表区域点击事件")]
	[Category("Behavior")]
	public event EventHandler<ChartAreaClickEventArgs>? AreaClick;

	/// <summary>
	/// 数据点悬停事件
	/// </summary>
	[Description("数据点悬停事件")]
	[Category("Behavior")]
	public event EventHandler<ChartPointHoverEventArgs>? PointHover;

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			animationTask?.Dispose();
			segmentAnimationTask?.Dispose();
		}
		base.Dispose(disposing);
	}

	private void StartAnimation()
	{
		animationTask?.Dispose();
		if (EnableAnimation && Config.Animation)
		{
			AnimationProgress = 0f;
			Animation.TotalFrames(10, AnimationDuration);
			animationTask = new AnimationTask(new AnimationFixedConfig((float i) =>
			{
				AnimationProgress = i;
				Invalidate();
			}, 10, Animation.TotalFrames(10, AnimationDuration), sw: true));
		}
		else
		{
			AnimationProgress = 1f;
		}
	}

	private void StartSegmentAnimation(Rectangle chartRect)
	{
		segmentAnimationTask?.Dispose();
		if (EnableAnimation && Config.Animation)
		{
			segmentAnimationProgress = 0f;
			segmentAnimationTask = new AnimationTask(new AnimationFixedConfig((float i) =>
			{
				segmentAnimationProgress = i;
				Invalidate(GetSegmentAnimationBounds(chartRect));
				if (i >= 1f)
				{
					previousSelectedSegmentPoint = null;
					previousSelectedSegmentIndex = -1;
				}
			}, 10, Animation.TotalFrames(10, AnimationDuration), sw: true));
		}
		else
		{
			segmentAnimationProgress = 1f;
			previousSelectedSegmentPoint = null;
			previousSelectedSegmentIndex = -1;
		}
	}

	private void ShowTooltipInternal(ChartDataPoint point, int x, int y)
	{
		if (ShowTooltip)
		{
			HideTooltip();
			string text = $"{point.Label}\nX: {point.X:F2}\nY: {point.Y:F2}";
			if (point.Radius > 0.0)
			{
				text += $"\nSize: {point.Radius:F2}";
			}
			Tooltip.Config component = new Tooltip.Config(this, text)
			{
				Offset = new Rectangle(x, y, 1, 1),
				Font = Font,
				ArrowAlign = TAlign.Top
			};
			tooltipForm = new TooltipForm(this, text, component);
			tooltipForm.Show(this);
		}
	}

	private void HideTooltip()
	{
		tooltipForm?.Close();
		tooltipForm = null;
	}

	/// <summary>
	/// 添加数据集
	/// </summary>
	/// <param name="dataset">数据集</param>
	public void AddDataset(ChartDataset dataset)
	{
		Datasets.Add(dataset);
		Invalidate();
	}

	/// <summary>
	/// 移除数据集
	/// </summary>
	/// <param name="dataset">数据集</param>
	public void RemoveDataset(ChartDataset dataset)
	{
		Datasets.Remove(dataset);
		Invalidate();
	}

	/// <summary>
	/// 清除所有数据集
	/// </summary>
	public void ClearDatasets()
	{
		Datasets.Clear();
		Invalidate();
	}

	/// <summary>
	/// 刷新图表
	/// </summary>
	public void RefreshChart()
	{
		Invalidate();
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		base.OnDraw(e);
		DrawChart(e.Canvas, e.Rect);
	}

	private void DrawChart(Canvas g, Rectangle rect)
	{
		Rectangle chartRect = CalculateChartRect(rect);
		DrawTitle(g, rect, chartRect);
		if (ShowGrid)
		{
			DrawGrid(g, chartRect);
		}
		if (ShowAxes)
		{
			DrawAxes(g, chartRect);
		}
		DrawChartData(g, chartRect);
		DrawCursor(g, chartRect);
		if (ShowLegend && Datasets.Count > 0)
		{
			DrawLegend(g, rect, chartRect);
		}
	}

	private Rectangle CalculateChartRect(Rectangle rect)
	{
		Rectangle result = rect;
		result.Inflate(-Padding, -Padding);
		if (!string.IsNullOrEmpty(Title))
		{
			int num = TitleFont?.Height ?? Font.Height;
			result.Y += num + 10;
			result.Height -= num + 10;
		}
		if (ShowLegend && Datasets.Count > 0)
		{
			int num2 = CalculateLegendHeight();
			switch (LegendPosition)
			{
			case ContentAlignment.TopLeft:
			case ContentAlignment.TopCenter:
			case ContentAlignment.TopRight:
				result.Y += num2 + LegendPadding;
				result.Height -= num2 + LegendPadding;
				break;
			case ContentAlignment.BottomLeft:
			case ContentAlignment.BottomCenter:
			case ContentAlignment.BottomRight:
				result.Height -= num2 + LegendPadding;
				break;
			}
		}
		if (ShowAxes)
		{
			if (ShowYAxisLabels)
			{
				int num3 = (int)(50f * Dpi);
				result.X += num3;
				result.Width -= num3;
			}
			if (ShowXAxisLabels)
			{
				int num4 = Font.Height + (int)(10f * Dpi);
				result.Height -= num4;
			}
		}
		return result;
	}

	private void DrawTitle(Canvas g, Rectangle rect, Rectangle chartRect)
	{
		if (Title != null)
		{
			Font font = TitleFont ?? Font;
			Size size = g.MeasureString(Title, font).SizeEm(font);
			g.DrawText(Title, font, TitleColor ?? Style.Db.Text, new Rectangle(rect.X + (rect.Width - size.Width) / 2, rect.Y, size.Width, size.Height));
		}
	}

	private void DrawLegend(Canvas g, Rectangle rect, Rectangle chartRect)
	{
		List<LegendItem> list = new List<LegendItem>(Datasets.Count);
		foreach (ChartDataset dataset in Datasets)
		{
			if (dataset.Visible)
			{
				Color color;
				if (dataset.LegendBoxFillColor.HasValue)
				{
					color = dataset.LegendBoxFillColor.Value;
				}
				else
				{
					color = (dataset.FillColor.HasValue ? dataset.FillColor.Value : Color.Gray);
				}
				list.Add(new LegendItem
				{
					Label = (dataset.Label ?? "数据集"),
					Color = color
				});
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		Size legendSize = CalculateLegendSize(g, list);
		Rectangle rect2 = CalculateLegendRect(rect, legendSize);
		using (SolidBrush brush = new SolidBrush(LegendBackColor ?? (Config.IsDark ? Color.FromArgb(45, 45, 45) : Color.FromArgb(240, 240, 240))))
		{
			g.Fill(brush, rect2);
		}
		using (Pen pen = new Pen(LegendBorderColor ?? (Config.IsDark ? Color.FromArgb(70, 70, 70) : Color.LightGray), Dpi))
		{
			g.Draw(pen, rect2);
		}
		int num = (int)(5f * Dpi);
		int num2 = (int)(15f * Dpi);
		int num3 = (int)(25f * Dpi);
		int num4 = rect2.Y + num;
		foreach (LegendItem item in list)
		{
			Size size = g.MeasureString(item.Label, Font);
			int num5 = Math.Max(num2, size.Height);
			Rectangle rect3 = new Rectangle(rect2.X + num, num4 + (num5 - num2) / 2, num2, num2);
			using (SolidBrush brush2 = new SolidBrush(item.Color))
			{
				g.Fill(brush2, rect3);
			}
			using (Pen pen2 = new Pen(Config.IsDark ? Color.White : Color.Black, Dpi))
			{
				g.Draw(pen2, rect3);
			}
			using (SolidBrush brush3 = new SolidBrush(Config.IsDark ? Color.White : ForeColor))
			{
				g.String(item.Label, Font, brush3, rect2.X + num3, num4 + (num5 - size.Height) / 2);
			}
			num4 += num5 + num;
		}
	}

	private Rectangle CalculateLegendRect(Rectangle rect, Size legendSize)
	{
		int num = (int)(5f * Dpi);
		int num2 = legendSize.Width;
		int num3 = legendSize.Height;
		int num4 = 0;
		int num5 = 0;
		switch (LegendPosition)
		{
		case ContentAlignment.TopLeft:
			num4 = rect.X + num;
			num5 = rect.Y + num;
			break;
		case ContentAlignment.TopCenter:
			num4 = rect.X + (rect.Width - num2) / 2;
			num5 = rect.Y + num;
			break;
		case ContentAlignment.TopRight:
			num4 = rect.Right - num2 - 5;
			num5 = rect.Y + num;
			break;
		case ContentAlignment.MiddleLeft:
			num4 = rect.X + num;
			num5 = rect.Y + (rect.Height - num3) / 2;
			break;
		case ContentAlignment.MiddleCenter:
			num4 = rect.X + (rect.Width - num2) / 2;
			num5 = rect.Y + (rect.Height - num3) / 2;
			break;
		case ContentAlignment.MiddleRight:
			num4 = rect.Right - num2 - num;
			num5 = rect.Y + (rect.Height - num3) / 2;
			break;
		case ContentAlignment.BottomLeft:
			num4 = rect.X + num;
			num5 = rect.Bottom - num3 - num;
			break;
		case ContentAlignment.BottomCenter:
			num4 = rect.X + (rect.Width - num2) / 2;
			num5 = rect.Bottom - num3 - num;
			break;
		case ContentAlignment.BottomRight:
			num4 = rect.Right - num2 - num;
			num5 = rect.Bottom - num3 - num;
			break;
		}
		return new Rectangle(num4, num5, num2, num3);
	}

	private Size CalculateLegendSize(Canvas g, List<LegendItem> legendItems)
	{
		int num = (int)(5f * Dpi);
		int num2 = (int)(15f * Dpi);
		int num3 = (int)(10f * Dpi);
		int num4 = num;
		int num5 = 0;
		int num6 = num4;
		foreach (LegendItem legendItem in legendItems)
		{
			Size size = g.MeasureString(legendItem.Label, Font);
			num5 = Math.Max(num5, size.Width);
			int num7 = Math.Max(num2, size.Height);
			num6 += num7 + num;
		}
		if (legendItems.Count > 0)
		{
			num6 -= num;
		}
		num6 += num4;
		return new Size(num4 + num2 + num3 + num5 + num4, num6);
	}

	private int CalculateLegendHeight()
	{
		int num = (int)(5f * Dpi);
		int num2 = (int)(20f * Dpi);
		return Datasets.Count((ChartDataset d) => d.Visible) * (num2 + num) + 10;
	}

	private void DrawGrid(Canvas g, Rectangle chartRect)
	{
		using Pen pen = new Pen(GridColor ?? Style.Db.BorderColor, Dpi);
		pen.DashStyle = DashStyle.Dot;
		int num = chartRect.Width / 10;
		for (int i = 1; i < 10; i++)
		{
			int x = chartRect.X + i * num;
			g.DrawLine(pen, x, chartRect.Y, x, chartRect.Bottom);
		}
		int num2 = chartRect.Height / 10;
		for (int j = 1; j < 10; j++)
		{
			int y = chartRect.Y + j * num2;
			g.DrawLine(pen, chartRect.X, y, chartRect.Right, y);
		}
	}

	private void DrawAxes(Canvas g, Rectangle chartRect)
	{
		using (Pen pen = new Pen(AxisColor ?? Style.Db.BorderColor, Dpi * 2f))
		{
			g.DrawLine(pen, chartRect.X, chartRect.Bottom, chartRect.Right, chartRect.Bottom);
			g.DrawLine(pen, chartRect.X, chartRect.Y, chartRect.X, chartRect.Bottom);
		}
		Color color = Style.Db.Text;
		Font font = Font;
		if (ShowYAxisLabels)
		{
			if (UsesHorizontalCategoryAxis())
			{
				DrawHorizontalCategoryAxisLabels(g, chartRect, font, color);
			}
			else
			{
				double minY = GetMinY();
				GetMaxY();
				double yRange = GetYRange();
				int num = 5;
				for (int i = 0; i <= num; i++)
				{
					double num2 = minY + yRange * (double)i / (double)num;
					int num3 = chartRect.Bottom - (int)((num2 - minY) / yRange * (double)chartRect.Height);
					string text = (string.IsNullOrEmpty(YAxisLabelFormat) ? num2.ToString("F1") : num2.ToString(YAxisLabelFormat));
					Size size = g.MeasureString(text, font);
					int num4 = chartRect.X - size.Width - (int)(5f * Dpi);
					int num5 = num3 - size.Height / 2;
					if (num4 > 0 && num5 > 0)
					{
						g.DrawText(text, font, color, new Rectangle(num4, num5, size.Width, size.Height));
					}
				}
			}
		}
		if (!ShowXAxisLabels)
		{
			return;
		}
		if (UsesVerticalCategoryAxis())
		{
			DrawVerticalCategoryAxisLabels(g, chartRect, font, color);
			return;
		}
		double minX = GetMinX();
		GetMaxX();
		double xRange = GetXRange();
		int num6 = 5;
		for (int j = 0; j <= num6; j++)
		{
			double num7 = minX + xRange * (double)j / (double)num6;
			int num8 = chartRect.X + (int)((num7 - minX) / xRange * (double)chartRect.Width);
			string text2 = (string.IsNullOrEmpty(XAxisLabelFormat) ? num7.ToString("F1") : num7.ToString(XAxisLabelFormat));
			Size size2 = g.MeasureString(text2, font);
			int num9 = num8 - size2.Width / 2;
			int num10 = chartRect.Bottom + (int)(5f * Dpi);
			if (num9 > 0 && num10 + size2.Height < Height)
			{
				g.DrawText(text2, font, color, new Rectangle(num9, num10, size2.Width, size2.Height));
			}
		}
	}

	private bool UsesVerticalCategoryAxis()
	{
		if (ChartType != TChartType.Bar)
		{
			return ChartType == TChartType.StackedBar;
		}
		return true;
	}

	private bool UsesHorizontalCategoryAxis()
	{
		if (ChartType != TChartType.HorizontalBar)
		{
			return ChartType == TChartType.StackedHorizontalBar;
		}
		return true;
	}

	private void DrawVerticalCategoryAxisLabels(Canvas g, Rectangle chartRect, Font labelFont, Color labelColor)
	{
		List<string> categoryLabels = GetCategoryLabels();
		if (categoryLabels.Count == 0)
		{
			return;
		}
		float num = (float)chartRect.Width / (float)categoryLabels.Count;
		for (int i = 0; i < categoryLabels.Count; i++)
		{
			string text = categoryLabels[i];
			Size size = g.MeasureString(text, labelFont);
			int num2 = (int)((float)chartRect.X + (float)i * num + num / 2f - (float)(size.Width / 2));
			int num3 = chartRect.Bottom + (int)(5f * Dpi);
			if (num2 > 0 && num3 + size.Height < Height)
			{
				g.DrawText(text, labelFont, labelColor, new Rectangle(num2, num3, size.Width, size.Height));
			}
		}
	}

	private List<string> GetCategoryLabels()
	{
		List<ChartDataset> list = Datasets.Where((ChartDataset d) => d.Visible).ToList();
		if (list.Count == 0)
		{
			return new List<string>();
		}
		int num = list.Max((ChartDataset d) => d.DataPoints.Count((ChartDataPoint p) => p.Visible));
		List<string> list2 = new List<string>(num);
		for (int num2 = 0; num2 < num; num2++)
		{
			ChartDataPoint chartDataPoint = null;
			foreach (ChartDataset item2 in list)
			{
				List<ChartDataPoint> list3 = item2.DataPoints.Where((ChartDataPoint p) => p.Visible).ToList();
				if (num2 < list3.Count)
				{
					chartDataPoint = list3[num2];
					break;
				}
			}
			List<string> list4 = list2;
			string item;
			if (chartDataPoint == null)
			{
				item = (num2 + 1).ToString();
			}
			else
			{
				item = ((!string.IsNullOrEmpty(chartDataPoint.Label)) ? chartDataPoint.Label : chartDataPoint.X.ToString("F1"));
			}
			list4.Add(item);
		}
		return list2;
	}

	private void DrawHorizontalCategoryAxisLabels(Canvas g, Rectangle chartRect, Font labelFont, Color labelColor)
	{
		List<string> categoryLabels = GetCategoryLabels();
		if (categoryLabels.Count == 0)
		{
			return;
		}
		float num = (float)chartRect.Height / (float)categoryLabels.Count;
		for (int i = 0; i < categoryLabels.Count; i++)
		{
			string text = categoryLabels[i];
			Size size = g.MeasureString(text, labelFont);
			float num2 = (float)chartRect.Y + (float)i * num + num / 2f;
			int num3 = chartRect.X - size.Width - (int)(5f * Dpi);
			int num4 = (int)(num2 - (float)(size.Height / 2));
			if (num3 > 0 && num4 > 0)
			{
				g.DrawText(text, labelFont, labelColor, new Rectangle(num3, num4, size.Width, size.Height));
			}
		}
	}

	private void DrawCursor(Canvas g, Rectangle chartRect)
	{
		if (CursorMode == TChartCursor.None || !mousePosition.HasValue || !chartRect.Contains(mousePosition.Value) || !SupportsCursor())
		{
			return;
		}
		Point value = mousePosition.Value;
		using (Pen pen = new Pen(AxisColor ?? Style.Db.Primary, Dpi))
		{
			pen.DashStyle = DashStyle.Dash;
			if (CursorMode == TChartCursor.Vertical || CursorMode == TChartCursor.Cross)
			{
				g.DrawLine(pen, value.X, chartRect.Y, value.X, chartRect.Bottom);
			}
			if (CursorMode == TChartCursor.Horizontal || CursorMode == TChartCursor.Cross)
			{
				g.DrawLine(pen, chartRect.X, value.Y, chartRect.Right, value.Y);
			}
		}
		if (ShowCursorAxisValue)
		{
			DrawCursorAxisValue(g, chartRect, value);
		}
	}

	private void DrawCursorAxisValue(Canvas g, Rectangle chartRect, Point point)
	{
		Color color = (Config.IsDark ? Color.White : Color.Black);
		using SolidBrush brush = new SolidBrush(Config.IsDark ? Color.FromArgb(220, 45, 45, 45) : Color.FromArgb(220, 255, 255, 255));
		using SolidBrush brush2 = new SolidBrush(color);
		if (CursorMode == TChartCursor.Vertical || CursorMode == TChartCursor.Cross)
		{
			double num = GetMinX() + (double)(point.X - chartRect.X) * GetXRange() / (double)chartRect.Width;
			string text = (string.IsNullOrEmpty(XAxisLabelFormat) ? num.ToString("F2") : num.ToString(XAxisLabelFormat));
			Size size = g.MeasureString(text, Font);
			Rectangle rect = new Rectangle(point.X - size.Width / 2 - 4, chartRect.Bottom + 2, size.Width + 8, size.Height + 4);
			g.Fill(brush, rect);
			g.String(text, Font, brush2, rect.X + 4, rect.Y + 2);
		}
		if (CursorMode == TChartCursor.Horizontal || CursorMode == TChartCursor.Cross)
		{
			double num2 = GetMinY() + (double)(chartRect.Bottom - point.Y) * GetYRange() / (double)chartRect.Height;
			string text2 = (string.IsNullOrEmpty(YAxisLabelFormat) ? num2.ToString("F2") : num2.ToString(YAxisLabelFormat));
			Size size2 = g.MeasureString(text2, Font);
			Rectangle rect2 = new Rectangle(chartRect.X - size2.Width - 10, point.Y - size2.Height / 2 - 2, size2.Width + 8, size2.Height + 4);
			g.Fill(brush, rect2);
			g.String(text2, Font, brush2, rect2.X + 4, rect2.Y + 2);
		}
	}

	private bool SupportsCursor()
	{
		switch (ChartType)
		{
		case TChartType.Area:
		case TChartType.Bubble:
		case TChartType.Line:
		case TChartType.Scatter:
		case TChartType.Spline:
		case TChartType.SplineArea:
		case TChartType.SteppedArea:
		case TChartType.SteppedLine:
			return true;
		default:
			return false;
		}
	}

	private bool IsSegmentChart()
	{
		if (ChartType != TChartType.Pie && ChartType != TChartType.Doughnut)
		{
			return ChartType == TChartType.PolarArea;
		}
		return true;
	}

	private SegmentHit? FindSegmentDataPoint(int x, int y, Rectangle chartRect)
	{
		return ChartType switch
		{
			TChartType.Pie => FindPieSegmentDataPoint(x, y, chartRect, doughnut: false), 
			TChartType.Doughnut => FindPieSegmentDataPoint(x, y, chartRect, doughnut: true), 
			TChartType.PolarArea => FindPolarSegmentDataPoint(x, y, chartRect), 
			_ => null, 
		};
	}

	private SegmentHit? FindPieSegmentDataPoint(int x, int y, Rectangle chartRect, bool doughnut)
	{
		List<ChartDataPoint> visiblePoints = GetVisiblePoints();
		if (visiblePoints.Count == 0)
		{
			return null;
		}
		int num = chartRect.X + chartRect.Width / 2;
		int num2 = chartRect.Y + chartRect.Height / 2;
		int num3 = Math.Min(chartRect.Width, chartRect.Height) / 2 - 20;
		int num4 = x - num;
		int num5 = y - num2;
		double num6 = Math.Sqrt(num4 * num4 + num5 * num5);
		if (num6 > (double)num3)
		{
			return null;
		}
		if (doughnut && num6 < (double)((float)num3 * 0.6f))
		{
			return null;
		}
		double num7 = visiblePoints.Sum((ChartDataPoint p) => Math.Abs(p.Y));
		if (num7 == 0.0)
		{
			return null;
		}
		double num8 = Math.Atan2(num5, num4) * 180.0 / Math.PI;
		if (num8 < -90.0)
		{
			num8 += 360.0;
		}
		double num9 = -90.0;
		for (int num10 = 0; num10 < visiblePoints.Count; num10++)
		{
			double num11 = Math.Abs(visiblePoints[num10].Y) / num7 * 360.0;
			double num12 = num9 + num11;
			if (num8 >= num9 && num8 <= num12)
			{
				return new SegmentHit(visiblePoints[num10], num10);
			}
			num9 = num12;
		}
		return null;
	}

	private SegmentHit? FindPolarSegmentDataPoint(int x, int y, Rectangle chartRect)
	{
		List<ChartDataPoint> visiblePoints = GetVisiblePoints();
		if (visiblePoints.Count == 0)
		{
			return null;
		}
		int num = chartRect.X + chartRect.Width / 2;
		int num2 = chartRect.Y + chartRect.Height / 2;
		int num3 = Math.Min(chartRect.Width, chartRect.Height) / 2 - 40;
		double num4 = visiblePoints.Max((ChartDataPoint p) => Math.Abs(p.Y));
		if (num4 == 0.0)
		{
			return null;
		}
		int num5 = x - num;
		int num6 = y - num2;
		double num7 = Math.Sqrt(num5 * num5 + num6 * num6);
		if (num7 > (double)num3)
		{
			return null;
		}
		double num8 = Math.Atan2(num6, num5) * 180.0 / Math.PI;
		if (num8 < -90.0)
		{
			num8 += 360.0;
		}
		double num9 = 360.0 / (double)visiblePoints.Count;
		for (int num10 = 0; num10 < visiblePoints.Count; num10++)
		{
			double num11 = -90.0 + num9 * (double)num10;
			double num12 = num11 + num9;
			double num13 = Math.Abs(visiblePoints[num10].Y) / num4 * (double)num3;
			if (num8 >= num11 && num8 <= num12 && num7 <= num13)
			{
				return new SegmentHit(visiblePoints[num10], num10);
			}
		}
		return null;
	}

	private float GetSelectedSegmentProgress(ChartDataPoint point)
	{
		if (selectedSegmentPoint == point)
		{
			return segmentAnimationProgress;
		}
		if (previousSelectedSegmentPoint == point)
		{
			return 1f - segmentAnimationProgress;
		}
		return 0f;
	}

	private Rectangle GetSegmentAnimationBounds(Rectangle chartRect)
	{
		Rectangle rectangle = Rectangle.Empty;
		if (selectedSegmentPoint != null && selectedSegmentIndex >= 0)
		{
			rectangle = Rectangle.Union(rectangle, GetSegmentBounds(chartRect, selectedSegmentIndex));
		}
		if (previousSelectedSegmentPoint != null && previousSelectedSegmentIndex >= 0)
		{
			rectangle = Rectangle.Union(rectangle, GetSegmentBounds(chartRect, previousSelectedSegmentIndex));
		}
		if (rectangle.IsEmpty)
		{
			return chartRect;
		}
		int num = (int)(40f * Dpi);
		rectangle.Inflate(num, num);
		rectangle.Intersect(ClientRectangle);
		return rectangle;
	}

	private Rectangle GetSegmentBounds(Rectangle chartRect, int index)
	{
		if (index < 0)
		{
			return Rectangle.Empty;
		}
		int num = chartRect.X + chartRect.Width / 2;
		int num2 = chartRect.Y + chartRect.Height / 2;
		int num3 = Math.Min(chartRect.Width, chartRect.Height) / 2 - ((ChartType == TChartType.PolarArea) ? 40 : 20);
		if (num3 <= 0)
		{
			return Rectangle.Empty;
		}
		List<ChartDataPoint> visiblePoints = GetVisiblePoints();
		if (index >= visiblePoints.Count)
		{
			return Rectangle.Empty;
		}
		double num5;
		double num6;
		if (ChartType == TChartType.PolarArea)
		{
			double num4 = visiblePoints.Max((ChartDataPoint p) => Math.Abs(p.Y));
			if (num4 == 0.0)
			{
				return Rectangle.Empty;
			}
			num5 = 360.0 / (double)visiblePoints.Count;
			num6 = -90.0 + num5 * (double)index;
			num3 = (int)(Math.Abs(visiblePoints[index].Y) / num4 * (double)num3);
		}
		else
		{
			double num7 = visiblePoints.Sum((ChartDataPoint p) => Math.Abs(p.Y));
			if (num7 == 0.0)
			{
				return Rectangle.Empty;
			}
			num6 = -90.0;
			for (int num8 = 0; num8 < index; num8++)
			{
				num6 += Math.Abs(visiblePoints[num8].Y) / num7 * 360.0;
			}
			num5 = Math.Abs(visiblePoints[index].Y) / num7 * 360.0;
		}
		num3 = Math.Max(1, num3);
		double num9 = num6 + num5;
		double minX = num;
		double maxX = num;
		double minY = num2;
		double maxY = num2;
		AddSegmentBoundsPoint(num6, num3, num, num2, ref minX, ref minY, ref maxX, ref maxY);
		AddSegmentBoundsPoint(num9, num3, num, num2, ref minX, ref minY, ref maxX, ref maxY);
		for (double num10 = Math.Ceiling(num6 / 90.0) * 90.0; num10 <= num9; num10 += 90.0)
		{
			AddSegmentBoundsPoint(num10, num3, num, num2, ref minX, ref minY, ref maxX, ref maxY);
		}
		return Rectangle.FromLTRB((int)Math.Floor(minX), (int)Math.Floor(minY), (int)Math.Ceiling(maxX), (int)Math.Ceiling(maxY));
	}

	private static void AddSegmentBoundsPoint(double angle, int radius, int centerX, int centerY, ref double minX, ref double minY, ref double maxX, ref double maxY)
	{
		double num = angle * Math.PI / 180.0;
		double val = (double)centerX + (double)radius * Math.Cos(num);
		double val2 = (double)centerY + (double)radius * Math.Sin(num);
		minX = Math.Min(minX, val);
		minY = Math.Min(minY, val2);
		maxX = Math.Max(maxX, val);
		maxY = Math.Max(maxY, val2);
	}

	private List<ChartDataPoint> GetVisiblePoints()
	{
		List<ChartDataPoint> list = new List<ChartDataPoint>();
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible))
		{
			list.AddRange(item.DataPoints.Where((ChartDataPoint p) => p.Visible));
		}
		return list;
	}

	private string GetPointValueText(ChartDataPoint point)
	{
		if (!string.IsNullOrEmpty(point.Label))
		{
			return $"{point.Label}: {point.Y:F2}";
		}
		return point.Y.ToString("F2");
	}

	private void DrawValueLabel(Canvas g, string text, float x, float y)
	{
		Color color = (Config.IsDark ? Color.White : Color.Black);
		Color color2 = (Config.IsDark ? Color.FromArgb(220, 45, 45, 45) : Color.FromArgb(220, 255, 255, 255));
		Size size = g.MeasureString(text, Font);
		Rectangle rect = new Rectangle((int)(x - (float)(size.Width / 2) - 4f), (int)(y - (float)(size.Height / 2) - 2f), size.Width + 8, size.Height + 4);
		using SolidBrush brush = new SolidBrush(color2);
		using SolidBrush brush2 = new SolidBrush(color);
		g.Fill(brush, rect);
		g.String(text, Font, brush2, rect.X + 4, rect.Y + 2);
	}

	private void DrawChartData(Canvas g, Rectangle chartRect)
	{
		switch (ChartType)
		{
		case TChartType.Line:
			DrawLineChart(g, chartRect);
			break;
		case TChartType.Bar:
			DrawBarChart(g, chartRect);
			break;
		case TChartType.Area:
			DrawAreaChart(g, chartRect);
			break;
		case TChartType.Pie:
			DrawPieChart(g, chartRect);
			break;
		case TChartType.Doughnut:
			DrawDoughnutChart(g, chartRect);
			break;
		case TChartType.Scatter:
			DrawScatterChart(g, chartRect);
			break;
		case TChartType.Bubble:
			DrawBubbleChart(g, chartRect);
			break;
		case TChartType.Radar:
			DrawRadarChart(g, chartRect);
			break;
		case TChartType.PolarArea:
			DrawPolarAreaChart(g, chartRect);
			break;
		case TChartType.Spline:
			DrawSplineChart(g, chartRect);
			break;
		case TChartType.SplineArea:
			DrawSplineAreaChart(g, chartRect);
			break;
		case TChartType.HorizontalBar:
			DrawHorizontalBarChart(g, chartRect);
			break;
		case TChartType.StackedBar:
			DrawStackedBarChart(g, chartRect);
			break;
		case TChartType.StackedHorizontalBar:
			DrawStackedHorizontalBarChart(g, chartRect);
			break;
		case TChartType.SteppedLine:
			DrawSteppedLineChart(g, chartRect);
			break;
		case TChartType.SteppedArea:
			DrawSteppedAreaChart(g, chartRect);
			break;
		}
	}

	private ChartDataPoint? FindNearestDataPointForHover(int x, int y, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return null;
		}
		double num = double.MaxValue;
		ChartDataPoint result = null;
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible))
		{
			for (int num2 = 0; num2 < item.DataPoints.Count; num2++)
			{
				ChartDataPoint chartDataPoint = item.DataPoints[num2];
				if (chartDataPoint.Visible)
				{
					Point dataPointLocation = GetDataPointLocation(chartDataPoint, chartRect);
					double num3 = Math.Sqrt(Math.Pow(x - dataPointLocation.X, 2.0) + Math.Pow(y - dataPointLocation.Y, 2.0));
					if (num3 < num && num3 < 10.0)
					{
						num = num3;
						result = chartDataPoint;
					}
				}
			}
		}
		return result;
	}

	private ChartDataPoint? FindNearestDataPoint(int x, int y, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return null;
		}
		double num = double.MaxValue;
		ChartDataPoint chartDataPoint = null;
		int datasetIndex = -1;
		int pointIndex = -1;
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible))
		{
			for (int num2 = 0; num2 < item.DataPoints.Count; num2++)
			{
				ChartDataPoint chartDataPoint2 = item.DataPoints[num2];
				if (chartDataPoint2.Visible)
				{
					Point dataPointLocation = GetDataPointLocation(chartDataPoint2, chartRect);
					double num3 = Math.Sqrt(Math.Pow(x - dataPointLocation.X, 2.0) + Math.Pow(y - dataPointLocation.Y, 2.0));
					if (num3 < num && num3 < 10.0)
					{
						num = num3;
						chartDataPoint = chartDataPoint2;
						datasetIndex = Datasets.IndexOf(item);
						pointIndex = num2;
					}
				}
			}
		}
		if (chartDataPoint != null)
		{
			PointClick?.Invoke(this, new ChartPointClickEventArgs(chartDataPoint, datasetIndex, pointIndex));
		}
		return chartDataPoint;
	}

	private Point GetDataPointLocation(ChartDataPoint point, Rectangle chartRect)
	{
		switch (ChartType)
		{
		case TChartType.Area:
		case TChartType.Line:
		case TChartType.Spline:
		case TChartType.SplineArea:
		case TChartType.SteppedArea:
		case TChartType.SteppedLine:
			return GetLineChartPointLocation(point, chartRect);
		case TChartType.Bar:
		case TChartType.HorizontalBar:
		case TChartType.StackedBar:
		case TChartType.StackedHorizontalBar:
			return GetBarChartPointLocation(point, chartRect);
		case TChartType.Bubble:
		case TChartType.Scatter:
			return GetScatterChartPointLocation(point, chartRect);
		case TChartType.Doughnut:
		case TChartType.Pie:
			return GetPieChartPointLocation(point, chartRect);
		case TChartType.PolarArea:
			return GetPolarAreaChartPointLocation(point, chartRect);
		case TChartType.Radar:
			return GetRadarChartPointLocation(point, chartRect);
		default:
			return new Point(chartRect.X + chartRect.Width / 2, chartRect.Y + chartRect.Height / 2);
		}
	}

	private Point GetLineChartPointLocation(ChartDataPoint point, Rectangle chartRect)
	{
		double xRange = GetXRange();
		double yRange = GetYRange();
		if (xRange == 0.0 || yRange == 0.0)
		{
			return new Point(chartRect.X, chartRect.Y);
		}
		float num = (float)chartRect.X + (float)((point.X - GetMinX()) / xRange * (double)chartRect.Width);
		float num2 = (float)chartRect.Bottom - (float)((point.Y - GetMinY()) / yRange * (double)chartRect.Height);
		return new Point((int)num, (int)num2);
	}

	private Point GetBarChartPointLocation(ChartDataPoint point, Rectangle chartRect)
	{
		double xRange = GetXRange();
		double yRange = GetYRange();
		if (xRange == 0.0 || yRange == 0.0)
		{
			return new Point(chartRect.X, chartRect.Y);
		}
		float num = (float)chartRect.X + (float)((point.X - GetMinX()) / xRange * (double)chartRect.Width);
		float num2 = (float)chartRect.Bottom - (float)((point.Y - GetMinY()) / yRange * (double)chartRect.Height);
		return new Point((int)num, (int)num2);
	}

	private Point GetScatterChartPointLocation(ChartDataPoint point, Rectangle chartRect)
	{
		double xRange = GetXRange();
		double yRange = GetYRange();
		if (xRange == 0.0 || yRange == 0.0)
		{
			return new Point(chartRect.X, chartRect.Y);
		}
		float num = (float)chartRect.X + (float)((point.X - GetMinX()) / xRange * (double)chartRect.Width);
		float num2 = (float)chartRect.Bottom - (float)((point.Y - GetMinY()) / yRange * (double)chartRect.Height);
		return new Point((int)num, (int)num2);
	}

	private Point GetPieChartPointLocation(ChartDataPoint point, Rectangle chartRect)
	{
		int num = chartRect.X + chartRect.Width / 2;
		int num2 = chartRect.Y + chartRect.Height / 2;
		int num3 = Math.Min(chartRect.Width, chartRect.Height) / 2 - 20;
		int num4 = 0;
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible))
		{
			for (int num5 = 0; num5 < item.DataPoints.Count; num5++)
			{
				if (item.DataPoints[num5] == point)
				{
					int num6 = Datasets.Where((ChartDataset d) => d.Visible).Sum((ChartDataset d) => d.DataPoints.Count);
					double num7 = Math.PI * 2.0 * (double)num4 / (double)num6 - Math.PI / 2.0;
					int num8 = num + (int)((double)num3 * Math.Cos(num7));
					int num9 = num2 + (int)((double)num3 * Math.Sin(num7));
					return new Point(num8, num9);
				}
				num4++;
			}
		}
		return new Point(num, num2);
	}

	private Point GetPolarAreaChartPointLocation(ChartDataPoint point, Rectangle chartRect)
	{
		int num = chartRect.X + chartRect.Width / 2;
		int num2 = chartRect.Y + chartRect.Height / 2;
		int num3 = Math.Min(chartRect.Width, chartRect.Height) / 2 - 40;
		List<ChartDataPoint> list = new List<ChartDataPoint>();
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible))
		{
			foreach (ChartDataPoint item2 in item.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				list.Add(item2);
			}
		}
		if (list.Count == 0)
		{
			return new Point(num, num2);
		}
		int num4 = list.IndexOf(point);
		if (num4 == -1)
		{
			return new Point(num, num2);
		}
		double num5 = list.Max((ChartDataPoint p) => Math.Abs(p.Y));
		if (num5 == 0.0)
		{
			return new Point(num, num2);
		}
		float num6 = 360f / (float)list.Count;
		float num7 = -90f + num6 * (float)num4 + num6 / 2f;
		int num8 = (int)(Math.Abs(point.Y) / num5 * (double)num3 * 0.7);
		int num9 = num + (int)((double)num8 * Math.Cos((double)num7 * Math.PI / 180.0));
		int num10 = num2 + (int)((double)num8 * Math.Sin((double)num7 * Math.PI / 180.0));
		return new Point(num9, num10);
	}

	private Point GetRadarChartPointLocation(ChartDataPoint point, Rectangle chartRect)
	{
		int num = chartRect.X + chartRect.Width / 2;
		int num2 = chartRect.Y + chartRect.Height / 2;
		int num3 = Math.Min(chartRect.Width, chartRect.Height) / 2 - 40;
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible && d.DataPoints.Count > 0))
		{
			List<ChartDataPoint> list = item.DataPoints.Where((ChartDataPoint p) => p.Visible).ToList();
			int num4 = list.IndexOf(point);
			if (num4 != -1)
			{
				double num5 = list.Max((ChartDataPoint p) => Math.Abs(p.Y));
				if (num5 == 0.0)
				{
					return new Point(num, num2);
				}
				int num6 = Math.Max(3, GetRadarAxisCount());
				double num7 = (double)num4 * (360.0 / (double)num6) - 90.0;
				double num8 = Math.Abs(point.Y) / num5;
				double num9 = (double)num3 * num8;
				int num10 = num + (int)(num9 * Math.Cos(num7 * Math.PI / 180.0));
				int num11 = num2 + (int)(num9 * Math.Sin(num7 * Math.PI / 180.0));
				return new Point(num10, num11);
			}
		}
		return new Point(num, num2);
	}

	private double GetMinX()
	{
		if (XMin.HasValue)
		{
			return XMin.Value;
		}
		if (Datasets.Count == 0)
		{
			return 0.0;
		}
		IEnumerable<ChartDataset> source = Datasets.Where((ChartDataset d) => d.Visible && d.DataPoints.Count > 0);
		if (!source.Any())
		{
			return 0.0;
		}
		return source.Min((ChartDataset d) => d.DataPoints.Where((ChartDataPoint p) => p.Visible).Min((ChartDataPoint p) => p.X));
	}

	private double GetMaxX()
	{
		if (XMax.HasValue)
		{
			return XMax.Value;
		}
		if (Datasets.Count == 0)
		{
			return 1.0;
		}
		IEnumerable<ChartDataset> source = Datasets.Where((ChartDataset d) => d.Visible && d.DataPoints.Count > 0);
		if (!source.Any())
		{
			return 1.0;
		}
		return source.Max((ChartDataset d) => d.DataPoints.Where((ChartDataPoint p) => p.Visible).Max((ChartDataPoint p) => p.X));
	}

	private double GetStackedMaxX()
	{
		if (XMax.HasValue)
		{
			return XMax.Value;
		}
		List<ChartDataset> list = Datasets.Where((ChartDataset d) => d.Visible).ToList();
		if (list.Count == 0)
		{
			return 1.0;
		}
		int num = list.Max((ChartDataset d) => d.DataPoints.Count((ChartDataPoint p) => p.Visible));
		if (num == 0)
		{
			return 1.0;
		}
		double num2 = 0.0;
		for (int num3 = 0; num3 < num; num3++)
		{
			double num4 = 0.0;
			foreach (ChartDataset item in list)
			{
				List<ChartDataPoint> list2 = item.DataPoints.Where((ChartDataPoint p) => p.Visible).ToList();
				if (num3 < list2.Count)
				{
					num4 += Math.Max(0.0, list2[num3].X);
				}
			}
			num2 = Math.Max(num2, num4);
		}
		if (!(num2 <= 0.0))
		{
			return num2;
		}
		return 1.0;
	}

	private double GetMinY()
	{
		if (YMin.HasValue)
		{
			return YMin.Value;
		}
		return 0.0;
	}

	private double GetMaxY()
	{
		if (YMax.HasValue)
		{
			return YMax.Value;
		}
		if (Datasets.Count == 0)
		{
			return 1.0;
		}
		IEnumerable<ChartDataset> source = Datasets.Where((ChartDataset d) => d.Visible && d.DataPoints.Count > 0);
		if (!source.Any())
		{
			return 1.0;
		}
		return source.Max((ChartDataset d) => d.DataPoints.Where((ChartDataPoint p) => p.Visible).Max((ChartDataPoint p) => p.Y));
	}

	private double GetStackedMaxY()
	{
		if (YMax.HasValue)
		{
			return YMax.Value;
		}
		List<ChartDataset> list = Datasets.Where((ChartDataset d) => d.Visible).ToList();
		if (list.Count == 0)
		{
			return 1.0;
		}
		int num = list.Max((ChartDataset d) => d.DataPoints.Count((ChartDataPoint p) => p.Visible));
		if (num == 0)
		{
			return 1.0;
		}
		double num2 = 0.0;
		for (int num3 = 0; num3 < num; num3++)
		{
			double num4 = 0.0;
			foreach (ChartDataset item in list)
			{
				List<ChartDataPoint> list2 = item.DataPoints.Where((ChartDataPoint p) => p.Visible).ToList();
				if (num3 < list2.Count)
				{
					num4 += Math.Max(0.0, list2[num3].Y);
				}
			}
			num2 = Math.Max(num2, num4);
		}
		if (!(num2 <= 0.0))
		{
			return num2;
		}
		return 1.0;
	}

	private double GetXRange()
	{
		double num = GetMaxX() - GetMinX();
		if (num != 0.0)
		{
			return num;
		}
		return 1.0;
	}

	private double GetYRange()
	{
		double num = GetMaxY() - GetMinY();
		if (num != 0.0)
		{
			return num;
		}
		return 1.0;
	}

	/// <summary>
	/// 统一的DPI缩放处理方法
	/// </summary>
	/// <param name="value">原始值</param>
	/// <returns>缩放后的值</returns>
	private float GetScaledValue(float value)
	{
		return value * Dpi;
	}

	/// <summary>
	/// 统一的DPI缩放处理方法（整数版本）
	/// </summary>
	/// <param name="value">原始值</param>
	/// <returns>缩放后的值</returns>
	private int GetScaledValue(int value)
	{
		return (int)((float)value * Dpi);
	}

	private void DrawLineChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double xRange = GetXRange();
		double yRange = GetYRange();
		if (xRange == 0.0 || yRange == 0.0)
		{
			return;
		}
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible && d.DataPoints.Count > 1))
		{
			List<Point> list = new List<Point>();
			foreach (ChartDataPoint item2 in item.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				float val = (float)chartRect.X + (float)((item2.X - GetMinX()) / xRange * (double)chartRect.Width);
				float val2 = (float)chartRect.Bottom - (float)((item2.Y - GetMinY()) / yRange * (double)chartRect.Height * (double)AnimationProgress);
				val = Math.Max(chartRect.Left, Math.Min(chartRect.Right, val));
				val2 = Math.Max(chartRect.Top, Math.Min(chartRect.Bottom, val2));
				list.Add(new Point((int)val, (int)val2));
			}
			if (list.Count < 2)
			{
				continue;
			}
			Color color;
			if (item.BorderColor.HasValue)
			{
				color = item.BorderColor.Value;
			}
			else
			{
				color = (item.FillColor.HasValue ? item.FillColor.Value : Color.Blue);
			}
			using (Pen pen = new Pen(color, GetScaledValue(item.BorderWidth)))
			{
				g.DrawLines(pen, list.ToArray());
			}
			using SolidBrush brush = new SolidBrush(item.FillColor.HasValue ? item.FillColor.Value : color);
			int scaledValue = GetScaledValue(3);
			int num = scaledValue * 2;
			foreach (Point item3 in list)
			{
				g.FillEllipse(brush, new Rectangle(item3.X - scaledValue, item3.Y - scaledValue, num, num));
			}
		}
	}

	private void DrawBarChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double minY = GetMinY();
		double yRange = GetYRange();
		if (yRange == 0.0)
		{
			return;
		}
		List<ChartDataset> list = Datasets.Where((ChartDataset d) => d.Visible).ToList();
		if (list.Count == 0)
		{
			return;
		}
		int num = list.Max((ChartDataset d) => d.DataPoints.Count((ChartDataPoint p) => p.Visible));
		if (num == 0)
		{
			return;
		}
		float num2 = (float)chartRect.Width / (float)num;
		float num3 = num2 * 0.8f;
		float num4 = num3 / (float)list.Count;
		for (int num5 = 0; num5 < list.Count; num5++)
		{
			ChartDataset chartDataset = list[num5];
			using SolidBrush brush = new SolidBrush(chartDataset.FillColor.HasValue ? chartDataset.FillColor.Value : Color.Blue);
			using Pen pen = new Pen(chartDataset.BorderColor.HasValue ? chartDataset.BorderColor.Value : Color.Black, GetScaledValue(chartDataset.BorderWidth));
			List<ChartDataPoint> list2 = chartDataset.DataPoints.Where((ChartDataPoint p) => p.Visible).ToList();
			for (int num6 = 0; num6 < list2.Count; num6++)
			{
				float num7 = (float)((list2[num6].Y - minY) / yRange * (double)chartRect.Height * (double)AnimationProgress);
				if (num7 > 0f)
				{
					float val = (float)chartRect.X + (float)num6 * num2 + (num2 - num3) / 2f + (float)num5 * num4;
					float val2 = (float)chartRect.Bottom - num7;
					val = Math.Max(chartRect.Left, Math.Min((float)chartRect.Right - num4, val));
					val2 = Math.Max(chartRect.Top, Math.Min(chartRect.Bottom, val2));
					num7 = Math.Min(num7, (float)chartRect.Bottom - val2);
					RectangleF rect = new RectangleF(val, val2, num4, num7);
					g.Fill(brush, rect);
					g.Draw(pen, rect);
				}
			}
		}
	}

	private void DrawAreaChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double xRange = GetXRange();
		double yRange = GetYRange();
		if (xRange == 0.0 || yRange == 0.0)
		{
			return;
		}
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible && d.DataPoints.Count > 1))
		{
			List<Point> list = new List<Point>();
			foreach (ChartDataPoint item2 in item.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				float num = (float)chartRect.X + (float)((item2.X - GetMinX()) / xRange * (double)chartRect.Width);
				float num2 = (float)chartRect.Bottom - (float)((item2.Y - GetMinY()) / yRange * (double)chartRect.Height * (double)AnimationProgress);
				list.Add(new Point((int)num, (int)num2));
			}
			if (list.Count >= 2)
			{
				list.Add(new Point(list[list.Count - 1].X, chartRect.Bottom));
				list.Add(new Point(list[0].X, chartRect.Bottom));
				Color color = (item.FillColor.HasValue ? item.FillColor.Value : Color.Blue);
				using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255f * item.Opacity), color)))
				{
					g.FillPolygon(brush, list.ToArray());
				}
				using Pen pen = new Pen(item.BorderColor.HasValue ? item.BorderColor.Value : color, item.BorderWidth * Dpi);
				g.DrawLines(pen, list.Take(list.Count - 2).ToArray());
			}
		}
	}

	private void DrawPieChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		int num = chartRect.X + chartRect.Width / 2;
		int num2 = chartRect.Y + chartRect.Height / 2;
		int num3 = Math.Min(chartRect.Width, chartRect.Height) / 2 - 20;
		double num4 = 0.0;
		List<ChartDataPoint> list = new List<ChartDataPoint>();
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible))
		{
			foreach (ChartDataPoint item2 in item.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				list.Add(item2);
				num4 += Math.Abs(item2.Y);
			}
		}
		if (num4 == 0.0)
		{
			return;
		}
		float num5 = -90f;
		Color[] array = new Color[8]
		{
			Color.Red,
			Color.Blue,
			Color.Green,
			Color.Yellow,
			Color.Orange,
			Color.Purple,
			Color.Pink,
			Color.Cyan
		};
		Color[] array2 = PieColors ?? array;
		for (int num6 = 0; num6 < list.Count; num6++)
		{
			ChartDataPoint chartDataPoint = list[num6];
			float num7 = (float)(Math.Abs(chartDataPoint.Y) / num4 * 360.0);
			float num8 = num5 + num7 / 2f;
			float selectedSegmentProgress = GetSelectedSegmentProgress(chartDataPoint);
			bool flag = selectedSegmentPoint == chartDataPoint;
			int num9 = (int)(10f * Dpi * selectedSegmentProgress);
			int num10 = num + (int)((double)num9 * Math.Cos((double)num8 * Math.PI / 180.0));
			int num11 = num2 + (int)((double)num9 * Math.Sin((double)num8 * Math.PI / 180.0));
			using (SolidBrush brush = new SolidBrush(chartDataPoint.Color.HasValue ? chartDataPoint.Color.Value : array2[num6 % array2.Length]))
			{
				using Pen pen = new Pen(Color.White, 2f);
				Rectangle rect = new Rectangle(num10 - num3, num11 - num3, num3 * 2, num3 * 2);
				g.FillPie(brush, rect, num5, num7);
				g.DrawPie(pen, rect, num5, num7);
			}
			if (!string.IsNullOrEmpty(chartDataPoint.Label))
			{
				int num12 = (int)((float)num3 * 0.7f);
				int num13 = num10 + (int)((double)num12 * Math.Cos((double)num8 * Math.PI / 180.0));
				int num14 = num11 + (int)((double)num12 * Math.Sin((double)num8 * Math.PI / 180.0));
				using SolidBrush brush2 = new SolidBrush(Config.IsDark ? Color.White : Color.Black);
				Size size = g.MeasureString(chartDataPoint.Label, Font);
				g.String(chartDataPoint.Label, Font, brush2, num13 - size.Width / 2, num14 - size.Height / 2);
			}
			if (flag)
			{
				int num15 = num3 + (int)(20f * Dpi);
				int num16 = num10 + (int)((double)num15 * Math.Cos((double)num8 * Math.PI / 180.0));
				int num17 = num11 + (int)((double)num15 * Math.Sin((double)num8 * Math.PI / 180.0));
				DrawValueLabel(g, GetPointValueText(chartDataPoint), num16, num17);
			}
			num5 += num7;
		}
	}

	private void DrawDoughnutChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		int num = chartRect.X + chartRect.Width / 2;
		int num2 = chartRect.Y + chartRect.Height / 2;
		int num3 = Math.Min(chartRect.Width, chartRect.Height) / 2 - 20;
		float num4 = (float)num3 * 0.6f;
		double num5 = 0.0;
		List<ChartDataPoint> list = new List<ChartDataPoint>();
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible))
		{
			foreach (ChartDataPoint item2 in item.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				list.Add(item2);
				num5 += Math.Abs(item2.Y);
			}
		}
		if (num5 == 0.0)
		{
			return;
		}
		float num6 = -90f;
		Color[] array = new Color[8]
		{
			Color.Red,
			Color.Blue,
			Color.Green,
			Color.Yellow,
			Color.Orange,
			Color.Purple,
			Color.Pink,
			Color.Cyan
		};
		Color[] array2 = PieColors ?? array;
		for (int num7 = 0; num7 < list.Count; num7++)
		{
			ChartDataPoint chartDataPoint = list[num7];
			float num8 = (float)(Math.Abs(chartDataPoint.Y) / num5 * 360.0);
			float num9 = num6 + num8 / 2f;
			float selectedSegmentProgress = GetSelectedSegmentProgress(chartDataPoint);
			bool flag = selectedSegmentPoint == chartDataPoint;
			int num10 = (int)(10f * Dpi * selectedSegmentProgress);
			int num11 = num + (int)((double)num10 * Math.Cos((double)num9 * Math.PI / 180.0));
			int num12 = num2 + (int)((double)num10 * Math.Sin((double)num9 * Math.PI / 180.0));
			using (SolidBrush brush = new SolidBrush(chartDataPoint.Color.HasValue ? chartDataPoint.Color.Value : array2[num7 % array2.Length]))
			{
				using Pen pen = new Pen(Color.White, 2f);
				Rectangle rect = new Rectangle(num11 - num3, num12 - num3, num3 * 2, num3 * 2);
				g.FillPie(brush, rect, num6, num8);
				Rectangle rect2 = new Rectangle((int)((float)num11 - num4), (int)((float)num12 - num4), (int)(num4 * 2f), (int)(num4 * 2f));
				using (SolidBrush brush2 = new SolidBrush(Config.IsDark ? Color.FromArgb(30, 30, 30) : BackColor))
				{
					g.FillPie(brush2, rect2, num6, num8);
				}
				g.DrawArc(pen, rect, num6, num8);
				g.DrawArc(pen, rect2, num6, num8);
			}
			if (!string.IsNullOrEmpty(chartDataPoint.Label))
			{
				float num13 = ((float)num3 + num4) / 2f;
				int num14 = num11 + (int)((double)num13 * Math.Cos((double)num9 * Math.PI / 180.0));
				int num15 = num12 + (int)((double)num13 * Math.Sin((double)num9 * Math.PI / 180.0));
				using SolidBrush brush3 = new SolidBrush(Config.IsDark ? Color.White : Color.Black);
				Size size = g.MeasureString(chartDataPoint.Label, Font);
				g.String(chartDataPoint.Label, Font, brush3, num14 - size.Width / 2, num15 - size.Height / 2);
			}
			if (flag)
			{
				int num16 = num3 + (int)(20f * Dpi);
				int num17 = num11 + (int)((double)num16 * Math.Cos((double)num9 * Math.PI / 180.0));
				int num18 = num12 + (int)((double)num16 * Math.Sin((double)num9 * Math.PI / 180.0));
				DrawValueLabel(g, GetPointValueText(chartDataPoint), num17, num18);
			}
			num6 += num8;
		}
	}

	private void DrawScatterChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double xRange = GetXRange();
		double yRange = GetYRange();
		if (xRange == 0.0 || yRange == 0.0)
		{
			return;
		}
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible))
		{
			Color color = (item.FillColor.HasValue ? item.FillColor.Value : Color.Blue);
			Color color2 = (item.BorderColor.HasValue ? item.BorderColor.Value : Color.Black);
			int num = 6;
			using SolidBrush brush = new SolidBrush(color);
			using Pen pen = new Pen(color2, item.BorderWidth * Dpi);
			foreach (ChartDataPoint item2 in item.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				float num2 = (float)chartRect.X + (float)((item2.X - GetMinX()) / xRange * (double)chartRect.Width);
				float num3 = (float)chartRect.Bottom - (float)((item2.Y - GetMinY()) / yRange * (double)chartRect.Height);
				RectangleF rect = new RectangleF(num2 - (float)(num / 2), num3 - (float)(num / 2), num, num);
				g.FillEllipse(brush, rect);
				g.DrawEllipse(pen, rect);
			}
		}
	}

	private void DrawBubbleChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double xRange = GetXRange();
		double yRange = GetYRange();
		double radiusRange = GetRadiusRange();
		if (xRange == 0.0 || yRange == 0.0)
		{
			return;
		}
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible))
		{
			Color baseColor = (item.FillColor.HasValue ? item.FillColor.Value : Color.Blue);
			Color color = (item.BorderColor.HasValue ? item.BorderColor.Value : Color.Black);
			using SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255f * item.Opacity), baseColor));
			using Pen pen = new Pen(color, item.BorderWidth * Dpi);
			foreach (ChartDataPoint item2 in item.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				float num = (float)chartRect.X + (float)((item2.X - GetMinX()) / xRange * (double)chartRect.Width);
				float num2 = (float)chartRect.Bottom - (float)((item2.Y - GetMinY()) / yRange * (double)chartRect.Height);
				float val = ((radiusRange > 0.0) ? ((float)(item2.Radius / radiusRange * 20.0)) : 10f);
				val = Math.Max(5f, Math.Min(val, 30f));
				RectangleF rect = new RectangleF(num - val, num2 - val, val * 2f, val * 2f);
				g.FillEllipse(brush, rect);
				g.DrawEllipse(pen, rect);
			}
		}
	}

	private double GetRadiusRange()
	{
		if (Datasets.Count == 0)
		{
			return 0.0;
		}
		return Datasets.Where((ChartDataset d) => d.Visible).Max((ChartDataset d) => (d.DataPoints.Count <= 0) ? 0.0 : d.DataPoints.Max((ChartDataPoint p) => p.Radius));
	}

	private int GetRadarAxisCount()
	{
		List<ChartDataset> list = Datasets.Where((ChartDataset d) => d.Visible).ToList();
		if (list.Count == 0)
		{
			return 0;
		}
		return list.Max((ChartDataset d) => d.DataPoints.Count((ChartDataPoint p) => p.Visible));
	}

	private void DrawRadarChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		int num = chartRect.X + chartRect.Width / 2;
		int num2 = chartRect.Y + chartRect.Height / 2;
		int num3 = Math.Min(chartRect.Width, chartRect.Height) / 2 - 40;
		int radarAxisCount = GetRadarAxisCount();
		if (radarAxisCount < 3)
		{
			return;
		}
		int num4 = 5;
		using (Pen pen = new Pen(GridColor.HasValue ? GridColor.Value : Color.LightGray, 1f))
		{
			for (int i = 1; i <= num4; i++)
			{
				int num5 = num3 * i / num4;
				Point[] array = new Point[radarAxisCount];
				for (int j = 0; j < radarAxisCount; j++)
				{
					double num6 = (double)j * (360.0 / (double)radarAxisCount) - 90.0;
					int num7 = num + (int)((double)num5 * Math.Cos(num6 * Math.PI / 180.0));
					int num8 = num2 + (int)((double)num5 * Math.Sin(num6 * Math.PI / 180.0));
					array[j] = new Point(num7, num8);
				}
				g.DrawPolygon(pen, array);
			}
			for (int k = 0; k < radarAxisCount; k++)
			{
				double num9 = (double)k * (360.0 / (double)radarAxisCount) - 90.0;
				int x = num + (int)((double)num3 * Math.Cos(num9 * Math.PI / 180.0));
				int y = num2 + (int)((double)num3 * Math.Sin(num9 * Math.PI / 180.0));
				g.DrawLine(pen, num, num2, x, y);
			}
		}
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible && d.DataPoints.Count > 0))
		{
			List<ChartDataPoint> list = item.DataPoints.Where((ChartDataPoint p) => p.Visible).ToList();
			if (list.Count == 0)
			{
				continue;
			}
			double num10 = list.Max((ChartDataPoint p) => Math.Abs(p.Y));
			if (num10 == 0.0)
			{
				continue;
			}
			Point[] array2 = new Point[list.Count];
			for (int num11 = 0; num11 < list.Count; num11++)
			{
				ChartDataPoint chartDataPoint = list[num11];
				double num12 = (double)num11 * (360.0 / (double)radarAxisCount) - 90.0;
				double num13 = Math.Abs(chartDataPoint.Y) / num10;
				double num14 = (double)num3 * num13;
				int num15 = num + (int)(num14 * Math.Cos(num12 * Math.PI / 180.0));
				int num16 = num2 + (int)(num14 * Math.Sin(num12 * Math.PI / 180.0));
				array2[num11] = new Point(num15, num16);
			}
			Color color = (item.FillColor.HasValue ? item.FillColor.Value : Color.Blue);
			using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)((double)(255f * item.Opacity) * 0.3), color)))
			{
				g.FillPolygon(brush, array2);
			}
			using (Pen pen2 = new Pen(item.BorderColor.HasValue ? item.BorderColor.Value : color, item.BorderWidth * Dpi))
			{
				g.DrawPolygon(pen2, array2);
			}
			using SolidBrush brush2 = new SolidBrush(color);
			int num17 = (int)(3f * Dpi);
			int num18 = num17 * 2;
			for (int num19 = 0; num19 < array2.Length; num19++)
			{
				Point point = array2[num19];
				g.FillEllipse(brush2, new Rectangle(point.X - num17, point.Y - num17, num18, num18));
				if (ShowRadarValues && num19 < list.Count)
				{
					string text = list[num19].Y.ToString("F2");
					DrawValueLabel(g, text, point.X, point.Y - (int)(12f * Dpi));
				}
			}
		}
	}

	private void DrawPolarAreaChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		int num = chartRect.X + chartRect.Width / 2;
		int num2 = chartRect.Y + chartRect.Height / 2;
		int num3 = Math.Min(chartRect.Width, chartRect.Height) / 2 - 40;
		List<ChartDataPoint> list = new List<ChartDataPoint>();
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible))
		{
			foreach (ChartDataPoint item2 in item.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				list.Add(item2);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		double num4 = list.Max((ChartDataPoint p) => Math.Abs(p.Y));
		if (num4 == 0.0)
		{
			return;
		}
		float num5 = 360f / (float)list.Count;
		float num6 = -90f;
		Color[] array = new Color[8]
		{
			Color.Red,
			Color.Blue,
			Color.Green,
			Color.Yellow,
			Color.Orange,
			Color.Purple,
			Color.Pink,
			Color.Cyan
		};
		Color[] array2 = PieColors ?? array;
		for (int num7 = 0; num7 < list.Count; num7++)
		{
			ChartDataPoint chartDataPoint = list[num7];
			float num8 = num5;
			int val = (int)(Math.Abs(chartDataPoint.Y) / num4 * (double)num3);
			float num9 = num6 + num8 / 2f;
			float selectedSegmentProgress = GetSelectedSegmentProgress(chartDataPoint);
			bool flag = selectedSegmentPoint == chartDataPoint;
			int num10 = (int)(10f * Dpi * selectedSegmentProgress);
			int num11 = num + (int)((double)num10 * Math.Cos((double)num9 * Math.PI / 180.0));
			int num12 = num2 + (int)((double)num10 * Math.Sin((double)num9 * Math.PI / 180.0));
			val = Math.Max(1, val);
			Color color = (chartDataPoint.Color.HasValue ? chartDataPoint.Color.Value : array2[num7 % array2.Length]);
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(178, color)))
			{
				using Pen pen = new Pen(color, 2f);
				Rectangle rect = new Rectangle(num11 - val, num12 - val, val * 2, val * 2);
				if (rect.Width > 0 && rect.Height > 0)
				{
					g.FillPie(brush, rect, num6, num8);
					g.DrawPie(pen, rect, num6, num8);
				}
			}
			if (!string.IsNullOrEmpty(chartDataPoint.Label))
			{
				int num13 = Math.Max(1, (int)((float)val * 0.7f));
				int num14 = num11 + (int)((double)num13 * Math.Cos((double)num9 * Math.PI / 180.0));
				int num15 = num12 + (int)((double)num13 * Math.Sin((double)num9 * Math.PI / 180.0));
				using SolidBrush brush2 = new SolidBrush(Config.IsDark ? Color.White : Color.Black);
				Size size = g.MeasureString(chartDataPoint.Label, Font);
				g.String(chartDataPoint.Label, Font, brush2, num14 - size.Width / 2, num15 - size.Height / 2);
			}
			if (flag)
			{
				int num16 = val + (int)(20f * Dpi);
				int num17 = num11 + (int)((double)num16 * Math.Cos((double)num9 * Math.PI / 180.0));
				int num18 = num12 + (int)((double)num16 * Math.Sin((double)num9 * Math.PI / 180.0));
				DrawValueLabel(g, GetPointValueText(chartDataPoint), num17, num18);
			}
			num6 += num8;
		}
	}

	private void DrawSplineChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double xRange = GetXRange();
		double yRange = GetYRange();
		if (xRange == 0.0 || yRange == 0.0)
		{
			return;
		}
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible && d.DataPoints.Count > 2))
		{
			List<PointF> list = new List<PointF>();
			foreach (ChartDataPoint item2 in item.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				float num = (float)chartRect.X + (float)((item2.X - GetMinX()) / xRange * (double)chartRect.Width);
				float num2 = (float)chartRect.Bottom - (float)((item2.Y - GetMinY()) / yRange * (double)chartRect.Height);
				list.Add(new PointF(num, num2));
			}
			if (list.Count < 3)
			{
				continue;
			}
			Color color;
			if (item.BorderColor.HasValue)
			{
				color = item.BorderColor.Value;
			}
			else
			{
				color = (item.FillColor.HasValue ? item.FillColor.Value : Color.Blue);
			}
			using (Pen pen = new Pen(color, item.BorderWidth * Dpi))
			{
				DrawSplineCurve(g, pen, list.ToArray());
			}
			using SolidBrush brush = new SolidBrush(item.FillColor.HasValue ? item.FillColor.Value : color);
			int num3 = (int)(3f * Dpi);
			int num4 = num3 * 2;
			foreach (PointF item3 in list)
			{
				g.FillEllipse(brush, new RectangleF(item3.X - (float)num3, item3.Y - (float)num3, num4, num4));
			}
		}
	}

	private void DrawSplineCurve(Canvas g, Pen pen, PointF[] points)
	{
		if (points.Length < 3)
		{
			return;
		}
		using GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddCurve(points, 0.5f);
		g.Draw(pen, graphicsPath);
	}

	private void DrawSplineAreaChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double xRange = GetXRange();
		double yRange = GetYRange();
		if (xRange == 0.0 || yRange == 0.0)
		{
			return;
		}
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible && d.DataPoints.Count > 2))
		{
			List<PointF> list = new List<PointF>();
			foreach (ChartDataPoint item2 in item.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				float num = (float)chartRect.X + (float)((item2.X - GetMinX()) / xRange * (double)chartRect.Width);
				float num2 = (float)chartRect.Bottom - (float)((item2.Y - GetMinY()) / yRange * (double)chartRect.Height);
				list.Add(new PointF(num, num2));
			}
			if (list.Count < 3)
			{
				continue;
			}
			list.Add(new PointF(list[list.Count - 1].X, chartRect.Bottom));
			list.Add(new PointF(list[0].X, chartRect.Bottom));
			Color color = item.FillColor ?? Color.Blue;
			using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255f * item.Opacity), color)))
			{
				using GraphicsPath graphicsPath = new GraphicsPath();
				graphicsPath.AddCurve(list.Take(list.Count - 2).ToArray(), 0.5f);
				graphicsPath.AddLine(list[list.Count - 3], list[list.Count - 2]);
				graphicsPath.AddLine(list[list.Count - 2], list[list.Count - 1]);
				graphicsPath.CloseFigure();
				g.Fill(brush, graphicsPath);
			}
			using Pen pen = new Pen(item.BorderColor.HasValue ? item.BorderColor.Value : color, item.BorderWidth * Dpi);
			using GraphicsPath graphicsPath2 = new GraphicsPath();
			graphicsPath2.AddCurve(list.Take(list.Count - 2).ToArray(), 0.5f);
			g.Draw(pen, graphicsPath2);
		}
	}

	private void DrawHorizontalBarChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double xRange = GetXRange();
		if (xRange == 0.0)
		{
			return;
		}
		List<ChartDataset> list = Datasets.Where((ChartDataset d) => d.Visible).ToList();
		if (list.Count == 0)
		{
			return;
		}
		int num = list.Max((ChartDataset d) => d.DataPoints.Count((ChartDataPoint p) => p.Visible));
		if (num == 0)
		{
			return;
		}
		float num2 = (float)chartRect.Height / (float)num;
		float num3 = num2 * 0.8f;
		float num4 = num3 / (float)list.Count;
		for (int num5 = 0; num5 < list.Count; num5++)
		{
			ChartDataset chartDataset = list[num5];
			using SolidBrush brush = new SolidBrush(chartDataset.FillColor ?? Color.Blue);
			using Pen pen = new Pen(chartDataset.BorderColor ?? Color.Black, chartDataset.BorderWidth * Dpi);
			List<ChartDataPoint> list2 = chartDataset.DataPoints.Where((ChartDataPoint p) => p.Visible).ToList();
			for (int num6 = 0; num6 < list2.Count; num6++)
			{
				float val = (float)((list2[num6].X - GetMinX()) / xRange * (double)chartRect.Width);
				int num7 = chartRect.X;
				float num8 = (float)chartRect.Y + (float)num6 * num2 + (num2 - num3) / 2f + (float)num5 * num4;
				val = Math.Max(0f, Math.Min(val, chartRect.Right - num7));
				RectangleF rect = new RectangleF(num7, num8, val, num4);
				g.Fill(brush, rect);
				g.Draw(pen, rect);
			}
		}
	}

	private void DrawStackedBarChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double num = GetStackedMaxY() - GetMinY();
		if (num == 0.0)
		{
			num = 1.0;
		}
		List<ChartDataset> list = Datasets.Where((ChartDataset d) => d.Visible).ToList();
		if (list.Count == 0)
		{
			return;
		}
		int num2 = list.Max((ChartDataset d) => d.DataPoints.Count((ChartDataPoint p) => p.Visible));
		if (num2 == 0)
		{
			return;
		}
		float num3 = (float)chartRect.Width / (float)num2;
		float num4 = num3 * 0.7f;
		for (int num5 = 0; num5 < num2; num5++)
		{
			float num6 = (float)chartRect.X + (float)num5 * num3 + (num3 - num4) / 2f;
			float num7 = chartRect.Bottom;
			foreach (ChartDataset item in list)
			{
				List<ChartDataPoint> list2 = item.DataPoints.Where((ChartDataPoint p) => p.Visible).ToList();
				if (num5 >= list2.Count)
				{
					continue;
				}
				float num8 = (float)(list2[num5].Y / num * (double)chartRect.Height * (double)AnimationProgress);
				float val = num7 - num8;
				val = Math.Max(chartRect.Top, val);
				num8 = num7 - val;
				using (SolidBrush brush = new SolidBrush(item.FillColor ?? Color.Blue))
				{
					using Pen pen = new Pen(item.BorderColor ?? Color.Black, item.BorderWidth * Dpi);
					RectangleF rect = new RectangleF(num6, val, num4, num8);
					g.Fill(brush, rect);
					g.Draw(pen, rect);
				}
				num7 = val;
			}
		}
	}

	private void DrawStackedHorizontalBarChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double num = GetStackedMaxX() - GetMinX();
		if (num == 0.0)
		{
			num = 1.0;
		}
		List<ChartDataset> list = Datasets.Where((ChartDataset d) => d.Visible).ToList();
		if (list.Count == 0)
		{
			return;
		}
		int num2 = list.Max((ChartDataset d) => d.DataPoints.Count((ChartDataPoint p) => p.Visible));
		if (num2 == 0)
		{
			return;
		}
		float num3 = (float)chartRect.Height / (float)num2;
		float num4 = num3 * 0.7f;
		for (int num5 = 0; num5 < num2; num5++)
		{
			float num6 = (float)chartRect.Y + (float)num5 * num3 + (num3 - num4) / 2f;
			float num7 = chartRect.X;
			foreach (ChartDataset item in list)
			{
				List<ChartDataPoint> list2 = item.DataPoints.Where((ChartDataPoint p) => p.Visible).ToList();
				if (num5 >= list2.Count)
				{
					continue;
				}
				float val = (float)(list2[num5].X / num * (double)chartRect.Width * (double)AnimationProgress);
				float num8 = num7;
				val = Math.Max(0f, Math.Min(val, (float)chartRect.Right - num8));
				using (SolidBrush brush = new SolidBrush(item.FillColor ?? Color.Blue))
				{
					using Pen pen = new Pen(item.BorderColor ?? Color.Black, item.BorderWidth * Dpi);
					RectangleF rect = new RectangleF(num8, num6, val, num4);
					g.Fill(brush, rect);
					g.Draw(pen, rect);
				}
				num7 += val;
			}
		}
	}

	private void DrawSteppedLineChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double xRange = GetXRange();
		double yRange = GetYRange();
		if (xRange == 0.0 || yRange == 0.0)
		{
			return;
		}
		foreach (ChartDataset item in Datasets.Where((ChartDataset d) => d.Visible && d.DataPoints.Count > 1))
		{
			List<Point> list = new List<Point>();
			foreach (ChartDataPoint item2 in item.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				float num = (float)chartRect.X + (float)((item2.X - GetMinX()) / xRange * (double)chartRect.Width);
				float num2 = (float)chartRect.Bottom - (float)((item2.Y - GetMinY()) / yRange * (double)chartRect.Height);
				list.Add(new Point((int)num, (int)num2));
			}
			if (list.Count < 2)
			{
				continue;
			}
			Color color;
			if (item.BorderColor.HasValue)
			{
				color = item.BorderColor.Value;
			}
			else
			{
				color = (item.FillColor.HasValue ? item.FillColor.Value : Color.Blue);
			}
			using (Pen pen = new Pen(color, item.BorderWidth * Dpi))
			{
				for (int num3 = 0; num3 < list.Count - 1; num3++)
				{
					Point point = list[num3];
					Point point2 = list[num3 + 1];
					g.DrawLine(pen, point.X, point.Y, point2.X, point.Y);
					g.DrawLine(pen, point2.X, point.Y, point2.X, point2.Y);
				}
			}
			using SolidBrush brush = new SolidBrush(item.FillColor.HasValue ? item.FillColor.Value : color);
			int num4 = (int)(3f * Dpi);
			int num5 = num4 * 2;
			foreach (Point item3 in list)
			{
				g.FillEllipse(brush, new Rectangle(item3.X - num4, item3.Y - num4, num5, num5));
			}
		}
	}

	private void DrawSteppedAreaChart(Canvas g, Rectangle chartRect)
	{
		if (Datasets.Count == 0)
		{
			return;
		}
		double xRange = GetXRange();
		double yRange = GetYRange();
		if (xRange == 0.0 || yRange == 0.0)
		{
			return;
		}
		foreach (ChartDataset item2 in Datasets.Where((ChartDataset d) => d.Visible && d.DataPoints.Count > 1))
		{
			List<Point> list = new List<Point>();
			foreach (ChartDataPoint item3 in item2.DataPoints.Where((ChartDataPoint p) => p.Visible))
			{
				float num = (float)chartRect.X + (float)((item3.X - GetMinX()) / xRange * (double)chartRect.Width);
				float num2 = (float)chartRect.Bottom - (float)((item3.Y - GetMinY()) / yRange * (double)chartRect.Height);
				list.Add(new Point((int)num, (int)num2));
			}
			if (list.Count >= 2)
			{
				List<Point> list2 = new List<Point>();
				for (int num3 = 0; num3 < list.Count - 1; num3++)
				{
					Point item = list[num3];
					Point point = list[num3 + 1];
					list2.Add(item);
					list2.Add(new Point(point.X, item.Y));
				}
				list2.Add(list[list.Count - 1]);
				list2.Add(new Point(list2[list2.Count - 1].X, chartRect.Bottom));
				list2.Add(new Point(list2[0].X, chartRect.Bottom));
				Color color = (item2.FillColor.HasValue ? item2.FillColor.Value : Color.Blue);
				using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255f * item2.Opacity), color)))
				{
					g.FillPolygon(brush, list2.ToArray());
				}
				using Pen pen = new Pen(item2.BorderColor.HasValue ? item2.BorderColor.Value : color, item2.BorderWidth * Dpi);
				g.DrawLines(pen, list2.Take(list2.Count - 2).ToArray());
			}
		}
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		HandleMouseClick(e.X, e.Y);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		mousePosition = new Point(e.X, e.Y);
		if (CursorMode != TChartCursor.None)
		{
			Invalidate();
		}
		if (ShowTooltip)
		{
			HandleMouseMove(e.X, e.Y);
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		mousePosition = null;
		HideTooltip();
		if (CursorMode != TChartCursor.None)
		{
			Invalidate();
		}
	}

	private void HandleMouseClick(int x, int y)
	{
		Rectangle chartRect = CalculateChartRect(ClientRectangle);
		if (!chartRect.Contains(x, y))
		{
			return;
		}
		if (IsSegmentChart())
		{
			SegmentHit segmentHit = FindSegmentDataPoint(x, y, chartRect);
			if (segmentHit != null)
			{
				if (selectedSegmentPoint != segmentHit.Point)
				{
					previousSelectedSegmentPoint = selectedSegmentPoint;
					previousSelectedSegmentIndex = selectedSegmentIndex;
					selectedSegmentPoint = segmentHit.Point;
					selectedSegmentIndex = segmentHit.Index;
					StartSegmentAnimation(chartRect);
					Invalidate(GetSegmentAnimationBounds(chartRect));
					PointClick?.Invoke(this, new ChartPointClickEventArgs(segmentHit.Point, 0, segmentHit.Index));
				}
				return;
			}
		}
		ChartDataPoint chartDataPoint = FindNearestDataPoint(x, y, chartRect);
		if (chartDataPoint != null)
		{
			PointClick?.Invoke(this, new ChartPointClickEventArgs(chartDataPoint));
		}
		else
		{
			AreaClick?.Invoke(this, new ChartAreaClickEventArgs(new Point(x, y)));
		}
	}

	private void HandleMouseMove(int x, int y)
	{
		if (!ShowTooltip)
		{
			return;
		}
		Rectangle chartRect = CalculateChartRect(ClientRectangle);
		if (chartRect.Contains(x, y))
		{
			if (EnablePieHoverValue && IsSegmentChart())
			{
				SegmentHit segmentHit = FindSegmentDataPoint(x, y, chartRect);
				if (segmentHit != null)
				{
					if (HoveredPoint != segmentHit.Point)
					{
						HoveredPoint = segmentHit.Point;
						ShowTooltipInternal(segmentHit.Point, x, y);
						PointHover?.Invoke(this, new ChartPointHoverEventArgs(segmentHit.Point, new Point(x, y)));
					}
					return;
				}
			}
			ChartDataPoint chartDataPoint = FindNearestDataPointForHover(x, y, chartRect);
			if (chartDataPoint != null)
			{
				if (HoveredPoint != chartDataPoint)
				{
					HoveredPoint = chartDataPoint;
					ShowTooltipInternal(chartDataPoint, x, y);
					PointHover?.Invoke(this, new ChartPointHoverEventArgs(chartDataPoint, new Point(x, y)));
				}
			}
			else if (HoveredPoint != null)
			{
				HoveredPoint = null;
				HideTooltip();
			}
		}
		else if (HoveredPoint != null)
		{
			HoveredPoint = null;
			HideTooltip();
		}
	}
}
