using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Progress 进度条
/// </summary>
/// <remarks>展示操作的当前进度。</remarks>
[Description("Progress 进度条")]
[ToolboxItem(true)]
[DefaultProperty("Value")]
public class Progress : IControl
{
	private Color? fore;

	private Color? back;

	private Color? fill;

	private string? fillExtend;

	private int radius;

	private TShapeProgress shape = TShapeProgress.Round;

	private string? text;

	private string? textUnit = "%";

	private bool useSystemText;

	private TType state;

	private float iconratio = 0.7f;

	private Image? icon;

	private string? iconSvg;

	private float valueratio = 0.4f;

	private float _value;

	private float _value_show;

	private bool loading;

	private float AnimationLoadingValue;

	private int loadingDuration = 1000;

	private float loadingOpacity = 1f;

	private Color? loadingColor;

	private int steps = 3;

	private int stepSize = 14;

	private int stepGap = 2;

	private AnimationTask? ThreadLoading;

	private AnimationTask? ThreadValue;

	private object? ownerTmp;

	private bool showInTaskbar;

	private bool canTaskbar;

	private ulong old_value;

	private ThumbnailProgressState old_state;

	private readonly FormatFlags s_c = FormatFlags.Center | FormatFlags.NoWrap;

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
	/// 进度条颜色
	/// </summary>
	[Description("进度条颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Fill
	{
		get
		{
			return fill;
		}
		set
		{
			if (!(fill == value))
			{
				fill = value;
				Invalidate();
				OnPropertyChanged("Fill");
			}
		}
	}

	/// <summary>
	/// 进度条渐变色
	/// </summary>
	[Description("进度条渐变色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? FillExtend
	{
		get
		{
			return fillExtend;
		}
		set
		{
			if (!(fillExtend == value))
			{
				fillExtend = value;
				Invalidate();
				OnPropertyChanged("FillExtend");
			}
		}
	}

	/// <summary>
	/// 圆角
	/// </summary>
	[Description("圆角")]
	[Category("Appearance")]
	[DefaultValue(0)]
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
	/// 形状
	/// </summary>
	[Description("形状")]
	[Category("Appearance")]
	[DefaultValue(TShapeProgress.Round)]
	public TShapeProgress Shape
	{
		get
		{
			return shape;
		}
		set
		{
			if (shape != value)
			{
				shape = value;
				Invalidate();
				OnPropertyChanged("Shape");
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
				text = value;
				if (useSystemText)
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
	/// 单位文本
	/// </summary>
	[Description("单位文本")]
	[Category("Appearance")]
	[DefaultValue("%")]
	[Localizable(true)]
	public string? TextUnit
	{
		get
		{
			return this.GetLangI(LocalizationTextUnit, textUnit);
		}
		set
		{
			if (!(textUnit == value))
			{
				textUnit = value;
				Invalidate();
				OnPropertyChanged("TextUnit");
			}
		}
	}

	[Description("单位文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationTextUnit { get; set; }

	/// <summary>
	/// 使用系统文本
	/// </summary>
	[Description("使用系统文本")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool UseSystemText
	{
		get
		{
			return useSystemText;
		}
		set
		{
			if (useSystemText != value)
			{
				useSystemText = value;
				Invalidate();
				OnPropertyChanged("UseSystemText");
			}
		}
	}

	/// <summary>
	/// 使文本居中显示
	/// </summary>
	[Description("使文本居中显示")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool UseTextCenter { get; set; }

	/// <summary>
	/// 显示进度文本小数点位数
	/// </summary>
	[Description("显示进度文本小数点位数")]
	[Category("Appearance")]
	[DefaultValue(0)]
	public int ShowTextDot { get; set; }

	/// <summary>
	/// 状态
	/// </summary>
	[Description("状态")]
	[Category("Appearance")]
	[DefaultValue(TType.None)]
	public TType State
	{
		get
		{
			return state;
		}
		set
		{
			if (state != value)
			{
				state = value;
				Invalidate();
				if (showInTaskbar)
				{
					ShowTaskbar();
				}
				OnPropertyChanged("State");
			}
		}
	}

	/// <summary>
	/// 仪表盘进度条缺口角度，可取值 0 ~ 295
	/// </summary>
	[Description("仪表盘进度条缺口角度，可取值 0 ~ 295")]
	[Category("Appearance")]
	[DefaultValue(0)]
	public int GapDegree { get; set; }

	/// <summary>
	/// 仪表盘进度条缺口位置
	/// </summary>
	[Description("仪表盘进度条缺口位置")]
	[Category("Appearance")]
	[DefaultValue(TAlignMini.Bottom)]
	public TAlignMini GapPlacement { get; set; } = TAlignMini.Bottom;

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
				Invalidate();
				OnPropertyChanged("IconRatio");
			}
		}
	}

	/// <summary>
	/// 圆形进度下的图标
	/// </summary>
	[Description("圆形进度下的图标")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Image? IconCircle
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
				if (shape == TShapeProgress.Circle)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 圆形进度下的图标SVG
	/// </summary>
	[Description("圆形进度下的图标SVG")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? IconSvgCircle
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
				if (shape == TShapeProgress.Circle)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 圆形图标是否旋转
	/// </summary>
	[Description("圆形图标是否旋转")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool IconCircleAngle { get; set; }

	/// <summary>
	/// 圆形图标边距
	/// </summary>
	[Description("圆形图标边距")]
	[Category("Appearance")]
	[DefaultValue(8)]
	public int IconCirclePadding { get; set; } = 8;

	/// <summary>
	/// 圆形图标颜色
	/// </summary>
	[Description("圆形图标颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? IconCircleColor { get; set; }

	/// <summary>
	/// 是否包含图片
	/// </summary>
	internal bool HasIcon
	{
		get
		{
			if (iconSvg == null)
			{
				return icon != null;
			}
			return true;
		}
	}

	/// <summary>
	/// 进度条比例
	/// </summary>
	[Description("进度条比例")]
	[Category("Appearance")]
	[DefaultValue(0.4f)]
	public float ValueRatio
	{
		get
		{
			return valueratio;
		}
		set
		{
			if (valueratio != value)
			{
				valueratio = value;
				Invalidate();
				OnPropertyChanged("ValueRatio");
			}
		}
	}

	/// <summary>
	/// 进度条
	/// </summary>
	[Description("进度条 0F-1F")]
	[Category("Data")]
	[DefaultValue(0f)]
	public float Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value == value)
			{
				return;
			}
			if (value < 0f)
			{
				value = 0f;
			}
			else if (value > 1f)
			{
				value = 1f;
			}
			_value = value;
			ThreadValue?.Dispose();
			ThreadValue = null;
			if (Config.HasAnimation("Progress", Name) && IsHandleCreated && Animation > 0)
			{
				int t = AntdUI.Animation.TotalFrames(10, Animation);
				if (_value > _value_show)
				{
					float s = _value_show;
					float v = Math.Abs(_value - s);
					ThreadValue = new AnimationTask((int i) =>
					{
						_value_show = s + AntdUI.Animation.Animate(i, t, v, AnimationType.Ball);
						Invalidate();
						return true;
					}, 10, t, () =>
					{
						_value_show = _value;
						Invalidate();
					});
				}
				else
				{
					float s2 = _value_show;
					float v2 = Math.Abs(_value_show - _value);
					ThreadValue = new AnimationTask((int i) =>
					{
						_value_show = s2 - AntdUI.Animation.Animate(i, t, v2, AnimationType.Ball);
						Invalidate();
						return true;
					}, 10, t, () =>
					{
						_value_show = _value;
						Invalidate();
					});
				}
			}
			else
			{
				_value_show = _value;
				Invalidate();
			}
			if (showInTaskbar)
			{
				ShowTaskbar();
			}
			OnPropertyChanged("Value");
		}
	}

	/// <summary>
	/// 加载状态
	/// </summary>
	[Description("加载状态")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Loading
	{
		get
		{
			return loading;
		}
		set
		{
			if (loading == value)
			{
				return;
			}
			loading = value;
			ThreadLoading?.Dispose();
			if (showInTaskbar)
			{
				ShowTaskbar(!loading);
			}
			if (loading)
			{
				ThreadLoading = new AnimationTask(new AnimationLoopConfig(this, () =>
				{
					AnimationLoadingValue = AnimationLoadingValue.Calculate(10f / (float)loadingDuration);
					if (AnimationLoadingValue > 1f)
					{
						AnimationLoadingValue = 0f;
						Invalidate();
						Thread.Sleep(1000);
					}
					Invalidate();
					return loading;
				}, 10).SetEnd(Invalidate).SetPriority());
			}
			else
			{
				Invalidate();
			}
			OnPropertyChanged("Loading");
		}
	}

	/// <summary>
	/// 动画铺满
	/// </summary>
	[Description("动画铺满")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool LoadingFull { get; set; }

	/// <summary>
	/// Loading 流动动画时长（毫秒）
	/// </summary>
	[Description("Loading 流动动画时长")]
	[Category("Appearance")]
	[DefaultValue(1000)]
	public int LoadingDuration
	{
		get
		{
			return loadingDuration;
		}
		set
		{
			if (value <= 0)
			{
				value = 1000;
			}
			if (loadingDuration != value)
			{
				loadingDuration = value;
				OnPropertyChanged("LoadingDuration");
			}
		}
	}

	/// <summary>
	/// Loading 流动光效不透明度
	/// </summary>
	[Description("Loading 流动光效不透明度")]
	[Category("Appearance")]
	[DefaultValue(1f)]
	public float LoadingOpacity
	{
		get
		{
			return loadingOpacity;
		}
		set
		{
			if (value <= 0f)
			{
				value = 1f;
			}
			if (loadingOpacity != value)
			{
				loadingOpacity = value;
				OnPropertyChanged("LoadingOpacity");
			}
		}
	}

	/// <summary>
	/// Loading 流动光效叠加颜色
	/// </summary>
	[Description("Loading 流动光效叠加颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? LoadingColor
	{
		get
		{
			return loadingColor;
		}
		set
		{
			if (!(loadingColor == value))
			{
				loadingColor = value;
				OnPropertyChanged("LoadingColor");
			}
		}
	}

	/// <summary>
	/// 动画时长
	/// </summary>
	[Description("动画时长")]
	[Category("Appearance")]
	[DefaultValue(200)]
	public int Animation { get; set; } = 200;

	/// <summary>
	/// 进度条总共步数
	/// </summary>
	[Description("进度条总共步数")]
	[Category("Appearance")]
	[DefaultValue(3)]
	public int Steps
	{
		get
		{
			return steps;
		}
		set
		{
			if (steps != value)
			{
				steps = value;
				if (shape == TShapeProgress.Steps)
				{
					Invalidate();
				}
				OnPropertyChanged("Steps");
			}
		}
	}

	/// <summary>
	/// 步数大小
	/// </summary>
	[Description("步数大小")]
	[Category("Appearance")]
	[DefaultValue(14)]
	public int StepSize
	{
		get
		{
			return stepSize;
		}
		set
		{
			if (stepSize != value)
			{
				stepSize = value;
				if (shape == TShapeProgress.Steps)
				{
					Invalidate();
				}
				OnPropertyChanged("StepSize");
			}
		}
	}

	/// <summary>
	/// 步数间隔
	/// </summary>
	[Description("步数间隔")]
	[Category("Appearance")]
	[DefaultValue(2)]
	public int StepGap
	{
		get
		{
			return stepGap;
		}
		set
		{
			if (stepGap != value)
			{
				stepGap = value;
				if (shape == TShapeProgress.Steps)
				{
					Invalidate();
				}
				OnPropertyChanged("StepGap");
			}
		}
	}

	/// <summary>
	/// 分段集合
	/// </summary>
	[Description("分段集合")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public ProgressSegment[]? Segments { get; set; }

	/// <summary>
	/// 窗口对象
	/// </summary>
	[Description("窗口对象")]
	[Category("任务栏")]
	[DefaultValue(null)]
	public ContainerControl? ContainerControl { get; set; }

	/// <summary>
	/// 任务栏中显示进度
	/// </summary>
	[Description("任务栏中显示进度")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool ShowInTaskbar
	{
		get
		{
			return showInTaskbar;
		}
		set
		{
			if (showInTaskbar == value)
			{
				return;
			}
			showInTaskbar = value;
			if (canTaskbar)
			{
				ContainerControl owner;
				if (showInTaskbar)
				{
					ShowTaskbar();
				}
				else if (GetOwner(out owner))
				{
					TaskbarProgressState(owner, ThumbnailProgressState.NoProgress);
				}
			}
			OnPropertyChanged("ShowInTaskbar");
		}
	}

	/// <summary>
	/// Value格式化时发生
	/// </summary>
	[Description("Value格式化时发生")]
	[Category("Behavior")]
	public event ProgressFormatEventHandler? ValueFormatChanged;

	protected override void Dispose(bool disposing)
	{
		ThreadLoading?.Dispose();
		ThreadValue?.Dispose();
		base.Dispose(disposing);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		ITask.Run(() =>
		{
			Thread.Sleep(100);
			canTaskbar = true;
			if (showInTaskbar)
			{
				ShowTaskbar();
			}
		});
	}

	private bool GetOwner(out ContainerControl? owner)
	{
		if (ownerTmp is ContainerControl containerControl)
		{
			owner = containerControl;
			return true;
		}
		owner = ContainerControl ?? Parent.FindPARENT();
		if (owner == null)
		{
			ownerTmp = 1;
			return false;
		}
		ownerTmp = owner;
		return true;
	}

	private void ShowTaskbar(bool sl = false)
	{
		if (!canTaskbar || !GetOwner(out ContainerControl owner))
		{
			return;
		}
		if (state == TType.None)
		{
			if (_value == 0f && loading)
			{
				TaskbarProgressValue(owner, 0uL);
				TaskbarProgressState(owner, ThumbnailProgressState.Indeterminate);
				return;
			}
			if (sl && old_state == ThumbnailProgressState.Indeterminate)
			{
				TaskbarProgressState(owner, ThumbnailProgressState.NoProgress);
			}
			TaskbarProgressState(owner, ThumbnailProgressState.Normal);
			TaskbarProgressValue(owner, (ulong)(_value * 100f));
			return;
		}
		switch (state)
		{
		case TType.Error:
			TaskbarProgressState(owner, ThumbnailProgressState.Error);
			break;
		case TType.Warn:
			TaskbarProgressState(owner, ThumbnailProgressState.Paused);
			break;
		default:
			TaskbarProgressState(owner, ThumbnailProgressState.Normal);
			break;
		}
		TaskbarProgressValue(owner, (ulong)(_value * 100f));
	}

	private void TaskbarProgressState(ContainerControl hwnd, ThumbnailProgressState state)
	{
		if (old_state == state)
		{
			return;
		}
		old_state = state;
		if (InvokeRequired)
		{
			Invoke(() =>
			{
				Windows7Taskbar.SetProgressState(hwnd.Handle, state);
			});
		}
		else
		{
			Windows7Taskbar.SetProgressState(hwnd.Handle, state);
		}
	}

	private void TaskbarProgressValue(ContainerControl hwnd, ulong value)
	{
		if (old_value == value)
		{
			return;
		}
		old_value = value;
		if (InvokeRequired)
		{
			Invoke(() =>
			{
				Windows7Taskbar.SetProgressValue(hwnd.Handle, value);
			});
		}
		else
		{
			Windows7Taskbar.SetProgressValue(hwnd.Handle, value);
		}
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		Canvas canvas = e.Canvas;
		Rectangle rect = e.Rect.PaddingRect(Padding);
		if (rect.Width != 0 && rect.Height != 0)
		{
			Color stateColor = GetStateColor(state, fill);
			switch (shape)
			{
			case TShapeProgress.Circle:
				PaintShapeCircle(canvas, rect, stateColor);
				break;
			case TShapeProgress.Mini:
				PaintShapeMini(canvas, rect, stateColor);
				break;
			case TShapeProgress.Steps:
				PaintShapeSteps(canvas, e.Rect, rect, stateColor);
				break;
			case TShapeProgress.Round:
				PaintShapeRound(canvas, e.Rect, rect, stateColor, round: true);
				break;
			default:
				PaintShapeRound(canvas, e.Rect, rect, stateColor, round: false);
				break;
			}
			base.OnDraw(e);
		}
	}

	private Color GetStateColor(TType state, Color? fill)
	{
		return state switch
		{
			TType.Success => fill ?? Colour.Success.Get(ColorScheme, "Progress", Name), 
			TType.Info => fill ?? Colour.Info.Get(ColorScheme, "Progress", Name), 
			TType.Warn => fill ?? Colour.Warning.Get(ColorScheme, "Progress", Name), 
			TType.Error => fill ?? Colour.Error.Get(ColorScheme, "Progress", Name), 
			_ => fill ?? Colour.Primary.Get(ColorScheme, "Progress", Name), 
		};
	}

	private void PaintShapeMini(Canvas g, Rectangle rect, Color color)
	{
		Color color2 = back ?? Color.FromArgb(40, color);
		rect.IconRectL(g.MeasureString("100" + TextUnit, Font).Height, out var icon_rect, out var text_rect, iconratio);
		if (icon_rect.Width != 0 && icon_rect.Height != 0)
		{
			using (SolidBrush brush = new SolidBrush(fore ?? Colour.Text.Get(ColorScheme, "Progress", Name)))
			{
				string text = ValueFormatChanged?.Invoke(this, new FloatEventArgs(_value_show)) ?? (useSystemText ? (Text ?? "") : ((_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit));
				g.String(text, Font, brush, new Rectangle(text_rect.X + 8, text_rect.Y, text_rect.Width - 8, text_rect.Height), FormatFlags.VerticalCenter | FormatFlags.Left | FormatFlags.NoWrap);
			}
			int num = ((radius == 0) ? ((int)Math.Round((float)icon_rect.Width * 0.2f)) : ((int)((float)radius * Dpi)));
			PaintProgressCircle(g, icon_rect, color, color2, num);
		}
	}

	private void PaintShapeSteps(Canvas g, Rectangle rect_t, Rectangle rect, Color color)
	{
		Color color2 = back ?? Colour.FillSecondary.Get(ColorScheme, "Progress", Name);
		Size size = g.MeasureString("100" + TextUnit, Font);
		int num = (int)((float)stepGap * Dpi);
		int num2 = (int)((float)size.Height * valueratio);
		float num3 = (int)((float)stepSize * Dpi);
		float num4 = 0f;
		int num5 = rect.Y + (rect.Height - num2) / 2;
		float num6 = (float)steps * _value_show;
		using (SolidBrush brush = new SolidBrush(color2))
		{
			if (num3 <= 0f)
			{
				float num7 = rect.Width;
				if (state == TType.None)
				{
					string text = ValueFormatChanged?.Invoke(this, new FloatEventArgs(_value_show)) ?? (useSystemText ? (Text ?? "") : ((_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit));
					num7 -= (float)(g.MeasureString(text, Font).Width + num2 / 2);
				}
				else
				{
					int num8 = (int)((float)size.Height * (iconratio + 0.1f));
					num7 -= (float)(num8 + num2 * 2 + num2 / 2);
				}
				num3 = (num7 - (float)num * ((float)steps - 1f)) / (float)steps;
			}
			if (num3 > 0f)
			{
				List<RectangleF> list = new List<RectangleF>(steps);
				for (int i = 0; i < steps; i++)
				{
					list.Add(new RectangleF((float)rect.X + num4, num5, num3, num2));
					num4 += num3 + (float)num;
				}
				Rectangle rect2 = new Rectangle(rect.X, rect.Y, (int)Math.Ceiling(num4), rect.Height);
				if (num6 > 0f)
				{
					float tmpw = (LoadingFull ? rect.Width : 0);
					using (Brush brush_fill = fillExtend.BrushEx(rect2, color, 90f))
					{
						PaintShapeSteps(g, brush, brush_fill, steps, num6, list, ref tmpw);
					}
					if (loading && AnimationLoadingValue > 0f && tmpw > 0f)
					{
						using GraphicsPath graphicsPath = new GraphicsPath();
						foreach (RectangleF item in list)
						{
							graphicsPath.AddRectangle(item);
						}
						using SolidBrush brush2 = new SolidBrush(Helper.ToColor(60f * loadingOpacity * (1f - AnimationLoadingValue), loadingColor ?? Colour.TextBase.Get(ColorScheme, "Progress", Name)));
						GraphicsState graphicsState = g.Save();
						g.SetClip(new RectangleF(rect.X, rect.Y, tmpw * _value_show * AnimationLoadingValue, rect.Height));
						g.Fill(brush2, graphicsPath);
						g.Restore(graphicsState);
					}
				}
				else
				{
					if (loading && LoadingFull)
					{
						using GraphicsPath graphicsPath2 = new GraphicsPath();
						foreach (RectangleF item2 in list)
						{
							graphicsPath2.AddRectangle(item2);
						}
						using SolidBrush brush3 = new SolidBrush(Helper.ToColor(80f * loadingOpacity * (1f - AnimationLoadingValue), loadingColor ?? Colour.TextBase.Get(ColorScheme, "Progress", Name)));
						GraphicsState graphicsState2 = g.Save();
						g.SetClip(new RectangleF(rect.X, rect.Y, (float)rect.Width * AnimationLoadingValue, rect.Height));
						g.Fill(brush3, graphicsPath2);
						g.Restore(graphicsState2);
					}
					for (int j = 0; j < steps; j++)
					{
						g.Fill(brush, list[j]);
					}
				}
			}
		}
		if (state == TType.None)
		{
			int num9 = (int)Math.Ceiling(num4 + (float)(num2 / 2));
			using SolidBrush brush4 = new SolidBrush(fore ?? Colour.Text.Get(ColorScheme, "Progress", Name));
			string text2 = ValueFormatChanged?.Invoke(this, new FloatEventArgs(_value_show)) ?? (useSystemText ? (Text ?? "") : ((_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit));
			g.String(text2, Font, brush4, new Rectangle(rect.X + num9, rect.Y, rect.Width - num9, rect.Height), FormatFlags.VerticalCenter | FormatFlags.Left | FormatFlags.NoWrap);
			return;
		}
		int num10 = (int)Math.Ceiling(num4);
		int num11 = (int)((float)size.Height * (iconratio + 0.1f));
		int num12 = num2 + num11;
		g.PaintIcons(state, new Rectangle(rect.X + num10 + num12 - num11, rect_t.Y + (rect_t.Height - num11) / 2, num11, num11), "Progress", ColorScheme);
	}

	private void PaintShapeSteps(Canvas g, Brush brush, Brush brush_fill, int steps, float prog, List<RectangleF> rects, ref float tmpw)
	{
		for (int i = 0; i < steps; i++)
		{
			if (prog > (float)i)
			{
				g.Fill(brush_fill, rects[i]);
				continue;
			}
			g.Fill(brush, rects[i]);
			tmpw = rects[i].Right;
		}
	}

	private void PaintShapeRound(Canvas g, Rectangle rect_t, Rectangle rect, Color color, bool round)
	{
		Color color2 = back ?? Colour.FillSecondary.Get(ColorScheme, "Progress", Name);
		float num = (float)radius * Dpi;
		if (round)
		{
			num = rect.Height;
		}
		if (state == TType.None)
		{
			if (UseTextCenter)
			{
				string text;
				if (ValueFormatChanged != null)
				{
					text = ValueFormatChanged(this, new FloatEventArgs(_value_show));
				}
				else
				{
					text = ((!useSystemText) ? ((_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit) : Text);
				}
				int num2 = (int)((float)g.MeasureString("龍Qq", Font).Height * valueratio);
				rect.Y += (rect.Height - num2) / 2;
				rect.Height = num2;
				PaintProgress(g, num, rect, color2, color);
				using SolidBrush brush = new SolidBrush(fore ?? Colour.Text.Get(ColorScheme, "Progress", Name));
				g.String(text, Font, brush, rect_t, s_c);
				return;
			}
			string text2;
			string text3;
			if (ValueFormatChanged == null)
			{
				if (useSystemText)
				{
					text2 = (text3 = Text);
				}
				else
				{
					string text4 = (_value_show * 100f).ToString("F" + ShowTextDot);
					char[] array = new char[text4.Length];
					array[0] = text4[0];
					for (int i = 1; i < text4.Length; i++)
					{
						if (text4[i] == '.')
						{
							array[i] = '.';
						}
						else
						{
							array[i] = '0';
						}
					}
					text2 = string.Join("", array) + TextUnit;
					text3 = text4 + TextUnit;
				}
			}
			else
			{
				text2 = (text3 = ValueFormatChanged(this, new FloatEventArgs(_value_show)));
			}
			if (text2 != null || text3 != null)
			{
				Size size = g.MeasureString(text2, Font);
				int num3 = (int)((float)size.Height * valueratio);
				int num4 = (int)Math.Ceiling((float)size.Width + (float)size.Height * 0.2f);
				Rectangle rect2 = new Rectangle(rect.Right - num4, rect_t.Y, num4, rect_t.Height);
				rect.Y += (rect.Height - num3) / 2;
				rect.Height = num3;
				rect.Width -= num4;
				if (rect.Width > 0)
				{
					PaintProgress(g, num, rect, color2, color);
				}
				using SolidBrush brush2 = new SolidBrush(fore ?? Colour.Text.Get(ColorScheme, "Progress", Name));
				g.String(text3, Font, brush2, rect2, FormatFlags.VerticalCenter | FormatFlags.Right | FormatFlags.NoWrap);
				return;
			}
			int num5 = (int)((float)g.MeasureString("龍Qq", Font).Height * valueratio);
			rect.Y += (rect.Height - num5) / 2;
			rect.Height = num5;
			PaintProgress(g, num, rect, color2, color);
		}
		else
		{
			string text5 = (_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit;
			Size size2 = g.MeasureString(text5, Font);
			int num6 = (int)((float)size2.Height * valueratio);
			int num7 = (int)((float)size2.Height * (iconratio + 0.1f));
			int num8 = num6 + num7;
			Rectangle rectangle = new Rectangle(rect.Right - num8, rect_t.Y, num8, rect_t.Height);
			rect.Y += (rect.Height - num6) / 2;
			rect.Height = num6;
			rect.Width -= num8;
			if (rect.Width > 0)
			{
				PaintProgress(g, num, rect, color2, color);
			}
			g.PaintIcons(state, new Rectangle(rectangle.Right - num7, rectangle.Y + (rectangle.Height - num7) / 2, num7, num7), "Progress", ColorScheme);
		}
	}

	private void PaintShapeCircle(Canvas g, Rectangle rect, Color color)
	{
		Color baseColor = back ?? Colour.FillSecondary.Get(ColorScheme, "Progress", Name);
		int num;
		if (rect.Width == rect.Height)
		{
			num = rect.Width;
		}
		else
		{
			num = ((rect.Width <= rect.Height) ? rect.Width : rect.Height);
		}
		int num2 = ((radius == 0) ? ((int)Math.Round((float)num * 0.04f)) : ((int)((float)radius * Dpi)));
		num -= num2;
		Rectangle rect2 = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y + (rect.Height - num) / 2, num, num);
		if (HasIcon)
		{
			int num3 = num - radius - (int)((float)IconCirclePadding * Dpi);
			_ = (rect2.Width - num3) / 2;
			if (IconCircleAngle)
			{
				GraphicsState graphicsState = g.Save();
				try
				{
					float num4 = (float)rect2.Width / 2f;
					int num5 = -num3 / 2;
					g.TranslateTransform((float)rect2.X + num4, (float)rect2.Y + num4);
					g.RotateTransform(_value_show * 360f);
					Rectangle rect3 = new Rectangle(num5, num5, num3, num3);
					if (icon != null)
					{
						g.Image(icon, rect3);
					}
					if (iconSvg != null)
					{
						g.Svg(iconSvg, rect3, IconCircleColor ?? Color.FromArgb(30, baseColor));
					}
				}
				finally
				{
					g.Restore(graphicsState);
				}
			}
			else
			{
				int num6 = (rect2.Width - num3) / 2;
				Rectangle rect4 = new Rectangle(rect2.X + num6, rect2.Y + num6, num3, num3);
				if (icon != null)
				{
					g.Image(icon, rect4);
				}
				if (iconSvg != null)
				{
					g.Svg(iconSvg, rect4, Color.FromArgb(30, baseColor));
				}
			}
		}
		PaintProgressCircle(g, rect2, color, baseColor, num2);
		if (_value_show > 0f)
		{
			if (state == TType.None)
			{
				using (SolidBrush brush = new SolidBrush(fore ?? Colour.Text.Get(ColorScheme, "Progress", Name)))
				{
					string text = ValueFormatChanged?.Invoke(this, new FloatEventArgs(_value_show)) ?? (useSystemText ? (Text ?? "") : ((_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit));
					g.String(text, Font, brush, rect, s_c);
					return;
				}
			}
			int num7 = (int)((float)rect2.Width * 0.26f);
			g.PaintIconGhosts(state, new Rectangle(rect.X + (rect.Width - num7) / 2, rect.Y + (rect.Height - num7) / 2, num7, num7), color);
			return;
		}
		using SolidBrush brush2 = new SolidBrush(fore ?? Colour.Text.Get(ColorScheme, "Progress", Name));
		string text2 = ValueFormatChanged?.Invoke(this, new FloatEventArgs(_value_show)) ?? (useSystemText ? (Text ?? "") : ((_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit));
		g.String(text2, Font, brush2, rect, s_c);
	}

	private void PaintProgressCircle(Canvas g, Rectangle rect, Color color, Color back, float size)
	{
		bool flag = false;
		int num2;
		int num3;
		if (GapDegree > 0)
		{
			flag = false;
			int num = Math.Min(GapDegree, 295);
			num2 = 360 - num;
			num3 = GapPlacement switch
			{
				TAlignMini.Top => 270, 
				TAlignMini.Right => 0, 
				TAlignMini.Left => 180, 
				_ => 90, 
			} + num / 2;
			if (num2 == 360)
			{
				g.DrawEllipse(back, size, rect);
			}
			else
			{
				g.DrawArc(back, size, rect, num3, num2);
			}
		}
		else
		{
			num3 = -90;
			num2 = 360;
			g.DrawEllipse(back, size, rect);
		}
		int num4 = 0;
		if (_value_show > 0f)
		{
			num4 = (int)Math.Round((float)num2 * _value_show);
			if (fillExtend.BrushEx(out Color[] colors, out float[] positions))
			{
				using GraphicsPath graphicsPath = new GraphicsPath();
				graphicsPath.AddArc(rect, num3, num4);
				g.DrawNativePathGradient(graphicsPath, size, colors, positions);
			}
			else
			{
				using Pen pen = new Pen(color, size);
				if (flag)
				{
					LineCap startCap = (pen.EndCap = LineCap.Round);
					pen.StartCap = startCap;
				}
				g.DrawArc(pen, rect, num3, num4);
			}
		}
		if (Segments != null && Segments.Length != 0)
		{
			for (int num5 = Segments.Length - 1; num5 > -1; num5--)
			{
				ProgressSegment progressSegment = Segments[num5];
				float sweepAngle = (float)num2 * progressSegment.Value;
				using Pen pen2 = new Pen(GetStateColor(progressSegment.State, progressSegment.Fill), size);
				if (flag)
				{
					LineCap startCap = (pen2.EndCap = LineCap.Round);
					pen2.StartCap = startCap;
				}
				g.DrawArc(pen2, rect, num3, sweepAngle);
			}
		}
		if (!loading || !(AnimationLoadingValue > 0f))
		{
			return;
		}
		if (_value_show > 0f)
		{
			using (Pen pen3 = new Pen(Helper.ToColor(60f * loadingOpacity * (1f - AnimationLoadingValue), loadingColor ?? Colour.BgBase.Get(ColorScheme, "Progress", Name)), size))
			{
				if (flag)
				{
					LineCap startCap = (pen3.EndCap = LineCap.Round);
					pen3.StartCap = startCap;
				}
				g.DrawArc(pen3, rect, num3, (int)((float)num4 * AnimationLoadingValue));
				return;
			}
		}
		if (!LoadingFull)
		{
			return;
		}
		num4 = num2;
		using Pen pen4 = new Pen(Helper.ToColor(80f * loadingOpacity * (1f - AnimationLoadingValue), loadingColor ?? Colour.BgBase.Get(ColorScheme, "Progress", Name)), size);
		if (flag)
		{
			LineCap startCap = (pen4.EndCap = LineCap.Round);
			pen4.StartCap = startCap;
		}
		g.DrawArc(pen4, rect, num3, (int)((float)num4 * AnimationLoadingValue));
	}

	private void PaintProgress(Canvas g, float radius, Rectangle rect, Color back, Color color)
	{
		using GraphicsPath path = rect.RoundPath(radius);
		g.Fill(back, path);
		bool flag = true;
		if (_value_show > 0f)
		{
			float num = (float)rect.Width * _value_show;
			if (num > radius)
			{
				RectangleF rect2 = new RectangleF(rect.X, rect.Y, num, rect.Height);
				using (GraphicsPath path2 = rect2.RoundPath(radius))
				{
					using Brush brush = fillExtend.BrushEx(rect2, color, 90f);
					g.Fill(brush, path2);
				}
				PaintSegments(g, radius, rect.X, rect.Y, rect.Width, rect.Height);
				if (loading && AnimationLoadingValue > 0f)
				{
					flag = false;
					float alpha = 60f * loadingOpacity * (1f - AnimationLoadingValue);
					using GraphicsPath path3 = new RectangleF(rect.X, rect.Y, num * AnimationLoadingValue, rect.Height).RoundPath(radius);
					g.Fill(Helper.ToColor(alpha, loadingColor ?? Colour.BgBase.Get(ColorScheme, "Progress", Name)), path3);
				}
			}
			else
			{
				using Bitmap image = new Bitmap(rect.Width, rect.Height);
				using (Canvas canvas = Graphics.FromImage(image).High(Dpi))
				{
					using (GraphicsPath path4 = new RectangleF(0f - num, 0f, num * 2f, rect.Height).RoundPath(radius))
					{
						canvas.Fill(color, path4);
					}
					PaintSegments(canvas, radius, 0, 0, rect.Width, rect.Height);
					if (loading && AnimationLoadingValue > 0f)
					{
						flag = false;
						float alpha2 = 60f * loadingOpacity * (1f - AnimationLoadingValue);
						using GraphicsPath path5 = new RectangleF(0f - num, 0f, num * 2f * AnimationLoadingValue, rect.Height).RoundPath(radius);
						canvas.Fill(Helper.ToColor(alpha2, loadingColor ?? Colour.BgBase.Get(ColorScheme, "Progress", Name)), path5);
					}
				}
				using TextureBrush textureBrush = new TextureBrush(image, WrapMode.Clamp);
				textureBrush.TranslateTransform(rect.X, rect.Y);
				g.Fill(textureBrush, path);
			}
		}
		if (!((loading && AnimationLoadingValue > 0f) & flag) || !LoadingFull)
		{
			return;
		}
		using SolidBrush brush2 = new SolidBrush(Helper.ToColor(80f * loadingOpacity * (1f - AnimationLoadingValue), loadingColor ?? Colour.BgBase.Get(ColorScheme, "Progress", Name)));
		using GraphicsPath path6 = new RectangleF(rect.X, rect.Y, (float)rect.Width * AnimationLoadingValue, rect.Height).RoundPath(radius);
		g.Fill(brush2, path6);
	}

	private void PaintSegments(Canvas g, float radius, int x, int y, int w, int h)
	{
		if (Segments == null || Segments.Length == 0)
		{
			return;
		}
		GraphicsPath graphicsPath = null;
		for (int num = Segments.Length - 1; num > -1; num--)
		{
			ProgressSegment progressSegment = Segments[num];
			float num2 = (float)w * progressSegment.Value;
			if (num2 > radius)
			{
				using GraphicsPath path = new RectangleF(x, y, num2, h).RoundPath(radius);
				g.Fill(GetStateColor(progressSegment.State, progressSegment.Fill), path);
			}
			else
			{
				if (graphicsPath == null)
				{
					graphicsPath = new Rectangle(x, y, w, h).RoundPath(radius);
				}
				GraphicsState graphicsState = g.Save();
				g.SetClip(graphicsPath);
				using (GraphicsPath path2 = new RectangleF(x - h, y, num2 + (float)h, h).RoundPath(radius))
				{
					g.Fill(GetStateColor(progressSegment.State, progressSegment.Fill), path2);
				}
				g.Restore(graphicsState);
			}
		}
		graphicsPath?.Dispose();
	}
}
