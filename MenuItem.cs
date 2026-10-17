using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace AntdUI;

public class MenuItem : BadgeConfig
{
	private Image? icon;

	private string? iconSvg;

	private string? text;

	private string? textSub;

	private bool visible = true;

	internal MenuItemCollection? items;

	private bool enabled = true;

	private AnimationTask? ThreadExpand;

	private bool expand = true;

	private string? badge;

	private string? badgeSvg;

	private TAlign badgeAlign = TAlign.Right;

	private float badgeSize = 0.6f;

	private bool badgeMode;

	private Color? badgefore;

	private Color? badgeback;

	private Color? badgeBorderColor;

	private float? badgeBorderWidth;

	private int badgeOffsetX = 1;

	private int badgeOffsetY = 1;

	private bool hover;

	internal float AnimationHoverValue;

	internal bool AnimationHover;

	private AnimationTask? ThreadHover;

	/// <summary>
	/// 序号
	/// </summary>
	public int Index { get; internal set; }

	/// <summary>
	/// ID
	/// </summary>
	[Description("ID")]
	[Category("Data")]
	[DefaultValue(null)]
	public string? ID { get; set; }

	/// <summary>
	/// 名称
	/// </summary>
	[Description("名称")]
	[Category("Data")]
	[DefaultValue(null)]
	public string? Name { get; set; }

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
				Invalidates();
			}
		}
	}

	/// <summary>
	/// 图标
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
				Invalidates();
			}
		}
	}

	/// <summary>
	/// 是否包含图片
	/// </summary>
	internal bool HasIcon
	{
		get
		{
			if (string.IsNullOrWhiteSpace(iconSvg))
			{
				return icon != null;
			}
			return true;
		}
	}

	/// <summary>
	/// 图标激活
	/// </summary>
	[Description("图标激活")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Image? IconActive { get; set; }

	/// <summary>
	/// 图标激活SVG
	/// </summary>
	[Description("图标激活SVG")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? IconActiveSvg { get; set; }

	/// <summary>
	/// 文本
	/// </summary>
	[Description("文本")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? Text
	{
		get
		{
			return Localization.GetLangI(LocalizationText, text, new string[2] { "{id}", ID });
		}
		set
		{
			if (!(text == value))
			{
				text = value;
				Invalidates();
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	/// <summary>
	/// 子文本
	/// </summary>
	[Description("子文本")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? SubText
	{
		get
		{
			return Localization.GetLangI(LocalizationSubText, textSub, new string[2] { "{id}", ID });
		}
		set
		{
			if (!(text == value))
			{
				textSub = value;
				Invalidates();
			}
		}
	}

	[Description("子文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationSubText { get; set; }

	/// <summary>
	/// 自定义字体
	/// </summary>
	[Description("自定义字体")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Font? Font { get; set; }

	/// <summary>
	/// 自定义子文本字体
	/// </summary>
	[Description("自定义子文本字体")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Font? FontSub { get; set; }

	/// <summary>
	/// 是否显示
	/// </summary>
	[Description("是否显示")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			if (visible != value)
			{
				visible = value;
				Invalidates();
			}
		}
	}

	/// <summary>
	/// 用户定义数据
	/// </summary>
	[Description("用户定义数据")]
	[Category("Data")]
	[DefaultValue(null)]
	public object? Tag { get; set; }

	/// <summary>
	/// 获取列表中所有列表项的集合
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("子集合")]
	[Category("Appearance")]
	public MenuItemCollection Sub
	{
		get
		{
			if (items == null)
			{
				items = new MenuItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

	/// <summary>
	/// 禁用状态
	/// </summary>
	[Description("禁用状态")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (enabled != value)
			{
				enabled = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 展开
	/// </summary>
	[Description("展开")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool Expand
	{
		get
		{
			return expand;
		}
		set
		{
			if (expand == value)
			{
				return;
			}
			expand = value;
			if (items != null && items.Count > 0)
			{
				if (PARENT == null)
				{
					return;
				}
				if (value && PARENT.Unique)
				{
					if (ParentItem == null)
					{
						foreach (MenuItem item in PARENT.Items)
						{
							if (item != this)
							{
								item.Expand = false;
							}
						}
					}
					else
					{
						foreach (MenuItem item2 in ParentItem.Sub)
						{
							if (item2 != this)
							{
								item2.Expand = false;
							}
						}
					}
				}
				if (((PARENT.Mode == TMenuMode.Inline && !PARENT.Collapsed) || PARENT.Mode == TMenuMode.InlineNoText) && Config.HasAnimation("Menu"))
				{
					if (PARENT.tmpAM)
					{
						ExpandProg = 1f;
						ArrowProg = (value ? 1f : (-1f));
						return;
					}
					ThreadExpand?.Dispose();
					ExpandTemp?.Dispose();
					ExpandTemp = null;
					object value2 = ThreadExpand?.Tag;
					ExpandThread = true;
					int totalFrames = Animation.TotalFrames(10, 200);
					ThreadExpand = new AnimationTask(new AnimationFixed2Config((int i, float val, float arrow) =>
					{
						ExpandProg = val;
						ArrowProg = arrow;
						Invalidates();
					}, 10, totalFrames, value2, value).SetEnd(() =>
					{
						ExpandProg = 1f;
						ArrowProg = (value ? 1f : (-1f));
						ExpandThread = false;
						ExpandTemp?.Dispose();
						ExpandTemp = null;
						Invalidates();
					}));
				}
				else
				{
					ExpandProg = 1f;
					ArrowProg = (value ? 1f : (-1f));
					Invalidates();
				}
			}
			else
			{
				expand = false;
				ExpandProg = 1f;
				ArrowProg = -1f;
				Invalidates();
			}
		}
	}

	[Description("是否可以展开")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool CanExpand
	{
		get
		{
			if (visible && items != null)
			{
				return items.Count > 0;
			}
			return false;
		}
	}

	/// <summary>
	/// 徽标内容
	/// </summary>
	[Description("徽标内容")]
	[Category("徽标")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? Badge
	{
		get
		{
			return badge;
		}
		set
		{
			if (!(badge == value))
			{
				badge = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 徽标SVG
	/// </summary>
	[Description("徽标SVG")]
	[Category("徽标")]
	[DefaultValue(null)]
	public string? BadgeSvg
	{
		get
		{
			return badgeSvg;
		}
		set
		{
			if (!(badgeSvg == value))
			{
				badgeSvg = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 徽标方向
	/// </summary>
	[Description("徽标方向")]
	[Category("徽标")]
	[DefaultValue(TAlign.Right)]
	public TAlign BadgeAlign
	{
		get
		{
			return badgeAlign;
		}
		set
		{
			if (badgeAlign != value)
			{
				badgeAlign = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标比例
	/// </summary>
	[Description("徽标比例")]
	[Category("徽标")]
	[DefaultValue(0.6f)]
	public float BadgeSize
	{
		get
		{
			return badgeSize;
		}
		set
		{
			if (badgeSize != value)
			{
				badgeSize = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标模式（镂空）
	/// </summary>
	[Description("徽标模式（镂空）")]
	[Category("徽标")]
	[DefaultValue(false)]
	public bool BadgeMode
	{
		get
		{
			return badgeMode;
		}
		set
		{
			if (badgeMode != value)
			{
				badgeMode = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标前景颜色
	/// </summary>
	[Description("徽标前景颜色")]
	[Category("徽标")]
	[DefaultValue(null)]
	public Color? BadgeFore
	{
		get
		{
			return badgefore;
		}
		set
		{
			if (!(badgefore == value))
			{
				badgefore = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标背景颜色
	/// </summary>
	[Description("徽标背景颜色")]
	[Category("徽标")]
	[DefaultValue(null)]
	public Color? BadgeBack
	{
		get
		{
			return badgeback;
		}
		set
		{
			if (!(badgeback == value))
			{
				badgeback = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标边框颜色
	/// </summary>
	[Description("徽标边框颜色")]
	[Category("徽标")]
	[DefaultValue(null)]
	public Color? BadgeBorderColor
	{
		get
		{
			return badgeBorderColor;
		}
		set
		{
			if (!(badgeBorderColor == value))
			{
				badgeBorderColor = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标边框宽度
	/// </summary>
	[Description("徽标边框宽度")]
	[Category("徽标")]
	[DefaultValue(null)]
	public float? BadgeBorderWidth
	{
		get
		{
			return badgeBorderWidth;
		}
		set
		{
			if (badgeBorderWidth != value)
			{
				badgeBorderWidth = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标偏移X
	/// </summary>
	[Description("徽标偏移X")]
	[Category("徽标")]
	[DefaultValue(1)]
	public int BadgeOffsetX
	{
		get
		{
			return badgeOffsetX;
		}
		set
		{
			if (badgeOffsetX != value)
			{
				badgeOffsetX = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 徽标偏移Y
	/// </summary>
	[Description("徽标偏移Y")]
	[Category("徽标")]
	[DefaultValue(1)]
	public int BadgeOffsetY
	{
		get
		{
			return badgeOffsetY;
		}
		set
		{
			if (badgeOffsetY != value)
			{
				badgeOffsetY = value;
				if (badge != null || badgeSvg != null)
				{
					Invalidate();
				}
			}
		}
	}

	/// <summary>
	/// 是否悬浮
	/// </summary>
	internal bool Hover
	{
		get
		{
			return hover;
		}
		set
		{
			if (hover == value)
			{
				return;
			}
			hover = value;
			if (Config.HasAnimation("Menu"))
			{
				ThreadHover?.Dispose();
				AnimationHover = true;
				ThreadHover = new AnimationTask(new AnimationFixedConfig((float i) =>
				{
					AnimationHoverValue = i;
					Invalidate();
				}, 20, Animation.TotalFrames(20, 200), value).SetEnd(() =>
				{
					AnimationHover = false;
				}));
			}
		}
	}

	/// <summary>
	/// 是否选中
	/// </summary>
	[Description("是否选中")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Select { get; set; }

	/// <summary>
	/// 自定义按钮的集合
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("自定义按钮的集合")]
	[Category("Appearance")]
	public List<MenuButton>? Button { get; set; }

	public MenuItem? ParentItem { get; internal set; }

	internal int Depth { get; set; }

	internal float? ArrowProg { get; set; }

	internal Menu? PARENT { get; set; }

	internal Rectangle rect { get; set; }

	internal Rectangle arrow_rect { get; set; }

	internal Rectangle txt_rect { get; set; }

	internal Rectangle ico_rect { get; set; }

	internal int SubY { get; set; }

	internal int SubHeight { get; set; }

	internal int ExpandHeight { get; set; }

	internal int ExpandRHeight { get; set; }

	internal float ExpandProg { get; set; }

	internal bool ExpandThread { get; set; }

	internal Bitmap? ExpandTemp { get; set; }

	internal bool show { get; set; }

	internal bool Show { get; set; }

	public MenuItem()
	{
	}

	public MenuItem(string text)
	{
		Text = text;
	}

	public MenuItem(string text, Image? icon)
	{
		Text = text;
		Icon = icon;
	}

	public MenuItem(string text, string? icon_svg)
	{
		Text = text;
		IconSvg = icon_svg;
	}

	public void Remove()
	{
		if (ParentItem == null)
		{
			PARENT?.Items.Remove(this);
		}
		else
		{
			ParentItem.items?.Remove(this);
		}
	}

	public void UpdateText(string newText)
	{
		Text = newText;
		Invalidate();
	}

	public float GetArrowProg()
	{
		if (ArrowProg.HasValue)
		{
			return ArrowProg.Value;
		}
		if (!expand)
		{
			return -1f;
		}
		return 1f;
	}

	public Rectangle Rect()
	{
		return Rect("");
	}

	public Rectangle Rect(int x, int y)
	{
		return Rect("", x, y);
	}

	public Rectangle Rect(string type, int x = 0, int y = 0)
	{
		if (x > 0 || y > 0)
		{
			return type switch
			{
				"Text" => new Rectangle(txt_rect.X - x, txt_rect.Y - y, txt_rect.Width, txt_rect.Height), 
				"Sub" => new Rectangle(rect.X - x, rect.Y - y, rect.Width, rect.Height + (Expand ? SubHeight : 0)), 
				"Icon" => new Rectangle(ico_rect.X - x, ico_rect.Y - y, ico_rect.Width, ico_rect.Height), 
				"Arrow" => new Rectangle(arrow_rect.X - x, arrow_rect.Y - y, arrow_rect.Width, arrow_rect.Height), 
				_ => new Rectangle(rect.X - x, rect.Y - y, rect.Width, rect.Height), 
			};
		}
		return type switch
		{
			"Text" => txt_rect, 
			"Sub" => new Rectangle(rect.X, rect.Y, rect.Width, rect.Height + (Expand ? SubHeight : 0)), 
			"Icon" => ico_rect, 
			"Arrow" => arrow_rect, 
			_ => rect, 
		};
	}

	internal int SetRect(Menu obj, int depth, bool indent, Rectangle _rect, int icon_size, int arrow_size, int gap, int gap2, int sp, int sp2, int inlineIndent, int iconsp, int scx, out int exr)
	{
		Depth = depth;
		rect = _rect;
		int num = gap;
		int num2 = gap2;
		int num3 = (_rect.Height - icon_size) / 2;
		if (indent && depth > 0)
		{
			int num4 = inlineIndent * depth;
			num += num4;
			num2 += num4;
		}
		else if (depth > 1)
		{
			int num5 = inlineIndent * (depth - 1);
			num += num5;
			num2 += num5;
		}
		if (HasIcon)
		{
			int num6 = icon_size + iconsp;
			ico_rect = new Rectangle(_rect.X + num, _rect.Y + num3, icon_size, icon_size);
			num += num6;
			num2 += num6;
			txt_rect = new Rectangle(_rect.X + num, _rect.Y, _rect.Width - num2, _rect.Height);
		}
		else
		{
			txt_rect = new Rectangle(_rect.X + num, _rect.Y, _rect.Width - num2, _rect.Height);
		}
		int num7 = _rect.Right - arrow_size - gap - scx;
		arrow_rect = new Rectangle(num7, _rect.Y + (_rect.Height - arrow_size) / 2, arrow_size, arrow_size);
		exr = 0;
		if (Button != null)
		{
			int num8 = icon_size + iconsp;
			foreach (MenuButton item in Button)
			{
				item.PARENT = obj;
				num7 -= num8;
				exr += num8;
				item.rect = new Rectangle(num7, _rect.Y + num3, icon_size, icon_size);
			}
		}
		Show = true;
		return num2;
	}

	internal int SetRectInlineNoText(Rectangle _rect, int icon_size, int arrow_size, int gap, int gap2, int sp, int sp2, int iconsp)
	{
		Depth = 0;
		rect = _rect;
		int num = gap;
		int num2 = gap2;
		if (HasIcon)
		{
			int num3 = icon_size + iconsp;
			int num4 = (_rect.Height - icon_size) / 2;
			ico_rect = new Rectangle(_rect.X + num, _rect.Y + num4, icon_size, icon_size);
			num += num3;
			num2 += num3;
			txt_rect = new Rectangle(_rect.X + num, _rect.Y, _rect.Width - num2, _rect.Height);
		}
		else
		{
			txt_rect = new Rectangle(_rect.X + num, _rect.Y, _rect.Width - num2, _rect.Height);
		}
		arrow_rect = new Rectangle(_rect.Right - (arrow_size - sp), _rect.Y + (_rect.Height - arrow_size) / 2, arrow_size, arrow_size);
		Show = true;
		return num2;
	}

	internal void SetRect(Rectangle _rect, Rectangle rect_text, int arrow_size, int sp)
	{
		Depth = 0;
		rect = _rect;
		txt_rect = rect_text;
		arrow_rect = new Rectangle(_rect.Right - (arrow_size + sp), _rect.Y + (_rect.Height - arrow_size) / 2, arrow_size, arrow_size);
		Show = true;
	}

	internal void SetRectNoArr(Rectangle _rect, Rectangle rect_text)
	{
		Depth = 0;
		rect = _rect;
		txt_rect = rect_text;
		Show = true;
	}

	internal void SetRectDivider(Rectangle _rect)
	{
		Depth = -1;
		rect = _rect;
		Show = true;
	}

	internal bool Contains(int x, int y, int sx, int sy, out MenuButton? btn)
	{
		btn = null;
		if (rect.Contains(x + sx, y + sy))
		{
			SetHover(val: true);
			if (Button != null)
			{
				foreach (MenuButton item in Button)
				{
					if (item.Contains(x, y, sx, sy))
					{
						btn = item;
						return true;
					}
				}
			}
			return true;
		}
		SetHover(val: false);
		return false;
	}

	internal bool Contains(int x, int y, int sx, int sy, ref int hand, ref int change)
	{
		if (rect.Contains(x + sx, y + sy))
		{
			if (SetHover(val: true))
			{
				change++;
			}
			if (Button != null)
			{
				foreach (MenuButton item in Button)
				{
					if (item.Contains(x, y, sx, sy, ref change))
					{
						hand++;
					}
				}
			}
			hand++;
			return true;
		}
		if (SetHover(val: false))
		{
			change++;
		}
		return false;
	}

	internal bool SetHover(bool val)
	{
		bool result = false;
		if (val)
		{
			if (!hover)
			{
				result = true;
			}
			Hover = true;
		}
		else
		{
			if (hover)
			{
				result = true;
			}
			Hover = false;
		}
		return result;
	}

	private void Invalidate()
	{
		PARENT?.Invalidate();
	}

	private void Invalidates()
	{
		PARENT?.ChangeList(print: true);
	}

	public MenuItem SetFont(Font? value)
	{
		Font = value;
		return this;
	}

	public MenuItem SetFontSub(Font? value)
	{
		FontSub = value;
		return this;
	}

	public MenuItem SetIcon(Image? img)
	{
		icon = img;
		return this;
	}

	public MenuItem SetIcon(string? svg)
	{
		iconSvg = svg;
		return this;
	}

	public MenuItem SetIcon(Image? img, Image? hover)
	{
		icon = img;
		IconActive = hover;
		return this;
	}

	public MenuItem SetIcon(string? svg, string? hover)
	{
		iconSvg = svg;
		IconActiveSvg = hover;
		return this;
	}

	public MenuItem SetID(string? value)
	{
		ID = value;
		return this;
	}

	public MenuItem SetName(string? value)
	{
		Name = value;
		return this;
	}

	public MenuItem SetText(string? value, string? localization = null)
	{
		text = value;
		LocalizationText = localization;
		return this;
	}

	public MenuItem SetSubText(string? value, string? localization = null)
	{
		textSub = value;
		LocalizationSubText = localization;
		return this;
	}

	public MenuItem SetVisible(bool value = false)
	{
		visible = value;
		return this;
	}

	public MenuItem SetEnabled(bool value = false)
	{
		enabled = value;
		return this;
	}

	public MenuItem SetExpand(bool value = true)
	{
		expand = value;
		return this;
	}

	public MenuItem SetSub(MenuItem value)
	{
		Sub.Add(value);
		return this;
	}

	public MenuItem SetSub(params MenuItem[] value)
	{
		Sub.AddRange(value);
		return this;
	}

	public MenuItem SetSub(IList<MenuItem> value)
	{
		Sub.AddRange(value);
		return this;
	}

	public MenuItem SetBadge(string? value = " ", TAlign align = TAlign.TR)
	{
		badge = value;
		badgeAlign = align;
		return this;
	}

	public MenuItem SetBadgeSvg(string? value, TAlign align = TAlign.TR)
	{
		badgeSvg = value;
		badgeAlign = align;
		return this;
	}

	public MenuItem SetBadgeAlign(TAlign value)
	{
		badgeAlign = value;
		return this;
	}

	public MenuItem SetBadgeOffset(int x, int y)
	{
		BadgeOffsetX = x;
		BadgeOffsetY = y;
		return this;
	}

	public MenuItem SetBadgeSize(float value)
	{
		BadgeSize = value;
		return this;
	}

	public MenuItem SetBadgeFore(Color? value)
	{
		BadgeFore = value;
		return this;
	}

	public MenuItem SetBadgeBack(Color? value)
	{
		BadgeBack = value;
		return this;
	}

	public MenuItem SetBadgeBorderColor(Color? value)
	{
		BadgeBorderColor = value;
		return this;
	}

	public MenuItem SetBadgeBorderWidth(float? value)
	{
		BadgeBorderWidth = value;
		return this;
	}

	public MenuItem SetBadgeBorder(float value, Color color)
	{
		BadgeBorderWidth = value;
		BadgeBorderColor = color;
		return this;
	}

	public MenuItem SetTag(object? value)
	{
		Tag = value;
		return this;
	}

	public override string? ToString()
	{
		return Text;
	}
}
