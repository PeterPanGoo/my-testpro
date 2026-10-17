using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Input 输入框
/// </summary>
/// <remarks>通过鼠标或键盘输入内容，是最基础的表单域的包装。</remarks>
/// <summary>
/// 透明文本框
/// </summary>
[Description("Input 输入框")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
[DefaultEvent("TextChanged")]
public class Input : IControl, IEventListener
{
	/// <summary>
	/// 无障碍对象：将 Input 暴露为可编辑文本（MSAA Text 角色 + 值），经系统 MSAA/UIA 桥接为 Edit 控件类型，供屏幕阅读器、系统触摸键盘与第三方屏幕键盘识别并自动唤起
	/// </summary>
	private class InputAccessibleObject : ControlAccessibleObject
	{
		private readonly Input input;

		/// <summary>
		/// 文本角色（MSAA ROLE_SYSTEM_TEXT，UIA 映射为 Edit）
		/// </summary>
		public override AccessibleRole Role => AccessibleRole.Text;

		public override string? Name => input.AccessibleName ?? input.PlaceholderText;

		public override string? Value
		{
			get
			{
				return input.Text;
			}
			set
			{
				if (!input.readOnly && value != null)
				{
					input.Text = value;
				}
			}
		}

		public override AccessibleStates State
		{
			get
			{
				AccessibleStates accessibleStates = base.State;
				if (input.readOnly)
				{
					accessibleStates |= AccessibleStates.ReadOnly;
				}
				return accessibleStates;
			}
		}

		public InputAccessibleObject(Input owner)
			: base(owner)
		{
			input = owner;
		}
	}

	public class TextStyle
	{
		public int Start { get; set; }

		public int Length { get; set; }

		/// <summary>
		/// 字体
		/// </summary>
		public Font? Font { get; set; }

		/// <summary>
		/// 文本颜色
		/// </summary>
		public Color? Fore { get; set; }

		/// <summary>
		/// 背景色
		/// </summary>
		public Color? Back { get; set; }

		public TextStyle(int start, int length)
		{
			Start = start;
			Length = length;
		}

		public TextStyle(int start, int length, Font? font, Color? fore, Color? back)
		{
			Start = start;
			Length = length;
			Font = font;
			Fore = fore;
			Back = back;
		}

		public TextStyle Set(int start, int length)
		{
			Start = start;
			Length = length;
			return this;
		}

		public TextStyle SetStart(int value)
		{
			Start = value;
			return this;
		}

		public TextStyle SetLength(int value)
		{
			Length = value;
			return this;
		}

		public TextStyle SetFont(Font? value)
		{
			Font = value;
			return this;
		}

		public TextStyle SetFore(Color? value)
		{
			Fore = value;
			return this;
		}

		public TextStyle SetBack(Color? value)
		{
			Back = value;
			return this;
		}
	}

	internal class ICaret
	{
		private Input control;

		public Rectangle Rect;

		public bool ReadShow;

		private bool show;

		internal bool flag;

		private AnimationTask? CaretPrint;

		public int X
		{
			get
			{
				return Rect.X;
			}
			set
			{
				if (Rect.X != value || !flag)
				{
					Rect.X = value;
					flag = true;
					control.Invalidate();
				}
			}
		}

		public int Y => Rect.Y;

		public int Width => Rect.Width;

		public int Height
		{
			get
			{
				return Rect.Height;
			}
			set
			{
				Rect.Height = value;
			}
		}

		public bool Show
		{
			get
			{
				return show;
			}
			set
			{
				if (show == value)
				{
					return;
				}
				show = value;
				CaretPrint?.Dispose();
				CaretPrint = null;
				if (!control.IsHandleCreated)
				{
					return;
				}
				if (show)
				{
					flag = true;
					if (ReadShow)
					{
						show = false;
						return;
					}
					CaretPrint = new AnimationTask(control, () =>
					{
						Flag = !flag;
						return show;
					}, control.CaretSpeed, null, control.CaretSpeed);
					if (control.SetCaretPostion())
					{
						control.Invalidate();
					}
				}
				else
				{
					CaretPrint = null;
				}
				control.Invalidate();
			}
		}

		public bool Flag
		{
			get
			{
				return flag;
			}
			set
			{
				if (flag != value)
				{
					flag = value;
					if (show)
					{
						control.Invalidate();
					}
				}
			}
		}

		public ICaret(Input input)
		{
			control = input;
			Rect = new Rectangle(-1, -1000, (input.Dpi < 1f) ? 1 : ((int)input.Dpi), 0);
		}

		public Rectangle SetXY(CacheCaret[] cache_font, int i, out bool rd)
		{
			CacheCaret cacheCaret = cache_font[i];
			rd = SetXY(cacheCaret.x, cacheCaret.y);
			return new Rectangle(cacheCaret.x, cacheCaret.y, Rect.Width, Rect.Height);
		}

		public bool SetXY(int x, int y)
		{
			if (Rect.X == x && Rect.Y == y)
			{
				return false;
			}
			Rect.X = x;
			Rect.Y = y;
			flag = true;
			return true;
		}

		public void Dispose()
		{
			CaretPrint?.Dispose();
		}
	}

	internal class TextHistoryRecord
	{
		public int SelectionStart { get; set; }

		public int SelectionLength { get; set; }

		public string Text { get; set; }

		public TextHistoryRecord(Input input)
		{
			SelectionStart = input.SelectionStart;
			SelectionLength = input.SelectionLength;
			Text = input.Text;
		}
	}

	/// <summary>
	/// 快捷键枚举
	/// </summary>
	public enum ShortcutKeys
	{
		None,
		/// <summary>
		/// BackSpace
		/// </summary>
		Back,
		/// <summary>
		/// BackSpace
		/// </summary>
		BackControl,
		/// <summary>
		/// 删除
		/// </summary>
		Delete,
		/// <summary>
		/// 制表符
		/// </summary>
		Tab,
		/// <summary>
		/// 回车
		/// </summary>
		Enter,
		/// <summary>
		/// 全选
		/// </summary>
		ControlA,
		/// <summary>
		/// 复制
		/// </summary>
		ControlC,
		/// <summary>
		/// 剪贴
		/// </summary>
		ControlX,
		/// <summary>
		/// 粘贴
		/// </summary>
		ControlV,
		/// <summary>
		/// 撤消
		/// </summary>
		ControlZ,
		/// <summary>
		/// 重做
		/// </summary>
		ControlY,
		Left,
		LeftShift,
		Right,
		RightShift,
		Up,
		UpShift,
		Down,
		DownShift,
		Home,
		HomeShift,
		HomeControl,
		HomeControlShift,
		End,
		EndShift,
		EndControl,
		EndControlShift,
		PageUp,
		PageDown
	}

	internal class CacheFont
	{
		public int i { get; set; }

		public int line { get; set; }

		public string text { get; set; }

		public Rectangle rect { get; set; }

		public bool ret { get; set; }

		public bool hide { get; set; }

		public bool emoji { get; set; }

		public int width { get; set; }

		/// <summary>
		/// 字符簇方向
		/// </summary>
		public TextDirection dir { get; set; }

		/// <summary>
		/// 字体
		/// </summary>
		public Font? font { get; set; }

		/// <summary>
		/// 文本颜色
		/// </summary>
		public Color? fore { get; set; }

		/// <summary>
		/// 背景色
		/// </summary>
		public Color? back { get; set; }

		public CacheFont(int index, string _text, bool _emoji, int _width)
		{
			i = index;
			text = _text;
			emoji = _emoji;
			width = _width;
		}

		public override string ToString()
		{
			return text;
		}
	}

	internal class CacheCaret
	{
		public int x { get; set; }

		public int y { get; set; }

		/// <summary>
		/// Caret 内序号
		/// </summary>
		public int index { get; set; }

		/// <summary>
		/// 真实的文本序号
		/// </summary>
		public int i { get; set; }
	}

	public class ScrollInfo
	{
		internal Input control;

		private bool scrollhover;

		internal int val;

		public Rectangle Rect { get; set; }

		public RectangleF Slider { get; set; }

		/// <summary>
		/// 滑块全高度（不算边距）
		/// </summary>
		public float SliderFull { get; set; }

		public bool Hover
		{
			get
			{
				return scrollhover;
			}
			set
			{
				if (scrollhover != value)
				{
					scrollhover = value;
					control.Invalidate();
				}
			}
		}

		public int Value
		{
			get
			{
				return val;
			}
			set
			{
				if (SetValue(value))
				{
					control.Invalidate();
				}
			}
		}

		public int Min { get; set; }

		public int Max { get; set; }

		public bool Show { get; set; }

		public bool Down { get; set; }

		public ScrollInfo(Input input)
		{
			control = input;
		}

		public virtual bool SetValue(int value)
		{
			if (value > Max)
			{
				value = Max;
			}
			if (value < Min)
			{
				value = Min;
			}
			if (val == value)
			{
				return false;
			}
			val = value;
			if (control.SyncScrollObj is Input input)
			{
				input.ScrollX.Value = val;
			}
			return true;
		}

		public void Clear()
		{
			Show = false;
			int num = (Max = 0);
			int num3 = (Min = num);
			val = num3;
		}
	}

	public class ScrollYInfo : ScrollInfo
	{
		public ScrollYInfo(Input input)
			: base(input)
		{
		}

		public override bool SetValue(int value)
		{
			if (value > Max)
			{
				value = Max;
			}
			if (value < Min)
			{
				value = Min;
			}
			if (val == value)
			{
				return false;
			}
			val = value;
			control.CaretInfo.flag = true;
			if (control.SyncScrollObj is Input input)
			{
				input.ScrollY.Value = val;
			}
			return true;
		}
	}

	[CompilerGenerated]
	private sealed class <>c__DisplayClass472_0
	{
		public Color[] colors;

		public AnimationType animationType;

		public Input <>4__this;

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

	internal Color? fore;

	private Color? back;

	private string? backExtend;

	private Image? backImage;

	private TFit backFit;

	private Color selection = Color.FromArgb(102, 0, 127, 255);

	internal float borderWidth = 1f;

	internal Color? borderColor;

	internal int radius = 6;

	internal bool round;

	private TType status;

	private float iconratio = 0.7f;

	private float? iconratioRight;

	private float icongap = 0.25f;

	private float paddgap = 0.4f;

	private Image? prefix;

	private string? prefixSvg;

	private string? prefixText;

	private Color? prefixFore;

	private Image? suffix;

	private string? suffixSvg;

	private string? suffixText;

	private Color? suffixFore;

	private TJoinMode joinMode;

	private bool allowclear;

	private bool is_clear;

	private bool is_clear_down;

	private bool is_prefix_down;

	private bool is_suffix_down;

	private bool autoscroll;

	private bool loading;

	private int AnimationLoadingValue;

	private int AnimationLoadingWaveValue;

	internal bool isempty = true;

	private string _text = "";

	private ImeMode imeMode;

	private RightToLeft rightToLeft = RightToLeft.Inherit;

	/// <summary>
	/// 文本是否包含 RTL 字符
	/// </summary>
	internal bool HasRTLText;

	/// <summary>
	/// RTL 连续字符簇宽度待校正
	/// </summary>
	internal bool rtl_run_dirty;

	private int selectionStart;

	private int selectionStartTemp;

	private int selectionLength;

	private bool readOnly;

	private bool multiline;

	private bool wordwrap = true;

	private int lineheight;

	private HorizontalAlignment textalign;

	private string? placeholderText;

	private Color? placeholderColor;

	private string? placeholderColorExtend;

	private bool IsPassWord;

	private string PassWordChar = "●";

	private bool useSystemPasswordChar;

	private char passwordChar;

	private TextStyle[]? styles;

	private bool AnimationFocus;

	private int AnimationFocusValue;

	private IntPtr m_hIMC;

	private Form? _contextMenu;

	internal ICaret CaretInfo;

	private int? tmpkeyx;

	private int CurrentPosIndex;

	private Color? colorBlink;

	private AnimationTask? ThreadAnimateBlink;

	/// <summary>
	/// 闪烁动画状态
	/// </summary>
	public bool AnimationBlinkState;

	private TAutoSize autoSize;

	private AnimationTask? ThreadHover;

	private AnimationTask? ThreadFocus;

	private AnimationTask? ThreadLoading;

	private int history_I = -1;

	private List<TextHistoryRecord> history_Log = new List<TextHistoryRecord>();

	internal bool hasAlt;

	private List<CacheFont>? cache_font;

	private CacheCaret[]? cache_caret;

	private bool HasEmoji;

	/// <summary>
	/// 缓存计算字体
	/// </summary>
	private TFixFont fix_cache_font = new TFixFont();

	internal Rectangle rect_text;

	internal Rectangle rect_l;

	internal Rectangle rect_r;

	internal Rectangle rect_d_ico;

	internal Rectangle rect_d_l;

	internal Rectangle rect_d_r;

	internal Rectangle? RECTDIV;

	internal int UR;

	private bool mDown;

	private bool mDownMove;

	private Point mDownLocation;

	private bool hover_clear;

	private List<string> sptext = new List<string>
	{
		" ", "\r", "\t", "\n", "\r\n", ",", "，", "。", "!", "！",
		"?", "？", ":", "；", "：", ";", "_", "=", "?", "?",
		"?", "\"", "“", "”", "(", ")", "（", "）", "<", ">",
		"【", "】", "《", "》", "/", "\\"
	};

	internal bool _mouseDown;

	internal int AnimationHoverValue;

	internal bool AnimationHover;

	internal bool _mouseHover;

	internal readonly FormatFlags sf_center = FormatFlags.Center | FormatFlags.NoWrap;

	internal readonly FormatFlags sf_center_rtl = FormatFlags.Center | FormatFlags.NoWrap | FormatFlags.DirectionRightToLeft;

	internal FormatFlags sf_placeholder = FormatFlags.NoWrapEllipsis | FormatFlags.VerticalCenter | FormatFlags.Left;

	private string? oldtmpY;

	private string? oldtmpX;

	private object? SyncScrollObj;

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
	/// 选中颜色
	/// </summary>
	[Description("选中颜色")]
	[Category("Appearance")]
	[DefaultValue(typeof(Color), "102, 0, 127, 255")]
	public Color SelectionColor
	{
		get
		{
			return selection;
		}
		set
		{
			if (!(selection == value))
			{
				selection = value;
				Invalidate();
				OnPropertyChanged("SelectionColor");
			}
		}
	}

	/// <summary>
	/// 光标颜色
	/// </summary>
	[Description("光标颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? CaretColor { get; set; }

	/// <summary>
	/// 光标速度
	/// </summary>
	[Description("光标速度")]
	[Category("Appearance")]
	[DefaultValue(1000)]
	public int CaretSpeed { get; set; } = 1000;

	/// <summary>
	/// 确定光标是可见的还是隐藏的
	/// </summary>
	[Description("确定光标是可见的还是隐藏的")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool CaretVisible { get; set; } = true;

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
				CalculateRect();
				Invalidate();
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
				Invalidate();
				OnPropertyChanged("BorderColor");
			}
		}
	}

	/// <summary>
	/// 悬停边框颜色
	/// </summary>
	[Description("悬停边框颜色")]
	[Category("边框")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderHover { get; set; }

	/// <summary>
	/// 激活边框颜色
	/// </summary>
	[Description("激活边框颜色")]
	[Category("边框")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderActive { get; set; }

	/// <summary>
	/// 左右边距
	/// </summary>
	[Description("左右边距")]
	[Category("Appearance")]
	[DefaultValue(0)]
	public int PaddingLR { get; set; }

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
				Invalidate();
				OnPropertyChanged("Radius");
			}
		}
	}

	/// <summary>
	/// 圆角样式
	/// </summary>
	[Description("圆角样式")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Round
	{
		get
		{
			return round;
		}
		set
		{
			if (round != value)
			{
				round = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("Round");
			}
		}
	}

	/// <summary>
	/// 设置校验状态
	/// </summary>
	[Description("设置校验状态")]
	[Category("Appearance")]
	[DefaultValue(TType.None)]
	public TType Status
	{
		get
		{
			return status;
		}
		set
		{
			if (status != value)
			{
				status = value;
				Invalidate();
				OnPropertyChanged("Status");
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
				CalculateRect();
				Invalidate();
				OnPropertyChanged("IconRatio");
			}
		}
	}

	/// <summary>
	/// 右图标比例
	/// </summary>
	[Description("右图标比例")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public float? IconRatioRight
	{
		get
		{
			return iconratioRight;
		}
		set
		{
			if (iconratioRight != value)
			{
				iconratioRight = value;
				if (HasSuffix)
				{
					CalculateRect();
					Invalidate();
				}
				OnPropertyChanged("IconRatioRight");
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
				CalculateRect();
				Invalidate();
				OnPropertyChanged("IconGap");
			}
		}
	}

	/// <summary>
	/// 边框间距比例
	/// </summary>
	[Description("边框间距比例")]
	[Category("Appearance")]
	[DefaultValue(0.4f)]
	public float PaddGap
	{
		get
		{
			return paddgap;
		}
		set
		{
			if (paddgap != value)
			{
				paddgap = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("PaddGap");
			}
		}
	}

	/// <summary>
	/// 前缀
	/// </summary>
	[Description("前缀")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Image? Prefix
	{
		get
		{
			return prefix;
		}
		set
		{
			if (prefix != value)
			{
				prefix = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("Prefix");
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
				CalculateRect();
				Invalidate();
				OnPropertyChanged("PrefixSvg");
			}
		}
	}

	/// <summary>
	/// 前缀文本
	/// </summary>
	[Description("前缀文本")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? PrefixText
	{
		get
		{
			return this.GetLangI(LocalizationPrefixText, prefixText);
		}
		set
		{
			if (!(prefixText == value))
			{
				prefixText = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("PrefixText");
			}
		}
	}

	[Description("前缀文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationPrefixText { get; set; }

	/// <summary>
	/// 前缀前景色
	/// </summary>
	[Description("前缀前景色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? PrefixFore
	{
		get
		{
			return prefixFore;
		}
		set
		{
			if (!(prefixFore == value))
			{
				prefixFore = value;
				if (HasPrefix)
				{
					Invalidate();
				}
				OnPropertyChanged("PrefixFore");
			}
		}
	}

	/// <summary>
	/// 前缀宽度
	/// </summary>
	[Description("前缀宽度")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? PrefixWidth { get; set; }

	/// <summary>
	/// 前缀对齐方式
	/// </summary>
	[Description("前缀对齐方式")]
	[Category("Appearance")]
	[DefaultValue(FormatFlags.Center | FormatFlags.NoWrap)]
	public FormatFlags PrefixFormat { get; set; } = FormatFlags.Center | FormatFlags.NoWrap;

	/// <summary>
	/// 是否包含前缀
	/// </summary>
	public virtual bool HasPrefix
	{
		get
		{
			if (prefixSvg == null)
			{
				return prefix != null;
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
	public Image? Suffix
	{
		get
		{
			return suffix;
		}
		set
		{
			if (suffix != value)
			{
				suffix = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("Suffix");
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
				CalculateRect();
				Invalidate();
				OnPropertyChanged("SuffixSvg");
			}
		}
	}

	/// <summary>
	/// 后缀文本
	/// </summary>
	[Description("后缀文本")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? SuffixText
	{
		get
		{
			return this.GetLangI(LocalizationSuffixText, suffixText);
		}
		set
		{
			if (!(suffixText == value))
			{
				suffixText = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("SuffixText");
			}
		}
	}

	[Description("后缀文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationSuffixText { get; set; }

	/// <summary>
	/// 后缀前景色
	/// </summary>
	[Description("后缀前景色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? SuffixFore
	{
		get
		{
			return suffixFore;
		}
		set
		{
			if (!(suffixFore == value))
			{
				suffixFore = value;
				if (HasSuffix)
				{
					Invalidate();
				}
				OnPropertyChanged("SuffixFore");
			}
		}
	}

	/// <summary>
	/// 后缀宽度
	/// </summary>
	[Description("后缀宽度")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? SuffixWidth { get; set; }

	/// <summary>
	/// 后缀对齐方式
	/// </summary>
	[Description("后缀对齐方式")]
	[Category("Appearance")]
	[DefaultValue(FormatFlags.Center | FormatFlags.NoWrap)]
	public FormatFlags SuffixFormat { get; set; } = FormatFlags.Center | FormatFlags.NoWrap;

	/// <summary>
	/// 是否包含后缀
	/// </summary>
	public virtual bool HasSuffix
	{
		get
		{
			if (suffixSvg == null)
			{
				return suffix != null;
			}
			return true;
		}
	}

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
				CalculateRect();
				Invalidate();
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
	public bool JoinLeft { get; set; }

	/// <summary>
	/// 连接右边
	/// </summary>
	[Obsolete("use JoinMode")]
	[Browsable(false)]
	[Description("连接右边")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool JoinRight { get; set; }

	/// <summary>
	/// 支持清除
	/// </summary>
	[Description("支持清除")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public virtual bool AllowClear
	{
		get
		{
			return allowclear;
		}
		set
		{
			if (allowclear != value)
			{
				allowclear = value;
				if (OnAllowClear())
				{
					CalculateRect();
					Invalidate();
				}
				OnPropertyChanged("AllowClear");
			}
		}
	}

	/// <summary>
	/// 是否显示滚动条
	/// </summary>
	[Description("是否显示滚动条")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public virtual bool AutoScroll
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
				Invalidate();
				OnPropertyChanged("AutoScroll");
			}
		}
	}

	/// <summary>
	/// 处理快捷键
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete]
	[Description("处理快捷键")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool HandShortcutKeys { get; set; } = true;

	/// <summary>
	/// 适配系统助记词
	/// </summary>
	[Description("适配系统助记词")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool AdapterSystemMnemonic { get; set; }

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
			BeforeAutoSize();
			CalculateRect();
			ThreadLoading?.Dispose();
			if (loading)
			{
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
	/// 文本是否为空
	/// </summary>
	public bool IsTextEmpty => isempty;

	/// <summary>
	/// 文本
	/// </summary>
	[Description("文本")]
	[Category("Appearance")]
	[DefaultValue("")]
	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
	public override string Text
	{
		get
		{
			return this.GetLangIN(LocalizationText, _text);
		}
		set
		{
			if (SetText(value, active: false))
			{
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 设置文本后选中到最后
	/// </summary>
	[Description("设置文本后选中到最后")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool SetTextSelectionEnd { get; set; }

	/// <summary>
	/// 文本总行
	/// </summary>
	[Description("文本总行")]
	[Category("Data")]
	[DefaultValue(0)]
	public int TextTotalLine { get; private set; }

	/// <summary>
	/// 多行文本集合
	/// </summary>
	[Description("多行文本集合")]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Editor("System.Windows.Forms.Design.StringArrayEditor", typeof(UITypeEditor))]
	public string[] Lines
	{
		get
		{
			return _text.Split(new string[2]
			{
				"\n",
				Environment.NewLine
			}, StringSplitOptions.None);
		}
		set
		{
			string text = ((value != null && value.Length != 0) ? string.Join(Environment.NewLine, value) : string.Empty);
			if (_text != text)
			{
				Text = text;
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	/// <summary>
	/// Emoji字体
	/// </summary>
	[Description("Emoji字体")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? EmojiFont { get; set; }

	/// <summary>
	/// IME(输入法编辑器)状态
	/// </summary>
	[Description("IME(输入法编辑器)状态")]
	[Category("Behavior")]
	[DefaultValue(ImeMode.NoControl)]
	public new ImeMode ImeMode
	{
		get
		{
			return imeMode;
		}
		set
		{
			if (imeMode != value)
			{
				imeMode = value;
				SetImeMode(value);
				OnPropertyChanged("ImeMode");
			}
		}
	}

	/// <summary>
	/// RTL（从右到左）书写方向；默认 Inherit：文本含 RTL 字符或父容器 RTL 时启用
	/// </summary>
	[Description("RTL（从右到左）书写方向")]
	[Category("Appearance")]
	[DefaultValue(RightToLeft.Inherit)]
	[AmbientValue(RightToLeft.Inherit)]
	public override RightToLeft RightToLeft
	{
		get
		{
			return rightToLeft;
		}
		set
		{
			if (rightToLeft != value)
			{
				rightToLeft = value;
				rtl_run_dirty = true;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("RightToLeft");
			}
		}
	}

	/// <summary>
	/// 是否按 RTL（从右到左）渲染（密码框、OTP 模式强制 LTR）
	/// </summary>
	internal bool IsRTL
	{
		get
		{
			if (!IsPassWord && !ModeRange)
			{
				if (rightToLeft != RightToLeft.Yes)
				{
					if (rightToLeft == RightToLeft.Inherit)
					{
						if (!HasRTLText)
						{
							return base.RightToLeft == RightToLeft.Yes;
						}
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	/// <summary>
	/// 所选文本的起点
	/// </summary>
	[Browsable(false)]
	[DefaultValue(0)]
	public int SelectionStart
	{
		get
		{
			return selectionStart;
		}
		set
		{
			if (SetSelectionStart(value))
			{
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 所选文本的长度
	/// </summary>
	[Browsable(false)]
	[DefaultValue(0)]
	public int SelectionLength
	{
		get
		{
			return selectionLength;
		}
		set
		{
			if (SetSelectionLength(value))
			{
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 多行编辑是否允许输入制表符
	/// </summary>
	[Description("多行编辑是否允许输入制表符")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool AcceptsTab { get; set; }

	/// <summary>
	/// 多行编辑是否允许输入退出键
	/// </summary>
	[Description("多行编辑是否允许输入退出键")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool AcceptsEscape { get; set; }

	/// <summary>
	/// 失去焦点时是否隐藏选定内容
	/// </summary>
	[Description("失去焦点时是否隐藏选定内容")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool HideSelection { get; set; } = true;

	/// <summary>
	/// 使用默认右键菜单
	/// </summary>
	[Description("使用默认右键菜单")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool UseContextMenu { get; set; } = true;

	/// <summary>
	/// 焦点离开清空选中
	/// </summary>
	[Obsolete("use HideSelection")]
	[Description("焦点离开清空选中")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool LostFocusClearSelection
	{
		get
		{
			return HideSelection;
		}
		set
		{
			HideSelection = value;
		}
	}

	/// <summary>
	/// 只读
	/// </summary>
	[Description("只读")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public virtual bool ReadOnly
	{
		get
		{
			return readOnly;
		}
		set
		{
			if (readOnly != value)
			{
				readOnly = value;
				SetImeMode(value ? ImeMode.Disable : imeMode);
				OnPropertyChanged("ReadOnly");
			}
		}
	}

	/// <summary>
	/// 多行文本
	/// </summary>
	[Description("多行文本")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public virtual bool Multiline
	{
		get
		{
			return multiline;
		}
		set
		{
			if (multiline != value)
			{
				multiline = value;
				if (multiline)
				{
					sf_placeholder &= ~(FormatFlags.VerticalCenter | FormatFlags.NoWrap);
					sf_placeholder |= FormatFlags.Top;
				}
				else
				{
					sf_placeholder &= ~FormatFlags.Top;
					sf_placeholder |= FormatFlags.VerticalCenter | FormatFlags.NoWrap;
				}
				CalculateRect();
				Invalidate();
				OnPropertyChanged("Multiline");
			}
		}
	}

	/// <summary>
	/// 自动换行
	/// </summary>
	[Description("自动换行")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public virtual bool WordWrap
	{
		get
		{
			return wordwrap;
		}
		set
		{
			if (wordwrap != value)
			{
				wordwrap = value;
				if (multiline)
				{
					CalculateRect();
					Invalidate();
				}
				OnPropertyChanged("WordWrap");
			}
		}
	}

	[Description("多行行高")]
	[Category("Behavior")]
	[DefaultValue(0)]
	public int LineHeight
	{
		get
		{
			return lineheight;
		}
		set
		{
			if (lineheight != value)
			{
				lineheight = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("LineHeight");
			}
		}
	}

	/// <summary>
	/// 文本对齐方向
	/// </summary>
	[Description("文本对齐方向")]
	[Category("Appearance")]
	[DefaultValue(HorizontalAlignment.Left)]
	public HorizontalAlignment TextAlign
	{
		get
		{
			return textalign;
		}
		set
		{
			if (textalign != value)
			{
				textalign = value;
				sf_placeholder = textalign.SetAlignment(sf_placeholder);
				CalculateRect();
				Invalidate();
				OnPropertyChanged("TextAlign");
			}
		}
	}

	/// <summary>
	/// 水印文本
	/// </summary>
	[Description("水印文本")]
	[Category("Behavior")]
	[DefaultValue(null)]
	[Localizable(true)]
	public virtual string? PlaceholderText
	{
		get
		{
			return this.GetLangI(LocalizationPlaceholderText, placeholderText);
		}
		set
		{
			if (!(placeholderText == value))
			{
				placeholderText = value;
				if (isempty && ShowPlaceholder)
				{
					Invalidate();
				}
				OnPropertyChanged("PlaceholderText");
			}
		}
	}

	[Description("水印文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationPlaceholderText { get; set; }

	/// <summary>
	/// 水印颜色
	/// </summary>
	[Description("水印颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? PlaceholderColor
	{
		get
		{
			return placeholderColor;
		}
		set
		{
			if (!(placeholderColor == value))
			{
				placeholderColor = value;
				if (isempty && ShowPlaceholder)
				{
					Invalidate();
				}
				OnPropertyChanged("PlaceholderColor");
			}
		}
	}

	/// <summary>
	/// 水印渐变色
	/// </summary>
	[Description("水印渐变色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? PlaceholderColorExtend
	{
		get
		{
			return placeholderColorExtend;
		}
		set
		{
			if (!(placeholderColorExtend == value))
			{
				placeholderColorExtend = value;
				if (isempty && ShowPlaceholder)
				{
					Invalidate();
				}
				OnPropertyChanged("PlaceholderColorExtend");
			}
		}
	}

	/// <summary>
	/// 文本最大长度
	/// </summary>
	[Description("文本最大长度")]
	[Category("Behavior")]
	[DefaultValue(32767)]
	public int MaxLength { get; set; } = 32767;

	/// <summary>
	/// 形态
	/// </summary>
	[Description("形态")]
	[Category("Appearance")]
	[DefaultValue(TVariant.Outlined)]
	public TVariant Variant { get; set; }

	/// <summary>
	/// 使用密码框
	/// </summary>
	[Description("使用密码框")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public virtual bool UseSystemPasswordChar
	{
		get
		{
			return useSystemPasswordChar;
		}
		set
		{
			if (useSystemPasswordChar != value)
			{
				useSystemPasswordChar = value;
				SetPassWord();
				OnPropertyChanged("UseSystemPasswordChar");
			}
		}
	}

	/// <summary>
	/// 自定义密码字符
	/// </summary>
	[Description("自定义密码字符")]
	[Category("Behavior")]
	[DefaultValue('\0')]
	public virtual char PasswordChar
	{
		get
		{
			return passwordChar;
		}
		set
		{
			if (passwordChar != value)
			{
				passwordChar = value;
				SetPassWord();
				OnPropertyChanged("PasswordChar");
			}
		}
	}

	/// <summary>
	/// 密码可以复制
	/// </summary>
	[Description("密码可以复制")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool PasswordCopy { get; set; }

	/// <summary>
	/// 密码可以粘贴
	/// </summary>
	[Description("密码可以粘贴")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool PasswordPaste { get; set; } = true;

	/// <summary>
	/// 虚拟模式
	/// </summary>
	[Description("虚拟模式")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool VirtualMode { get; set; }

	/// <summary>
	/// 是否可以撤消
	/// </summary>
	[Browsable(false)]
	public bool CanUndo
	{
		get
		{
			if (IsPassWord && !PasswordCopy)
			{
				return false;
			}
			if (history_Log.Count > 0)
			{
				int num = ((history_I != -1) ? (history_I - 1) : (history_Log.Count - 1));
				if (num > -1)
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// 是否可以重做
	/// </summary>
	[Browsable(false)]
	public bool CanRedo
	{
		get
		{
			if (IsPassWord && !PasswordCopy)
			{
				return false;
			}
			if (history_Log.Count > 0 && history_I > -1)
			{
				int num = history_I + 1;
				if (history_Log.Count > num)
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// 获取设置当前选中文本
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("获取设置当前选中文本")]
	[Category("Data")]
	[DefaultValue(null)]
	public virtual string? SelectedText
	{
		get
		{
			return GetSelectionText();
		}
		set
		{
			if (value == null || string.IsNullOrEmpty(value))
			{
				SelectionLength = 0;
				return;
			}
			int num = _text.IndexOf(value);
			if (num > -1)
			{
				bool flag = SetSelectionStart(num);
				bool flag2 = SetSelectionLength(value.Length);
				if (flag | flag2)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 是否存在焦点
	/// </summary>
	[Browsable(false)]
	[Description("是否存在焦点")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool HasFocus { get; private set; }

	/// <summary>
	/// 禁止输入
	/// </summary>
	protected virtual bool BanInput => false;

	protected virtual bool HasValue => false;

	protected virtual bool ModeRange => false;

	/// <summary>
	/// 是否显示水印
	/// </summary>
	protected virtual bool ShowPlaceholder => true;

	protected virtual bool HasAnimation => true;

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

	public virtual Size PSize => this.GDI((Canvas g) => GetPSize(g));

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal bool ExtraMouseDown
	{
		get
		{
			return _mouseDown;
		}
		set
		{
			if (_mouseDown == value)
			{
				return;
			}
			_mouseDown = value;
			ChangeMouseHover(_mouseHover, value);
			if (Config.HasAnimation("Input", Name) && HasAnimation && WaveSize > 0)
			{
				ThreadFocus?.Dispose();
				AnimationFocus = true;
				AnimationLinearConfig animationLinearConfig = new AnimationLinearConfig(this, (int i) =>
				{
					AnimationFocusValue = i;
					Invalidate();
					return true;
				}, 20).SetValue(AnimationFocusValue);
				if (value)
				{
					animationLinearConfig.SetAdd(4).SetMax(30);
				}
				else
				{
					animationLinearConfig.SetAdd(-4).SetMax(0);
				}
				ThreadFocus = new AnimationTask(animationLinearConfig.SetEnd(() =>
				{
					AnimationFocus = false;
				}));
			}
			else
			{
				Invalidate();
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal bool ExtraMouseHover
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
			ChangeMouseHover(value, _mouseDown);
			if (!Enabled)
			{
				return;
			}
			if (OnAllowClear())
			{
				CalculateRect();
			}
			if (Config.HasAnimation("Input", Name) && HasAnimation && !ExtraMouseDown)
			{
				ThreadHover?.Dispose();
				AnimationHover = true;
				ThreadHover = new AnimationTask(new AnimationLinearConfig(this, (int i) =>
				{
					AnimationHoverValue = i;
					Invalidate();
					return true;
				}, 10).SetValueColor(AnimationHoverValue, value, 20).SetEnd(() =>
				{
					AnimationHover = false;
				}));
			}
			else
			{
				AnimationHoverValue = 255;
			}
			Invalidate();
		}
	}

	public override Rectangle ReadRectangle => ClientRectangle.PaddingRect(Padding).ReadRect(((float)WaveSize + borderWidth / 2f) * Dpi, joinMode, JoinLeft, JoinRight);

	public override GraphicsPath RenderRegion => Helper.PathJoin(ReadRectangle, radius, Dpi, round, joinMode, JoinLeft, JoinRight);

	private ScrollInfo ScrollX { get; set; }

	private ScrollInfo ScrollY { get; set; }

	[Description("清空 点击时发生")]
	[Category("Behavior")]
	public event MouseEventHandler? ClearClick;

	[Description("前缀 点击时发生")]
	[Category("Behavior")]
	public event MouseEventHandler? PrefixClick;

	[Description("后缀 点击时发生")]
	[Category("Behavior")]
	public event MouseEventHandler? SuffixClick;

	[Description("验证字符时发生")]
	[Category("Behavior")]
	public event InputVerifyCharEventHandler? VerifyChar;

	[Description("验证键盘时发生")]
	[Category("Behavior")]
	public event InputVerifyKeyboardEventHandler? VerifyKeyboard;

	protected override AccessibleObject CreateAccessibilityInstance()
	{
		return new InputAccessibleObject(this);
	}

	public Input()
		: base(ControlType.Select)
	{
		base.BackColor = Color.Transparent;
		CaretInfo = new ICaret(this);
		ScrollY = new ScrollYInfo(this);
		ScrollX = new ScrollInfo(this);
	}

	private bool OnAllowClear()
	{
		bool flag = !ReadOnly && allowclear && _mouseHover && (!isempty || HasValue);
		if (is_clear == flag)
		{
			return false;
		}
		is_clear = flag;
		return true;
	}

	private void SetImeMode(ImeMode value)
	{
		if (InvokeRequired)
		{
			Invoke(() =>
			{
				SetImeMode(value);
			});
		}
		else
		{
			base.ImeMode = value;
		}
	}

	protected override void OnRightToLeftChanged(EventArgs e)
	{
		base.OnRightToLeftChanged(e);
		if (rightToLeft == RightToLeft.Inherit)
		{
			rtl_run_dirty = true;
			CalculateRect();
			Invalidate();
		}
	}

	private bool SetSelectionStart(int value, bool caret = true)
	{
		if (value < 0)
		{
			value = 0;
		}
		else if (value > 0)
		{
			if (cache_font == null)
			{
				value = 0;
			}
			else if (value > cache_font.Count)
			{
				value = cache_font.Count;
			}
		}
		if (selectionStart == value)
		{
			if (caret)
			{
				return SetCaretPostion(value + 1, rd: false);
			}
			return false;
		}
		selectionStart = (selectionStartTemp = OnSetSelectionStart(value));
		bool result = false;
		if (caret)
		{
			result = ((cache_font != null && cache_font.Count > selectionStart) ? SetCaretPostion(selectionStart + 1, rd: false) : SetCaretPostion(selectionStart + 1, rd: false));
		}
		OnPropertyChanged("SelectionStart");
		return result;
	}

	private bool SetSelectionLength(int value)
	{
		if (selectionLength == value)
		{
			return false;
		}
		selectionLength = value;
		OnPropertyChanged("SelectionLength");
		return true;
	}

	private void SetPassWord()
	{
		if (passwordChar != '\0')
		{
			PassWordChar = passwordChar.ToString();
			IsPassWord = true;
		}
		else if (useSystemPasswordChar)
		{
			PassWordChar = "●";
			IsPassWord = true;
		}
		else
		{
			IsPassWord = false;
		}
		SetImeMode(IsPassWord ? ImeMode.Disable : imeMode);
		FixFontWidth(force: true);
		Invalidate();
	}

	/// <summary>
	/// 将文本追加到当前文本中
	/// </summary>
	/// <param name="text">追加的文本</param>
	public void AppendText(string text)
	{
		int start = ((cache_font != null) ? (cache_font[cache_font.Count - 1].i + 1) : 0);
		bool flag = SetTextAppend(text, start, out var _);
		bool flag2 = SetSelectionStart(selectionStart + GraphemeSplitter.EachCount(text));
		if (flag | flag2)
		{
			Invalidate();
		}
	}

	/// <summary>
	/// 追加文本到末尾
	/// </summary>
	/// <param name="text">追加的文本</param>
	/// <param name="config">文本配置</param>
	public void AppendText(string text, TextOpConfig config)
	{
		int start = ((cache_font != null) ? (cache_font[cache_font.Count - 1].i + 1) : 0);
		int num = selectionStart;
		bool flag = SetTextAppend(text, start, out var len);
		bool flag2 = false;
		bool flag3 = false;
		if (config.Font != null || config.Fore.HasValue || config.Back.HasValue)
		{
			TextStyle style = new TextStyle(start, len, config.Font, config.Fore, config.Back);
			flag2 = SetStyle(style, rd: false);
		}
		switch (config.CursorBehavior)
		{
		case CursorBehavior.EndOfNewText:
			flag3 = SetSelectionStart(num + len);
			break;
		case CursorBehavior.EndOfWholeText:
			if (cache_font != null)
			{
				flag3 = SetSelectionStart(cache_font.Count);
			}
			break;
		}
		if (config.Redraw && (flag | flag3 | flag2))
		{
			Invalidate();
		}
	}

	/// <summary>
	/// 插入文本
	/// </summary>
	/// <param name="startIndex">开始位置</param>
	/// <param name="text">文本</param>
	/// <param name="config">文本配置</param>
	public void InsertText(int startIndex, string text, TextOpConfig config)
	{
		if (cache_font == null)
		{
			AppendText(text, config);
			return;
		}
		int num = selectionStartTemp;
		int num2 = selectionLength;
		if (num2 > 0)
		{
			SetTextRemove(ref cache_font, num, num2, r: true);
		}
		bool flag = SetTextIn(text, startIndex, out var len);
		bool flag2 = false;
		bool flag3 = false;
		if (config.Font != null || config.Fore.HasValue || config.Back.HasValue)
		{
			TextStyle style = new TextStyle(num, len, config.Font, config.Fore, config.Back);
			flag2 = SetStyle(style, rd: false);
		}
		switch (config.CursorBehavior)
		{
		case CursorBehavior.EndOfNewText:
			flag3 = SetSelectionStart(num + len);
			break;
		case CursorBehavior.EndOfWholeText:
			if (cache_font != null)
			{
				flag3 = SetSelectionStart(cache_font.Count);
			}
			break;
		case CursorBehavior.StartOfNewText:
			flag3 = SetSelectionStart(num);
			break;
		}
		if (config.Redraw && (flag | flag3 | flag2))
		{
			Invalidate();
		}
	}

	/// <summary>
	/// 清除所有文本
	/// </summary>
	public void Clear()
	{
		Text = "";
	}

	/// <summary>
	/// 清除撤消缓冲区信息
	/// </summary>
	public void ClearUndo()
	{
		history_Log.Clear();
	}

	/// <summary>
	/// 复制
	/// </summary>
	public void Copy()
	{
		if (!IsPassWord || PasswordCopy)
		{
			string selectionText = GetSelectionText();
			if (!string.IsNullOrEmpty(selectionText))
			{
				this.ClipboardSetText(selectionText);
			}
		}
	}

	/// <summary>
	/// 剪贴
	/// </summary>
	public void Cut()
	{
		if (!IsPassWord || PasswordCopy)
		{
			string selectionText = GetSelectionText();
			if (!string.IsNullOrEmpty(selectionText))
			{
				this.ClipboardSetText(selectionText);
				ProcessBackSpaceKey();
			}
		}
	}

	/// <summary>
	/// 粘贴
	/// </summary>
	public void Paste()
	{
		if (IsPassWord && !PasswordPaste)
		{
			return;
		}
		string text = this.ClipboardGetText();
		if (text == null || string.IsNullOrEmpty(text))
		{
			return;
		}
		List<string> list = new List<string>(text.Length);
		string text2 = text;
		for (int i = 0; i < text2.Length; i++)
		{
			char key = text2[i];
			if (Verify(key, out string change))
			{
				list.Add(change ?? key.ToString());
			}
		}
		if (list.Count > 0)
		{
			EnterText(string.Join("", list));
		}
	}

	/// <summary>
	/// 取消全部选中
	/// </summary>
	public void DeselectAll()
	{
		SelectionLength = 0;
	}

	/// <summary>
	/// 撤消
	/// </summary>
	public void Undo()
	{
		if ((IsPassWord && !PasswordCopy) || history_Log.Count <= 0)
		{
			return;
		}
		int num;
		if (history_I == -1)
		{
			num = history_Log.Count - 1;
			AddHistoryRecord();
		}
		else
		{
			num = history_I - 1;
		}
		if (num > -1)
		{
			TextHistoryRecord textHistoryRecord = history_Log[num];
			history_I = num;
			bool flag = SetText(textHistoryRecord.Text, active: true);
			bool flag2 = SetSelectionStart(textHistoryRecord.SelectionStart);
			bool flag3 = SetSelectionLength(textHistoryRecord.SelectionLength);
			if (flag | flag2 | flag3)
			{
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 重做
	/// </summary>
	public void Redo()
	{
		if ((IsPassWord && !PasswordCopy) || history_Log.Count <= 0 || history_I <= -1)
		{
			return;
		}
		int num = history_I + 1;
		if (history_Log.Count > num)
		{
			TextHistoryRecord textHistoryRecord = history_Log[num];
			history_I = num;
			bool flag = SetText(textHistoryRecord.Text, active: true);
			bool flag2 = SetSelectionStart(textHistoryRecord.SelectionStart);
			bool flag3 = SetSelectionLength(textHistoryRecord.SelectionLength);
			if (flag | flag2 | flag3)
			{
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 文本选择范围
	/// </summary>
	/// <param name="start">第一个字符的位置</param>
	/// <param name="length">字符长度</param>
	/// <param name="rd">是否渲染</param>
	public void Select(int start, int length)
	{
		bool flag = SetSelectionStart(start);
		bool flag2 = SetSelectionLength(length);
		if (flag | flag2)
		{
			Invalidate();
		}
	}

	/// <summary>
	/// 设置样式
	/// </summary>
	/// <param name="start">第一个字符的位置</param>
	/// <param name="length">字符长度</param>
	/// <param name="font">字体</param>
	/// <param name="fore">文本颜色</param>
	/// <param name="back">背景颜色</param>
	public bool SetStyle(int start, int length, Font? font = null, Color? fore = null, Color? back = null)
	{
		return SetStyle(new TextStyle(start, length, font, fore, back));
	}

	/// <summary>
	/// 设置样式
	/// </summary>
	public bool SetStyle(TextStyle style, bool rd = true)
	{
		if (styles == null)
		{
			styles = new TextStyle[1] { style };
		}
		else
		{
			List<TextStyle> list = new List<TextStyle>(styles.Length + 1);
			list.AddRange(styles);
			list.Add(style);
			styles = list.ToArray();
		}
		if (ApplyStyle(style))
		{
			if (rd)
			{
				Invalidate();
			}
			return true;
		}
		return false;
	}

	private void SetStyle()
	{
		if (styles != null)
		{
			TextStyle[] array = styles;
			foreach (TextStyle style in array)
			{
				ApplyStyle(style);
			}
		}
	}

	private bool ApplyStyle(TextStyle style)
	{
		if (cache_font == null)
		{
			return false;
		}
		int num = style.Start + style.Length;
		if (style.Start < 0 || num > cache_font.Count)
		{
			return false;
		}
		for (int i = style.Start; i < num; i++)
		{
			CacheFont cacheFont = cache_font[i];
			cacheFont.font = style.Font;
			cacheFont.fore = style.Fore;
			cacheFont.back = style.Back;
		}
		return true;
	}

	/// <summary>
	/// 清空样式
	/// </summary>
	public void ClearStyle(bool r = true)
	{
		if (styles == null)
		{
			return;
		}
		styles = null;
		if (cache_font != null)
		{
			for (int i = 0; i < cache_font.Count; i++)
			{
				CacheFont cacheFont = cache_font[i];
				cacheFont.font = null;
				cacheFont.fore = null;
				cacheFont.back = null;
			}
			if (r)
			{
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 选择最后文本
	/// </summary>
	public void SelectLast()
	{
		if (cache_font != null)
		{
			bool flag = SetSelectionStart(cache_font.Count);
			bool flag2 = SetSelectionLength(0);
			bool flag3 = SetCaretPostion(cache_font.Count + 1);
			if (flag | flag2 | flag3)
			{
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 选择所有文本
	/// </summary>
	public void SelectAll()
	{
		if (cache_font != null)
		{
			bool flag = SetSelectionStart(0);
			bool flag2 = SetSelectionLength(cache_font.Count);
			bool flag3 = SetCaretPostion(cache_font.Count + 1);
			if (flag | flag2 | flag3)
			{
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 当前位置插入文本
	/// </summary>
	/// <param name="text">文本</param>
	public void EnterText(string text)
	{
		EnterText(text, isMax: true, isVirtual: false);
	}

	/// <summary>
	/// 当前位置插入文本
	/// </summary>
	/// <param name="text">文本</param>
	/// <param name="isMax">是否限制MaxLength</param>
	public void EnterText(string text, bool isMax, bool isVirtual)
	{
		CloseContextMenu();
		if (ReadOnly || BanInput)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int? num3 = null;
		if (cache_font == null)
		{
			if (isMax && text.Length > MaxLength)
			{
				text = text.Substring(0, MaxLength);
				if (text.Length == 0)
				{
					return;
				}
			}
			AddHistoryRecord();
			if (SetText(text, active: true, changed: false))
			{
				num2++;
			}
		}
		else if (selectionLength > 0)
		{
			int num4 = selectionStartTemp;
			int num5 = selectionLength;
			if (isMax && cache_font.Count - num5 + text.Length > MaxLength)
			{
				if (MaxLength <= cache_font.Count)
				{
					return;
				}
				text = text.Substring(0, num5);
				if (text.Length == 0)
				{
					return;
				}
			}
			AddHistoryRecord();
			if (isVirtual || VirtualMode)
			{
				SetTextRemove(ref cache_font, num4, num5, r: true);
				if (SetTextIn(text, num4, out var len, changed: false))
				{
					num2++;
				}
				num3 = len;
			}
			else
			{
				int num6 = num4 + num5;
				List<string> list = new List<string>(num5);
				foreach (CacheFont item in cache_font)
				{
					if (item.i < num4 || item.i >= num6)
					{
						list.Add(item.text);
					}
				}
				list.Insert(num4, text);
				if (SetText(string.Join("", list), active: true, changed: false))
				{
					num2++;
				}
			}
			if (SetSelectionLength(0))
			{
				num2++;
			}
			num = num4;
		}
		else
		{
			int num7 = selectionStart - 1;
			if (isMax && cache_font.Count + text.Length > MaxLength)
			{
				if (MaxLength <= cache_font.Count)
				{
					return;
				}
				text = text.Substring(0, MaxLength - cache_font.Count);
				if (text.Length == 0)
				{
					return;
				}
			}
			AddHistoryRecord();
			num = num7 + 1;
			if (isVirtual || VirtualMode)
			{
				if (SetTextIn(text, num, out var len2, changed: false))
				{
					num2++;
				}
				num3 = len2;
			}
			else
			{
				List<string> list2 = new List<string>(cache_font.Count);
				foreach (CacheFont item2 in cache_font)
				{
					list2.Add(item2.text);
				}
				list2.Insert(num, text);
				if (SetText(string.Join("", list2), active: true, changed: false))
				{
					num2++;
				}
			}
		}
		int num8 = num3 ?? GraphemeSplitter.EachCount(text);
		if (SetSelectionStart(num + num8))
		{
			num2++;
		}
		OnTextChanged(EventArgs.Empty);
		OnPropertyChanged("Text");
		if (num2 > 0)
		{
			Invalidate();
		}
	}

	/// <summary>
	/// 获取当前选中文本
	/// </summary>
	public string? GetSelectionText()
	{
		if (cache_font == null)
		{
			return null;
		}
		if (selectionLength > 0)
		{
			int num = selectionStartTemp;
			int num2 = selectionLength;
			int num3 = num + num2;
			if (num3 > cache_font.Count)
			{
				num3 = cache_font.Count;
			}
			List<string> list = new List<string>(num2);
			foreach (CacheFont item in cache_font)
			{
				if (item.i >= num && num3 > item.i)
				{
					list.Add(item.text);
				}
			}
			return string.Join("", list);
		}
		return null;
	}

	/// <summary>
	/// 内容滚动到当前插入符号位置
	/// </summary>
	public void ScrollToCaret()
	{
		if (cache_font != null)
		{
			Rectangle rectangle = ((CurrentPosIndex < cache_font.Count) ? cache_font[CurrentPosIndex].rect : cache_font[cache_font.Count - 1].rect);
			ScrollY.Value = rectangle.Bottom;
		}
	}

	/// <summary>
	/// 内容滚动到最下面
	/// </summary>
	public void ScrollToEnd()
	{
		ScrollY.Value = ScrollY.Max;
	}

	/// <summary>
	/// 滚动到指定行
	/// </summary>
	public void ScrollLine(int i)
	{
		int num = CaretInfo.Height + ((lineheight > 0) ? ((int)((float)lineheight * Dpi)) : 0);
		ScrollY.Value = num * i;
	}

	/// <summary>
	/// 查找指定字符串在文本中首次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <returns>首次出现的字符簇索引，未找到返回 -1</returns>
	public int IndexOf(string value)
	{
		return IndexOf(value, 0, StringComparison.CurrentCulture);
	}

	/// <summary>
	/// 从指定位置开始查找字符串首次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <param name="startIndex">开始搜索的字符簇索引</param>
	/// <returns>首次出现的字符簇索引，未找到返回 -1</returns>
	public int IndexOf(string value, int startIndex)
	{
		return IndexOf(value, startIndex, StringComparison.CurrentCulture);
	}

	/// <summary>
	/// 从指定位置开始，在指定范围内查找字符串首次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <param name="startIndex">开始搜索的字符簇索引</param>
	/// <param name="count">要搜索的字符簇数量</param>
	/// <returns>首次出现的字符簇索引，未找到返回 -1</returns>
	public int IndexOf(string value, int startIndex, int count)
	{
		return IndexOf(value, startIndex, count, StringComparison.CurrentCulture);
	}

	/// <summary>
	/// 查找指定字符串在文本中首次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <param name="comparisonType">字符串比较类型</param>
	/// <returns>首次出现的字符簇索引，未找到返回 -1</returns>
	public int IndexOf(string value, StringComparison comparisonType)
	{
		return IndexOf(value, 0, comparisonType);
	}

	/// <summary>
	/// 从指定位置开始查找字符串首次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <param name="startIndex">开始搜索的字符簇索引</param>
	/// <param name="comparisonType">字符串比较类型</param>
	/// <returns>首次出现的字符簇索引，未找到返回 -1</returns>
	public int IndexOf(string value, int startIndex, StringComparison comparisonType)
	{
		if (cache_font == null)
		{
			return -1;
		}
		return IndexOf(cache_font, value, startIndex, null, comparisonType);
	}

	/// <summary>
	/// 从指定位置开始，在指定范围内查找字符串首次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <param name="startIndex">开始搜索的字符簇索引</param>
	/// <param name="count">要搜索的字符簇数量</param>
	/// <param name="comparisonType">字符串比较类型</param>
	/// <returns>首次出现的字符簇索引，未找到返回 -1</returns>
	public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType)
	{
		if (cache_font == null)
		{
			return -1;
		}
		return IndexOf(cache_font, value, startIndex, count, comparisonType);
	}

	private int IndexOf(List<CacheFont> cache_font, string value, int startIndex, int? count, StringComparison comparisonType)
	{
		if (startIndex < 0)
		{
			startIndex = 0;
		}
		if (startIndex >= cache_font.Count)
		{
			return -1;
		}
		if (count.HasValue)
		{
			if (count < 0)
			{
				count = 0;
			}
			if (startIndex + count > cache_font.Count)
			{
				count = cache_font.Count - startIndex;
			}
		}
		else
		{
			count = cache_font.Count - startIndex;
		}
		string[] array = IndexSplitter(value);
		int num = array.Length;
		if (num == 0 || num > count)
		{
			return -1;
		}
		for (int i = startIndex; i <= startIndex + count - num; i++)
		{
			if (IndexOf(cache_font, i, num, array, comparisonType))
			{
				return i;
			}
		}
		return -1;
	}

	private bool IndexOf(List<CacheFont> cache_font, int start, int valueLength, string[] valueParts, StringComparison comparisonType)
	{
		for (int i = 0; i < valueLength; i++)
		{
			int num = start + i;
			if (num >= cache_font.Count)
			{
				return false;
			}
			if (!string.Equals(cache_font[num].text, valueParts[i], comparisonType))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 查找指定字符串在文本中最后一次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <returns>最后一次出现的字符簇索引，未找到返回 -1</returns>
	public int LastIndexOf(string value)
	{
		if (cache_font == null)
		{
			return -1;
		}
		return LastIndexOf(cache_font, value, cache_font.Count - 1, null, StringComparison.CurrentCulture);
	}

	/// <summary>
	/// 从指定位置开始向前查找字符串最后一次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <param name="startIndex">开始搜索的字符簇索引（从该位置向前搜索）</param>
	/// <returns>最后一次出现的字符簇索引，未找到返回 -1</returns>
	public int LastIndexOf(string value, int startIndex)
	{
		return LastIndexOf(value, startIndex, StringComparison.CurrentCulture);
	}

	/// <summary>
	/// 从指定位置开始，在指定范围内向前查找字符串最后一次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <param name="startIndex">开始搜索的字符簇索引（从该位置向前搜索）</param>
	/// <param name="count">要搜索的字符簇数量</param>
	/// <returns>最后一次出现的字符簇索引，未找到返回 -1</returns>
	public int LastIndexOf(string value, int startIndex, int count)
	{
		return LastIndexOf(value, startIndex, count, StringComparison.CurrentCulture);
	}

	/// <summary>
	/// 查找指定字符串在文本中最后一次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <param name="comparisonType">字符串比较类型</param>
	/// <returns>最后一次出现的字符簇索引，未找到返回 -1</returns>
	public int LastIndexOf(string value, StringComparison comparisonType)
	{
		if (cache_font == null)
		{
			return -1;
		}
		return LastIndexOf(value, cache_font.Count - 1, comparisonType);
	}

	/// <summary>
	/// 从指定位置开始向前查找字符串最后一次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <param name="startIndex">开始搜索的字符簇索引（从该位置向前搜索）</param>
	/// <param name="comparisonType">字符串比较类型</param>
	/// <returns>最后一次出现的字符簇索引，未找到返回 -1</returns>
	public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
	{
		if (cache_font == null)
		{
			return -1;
		}
		return LastIndexOf(cache_font, value, startIndex, null, comparisonType);
	}

	/// <summary>
	/// 从指定位置开始，在指定范围内向前查找字符串最后一次出现的位置（基于字符簇索引）
	/// </summary>
	/// <param name="value">要查找的字符串</param>
	/// <param name="startIndex">开始搜索的字符簇索引（从该位置向前搜索）</param>
	/// <param name="count">要搜索的字符簇数量</param>
	/// <param name="comparisonType">字符串比较类型</param>
	/// <returns>最后一次出现的字符簇索引，未找到返回 -1</returns>
	public int LastIndexOf(string value, int startIndex, int count, StringComparison comparisonType)
	{
		if (cache_font == null)
		{
			return -1;
		}
		return LastIndexOf(cache_font, value, startIndex, count, comparisonType);
	}

	private int LastIndexOf(List<CacheFont> cache_font, string value, int startIndex, int? count, StringComparison comparisonType)
	{
		if (startIndex < 0)
		{
			startIndex = 0;
		}
		if (startIndex >= cache_font.Count)
		{
			startIndex = cache_font.Count - 1;
		}
		if (count.HasValue)
		{
			if (count < 0)
			{
				count = 0;
			}
			if (count > startIndex + 1)
			{
				count = startIndex + 1;
			}
		}
		else
		{
			count = startIndex + 1;
		}
		string[] array = IndexSplitter(value);
		int num = array.Length;
		if (num == 0 || num > count)
		{
			return -1;
		}
		int num2 = startIndex - count.Value + 1;
		if (num2 < 0)
		{
			num2 = 0;
		}
		for (int num3 = startIndex; num3 >= num2; num3--)
		{
			if (num3 + num <= cache_font.Count && IndexOf(cache_font, num3, num, array, comparisonType))
			{
				return num3;
			}
		}
		return -1;
	}

	private string[] IndexSplitter(string value)
	{
		List<string> valueParts = new List<string>(value.Length);
		GraphemeSplitter.Each(value, (string txt, int ntype) =>
		{
			valueParts.Add(txt);
		});
		return valueParts.ToArray();
	}

	/// <summary>
	/// 从指定字符簇位置开始截取子字符串
	/// </summary>
	/// <param name="startIndex">开始截取的字符簇索引</param>
	/// <returns>从指定位置开始的子字符串</returns>
	public string Substring(int startIndex)
	{
		if (cache_font == null)
		{
			return "";
		}
		return Substring(cache_font, startIndex, cache_font.Count - startIndex);
	}

	/// <summary>
	/// 从指定字符簇位置开始截取指定长度的子字符串
	/// </summary>
	/// <param name="startIndex">开始截取的字符簇索引</param>
	/// <param name="length">要截取的字符簇数量</param>
	/// <returns>截取的子字符串</returns>
	public string Substring(int startIndex, int length)
	{
		if (cache_font == null)
		{
			return "";
		}
		if (length < 0)
		{
			length = 0;
		}
		if (startIndex + length > cache_font.Count)
		{
			length = cache_font.Count - startIndex;
		}
		return Substring(cache_font, startIndex, length);
	}

	private string Substring(List<CacheFont> cache_font, int startIndex, int length)
	{
		if (startIndex < 0)
		{
			startIndex = 0;
		}
		if (startIndex >= cache_font.Count)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder(length);
		for (int i = startIndex; i < startIndex + length; i++)
		{
			stringBuilder.Append(cache_font[i].text);
		}
		return stringBuilder.ToString();
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		FixFontWidth(force: true);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		HasFocus = true;
		CaretInfo.Show = CaretVisible;
		ExtraMouseDown = true;
	}

	protected override void OnLostFocus(EventArgs e)
	{
		HasFocus = false;
		CaretInfo.Show = false;
		if (HideSelection)
		{
			SelectionLength = 0;
		}
		ExtraMouseDown = false;
		base.OnLostFocus(e);
	}

	protected override void WndProc(ref System.Windows.Forms.Message m)
	{
		switch (m.Msg)
		{
		case 258:
		case 265:
			IKeyPress((char)m.WParam.ToInt32());
			break;
		case 256:
		case 260:
			HandKeyDown(GetKeyBoard(m.WParam.ToInt32()));
			break;
		case 269:
			m_hIMC = Win32.Imm32.ImmGetContext(Handle);
			OnImeStartPrivate(m_hIMC);
			break;
		case 270:
			Win32.Imm32.ImmReleaseContext(Handle, m_hIMC);
			break;
		case 271:
			if (((int)m.LParam & 0x800) == 2048)
			{
				m.Result = (IntPtr)1;
				OnImeResultStrPrivate(m_hIMC, Win32.Imm32.ImmGetCompositionString(m_hIMC, 2048));
				return;
			}
			break;
		case 123:
			CloseContextMenu();
			if (UseContextMenu)
			{
				OnOpenContextMenu();
			}
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

	private void CloseContextMenu()
	{
		_contextMenu?.Close();
		_contextMenu = null;
	}

	protected virtual void OnOpenContextMenu()
	{
		List<IContextMenuStripItem> list = new List<IContextMenuStripItem>(9);
		if (readOnly)
		{
			list.Add(new ContextMenuStripItem().SetText("复制", "{id}").SetID("Copy").SetSubText("Ctrl+C")
				.SetEnabled(selectionLength > 0));
			list.Add(new ContextMenuStripItemDivider());
			list.Add(new ContextMenuStripItem().SetText("全选", "{id}").SetID("SelectAll").SetSubText("Ctrl+A")
				.SetEnabled(!isempty));
		}
		else
		{
			bool canUndo = CanUndo;
			bool canRedo = CanRedo;
			if (canUndo | canRedo)
			{
				if (canUndo)
				{
					list.Add(new ContextMenuStripItem().SetText("撤销", "{id}").SetID("Undo").SetSubText("Ctrl+Z"));
				}
				if (canRedo)
				{
					list.Add(new ContextMenuStripItem().SetText("重做", "{id}").SetID("Redo").SetSubText("Ctrl+Y"));
				}
				list.Add(new ContextMenuStripItemDivider());
			}
			if (!IsPassWord || (IsPassWord && PasswordCopy))
			{
				list.Add(new ContextMenuStripItem().SetText("剪切", "{id}").SetID("Cut").SetSubText("Ctrl+X")
					.SetEnabled(selectionLength > 0));
				list.Add(new ContextMenuStripItem().SetText("复制", "{id}").SetID("Copy").SetSubText("Ctrl+C")
					.SetEnabled(selectionLength > 0));
			}
			if (!IsPassWord || (IsPassWord && PasswordPaste))
			{
				list.Add(new ContextMenuStripItem().SetText("粘贴", "{id}").SetID("Paste").SetSubText("Ctrl+V"));
			}
			list.Add(new ContextMenuStripItem().SetText("删除", "{id}").SetID("Delete").SetSubText("Del")
				.SetEnabled(selectionLength > 0));
			list.Add(new ContextMenuStripItemDivider());
			list.Add(new ContextMenuStripItem().SetText("全选", "{id}").SetID("SelectAll").SetSubText("Ctrl+A")
				.SetEnabled(!isempty));
		}
		ContextMenuStrip.Config config = new ContextMenuStrip.Config(this, (IContextMenuStrip item) =>
		{
			string iD = item.ID;
			if (iD != null)
			{
				switch (iD.Length)
				{
				case 4:
					switch (iD[0])
					{
					case 'U':
						if (iD == "Undo")
						{
							Undo();
						}
						break;
					case 'R':
						if (iD == "Redo")
						{
							Redo();
						}
						break;
					case 'C':
						if (iD == "Copy")
						{
							Copy();
						}
						break;
					}
					break;
				case 3:
					if (iD == "Cut")
					{
						Cut();
					}
					break;
				case 5:
					if (iD == "Paste")
					{
						Paste();
					}
					break;
				case 6:
					if (iD == "Delete")
					{
						ProcessBackSpaceKey();
					}
					break;
				case 9:
					if (iD == "SelectAll")
					{
						SelectAll();
					}
					break;
				case 7:
				case 8:
					break;
				}
			}
		}, list.ToArray()).SetTopMost();
		if (!_mouseHover)
		{
			config.SetLocation(PointToScreen(CaretInfo.Rect.Location));
		}
		_contextMenu = config.open();
	}

	protected virtual void ModeRangeCaretPostion(bool Null)
	{
	}

	protected virtual bool HasLeft()
	{
		return false;
	}

	protected virtual int[] UseLeft(Rectangle rect, int font_height)
	{
		return new int[2];
	}

	protected virtual void UseLeftAutoHeight(int height, int y)
	{
	}

	protected virtual void IBackSpaceKey()
	{
	}

	protected virtual bool IMouseDown(int x, int y)
	{
		return false;
	}

	protected virtual bool IMouseMove(int x, int y)
	{
		return false;
	}

	protected virtual bool IMouseUp(int x, int y)
	{
		return false;
	}

	/// <summary>
	/// 清空值
	/// </summary>
	protected virtual void OnClearValue()
	{
		Text = "";
	}

	/// <summary>
	/// 点击内容
	/// </summary>
	protected virtual void OnClickContent(MouseEventArgs e)
	{
	}

	/// <summary>
	/// 改变鼠标状态
	/// </summary>
	/// <param name="Hover">移入</param>
	/// <param name="Focus">焦点</param>
	protected virtual void ChangeMouseHover(bool Hover, bool Focus)
	{
	}

	protected virtual void SetCaretPostion(ref int x, ref int y)
	{
	}

	/// <summary>
	/// 通过坐标系查找光标位置
	/// </summary>
	private CacheCaret? GetCaretPostion(int x, int y)
	{
		SetCaretPostion(ref x, ref y);
		if (cache_caret == null)
		{
			return null;
		}
		return FindNearestFont(x, y, cache_caret);
	}

	private CacheCaret? GetCaretPostionDown()
	{
		if (cache_caret == null)
		{
			return null;
		}
		int value = tmpkeyx ?? CaretInfo.X;
		int num = CaretInfo.Y + CaretInfo.Height;
		tmpkeyx = value;
		SetCaretPostion(ref value, ref num);
		return GetCaretPostionDirection(value, num, cache_caret);
	}

	private CacheCaret? GetCaretPostionUp()
	{
		if (cache_caret == null)
		{
			return null;
		}
		int value = tmpkeyx ?? CaretInfo.X;
		int num = CaretInfo.Y - CaretInfo.Height;
		tmpkeyx = value;
		SetCaretPostion(ref value, ref num);
		return GetCaretPostionDirection(value, num, cache_caret);
	}

	private CacheCaret? GetCaretPostionDirection(int x, int y, CacheCaret[] cache_caret)
	{
		CacheCaret cacheCaret = cache_caret[0];
		CacheCaret cacheCaret2 = cache_caret[cache_caret.Length - 1];
		if (multiline)
		{
			if (IsRTL)
			{
				if (x > cacheCaret.x && y < cacheCaret.y)
				{
					return cacheCaret;
				}
				if (x < cacheCaret2.x && y > cacheCaret2.y)
				{
					return cacheCaret2;
				}
			}
			else
			{
				if (x < cacheCaret.x && y < cacheCaret.y)
				{
					return cacheCaret;
				}
				if (x > cacheCaret2.x && y > cacheCaret2.y)
				{
					return cacheCaret2;
				}
			}
		}
		else if (IsRTL)
		{
			if (x > cacheCaret.x)
			{
				return cacheCaret;
			}
			if (x < cacheCaret2.x)
			{
				return cacheCaret2;
			}
		}
		else
		{
			if (x < cacheCaret.x)
			{
				return cacheCaret;
			}
			if (x > cacheCaret2.x)
			{
				return cacheCaret2;
			}
		}
		return FindNearestFontX(x, FindNearestFontY(y, cache_caret));
	}

	/// <summary>
	/// 寻找最近的矩形和距离的辅助方法
	/// </summary>
	private CacheCaret FindNearestFont(int x, int y, CacheCaret[] cache_caret)
	{
		tmpkeyx = null;
		CacheCaret cacheCaret = cache_caret[0];
		CacheCaret cacheCaret2 = cache_caret[cache_caret.Length - 1];
		if (multiline)
		{
			if (IsRTL)
			{
				if (x > cacheCaret.x && y < cacheCaret.y)
				{
					return cacheCaret;
				}
				if (x < cacheCaret2.x && y > cacheCaret2.y)
				{
					return cacheCaret2;
				}
			}
			else
			{
				if (x < cacheCaret.x && y < cacheCaret.y)
				{
					return cacheCaret;
				}
				if (x > cacheCaret2.x && y > cacheCaret2.y)
				{
					return cacheCaret2;
				}
			}
		}
		else if (IsRTL)
		{
			if (x > cacheCaret.x)
			{
				return cacheCaret;
			}
			if (x < cacheCaret2.x)
			{
				return cacheCaret2;
			}
		}
		else
		{
			if (x < cacheCaret.x)
			{
				return cacheCaret;
			}
			if (x > cacheCaret2.x)
			{
				return cacheCaret2;
			}
		}
		return FindNearestFontX(x, FindNearestFontY(y, cache_caret, CaretInfo.Height / 2));
	}

	private CacheCaret[] FindNearestFontY(int y, CacheCaret[] cache_caret, int offset = 0)
	{
		CacheCaret cacheCaret = cache_caret[0];
		int num = 1;
		int num2 = Math.Abs(cache_caret[0].y - y);
		for (int i = 1; i < cache_caret.Length; i++)
		{
			int num3 = cache_caret[i].y;
			if (num3 == cache_caret[0].y)
			{
				num++;
			}
			int num4 = Math.Abs(num3 + offset - y);
			if (num4 < num2)
			{
				num2 = num4;
				cacheCaret = cache_caret[i];
				if (num2 == 0)
				{
					break;
				}
			}
		}
		List<CacheCaret> list = new List<CacheCaret>(num);
		foreach (CacheCaret cacheCaret2 in cache_caret)
		{
			if (cacheCaret2.y == cacheCaret.y)
			{
				list.Add(cacheCaret2);
			}
			else if (cacheCaret2.y > cacheCaret.y)
			{
				return list.ToArray();
			}
		}
		return list.ToArray();
	}

	private CacheCaret FindNearestFontX(int x, CacheCaret[] cache_caret)
	{
		CacheCaret cacheCaret = cache_caret[0];
		int num = Math.Abs(cacheCaret.x - x);
		for (int i = 1; i < cache_caret.Length; i++)
		{
			int num2 = Math.Abs(cache_caret[i].x - x);
			if (num2 < num)
			{
				num = num2;
				cacheCaret = cache_caret[i];
				if (num == 0)
				{
					break;
				}
			}
		}
		return cacheCaret;
	}

	internal bool SetCaretPostion(bool rd = true)
	{
		return SetCaretPostion(CurrentPosIndex, rd);
	}

	internal bool SetCaretPostion(int PosIndex, bool rd = true)
	{
		if (PosIndex < 0)
		{
			return false;
		}
		CurrentPosIndex = PosIndex;
		if (cache_caret == null)
		{
			if (ModeRange)
			{
				ModeRangeCaretPostion(Null: true);
			}
			else if (textalign == HorizontalAlignment.Center)
			{
				CaretInfo.X = rect_text.X + rect_text.Width / 2;
			}
			else if (IsRTL)
			{
				if (textalign == HorizontalAlignment.Right)
				{
					CaretInfo.X = rect_text.X;
				}
				else
				{
					CaretInfo.X = rect_text.Right;
				}
			}
			else if (textalign == HorizontalAlignment.Right)
			{
				CaretInfo.X = rect_text.Right;
			}
			CaretInfo.flag = true;
			return true;
		}
		if (PosIndex > cache_caret.Length - 1)
		{
			PosIndex = cache_caret.Length - 1;
			return false;
		}
		Rectangle r = CaretInfo.SetXY(cache_caret, PosIndex, out var rd2);
		if (ModeRange)
		{
			ModeRangeCaretPostion(Null: false);
		}
		return ScrollIFTo(r, rd) | rd2;
	}

	private void OnImeStartPrivate(IntPtr hIMC)
	{
		try
		{
			Point location = CaretInfo.Rect.Location;
			location.Offset(0, -ScrollY.Value);
			Win32.Imm32.CANDIDATEFORM fuck = new Win32.Imm32.CANDIDATEFORM
			{
				dwStyle = 64,
				ptCurrentPos = location
			};
			Win32.Imm32.ImmSetCandidateWindow(hIMC, ref fuck);
			Win32.Imm32.COMPOSITIONFORM lpCompForm = new Win32.Imm32.COMPOSITIONFORM
			{
				dwStyle = 32,
				ptCurrentPos = location
			};
			Win32.Imm32.ImmSetCompositionWindow(hIMC, ref lpCompForm);
			Win32.Imm32.LOGFONT logFont = new Win32.Imm32.LOGFONT
			{
				lfHeight = CaretInfo.Height,
				lfFaceName = Font.Name + "\0"
			};
			Win32.Imm32.ImmSetCompositionFont(hIMC, ref logFont);
		}
		catch
		{
		}
	}

	private void OnImeResultStrPrivate(IntPtr hIMC, string? strResult)
	{
		try
		{
			Win32.Imm32.COMPOSITIONFORM lpCompForm = new Win32.Imm32.COMPOSITIONFORM
			{
				dwStyle = 32,
				ptCurrentPos = CaretInfo.Rect.Location
			};
			Win32.Imm32.ImmSetCompositionWindow(hIMC, ref lpCompForm);
			if (strResult == null || string.IsNullOrEmpty(strResult))
			{
				return;
			}
			List<string> list = new List<string>(strResult.Length);
			for (int i = 0; i < strResult.Length; i++)
			{
				char key = strResult[i];
				if (Verify(key, out string change))
				{
					list.Add(change ?? key.ToString());
				}
			}
			if (list.Count > 0)
			{
				EnterText(string.Join("", list));
			}
		}
		catch
		{
		}
	}

	protected virtual bool Verify(char key, out string? change)
	{
		if (VerifyChar == null)
		{
			change = null;
			return true;
		}
		InputVerifyCharEventArgs e = new InputVerifyCharEventArgs(key);
		VerifyChar(this, e);
		change = e.ReplaceText;
		return e.Result;
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
		<>c__DisplayClass472_0 CS$<>8__locals0;
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
				colorBlink = System.Runtime.CompilerServices.Unsafe.As<<>c__DisplayClass472_0, Color>(ref CS$<>8__locals0).BlendColors(Helper.ToColorN(val, end));
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

	private Size GetPSize(Canvas g)
	{
		int num = CaretInfo.Height;
		int num2 = (int)Math.Ceiling((float)((int)((float)num * paddgap) * 2) + ((float)WaveSize + borderWidth / 2f) * Dpi * 2f);
		int num3 = 0;
		string? text = PrefixText;
		string text2 = SuffixText;
		bool flag = text != null;
		bool flag2 = text2 != null;
		bool hasPrefix = HasPrefix;
		bool hasSuffix = HasSuffix;
		if ((loading && LoadingValue > -1f) || is_clear)
		{
			int num4 = (int)((float)num * iconratio);
			int num5 = num4;
			if (iconratioRight.HasValue)
			{
				num5 = (int)((float)num4 * iconratioRight.Value);
			}
			int num6 = (int)((float)num * icongap);
			if (flag)
			{
				num3 += MeasurePrefix(g) + num5 + num6 * 2;
			}
			else
			{
				num3 = ((!hasPrefix) ? (num3 + (num5 + num6)) : (num3 + (num4 + num5 + num6 * 2)));
			}
		}
		else if (flag | flag2 | hasPrefix | hasSuffix)
		{
			int num7 = (int)((float)num * icongap);
			if (flag | flag2)
			{
				if (flag & flag2)
				{
					num3 += MeasurePrefix(g) + MeasureSuffix(g) + num7 * 2;
				}
				else if (!(hasPrefix | hasSuffix))
				{
					num3 = ((!flag) ? (num3 + (MeasureSuffix(g) + num7)) : (num3 + (MeasurePrefix(g) + num7)));
				}
				else if (flag)
				{
					if (hasSuffix)
					{
						int num8 = (int)((float)num * (iconratioRight ?? iconratio));
						num3 += MeasurePrefix(g) + num8 + num7 * 2;
					}
					else
					{
						num3 += MeasurePrefix(g) + num7;
					}
				}
				else if (hasPrefix)
				{
					int num9 = (int)((float)num * iconratio);
					num3 += num9 + MeasureSuffix(g) + num7 * 2;
				}
				else
				{
					num3 += MeasureSuffix(g) + num7;
				}
			}
			else if (hasPrefix | hasSuffix)
			{
				int num10 = (int)((float)num * iconratio);
				int num11 = num10;
				if (iconratioRight.HasValue)
				{
					num11 = (int)((float)num * iconratioRight.Value);
				}
				if (hasPrefix & hasSuffix)
				{
					num3 += num10 + num11 + num7 * 2;
				}
				else
				{
					num3 = ((!hasPrefix) ? (num3 + (num11 + num7)) : (num3 + (num10 + num7)));
				}
			}
		}
		if (cache_font == null)
		{
			if (ShowPlaceholder && PlaceholderText != null)
			{
				num3 += g.MeasureText(PlaceholderText, Font).Width;
			}
			return new Size(num2 + num3, num + num2);
		}
		if (multiline)
		{
			int num12 = 0;
			int num13 = 0;
			int num14 = 0;
			if (ShowPlaceholder && PlaceholderText != null)
			{
				num12 = g.MeasureText(PlaceholderText, Font).Width;
			}
			foreach (CacheFont item in cache_font)
			{
				if (item.text == "\n" || item.text == "\r\n")
				{
					if (num13 > num12)
					{
						num12 = num13;
					}
					num14 += num;
					num13 = 0;
				}
				else if (!(item.text == "\r"))
				{
					num13 += item.width;
				}
			}
			if (num13 > num12)
			{
				num12 = num13;
			}
			return new Size(num2 + num3 + num12, num + num14 + num2);
		}
		int num15 = 0;
		foreach (CacheFont item2 in cache_font)
		{
			num15 += item2.width;
		}
		if (ShowPlaceholder && PlaceholderText != null)
		{
			int num16 = g.MeasureText(PlaceholderText, Font).Width;
			if (num16 > num15)
			{
				num15 = num16;
			}
		}
		return new Size(num2 + num3 + num15, num + num2);
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

	public virtual void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG)
		{
			CalculateRect();
			BeforeAutoSize();
		}
	}

	protected override void Dispose(bool disposing)
	{
		ThreadFocus?.Dispose();
		ThreadHover?.Dispose();
		ThreadLoading?.Dispose();
		CaretInfo.Dispose();
		ThreadAnimateBlink?.Dispose();
		fix_cache_font.Dispose();
		base.Dispose(disposing);
	}

	private void AddHistoryRecord()
	{
		if (history_I > -1)
		{
			history_Log.RemoveRange(history_I + 1, history_Log.Count - (history_I + 1));
			history_I = -1;
		}
		if (!IsPassWord || PasswordCopy)
		{
			history_Log.Add(new TextHistoryRecord(this));
		}
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if ((keyData & Keys.Alt) != Keys.Alt)
		{
			hasAlt = false;
			switch (keyData & Keys.KeyCode)
			{
			case Keys.Tab:
				if (Multiline && AcceptsTab)
				{
					return (keyData & Keys.Control) == 0;
				}
				return false;
			case Keys.Escape:
				return AcceptsEscape;
			case Keys.Left:
			case Keys.Up:
			case Keys.Right:
			case Keys.Down:
				return true;
			case Keys.Back:
				return !readOnly;
			}
		}
		else
		{
			hasAlt = true;
		}
		return base.IsInputKey(keyData);
	}

	protected override bool ProcessDialogChar(char charCode)
	{
		if (AdapterSystemMnemonic && (Control.ModifierKeys & Keys.Alt) == 0)
		{
			OnKeyPress(new KeyPressEventArgs(charCode));
			IKeyPress(charCode);
			return true;
		}
		return base.ProcessDialogChar(charCode);
	}

	public void HandKeyBoard(Keys key)
	{
		if (OnVerifyKeyboard(key))
		{
			switch (key)
			{
			case Keys.Back:
				ProcessShortcutKeys(ShortcutKeys.Back);
				break;
			case Keys.Back | Keys.Control:
				ProcessShortcutKeys(ShortcutKeys.BackControl);
				break;
			case Keys.Delete:
				ProcessShortcutKeys(ShortcutKeys.Delete);
				break;
			case Keys.Tab:
				ProcessShortcutKeys(ShortcutKeys.Tab);
				break;
			case Keys.Return:
				ProcessShortcutKeys(ShortcutKeys.Enter);
				break;
			case Keys.A | Keys.Control:
				ProcessShortcutKeys(ShortcutKeys.ControlA);
				break;
			case Keys.C | Keys.Control:
				ProcessShortcutKeys(ShortcutKeys.ControlC);
				break;
			case Keys.X | Keys.Control:
				ProcessShortcutKeys(ShortcutKeys.ControlX);
				break;
			case Keys.V | Keys.Control:
				ProcessShortcutKeys(ShortcutKeys.ControlV);
				break;
			case Keys.Z | Keys.Control:
				ProcessShortcutKeys(ShortcutKeys.ControlZ);
				break;
			case Keys.Y | Keys.Control:
				ProcessShortcutKeys(ShortcutKeys.ControlY);
				break;
			case Keys.Left:
				ProcessShortcutKeys(ShortcutKeys.Left);
				break;
			case Keys.Left | Keys.Shift:
				ProcessShortcutKeys(ShortcutKeys.LeftShift);
				break;
			case Keys.Up:
				ProcessShortcutKeys(ShortcutKeys.Up);
				break;
			case Keys.Up | Keys.Shift:
				ProcessShortcutKeys(ShortcutKeys.UpShift);
				break;
			case Keys.Right:
				ProcessShortcutKeys(ShortcutKeys.Right);
				break;
			case Keys.Right | Keys.Shift:
				ProcessShortcutKeys(ShortcutKeys.RightShift);
				break;
			case Keys.Down:
				ProcessShortcutKeys(ShortcutKeys.Down);
				break;
			case Keys.Down | Keys.Shift:
				ProcessShortcutKeys(ShortcutKeys.DownShift);
				break;
			case Keys.Home:
				ProcessShortcutKeys(ShortcutKeys.Home);
				break;
			case Keys.Home | Keys.Control:
				ProcessShortcutKeys(ShortcutKeys.HomeControl);
				break;
			case Keys.Home | Keys.Shift:
				ProcessShortcutKeys(ShortcutKeys.HomeShift);
				break;
			case Keys.Home | Keys.Shift | Keys.Control:
				ProcessShortcutKeys(ShortcutKeys.HomeControlShift);
				break;
			case Keys.End:
				ProcessShortcutKeys(ShortcutKeys.End);
				break;
			case Keys.End | Keys.Control:
				ProcessShortcutKeys(ShortcutKeys.EndControl);
				break;
			case Keys.End | Keys.Shift:
				ProcessShortcutKeys(ShortcutKeys.EndShift);
				break;
			case Keys.End | Keys.Shift | Keys.Control:
				ProcessShortcutKeys(ShortcutKeys.EndControlShift);
				break;
			case Keys.Prior:
				ProcessShortcutKeys(ShortcutKeys.PageUp);
				break;
			case Keys.Next:
				ProcessShortcutKeys(ShortcutKeys.PageDown);
				break;
			}
		}
	}

	private void HandKeyDown(Keys key)
	{
		if (!OnVerifyKeyboard(key))
		{
			return;
		}
		switch (key)
		{
		case Keys.Tab:
			if (AcceptsTab)
			{
				ProcessShortcutKeys(ShortcutKeys.Tab);
			}
			break;
		case Keys.Left:
			ProcessShortcutKeys(ShortcutKeys.Left);
			break;
		case Keys.Left | Keys.Shift:
			ProcessShortcutKeys(ShortcutKeys.LeftShift);
			break;
		case Keys.Up:
			ProcessShortcutKeys(ShortcutKeys.Up);
			break;
		case Keys.Up | Keys.Shift:
			ProcessShortcutKeys(ShortcutKeys.UpShift);
			break;
		case Keys.Right:
			ProcessShortcutKeys(ShortcutKeys.Right);
			break;
		case Keys.Right | Keys.Shift:
			ProcessShortcutKeys(ShortcutKeys.RightShift);
			break;
		case Keys.Down:
			ProcessShortcutKeys(ShortcutKeys.Down);
			break;
		case Keys.Down | Keys.Shift:
			ProcessShortcutKeys(ShortcutKeys.DownShift);
			break;
		case Keys.Back:
			ProcessShortcutKeys(ShortcutKeys.Back);
			break;
		case Keys.Back | Keys.Control:
			ProcessShortcutKeys(ShortcutKeys.BackControl);
			break;
		case Keys.Delete:
			ProcessShortcutKeys(ShortcutKeys.Delete);
			break;
		case Keys.Return:
			ProcessShortcutKeys(ShortcutKeys.Enter);
			break;
		case Keys.A | Keys.Control:
			ProcessShortcutKeys(ShortcutKeys.ControlA);
			break;
		case Keys.C | Keys.Control:
			ProcessShortcutKeys(ShortcutKeys.ControlC);
			break;
		case Keys.X | Keys.Control:
			ProcessShortcutKeys(ShortcutKeys.ControlX);
			break;
		case Keys.V | Keys.Control:
			ProcessShortcutKeys(ShortcutKeys.ControlV);
			break;
		case Keys.Z | Keys.Control:
			ProcessShortcutKeys(ShortcutKeys.ControlZ);
			break;
		case Keys.Y | Keys.Control:
			ProcessShortcutKeys(ShortcutKeys.ControlY);
			break;
		case Keys.Home:
			ProcessShortcutKeys(ShortcutKeys.Home);
			break;
		case Keys.Home | Keys.Control:
			ProcessShortcutKeys(ShortcutKeys.HomeControl);
			break;
		case Keys.Home | Keys.Shift:
			ProcessShortcutKeys(ShortcutKeys.HomeShift);
			break;
		case Keys.Home | Keys.Shift | Keys.Control:
			ProcessShortcutKeys(ShortcutKeys.HomeControlShift);
			break;
		case Keys.End:
			ProcessShortcutKeys(ShortcutKeys.End);
			break;
		case Keys.End | Keys.Control:
			ProcessShortcutKeys(ShortcutKeys.EndControl);
			break;
		case Keys.End | Keys.Shift:
			ProcessShortcutKeys(ShortcutKeys.EndShift);
			break;
		case Keys.End | Keys.Shift | Keys.Control:
			ProcessShortcutKeys(ShortcutKeys.EndControlShift);
			break;
		case Keys.Prior:
			ProcessShortcutKeys(ShortcutKeys.PageUp);
			break;
		case Keys.Next:
			ProcessShortcutKeys(ShortcutKeys.PageDown);
			break;
		}
	}

	private bool HandKeyUp(Keys key)
	{
		return false;
	}

	private Keys GetKeyBoard(int code)
	{
		bool flag = Control.ModifierKeys.HasFlag(Keys.Control);
		bool flag2 = Control.ModifierKeys.HasFlag(Keys.Shift);
		if (flag & flag2)
		{
			return (Keys)(code | 0x20000 | 0x10000);
		}
		if (flag)
		{
			if (code == 131072)
			{
				return (Keys)code;
			}
			return (Keys)(code | 0x20000);
		}
		if (flag2)
		{
			if (code == 65536)
			{
				return (Keys)code;
			}
			return (Keys)(code | 0x10000);
		}
		return (Keys)code;
	}

	internal void IKeyPress(char keyChar)
	{
		if (keyChar >= ' ' && keyChar != '\u007f' && Verify(keyChar, out string change))
		{
			EnterText(change ?? keyChar.ToString());
		}
	}

	protected virtual bool OnVerifyKeyboard(Keys keyData)
	{
		if (VerifyKeyboard == null)
		{
			return true;
		}
		InputVerifyKeyboardEventArgs e = new InputVerifyKeyboardEventArgs(keyData);
		VerifyKeyboard(this, e);
		return e.Result;
	}

	/// <summary>
	/// 主动触发快捷键
	/// </summary>
	/// <param name="keyData">键盘数据</param>
	/// <returns>返回true拦截消息</returns>
	public virtual void ProcessShortcutKeys(ShortcutKeys keyData)
	{
		switch (keyData)
		{
		case ShortcutKeys.Back:
			ProcessBackSpaceKey();
			break;
		case ShortcutKeys.BackControl:
			ProcessBackSpaceKey(ctrl: true);
			break;
		case ShortcutKeys.Delete:
			ProcessDelete();
			break;
		case ShortcutKeys.Left:
			if (IsRTL)
			{
				ProcessRightKey(shift: false);
			}
			else
			{
				ProcessLeftKey(shift: false);
			}
			break;
		case ShortcutKeys.LeftShift:
			if (IsRTL)
			{
				ProcessRightKey(shift: true);
			}
			else
			{
				ProcessLeftKey(shift: true);
			}
			break;
		case ShortcutKeys.Up:
			if (multiline)
			{
				ProcessUpKey(shift: false);
			}
			else
			{
				ProcessLeftKey(shift: false);
			}
			break;
		case ShortcutKeys.UpShift:
			if (multiline)
			{
				ProcessUpKey(shift: true);
			}
			else
			{
				ProcessLeftKey(shift: false);
			}
			break;
		case ShortcutKeys.Right:
			if (IsRTL)
			{
				ProcessLeftKey(shift: false);
			}
			else
			{
				ProcessRightKey(shift: false);
			}
			break;
		case ShortcutKeys.RightShift:
			if (IsRTL)
			{
				ProcessLeftKey(shift: true);
			}
			else
			{
				ProcessRightKey(shift: true);
			}
			break;
		case ShortcutKeys.Down:
			if (multiline)
			{
				ProcessDownKey(shift: false);
			}
			else
			{
				ProcessRightKey(shift: false);
			}
			break;
		case ShortcutKeys.DownShift:
			if (multiline)
			{
				ProcessDownKey(shift: true);
			}
			else
			{
				ProcessRightKey(shift: false);
			}
			break;
		case ShortcutKeys.Home:
			ProcessHomeKey(ctrl: false, shift: false);
			break;
		case ShortcutKeys.End:
			ProcessEndKey(ctrl: false, shift: false);
			break;
		case ShortcutKeys.HomeControl:
			ProcessHomeKey(ctrl: true, shift: false);
			break;
		case ShortcutKeys.EndControl:
			ProcessEndKey(ctrl: true, shift: false);
			break;
		case ShortcutKeys.HomeShift:
			ProcessHomeKey(ctrl: false, shift: true);
			break;
		case ShortcutKeys.EndShift:
			ProcessEndKey(ctrl: false, shift: true);
			break;
		case ShortcutKeys.HomeControlShift:
			ProcessHomeKey(ctrl: true, shift: true);
			break;
		case ShortcutKeys.EndControlShift:
			ProcessEndKey(ctrl: true, shift: true);
			break;
		case ShortcutKeys.Tab:
			if (AcceptsTab)
			{
				EnterText("\t", isMax: true, isVirtual: true);
			}
			break;
		case ShortcutKeys.Enter:
			if (multiline)
			{
				EnterText(Environment.NewLine, isMax: true, isVirtual: true);
			}
			break;
		case ShortcutKeys.ControlA:
			SelectAll();
			break;
		case ShortcutKeys.ControlC:
			Copy();
			break;
		case ShortcutKeys.ControlX:
			Cut();
			break;
		case ShortcutKeys.ControlV:
			Paste();
			break;
		case ShortcutKeys.ControlZ:
			Undo();
			break;
		case ShortcutKeys.ControlY:
			Redo();
			break;
		case ShortcutKeys.PageUp:
		{
			if (!ScrollY.Show || cache_font == null)
			{
				break;
			}
			bool flag4 = SetSelectionLength(0);
			CacheCaret caretPostionUp = GetCaretPostionUp();
			if (caretPostionUp == null)
			{
				if (SetSelectionStart(0) | flag4)
				{
					Invalidate();
				}
				break;
			}
			bool flag5 = SetSelectionStart(caretPostionUp.i, caret: false);
			bool flag6 = SetCaretPostion(caretPostionUp.index);
			if (flag5 | flag6)
			{
				Invalidate();
			}
			break;
		}
		case ShortcutKeys.PageDown:
			if (ScrollY.Show && cache_font != null)
			{
				bool flag = SetSelectionLength(0);
				bool flag2 = false;
				CacheCaret caretPostionDown = GetCaretPostionDown();
				bool flag3;
				if (caretPostionDown == null)
				{
					flag3 = SetSelectionStart(cache_font.Count);
				}
				else
				{
					flag3 = SetSelectionStart(caretPostionDown.i, caret: false);
					flag2 = SetCaretPostion(caretPostionDown.index);
				}
				if (flag3 | flag | flag2)
				{
					Invalidate();
				}
			}
			break;
		}
	}

	/// <summary>
	/// 删除文本
	/// </summary>
	private void ProcessBackSpaceKey(bool ctrl = false)
	{
		if (ReadOnly || BanInput)
		{
			return;
		}
		if (cache_font == null)
		{
			IBackSpaceKey();
		}
		else if (selectionLength > 0)
		{
			int start = selectionStartTemp;
			int end = selectionLength;
			AddHistoryRecord();
			ProcessBackSpaceKey(cache_font, start, end);
		}
		else
		{
			if (selectionStart <= 0)
			{
				return;
			}
			AddHistoryRecord();
			int num = selectionStart - 1;
			if (ctrl && num > 1)
			{
				ProcessBackSpaceKey(cache_font, FindStart(cache_font, num - 1), num + 1);
				return;
			}
			int num2 = num;
			int num3 = CurrentPosIndex - 1;
			bool flag = SetTextRemove(ref cache_font, num2, 1);
			bool flag2 = SetSelectionStart(num2, caret: false);
			bool flag3 = SetCaretPostion((num2 != 0) ? num3 : 0, rd: false);
			if (flag | flag2 | flag3)
			{
				Invalidate();
			}
		}
	}

	private void ProcessBackSpaceKey(List<CacheFont> cache_font, int start, int end)
	{
		bool flag = SetTextRemove(ref cache_font, start, end);
		bool flag2 = SetSelectionLength(0);
		bool flag3 = SetSelectionStart(start);
		if (flag | flag3 | flag2)
		{
			Invalidate();
		}
	}

	/// <summary>
	/// 删除文本
	/// </summary>
	private void ProcessDelete()
	{
		if (cache_font == null || ReadOnly || BanInput)
		{
			return;
		}
		if (selectionLength > 0)
		{
			int num = selectionStartTemp;
			int end = selectionLength;
			AddHistoryRecord();
			bool flag = SetTextRemove(ref cache_font, num, end);
			bool flag2 = SetSelectionLength(0);
			bool flag3 = SetSelectionStart(num);
			if (flag | flag3 | flag2)
			{
				Invalidate();
			}
		}
		else if (selectionStart < cache_font.Count)
		{
			int num2 = selectionStart;
			bool flag4 = SetTextRemove(ref cache_font, num2, 1);
			bool flag5 = SetSelectionStart(num2);
			if (flag4 | flag5)
			{
				Invalidate();
			}
		}
	}

	private void ProcessLeftKey(bool shift)
	{
		if (shift)
		{
			int num = selectionStartTemp;
			if (selectionStartTemp == selectionStart || selectionStartTemp < selectionStart)
			{
				selectionStartTemp--;
			}
			if (selectionStartTemp < 0)
			{
				selectionStartTemp = 0;
			}
			if (num != selectionStartTemp)
			{
				selectionStart = selectionStartTemp;
				bool flag = SetSelectionLength(selectionLength + 1);
				bool flag2 = SetCaretPostion(CurrentPosIndex - 1);
				if (flag | flag2)
				{
					Invalidate();
				}
			}
		}
		else if (SelectionLength > 0)
		{
			bool flag3;
			if (selectionStartTemp < selectionStart)
			{
				flag3 = SetSelectionStart(selectionStartTemp);
			}
			else
			{
				int value = selectionStart;
				selectionStart--;
				flag3 = SetSelectionStart(value);
			}
			bool flag4 = SetSelectionLength(0);
			if (flag3 | flag4)
			{
				Invalidate();
			}
		}
		else
		{
			bool flag5 = SetSelectionLength(0);
			if (SetSelectionStart(selectionStart - 1) | flag5)
			{
				Invalidate();
			}
		}
	}

	private void ProcessRightKey(bool shift)
	{
		if (shift)
		{
			bool flag;
			if (selectionStart > selectionStartTemp)
			{
				selectionStartTemp++;
				flag = SetSelectionLength(selectionLength - 1);
			}
			else
			{
				flag = SetSelectionLength(selectionLength + 1);
			}
			bool flag2 = SetCaretPostion(CurrentPosIndex + 1);
			if (flag | flag2)
			{
				Invalidate();
			}
			return;
		}
		bool flag3;
		bool flag4;
		if (SelectionLength > 0)
		{
			if (selectionStartTemp > selectionStart)
			{
				flag3 = SetSelectionStart(selectionStartTemp + selectionLength);
			}
			else
			{
				int num = selectionStart;
				selectionStart--;
				flag3 = SetSelectionStart(num + selectionLength);
			}
			flag4 = SetSelectionLength(0);
		}
		else
		{
			flag4 = SetSelectionLength(0);
			flag3 = SetSelectionStart(selectionStart + 1);
		}
		if (flag3 | flag4)
		{
			Invalidate();
		}
	}

	private void ProcessUpKey(bool shift)
	{
		bool flag;
		bool flag2;
		if (shift)
		{
			if (cache_font == null)
			{
				flag = SetSelectionLength(0);
				flag2 = SetSelectionStart(selectionStart - 1);
			}
			else
			{
				int num = selectionStartTemp;
				int num2 = cache_font.Count - 1;
				if (num > num2)
				{
					num = num2;
				}
				CacheCaret caretPostionUp = GetCaretPostionUp();
				if (caretPostionUp == null)
				{
					flag2 = (flag = false);
				}
				else
				{
					flag2 = SetSelectionStart(caretPostionUp.i);
					flag = SetSelectionLength(selectionLength + (num - caretPostionUp.i + ((num >= num2) ? 1 : 0)));
				}
			}
		}
		else
		{
			flag = SetSelectionLength(0);
			if (cache_font == null)
			{
				flag2 = SetSelectionStart(selectionStart - 1);
			}
			else
			{
				CacheCaret caretPostionUp2 = GetCaretPostionUp();
				flag2 = caretPostionUp2 != null && ((caretPostionUp2.i != selectionStart) ? SetSelectionStart(caretPostionUp2.i) : SetSelectionStart(selectionStart - 1));
			}
		}
		if (flag2 | flag)
		{
			Invalidate();
		}
	}

	private void ProcessDownKey(bool shift)
	{
		bool flag;
		bool flag2;
		if (shift)
		{
			if (cache_font == null)
			{
				flag = SetSelectionLength(0);
				flag2 = SetSelectionStart(selectionStart + 1);
			}
			else
			{
				int num = selectionStartTemp + selectionLength;
				if (num > cache_font.Count - 1)
				{
					return;
				}
				CacheCaret caretPostionDown = GetCaretPostionDown();
				flag = caretPostionDown != null && SetSelectionLength(selectionLength + (caretPostionDown.i - num));
				if (caretPostionDown == null)
				{
					CurrentPosIndex = selectionStart + selectionLength;
				}
				else
				{
					CurrentPosIndex = caretPostionDown.index;
				}
				flag2 = SetCaretPostion();
			}
		}
		else
		{
			flag = SetSelectionLength(0);
			if (cache_font == null)
			{
				flag2 = SetSelectionStart(selectionStart + 1);
			}
			else
			{
				CacheCaret caretPostionDown2 = GetCaretPostionDown();
				flag2 = caretPostionDown2 != null && SetSelectionStart(caretPostionDown2.i);
			}
		}
		if (flag2 | flag)
		{
			Invalidate();
		}
	}

	private void ProcessHomeKey(bool ctrl, bool shift)
	{
		bool flag = false;
		bool flag2;
		if (ctrl & shift)
		{
			int num = selectionStartTemp;
			if (num == 0)
			{
				return;
			}
			if (ScrollY.Show)
			{
				ScrollY.Value = 0;
			}
			flag = SetSelectionStart(0);
			flag2 = SetSelectionLength(selectionLength + num);
		}
		else
		{
			flag2 = SetSelectionLength(0);
			if (ctrl)
			{
				if (ScrollY.Show)
				{
					ScrollY.Value = 0;
				}
				flag = SetSelectionStart(0);
			}
			else if (multiline)
			{
				if (cache_font == null)
				{
					return;
				}
				int num2 = selectionStartTemp;
				if (num2 > 0)
				{
					int num3 = FindStartY(cache_font, num2 - 1);
					if (num3 == num2)
					{
						return;
					}
					flag = SetSelectionStart(num3);
				}
			}
			else
			{
				if (ScrollY.Show)
				{
					ScrollY.Value = 0;
				}
				flag = SetSelectionStart(0);
			}
		}
		if (flag | flag2)
		{
			Invalidate();
		}
	}

	private void ProcessEndKey(bool ctrl, bool shift)
	{
		if (cache_font == null)
		{
			return;
		}
		if (ctrl & shift)
		{
			if (selectionStartTemp + selectionLength <= cache_font.Count - 1)
			{
				if (ScrollY.Show)
				{
					ScrollY.Value = ScrollY.Max;
				}
				SelectionLength += cache_font.Count - selectionStartTemp;
			}
			return;
		}
		bool flag = false;
		bool flag2;
		if (ctrl)
		{
			if (ScrollY.Show)
			{
				ScrollY.Value = ScrollY.Max;
			}
			flag = SetSelectionLength(0);
			flag2 = SetSelectionStart(cache_font.Count);
		}
		else if (multiline)
		{
			int num = selectionStartTemp + selectionLength;
			if (num > cache_font.Count - 1)
			{
				return;
			}
			int num2 = FindEndY(cache_font, num) + 1;
			if (num2 == num)
			{
				return;
			}
			flag2 = SetSelectionStart(num2);
		}
		else
		{
			if (ScrollY.Show)
			{
				ScrollY.Value = ScrollY.Max;
			}
			flag = SetSelectionLength(0);
			flag2 = SetSelectionStart(cache_font.Count);
		}
		if (flag2 | flag)
		{
			Invalidate();
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		FixFontWidth(force: true);
		this.AddListener();
	}

	private bool FixFontWidth(bool force = false)
	{
		HasEmoji = false;
		HasRTLText = false;
		string text = Text;
		if (force)
		{
			rtl_run_dirty = true;
			fix_cache_font.Clear();
			return this.GDI((Canvas g) =>
			{
				_ = Dpi;
				int fontHeight = fix_cache_font.Height(g, Font);
				int num = 0;
				if (isempty)
				{
					TextTotalLine = 0;
					if (ScrollX.SetValue(0))
					{
						num++;
					}
					if (ScrollY.SetValue(0))
					{
						num++;
					}
					cache_font = null;
					cache_caret = null;
				}
				else
				{
					FixFontWidth(g, text, 0, force, fontHeight);
				}
				CaretInfo.Height = fontHeight;
				return CalculateRect() || num > 0;
			});
		}
		if (isempty)
		{
			return CleanCacheFont();
		}
		return this.GDI((Canvas g) =>
		{
			int fontHeight = fix_cache_font.Height(g, Font);
			if (text == null)
			{
				CaretInfo.Height = fontHeight;
				return false;
			}
			rtl_run_dirty = true;
			FixFontWidth(g, text, 0, force, fontHeight);
			CaretInfo.Height = fontHeight;
			return CalculateRect();
		});
	}

	private void FixFontWidth(Canvas g, string text, int start, bool force, int fontHeight)
	{
		if (force || cache_font == null)
		{
			fix_cache_font.Clear();
		}
		cache_font = FixFontWidth(g, text, start, fontHeight, out var _);
		SetStyle();
	}

	private List<CacheFont> FixFontWidth(string text, int start, out int length)
	{
		int len = 0;
		List<CacheFont> result = this.GDI((Canvas g) => FixFontWidth(g, text, start, null, out len));
		length = len;
		return result;
	}

	private List<CacheFont> FixFontWidth(Canvas g, string text, int start, int? fontHeight, out int length)
	{
		int len = 0;
		List<CacheFont> font_widths = new List<CacheFont>(text.Length);
		if (IsPassWord)
		{
			int num = fix_cache_font.Width(g, Font, PassWordChar);
			foreach (char c in text)
			{
				font_widths.Add(new CacheFont(start, c.ToString(), _emoji: false, num));
				len++;
				start++;
			}
		}
		else if (fontHeight.HasValue)
		{
			GraphemeSplitter.Each(text, (string txt, int ntype) =>
			{
				if (GraphemeSplitter.IsEmoji(ntype, txt))
				{
					HasEmoji = true;
					font_widths.Add(new CacheFont(start, txt, _emoji: true, fontHeight.Value));
				}
				else
				{
					TextDirection direction = GraphemeSplitter.GetDirection(txt);
					if (direction == TextDirection.RTL)
					{
						HasRTLText = true;
					}
					font_widths.Add(new CacheFont(start, txt, _emoji: false, fix_cache_font.Width(g, Font, txt))
					{
						dir = direction
					});
				}
				len++;
				start++;
			});
		}
		else
		{
			GraphemeSplitter.Each(text, (string txt, int ntype) =>
			{
				if (GraphemeSplitter.IsEmoji(ntype, txt))
				{
					HasEmoji = true;
					font_widths.Add(new CacheFont(start, txt, _emoji: true, fix_cache_font.Height(g, Font)));
				}
				else
				{
					TextDirection direction = GraphemeSplitter.GetDirection(txt);
					if (direction == TextDirection.RTL)
					{
						HasRTLText = true;
					}
					font_widths.Add(new CacheFont(start, txt, _emoji: false, fix_cache_font.Width(g, Font, txt))
					{
						dir = direction
					});
				}
				len++;
				start++;
			});
		}
		length = len;
		return font_widths;
	}

	private bool CleanCacheFont()
	{
		TextTotalLine = 0;
		HasRTLText = false;
		bool flag = ScrollX.SetValue(0);
		bool flag2 = ScrollY.SetValue(0);
		cache_font = null;
		cache_caret = null;
		return CalculateRect() | flag | flag2;
	}

	internal bool CalculateRect()
	{
		BeforeAutoSize();
		Rectangle rect = (RECTDIV.HasValue ? RECTDIV.Value.PaddingRect(Padding).ReadRect(((float)WaveSize + borderWidth / 2f) * Dpi, joinMode, JoinLeft, JoinRight) : ReadRectangle);
		int num = (int)((float)CaretInfo.Height * paddgap);
		int sps = num * 2;
		rect.Width -= UR;
		RectAuto(rect, num, sps);
		int num2 = 0;
		if (cache_font == null)
		{
			TextTotalLine = 0;
			oldtmpY = (oldtmpX = null);
			ScrollX.Clear();
			ScrollY.Clear();
			if (ModeRange)
			{
				HandTextModeRange();
			}
			if (CaretInfo.SetXY(rect_text.X, rect_text.Y))
			{
				num2++;
			}
		}
		else
		{
			if (rtl_run_dirty)
			{
				rtl_run_dirty = false;
				if (IsRTL)
				{
					this.GDI((Canvas g) =>
					{
						FixRTLRunWidth(g, cache_font);
					});
				}
			}
			int num3 = 0;
			if (multiline)
			{
				Rectangle rectText = rect_text;
				bool flag = false;
				if (ScrollY.Show)
				{
					rectText.Width -= (int)(16f * Dpi) - num;
				}
				else
				{
					flag = true;
				}
				int num4 = CaretInfo.Height + ((lineheight > 0) ? ((int)((float)lineheight * Dpi)) : 0);
				int[] array = CalculateRectMultiline(cache_font, rectText, num, num4);
				if (flag && array[1] + num4 > rect_text.Height)
				{
					rectText.Width -= (int)(16f * Dpi) - num;
					array = CalculateRectMultiline(cache_font, rectText, num, num4);
				}
				num3 = array[0];
				HandTextAlignCore(cache_font);
			}
			else
			{
				TextTotalLine = 1;
				HandTextAlign(cache_font);
			}
			if (cache_font.Count == 0)
			{
				TextTotalLine = 0;
				isempty = true;
				_text = "";
				cache_font = null;
				cache_caret = null;
				return true;
			}
			CacheFont cacheFont = cache_font[cache_font.Count - 1];
			bool isRTL = IsRTL;
			List<CacheCaret> list = new List<CacheCaret>(cache_font.Count + 2)
			{
				new CacheCaret
				{
					x = (isRTL ? cache_font[0].rect.Right : cache_font[0].rect.X),
					y = rect_text.Y,
					i = 0,
					index = 0
				}
			};
			int num5 = 1;
			for (int num6 = 0; num6 < cache_font.Count; num6++)
			{
				CacheFont cacheFont2 = cache_font[num6];
				if (cacheFont2.ret)
				{
					if (num6 > 0)
					{
						CacheFont cacheFont3 = cache_font[num6 - 1];
						list.Add(new CacheCaret
						{
							index = num5,
							i = num6,
							x = (isRTL ? cacheFont3.rect.X : cacheFont3.rect.Right),
							y = cacheFont3.rect.Y
						});
					}
					else
					{
						list.Add(new CacheCaret
						{
							index = num5,
							i = num6 + 1,
							x = cacheFont2.rect.X,
							y = cacheFont2.rect.Y
						});
					}
				}
				else
				{
					list.Add(new CacheCaret
					{
						index = num5,
						i = num6,
						x = (isRTL ? cacheFont2.rect.Right : cacheFont2.rect.X),
						y = cacheFont2.rect.Y
					});
				}
				num5++;
			}
			list.Add(new CacheCaret
			{
				index = num5,
				i = cache_font.Count,
				x = (isRTL ? cacheFont.rect.X : cacheFont.rect.Right),
				y = cacheFont.rect.Y
			});
			cache_caret = list.ToArray();
			if (isRTL)
			{
				int num7 = int.MaxValue;
				int num8 = int.MinValue;
				foreach (CacheFont item in cache_font)
				{
					if (item.rect.X < num7)
					{
						num7 = item.rect.X;
					}
					if (item.rect.Right > num8)
					{
						num8 = item.rect.Right;
					}
				}
				ScrollX.Min = -Math.Max(0, rect_text.X - num7);
				ScrollX.Max = Math.Max(0, num8 - rect_text.Right);
			}
			else
			{
				ScrollX.Max = cacheFont.rect.Right - rect_text.Right;
				switch (textalign)
				{
				case HorizontalAlignment.Center:
					if (ScrollX.Max > 0)
					{
						ScrollX.Min = -ScrollX.Max;
						break;
					}
					ScrollX.Min = ScrollX.Max;
					ScrollX.Max = -ScrollX.Max;
					break;
				case HorizontalAlignment.Right:
					ScrollX.Min = cache_font[0].rect.Right - rect.Right + num;
					ScrollX.Max = 0;
					break;
				default:
					ScrollX.Min = 0;
					break;
				}
			}
			ScrollY.Max = cacheFont.rect.Bottom - rect.Height + num;
			if (multiline)
			{
				if (ScrollX.SetValue(0))
				{
					num2++;
				}
				if (wordwrap)
				{
					ScrollX.Show = false;
				}
				else if (isRTL)
				{
					ScrollX.Show = ScrollX.Max > 0 || ScrollX.Min < 0;
				}
				else
				{
					ScrollX.Max = num3 - rect_text.Right;
					ScrollX.Show = num3 > rect.Right;
				}
				ScrollY.Show = cacheFont.rect.Bottom > rect.Bottom;
				if (ScrollY.Show)
				{
					if (ScrollY.Value > ScrollY.Max && ScrollY.SetValue(ScrollY.Max))
					{
						num2++;
					}
				}
				else if (ScrollY.SetValue(0))
				{
					num2++;
				}
			}
			else
			{
				oldtmpY = (oldtmpX = null);
				ScrollY.Show = false;
				ScrollY.Value = 0;
				if (isRTL)
				{
					ScrollX.Show = ScrollX.Max > 0 || ScrollX.Min < 0;
					if (ScrollX.Show)
					{
						if (ScrollX.Value > ScrollX.Max && ScrollX.SetValue(ScrollX.Max))
						{
							num2++;
						}
						else if (ScrollX.Value < ScrollX.Min && ScrollX.SetValue(ScrollX.Min))
						{
							num2++;
						}
					}
					else if (ScrollX.SetValue(0))
					{
						num2++;
					}
				}
				else
				{
					if (textalign == HorizontalAlignment.Right)
					{
						ScrollX.Show = cacheFont.rect.Right < rect.Right;
					}
					else
					{
						ScrollX.Show = cacheFont.rect.Right > rect_text.Right;
					}
					if (ScrollX.Show)
					{
						if (ScrollX.Value > ScrollX.Max && ScrollX.SetValue(ScrollX.Max))
						{
							num2++;
						}
					}
					else if (ScrollX.SetValue(0))
					{
						num2++;
					}
				}
			}
		}
		if (SetCaretPostion())
		{
			num2++;
		}
		return num2 > 0;
	}

	private int[] CalculateRectMultiline(List<CacheFont> cache_font, Rectangle rectText, int sps, int lineHeight)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		bool isRTL = IsRTL;
		foreach (CacheFont item in cache_font)
		{
			if (item.text == "\n" || item.text == "\r\n")
			{
				bool hide = (item.ret = true);
				item.hide = hide;
				item.line = num4;
				num4++;
				if (num2 > num)
				{
					num = num2;
				}
				num3 += lineHeight;
				num2 = 0;
				item.rect = new Rectangle(isRTL ? (rectText.Right - num2) : (rectText.X + num2), rectText.Y + num3, 0, CaretInfo.Height);
				continue;
			}
			if (item.text == " " || item.text == "\t")
			{
				item.hide = true;
			}
			else
			{
				if (item.text == "\r")
				{
					item.hide = true;
					item.rect = new Rectangle(isRTL ? (rectText.Right - num2 - item.width) : (rectText.X + num2), rectText.Y + num3, item.width, CaretInfo.Height);
					continue;
				}
				if (wordwrap && num2 + item.width > rectText.Width)
				{
					num4++;
					num3 += lineHeight;
					num2 = 0;
				}
			}
			item.line = num4;
			item.rect = new Rectangle(isRTL ? (rectText.Right - num2 - item.width) : (rectText.X + num2), rectText.Y + num3, item.width, CaretInfo.Height);
			num2 += item.width;
		}
		TextTotalLine = num4;
		return new int[2] { num, num3 };
	}

	private int HandTextModeRange()
	{
		int num = CaretInfo.Height;
		int num2 = rect_text.Width / 2;
		int num3 = num2 - num;
		rect_d_ico = new Rectangle(rect_text.X + num2 - num / 2, rect_text.Y + (rect_text.Height - num) / 2, num, num);
		rect_d_l = new Rectangle(rect_text.X, rect_text.Y, num3, rect_text.Height);
		rect_d_r = new Rectangle(rect_text.X + num2 + num, rect_text.Y, num3, rect_text.Height);
		return num;
	}

	private void HandTextAlign(List<CacheFont> cache_font)
	{
		int num = 0;
		if (ModeRange)
		{
			int num2 = HandTextModeRange();
			int num3 = GetTabIndex(cache_font);
			List<int> list = new List<int>(cache_font.Count);
			List<int> list2 = new List<int>(list.Count);
			if (num3 == -1)
			{
				for (int i = 0; i < cache_font.Count; i++)
				{
					CacheFont cacheFont = cache_font[i];
					cacheFont.rect = new Rectangle(rect_d_l.X + num, rect_text.Y, cacheFont.width, num2);
					num += cacheFont.width;
					list.Add(i);
				}
			}
			else if (num3 > 0)
			{
				for (int j = 0; j < num3; j++)
				{
					CacheFont cacheFont2 = cache_font[j];
					cacheFont2.rect = new Rectangle(rect_d_l.X + num, rect_text.Y, cacheFont2.width, num2);
					num += cacheFont2.width;
					list.Add(j);
				}
				Rectangle rect = cache_font[num3 - 1].rect;
				cache_font[num3].rect = new Rectangle(rect.Right, rect.Y, 0, rect.Height);
				int num4 = 0;
				for (int k = num3 + 1; k < cache_font.Count; k++)
				{
					CacheFont cacheFont3 = cache_font[k];
					cacheFont3.rect = new Rectangle(rect_d_r.X + num4, rect_text.Y, cacheFont3.width, num2);
					num4 += cacheFont3.width;
					list2.Add(k);
				}
			}
			else
			{
				cache_font[0].rect = new Rectangle(rect_d_r.X, rect_d_r.Y, cache_font[0].width, num2);
				int num5 = 0;
				for (int l = num3 + 1; l < cache_font.Count; l++)
				{
					CacheFont cacheFont4 = cache_font[l];
					cacheFont4.rect = new Rectangle(rect_d_r.X + num5, rect_text.Y, cacheFont4.width, num2);
					num5 += cacheFont4.width;
					list2.Add(l);
				}
			}
			if (textalign == HorizontalAlignment.Right)
			{
				if (list.Count > 0)
				{
					int num6 = rect_d_l.Right - cache_font[list[list.Count - 1]].rect.Right;
					foreach (int item in list)
					{
						CacheFont cacheFont5 = cache_font[item];
						Rectangle rect2 = cacheFont5.rect;
						rect2.Offset(num6, 0);
						cacheFont5.rect = rect2;
					}
					if (num3 > 0)
					{
						Rectangle rect3 = cache_font[num3].rect;
						rect3.Offset(num6, 0);
						cache_font[num3].rect = rect3;
					}
				}
				if (list2.Count <= 0)
				{
					return;
				}
				int num7 = rect_d_r.Right - cache_font[list2[list2.Count - 1]].rect.Right;
				{
					foreach (int item2 in list2)
					{
						CacheFont cacheFont6 = cache_font[item2];
						Rectangle rect4 = cacheFont6.rect;
						rect4.Offset(num7, 0);
						cacheFont6.rect = rect4;
					}
					return;
				}
			}
			if (textalign != HorizontalAlignment.Center)
			{
				return;
			}
			if (list.Count > 0)
			{
				int num8 = (rect_d_l.Right - cache_font[list[list.Count - 1]].rect.Right) / 2;
				foreach (int item3 in list)
				{
					CacheFont cacheFont7 = cache_font[item3];
					Rectangle rect5 = cacheFont7.rect;
					rect5.Offset(num8, 0);
					cacheFont7.rect = rect5;
				}
				if (num3 > 0)
				{
					Rectangle rect6 = cache_font[num3].rect;
					rect6.Offset(num8, 0);
					cache_font[num3].rect = rect6;
				}
			}
			if (list2.Count <= 0)
			{
				return;
			}
			int num9 = (rect_d_r.Right - cache_font[list2[list2.Count - 1]].rect.Right) / 2;
			{
				foreach (int item4 in list2)
				{
					CacheFont cacheFont8 = cache_font[item4];
					Rectangle rect7 = cacheFont8.rect;
					rect7.Offset(num9, 0);
					cacheFont8.rect = rect7;
				}
				return;
			}
		}
		if (IsRTL)
		{
			foreach (CacheFont item5 in cache_font)
			{
				item5.rect = new Rectangle(rect_text.Right - num - item5.width, rect_text.Y, item5.width, CaretInfo.Height);
				num += item5.width;
			}
		}
		else
		{
			foreach (CacheFont item6 in cache_font)
			{
				item6.rect = new Rectangle(rect_text.X + num, rect_text.Y, item6.width, CaretInfo.Height);
				num += item6.width;
			}
		}
		HandTextAlignCore(cache_font);
	}

	private void HandTextAlignCore(List<CacheFont> cache_font)
	{
		if (IsRTL)
		{
			int num = -1;
			List<CacheFont> list = new List<CacheFont>(cache_font.Count);
			foreach (CacheFont item in cache_font)
			{
				if (item.rect.Y != num)
				{
					num = item.rect.Y;
					HandTextAlignRTL(ref list);
				}
				list.Add(item);
			}
			HandTextAlignRTL(ref list);
		}
		else if (textalign == HorizontalAlignment.Right)
		{
			int num2 = -1;
			List<CacheFont> list2 = new List<CacheFont>(cache_font.Count);
			foreach (CacheFont item2 in cache_font)
			{
				if (item2.rect.Y != num2)
				{
					num2 = item2.rect.Y;
					HandTextAlignRight(ref list2);
				}
				list2.Add(item2);
			}
			HandTextAlignRight(ref list2);
		}
		else
		{
			if (textalign != HorizontalAlignment.Center)
			{
				return;
			}
			int num3 = -1;
			List<CacheFont> list3 = new List<CacheFont>(cache_font.Count);
			foreach (CacheFont item3 in cache_font)
			{
				if (item3.rect.Y != num3)
				{
					num3 = item3.rect.Y;
					HandTextAlignCenter(ref list3);
				}
				list3.Add(item3);
			}
			HandTextAlignCenter(ref list3);
		}
	}

	private void HandTextAlignRight(ref List<CacheFont> list)
	{
		if (list.Count <= 0)
		{
			return;
		}
		CacheFont cacheFont = list[list.Count - 1];
		if (cacheFont.rect.Right < rect_text.Right)
		{
			int num = rect_text.Right - cacheFont.rect.Right;
			foreach (CacheFont item in list)
			{
				Rectangle rect = item.rect;
				rect.Offset(num, 0);
				item.rect = rect;
			}
		}
		list.Clear();
	}

	private void HandTextAlignCenter(ref List<CacheFont> list)
	{
		if (list.Count <= 0)
		{
			return;
		}
		CacheFont cacheFont = list[list.Count - 1];
		if (cacheFont.rect.Right < rect_text.Right)
		{
			int num = (rect_text.Right - cacheFont.rect.Right) / 2;
			foreach (CacheFont item in list)
			{
				Rectangle rect = item.rect;
				rect.Offset(num, 0);
				item.rect = rect;
			}
		}
		list.Clear();
	}

	private void HandTextAlignRTL(ref List<CacheFont> list)
	{
		if (list.Count <= 0)
		{
			return;
		}
		int num = int.MaxValue;
		int num2 = int.MinValue;
		foreach (CacheFont item in list)
		{
			if (item.rect.X < num)
			{
				num = item.rect.X;
			}
			if (item.rect.Right > num2)
			{
				num2 = item.rect.Right;
			}
		}
		int num3 = 0;
		if (textalign == HorizontalAlignment.Right)
		{
			num3 = rect_text.X - num;
		}
		else if (textalign == HorizontalAlignment.Center)
		{
			num3 = (rect_text.X + rect_text.Right - num - num2) / 2;
		}
		if (num3 != 0)
		{
			foreach (CacheFont item2 in list)
			{
				Rectangle rect = item2.rect;
				rect.Offset(num3, 0);
				item2.rect = rect;
			}
		}
		list.Clear();
	}

	/// <summary>
	/// 校正 RTL 连续字符簇宽度：整 run 测量（含上下文整形/连写）后按比例分配回各簇，保证簇宽之和等于整形后的 run 宽度
	/// </summary>
	internal void FixRTLRunWidth(Canvas g, List<CacheFont> cache_font)
	{
		int count = cache_font.Count;
		for (int i = 0; i < count; i++)
		{
			if (cache_font[i].dir != TextDirection.RTL)
			{
				continue;
			}
			int num = i;
			for (int j = i + 1; j < count; j++)
			{
				CacheFont cacheFont = cache_font[j];
				if (cacheFont.ret || (cacheFont.dir != TextDirection.RTL && cacheFont.dir != TextDirection.Neutral))
				{
					break;
				}
				num = j;
			}
			while (num > i && cache_font[num].dir == TextDirection.Neutral)
			{
				num--;
			}
			if (num > i)
			{
				StringBuilder stringBuilder = new StringBuilder();
				int num2 = 0;
				for (int k = i; k <= num; k++)
				{
					stringBuilder.Append(cache_font[k].text);
					num2 += cache_font[k].width;
				}
				if (num2 > 0)
				{
					int num3 = g.MeasureString(stringBuilder.ToString(), Font).Width;
					if (num3 != num2 && num3 > 0)
					{
						int num4 = 0;
						for (int l = i; l < num; l++)
						{
							int num5 = (int)((long)cache_font[l].width * (long)num3 / num2);
							cache_font[l].width = num5;
							num4 += num5;
						}
						cache_font[num].width = num3 - num4;
					}
				}
			}
			i = num;
		}
	}

	private int GetTabIndex(List<CacheFont> cache_font)
	{
		foreach (CacheFont item in cache_font)
		{
			if (item.text == "\t")
			{
				return item.i;
			}
		}
		return -1;
	}

	private void RectAuto(Rectangle rect, int sps, int sps2)
	{
		int read_height = CaretInfo.Height;
		string text = PrefixText;
		string text2 = SuffixText;
		bool has_prefixText = text != null;
		bool has_suffixText = text2 != null;
		bool has_prefix = HasPrefix;
		bool has_suffix = HasSuffix;
		if ((loading && LoadingValue > -1f) || is_clear)
		{
			int icon_right_size;
			int num = (icon_right_size = (int)((float)read_height * iconratio));
			if (iconratioRight.HasValue)
			{
				icon_right_size = (int)((float)read_height * iconratioRight.Value);
			}
			if (has_prefixText)
			{
				this.GDI((Canvas g) =>
				{
					RectLR(rect, read_height, sps, sps2, MeasurePrefix(g), read_height, icon_right_size, icon_right_size);
				});
			}
			else if (has_prefix)
			{
				RectLR(rect, read_height, sps, sps2, num, num, icon_right_size, icon_right_size);
			}
			else
			{
				RectR(rect, read_height, sps, sps2, icon_right_size, icon_right_size);
			}
			return;
		}
		if (has_prefixText | has_suffixText | has_prefix | has_suffix)
		{
			if (has_prefixText | has_suffixText)
			{
				this.GDI((Canvas g) =>
				{
					if (has_prefixText & has_suffixText)
					{
						RectLR(rect, read_height, sps, sps2, MeasurePrefix(g), read_height, MeasureSuffix(g), read_height);
					}
					else if (has_prefix | has_suffix)
					{
						if (has_prefixText)
						{
							if (has_suffix)
							{
								int num9 = (int)((float)read_height * (iconratioRight ?? iconratio));
								RectLR(rect, read_height, sps, sps2, MeasurePrefix(g), read_height, num9, num9);
							}
							else
							{
								RectL(rect, read_height, sps, sps2, MeasurePrefix(g));
							}
						}
						else if (has_prefix)
						{
							int num10 = (int)((float)read_height * iconratio);
							RectLR(rect, read_height, sps, sps2, num10, num10, MeasureSuffix(g), read_height);
						}
						else
						{
							RectR(rect, read_height, sps, sps2, MeasureSuffix(g));
						}
					}
					else if (has_prefixText)
					{
						RectL(rect, read_height, sps, sps2, MeasurePrefix(g));
					}
					else
					{
						RectR(rect, read_height, sps, sps2, MeasureSuffix(g));
					}
				});
			}
			else if (has_prefix | has_suffix)
			{
				int num2 = (int)((float)read_height * iconratio);
				int num3 = num2;
				if (iconratioRight.HasValue)
				{
					num3 = (int)((float)read_height * iconratioRight.Value);
				}
				if (has_prefix & has_suffix)
				{
					RectLR(rect, read_height, sps, sps2, num2, num2, num3, num3);
				}
				else if (has_prefix)
				{
					RectL(rect, read_height, sps, sps2, num2, num2);
				}
				else
				{
					RectR(rect, read_height, sps, sps2, num3, num3);
				}
			}
			return;
		}
		int num4 = -1;
		if (HasLeft())
		{
			int[] array = UseLeft(rect, CaretInfo.Height);
			if (array[0] > 0 || array[1] > 0)
			{
				int num5 = array[0];
				int num6 = array[1];
				rect.X += num5;
				rect.Width -= num5;
				rect.Y += num6;
				rect.Height -= num6;
				num4 = num6;
			}
		}
		ref Rectangle reference = ref rect_l;
		int num7 = (rect_r.Width = 0);
		reference.Width = num7;
		if (multiline)
		{
			rect_text = Helper.DeflateRectLR(rect.X + sps, rect.Y + sps, rect.Width - sps2, rect.Height - sps2, PaddingLR, Dpi);
		}
		else
		{
			rect_text = Helper.DeflateRectLR(rect.X + sps, rect.Y + (rect.Height - read_height) / 2, rect.Width - sps2, read_height, PaddingLR, Dpi);
		}
		if (num4 > -1)
		{
			UseLeftAutoHeight(read_height, num4);
		}
	}

	private int MeasurePrefix(Canvas g)
	{
		if (!PrefixWidth.HasValue)
		{
			return g.MeasureString(PrefixText, Font).Width;
		}
		return (int)((float?)PrefixWidth * Dpi).Value;
	}

	private int MeasureSuffix(Canvas g)
	{
		if (!SuffixWidth.HasValue)
		{
			return g.MeasureString(SuffixText, Font).Width;
		}
		return (int)((float?)SuffixWidth * Dpi).Value;
	}

	private void RectLR(Rectangle rect, int read_height, int sps, int sps2, int w_L, int h_L, int w_R, int h_R)
	{
		int num = -1;
		int num2 = (int)((float)read_height * icongap);
		bool[] array = RectLR(num2, sps, w_L, h_L, w_R, h_R, rect, out var hasx, out var hasr);
		int[] array2 = (HasLeft() ? UseLeft(new Rectangle(rect.X + hasx, rect.Y, rect.Width - hasr, rect.Height), CaretInfo.Height) : new int[2]);
		if (multiline)
		{
			rect_l = new Rectangle(rect.X + sps, rect.Y + sps + (read_height - h_L) / 2, w_L, h_L);
			if (array2[0] > 0 || array2[1] > 0)
			{
				int num3 = array2[0];
				int num4 = array2[1];
				rect_text = Helper.DeflateRectLR(rect.X + hasx + num3, rect.Y + sps + num4, rect.Width - hasr - num3, rect.Height - sps2 - num4, PaddingLR, Dpi);
				num = num4;
			}
			else
			{
				rect_text = Helper.DeflateRectLR(rect.X + hasx, rect.Y + sps, rect.Width - w_L - w_R - (sps + num2) * 2, rect.Height - sps2, PaddingLR, Dpi);
			}
			rect_r = new Rectangle(rect_text.Right + num2, rect.Y + sps + (read_height - h_R) / 2, w_R, h_R);
		}
		else
		{
			int num5 = rect.Y + (rect.Height - read_height) / 2;
			int num6 = (rect.Height - h_L) / 2;
			int num7 = (rect.Height - h_R) / 2;
			if (array2[0] > 0 || array2[1] > 0)
			{
				int num8 = array2[0];
				int num9 = array2[1];
				rect_text = Helper.DeflateRectLR(rect.X + hasx + num8, num5 + num9 / 2, rect.Width - hasr - num8, read_height - num9, PaddingLR, Dpi);
				num = num9;
			}
			else
			{
				rect_text = Helper.DeflateRectLR(rect.X + hasx, num5, rect.Width - hasr, read_height, PaddingLR, Dpi);
			}
			if (array[0])
			{
				rect_l = new Rectangle(rect.X + num6, rect.Y + num6, w_L, h_L);
			}
			else
			{
				rect_l = new Rectangle(rect.X + sps, rect.Y + num6, w_L, h_L);
			}
			if (array[1])
			{
				rect_r = new Rectangle(rect.Right - w_R - num7, rect.Y + num7, w_R, h_R);
			}
			else
			{
				rect_r = new Rectangle(rect_text.Right + num2, rect.Y + num7, w_R, h_R);
			}
		}
		if (num > -1)
		{
			UseLeftAutoHeight(read_height, num);
		}
	}

	private bool[] RectLR(int sp, int sps, int w_L, int h_L, int w_R, int h_R, Rectangle rect, out int hasx, out int hasr)
	{
		if (multiline)
		{
			hasx = sps + w_L + sp;
			hasr = w_L + w_R + (sps + sp) * 2;
			return new bool[2];
		}
		bool flag = round && w_L == h_L;
		bool flag2 = round && w_R == h_R;
		if (flag & flag2)
		{
			hasx = rect.Height - sp;
			hasr = (rect.Height - sp) * 2;
		}
		else if (flag)
		{
			hasx = rect.Height - sp;
			hasr = w_L + w_R + (sps + sp) * 2;
		}
		else if (flag2)
		{
			hasx = sps + w_L + sp;
			hasr = w_L + w_R + (sps + sp) * 2;
		}
		else
		{
			hasx = sps + w_L + sp;
			hasr = w_L + w_R + (sps + sp) * 2;
		}
		return new bool[2] { flag, flag2 };
	}

	private void RectL(Rectangle rect, int read_height, int sps, int sps2, int w)
	{
		int num = -1;
		int sp = (int)((float)read_height * icongap);
		bool flag = RectL(sp, sps, w, rect, can_round: false, out var hasx);
		int num2 = hasx + sps;
		int[] array = (HasLeft() ? UseLeft(new Rectangle(rect.X + hasx, rect.Y, rect.Width - hasx, rect.Height), CaretInfo.Height) : new int[2]);
		if (multiline)
		{
			int num3 = rect.Y + sps;
			int num4 = rect.Height - sps2;
			rect_l = new Rectangle(rect.X + sps, num3, w, read_height);
			if (array[0] > 0 || array[1] > 0)
			{
				int num5 = array[0];
				int num6 = array[1];
				rect_text = Helper.DeflateRectLR(rect.X + hasx + num5, num3 + num6, rect.Width - num2 - num5, num4 - num6, PaddingLR, Dpi);
				num = num6;
			}
			else
			{
				rect_text = Helper.DeflateRectLR(rect.X + hasx, num3, rect.Width - num2, num4, PaddingLR, Dpi);
			}
		}
		else
		{
			int num7 = (rect.Height - read_height) / 2;
			int num8 = rect.Y + num7;
			if (array[0] > 0 || array[1] > 0)
			{
				int num9 = array[0];
				int num10 = array[1];
				rect_text = Helper.DeflateRectLR(rect.X + hasx + num9, num8 + num10 / 2, rect.Width - num2 - num9, read_height - num10, PaddingLR, Dpi);
				num = num10;
			}
			else
			{
				rect_text = Helper.DeflateRectLR(rect.X + hasx, num8, rect.Width - num2, read_height, PaddingLR, Dpi);
			}
			if (flag)
			{
				rect_l = new Rectangle(rect.X + num7, num8, w, read_height);
			}
			else
			{
				rect_l = new Rectangle(rect.X + sps, num8, w, read_height);
			}
		}
		rect_r.Width = 0;
		if (num > -1)
		{
			UseLeftAutoHeight(read_height, num);
		}
	}

	private void RectL(Rectangle rect, int read_height, int sps, int sps2, int w, int h)
	{
		int num = -1;
		int sp = (int)((float)read_height * icongap);
		bool flag = RectL(sp, sps, w, rect, can_round: true, out var hasx);
		int num2 = hasx + sps;
		int[] array = (HasLeft() ? UseLeft(new Rectangle(rect.X + hasx, rect.Y, rect.Width - hasx, rect.Height), CaretInfo.Height) : new int[2]);
		if (multiline)
		{
			rect_l = new Rectangle(rect.X + sps, rect.Y + sps + (read_height - h) / 2, w, h);
			if (array[0] > 0 || array[1] > 0)
			{
				int num3 = array[0];
				int num4 = array[1];
				rect_text = Helper.DeflateRectLR(rect.X + hasx + num3, rect.Y + sps + num4, rect.Width - num2 - num3, rect.Height - sps2 - num4, PaddingLR, Dpi);
				num = num4;
			}
			else
			{
				rect_text = Helper.DeflateRectLR(rect.X + hasx, rect.Y + sps, rect.Width - num2, rect.Height - sps2, PaddingLR, Dpi);
			}
		}
		else
		{
			int num5 = rect.Y + (rect.Height - read_height) / 2;
			int num6 = (rect.Height - h) / 2;
			if (array[0] > 0 || array[1] > 0)
			{
				int num7 = array[0];
				int num8 = array[1];
				rect_text = Helper.DeflateRectLR(rect.X + hasx + num7, num5 + num8 / 2, rect.Width - num2 - num7, read_height - num8, PaddingLR, Dpi);
				num = num8;
			}
			else
			{
				rect_text = Helper.DeflateRectLR(rect.X + hasx, num5, rect.Width - num2, read_height, PaddingLR, Dpi);
			}
			if (flag)
			{
				rect_l = new Rectangle(rect.X + num6, rect.Y + num6, w, h);
			}
			else
			{
				rect_l = new Rectangle(rect.X + sps, rect.Y + num6, w, h);
			}
		}
		rect_r.Width = 0;
		if (num > -1)
		{
			UseLeftAutoHeight(read_height, num);
		}
	}

	private bool RectL(int sp, int sps, int w, Rectangle rect, bool can_round, out int hasx)
	{
		if (multiline)
		{
			hasx = sps + w + sp;
			return false;
		}
		if (round & can_round)
		{
			hasx = rect.Height - sp;
			return true;
		}
		hasx = sps + w + sp;
		return false;
	}

	private void RectR(Rectangle rect, int read_height, int sps, int sps2, int w)
	{
		int num = -1;
		int num2 = (int)((float)read_height * icongap);
		RectL(num2, sps, w, rect, can_round: false, out var hasx);
		int num3 = hasx + sps;
		if (HasLeft())
		{
			int[] array = UseLeft(new Rectangle(rect.X, rect.Y, rect.Width - num2, rect.Height), CaretInfo.Height);
			if (array[0] > 0 || array[1] > 0)
			{
				int num4 = array[0];
				int num5 = array[1];
				rect.X += num4;
				rect.Width -= num4;
				rect.Y += num5;
				rect.Height -= num5;
				num = num5;
			}
		}
		if (multiline)
		{
			rect_text = Helper.DeflateRectLR(rect.X + sps, rect.Y + sps, rect.Width - num3, rect.Height - sps2, PaddingLR, Dpi);
			rect_r = new Rectangle(rect_text.Right + num2, rect_text.Y, w, read_height);
		}
		else
		{
			int num6 = (rect.Height - read_height) / 2;
			rect_text = Helper.DeflateRectLR(rect.X + sps, rect.Y + (rect.Height - read_height) / 2, rect.Width - num3, read_height, PaddingLR, Dpi);
			rect_r = new Rectangle(rect_text.Right + num2, rect.Y + num6, w, read_height);
		}
		rect_l.Width = 0;
		if (num > -1)
		{
			UseLeftAutoHeight(read_height, num);
		}
	}

	private void RectR(Rectangle rect, int read_height, int sps, int sps2, int w, int h)
	{
		int num = -1;
		int num2 = (int)((float)read_height * icongap);
		bool flag = RectL(num2, sps, w, rect, can_round: false, out var hasx);
		int num3 = hasx + sps;
		if (HasLeft())
		{
			int[] array = UseLeft(new Rectangle(rect.X, rect.Y, rect.Width - num2, rect.Height), CaretInfo.Height);
			if (array[0] > 0 || array[1] > 0)
			{
				int num4 = array[0];
				int num5 = array[1];
				rect.X += num4;
				rect.Width -= num4;
				rect.Y += num5;
				rect.Height -= num5;
				num = num5;
			}
		}
		if (multiline)
		{
			rect_text = Helper.DeflateRectLR(rect.X + sps, rect.Y + sps, rect.Width - num3, rect.Height - sps2, PaddingLR, Dpi);
			rect_r = new Rectangle(rect_text.Right + num2, rect.Y + sps + (read_height - h) / 2, w, h);
		}
		else
		{
			int num6 = (rect.Height - h) / 2;
			rect_text = Helper.DeflateRectLR(rect.X + sps, rect.Y + (rect.Height - read_height) / 2, rect.Width - num3, read_height, PaddingLR, Dpi);
			if (flag)
			{
				rect_r = new Rectangle(rect.Right - w - num6, rect.Y + num6, w, h);
			}
			else
			{
				rect_r = new Rectangle(rect_text.Right + num2, rect.Y + num6, w, h);
			}
		}
		rect_l.Width = 0;
		if (num > -1)
		{
			UseLeftAutoHeight(read_height, num);
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		CalculateRect();
		base.OnSizeChanged(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();
		Select();
		is_prefix_down = (is_suffix_down = false);
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		if (cache_font != null && e.Clicks > 1 && !BanInput)
		{
			mDownMove = (mDown = false);
			if (IsPassWord)
			{
				SelectAll();
				return;
			}
			CacheCaret caretPostion = GetCaretPostion(e.X + ScrollX.Value, e.Y + ScrollY.Value);
			if (caretPostion != null)
			{
				int num = 0;
				if (caretPostion.i > 0)
				{
					num = FindStart(cache_font, caretPostion.i - 2);
				}
				int num2 = ((caretPostion.i < cache_font.Count) ? FindEnd(cache_font, caretPostion.i) : cache_font.Count);
				bool flag = SetSelectionStart(num);
				bool flag2 = SetSelectionLength(num2 - num);
				if (flag | flag2)
				{
					Invalidate();
				}
			}
		}
		else if (is_clear && rect_r.Contains(e.X, e.Y))
		{
			is_clear_down = true;
		}
		else if ((HasPrefix || PrefixText != null) && rect_l.Contains(e.X, e.Y) && PrefixClick != null)
		{
			is_prefix_down = true;
		}
		else if ((HasSuffix || SuffixText != null) && rect_r.Contains(e.X, e.Y) && SuffixClick != null)
		{
			is_suffix_down = true;
		}
		else
		{
			if (IMouseDown(e.X, e.Y))
			{
				return;
			}
			if (autoscroll)
			{
				if (ScrollY.Show && ScrollY.Hover)
				{
					float num3 = ((float)(e.Y - ScrollY.Rect.Top) - ScrollY.SliderFull / 2f) / ((float)ScrollY.Rect.Height - ScrollY.SliderFull);
					ScrollY.Down = true;
					ScrollY.Value = (int)(num3 * (float)ScrollY.Max);
					SetCursor(val: false);
					Window.CanHandMessage = false;
					return;
				}
				if (ScrollX.Show && ScrollX.Hover)
				{
					float num4 = ((float)(e.X - ScrollX.Rect.Left) - ScrollX.SliderFull / 2f) / ((float)ScrollX.Rect.Width - ScrollX.SliderFull);
					ScrollX.Down = true;
					ScrollX.Value = (int)(num4 * (float)ScrollX.Max);
					SetCursor(val: false);
					Window.CanHandMessage = false;
					return;
				}
			}
			mDownMove = false;
			mDownLocation = e.Location;
			if (BanInput)
			{
				return;
			}
			CacheCaret caretPostion2 = GetCaretPostion(e.X + ScrollX.Value, e.Y + ScrollY.Value);
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			if (caretPostion2 == null)
			{
				flag3 = SetSelectionStart(0);
				flag4 = SetSelectionLength(0);
				flag5 = SetCaretPostion(selectionStart);
			}
			else
			{
				if (Control.ModifierKeys.HasFlag(Keys.Shift))
				{
					if (caretPostion2.i > selectionStartTemp)
					{
						if (selectionStart != selectionStartTemp)
						{
							flag3 = SetSelectionStart(selectionStartTemp);
						}
						flag4 = SetSelectionLength(caretPostion2.i - selectionStartTemp);
					}
					else
					{
						int num5 = selectionStartTemp - caretPostion2.i;
						flag3 = SetSelectionStart(caretPostion2.i, caret: false);
						flag4 = SetSelectionLength(num5);
					}
				}
				else
				{
					flag3 = SetSelectionStart(caretPostion2.i);
					flag4 = SetSelectionLength(0);
				}
				flag5 = SetCaretPostion(caretPostion2.index);
			}
			if (cache_font != null)
			{
				mDown = true;
			}
			else if (ModeRange && SetCaretPostion())
			{
				flag5 = true;
			}
			CaretInfo.flag = true;
			if (flag3 | flag4 | flag5)
			{
				Invalidate();
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (ScrollY.Down)
		{
			float num = ((float)(e.Y - ScrollY.Rect.Top) - ScrollY.SliderFull / 2f) / ((float)ScrollY.Rect.Height - ScrollY.SliderFull);
			ScrollY.Value = (int)(num * (float)ScrollY.Max);
			Window.CanHandMessage = false;
			return;
		}
		if (ScrollX.Down)
		{
			float num2 = ((float)(e.X - ScrollX.Rect.Left) - ScrollX.SliderFull / 2f) / ((float)ScrollX.Rect.Width - ScrollX.SliderFull);
			ScrollX.Value = (int)(num2 * (float)ScrollX.Max);
			Window.CanHandMessage = false;
			return;
		}
		if (mDown && cache_font != null)
		{
			mDownMove = true;
			SetCursor(CursorType.IBeam);
			CacheCaret caretPostion = GetCaretPostion(mDownLocation.X + ScrollX.Value + (e.X - mDownLocation.X), mDownLocation.Y + ScrollY.Value + (e.Y - mDownLocation.Y));
			if (caretPostion == null)
			{
				Window.CanHandMessage = false;
				return;
			}
			bool flag = SetSelectionLength(Math.Abs(caretPostion.i - selectionStart));
			if (caretPostion.i > selectionStart)
			{
				selectionStartTemp = selectionStart;
			}
			else
			{
				selectionStartTemp = caretPostion.i;
			}
			bool flag2 = SetCaretPostion(caretPostion.index);
			if (flag | flag2)
			{
				Invalidate();
			}
			Window.CanHandMessage = false;
			return;
		}
		bool flag3 = true;
		if (autoscroll)
		{
			if (ScrollY.Show)
			{
				ScrollY.Hover = ScrollY.Rect.Contains(e.X, e.Y);
				if (ScrollY.Hover)
				{
					flag3 = false;
				}
			}
			if (ScrollX.Show)
			{
				ScrollX.Hover = ScrollX.Rect.Contains(e.X, e.Y);
				if (ScrollX.Hover)
				{
					flag3 = false;
				}
			}
		}
		if (is_clear)
		{
			bool flag4 = rect_r.Contains(e.X, e.Y);
			if (hover_clear != flag4)
			{
				hover_clear = flag4;
				Invalidate();
			}
			if (flag4)
			{
				SetCursor(val: true);
				return;
			}
		}
		if (((HasPrefix || PrefixText != null) && rect_l.Contains(e.X, e.Y) && PrefixClick != null) || ((HasSuffix || SuffixText != null) && rect_r.Contains(e.X, e.Y) && SuffixClick != null))
		{
			SetCursor(val: true);
		}
		else if (IMouseMove(e.X, e.Y))
		{
			SetCursor(val: true);
		}
		else if (CaretInfo.ReadShow)
		{
			if (rect_text.Contains(e.X, e.Y))
			{
				SetCursor(val: true);
			}
			else
			{
				SetCursor(val: false);
			}
		}
		else if (flag3 && rect_text.Contains(e.X, e.Y))
		{
			SetCursor(CursorType.IBeam);
		}
		else
		{
			SetCursor(val: false);
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (MouseWheelCore(e.Delta) && e is HandledMouseEventArgs e2)
		{
			e2.Handled = true;
		}
		base.OnMouseWheel(e);
	}

	private bool MouseWheelCore(int delta)
	{
		if (ScrollY.Show && autoscroll)
		{
			if (delta == 0)
			{
				return false;
			}
			int value = ScrollY.Value;
			ScrollY.Value -= delta;
			if (value == ScrollY.Value)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		bool flag = mDown;
		mDown = false;
		Window.CanHandMessage = true;
		ScrollInfo scrollY = ScrollY;
		bool down = (ScrollX.Down = false);
		scrollY.Down = down;
		if (is_clear_down)
		{
			if (rect_r.Contains(e.X, e.Y))
			{
				OnClearValue();
				OnClearClick(e);
			}
			is_clear_down = false;
		}
		else if (is_prefix_down && PrefixClick != null)
		{
			PrefixClick(this, e);
			is_prefix_down = (is_suffix_down = false);
		}
		else if (is_suffix_down && SuffixClick != null)
		{
			SuffixClick(this, e);
			is_prefix_down = (is_suffix_down = false);
		}
		else
		{
			if (IMouseUp(e.X, e.Y))
			{
				return;
			}
			if (flag && mDownMove && mDownLocation != e.Location && cache_font != null)
			{
				CacheCaret caretPostion = GetCaretPostion(e.X + ScrollX.Value, e.Y + ScrollY.Value);
				if (caretPostion == null)
				{
					return;
				}
				if (selectionStart == caretPostion.i)
				{
					SelectionLength = 0;
					return;
				}
				if (caretPostion.i > selectionStart)
				{
					bool flag3 = SetSelectionLength(Math.Abs(caretPostion.i - selectionStart));
					bool flag4 = SetCaretPostion(caretPostion.index);
					if (flag3 | flag4)
					{
						Invalidate();
					}
					return;
				}
				bool flag5 = SetSelectionLength(Math.Abs(caretPostion.i - selectionStart));
				bool flag6 = SetSelectionStart(caretPostion.i, caret: false);
				bool flag7 = SetCaretPostion(caretPostion.index);
				bool flag8 = ScrollX.SetValue(ScrollX.Value);
				if (flag6 | flag5 | flag7 | flag8)
				{
					Invalidate();
				}
			}
			else
			{
				OnClickContent(e);
			}
		}
	}

	/// <summary>
	/// 查找前面
	/// </summary>
	private int FindStart(List<CacheFont> cache_font, int index)
	{
		for (int num = index; num >= 0; num--)
		{
			if (sptext.Contains(cache_font[num].text))
			{
				return num + 1;
			}
		}
		return 0;
	}

	/// <summary>
	/// 查找后面
	/// </summary>
	private int FindEnd(List<CacheFont> cache_font, int index)
	{
		int count = cache_font.Count;
		for (int i = index; i < count; i++)
		{
			if (sptext.Contains(cache_font[i].text))
			{
				return i;
			}
		}
		return count;
	}

	/// <summary>
	/// 查找行前面
	/// </summary>
	private int FindStartY(List<CacheFont> cache_font, int index)
	{
		int line = cache_font[index].line;
		int result = 0;
		int num = index;
		while (num >= 0)
		{
			if (cache_font[num].line == line)
			{
				result = num;
				num--;
				continue;
			}
			return result;
		}
		return 0;
	}

	/// <summary>
	/// 查找行后面
	/// </summary>
	private int FindEndY(List<CacheFont> cache_font, int index)
	{
		int line = cache_font[index].line;
		int num = 0;
		int count = cache_font.Count;
		for (int i = index + 1; i < count; i++)
		{
			if (cache_font[i].line == line)
			{
				num = i;
				continue;
			}
			if (num != 0)
			{
				return num;
			}
			return index;
		}
		return count;
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		ExtraMouseHover = true;
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		ExtraMouseHover = false;
		base.OnMouseLeave(e);
	}

	protected virtual void OnClearClick(MouseEventArgs e)
	{
		ClearClick?.Invoke(this, e);
	}

	public new virtual void Invalidate()
	{
		base.Invalidate();
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		Canvas canvas = e.Canvas;
		Rectangle rect = e.Rect.PaddingRect(Padding);
		Rectangle rect_read = rect.ReadRect(((float)WaveSize + borderWidth / 2f) * Dpi, joinMode, JoinLeft, JoinRight);
		IPaint(canvas, rect, rect_read);
		base.OnDraw(e);
	}

	internal void IPaint(Canvas g, Rectangle rect, Rectangle rect_read)
	{
		float num = (round ? ((float)rect_read.Height) : ((float)radius * Dpi));
		if (backImage != null)
		{
			g.Image(rect_read, backImage, backFit, num, round: false);
		}
		using GraphicsPath path = Helper.PathJoin(rect_read, num, joinMode, JoinLeft, JoinRight);
		Color def = back ?? Colour.BgContainer.Get(ColorScheme, "Input", Name);
		Color color = fore ?? Colour.Text.Get(ColorScheme, "Input", Name);
		Color color2 = borderColor ?? Colour.BorderColor.Get(ColorScheme, "Input", Name);
		Color color3 = BorderHover ?? Colour.PrimaryHover.Get(ColorScheme, "Input", Name);
		Color color4 = BorderActive ?? Colour.Primary.Get(ColorScheme, "Input", Name);
		switch (status)
		{
		case TType.Success:
			color2 = Colour.SuccessBorder.Get(ColorScheme, "Input", Name);
			color3 = Colour.SuccessHover.Get(ColorScheme, "Input", Name);
			color4 = Colour.Success.Get(ColorScheme, "Input", Name);
			break;
		case TType.Error:
			color2 = Colour.ErrorBorder.Get(ColorScheme, "Input", Name);
			color3 = Colour.ErrorHover.Get(ColorScheme, "Input", Name);
			color4 = Colour.Error.Get(ColorScheme, "Input", Name);
			break;
		case TType.Warn:
			color2 = Colour.WarningBorder.Get(ColorScheme, "Input", Name);
			color3 = Colour.WarningHover.Get(ColorScheme, "Input", Name);
			color4 = Colour.Warning.Get(ColorScheme, "Input", Name);
			break;
		}
		PaintClick(g, path, rect, color4, num);
		if (Enabled)
		{
			using (Brush brush = backExtend.BrushEx(rect_read, def))
			{
				g.Fill(brush, path);
			}
			PaintLoadingWave(g, path, rect_read);
			PaintIcon(g, color);
			PaintText(g, color, rect_read.Right, rect_read.Bottom);
			PaintOtherBor(g, rect_read, num, def, color2, color4);
			PaintScroll(g, rect_read, num);
			if (!(borderWidth > 0f))
			{
				return;
			}
			float num2 = borderWidth * Dpi;
			if (Variant == TVariant.Underlined)
			{
				if (AnimationHover)
				{
					g.DrawLine(color2.BlendColors(AnimationHoverValue, color3), num2, rect_read.X, rect_read.Bottom, rect_read.Right, rect_read.Bottom);
				}
				else if (ExtraMouseDown)
				{
					g.DrawLine(color4, num2, rect_read.X, rect_read.Bottom, rect_read.Right, rect_read.Bottom);
				}
				else if (ExtraMouseHover)
				{
					g.DrawLine(color3, num2, rect_read.X, rect_read.Bottom, rect_read.Right, rect_read.Bottom);
				}
				else if (AnimationBlinkState && colorBlink.HasValue)
				{
					g.DrawLine(colorBlink.Value, num2, rect_read.X, rect_read.Bottom, rect_read.Right, rect_read.Bottom);
				}
				else
				{
					g.DrawLine(color2, num2, rect_read.X, rect_read.Bottom, rect_read.Right, rect_read.Bottom);
				}
			}
			else if (AnimationHover)
			{
				g.Draw(color2.BlendColors(AnimationHoverValue, color3), num2, path);
			}
			else if (ExtraMouseDown)
			{
				g.Draw(color4, num2, path);
			}
			else if (ExtraMouseHover)
			{
				g.Draw(color3, num2, path);
			}
			else if (AnimationBlinkState && colorBlink.HasValue)
			{
				g.Draw(colorBlink.Value, num2, path);
			}
			else
			{
				g.Draw(color2, num2, path);
			}
		}
		else
		{
			g.Fill(Colour.FillTertiary.GetSymbol(ColorScheme, "bgDisabled", "Input", Name), path);
			PaintLoadingWave(g, path, rect_read);
			Color symbol = Colour.TextQuaternary.GetSymbol(ColorScheme, "foreDisabled", "Input", Name);
			PaintIcon(g, symbol);
			PaintText(g, symbol, rect_read.Right, rect_read.Bottom);
			PaintOtherBor(g, rect_read, num, def, color2, color4);
			PaintScroll(g, rect_read, num);
			if (borderWidth > 0f)
			{
				g.Draw(color2, borderWidth * Dpi, path);
			}
		}
	}

	private void PaintLoadingWave(Canvas g, GraphicsPath path, Rectangle rect)
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

	private void PaintScroll(Canvas g, Rectangle rect_read, float _radius)
	{
		if (!autoscroll || (!ScrollY.Show && !ScrollX.Show))
		{
			return;
		}
		int num = (int)(16f * Dpi);
		int num2 = (int)(6f * Dpi);
		int num3 = (int)((float)(Config.ScrollMinSizeY ?? SystemInformation.VerticalScrollBarThumbHeight) * Dpi);
		int num4 = (int)((float)(Config.ScrollMinSizeX ?? SystemInformation.HorizontalScrollBarThumbWidth) * Dpi);
		Color baseColor = Colour.TextBase.Get(ColorScheme, "Input", Name);
		if (ScrollX.Show)
		{
			if (ScrollY.Show)
			{
				ScrollX.Rect = new Rectangle(rect_read.X, rect_read.Bottom - num, rect_read.Width - num, num);
				if (IsPaintScroll())
				{
					Color brush = Color.FromArgb(10, baseColor);
					if (joinMode == TJoinMode.Right || joinMode == TJoinMode.Top || JoinLeft)
					{
						g.Fill(brush, ScrollX.Rect);
					}
					else
					{
						using GraphicsPath path = ScrollX.Rect.RoundPath(_radius, roundTL: false, roundTR: false, roundBR: false, roundBL: true);
						g.Fill(brush, path);
					}
				}
			}
			else
			{
				ScrollX.Rect = new Rectangle(rect_read.X, rect_read.Bottom - num, rect_read.Width, num);
				if (IsPaintScroll())
				{
					Color brush2 = Color.FromArgb(10, baseColor);
					g.Fill(brush2, ScrollX.Rect);
				}
			}
			float num5 = ScrollX.Value;
			float num6 = ScrollX.Max + ScrollX.Rect.Width;
			float num7 = (float)(ScrollX.Rect.Height - num2) / 2f;
			float num8 = num7 * 2f;
			float num9 = (float)num4 + num8;
			float num10 = (float)ScrollX.Rect.Width / num6 * (float)ScrollX.Rect.Width;
			float num11 = num10 - (float)num;
			if (num11 < num9)
			{
				num11 = num9;
			}
			else if (num11 < (float)num)
			{
				num11 = num;
			}
			float num12 = ((num5 == 0f) ? 0f : (num5 / (num6 - (float)ScrollX.Rect.Width) * ((float)ScrollX.Rect.Width - num11)));
			ScrollX.Slider = new RectangleF((float)ScrollX.Rect.X + num12 + num7, (float)ScrollX.Rect.Y + num7, num11 - num8, num2);
			if (num10 < (float)num4)
			{
				num10 = num4;
			}
			else if (num10 < (float)num)
			{
				num10 = num;
			}
			ScrollX.SliderFull = num10;
			int alpha = (ScrollX.Hover ? 141 : 110);
			using GraphicsPath path2 = ScrollX.Slider.RoundPath(ScrollX.Slider.Width);
			g.Fill(Color.FromArgb(alpha, baseColor), path2);
		}
		if (!ScrollY.Show)
		{
			return;
		}
		ScrollY.Rect = new Rectangle(rect_read.Right - num, rect_read.Y, num, rect_read.Height);
		if (IsPaintScroll())
		{
			Color brush3 = Color.FromArgb(10, baseColor);
			if (joinMode == TJoinMode.Left || JoinRight)
			{
				g.Fill(brush3, ScrollY.Rect);
			}
			else if (joinMode == TJoinMode.Top)
			{
				using GraphicsPath path3 = ScrollY.Rect.RoundPath(_radius, roundTL: false, roundTR: true, roundBR: false, roundBL: false);
				g.Fill(brush3, path3);
			}
			else if (joinMode == TJoinMode.Bottom)
			{
				using GraphicsPath path4 = ScrollY.Rect.RoundPath(_radius, roundTL: false, roundTR: false, roundBR: true, roundBL: false);
				g.Fill(brush3, path4);
			}
			else
			{
				using GraphicsPath path5 = ScrollY.Rect.RoundPath(_radius, roundTL: false, roundTR: true, roundBR: true, roundBL: false);
				g.Fill(brush3, path5);
			}
		}
		float num13 = ScrollY.Value;
		float num14 = ScrollY.Max + ScrollY.Rect.Height;
		float num15 = (float)(ScrollY.Rect.Width - num2) / 2f;
		float num16 = num15 * 2f;
		float num17 = (float)num3 + num16;
		float num18 = (float)ScrollY.Rect.Height / num14 * (float)ScrollY.Rect.Height;
		float num19 = num18 - (float)num;
		if (num19 < num17)
		{
			num19 = num17;
		}
		else if (num19 < (float)num)
		{
			num19 = num;
		}
		float num20 = ((num13 == 0f) ? 0f : (num13 / (num14 - (float)ScrollY.Rect.Height) * ((float)ScrollY.Rect.Height - num19)));
		ScrollY.Slider = new RectangleF((float)ScrollY.Rect.X + num15, (float)ScrollY.Rect.Y + num20 + num15, num2, num19 - num16);
		if (num18 < (float)num3)
		{
			num18 = num3;
		}
		else if (num18 < (float)num)
		{
			num18 = num;
		}
		ScrollY.SliderFull = num18;
		int alpha2 = (ScrollY.Hover ? 141 : 110);
		using GraphicsPath path6 = ScrollY.Slider.RoundPath(ScrollY.Slider.Width);
		g.Fill(Color.FromArgb(alpha2, baseColor), path6);
	}

	private bool IsPaintScroll()
	{
		if (Config.ScrollBarHide)
		{
			if (ScrollY.Show && ScrollX.Show)
			{
				if (!ScrollY.Hover)
				{
					return ScrollX.Hover;
				}
				return true;
			}
			return ScrollY.Hover;
		}
		return true;
	}

	private void PaintIcon(Canvas g, Color _fore)
	{
		string text = PrefixText;
		string text2 = SuffixText;
		if (text != null)
		{
			g.String(text, Font, prefixFore ?? _fore, rect_l, PrefixFormat);
		}
		else if (prefixSvg != null)
		{
			g.Svg(prefixSvg, rect_l, prefixFore ?? fore ?? Colour.Text.Get(ColorScheme, "Input", Name));
		}
		else if (prefix != null)
		{
			g.Image(prefix, rect_l);
		}
		if (loading && LoadingValue > -1f)
		{
			g.PaintLoading(LoadingIcon, LoadingSvg, rect_r, AnimationLoadingValue, LoadingValue, Colour.TextQuaternary.Get(ColorScheme, "Input", Name), CaretInfo.Height, 0.06f);
		}
		else if (is_clear)
		{
			g.Svg(SvgDb.IcoError, rect_r, hover_clear ? Colour.TextTertiary.Get(ColorScheme, "Input", Name) : Colour.TextQuaternary.Get(ColorScheme, "Input", Name));
		}
		else if (text2 != null)
		{
			g.String(text2, Font, suffixFore ?? _fore, rect_r, SuffixFormat);
		}
		else if (suffixSvg != null)
		{
			g.Svg(suffixSvg, rect_r, suffixFore ?? fore ?? Colour.Text.Get(ColorScheme, "Input", Name));
		}
		else if (suffix != null)
		{
			g.Image(suffix, rect_r);
		}
		else
		{
			PaintRIcon(g, rect_r);
		}
	}

	private void PaintText(Canvas g, Color _fore, int w, int h)
	{
		GraphicsState graphicsState = g.Save();
		if (multiline)
		{
			g.SetClip(rect_text);
		}
		else if (RECTDIV.HasValue)
		{
			g.SetClip(RECTDIV.Value);
		}
		else
		{
			g.SetClip(new Rectangle(rect_text.X, 0, rect_text.Width, Height));
		}
		if (cache_font != null)
		{
			PaintText(g, _fore, w, h, cache_font);
		}
		else if (ShowPlaceholder && PlaceholderText != null)
		{
			using Brush brush = placeholderColorExtend.BrushEx(rect_text, placeholderColor ?? Colour.TextQuaternary.Get(ColorScheme, "Input", Name));
			g.DrawText(PlaceholderText, Font, brush, rect_text, IsRTL ? (sf_placeholder | FormatFlags.DirectionRightToLeft) : sf_placeholder);
		}
		if (CaretInfo.Show && CaretInfo.Flag)
		{
			g.ResetClip();
			if (multiline)
			{
				g.SetClip(new Rectangle(0, rect_text.Y, w, rect_text.Height));
			}
			else if (RECTDIV.HasValue)
			{
				g.SetClip(RECTDIV.Value);
			}
			else
			{
				g.SetClip(new Rectangle(0, 0, w, Height));
			}
			g.TranslateTransform(-ScrollX.Value, -ScrollY.Value);
			using SolidBrush brush2 = new SolidBrush(CaretColor ?? _fore);
			g.Fill(brush2, CaretInfo.Rect);
		}
		g.Restore(graphicsState);
	}

	private void PaintText(Canvas g, Color _fore, int w, int h, List<CacheFont> cache_font)
	{
		GraphicsState graphicsState = g.Save();
		g.TranslateTransform(-ScrollX.Value, -ScrollY.Value);
		List<CacheFont> list = PCSText(g, _fore, w, h, cache_font);
		if (styles != null)
		{
			foreach (CacheFont item in list)
			{
				if (item.back.HasValue)
				{
					g.Fill(item.back.Value, item.rect);
				}
			}
		}
		PaintTextSelected(g, cache_font);
		PaintText(g, _fore, cache_font, list);
		g.Restore(graphicsState);
	}

	private List<CacheFont> PCSText(Canvas g, Color _fore, int w, int h, List<CacheFont> cache_font)
	{
		List<CacheFont> list = new List<CacheFont>(cache_font.Count);
		if (IsPassWord)
		{
			if (ScrollY.Show)
			{
				foreach (CacheFont item in cache_font)
				{
					if (!item.ret && item.rect.Y > ScrollY.Value - item.rect.Height && item.rect.Bottom < ScrollY.Value + h + item.rect.Height)
					{
						list.Add(item);
					}
				}
			}
			else if (ScrollX.Show)
			{
				foreach (CacheFont item2 in cache_font)
				{
					if (!item2.ret && item2.rect.X > ScrollX.Value - item2.rect.Width && item2.rect.Right < ScrollX.Value + w + item2.rect.Width)
					{
						list.Add(item2);
					}
				}
			}
			else
			{
				foreach (CacheFont item3 in cache_font)
				{
					if (!item3.ret)
					{
						list.Add(item3);
					}
				}
			}
		}
		else if (ScrollY.Show)
		{
			foreach (CacheFont item4 in cache_font)
			{
				if (!item4.hide && item4.rect.Y > ScrollY.Value - item4.rect.Height && item4.rect.Bottom < ScrollY.Value + h + item4.rect.Height)
				{
					list.Add(item4);
				}
			}
		}
		else if (ScrollX.Show)
		{
			foreach (CacheFont item5 in cache_font)
			{
				if (!item5.hide && item5.rect.X > ScrollX.Value - item5.rect.Width && item5.rect.Right < ScrollX.Value + w + item5.rect.Width)
				{
					list.Add(item5);
				}
			}
		}
		else
		{
			foreach (CacheFont item6 in cache_font)
			{
				if (!item6.hide)
				{
					list.Add(item6);
				}
			}
		}
		return list;
	}

	private void PaintText(Canvas g, Color _fore, List<CacheFont> cache_font, List<CacheFont> tmp)
	{
		using SolidBrush solidBrush = new SolidBrush(_fore);
		if (IsPassWord)
		{
			foreach (CacheFont item in tmp)
			{
				g.String(PassWordChar, item.font ?? Font, solidBrush, item.rect, sf_center);
			}
			return;
		}
		if (IsRTL)
		{
			PaintTextRTL(g, tmp, solidBrush, _fore);
			return;
		}
		if (HasEmoji)
		{
			using (Font font = new Font(EmojiFont ?? Config.EmojiFont, Font.Size))
			{
				foreach (CacheFont item2 in tmp)
				{
					if (item2.emoji)
					{
						if (SvgDb.Emoji.TryGetValue(item2.text, out string value))
						{
							g.Svg(value, item2.rect, solidBrush.Color);
						}
						else
						{
							StringEmoji(g, item2.text, font, item2, solidBrush);
						}
					}
					else
					{
						String(g, Font, item2, solidBrush);
					}
				}
				return;
			}
		}
		foreach (CacheFont item3 in tmp)
		{
			String(g, Font, item3, solidBrush);
		}
	}

	/// <summary>
	/// RTL 渲染：将连续同样式、非 Emoji 的 RTL（及两侧被 RTL 夹住的中性）字符簇聚合为整段文本一次性绘制，由 GDI+ 完成双向排版与连写整形
	/// </summary>
	private void PaintTextRTL(Canvas g, List<CacheFont> tmp, Brush fore, Color fore_color)
	{
		Font font = null;
		try
		{
			int num = 0;
			while (num < tmp.Count)
			{
				CacheFont cacheFont = tmp[num];
				if (cacheFont.emoji)
				{
					if (font == null)
					{
						font = new Font(EmojiFont ?? Config.EmojiFont, Font.Size);
					}
					if (SvgDb.Emoji.TryGetValue(cacheFont.text, out string value))
					{
						g.Svg(value, cacheFont.rect, fore_color);
					}
					else
					{
						StringEmoji(g, cacheFont.text, font, cacheFont, fore);
					}
					num++;
					continue;
				}
				if (cacheFont.ret || cacheFont.dir != TextDirection.RTL)
				{
					String(g, Font, cacheFont, fore);
					num++;
					continue;
				}
				int num2 = num;
				for (int i = num + 1; i < tmp.Count; i++)
				{
					CacheFont cacheFont2 = tmp[i];
					if (cacheFont2.emoji || cacheFont2.ret || cacheFont2.dir == TextDirection.LTR || cacheFont2.rect.Y != cacheFont.rect.Y || cacheFont2.font != cacheFont.font)
					{
						break;
					}
					Color? color = cacheFont2.fore;
					Color? color2 = cacheFont.fore;
					if (color.HasValue != color2.HasValue || (color.HasValue && color.GetValueOrDefault() != color2.GetValueOrDefault()) || cacheFont2.back != cacheFont.back)
					{
						break;
					}
					num2 = i;
				}
				while (num2 > num && tmp[num2].dir == TextDirection.Neutral)
				{
					num2--;
				}
				if (num2 == num)
				{
					String(g, Font, cacheFont, fore);
					num++;
					continue;
				}
				StringBuilder stringBuilder = new StringBuilder();
				int num3 = int.MaxValue;
				int num4 = int.MinValue;
				for (int j = num; j <= num2; j++)
				{
					CacheFont cacheFont3 = tmp[j];
					stringBuilder.Append(cacheFont3.text);
					if (cacheFont3.rect.X < num3)
					{
						num3 = cacheFont3.rect.X;
					}
					if (cacheFont3.rect.Right > num4)
					{
						num4 = cacheFont3.rect.Right;
					}
				}
				g.String(stringBuilder.ToString(), cacheFont.font ?? Font, cacheFont.fore, fore, Rectangle.FromLTRB(num3, cacheFont.rect.Y, num4, cacheFont.rect.Bottom), sf_center_rtl);
				num = num2 + 1;
			}
		}
		finally
		{
			font?.Dispose();
		}
	}

	private void PaintTextSelected(Canvas g, List<CacheFont> cache_font)
	{
		if (selectionLength <= 0 || cache_font.Count <= selectionStartTemp || BanInput)
		{
			return;
		}
		try
		{
			int num = selectionStartTemp;
			int num2 = num + selectionLength - 1;
			if (num2 > cache_font.Count - 1)
			{
				num2 = cache_font.Count - 1;
			}
			_ = cache_font[num];
			using SolidBrush brush = new SolidBrush(selection);
			Dictionary<int, CacheFont> dictionary = new Dictionary<int, CacheFont>(6);
			for (int i = num; i <= num2; i++)
			{
				CacheFont cacheFont = cache_font[i];
				if (cacheFont.ret)
				{
					dictionary.Add(cacheFont.line + 1, cacheFont);
					continue;
				}
				if (dictionary.ContainsKey(cacheFont.line))
				{
					dictionary.Remove(cacheFont.line);
				}
				g.Fill(brush, cacheFont.rect);
			}
			foreach (KeyValuePair<int, CacheFont> item in dictionary)
			{
				g.Fill(brush, item.Value.rect.X, item.Value.rect.Y, item.Value.width, item.Value.rect.Height);
			}
		}
		catch
		{
		}
	}

	private void String(Canvas g, Font font, CacheFont cache, Brush brush)
	{
		g.String(cache.text, cache.font ?? font, cache.fore, brush, cache.rect, sf_center);
	}

	private void StringEmoji(Canvas g, string? text, Font font, CacheFont cache, Brush brush)
	{
		g.String(text, cache.font ?? font, cache.fore, brush, cache.rect, sf_center);
	}

	protected virtual void PaintRIcon(Canvas g, Rectangle rect)
	{
	}

	protected virtual void PaintOtherBor(Canvas g, Rectangle rect_read, float radius, Color back, Color borderColor, Color borderActive)
	{
	}

	internal void PaintClick(Canvas g, GraphicsPath path, Rectangle rect, Color color, float radius)
	{
		if (AnimationFocus)
		{
			if (AnimationFocusValue > 0)
			{
				using (GraphicsPath graphicsPath = Helper.PathJoin(rect, radius, joinMode, JoinLeft, JoinRight))
				{
					graphicsPath.AddPath(path, connect: false);
					g.Fill(Helper.ToColor(AnimationFocusValue, color), graphicsPath);
				}
			}
		}
		else if (ExtraMouseDown && WaveSize > 0)
		{
			using (GraphicsPath graphicsPath2 = Helper.PathJoin(rect, radius, joinMode, JoinLeft, JoinRight))
			{
				graphicsPath2.AddPath(path, connect: false);
				g.Fill(Color.FromArgb(30, color), graphicsPath2);
			}
		}
	}

	private bool ScrollIFTo(Rectangle r, bool rd = true)
	{
		if (ScrollY.Show)
		{
			if (ScrollIFToY(r, rd) || (ScrollX.Show && ScrollIFToX(r, rd)))
			{
				return true;
			}
			return false;
		}
		if (ScrollX.Show)
		{
			return ScrollIFToX(r, rd);
		}
		oldtmpY = (oldtmpX = null);
		if ((ScrollX.SetValue(0) || ScrollY.SetValue(0)) && rd)
		{
			Invalidate();
		}
		return false;
	}

	private bool ScrollIFToY(Rectangle r, bool rd = true)
	{
		string text = r.Y.ToString() + lineheight;
		if (text == oldtmpY)
		{
			return false;
		}
		oldtmpY = text;
		int num = CaretInfo.Y - ScrollY.Value;
		int num2 = (int)((float)rect_text.Height * 0.3f);
		if (num < rect_text.Y - num2 || num + CaretInfo.Height > rect_text.Bottom + num2)
		{
			if (num < rect_text.Y)
			{
				if (ScrollY.SetValue(r.Y - rect_text.Y) & rd)
				{
					Invalidate();
				}
			}
			else if (num + CaretInfo.Height > rect_text.Bottom && (ScrollY.SetValue(r.Bottom - rect_text.Height + CaretInfo.Height) & rd))
			{
				Invalidate();
			}
			return false;
		}
		ITask.Run(() =>
		{
			ScrollToY(r);
		});
		return true;
	}

	private bool ScrollIFToX(Rectangle r, bool rd = true)
	{
		string text = r.X + cache_font?.Count.ToString();
		if (text == oldtmpX)
		{
			return false;
		}
		oldtmpX = text;
		int num = CaretInfo.X - ScrollX.Value;
		int num2 = (int)((float)rect_text.Width * 0.3f);
		if (num < rect_text.X - num2 || num + CaretInfo.Width > rect_text.Right + num2)
		{
			if (num < rect_text.X)
			{
				if (ScrollX.SetValue(r.X - rect_text.X) & rd)
				{
					Invalidate();
				}
			}
			else if (num + CaretInfo.Width > rect_text.Right && (ScrollX.SetValue(r.Right - rect_text.Width + CaretInfo.Width) & rd))
			{
				Invalidate();
			}
			return false;
		}
		ITask.Run(() =>
		{
			ScrollToX(r);
		});
		return true;
	}

	private void ScrollToY(Rectangle r)
	{
		int stepSize = CaretInfo.Height;
		int count = 0;
		int num = CaretInfo.Y - ScrollY.Value;
		if (num < rect_text.Y)
		{
			ScrollToYUP(r, stepSize, ref count);
		}
		else if (num + CaretInfo.Height > rect_text.Bottom)
		{
			ScrollToYDOWN(r, stepSize, ref count);
		}
	}

	private void ScrollToYUP(Rectangle r, int stepSize, ref int count)
	{
		while (CaretInfo.Y - ScrollY.Value < rect_text.Y)
		{
			int num = ScrollY.Value - stepSize;
			bool flag = ScrollY.SetValue(num);
			if (ScrollY.Value == num)
			{
				count++;
				if (flag)
				{
					Invalidate();
				}
				SleepGear(count);
				continue;
			}
			break;
		}
	}

	private void ScrollToYDOWN(Rectangle r, int stepSize, ref int count)
	{
		while (CaretInfo.Y - ScrollY.Value + CaretInfo.Height > rect_text.Bottom)
		{
			int num = ScrollY.Value + stepSize;
			bool flag = ScrollY.SetValue(num);
			if (ScrollY.Value == num)
			{
				count++;
				if (flag)
				{
					Invalidate();
				}
				SleepGear(count);
				continue;
			}
			break;
		}
	}

	private void ScrollToX(Rectangle r)
	{
		int stepSize = CaretInfo.Height;
		int count = 0;
		int num = CaretInfo.X - ScrollX.Value;
		if (num < rect_text.X)
		{
			ScrollToXUP(r, stepSize, ref count);
		}
		else if (num + CaretInfo.Width > rect_text.Right)
		{
			ScrollToXDOWN(r, stepSize, ref count);
		}
	}

	private void ScrollToXUP(Rectangle r, int stepSize, ref int count)
	{
		while (CaretInfo.X - ScrollX.Value < rect_text.X)
		{
			int num = ScrollX.Value - stepSize;
			bool flag = ScrollX.SetValue(num);
			if (ScrollX.Value == num)
			{
				count++;
				if (flag)
				{
					Invalidate();
				}
				SleepGear(count);
				continue;
			}
			break;
		}
	}

	private void ScrollToXDOWN(Rectangle r, int stepSize, ref int count)
	{
		while (CaretInfo.X - ScrollX.Value + CaretInfo.Width > rect_text.Right)
		{
			int num = ScrollX.Value + stepSize;
			bool flag = ScrollX.SetValue(num);
			if (ScrollX.Value == num)
			{
				count++;
				if (flag)
				{
					Invalidate();
				}
				SleepGear(count);
				continue;
			}
			break;
		}
	}

	private void SleepGear(int count)
	{
		if (count > 7)
		{
			Thread.Sleep(1);
		}
		else if (count > 5)
		{
			Thread.Sleep(10);
		}
		else if (count > 3)
		{
			Thread.Sleep(30);
		}
		else
		{
			Thread.Sleep(50);
		}
	}

	public Input SyncScroll(Input input)
	{
		SyncScrollObj = input;
		return this;
	}

	private bool SetText(string value, bool active, bool changed = true)
	{
		if (value == null)
		{
			value = "";
		}
		if (_text == value)
		{
			return false;
		}
		_text = value;
		isempty = string.IsNullOrEmpty(_text);
		OnAllowClear();
		FixFontWidth();
		if (active)
		{
			OnSetText(value, isempty);
		}
		if (!DesignMode)
		{
			if (isempty)
			{
				if (selectionStart > 0)
				{
					SetSelectionStart(0);
				}
			}
			else if (cache_font != null && (SetTextSelectionEnd || cache_font.Count < selectionStart))
			{
				SetSelectionStart(cache_font.Count);
			}
		}
		if (changed)
		{
			OnTextChanged(EventArgs.Empty);
			OnPropertyChanged("Text");
		}
		return true;
	}

	private bool SetTextIn(string text, int start, out int len, bool changed = true)
	{
		List<CacheFont> collection = FixFontWidth(text, start, out len);
		if (cache_font == null)
		{
			cache_font = collection;
			_text = text;
		}
		else
		{
			cache_font.InsertRange(start, collection);
			List<string> list = new List<string>(cache_font.Count);
			for (int i = 0; i < cache_font.Count; i++)
			{
				cache_font[i].i = i;
				list.Add(cache_font[i].text);
			}
			_text = string.Join("", list);
		}
		isempty = false;
		OnAllowClear();
		rtl_run_dirty = true;
		CalculateRect();
		OnSetText(_text, isempty);
		if (changed)
		{
			OnTextChanged(EventArgs.Empty);
			OnPropertyChanged("Text");
		}
		return true;
	}

	private bool SetTextAppend(string text, int start, out int len)
	{
		string text2 = (_text += text);
		List<CacheFont> collection = FixFontWidth(text, start, out len);
		if (cache_font == null)
		{
			cache_font = collection;
		}
		else
		{
			cache_font.AddRange(collection);
		}
		isempty = false;
		OnAllowClear();
		rtl_run_dirty = true;
		CalculateRect();
		OnSetText(text2, isempty);
		OnTextChanged(EventArgs.Empty);
		OnPropertyChanged("Text");
		return true;
	}

	private bool SetTextRemove(ref List<CacheFont> cache_font, int start, int end, bool r = false)
	{
		try
		{
			cache_font.RemoveRange(start, end);
		}
		catch
		{
			cache_font.RemoveAll((CacheFont a) => a.i == start);
		}
		if (cache_font.Count > 0)
		{
			HasRTLText = false;
			List<string> list = new List<string>(cache_font.Count);
			for (int num = 0; num < cache_font.Count; num++)
			{
				cache_font[num].i = num;
				list.Add(cache_font[num].text);
				if (cache_font[num].dir == TextDirection.RTL)
				{
					HasRTLText = true;
				}
			}
			string text = string.Join("", list);
			if (_text == text)
			{
				return false;
			}
			isempty = false;
			_text = text;
			if (r)
			{
				return true;
			}
			OnAllowClear();
			rtl_run_dirty = true;
			CalculateRect();
			OnSetText(text, isempty);
		}
		else
		{
			string text2 = "";
			if (_text == text2)
			{
				return false;
			}
			isempty = true;
			_text = text2;
			if (r)
			{
				return true;
			}
			OnAllowClear();
			CleanCacheFont();
			OnSetText(text2, isempty);
		}
		OnTextChanged(EventArgs.Empty);
		OnPropertyChanged("Text");
		return true;
	}

	protected virtual int OnSetSelectionStart(int value)
	{
		return value;
	}

	protected virtual void OnSetText(string text, bool isempty)
	{
	}
}
