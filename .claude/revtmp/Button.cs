using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Button 按钮
/// </summary>
/// <remarks>按钮用于开始一个即时操作。</remarks>
[Description("Button 按钮")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
public class Button : IControl, IButtonControl, IEventListener
{
	[CompilerGenerated]
	private sealed class <>c__DisplayClass317_0
	{
		public Color[] colors;

		public AnimationType animationType;

		public Button <>4__this;

		public int transition_interval;

		public Color tmp;

		public int index;

		public int len;

		public int t;

		internal bool <AnimationBlinkTransition>b__0()
		{
			Color start = tmp;
			Color end = colors[index];
			index++;
			if (index > len - 1)
			{
				index = 0;
			}
			tmp = end;
			new AnimationTask((int i) =>
			{
				float val = Animation.Animate(i, t, 1f, animationType);
				<>4__this.colorBlink = start.BlendColors(Helper.ToColorN(val, end));
				<>4__this.Invalidate();
				return <>4__this.AnimationBlinkState;
			}, transition_interval, t, () =>
			{
				<>4__this.colorBlink = end;
				<>4__this.Invalidate();
			}).Wait();
			return <>4__this.AnimationBlinkState;
		}
	}

	private Color? fore;

	private Color? back;

	private string? backExtend;

	private Image? backImage;

	private TFit backFit;

	private Color? defaultback;

	private Color? defaultbordercolor;

	private float borderWidth;

	private int radius = 6;

	private TShape shape;

	private TButtonDisplayStyle displayStyle;

	private TTypeMini type;

	private bool ghost;

	private bool showArrow;

	private bool isLink;

	private bool useMnemonic = true;

	private string? text;

	private FormatFlags sf = FormatFlags.Center | FormatFlags.NoWrap | FormatFlags.HotkeyPrefixShow;

	private ContentAlignment textAlign = ContentAlignment.MiddleCenter;

	private int? virtualWidth;

	private bool textCenterHasIcon;

	private bool autoEllipsis;

	private bool textMultiLine;

	private float iconratio = 0.7f;

	private float icongap = 0.25f;

	private Image? icon;

	private string? iconSvg;

	private TAlignMini iconPosition = TAlignMini.Left;

	private bool AnimationIconToggle;

	private float AnimationIconToggleValue;

	private bool toggle;

	private string? toggleText;

	private Image? iconToggle;

	private string? iconSvgToggle;

	private Color? foreToggle;

	private TTypeMini? typeToggle;

	private Color? backToggle;

	private string? backExtendToggle;

	private bool loading;

	private int AnimationLoadingValue;

	private int AnimationLoadingWaveValue;

	private TJoinMode joinMode;

	private bool joinLeft;

	private bool joinRight;

	private AnimationTask? ThreadHover;

	private AnimationTask? ThreadIconHover;

	private AnimationTask? ThreadIconToggle;

	private AnimationTask? ThreadClick;

	private AnimationTask? ThreadLoading;

	private bool AnimationClick;

	private float AnimationClickValue;

	private bool _mouseDown;

	private int AnimationHoverValue;

	private bool AnimationHover;

	private bool AnimationIconHover;

	private float AnimationIconHoverValue;

	private bool _mouseHover;

	private Color? colorBlink;

	private AnimationTask? ThreadAnimateBlink;

	/// <summary>
	/// 闪烁动画状态
	/// </summary>
	public bool AnimationBlinkState;

	private bool init;

	private int clicks = 1;

	private TAutoSize autoSize;

	private bool hasFocus;

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
	/// 悬停文字颜色
	/// </summary>
	[Description("悬停文字颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeHover { get; set; }

	/// <summary>
	/// 激活文字颜色
	/// </summary>
	[Description("激活文字颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeActive { get; set; }

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
	/// 悬停背景颜色
	/// </summary>
	[Description("悬停背景颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BackHover { get; set; }

	/// <summary>
	/// 激活背景颜色
	/// </summary>
	[Description("激活背景颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BackActive { get; set; }

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
	/// Default模式背景颜色
	/// </summary>
	[Description("Default模式背景颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
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
					Invalidate();
				}
				OnPropertyChanged("DefaultBack");
			}
		}
	}

	/// <summary>
	/// Default模式边框颜色
	/// </summary>
	[Description("Default模式边框颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
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
					Invalidate();
				}
				OnPropertyChanged("DefaultBorderColor");
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
				Invalidate();
				OnPropertyChanged("BorderWidth");
			}
		}
	}

	/// <summary>
	/// 波浪大小
	/// </summary>
	[Description("波浪大小")]
	[Category("Appearance")]
	[DefaultValue(4)]
	public int WaveSize { get; set; } = 4;

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
	/// 形状
	/// </summary>
	[Description("形状")]
	[Category("Appearance")]
	[DefaultValue(TShape.Default)]
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
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("Shape");
			}
		}
	}

	/// <summary>
	/// 指定显示图像还是文本
	/// </summary>
	[Description("指定显示图像还是文本")]
	[Category("Appearance")]
	[DefaultValue(TButtonDisplayStyle.Default)]
	public TButtonDisplayStyle DisplayStyle
	{
		get
		{
			return displayStyle;
		}
		set
		{
			if (displayStyle != value)
			{
				displayStyle = value;
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("DisplayStyle");
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
	/// 幽灵属性，使按钮背景透明
	/// </summary>
	[Description("幽灵属性，使按钮背景透明")]
	[Category("Appearance")]
	[DefaultValue(false)]
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
				Invalidate();
				OnPropertyChanged("Ghost");
			}
		}
	}

	/// <summary>
	/// 响应真实区域
	/// </summary>
	[Description("响应真实区域")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool RespondRealAreas { get; set; }

	/// <summary>
	/// 显示箭头
	/// </summary>
	[Description("显示箭头")]
	[Category("Behavior")]
	[DefaultValue(false)]
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
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("ShowArrow");
			}
		}
	}

	/// <summary>
	/// 箭头链接样式
	/// </summary>
	[Description("箭头链接样式")]
	[Category("Behavior")]
	[DefaultValue(false)]
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
				Invalidate();
				OnPropertyChanged("IsLink");
			}
		}
	}

	/// <summary>
	/// 箭头角度
	/// </summary>
	[Browsable(false)]
	[Description("箭头角度")]
	[Category("Appearance")]
	[DefaultValue(-1f)]
	public float ArrowProg { get; set; } = -1f;

	/// <summary>
	/// 助记键
	/// </summary>
	[Description("如果为 true，则前面有(&)号 的第一个字符将用作按钮的助记键")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool UseMnemonic
	{
		get
		{
			return useMnemonic;
		}
		set
		{
			if (useMnemonic != value)
			{
				useMnemonic = value;
				if (value)
				{
					sf |= FormatFlags.HotkeyPrefixShow;
				}
				else
				{
					sf &= ~FormatFlags.HotkeyPrefixShow;
				}
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
			if (string.IsNullOrEmpty(value))
			{
				value = null;
			}
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

	[Description("虚拟宽度")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? VirtualWidth
	{
		get
		{
			return virtualWidth;
		}
		set
		{
			if (virtualWidth != value)
			{
				virtualWidth = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 文本居中显示(包含图标后)
	/// </summary>
	[Description("文本居中显示(包含图标后)")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool TextCenterHasIcon
	{
		get
		{
			return textCenterHasIcon;
		}
		set
		{
			if (textCenterHasIcon != value)
			{
				textCenterHasIcon = value;
				if (HasIcon)
				{
					Invalidate();
				}
				OnPropertyChanged("TextCenterHasIcon");
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
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("IconRatio");
			}
		}
	}

	/// <summary>
	/// 图标与文字间距比例
	/// </summary>
	[Description("图标与文字间距比例")]
	[Category("Appearance")]
	[DefaultValue(0.25f)]
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
				Invalidate();
				OnPropertyChanged("IconGap");
			}
		}
	}

	/// <summary>
	/// 图标
	/// </summary>
	[Description("图标")]
	[Category("Appearance")]
	[DefaultValue(null)]
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
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("Icon");
			}
		}
	}

	/// <summary>
	/// 图标SVG
	/// </summary>
	[Description("图标SVG")]
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
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("IconSvg");
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
	/// 图标大小
	/// </summary>
	[Description("图标大小")]
	[Category("Appearance")]
	[DefaultValue(typeof(Size), "0, 0")]
	public Size IconSize { get; set; }

	/// <summary>
	/// 悬停图标
	/// </summary>
	[Description("悬停图标")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Image? IconHover { get; set; }

	/// <summary>
	/// 悬停图标SVG
	/// </summary>
	[Description("悬停图标SVG")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? IconHoverSvg { get; set; }

	/// <summary>
	/// 悬停图标动画时长
	/// </summary>
	[Description("悬停图标动画时长")]
	[Category("Appearance")]
	[DefaultValue(200)]
	public int IconHoverAnimation { get; set; } = 200;

	/// <summary>
	/// 按钮图标组件的位置
	/// </summary>
	[Description("按钮图标组件的位置")]
	[Category("Appearance")]
	[DefaultValue(TAlignMini.Left)]
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
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("IconPosition");
			}
		}
	}

	/// <summary>
	/// 点击时自动改变选中状态
	/// </summary>
	[Description("点击时自动改变选中状态")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool AutoToggle { get; set; }

	/// <summary>
	/// 选中状态
	/// </summary>
	[Description("选中状态")]
	[Category("切换")]
	[DefaultValue(false)]
	public bool Toggle
	{
		get
		{
			return toggle;
		}
		set
		{
			if (value == toggle)
			{
				return;
			}
			toggle = value;
			if (Config.HasAnimation("Button", Name))
			{
				if (IconToggleAnimation > 0 && HasIcon && HasToggleIcon)
				{
					ThreadIconHover?.Dispose();
					ThreadIconHover = null;
					AnimationIconHover = false;
					ThreadIconToggle?.Dispose();
					AnimationIconToggle = true;
					ThreadIconToggle = new AnimationTask(new AnimationFixedConfig((float i) =>
					{
						AnimationIconToggleValue = i;
						Invalidate();
					}, 10, Animation.TotalFrames(10, IconToggleAnimation), value).SetEnd(() =>
					{
						AnimationIconToggle = false;
					}));
				}
				else
				{
					Invalidate();
				}
			}
			else
			{
				Invalidate();
			}
			OnToggleChanged(value);
			OnPropertyChanged("Toggle");
		}
	}

	/// <summary>
	/// 切换文本
	/// </summary>
	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
	[Description("切换文本")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? ToggleText
	{
		get
		{
			return this.GetLangI(LocalizationToggleText, toggleText);
		}
		set
		{
			if (!(toggleText == value))
			{
				toggleText = value;
				if (toggle && BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("ToggleText");
			}
		}
	}

	[Description("切换文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationToggleText { get; set; }

	/// <summary>
	/// 切换图标
	/// </summary>
	[Description("切换图标")]
	[Category("切换")]
	[DefaultValue(null)]
	public Image? ToggleIcon
	{
		get
		{
			return iconToggle;
		}
		set
		{
			if (iconToggle != value)
			{
				iconToggle = value;
				if (toggle)
				{
					Invalidate();
				}
				OnPropertyChanged("ToggleIcon");
			}
		}
	}

	/// <summary>
	/// 切换图标SVG
	/// </summary>
	[Description("切换图标SVG")]
	[Category("切换")]
	[DefaultValue(null)]
	public string? ToggleIconSvg
	{
		get
		{
			return iconSvgToggle;
		}
		set
		{
			if (!(iconSvgToggle == value))
			{
				iconSvgToggle = value;
				if (toggle)
				{
					Invalidate();
				}
				OnPropertyChanged("ToggleIconSvg");
			}
		}
	}

	/// <summary>
	/// 是否包含切换图标
	/// </summary>
	public bool HasToggleIcon
	{
		get
		{
			if (iconSvgToggle == null)
			{
				return iconToggle != null;
			}
			return true;
		}
	}

	/// <summary>
	/// 切换悬停图标
	/// </summary>
	[Description("切换悬停图标")]
	[Category("切换")]
	[DefaultValue(null)]
	public Image? ToggleIconHover { get; set; }

	/// <summary>
	/// 切换悬停图标SVG
	/// </summary>
	[Description("切换悬停图标SVG")]
	[Category("切换")]
	[DefaultValue(null)]
	public string? ToggleIconHoverSvg { get; set; }

	/// <summary>
	/// 图标切换动画时长
	/// </summary>
	[Description("图标切换动画时长")]
	[Category("切换")]
	[DefaultValue(200)]
	public int IconToggleAnimation { get; set; } = 200;

	/// <summary>
	/// 文字颜色
	/// </summary>
	[Description("切换文字颜色")]
	[Category("切换")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ToggleFore
	{
		get
		{
			return foreToggle;
		}
		set
		{
			if (foreToggle == value)
			{
				foreToggle = value;
			}
			foreToggle = value;
			if (toggle)
			{
				Invalidate();
			}
			OnPropertyChanged("ToggleFore");
		}
	}

	/// <summary>
	/// 切换悬停文字颜色
	/// </summary>
	[Description("切换悬停文字颜色")]
	[Category("切换")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ToggleForeHover { get; set; }

	/// <summary>
	/// 切换激活文字颜色
	/// </summary>
	[Description("切换激活文字颜色")]
	[Category("切换")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ToggleForeActive { get; set; }

	/// <summary>
	/// 切换类型
	/// </summary>
	[Description("切换类型")]
	[Category("切换")]
	[DefaultValue(null)]
	public TTypeMini? ToggleType
	{
		get
		{
			return typeToggle;
		}
		set
		{
			if (typeToggle != value)
			{
				typeToggle = value;
				if (toggle)
				{
					Invalidate();
				}
				OnPropertyChanged("ToggleType");
			}
		}
	}

	/// <summary>
	/// 切换背景颜色
	/// </summary>
	[Description("切换背景颜色")]
	[Category("切换")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ToggleBack
	{
		get
		{
			return backToggle;
		}
		set
		{
			if (!(backToggle == value))
			{
				backToggle = value;
				if (toggle)
				{
					Invalidate();
				}
				OnPropertyChanged("ToggleBack");
			}
		}
	}

	/// <summary>
	/// 切换背景渐变色
	/// </summary>
	[Description("切换背景渐变色")]
	[Category("切换")]
	[DefaultValue(null)]
	public string? ToggleBackExtend
	{
		get
		{
			return backExtendToggle;
		}
		set
		{
			if (!(backExtendToggle == value))
			{
				backExtendToggle = value;
				if (toggle)
				{
					Invalidate();
				}
				OnPropertyChanged("ToggleBackExtend");
			}
		}
	}

	/// <summary>
	/// 切换悬停背景颜色
	/// </summary>
	[Description("切换悬停背景颜色")]
	[Category("切换")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ToggleBackHover { get; set; }

	/// <summary>
	/// 切换激活背景颜色
	/// </summary>
	[Description("切换激活背景颜色")]
	[Category("切换")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ToggleBackActive { get; set; }

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
			SetCursor(_mouseHover && Enabled && !value);
			BeforeAutoSize();
			ThreadLoading?.Dispose();
			if (loading)
			{
				AnimationClickValue = 0f;
				ThreadLoading = new AnimationTask(new AnimationLinearConfig(this, (int i) =>
				{
					AnimationLoadingWaveValue++;
					if (AnimationLoadingWaveValue > 100)
					{
						AnimationLoadingWaveValue = 0;
					}
					AnimationLoadingValue = i;
					Invalidate();
					return loading;
				}, 10, 360, 6).SetEnd(Invalidate));
			}
			else
			{
				Invalidate();
			}
			OnPropertyChanged("Loading");
		}
	}

	/// <summary>
	/// 加载图标
	/// </summary>
	[Description("加载图标")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Image? LoadingIcon { get; set; }

	/// <summary>
	/// 加载图标SVG
	/// </summary>
	[Description("加载图标SVG")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? LoadingSvg { get; set; }

	/// <summary>
	/// 加载响应点击
	/// </summary>
	[Description("加载响应点击")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool LoadingRespondClick { get; set; }

	/// <summary>
	/// 加载进度
	/// </summary>
	[Description("加载进度")]
	[Category("加载")]
	[DefaultValue(0.3f)]
	public float LoadingValue { get; set; } = 0.3f;

	/// <summary>
	/// 水波进度
	/// </summary>
	[Description("水波进度")]
	[Category("加载")]
	[DefaultValue(0f)]
	public float LoadingWaveValue { get; set; }

	/// <summary>
	/// 水波颜色
	/// </summary>
	[Description("水波颜色")]
	[Category("加载")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? LoadingWaveColor { get; set; }

	/// <summary>
	/// 水波是否垂直
	/// </summary>
	[Description("水波是否垂直")]
	[Category("加载")]
	[DefaultValue(false)]
	public bool LoadingWaveVertical { get; set; }

	/// <summary>
	/// 水波大小
	/// </summary>
	[Description("水波大小")]
	[Category("加载")]
	[DefaultValue(2)]
	public int LoadingWaveSize { get; set; } = 2;

	/// <summary>
	/// 水波数量
	/// </summary>
	[Description("水波数量")]
	[Category("加载")]
	[DefaultValue(1)]
	public int LoadingWaveCount { get; set; } = 1;

	/// <summary>
	/// 组合模式
	/// </summary>
	[Description("组合模式")]
	[Category("Appearance")]
	[DefaultValue(TJoinMode.None)]
	public TJoinMode JoinMode
	{
		get
		{
			return joinMode;
		}
		set
		{
			if (joinMode != value)
			{
				joinMode = value;
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("JoinMode");
			}
		}
	}

	/// <summary>
	/// 连接左边
	/// </summary>
	[Obsolete("use JoinMode")]
	[Browsable(false)]
	[Description("连接左边")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool JoinLeft
	{
		get
		{
			return joinLeft;
		}
		set
		{
			if (joinLeft != value)
			{
				joinLeft = value;
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("JoinLeft");
			}
		}
	}

	/// <summary>
	/// 连接右边
	/// </summary>
	[Obsolete("use JoinMode")]
	[Browsable(false)]
	[Description("连接右边")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool JoinRight
	{
		get
		{
			return joinRight;
		}
		set
		{
			if (joinRight != value)
			{
				joinRight = value;
				if (BeforeAutoSize())
				{
					Invalidate();
				}
				OnPropertyChanged("JoinRight");
			}
		}
	}

	[Description("激活状态")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool ExtraMouseDown
	{
		get
		{
			return _mouseDown;
		}
		set
		{
			if (_mouseDown != value)
			{
				_mouseDown = value;
				Invalidate();
			}
		}
	}

	[Description("悬停状态")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool ExtraMouseHover
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
			bool enabled = Enabled;
			SetCursor((value & enabled) && !loading);
			if (!enabled)
			{
				return;
			}
			int a = GetColorO().A;
			if (Config.HasAnimation("Button", Name))
			{
				if (IconHoverAnimation > 0 && ((toggle && HasToggleIcon && (ToggleIconHoverSvg != null || ToggleIconHover != null)) || (HasIcon && (IconHoverSvg != null || IconHover != null))))
				{
					ThreadIconHover?.Dispose();
					AnimationIconHover = true;
					ThreadIconHover = new AnimationTask(new AnimationFixedConfig((float i) =>
					{
						AnimationIconHoverValue = i;
						Invalidate();
					}, 10, Animation.TotalFrames(10, IconHoverAnimation), value).SetEnd(() =>
					{
						AnimationIconHover = false;
					}));
				}
				if (a > 0)
				{
					int add = a / 12;
					ThreadHover?.Dispose();
					AnimationHover = true;
					ThreadHover = new AnimationTask(new AnimationLinearConfig(this, (int i) =>
					{
						AnimationHoverValue = i;
						Invalidate();
						return true;
					}, 10).SetValueColor(AnimationHoverValue, value, add, a).SetEnd(() =>
					{
						AnimationHover = false;
					}));
				}
				else
				{
					AnimationHoverValue = a;
					Invalidate();
				}
			}
			else
			{
				AnimationHoverValue = a;
			}
			Invalidate();
		}
	}

	public override Rectangle ReadRectangle => ClientRectangle.PaddingRect(Padding).ReadRect(((float)WaveSize + borderWidth / 2f) * Dpi, shape, joinMode, joinLeft, joinRight);

	public override GraphicsPath RenderRegion => Helper.PathJoin(ReadRectangle, radius, Dpi, shape, joinMode, joinLeft, joinRight);

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
		string value = (toggle ? (ToggleText ?? Text) : Text);
		Size size = MeasureText(g, value, out var txt_height);
		int num = (int)((float)txt_height * iconratio);
		int num2 = (int)((float)txt_height * 1.02f);
		int num3 = (int)((float)WaveSize * Dpi);
		int num4 = num3;
		int num5 = Math.Max(size.Height, num);
		if (Shape == TShape.Circle || string.IsNullOrEmpty(value) || displayStyle == TButtonDisplayStyle.Image)
		{
			int num6 = num5 + num3 + num2;
			return new Size(num6, num6);
		}
		if (joinMode > TJoinMode.None)
		{
			num4 = 0;
		}
		else if (joinLeft || joinRight)
		{
			num4 = 0;
		}
		bool flag = (loading && LoadingValue > -1f) || (HasIcon && displayStyle == TButtonDisplayStyle.Default);
		if (flag && showArrow)
		{
			int num7 = (int)((float)txt_height * icongap);
			int num8 = num2 + num3;
			int num9 = num8 + num4 + (num + num7) * 2;
			if (IconPosition == TAlignMini.Top || IconPosition == TAlignMini.Bottom)
			{
				return new Size(size.Width + num8, size.Height + num9);
			}
			return new Size(size.Width + num9, num5 + num8);
		}
		if (flag)
		{
			int num10 = (int)((float)txt_height * icongap);
			int num11 = num2 + num3;
			int num12 = num11 + num4 + num + num10;
			if (IconPosition == TAlignMini.Top || IconPosition == TAlignMini.Bottom)
			{
				return new Size(size.Width + num11, size.Height + num12);
			}
			return new Size(size.Width + num12, num5 + num11);
		}
		if (showArrow)
		{
			int num13 = (int)((float)txt_height * icongap);
			int num14 = num2 + num3;
			int num15 = num14 + num4 + num + num13;
			return new Size(size.Width + num15, num5 + num14);
		}
		return new Size(size.Width + num2 + num3 + num4, num5 + num2 + num3);
	});

	[DefaultValue(DialogResult.None)]
	public DialogResult DialogResult { get; set; }

	/// <summary>
	/// 是否存在焦点
	/// </summary>
	[Browsable(false)]
	[Description("是否存在焦点")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool HasFocus
	{
		get
		{
			return hasFocus;
		}
		private set
		{
			if (value && (_mouseDown || _mouseHover))
			{
				value = false;
			}
			if (hasFocus != value)
			{
				hasFocus = value;
				Invalidate();
			}
		}
	}

	[Description("Toggle 改变时发生")]
	[Category("Behavior")]
	public event BoolEventHandler? ToggleChanged;

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public new event EventHandler? DoubleClick
	{
		add
		{
			base.DoubleClick += value;
		}
		remove
		{
			base.DoubleClick -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public new event MouseEventHandler? MouseDoubleClick
	{
		add
		{
			base.MouseDoubleClick += value;
		}
		remove
		{
			base.MouseDoubleClick -= value;
		}
	}

	public Button()
		: base(ControlType.Button)
	{
		base.BackColor = Color.Transparent;
	}

	protected override bool ProcessMnemonic(char charCode)
	{
		if (useMnemonic && CanProcessMnemonicBefore(charCode, text) && CanProcessMnemonicAfter())
		{
			PerformClick();
			return true;
		}
		return base.ProcessMnemonic(charCode);
	}

	public Button SetIcon(Image? value, string? svg = null)
	{
		icon = value;
		iconSvg = svg;
		Invalidate();
		return this;
	}

	public Button SetIcon(string? value, Image? img = null)
	{
		icon = img;
		iconSvg = value;
		Invalidate();
		return this;
	}

	public virtual void ClickAnimation()
	{
		if (WaveSize <= 0 || !Config.HasAnimation("Button", Name))
		{
			return;
		}
		ThreadClick?.Dispose();
		AnimationClickValue = 0f;
		AnimationClick = true;
		ThreadClick = new AnimationTask(new AnimationLoopConfig(this, () =>
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
			Invalidate();
			return true;
		}, 50).SetEnd(() =>
		{
			AnimationClick = false;
			Invalidate();
		}));
	}

	/// <summary>
	/// 开始闪烁动画
	/// </summary>
	/// <param name="interval">动画间隔时长（毫秒）</param>
	/// <param name="colors">色彩值</param>
	public void AnimationBlink(int interval, params Color[] colors)
	{
		ThreadAnimateBlink?.Dispose();
		if (colors.Length <= 1)
		{
			return;
		}
		AnimationBlinkState = true;
		int index = 0;
		int len = colors.Length;
		ThreadAnimateBlink = new AnimationTask(this, () =>
		{
			colorBlink = colors[index];
			index++;
			if (index > len - 1)
			{
				index = 0;
			}
			Invalidate();
			return AnimationBlinkState;
		}, interval, Invalidate);
	}

	/// <summary>
	/// 开始闪烁动画（颜色过度动画）
	/// </summary>
	/// <param name="interval">动画间隔时长（毫秒）</param>
	/// <param name="colors">色彩值</param>
	public void AnimationBlinkTransition(int interval, params Color[] colors)
	{
		AnimationBlinkTransition(interval, 10, AnimationType.Liner, colors);
	}

	/// <summary>
	/// 开始闪烁动画（颜色过度动画）
	/// </summary>
	/// <param name="interval">动画间隔时长（毫秒）</param>
	/// <param name="transition_interval">过度动画间隔时长（毫秒）</param>
	/// <param name="animationType">过度动画类型</param>
	/// <param name="colors">色彩值</param>
	public void AnimationBlinkTransition(int interval, int transition_interval, AnimationType animationType, params Color[] colors)
	{
		ThreadAnimateBlink?.Dispose();
		if (colors.Length <= 1 || interval <= transition_interval)
		{
			return;
		}
		AnimationBlinkState = true;
		int index = 0;
		int len = colors.Length;
		Color tmp = colors[index];
		index++;
		if (index > len - 1)
		{
			index = 0;
		}
		int t = Animation.TotalFrames(transition_interval, interval);
		<>c__DisplayClass317_0 CS$<>8__locals0;
		ThreadAnimateBlink = new AnimationTask(this, () =>
		{
			Color end = colors[index];
			index++;
			if (index > len - 1)
			{
				index = 0;
			}
			tmp = end;
			new AnimationTask((int i) =>
			{
				float val = Animation.Animate(i, t, 1f, animationType);
				colorBlink = System.Runtime.CompilerServices.Unsafe.As<<>c__DisplayClass317_0, Color>(ref CS$<>8__locals0).BlendColors(Helper.ToColorN(val, end));
				Invalidate();
				return AnimationBlinkState;
			}, transition_interval, t, () =>
			{
				colorBlink = end;
				Invalidate();
			}).Wait();
			return AnimationBlinkState;
		});
	}

	public void StopAnimationBlink()
	{
		AnimationBlinkState = false;
		ThreadAnimateBlink?.Dispose();
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		init = true;
		Canvas canvas = e.Canvas;
		Rectangle rect_t = e.Rect.PaddingRect(Padding);
		Rectangle readRectangle = ReadRectangle;
		if (readRectangle.Width == 0 || readRectangle.Height == 0)
		{
			return;
		}
		float num = ((shape == TShape.Round || shape == TShape.Circle) ? ((float)readRectangle.Height) : ((float)radius * Dpi));
		if (backImage != null)
		{
			canvas.Image(readRectangle, backImage, backFit, num, shape);
		}
		bool flag = type == TTypeMini.Default;
		bool enabled = Enabled;
		string text;
		if (toggle)
		{
			text = ToggleText ?? Text;
			if (typeToggle.HasValue)
			{
				flag = typeToggle.Value == TTypeMini.Default;
			}
		}
		else
		{
			text = Text;
		}
		if (flag)
		{
			GetDefaultColorConfig(out var Fore, out var Color, out var backHover, out var backActive);
			using GraphicsPath graphicsPath = Helper.PathJoin(readRectangle, num, shape, joinMode, joinLeft, joinRight);
			if (AnimationClick)
			{
				float num2 = (float)readRectangle.Width + (float)(rect_t.Width - readRectangle.Width) * AnimationClickValue;
				float num3 = (float)readRectangle.Height + (float)(rect_t.Height - readRectangle.Height) * AnimationClickValue;
				if (shape == TShape.Circle)
				{
					if (num2 > num3)
					{
						num2 = num3;
					}
					else
					{
						num3 = num2;
					}
				}
				float alpha = 100f * (1f - AnimationClickValue);
				using GraphicsPath graphicsPath2 = new RectangleF((float)rect_t.X + ((float)rect_t.Width - num2) / 2f, (float)rect_t.Y + ((float)rect_t.Height - num3) / 2f, num2, num3).RoundPath(num, shape);
				graphicsPath2.AddPath(graphicsPath, connect: false);
				canvas.Fill(Helper.ToColor(alpha, Color), graphicsPath2);
			}
			if (enabled)
			{
				if (!ghost)
				{
					if (WaveSize > 0)
					{
						PaintShadow(canvas, rect_t, readRectangle, graphicsPath, Colour.FillQuaternary.Get(ColorScheme, "Button", Name), num);
					}
					canvas.Fill(defaultback ?? Colour.DefaultBg.Get(ColorScheme, "Button", Name), graphicsPath);
				}
				if (borderWidth > 0f)
				{
					PaintLoadingWave(canvas, graphicsPath, readRectangle, num);
					float num4 = borderWidth * Dpi;
					if (ExtraMouseDown)
					{
						canvas.Draw(backActive, num4, graphicsPath);
						PaintTextLoading(canvas, text, backActive, readRectangle, enabled, num);
					}
					else if (AnimationHover)
					{
						Color overlay = Helper.ToColor(AnimationHoverValue, backHover);
						canvas.Draw(Colour.DefaultBorder.Get(ColorScheme, "Button", Name).BlendColors(overlay), num4, graphicsPath);
						PaintTextLoading(canvas, text, Fore.BlendColors(overlay), readRectangle, enabled, num);
					}
					else if (ExtraMouseHover)
					{
						canvas.Draw(backHover, num4, graphicsPath);
						PaintTextLoading(canvas, text, backHover, readRectangle, enabled, num);
					}
					else
					{
						if (AnimationBlinkState && colorBlink.HasValue)
						{
							canvas.Draw(colorBlink.Value, num4, graphicsPath);
						}
						else
						{
							canvas.Draw(defaultbordercolor ?? Colour.DefaultBorder.Get(ColorScheme, "Button", Name), num4, graphicsPath);
						}
						PaintTextLoading(canvas, text, Fore, readRectangle, enabled, num);
					}
				}
				else
				{
					if (ExtraMouseDown)
					{
						canvas.Fill(backActive, graphicsPath);
					}
					else if (AnimationHover)
					{
						canvas.Fill(Helper.ToColor(AnimationHoverValue, backHover), graphicsPath);
					}
					else if (ExtraMouseHover)
					{
						canvas.Fill(backHover, graphicsPath);
					}
					PaintLoadingWave(canvas, graphicsPath, readRectangle, num);
					PaintTextLoading(canvas, text, Fore, readRectangle, enabled, num);
				}
			}
			else
			{
				PaintLoadingWave(canvas, graphicsPath, readRectangle, num);
				if (!ghost)
				{
					canvas.Fill(Colour.FillTertiary.GetSymbol(ColorScheme, "bgDisabled", "Button", Name), graphicsPath);
				}
				PaintTextLoading(canvas, text, Colour.TextQuaternary.GetSymbol(ColorScheme, "foreDisabled", "Button", Name), readRectangle, enabled, num);
			}
		}
		else
		{
			GetColorConfig(out var Fore2, out var foreHover, out var foreActive, out var Back, out var backHover2, out var backActive2);
			using GraphicsPath graphicsPath3 = Helper.PathJoin(readRectangle, num, shape, joinMode, joinLeft, joinRight);
			if (AnimationClick)
			{
				float num5 = (float)readRectangle.Width + (float)(rect_t.Width - readRectangle.Width) * AnimationClickValue;
				float num6 = (float)readRectangle.Height + (float)(rect_t.Height - readRectangle.Height) * AnimationClickValue;
				if (shape == TShape.Circle)
				{
					if (num5 > num6)
					{
						num5 = num6;
					}
					else
					{
						num6 = num5;
					}
				}
				float alpha2 = 100f * (1f - AnimationClickValue);
				using GraphicsPath graphicsPath4 = new RectangleF((float)rect_t.X + ((float)rect_t.Width - num5) / 2f, (float)rect_t.Y + ((float)rect_t.Height - num6) / 2f, num5, num6).RoundPath(num, shape);
				graphicsPath4.AddPath(graphicsPath3, connect: false);
				canvas.Fill(Helper.ToColor(alpha2, Back), graphicsPath4);
			}
			if (ghost)
			{
				PaintLoadingWave(canvas, graphicsPath3, readRectangle, num);
				if (borderWidth > 0f)
				{
					float num7 = borderWidth * Dpi;
					if (ExtraMouseDown)
					{
						canvas.Draw(backActive2, num7, graphicsPath3);
						PaintTextLoading(canvas, text, backActive2, readRectangle, enabled, num);
					}
					else if (AnimationHover)
					{
						Color overlay2 = Helper.ToColor(AnimationHoverValue, backHover2);
						canvas.Draw((enabled ? Back : Colour.FillTertiary.Get(ColorScheme, "Button", Name)).BlendColors(overlay2), num7, graphicsPath3);
						PaintTextLoading(canvas, text, Back.BlendColors(overlay2), readRectangle, enabled, num);
					}
					else if (ExtraMouseHover)
					{
						canvas.Draw(backHover2, num7, graphicsPath3);
						PaintTextLoading(canvas, text, backHover2, readRectangle, enabled, num);
					}
					else if (enabled)
					{
						if (toggle)
						{
							using Brush brush = backExtendToggle.BrushEx(readRectangle, Back);
							canvas.Draw(brush, num7, graphicsPath3);
						}
						else
						{
							using Brush brush2 = backExtend.BrushEx(readRectangle, Back);
							canvas.Draw(brush2, num7, graphicsPath3);
						}
						PaintTextLoading(canvas, text, Back, readRectangle, enabled, num);
					}
					else
					{
						canvas.Draw(Colour.FillTertiary.Get(ColorScheme, "Button", Name), num7, graphicsPath3);
						PaintTextLoading(canvas, text, Colour.TextQuaternary.GetSymbol(ColorScheme, "foreDisabled", "Button", Name), readRectangle, enabled, num);
					}
				}
				else
				{
					PaintTextLoading(canvas, text, enabled ? Back : Colour.TextQuaternary.GetSymbol(ColorScheme, "foreDisabled", "Button", Name), readRectangle, enabled, num);
				}
			}
			else if (enabled)
			{
				if (WaveSize > 0)
				{
					PaintShadow(canvas, rect_t, readRectangle, graphicsPath3, Back.rgba((Config.Mode == TMode.Dark) ? 0.15f : 0.1f), num);
				}
				if (toggle)
				{
					using Brush brush3 = backExtendToggle.BrushEx(readRectangle, Back);
					canvas.Fill(brush3, graphicsPath3);
				}
				else
				{
					using Brush brush4 = backExtend.BrushEx(readRectangle, Back);
					canvas.Fill(brush4, graphicsPath3);
				}
				if (ExtraMouseDown)
				{
					canvas.Fill(backActive2, graphicsPath3);
					PaintLoadingWave(canvas, graphicsPath3, readRectangle, num);
					PaintTextLoading(canvas, text, foreActive, readRectangle, enabled, num);
				}
				else if (AnimationHover)
				{
					canvas.Fill(Helper.ToColor(AnimationHoverValue, backHover2), graphicsPath3);
					PaintLoadingWave(canvas, graphicsPath3, readRectangle, num);
					if (Fore2 == foreHover)
					{
						PaintTextLoading(canvas, text, Fore2, readRectangle, enabled, num);
					}
					else
					{
						PaintTextLoading(canvas, text, Fore2.BlendColors(Helper.ToColor(AnimationHoverValue, foreHover)), readRectangle, enabled, num);
					}
				}
				else if (ExtraMouseHover)
				{
					canvas.Fill(backHover2, graphicsPath3);
					PaintLoadingWave(canvas, graphicsPath3, readRectangle, num);
					PaintTextLoading(canvas, text, foreHover, readRectangle, enabled, num);
				}
				else
				{
					PaintLoadingWave(canvas, graphicsPath3, readRectangle, num);
					PaintTextLoading(canvas, text, Fore2, readRectangle, enabled, num);
				}
			}
			else
			{
				canvas.Fill(Colour.FillTertiary.GetSymbol(ColorScheme, "bgDisabled", "Button", Name), graphicsPath3);
				PaintLoadingWave(canvas, graphicsPath3, readRectangle, num);
				PaintTextLoading(canvas, text, Colour.TextQuaternary.GetSymbol(ColorScheme, "foreDisabled", "Button", Name), readRectangle, enabled, num);
			}
		}
		base.OnDraw(e);
	}

	/// <summary>
	/// 绘制阴影
	/// </summary>
	protected virtual void PaintShadow(Canvas g, Rectangle rect_t, Rectangle rect, GraphicsPath path, Color color, float radius)
	{
		float num = (float)WaveSize * Dpi / 2f;
		using GraphicsPath graphicsPath = new RectangleF(rect.X, (float)rect.Y + num, rect.Width, rect.Height).RoundPath(radius);
		graphicsPath.AddPath(path, connect: false);
		g.Fill(color, graphicsPath);
	}

	protected virtual void PaintLoadingWave(Canvas g, GraphicsPath path, Rectangle rect, float radius)
	{
		if (!loading || !(LoadingWaveValue > 0f))
		{
			return;
		}
		using SolidBrush brush = new SolidBrush(LoadingWaveColor ?? Colour.Fill.Get(ColorScheme, "Button", Name));
		if (LoadingWaveValue >= 1f)
		{
			g.Fill(brush, path);
		}
		else if (LoadingWaveCount > 0)
		{
			GraphicsState graphicsState = g.Save();
			g.SetClip(path);
			g.ResetTransform();
			int num = (int)((float)LoadingWaveSize * Dpi);
			int num2 = LoadingWaveCount * 2 + 2;
			if (num2 < 6)
			{
				num2 = 6;
			}
			if (LoadingWaveVertical)
			{
				int num3 = (int)((float)rect.Height * LoadingWaveValue);
				if (num3 > 0)
				{
					num3 = rect.Height - num3 + rect.Y;
					int num4 = rect.Width / LoadingWaveCount;
					int num5 = num4 * 2;
					int num6 = num3 - num;
					int num7 = rect.X + num4 * num2;
					using GraphicsPath graphicsPath = new GraphicsPath();
					g.TranslateTransform(0f - ((float)num4 + (float)num5 * ((float)AnimationLoadingWaveValue / 100f)), 0f);
					graphicsPath.AddLine(num7, num3, num7, rect.Bottom);
					graphicsPath.AddLine(num7, rect.Bottom, rect.X, rect.Bottom);
					graphicsPath.AddLine(rect.X, rect.Bottom, rect.X, num3);
					bool flag = true;
					List<PointF> list = new List<PointF>(num2);
					for (int i = 0; i < num2 + 1; i++)
					{
						list.Add(new PointF(rect.X + num4 * i, flag ? num3 : num6));
						flag = !flag;
					}
					graphicsPath.AddCurve(list.ToArray());
					g.Fill(brush, graphicsPath);
				}
			}
			else
			{
				int num8 = (int)((float)rect.Width * LoadingWaveValue);
				if (num8 > 0)
				{
					num8 += rect.X;
					int num9 = rect.Height / LoadingWaveCount;
					int num10 = num9 * 2;
					int num11 = num8 + num;
					int num12 = rect.Y + num9 * num2;
					using GraphicsPath graphicsPath2 = new GraphicsPath();
					g.TranslateTransform(0f, 0f - ((float)num9 + (float)num10 * ((float)AnimationLoadingWaveValue / 100f)));
					graphicsPath2.AddLine(num8, num12, rect.X, num12);
					graphicsPath2.AddLine(rect.X, num12, rect.X, rect.Y);
					graphicsPath2.AddLine(rect.X, rect.Y, num8, rect.Y);
					bool flag2 = true;
					List<PointF> list2 = new List<PointF>(num2);
					for (int j = 0; j < num2 + 1; j++)
					{
						list2.Add(new PointF(flag2 ? num8 : num11, rect.Y + num9 * j));
						flag2 = !flag2;
					}
					graphicsPath2.AddCurve(list2.ToArray());
					g.Fill(brush, graphicsPath2);
				}
			}
			g.Restore(graphicsState);
		}
		else if (LoadingWaveVertical)
		{
			int num13 = (int)((float)rect.Height * LoadingWaveValue);
			if (num13 > 0)
			{
				GraphicsState graphicsState2 = g.Save();
				g.SetClip(new Rectangle(rect.X, rect.Y + rect.Height - num13, rect.Width, num13));
				g.Fill(brush, path);
				g.Restore(graphicsState2);
			}
		}
		else
		{
			int num14 = (int)((float)rect.Width * LoadingWaveValue);
			if (num14 > 0)
			{
				GraphicsState graphicsState3 = g.Save();
				g.SetClip(new Rectangle(rect.X, rect.Y, num14, rect.Height));
				g.Fill(brush, path);
				g.Restore(graphicsState3);
			}
		}
	}

	private Size MeasureText(Canvas g, string? text, Rectangle rect, out int txt_height, out bool multiLine)
	{
		Size result = g.MeasureText("龍Qq", Font);
		txt_height = result.Height;
		multiLine = false;
		if (text == null)
		{
			return result;
		}
		Size result2 = g.MeasureText(text, Font);
		if (result2.Width > rect.Width && (textMultiLine || text.Contains("\n")))
		{
			multiLine = true;
			return g.MeasureText(text, Font, rect.Width);
		}
		return result2;
	}

	private Size MeasureText(Canvas g, string? text, out int txt_height)
	{
		Size result = g.MeasureText("龍Qq", Font);
		txt_height = result.Height;
		if (text == null)
		{
			return result;
		}
		return g.MeasureText(text, Font);
	}

	private void PaintTextLoading(Canvas g, string? text, Color color, Rectangle rect_read, bool enabled, float radius)
	{
		if (enabled && hasFocus && Config.FocusBorderEnabled && WaveSize > 0)
		{
			float num = (float)WaveSize * Dpi / 2f;
			float num2 = num * 2f;
			float num3 = radius + num;
			using GraphicsPath path = Helper.PathJoin(new RectangleF((float)rect_read.X - num, (float)rect_read.Y - num, (float)rect_read.Width + num2, (float)rect_read.Height + num2), num3, shape, joinMode, joinLeft, joinRight);
			g.Draw(Colour.PrimaryBorder.Get(ColorScheme, "Button", Name), num, path);
		}
		bool flag = loading && LoadingValue > -1f;
		Size font_size = MeasureText(g, text, rect_read, out var txt_height, out var multiLine);
		if (virtualWidth.HasValue)
		{
			font_size.Width = virtualWidth.Value;
		}
		if (text == null || displayStyle == TButtonDisplayStyle.Image)
		{
			Rectangle iconRectCenter = GetIconRectCenter(font_size, rect_read);
			if (flag)
			{
				PaintLoading(g, iconRectCenter, color, rect_read.Height, 0.06f);
			}
			else if (PaintIcon(g, color, iconRectCenter, hastxt: false, enabled) && showArrow)
			{
				int num4 = (int)((float)txt_height * IconRatio);
				Rectangle rect = new Rectangle(rect_read.X + (rect_read.Width - num4) / 2, rect_read.Y + (rect_read.Height - num4) / 2, num4, num4);
				PaintTextArrow(g, rect, color);
			}
			return;
		}
		if (textMultiLine && font_size.Width > rect_read.Width)
		{
			font_size.Width = rect_read.Width;
		}
		Rectangle rect_text;
		if (displayStyle == TButtonDisplayStyle.Default)
		{
			bool flag2 = flag || HasIcon;
			bool flag3 = showArrow;
			if (flag2 | flag3)
			{
				if (flag2 & flag3)
				{
					rect_text = RectAlignLR(g, txt_height, multiLine, Font, iconPosition, iconratio, icongap, font_size, rect_read, out var rect_l, out var rect_r);
					if (flag)
					{
						PaintLoading(g, rect_l, color, rect_l.Height, 0.14f);
					}
					else
					{
						PaintIcon(g, color, rect_l, hastxt: true, enabled);
					}
					PaintTextArrow(g, rect_r, color);
				}
				else if (flag2)
				{
					rect_text = RectAlignL(g, txt_height, multiLine, textCenterHasIcon, Font, iconPosition, iconratio, icongap, font_size, rect_read, out var rect_l2);
					if (flag)
					{
						PaintLoading(g, rect_l2, color, rect_l2.Height, 0.14f);
					}
					else
					{
						PaintIcon(g, color, rect_l2, hastxt: true, enabled);
					}
				}
				else
				{
					rect_text = RectAlignR(g, txt_height, multiLine, Font, iconPosition, iconratio, icongap, font_size, rect_read, out var rect_r2);
					PaintTextArrow(g, rect_r2, color);
				}
			}
			else
			{
				int num5 = (int)((float)txt_height * 0.4f);
				int num6 = num5 * 2;
				rect_text = new Rectangle(rect_read.X + num5, rect_read.Y + num5, rect_read.Width - num6, rect_read.Height - num6);
				PaintTextAlign(rect_read, ref rect_text);
			}
		}
		else if (flag)
		{
			rect_text = RectAlignL(g, txt_height, multiLine, textCenterHasIcon, Font, iconPosition, iconratio, icongap, font_size, rect_read, out var rect_l3);
			PaintLoading(g, rect_l3, color, rect_l3.Height, 0.14f);
		}
		else
		{
			int num7 = (int)((float)txt_height * 0.4f);
			int num8 = num7 * 2;
			rect_text = new Rectangle(rect_read.X + num7, rect_read.Y + num7, rect_read.Width - num8, rect_read.Height - num8);
			PaintTextAlign(rect_read, ref rect_text);
		}
		g.DrawText(text, Font, color, rect_text, sf);
	}

	private void PaintLoading(Canvas g, Rectangle rect, Color color, int size, float ratio)
	{
		g.PaintLoading(LoadingIcon, LoadingSvg, rect, AnimationLoadingValue, LoadingValue, color, size, ratio);
	}

	private void PaintTextArrow(Canvas g, Rectangle rect, Color color)
	{
		using Pen pen = new Pen(color, 2f * Dpi);
		LineCap startCap = (pen.EndCap = LineCap.Round);
		pen.StartCap = startCap;
		if (isLink)
		{
			g.DrawLines(pen, rect.TriangleLinesVertical(ArrowProg));
		}
		else
		{
			g.DrawLines(pen, rect.TriangleLinesHorizontal(ArrowProg));
		}
	}

	internal static Rectangle RectAlignL(Canvas g, int font_Height, bool multiLine, bool textCenter, Font font, TAlignMini iconPosition, float iconratio, float icongap, Size font_size, Rectangle rect_read, out Rectangle rect_l)
	{
		int num = (int)((float)font_Height * iconratio);
		int num2 = (int)((float)font_Height * icongap);
		if (multiLine && (iconPosition == TAlignMini.Left || iconPosition == TAlignMini.Right))
		{
			int num3 = num + num2;
			if (font_size.Width + num3 > rect_read.Width)
			{
				font_size.Width = rect_read.Width - num3;
			}
		}
		Rectangle result;
		if (textCenter)
		{
			switch (iconPosition)
			{
			case TAlignMini.Top:
			{
				int num7 = rect_read.Y + (rect_read.Height - font_size.Height) / 2;
				result = new Rectangle(rect_read.X, num7, rect_read.Width, font_size.Height);
				rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num7 - num - num2, num, num);
				break;
			}
			case TAlignMini.Bottom:
			{
				int num6 = rect_read.Y + (rect_read.Height - font_size.Height) / 2;
				result = new Rectangle(rect_read.X, num6, rect_read.Width, font_size.Height);
				rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num6 + font_size.Height + num2, num, num);
				break;
			}
			case TAlignMini.Right:
			{
				int num5 = rect_read.X + (rect_read.Width - font_size.Width) / 2;
				result = new Rectangle(num5, rect_read.Y, font_size.Width, rect_read.Height);
				rect_l = new Rectangle(num5 + font_size.Width + num2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				break;
			}
			default:
			{
				int num4 = rect_read.X + (rect_read.Width - font_size.Width) / 2;
				result = new Rectangle(num4, rect_read.Y, font_size.Width, rect_read.Height);
				rect_l = new Rectangle(num4 - num - num2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				break;
			}
			}
		}
		else
		{
			switch (iconPosition)
			{
			case TAlignMini.Top:
			{
				int num11 = rect_read.Y + (rect_read.Height - (font_size.Height + num + num2)) / 2;
				result = new Rectangle(rect_read.X, num11 + num + num2, rect_read.Width, font_size.Height);
				rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num11, num, num);
				break;
			}
			case TAlignMini.Bottom:
			{
				int num10 = rect_read.Y + (rect_read.Height - (font_size.Height + num + num2)) / 2;
				result = new Rectangle(rect_read.X, num10, rect_read.Width, font_size.Height);
				rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num10 + font_size.Height + num2, num, num);
				break;
			}
			case TAlignMini.Right:
			{
				int num9 = rect_read.X + (rect_read.Width - (font_size.Width + num + num2)) / 2;
				result = new Rectangle(num9, rect_read.Y, font_size.Width, rect_read.Height);
				rect_l = new Rectangle(num9 + font_size.Width + num2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				break;
			}
			default:
			{
				int num8 = rect_read.X + (rect_read.Width - (font_size.Width + num + num2)) / 2;
				result = new Rectangle(num8 + num + num2, rect_read.Y, font_size.Width, rect_read.Height);
				rect_l = new Rectangle(num8, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				break;
			}
			}
		}
		return result;
	}

	internal static Rectangle RectAlignLR(Canvas g, int font_Height, bool multiLine, Font font, TAlignMini iconPosition, float iconratio, float icongap, Size font_size, Rectangle rect_read, out Rectangle rect_l, out Rectangle rect_r)
	{
		int num = (int)((float)font_Height * iconratio);
		int num2 = (int)((float)font_Height * icongap);
		int num3 = (int)((float)font_Height * 0.4f);
		if (multiLine && (iconPosition == TAlignMini.Left || iconPosition == TAlignMini.Right))
		{
			int num4 = (num + num2) * 2;
			if (font_size.Width + num4 > rect_read.Width)
			{
				font_size.Width = rect_read.Width - num4;
			}
		}
		Rectangle result;
		switch (iconPosition)
		{
		case TAlignMini.Top:
		{
			int num10 = rect_read.Y + (rect_read.Height - (font_size.Height + num + num2)) / 2;
			result = new Rectangle(rect_read.X, num10 + num + num2, rect_read.Width, font_size.Height);
			rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num10, num, num);
			rect_r = new Rectangle(rect_read.Right - num - num3, result.Y + (result.Height - num) / 2, num, num);
			break;
		}
		case TAlignMini.Bottom:
		{
			int num9 = rect_read.Y + (rect_read.Height - (font_size.Height + num + num2)) / 2;
			result = new Rectangle(rect_read.X, num9, rect_read.Width, font_size.Height);
			rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num9 + font_size.Height + num2, num, num);
			rect_r = new Rectangle(rect_read.Right - num - num3, result.Y + (result.Height - num) / 2, num, num);
			break;
		}
		case TAlignMini.Right:
		{
			int num7 = rect_read.X + (rect_read.Width - (font_size.Width + num + num2 + num3)) / 2;
			int num8 = rect_read.Y + (rect_read.Height - num) / 2;
			result = new Rectangle(num7, rect_read.Y, font_size.Width, rect_read.Height);
			rect_l = new Rectangle(num7 + font_size.Width + num2, num8, num, num);
			rect_r = new Rectangle(rect_read.X + num3, num8, num, num);
			break;
		}
		default:
		{
			int num5 = rect_read.X + (rect_read.Width - (font_size.Width + num + num2 + num3)) / 2;
			int num6 = rect_read.Y + (rect_read.Height - num) / 2;
			result = new Rectangle(num5 + num + num2, rect_read.Y, font_size.Width, rect_read.Height);
			rect_l = new Rectangle(num5, num6, num, num);
			rect_r = new Rectangle(rect_read.Right - num - num3, num6, num, num);
			break;
		}
		}
		return result;
	}

	internal static Rectangle RectAlignR(Canvas g, int font_Height, bool multiLine, Font font, TAlignMini iconPosition, float iconratio, float icongap, Size font_size, Rectangle rect_read, out Rectangle rect_r)
	{
		int num = (int)((float)font_Height * iconratio);
		int num2 = (int)((float)font_Height * icongap);
		int num3 = (int)((float)font_Height * 0.4f);
		int num4 = num + num2;
		if (multiLine && (iconPosition == TAlignMini.Left || iconPosition == TAlignMini.Right))
		{
			int num5 = num + num2;
			if (font_size.Width + num5 > rect_read.Width)
			{
				font_size.Width = rect_read.Width - num5;
			}
		}
		Rectangle result;
		switch (iconPosition)
		{
		case TAlignMini.Right:
		case TAlignMini.Bottom:
			result = new Rectangle(rect_read.X + num4, rect_read.Y, rect_read.Width - num4, rect_read.Height);
			rect_r = new Rectangle(rect_read.X + num3, rect_read.Y + (rect_read.Height - num) / 2, num, num);
			break;
		default:
			result = new Rectangle(rect_read.X, rect_read.Y, rect_read.Width - num4, rect_read.Height);
			rect_r = new Rectangle(rect_read.Right - num - num3, rect_read.Y + (rect_read.Height - num) / 2, num, num);
			break;
		}
		return result;
	}

	private void PaintTextAlign(Rectangle rect_read, ref Rectangle rect_text)
	{
		switch (textAlign)
		{
		case ContentAlignment.MiddleLeft:
		case ContentAlignment.MiddleCenter:
		case ContentAlignment.MiddleRight:
			rect_text.Y = rect_read.Y;
			rect_text.Height = rect_read.Height;
			break;
		case ContentAlignment.TopLeft:
		case ContentAlignment.TopCenter:
		case ContentAlignment.TopRight:
			rect_text.Height = rect_read.Height - rect_text.Y;
			break;
		}
	}

	/// <summary>
	/// 居中的图标绘制区域
	/// </summary>
	/// <param name="font_size">字体大小</param>
	/// <param name="rect_read">客户区域</param>
	private Rectangle GetIconRectCenter(Size font_size, Rectangle rect_read)
	{
		if (IconSize.Width > 0 && IconSize.Height > 0)
		{
			int num = (int)((float)IconSize.Width * Dpi);
			int num2 = (int)((float)IconSize.Height * Dpi);
			return new Rectangle(rect_read.X + (rect_read.Width - num) / 2, rect_read.Y + (rect_read.Height - num2) / 2, num, num2);
		}
		int num3 = (int)((float)font_size.Height * IconRatio * 1.125f);
		return new Rectangle(rect_read.X + (rect_read.Width - num3) / 2, rect_read.Y + (rect_read.Height - num3) / 2, num3, num3);
	}

	/// <summary>
	/// 渲染图标
	/// </summary>
	/// <param name="g">GDI</param>
	/// <param name="color">颜色</param>
	/// <param name="rect_o">区域</param>
	/// <param name="hastxt">包含文本</param>
	/// <param name="enabled">使能</param>
	private bool PaintIcon(Canvas g, Color? color, Rectangle rect_o, bool hastxt, bool enabled)
	{
		Rectangle rect = (hastxt ? GetIconRect(rect_o) : rect_o);
		if (AnimationIconHover)
		{
			PaintCoreIcon(g, rect, color, 1f - AnimationIconHoverValue);
			PaintCoreIconHover(g, rect, color, AnimationIconHoverValue);
			return false;
		}
		if (AnimationIconToggle)
		{
			float opacity = 1f - AnimationIconToggleValue;
			if (ExtraMouseHover)
			{
				if (!PaintCoreIcon(g, IconHover, IconHoverSvg, rect, color, opacity))
				{
					PaintCoreIcon(g, icon, iconSvg, rect, color, opacity);
				}
				if (!PaintCoreIcon(g, ToggleIconHover, ToggleIconHoverSvg, rect, color, AnimationIconToggleValue))
				{
					PaintCoreIcon(g, ToggleIcon, ToggleIconSvg, rect, color, AnimationIconToggleValue);
				}
			}
			else
			{
				PaintCoreIcon(g, icon, iconSvg, rect, color, opacity);
				PaintCoreIcon(g, iconToggle, iconSvgToggle, rect, color, AnimationIconToggleValue);
			}
			return false;
		}
		if (enabled)
		{
			if (ExtraMouseHover && PaintCoreIconHover(g, rect, color))
			{
				return false;
			}
			if (PaintCoreIcon(g, rect, color))
			{
				return false;
			}
		}
		else
		{
			if (ExtraMouseHover && PaintCoreIconHover(g, rect, color, 0.3f))
			{
				return false;
			}
			if (PaintCoreIcon(g, rect, color, 0.3f))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 图标绘制区域
	/// </summary>
	/// <param name="rectl">图标区域</param>
	private Rectangle GetIconRect(Rectangle rectl)
	{
		if (IconSize.Width > 0 && IconSize.Height > 0)
		{
			int num = (int)((float)IconSize.Width * Dpi);
			int num2 = (int)((float)IconSize.Height * Dpi);
			return new Rectangle(rectl.X + (rectl.Width - num) / 2, rectl.Y + (rectl.Height - num2) / 2, num, num2);
		}
		return rectl;
	}

	private bool PaintCoreIcon(Canvas g, Rectangle rect, Color? color, float opacity = 1f)
	{
		if (!toggle)
		{
			return PaintCoreIcon(g, icon, iconSvg, rect, color, opacity);
		}
		return PaintCoreIcon(g, iconToggle, iconSvgToggle, rect, color, opacity);
	}

	private bool PaintCoreIconHover(Canvas g, Rectangle rect, Color? color, float opacity = 1f)
	{
		if (!toggle)
		{
			return PaintCoreIcon(g, IconHover, IconHoverSvg, rect, color, opacity);
		}
		return PaintCoreIcon(g, ToggleIconHover, ToggleIconHoverSvg, rect, color, opacity);
	}

	private bool PaintCoreIcon(Canvas g, Image? icon, string? iconSvg, Rectangle rect, Color? color, float opacity = 1f)
	{
		int num = 0;
		if (icon != null)
		{
			g.Image(icon, rect, opacity);
			num++;
		}
		if (iconSvg != null && g.Svg(iconSvg, rect, opacity, color))
		{
			num++;
		}
		return num > 0;
	}

	private Color GetColorO()
	{
		if (toggle)
		{
			if (typeToggle.HasValue)
			{
				return GetColorO(typeToggle.Value);
			}
			return GetColorO(type);
		}
		return GetColorO(type);
	}

	private Color GetColorO(TTypeMini type)
	{
		Color result = type switch
		{
			TTypeMini.Default => (!(borderWidth > 0f)) ? Colour.FillSecondary.Get(ColorScheme, "Button", Name) : Colour.PrimaryHover.Get(ColorScheme, "Button", Name), 
			TTypeMini.Success => Colour.SuccessHover.Get(ColorScheme, "Button", Name), 
			TTypeMini.Error => Colour.ErrorHover.Get(ColorScheme, "Button", Name), 
			TTypeMini.Info => Colour.InfoHover.Get(ColorScheme, "Button", Name), 
			TTypeMini.Warn => Colour.WarningHover.Get(ColorScheme, "Button", Name), 
			_ => Colour.PrimaryHover.Get(ColorScheme, "Button", Name), 
		};
		if (BackHover.HasValue)
		{
			result = BackHover.Value;
		}
		return result;
	}

	private void GetDefaultColorConfig(out Color Fore, out Color Color, out Color backHover, out Color backActive)
	{
		Fore = Colour.DefaultColor.Get(ColorScheme, "Button", Name);
		Color = Colour.Primary.Get(ColorScheme, "Button", Name);
		if (borderWidth > 0f)
		{
			backHover = Colour.PrimaryHover.Get(ColorScheme, "Button", Name);
			backActive = Colour.PrimaryActive.Get(ColorScheme, "Button", Name);
		}
		else
		{
			backHover = Colour.FillSecondary.Get(ColorScheme, "Button", Name);
			backActive = Colour.Fill.Get(ColorScheme, "Button", Name);
		}
		if (toggle)
		{
			if (foreToggle.HasValue)
			{
				Fore = foreToggle.Value;
			}
			if (ToggleBackHover.HasValue)
			{
				backHover = ToggleBackHover.Value;
			}
			if (ToggleBackActive.HasValue)
			{
				backActive = ToggleBackActive.Value;
			}
		}
		else
		{
			if (fore.HasValue)
			{
				Fore = fore.Value;
			}
			if (BackHover.HasValue)
			{
				backHover = BackHover.Value;
			}
			if (BackActive.HasValue)
			{
				backActive = BackActive.Value;
			}
		}
		if (AnimationBlinkState && colorBlink.HasValue)
		{
			Color = colorBlink.Value;
		}
		if (loading && LoadingValue > -1f)
		{
			Fore = Color.FromArgb(165, Fore);
			Color = Color.FromArgb(165, Color);
		}
	}

	private void GetColorConfig(out Color Fore, out Color foreHover, out Color foreActive, out Color Back, out Color backHover, out Color backActive)
	{
		if (toggle)
		{
			if (typeToggle.HasValue)
			{
				GetColorConfig(typeToggle.Value, out Fore, out Back, out backHover, out backActive);
			}
			else
			{
				GetColorConfig(type, out Fore, out Back, out backHover, out backActive);
			}
			if (foreToggle.HasValue)
			{
				Fore = foreToggle.Value;
			}
			foreHover = (foreActive = Fore);
			if (ToggleForeHover.HasValue)
			{
				foreHover = ToggleForeHover.Value;
			}
			if (ToggleForeActive.HasValue)
			{
				foreActive = ToggleForeActive.Value;
			}
			if (backToggle.HasValue)
			{
				Back = backToggle.Value;
			}
			if (ToggleBackHover.HasValue)
			{
				backHover = ToggleBackHover.Value;
			}
			if (ToggleBackActive.HasValue)
			{
				backActive = ToggleBackActive.Value;
			}
			if (loading && LoadingValue > -1f)
			{
				Back = Color.FromArgb(165, Back);
			}
			return;
		}
		GetColorConfig(type, out Fore, out Back, out backHover, out backActive);
		if (fore.HasValue)
		{
			Fore = fore.Value;
		}
		foreHover = (foreActive = Fore);
		if (ForeHover.HasValue)
		{
			foreHover = ForeHover.Value;
		}
		if (ForeActive.HasValue)
		{
			foreActive = ForeActive.Value;
		}
		if (back.HasValue)
		{
			Back = back.Value;
		}
		if (BackHover.HasValue)
		{
			backHover = BackHover.Value;
		}
		if (BackActive.HasValue)
		{
			backActive = BackActive.Value;
		}
		if (AnimationBlinkState && colorBlink.HasValue)
		{
			Back = colorBlink.Value;
		}
		if (loading && LoadingValue > -1f)
		{
			Back = Color.FromArgb(165, Back);
		}
	}

	private void GetColorConfig(TTypeMini type, out Color Fore, out Color Back, out Color backHover, out Color backActive)
	{
		switch (type)
		{
		case TTypeMini.Error:
			Back = Colour.Error.Get(ColorScheme, "Button", Name);
			Fore = Colour.ErrorColor.Get(ColorScheme, "Button", Name);
			backHover = Colour.ErrorHover.Get(ColorScheme, "Button", Name);
			backActive = Colour.ErrorActive.Get(ColorScheme, "Button", Name);
			break;
		case TTypeMini.Success:
			Back = Colour.Success.Get(ColorScheme, "Button", Name);
			Fore = Colour.SuccessColor.Get(ColorScheme, "Button", Name);
			backHover = Colour.SuccessHover.Get(ColorScheme, "Button", Name);
			backActive = Colour.SuccessActive.Get(ColorScheme, "Button", Name);
			break;
		case TTypeMini.Info:
			Back = Colour.Info.Get(ColorScheme, "Button", Name);
			Fore = Colour.InfoColor.Get(ColorScheme, "Button", Name);
			backHover = Colour.InfoHover.Get(ColorScheme, "Button", Name);
			backActive = Colour.InfoActive.Get(ColorScheme, "Button", Name);
			break;
		case TTypeMini.Warn:
			Back = Colour.Warning.Get(ColorScheme, "Button", Name);
			Fore = Colour.WarningColor.Get(ColorScheme, "Button", Name);
			backHover = Colour.WarningHover.Get(ColorScheme, "Button", Name);
			backActive = Colour.WarningActive.Get(ColorScheme, "Button", Name);
			break;
		default:
			Back = Colour.Primary.Get(ColorScheme, "Button", Name);
			Fore = Colour.PrimaryColor.Get(ColorScheme, "Button", Name);
			backHover = Colour.PrimaryHover.Get(ColorScheme, "Button", Name);
			backActive = Colour.PrimaryActive.Get(ColorScheme, "Button", Name);
			break;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (RespondRealAreas)
		{
			using GraphicsPath graphicsPath = Helper.PathJoin(ReadRectangle, radius, Dpi, shape, joinMode, joinLeft, joinRight);
			ExtraMouseHover = graphicsPath.IsVisible(e.X, e.Y);
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		if (!RespondRealAreas)
		{
			ExtraMouseHover = true;
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnLeave(EventArgs e)
	{
		base.OnLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (CanClick(e.X, e.Y))
		{
			clicks = e.Clicks;
			init = false;
			Focus();
			ExtraMouseDown = true;
			base.OnMouseDown(e);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		Helper.GenerateMouseEventArgs(ref e, clicks);
		base.OnMouseUp(e);
		if (!ExtraMouseDown)
		{
			return;
		}
		ExtraMouseDown = false;
		if (CanClick(e.X, e.Y))
		{
			if (e.Button == MouseButtons.Left)
			{
				ClickAnimation();
				OnClick(e);
			}
			OnMouseClick(e);
		}
	}

	protected virtual void OnToggleChanged(bool e)
	{
		ToggleChanged?.Invoke(this, new BoolEventArgs(e));
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

	protected override void OnKeyUp(KeyEventArgs e)
	{
		base.OnKeyUp(e);
		if (e.KeyCode == Keys.Space && CanClick())
		{
			ClickAnimation();
			OnClick(EventArgs.Empty);
			e.Handled = true;
		}
	}

	/// <summary>
	/// 是否默认按钮
	/// </summary>
	public void NotifyDefault(bool value)
	{
	}

	public void PerformClick()
	{
		if (CanSelect && CanClick())
		{
			ClickAnimation();
			OnClick(EventArgs.Empty);
		}
	}

	private bool CanClick(int x, int y)
	{
		if (loading)
		{
			return LoadingRespondClick;
		}
		if (RespondRealAreas)
		{
			using (GraphicsPath graphicsPath = Helper.PathJoin(ReadRectangle, radius, Dpi, shape, joinMode, joinLeft, joinRight))
			{
				return graphicsPath.IsVisible(x, y);
			}
		}
		return ClientRectangle.Contains(x, y);
	}

	private bool CanClick()
	{
		if (loading)
		{
			return LoadingRespondClick;
		}
		return true;
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		if (init)
		{
			HasFocus = true;
		}
	}

	protected override void OnLostFocus(EventArgs e)
	{
		HasFocus = false;
		base.OnLostFocus(e);
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

	protected override void OnClick(EventArgs e)
	{
		if (AutoToggle)
		{
			Toggle = !toggle;
		}
		Form form = FindForm();
		if (form != null)
		{
			form.DialogResult = DialogResult;
		}
		base.OnClick(e);
	}

	protected override void Dispose(bool disposing)
	{
		ThreadClick?.Dispose();
		ThreadHover?.Dispose();
		ThreadIconHover?.Dispose();
		ThreadIconToggle?.Dispose();
		ThreadLoading?.Dispose();
		ThreadAnimateBlink?.Dispose();
		base.Dispose(disposing);
	}
}
