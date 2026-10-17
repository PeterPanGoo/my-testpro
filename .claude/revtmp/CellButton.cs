using System;
using System.Drawing;

namespace AntdUI;

/// <summary>
/// 按钮
/// </summary>
/// <seealso cref="T:AntdUI.CellLink" />
public class CellButton : CellLink
{
	private Color? fore;

	private Color? back;

	private string? backExtend;

	private Color? defaultback;

	private Color? defaultbordercolor;

	internal float borderWidth;

	private float iconratio = 0.7f;

	private float icongap = 0.25f;

	private Image? icon;

	private string? iconSvg;

	private TAlignMini iconPosition = TAlignMini.Left;

	private int radius = 6;

	private TShape shape;

	private TTypeMini type;

	private bool ghost;

	internal float ArrowProg = -1f;

	private bool showArrow;

	private bool isLink;

	private bool loading;

	internal int AnimationLoadingValue;

	private AnimationTask? ThreadLoading;

	/// <summary>
	/// 文字颜色
	/// </summary>
	public Color? Fore
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
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// 背景颜色
	/// </summary>
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
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// 悬停背景颜色
	/// </summary>
	public Color? BackHover { get; set; }

	/// <summary>
	/// 激活背景颜色
	/// </summary>
	public Color? BackActive { get; set; }

	/// <summary>
	/// 背景渐变色
	/// </summary>
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
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// Default模式背景颜色
	/// </summary>
	public Color? DefaultBack
	{
		get
		{
			return defaultback;
		}
		set
		{
			if (!(defaultback == value))
			{
				defaultback = value;
				if (type == TTypeMini.Default)
				{
					OnPropertyChanged();
				}
			}
		}
	}

	/// <summary>
	/// Default模式边框颜色
	/// </summary>
	public Color? DefaultBorderColor
	{
		get
		{
			return defaultbordercolor;
		}
		set
		{
			if (!(defaultbordercolor == value))
			{
				defaultbordercolor = value;
				if (type == TTypeMini.Default)
				{
					OnPropertyChanged();
				}
			}
		}
	}

	/// <summary>
	/// 边框宽度
	/// </summary>
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
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// 图标比例
	/// </summary>
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
				OnPropertyChanged(layout: true);
			}
		}
	}

	/// <summary>
	/// 图标与文字间距比例
	/// </summary>
	public float IconGap
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
				OnPropertyChanged(layout: true);
			}
		}
	}

	/// <summary>
	/// 图标
	/// </summary>
	public Image? Icon
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
				OnPropertyChanged(layout: true);
			}
		}
	}

	/// <summary>
	/// 图标SVG
	/// </summary>
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
				OnPropertyChanged(layout: true);
			}
		}
	}

	/// <summary>
	/// 是否包含图标
	/// </summary>
	public bool HasIcon
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
	/// 悬停图标
	/// </summary>
	public Image? IconHover { get; set; }

	/// <summary>
	/// 悬停图标SVG
	/// </summary>
	public string? IconHoverSvg { get; set; }

	/// <summary>
	/// 悬停图标动画时长
	/// </summary>
	public int IconHoverAnimation { get; set; } = 200;

	/// <summary>
	/// 按钮图标组件的位置
	/// </summary>
	public TAlignMini IconPosition
	{
		get
		{
			return iconPosition;
		}
		set
		{
			if (iconPosition != value)
			{
				iconPosition = value;
				OnPropertyChanged(layout: true);
			}
		}
	}

	/// <summary>
	/// 圆角
	/// </summary>
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
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// 形状
	/// </summary>
	public TShape Shape
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
				OnPropertyChanged(layout: true);
			}
		}
	}

	/// <summary>
	/// 类型
	/// </summary>
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
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// 幽灵属性，使按钮背景透明
	/// </summary>
	public bool Ghost
	{
		get
		{
			return ghost;
		}
		set
		{
			if (ghost != value)
			{
				ghost = value;
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// 下拉框箭头是否显示
	/// </summary>
	public bool ShowArrow
	{
		get
		{
			return showArrow;
		}
		set
		{
			if (showArrow != value)
			{
				showArrow = value;
				OnPropertyChanged(layout: true);
			}
		}
	}

	/// <summary>
	/// 下拉框箭头是否链接样式
	/// </summary>
	public bool IsLink
	{
		get
		{
			return isLink;
		}
		set
		{
			if (isLink != value)
			{
				isLink = value;
				if (showArrow)
				{
					OnPropertyChanged();
				}
			}
		}
	}

	/// <summary>
	/// 间距
	/// </summary>
	public int? Gap { get; set; }

	/// <summary>
	/// 加载状态
	/// </summary>
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
			OnPropertyChanged(layout: true);
			if (loading)
			{
				AnimationClickValue = 0f;
				ThreadLoading = new AnimationTask(new AnimationLinearConfig(PARENT.PARENT, (int i) =>
				{
					AnimationLoadingValue = i;
					OnPropertyChanged();
					return loading;
				}, 10, 360, 6).SetEnd(() =>
				{
					OnPropertyChanged(layout: true);
				}));
			}
		}
	}

	/// <summary>
	/// 加载图标
	/// </summary>
	public Image? LoadingIcon { get; set; }

	/// <summary>
	/// 加载图标SVG
	/// </summary>
	public string? LoadingSvg { get; set; }

	/// <summary>
	/// 加载响应点击
	/// </summary>
	public bool LoadingRespondClick { get; set; }

	/// <summary>
	/// 加载进度
	/// </summary>
	public float LoadingValue { get; set; } = 0.3f;

	internal override bool ExtraMouseHover
	{
		get
		{
			return _mouseHover;
		}
		set
		{
			if (_mouseHover == value)
			{
				return;
			}
			_mouseHover = value;
			if (!Enabled)
			{
				return;
			}
			Color color = Type switch
			{
				TTypeMini.Default => (!(BorderWidth > 0f)) ? Colour.FillSecondary.Get(PARENT.PARENT.ColorScheme, "Button", PARENT.PARENT.Name) : Colour.PrimaryHover.Get(PARENT.PARENT.ColorScheme, "Button", PARENT.PARENT.Name), 
				TTypeMini.Success => Colour.SuccessHover.Get(PARENT.PARENT.ColorScheme, "Button", PARENT.PARENT.Name), 
				TTypeMini.Error => Colour.ErrorHover.Get(PARENT.PARENT.ColorScheme, "Button", PARENT.PARENT.Name), 
				TTypeMini.Info => Colour.InfoHover.Get(PARENT.PARENT.ColorScheme, "Button", PARENT.PARENT.Name), 
				TTypeMini.Warn => Colour.WarningHover.Get(PARENT.PARENT.ColorScheme, "Button", PARENT.PARENT.Name), 
				_ => Colour.PrimaryHover.Get(PARENT.PARENT.ColorScheme, "Button", PARENT.PARENT.Name), 
			};
			if (BackHover.HasValue)
			{
				color = BackHover.Value;
			}
			if (Config.HasAnimation("Table"))
			{
				if (IconHoverAnimation > 0 && HasIcon && (IconHoverSvg != null || IconHover != null))
				{
					ThreadImageHover?.Dispose();
					AnimationImageHover = true;
					ThreadImageHover = new AnimationTask(new AnimationFixedConfig((float i) =>
					{
						AnimationImageHoverValue = i;
						OnPropertyChanged();
					}, 10, Animation.TotalFrames(10, IconHoverAnimation), value).SetEnd(() =>
					{
						AnimationImageHover = false;
					}));
				}
				if (color.A > 0)
				{
					int add = color.A / 12;
					ThreadHover?.Dispose();
					AnimationHover = true;
					ThreadHover = new AnimationTask(new AnimationLinearConfig(PARENT.PARENT, (int i) =>
					{
						AnimationHoverValue = i;
						OnPropertyChanged();
						return true;
					}, 10).SetValueColor(AnimationHoverValue, value, add, color.A).SetEnd(() =>
					{
						AnimationHover = false;
					}));
				}
				else
				{
					AnimationHoverValue = color.A;
					OnPropertyChanged();
				}
			}
			else
			{
				AnimationHoverValue = color.A;
			}
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// 按钮
	/// </summary>
	/// <param name="id">id</param>
	public CellButton(string id)
		: base(id)
	{
	}

	/// <summary>
	/// 按钮
	/// </summary>
	/// <param name="id">id</param>
	/// <param name="text">文本</param>
	public CellButton(string id, string? text)
		: base(id, text)
	{
	}

	/// <summary>
	/// 按钮
	/// </summary>
	/// <param name="id">id</param>
	/// <param name="text">文本</param>
	/// <param name="_type">类型</param>
	public CellButton(string id, string text, TTypeMini _type)
		: base(id, text)
	{
		type = _type;
	}

	/// <summary>
	/// 按钮
	/// </summary>
	/// <param name="id">id</param>
	/// <param name="_type">类型</param>
	public CellButton(string id, TTypeMini _type)
		: base(id)
	{
		type = _type;
	}

	public CellButton SetFore(Color? value)
	{
		fore = value;
		return this;
	}

	public CellButton SetBack(Color? value)
	{
		back = value;
		return this;
	}

	public CellButton SetBack(Color? value, Color? hover, Color? active = null)
	{
		back = value;
		BackHover = hover;
		BackActive = active;
		return this;
	}

	public CellButton SetBack(string? value)
	{
		backExtend = value;
		return this;
	}

	public CellButton SetDefaultBack(Color? value)
	{
		defaultback = value;
		return this;
	}

	public CellButton SetDefault(Color? value, Color? bor)
	{
		defaultback = value;
		defaultbordercolor = bor;
		return this;
	}

	public CellButton SetBorder(float value = 1f)
	{
		borderWidth = value;
		return this;
	}

	public CellButton SetIcon(Image? img)
	{
		icon = img;
		return this;
	}

	public CellButton SetIcon(string? svg)
	{
		iconSvg = svg;
		return this;
	}

	public CellButton SetIcon(Image? img, Image? hover)
	{
		icon = img;
		IconHover = hover;
		return this;
	}

	public CellButton SetIcon(string? svg, string? hover)
	{
		iconSvg = svg;
		IconHoverSvg = hover;
		return this;
	}

	public CellButton SetIconHover(string? svg, int animation = 200)
	{
		IconHoverSvg = svg;
		IconHoverAnimation = animation;
		return this;
	}

	public CellButton SetIconHover(Image? img, int animation = 200)
	{
		IconHover = img;
		IconHoverAnimation = animation;
		return this;
	}

	public CellButton SetIconPosition(TAlignMini align)
	{
		iconPosition = align;
		return this;
	}

	public CellButton SetIconRatio(float value = 1f)
	{
		iconratio = value;
		return this;
	}

	public CellButton SetIconGap(float value = 0f)
	{
		icongap = value;
		return this;
	}

	public CellButton SetRadius(int value = 0)
	{
		radius = value;
		return this;
	}

	public CellButton SetShape(TShape value = TShape.Round)
	{
		shape = value;
		return this;
	}

	public CellButton SetType(TTypeMini value = TTypeMini.Primary)
	{
		type = value;
		return this;
	}

	public CellButton SetGhost(bool value = true)
	{
		ghost = value;
		return this;
	}

	public CellButton SetArrow(bool value = true)
	{
		showArrow = value;
		return this;
	}

	public CellButton SetIsLink(bool value = true)
	{
		isLink = value;
		return this;
	}

	public CellButton SetArrow(bool value, bool link)
	{
		showArrow = value;
		isLink = value;
		return this;
	}

	public CellButton SetArrowProg(float value = 1f)
	{
		ArrowProg = value;
		return this;
	}

	public CellButton SetLoading(bool value = true)
	{
		loading = value;
		return this;
	}

	public CellButton SetLoadingValue(float value = -1f)
	{
		LoadingValue = value;
		return this;
	}

	public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
	{
		Table.PaintButton(g, font, PARENT.PARENT.Gaps, Rect, this, enable, PARENT.PARENT.ColorScheme, PARENT.PARENT.Name);
	}

	public override Size GetSize(Canvas g, Font font, TableGaps gap)
	{
		if (Gap.HasValue)
		{
			int num = (int)((float)Gap.Value * g.Dpi);
			return GetSizeCore(g, font, num, num * 2);
		}
		if (PARENT.PARENT.GapCell.HasValue)
		{
			int num2 = (int)((float)PARENT.PARENT.GapCell.Value * g.Dpi);
			return GetSizeCore(g, font, num2, num2 * 2);
		}
		return GetSizeCore(g, font, gap.x, gap.x2);
	}

	private Size GetSizeCore(Canvas g, Font font, int gap, int gap2)
	{
		if (string.IsNullOrEmpty(Text))
		{
			int num = g.MeasureString("龍Qq", font).Height + gap;
			return new Size(num, num);
		}
		Size size = g.MeasureText(Text ?? "龍Qq", font);
		bool flag = HasIcon || loading;
		if (flag || ShowArrow)
		{
			if (flag && (IconPosition == TAlignMini.Top || IconPosition == TAlignMini.Bottom))
			{
				int num2 = (int)Math.Ceiling((float)size.Height * 1.2f);
				return new Size(size.Width + gap2 * 2 + num2, size.Height + gap + num2);
			}
			int height = size.Height + gap;
			if (flag && ShowArrow)
			{
				return new Size(size.Width + gap2 + size.Height * 2, height);
			}
			if (flag)
			{
				return new Size(size.Width + gap2 + (int)Math.Ceiling((float)size.Height * 1.2f), height);
			}
			return new Size(size.Width + gap2 + (int)Math.Ceiling((float)size.Height * 0.8f), height);
		}
		return new Size(size.Width + gap2, size.Height + gap);
	}

	internal override void Click()
	{
		if (!Config.HasAnimation("Table"))
		{
			return;
		}
		ThreadClick?.Dispose();
		AnimationClickValue = 0f;
		AnimationClick = true;
		ThreadClick = new AnimationTask(new AnimationLoopConfig(PARENT.PARENT, () =>
		{
			if ((double)AnimationClickValue > 0.6)
			{
				AnimationClickValue = AnimationClickValue.Calculate(0.04f);
			}
			else
			{
				AnimationClickValue += (AnimationClickValue = AnimationClickValue.Calculate(0.1f));
			}
			if (AnimationClickValue > 1f)
			{
				AnimationClickValue = 0f;
				return false;
			}
			OnPropertyChanged();
			return true;
		}, 50).SetEnd(() =>
		{
			AnimationClick = false;
			OnPropertyChanged();
		}));
	}
}
