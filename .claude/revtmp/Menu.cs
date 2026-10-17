using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Menu 导航菜单
/// </summary>
/// <remarks>为页面和功能提供导航的菜单列表。</remarks>
[Description("Menu 导航菜单")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("SelectChanged")]
public class Menu : IControl, SubLayeredForm, IEventListener
{
	private Color? fore;

	private int radius = 6;

	private bool round;

	private int? _gap;

	private int? icongap;

	private int? _itemMargin;

	private int? _inlineIndent;

	private float iconratio = 1.2f;

	private float? arrowRatio;

	private TMenuMode mode;

	private bool indent;

	private bool unique;

	private bool collapsed;

	private bool scrollBarBlock;

	private MenuItem? selectItem;

	private MenuItem? focusItem;

	private MenuItemCollection? items;

	private bool pauseLayout;

	/// <summary>
	/// 滚动条
	/// </summary>
	[Browsable(false)]
	public ScrollBar ScrollBar;

	private int collapseWidth;

	private int collapsedWidth;

	private bool scroll_show;

	private bool hover_r;

	private Rectangle rect_r;

	private Rectangle rect_r_ico;

	private readonly FormatFlags SL = FormatFlags.NoWrapEllipsis | FormatFlags.VerticalCenter | FormatFlags.Left;

	private MenuItem? mDown;

	private int mouseClicks;

	private MenuItem? tmpItem;

	private TooltipForm? toolTip;

	internal bool tmpAM;

	private ILayeredForm? subForm;

	internal int select_x;

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
	/// 激活字体颜色
	/// </summary>
	[Description("激活字体颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeActive { get; set; }

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
				Invalidate();
				OnPropertyChanged("Round");
			}
		}
	}

	/// <summary>
	/// 焦点模式
	/// </summary>
	[Description("焦点模式")]
	[Category("Appearance")]
	[DefaultValue(TFocusMode.None)]
	public TFocusMode FocusMode { get; set; }

	/// <summary>
	/// 焦点模式颜色
	/// </summary>
	[Description("焦点模式颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? FocusModeColor { get; set; }

	/// <summary>
	/// 焦点模式大小
	/// </summary>
	[Description("焦点模式大小")]
	[Category("Appearance")]
	[DefaultValue(2)]
	public int FocusModeSize { get; set; } = 2;

	/// <summary>
	/// 焦点模式大小比例
	/// </summary>
	[Description("焦点模式大小比例")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public float? FocusModeSizeRatio { get; set; }

	/// <summary>
	/// 焦点模式方向
	/// </summary>
	[Description("焦点模式方向")]
	[Category("Appearance")]
	[DefaultValue(TAlignMini.None)]
	public TAlignMini FocusModeAlign { get; set; }

	/// <summary>
	/// 色彩模式
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("use ColorScheme")]
	[Description("色彩模式")]
	[Category("Appearance")]
	[DefaultValue(TAMode.Auto)]
	public TAMode Theme
	{
		get
		{
			return ColorScheme;
		}
		set
		{
			ColorScheme = value;
		}
	}

	/// <summary>
	/// 间距
	/// </summary>
	[Description("间距")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? Gap
	{
		get
		{
			return _gap;
		}
		set
		{
			if (_gap != value)
			{
				_gap = value;
				ChangeList(print: true);
				OnPropertyChanged("Gap");
			}
		}
	}

	/// <summary>
	/// 图标与文字间距比例
	/// </summary>
	[Description("图标与文字间距比例")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? IconGap
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
				ChangeList(print: true);
				OnPropertyChanged("IconGap");
			}
		}
	}

	/// <summary>
	/// 菜单项外间距
	/// </summary>
	[Description("菜单项外间距")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? itemMargin
	{
		get
		{
			return _itemMargin;
		}
		set
		{
			if (_itemMargin != value)
			{
				_itemMargin = value;
				ChangeList(print: true);
				OnPropertyChanged("itemMargin");
			}
		}
	}

	/// <summary>
	/// 缩进宽度
	/// </summary>
	[Description("缩进宽度")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? InlineIndent
	{
		get
		{
			return _inlineIndent;
		}
		set
		{
			if (_inlineIndent != value)
			{
				_inlineIndent = value;
				ChangeList(print: true);
				OnPropertyChanged("InlineIndent");
			}
		}
	}

	/// <summary>
	/// 图标比例
	/// </summary>
	[Description("图标比例")]
	[Category("Appearance")]
	[DefaultValue(1.2f)]
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
				ChangeList(print: true);
				OnPropertyChanged("IconRatio");
			}
		}
	}

	/// <summary>
	/// 箭头比例
	/// </summary>
	[Description("箭头比例")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public float? ArrowRatio
	{
		get
		{
			return arrowRatio;
		}
		set
		{
			if (arrowRatio != value)
			{
				arrowRatio = value;
				ChangeList(print: true);
				OnPropertyChanged("ArrowRatio");
			}
		}
	}

	/// <summary>
	/// 菜单类型
	/// </summary>
	[Description("菜单类型")]
	[Category("Appearance")]
	[DefaultValue(TMenuMode.Inline)]
	public TMenuMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			if (mode != value)
			{
				mode = value;
				ChangeList(print: true);
				OnPropertyChanged("Mode");
			}
		}
	}

	/// <summary>
	/// 菜单类型是否需要弹出下拉
	/// </summary>
	public bool IsModeNeedDropDown
	{
		get
		{
			if (!IsModeHorizontal)
			{
				return mode == TMenuMode.Vertical;
			}
			return true;
		}
	}

	/// <summary>
	/// 菜单类型是否横向
	/// </summary>
	public bool IsModeHorizontal
	{
		get
		{
			if (mode != TMenuMode.Horizontal)
			{
				return mode == TMenuMode.Horizontal_Arrow;
			}
			return true;
		}
	}

	/// <summary>
	/// 触发下拉的行为
	/// </summary>
	[Description("触发下拉的行为")]
	[Category("Behavior")]
	[DefaultValue(Trigger.Hover)]
	public Trigger Trigger { get; set; } = Trigger.Hover;

	/// <summary>
	/// 常规缩进
	/// </summary>
	[Description("常规缩进")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Indent
	{
		get
		{
			return indent;
		}
		set
		{
			if (indent != value)
			{
				indent = value;
				ChangeList(print: true);
				OnPropertyChanged("Indent");
			}
		}
	}

	/// <summary>
	/// 只保持一个子菜单的展开
	/// </summary>
	[Description("只保持一个子菜单的展开")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Unique
	{
		get
		{
			return unique;
		}
		set
		{
			if (unique != value)
			{
				unique = value;
				if (unique)
				{
					UniqueHand(items);
				}
			}
		}
	}

	/// <summary>
	/// 显示子菜单背景
	/// </summary>
	[Description("显示子菜单背景")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool ShowSubBack { get; set; }

	/// <summary>
	/// 自动折叠
	/// </summary>
	[Description("自动折叠")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool AutoCollapse { get; set; }

	/// <summary>
	/// 是否折叠
	/// </summary>
	[Description("是否折叠")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Collapsed
	{
		get
		{
			return collapsed;
		}
		set
		{
			if (collapsed != value)
			{
				collapsed = value;
				OnPropertyChanged("Collapsed");
			}
		}
	}

	/// <summary>
	/// 超出文字提示配置
	/// </summary>
	[Browsable(false)]
	[Description("超出文字提示配置")]
	[Category("Behavior")]
	[DefaultValue(null)]
	public TooltipConfig? TooltipConfig { get; set; }

	/// <summary>
	/// 鼠标右键控制
	/// </summary>
	[Description("鼠标右键控制")]
	[Category("Action")]
	[DefaultValue(true)]
	public bool MouseRightCtrl { get; set; } = true;

	/// <summary>
	/// 滚动条遮挡
	/// </summary>
	[Description("滚动条遮挡")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool ScrollBarBlock
	{
		get
		{
			return scrollBarBlock;
		}
		set
		{
			if (scrollBarBlock != value)
			{
				scrollBarBlock = value;
				ChangeList(print: true);
				OnPropertyChanged("ScrollBarBlock");
			}
		}
	}

	/// <summary>
	/// 下拉边距
	/// </summary>
	[Description("下拉边距")]
	[Category("Appearance")]
	[DefaultValue(typeof(Size), "16, 10")]
	public Size DropDownPadding { get; set; } = new Size(16, 10);

	/// <summary>
	/// 下拉图标比例
	/// </summary>
	[Description("下拉图标比例")]
	[Category("Appearance")]
	[DefaultValue(0.7f)]
	public float DropIconRatio { get; set; } = 0.7f;

	/// <summary>
	/// 下拉图标边距比例
	/// </summary>
	[Description("下拉图标边距比例")]
	[Category("Appearance")]
	[DefaultValue(0.25f)]
	public float DropIconGap { get; set; } = 0.25f;

	/// <summary>
	/// 下拉菜单偏移量（水平模式下子菜单相对主菜单的偏移）
	/// </summary>
	[Description("下拉菜单偏移量")]
	[Category("Appearance")]
	[DefaultValue(typeof(Size), "0, 0")]
	public Size DropDownOffset { get; set; } = new Size(0, 0);

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public MenuItem? SelectItem
	{
		get
		{
			return selectItem;
		}
		private set
		{
			focusItem = null;
			if (selectItem != value)
			{
				selectItem = value;
				if (value != null)
				{
					OnSelectChanged(value);
				}
			}
		}
	}

	/// <summary>
	/// 菜单集合
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("菜单集合")]
	[Category("Data")]
	public MenuItemCollection Items
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

	[Browsable(false)]
	[Description("暂停布局")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool PauseLayout
	{
		get
		{
			return pauseLayout;
		}
		set
		{
			if (pauseLayout != value)
			{
				pauseLayout = value;
				if (!value)
				{
					ChangeList(print: true);
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	/// <summary>
	/// 展开之前宽度
	/// </summary>
	public int CollapseWidth => collapseWidth;

	/// <summary>
	/// 展开后宽度
	/// </summary>
	public int CollapsedWidth => collapsedWidth;

	protected override bool CanMouseMove { get; set; } = true;

	/// <summary>
	/// Select 属性值更改时发生
	/// </summary>
	[Description("Select 属性值更改时发生")]
	[Category("Behavior")]
	public event SelectEventHandler? SelectChanged;

	/// <summary>
	/// 点击项时发生
	/// </summary>
	[Description("点击项时发生")]
	[Category("Behavior")]
	public event MenuItemEventHandler? ItemClick;

	/// <summary>
	/// 空白项目点击事件（包含鼠标信息）
	/// </summary>
	[Description("空白项目点击事件")]
	[Category("Behavior")]
	public event MouseEventHandler? NonItemClick;

	/// <summary>
	/// Select 属性值更改前发生
	/// </summary>
	[Description("Select 属性值更改前发生")]
	[Category("Behavior")]
	public event SelectBoolEventHandler? SelectChanging;

	/// <summary>
	/// 自定义按钮点击时发生
	/// </summary>
	[Description("自定义按钮点击时发生")]
	[Category("Behavior")]
	public event MenuCustomButtonEventHandler? CustomButtonClick;

	private void UniqueHand(MenuItemCollection? items)
	{
		if (items == null)
		{
			return;
		}
		foreach (MenuItem item in items)
		{
			if (!item.Expand)
			{
				continue;
			}
			UniqueHand(item.Sub);
			{
				foreach (MenuItem item2 in items)
				{
					if (item != item2)
					{
						item2.Expand = false;
					}
				}
				break;
			}
		}
	}

	public void SelectIndex(int i1, bool focus = true)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		IUSelect(items);
		if (items.ListExceed(i1))
		{
			Invalidate();
			return;
		}
		MenuItem menuItem = items[i1];
		menuItem.Select = true;
		SelectItem = menuItem;
		OnItemClick(menuItem, new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
		if (focus)
		{
			Focus(menuItem);
		}
		Invalidate();
	}

	public void SelectIndex(int i1, int i2, bool focus = true)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		IUSelect(items);
		if (items.ListExceed(i1))
		{
			Invalidate();
			return;
		}
		MenuItem menuItem = items[i1];
		if (menuItem.items.ListExceed(i2))
		{
			Invalidate();
			return;
		}
		MenuItem menuItem2 = menuItem.Sub[i2];
		bool flag = (menuItem2.Select = true);
		menuItem.Select = flag;
		SelectItem = menuItem2;
		OnItemClick(menuItem2, new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
		if (focus)
		{
			Focus(menuItem2);
		}
		Invalidate();
	}

	public void SelectIndex(int i1, int i2, int i3, bool focus = true)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		IUSelect(items);
		if (items.ListExceed(i1))
		{
			Invalidate();
			return;
		}
		MenuItem menuItem = items[i1];
		if (menuItem.items.ListExceed(i2))
		{
			Invalidate();
			return;
		}
		MenuItem menuItem2 = menuItem.Sub[i2];
		if (menuItem2.items.ListExceed(i3))
		{
			Invalidate();
			return;
		}
		MenuItem menuItem3 = menuItem2.Sub[i3];
		bool flag = (menuItem3.Select = true);
		bool flag3 = (menuItem2.Select = flag);
		menuItem.Select = flag3;
		SelectItem = menuItem3;
		OnItemClick(menuItem3, new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
		if (focus)
		{
			Focus(menuItem3);
		}
		Invalidate();
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		ChangeList();
		this.AddListener();
		MenuItem menuItem = GetSelectItem(out List<MenuItem> list);
		if (menuItem == null)
		{
			return;
		}
		selectItem = menuItem;
		foreach (MenuItem item in list)
		{
			item.Select = true;
		}
	}

	public MenuItem? GetSelectItem()
	{
		List<MenuItem> list = new List<MenuItem>(0);
		return GetSelectItem(ref list, items);
	}

	public MenuItem? GetSelectItem(out List<MenuItem> list)
	{
		list = new List<MenuItem>(0);
		return GetSelectItem(ref list, items);
	}

	private MenuItem? GetSelectItem(ref List<MenuItem> list, MenuItemCollection? items)
	{
		if (items == null || items.Count == 0)
		{
			return null;
		}
		foreach (MenuItem item in items)
		{
			List<MenuItem> list2 = new List<MenuItem>(list.Count + 1);
			list2.AddRange(list);
			list2.Add(item);
			MenuItem menuItem = GetSelectItem(ref list2, item.Sub);
			if (menuItem == null)
			{
				if (item.Select)
				{
					list = list2;
					return item;
				}
				continue;
			}
			list = list2;
			return menuItem;
		}
		return null;
	}

	protected override void OnFontChanged(EventArgs e)
	{
		ChangeList();
		base.OnFontChanged(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		ChangeList();
		base.OnSizeChanged(e);
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG)
		{
			ChangeList();
		}
	}

	private bool CanLayout(out bool canPrint)
	{
		if (IsHandleCreated)
		{
			Rectangle clientRectangle = ClientRectangle;
			if (pauseLayout || clientRectangle.Width == 0 || clientRectangle.Height == 0)
			{
				canPrint = false;
				return false;
			}
			canPrint = true;
			if (items == null || items.Count == 0)
			{
				return false;
			}
			return true;
		}
		canPrint = false;
		return false;
	}

	internal void ChangeList(bool print = false)
	{
		if (CanLayout(out var canPrint))
		{
			Rectangle _rect = ClientRectangle;
			Rectangle rect = _rect.PaddingRect(Padding);
			int x = 0;
			int y = 0;
			int icon_count = 0;
			this.GDI((Canvas g) =>
			{
				Size size = g.MeasureString("龍Qq", Font);
				int num = (int)((float)size.Height * iconratio);
				int num2 = (_gap.HasValue ? ((int)((float)_gap.Value * Dpi)) : ((int)((float)size.Height * 0.8f)));
				int num3 = num2 * 2;
				int divider = (int)Dpi;
				int num4 = (_itemMargin.HasValue ? ((int)((float)_itemMargin.Value * Dpi)) : ((int)((float)size.Height * 0.2f)));
				int sp = num4 * 2;
				int num5 = size.Height + num3;
				int inlineIndent = (_inlineIndent.HasValue ? ((int)((float)_inlineIndent.Value * Dpi)) : ((int)((float)size.Height * 1.2f)));
				int iconsp = (icongap.HasValue ? ((int)((float)icongap.Value * Dpi)) : (size.Height / 2));
				if (mode == TMenuMode.Horizontal)
				{
					ChangeListHorizontal(rect, g, items, ref x, num, num2, num3, divider, num4, sp, iconsp);
					scroll_show = x > rect.Width;
					if (scroll_show)
					{
						rect_r = new Rectangle(rect.Right - rect.Height, rect.Y, rect.Height, rect.Height);
						int num6 = (int)((float)rect_r.Height * 0.6f);
						int num7 = (rect_r.Height - num6) / 2;
						rect_r_ico = new Rectangle(rect_r.X + num7, rect_r.Y + num7, num6, num6);
					}
				}
				else if (mode == TMenuMode.Horizontal_Arrow)
				{
					int arrow_size = (arrowRatio.HasValue ? ((int)((float)size.Height * arrowRatio.Value)) : num);
					ChangeListHorizontalArrow(rect, g, items, ref x, num, arrow_size, num2, num3, divider, num4, sp, iconsp);
					scroll_show = x > rect.Width;
					if (scroll_show)
					{
						rect_r = new Rectangle(rect.Right - rect.Height, rect.Y, rect.Height, rect.Height);
						int num8 = (int)((float)rect_r.Height * 0.6f);
						int num9 = (rect_r.Height - num8) / 2;
						rect_r_ico = new Rectangle(rect_r.X + num9, rect_r.Y + num9, num8, num8);
					}
				}
				else
				{
					scroll_show = false;
					collapseWidth = num + num3 + Padding.Horizontal;
					if (mode == TMenuMode.InlineNoText)
					{
						int arrow_size2 = (arrowRatio.HasValue ? ((int)((float)size.Height * arrowRatio.Value)) : ((int)((float)num * 0.85f)));
						collapsedWidth = ChangeListInlineNoText(rect, g, null, items, ref y, num5, num, arrow_size2, num2, num3, divider, num4, sp, iconsp) + Padding.Horizontal;
					}
					else
					{
						int arrow_size3 = (arrowRatio.HasValue ? ((int)((float)size.Height * arrowRatio.Value)) : num);
						int scx;
						if (ScrollBar.Show && scrollBarBlock)
						{
							rect.Width -= ScrollBar.SIZE;
							ChangeListY(rect, items, ref icon_count, num5, num4);
							_ = Padding.Vertical;
							scx = 0;
						}
						else
						{
							scx = ((ChangeListY(rect, items, ref icon_count, num5, num4) + Padding.Vertical > _rect.Height) ? ScrollBar.SIZE : 0);
						}
						collapsedWidth = ChangeList(rect, g, null, items, ref y, num5, num, arrow_size3, num2, num3, divider, num4, sp, inlineIndent, iconsp, scx, 0) + Padding.Horizontal;
					}
					if (AutoCollapse)
					{
						if (icon_count > 0)
						{
							collapsed = collapsedWidth >= _rect.Width;
						}
						else
						{
							collapsed = false;
						}
					}
					if (collapsed)
					{
						ChangeUTitle(items);
					}
				}
			});
			ScrollBar.SetVrSize(y + Padding.Vertical, _rect);
		}
		if (print & canPrint)
		{
			Invalidate();
		}
	}

	private int ChangeListY(Rectangle rect, MenuItemCollection items, ref int icon_count, int height, int sp)
	{
		int num = 0;
		foreach (MenuItem item in items)
		{
			if (item is MenuDividerItem)
			{
				continue;
			}
			if (item.HasIcon)
			{
				icon_count++;
			}
			if (item.Visible)
			{
				num += height + sp;
				if ((mode == TMenuMode.Inline || mode == TMenuMode.InlineNoText) && item.Expand)
				{
					num += ChangeListY(rect, item.Sub, ref icon_count, height, sp);
				}
			}
		}
		return num;
	}

	private int ChangeList(Rectangle rect, Canvas g, MenuItem? Parent, MenuItemCollection items, ref int y, int height, int icon_size, int arrow_size, int gap, int gap2, int divider, int sp, int sp2, int inlineIndent, int iconsp, int scx, int depth)
	{
		int num = 0;
		int num2 = 0;
		foreach (MenuItem item in items)
		{
			item.Index = num2;
			num2++;
			item.PARENT = this;
			item.ParentItem = Parent;
			if (item is MenuDividerItem)
			{
				item.SetRectDivider(new Rectangle(rect.X, rect.Y + y, rect.Width, divider));
				y += divider + sp;
				continue;
			}
			int num3 = item.SetRect(this, depth, Indent, new Rectangle(rect.X, rect.Y + y, rect.Width, height), icon_size, arrow_size, gap, gap2, sp, sp2, inlineIndent, iconsp, scx, out var exr);
			if (!item.Visible)
			{
				continue;
			}
			int num4 = g.MeasureText(item.Text, item.Font ?? Font).Width + num3 + gap2 + scx + exr;
			if (num4 > num)
			{
				num = num4;
			}
			y += height + sp;
			if (mode != TMenuMode.Inline || !item.CanExpand)
			{
				continue;
			}
			if (collapsed)
			{
				int num5 = y;
				int num6 = ChangeList(rect, g, item, item.Sub, ref y, height, icon_size, arrow_size, gap, gap2, divider, sp, sp2, inlineIndent, iconsp, scx, depth + 1);
				if (num6 > num)
				{
					num = num6;
				}
				y = num5;
				continue;
			}
			int num7 = y;
			int num8 = ChangeList(rect, g, item, item.Sub, ref y, height, icon_size, arrow_size, gap, gap2, divider, sp, sp2, inlineIndent, iconsp, scx, depth + 1);
			if (num8 > num)
			{
				num = num8;
			}
			item.SubY = rect.Y + num7 - sp / 2;
			item.SubHeight = y - num7;
			if ((item.Expand || item.ExpandThread) && item.ExpandProg > 0f)
			{
				item.ExpandHeight = y - num7;
				item.ExpandRHeight = (int)((float)item.ExpandHeight * item.ExpandProg);
				y = num7 + item.ExpandRHeight;
			}
			else if (!item.Expand)
			{
				y = num7;
			}
		}
		return num;
	}

	private int ChangeListInlineNoText(Rectangle rect, Canvas g, MenuItem? Parent, MenuItemCollection items, ref int y, int height, int icon_size, int arrow_size, int gap, int gap2, int divider, int sp, int sp2, int iconsp)
	{
		int num = 0;
		int num2 = 0;
		foreach (MenuItem item in items)
		{
			item.Index = num2;
			num2++;
			item.PARENT = this;
			item.ParentItem = Parent;
			if (item is MenuDividerItem)
			{
				item.SetRectDivider(new Rectangle(rect.X, rect.Y + y, rect.Width, divider));
				y += divider + sp;
				continue;
			}
			int num3 = item.SetRectInlineNoText(new Rectangle(rect.X, rect.Y + y, rect.Width, height), icon_size, arrow_size, gap, gap2, sp, sp2, iconsp);
			if (!item.Visible)
			{
				continue;
			}
			int num4 = g.MeasureText(item.Text, item.Font ?? Font).Width + num3 + gap2;
			if (num4 > num)
			{
				num = num4;
			}
			y += height + sp;
			if (!item.CanExpand)
			{
				continue;
			}
			if (collapsed)
			{
				int num5 = y;
				int num6 = ChangeListInlineNoText(rect, g, item, item.Sub, ref y, height, icon_size, arrow_size, gap, gap2, divider, sp, sp2, iconsp);
				if (num6 > num)
				{
					num = num6;
				}
				y = num5;
				continue;
			}
			int num7 = y;
			int num8 = ChangeListInlineNoText(rect, g, item, item.Sub, ref y, height, icon_size, arrow_size, gap, gap2, divider, sp, sp2, iconsp);
			if (num8 > num)
			{
				num = num8;
			}
			item.SubY = rect.Y + num7 - sp / 2;
			item.SubHeight = y - num7;
			if ((item.Expand || item.ExpandThread) && item.ExpandProg > 0f)
			{
				item.ExpandHeight = y - num7;
				item.ExpandRHeight = (int)((float)item.ExpandHeight * item.ExpandProg);
				y = num7 + item.ExpandRHeight;
			}
			else if (!item.Expand)
			{
				y = num7;
			}
		}
		return num;
	}

	private void ChangeListHorizontal(Rectangle rect, Canvas g, MenuItemCollection items, ref int x, int icon_size, int gap, int gap2, int divider, int sp, int sp2, int iconsp)
	{
		int num = 0;
		foreach (MenuItem item in items)
		{
			item.Index = num;
			num++;
			item.PARENT = this;
			if (item is MenuDividerItem)
			{
				item.SetRectDivider(new Rectangle(rect.X + x, rect.Y, divider, rect.Height));
				x += divider + sp;
			}
			else
			{
				int num2 = g.MeasureText(item.Text, item.Font ?? Font).Width;
				if (item.HasIcon)
				{
					int num3 = icon_size + iconsp;
					int num4 = gap2 + num3;
					int num5 = (rect.Height - icon_size) / 2;
					int num6 = num2 + gap2 + num3;
					Rectangle rect2 = new Rectangle(rect.X + x, rect.Y, num6, rect.Height);
					item.ico_rect = new Rectangle(rect2.X + gap, rect2.Y + num5, icon_size, icon_size);
					item.SetRectNoArr(rect2, new Rectangle(rect2.X + gap + num3, rect2.Y, rect2.Width - num4, rect2.Height));
					if (item.Visible)
					{
						x += num6 + sp;
					}
				}
				else
				{
					int num7 = num2 + gap2;
					Rectangle rect3 = new Rectangle(rect.X + x, rect.Y, num7, rect.Height);
					item.SetRectNoArr(rect3, new Rectangle(rect3.X + gap, rect3.Y, rect3.Width - gap2, rect3.Height));
					if (item.Visible)
					{
						x += num7 + sp;
					}
				}
			}
			MenuItemCollection? menuItemCollection = item.items;
			if (menuItemCollection != null && menuItemCollection.Count > 0)
			{
				item.SubY = rect.Height;
			}
		}
	}

	private void ChangeListHorizontalArrow(Rectangle rect, Canvas g, MenuItemCollection items, ref int x, int icon_size, int arrow_size, int gap, int gap2, int divider, int sp, int sp2, int iconsp)
	{
		int num = 0;
		foreach (MenuItem item in items)
		{
			item.Index = num;
			num++;
			item.PARENT = this;
			if (item is MenuDividerItem)
			{
				item.SetRectDivider(new Rectangle(rect.X + x, rect.Y, divider, rect.Height));
				x += divider + sp;
			}
			else
			{
				int num2 = g.MeasureText(item.Text, item.Font ?? Font).Width;
				if (item.HasIcon)
				{
					int num3 = icon_size + iconsp;
					int num4 = gap2 + num3;
					int num5 = (rect.Height - icon_size) / 2;
					if (item.CanExpand)
					{
						int num6 = num2 + gap2 + num3 + sp + arrow_size / 2;
						Rectangle rect2 = new Rectangle(rect.X + x, rect.Y, num6, rect.Height);
						item.ico_rect = new Rectangle(rect2.X + gap, rect2.Y + num5, icon_size, icon_size);
						item.SetRect(rect2, new Rectangle(rect2.X + gap + num3, rect2.Y, rect2.Width - num4, rect2.Height), arrow_size, sp);
						if (item.Visible)
						{
							x += num6 + sp;
						}
					}
					else
					{
						int num7 = num2 + gap2 + num3;
						Rectangle rect3 = new Rectangle(rect.X + x, rect.Y, num7, rect.Height);
						item.ico_rect = new Rectangle(rect3.X + gap, rect3.Y + num5, icon_size, icon_size);
						item.SetRectNoArr(rect3, new Rectangle(rect3.X + gap + num3, rect3.Y, rect3.Width - num4, rect3.Height));
						if (item.Visible)
						{
							x += num7 + sp;
						}
					}
				}
				else if (item.CanExpand)
				{
					int num8 = num2 + gap2 + sp + arrow_size / 2;
					Rectangle rect4 = new Rectangle(rect.X + x, rect.Y, num8, rect.Height);
					item.SetRect(rect4, new Rectangle(rect4.X + gap, rect4.Y, rect4.Width - gap2, rect4.Height), arrow_size, sp);
					if (item.Visible)
					{
						x += num8 + sp;
					}
				}
				else
				{
					int num9 = num2 + gap2;
					Rectangle rect5 = new Rectangle(rect.X + x, rect.Y, num9, rect.Height);
					item.SetRectNoArr(rect5, new Rectangle(rect5.X + gap, rect5.Y, rect5.Width - gap2, rect5.Height));
					if (item.Visible)
					{
						x += num9 + sp;
					}
				}
			}
			MenuItemCollection? menuItemCollection = item.items;
			if (menuItemCollection != null && menuItemCollection.Count > 0)
			{
				item.SubY = rect.Height;
			}
		}
	}

	private void ChangeUTitle(MenuItemCollection items)
	{
		foreach (MenuItem item in items)
		{
			if (!(item is MenuDividerItem))
			{
				item.ico_rect = new Rectangle(item.rect.X + (item.rect.Width - item.ico_rect.Width) / 2, item.ico_rect.Y, item.ico_rect.Width, item.ico_rect.Height);
				if (item.Visible && item.CanExpand)
				{
					ChangeUTitle(item.Sub);
				}
			}
		}
	}

	public Menu()
	{
		ScrollBar = new ScrollBar(this);
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		if (items == null || items.Count == 0)
		{
			base.OnDraw(e);
			return;
		}
		Canvas canvas = e.Canvas;
		if (scroll_show)
		{
			canvas.SetClip(new Rectangle(e.Rect.X, e.Rect.Y, rect_r.Right - rect_r.Height, e.Rect.Height));
		}
		int value = ScrollBar.Value;
		canvas.TranslateTransform(0f, -value);
		bool enabled = Enabled;
		Color symbol = Colour.TextQuaternary.GetSymbol(ColorScheme, "foreDisabled", "Menu", Name);
		Color value2;
		Color color;
		Color fore_active;
		Color back_active;
		if (Config.IsDark || ColorScheme == TAMode.Dark)
		{
			value2 = fore ?? Colour.TextSecondary.Get(ColorScheme, "Menu", Name);
			color = (fore_active = ForeActive ?? Colour.TextBase.Get(ColorScheme, "Menu", Name));
			back_active = BackActive ?? Colour.Primary.Get(ColorScheme, "Menu", Name);
		}
		else
		{
			value2 = fore ?? Colour.TextBase.Get(ColorScheme, "Menu", Name);
			fore_active = ForeActive ?? Colour.Primary.Get(ColorScheme, "Menu", Name);
			color = BackHover ?? Colour.FillSecondary.Get(ColorScheme, "Menu", Name);
			back_active = BackActive ?? Colour.PrimaryBg.Get(ColorScheme, "Menu", Name);
		}
		float num = (float)radius * Dpi;
		using (SolidBrush sub_bg = new SolidBrush(Colour.FillQuaternary.Get(ColorScheme, "Menu", Name)))
		{
			using SolidBrush brush_split = new SolidBrush(Colour.Split.Get(ColorScheme, "Menu", Name));
			PaintItems(canvas, e.Rect, value, items, value2, fore_active, symbol, color, back_active, num, sub_bg, brush_split, enabled);
		}
		canvas.ResetTransform();
		if (scroll_show)
		{
			canvas.ResetClip();
			if (hover_r)
			{
				using GraphicsPath path = rect_r.RoundPath(num);
				canvas.Fill(color, path);
			}
			canvas.Svg("EllipsisOutlined", rect_r_ico, value2);
		}
		ScrollBar.Paint(canvas);
		base.OnDraw(e);
	}

	private void PaintItems(Canvas g, Rectangle rect, int sy, MenuItemCollection items, Color fore, Color fore_active, Color fore_enabled, Color back_hover, Color back_active, float radius, SolidBrush sub_bg, SolidBrush brush_split, bool enable)
	{
		foreach (MenuItem item in items)
		{
			item.show = item.Show && item.Visible && rect.IsItemVisibleExpand(sy, item.rect, item.Expand, item.SubHeight);
			if (!item.show)
			{
				continue;
			}
			if (item.Depth == -1)
			{
				g.Fill(brush_split, item.rect);
				continue;
			}
			PaintIt(g, item, fore, fore_active, fore_enabled, back_hover, back_active, radius, enable);
			if (!collapsed && (item.Expand || item.ExpandThread) && item.items != null && item.items.Count > 0)
			{
				GraphicsState graphicsState = g.Save();
				if (item.ExpandThread)
				{
					g.SetClip(new RectangleF(rect.X, item.rect.Bottom, rect.Width, (float)item.ExpandHeight * item.ExpandProg));
				}
				if (ShowSubBack)
				{
					g.Fill(sub_bg, new RectangleF(rect.X, item.SubY, rect.Width, item.SubHeight));
				}
				PaintItemExpand(g, rect, sy, item.items, fore, fore_active, fore_enabled, back_hover, back_active, radius, brush_split, enable);
				g.Restore(graphicsState);
			}
		}
	}

	private void PaintItemExpand(Canvas g, Rectangle rect, int sy, MenuItemCollection items, Color fore, Color fore_active, Color fore_enabled, Color back_hover, Color back_active, float radius, SolidBrush brush_split, bool enable)
	{
		foreach (MenuItem item in items)
		{
			item.show = item.Show && item.Visible && rect.IsItemVisibleExpand(sy, item.rect, item.Expand, item.SubHeight);
			if (!item.show)
			{
				continue;
			}
			if (item.Depth == -1)
			{
				g.Fill(brush_split, item.rect);
				continue;
			}
			PaintIt(g, item, fore, fore_active, fore_enabled, back_hover, back_active, radius, enable);
			if ((!item.Expand && !item.ExpandThread) || item.items == null || item.items.Count <= 0)
			{
				continue;
			}
			if (item.ExpandThread)
			{
				if (item.ExpandTemp == null)
				{
					item.ExpandTemp = new Bitmap(rect.Width, item.ExpandHeight);
					using Canvas canvas = Graphics.FromImage(item.ExpandTemp).HighLay(Dpi, text: true);
					canvas.TranslateTransform(0f, -item.rect.Bottom);
					PaintItemExpand(canvas, rect, sy, item.items, fore, fore_active, fore_enabled, back_hover, back_active, radius, brush_split, enable);
				}
				g.Image(item.ExpandTemp, new Rectangle(rect.X, item.rect.Bottom, item.ExpandTemp.Width, item.ExpandRHeight), item.ExpandTemp.Width, item.ExpandRHeight, item.ExpandProg);
			}
			else
			{
				PaintItemExpand(g, rect, sy, item.items, fore, fore_active, fore_enabled, back_hover, back_active, radius, brush_split, enable);
			}
		}
	}

	private void PaintIt(Canvas g, MenuItem it, Color fore, Color fore_active, Color fore_enabled, Color back_hover, Color back_active, float radius, bool enable)
	{
		if (collapsed)
		{
			PaintItemMini(g, it, fore, fore_active, fore_enabled, back_hover, back_active, radius, enable);
		}
		else
		{
			PaintItem(g, it, fore, fore_active, fore_enabled, back_hover, back_active, radius, enable);
		}
		it.PaintBadge(Font, it.rect, g, ColorScheme, Name);
	}

	private void PaintItemMini(Canvas g, MenuItem it, Color fore, Color fore_active, Color fore_enabled, Color back_hover, Color back_active, float radius, bool enable)
	{
		if (enable && it.Enabled)
		{
			if (Config.IsDark || ColorScheme == TAMode.Dark)
			{
				if (it.Select)
				{
					PaintBack(g, back_active, it.rect, radius);
					PaintIcon(g, it, fore_active);
				}
				else if (it.AnimationHover)
				{
					PaintIcon(g, it, fore.BlendColors(it.AnimationHoverValue, back_hover));
				}
				else if (it.Hover)
				{
					PaintIcon(g, it, back_hover);
				}
				else
				{
					PaintIcon(g, it, fore);
				}
			}
			else if (it.Select)
			{
				PaintBack(g, back_active, it.rect, radius);
				PaintIcon(g, it, fore_active);
			}
			else
			{
				if (it.AnimationHover)
				{
					PaintBack(g, Helper.ToColorN(it.AnimationHoverValue, back_hover), it.rect, radius);
				}
				else if (it.Hover)
				{
					PaintBack(g, back_hover, it.rect, radius);
				}
				PaintIcon(g, it, fore);
			}
		}
		else
		{
			if (it.Select)
			{
				PaintBack(g, back_active, it.rect, radius);
			}
			PaintIcon(g, it, fore_enabled);
		}
	}

	private void PaintItem(Canvas g, MenuItem it, Color fore, Color fore_active, Color fore_enabled, Color back_hover, Color back_active, float radius, bool enable)
	{
		if (enable && it.Enabled)
		{
			if (Config.IsDark || ColorScheme == TAMode.Dark)
			{
				if (it.Select)
				{
					if (it.CanExpand)
					{
						if (IsModeNeedDropDown)
						{
							PaintBack(g, back_active, it.rect, radius);
						}
						PaintTextIconExpand(g, it, fore_active);
					}
					else
					{
						PaintBack(g, back_active, it.rect, radius);
						PaintTextIcon(g, it, fore_active, radius);
					}
				}
				else if (it.AnimationHover)
				{
					PaintTextIconExpand(g, it, fore.BlendColors(it.AnimationHoverValue, back_hover));
				}
				else if (it.Hover)
				{
					PaintTextIconExpand(g, it, back_hover);
				}
				else
				{
					PaintTextIconExpand(g, it, fore);
				}
			}
			else if (it.Select)
			{
				if (it.CanExpand)
				{
					if (IsModeNeedDropDown)
					{
						PaintBack(g, back_active, it.rect, radius);
					}
					PaintTextIconExpand(g, it, fore_active);
				}
				else
				{
					PaintBack(g, back_active, it.rect, radius);
					PaintTextIcon(g, it, fore_active, radius);
				}
			}
			else
			{
				if (it.AnimationHover)
				{
					PaintBack(g, Helper.ToColorN(it.AnimationHoverValue, back_hover), it.rect, radius);
				}
				else if (it.Hover)
				{
					PaintBack(g, back_hover, it.rect, radius);
				}
				PaintTextIconExpand(g, it, fore);
			}
			PaintCustomButton(g, it, fore, fore_active);
			if (focusItem == it)
			{
				PaintFocus(g, it, fore, radius);
			}
			return;
		}
		if (it.Select)
		{
			if (it.CanExpand)
			{
				if (IsModeNeedDropDown)
				{
					PaintBack(g, back_active, it.rect, radius);
				}
				using Pen pen = new Pen(fore_active, Dpi * 2f);
				LineCap startCap = (pen.EndCap = LineCap.Round);
				pen.StartCap = startCap;
				g.DrawLines(pen, it.arrow_rect.TriangleLinesVertical(it.GetArrowProg(), 0.4f));
			}
			else
			{
				PaintBack(g, back_active, it.rect, radius);
			}
		}
		else if (it.CanExpand)
		{
			using Pen pen2 = new Pen(fore_enabled, Dpi * 2f);
			LineCap startCap = (pen2.EndCap = LineCap.Round);
			pen2.StartCap = startCap;
			g.DrawLines(pen2, it.arrow_rect.TriangleLinesVertical(it.GetArrowProg(), 0.4f));
		}
		PaintTextIcon(g, it, fore_enabled, radius);
	}

	private void PaintTextIcon(Canvas g, MenuItem it, Color fore, float radius, bool focusMode = true)
	{
		using (SolidBrush brush = new SolidBrush(fore))
		{
			if (mode != TMenuMode.InlineNoText)
			{
				if (it.SubText != null)
				{
					Font font = it.FontSub ?? Font;
					int num = font.Height;
					Rectangle txt_rect = it.txt_rect;
					txt_rect.Height -= num;
					g.DrawText(it.Text, it.Font ?? Font, brush, txt_rect, SL);
					using SolidBrush brush2 = new SolidBrush(Colour.TextQuaternary.Get(ColorScheme, "Menu", Name));
					Rectangle txt_rect2 = it.txt_rect;
					txt_rect2.Height += num;
					g.DrawText(it.SubText, font, brush2, txt_rect2, SL);
				}
				else
				{
					g.DrawText(it.Text, it.Font ?? Font, brush, it.txt_rect, SL);
				}
			}
			if (focusMode && focusItem == null)
			{
				PaintFocus(g, it, fore, radius);
			}
		}
		PaintIcon(g, it, fore);
	}

	private void PaintFocus(Canvas g, MenuItem it, Color fore, float radius)
	{
		switch (FocusMode)
		{
		case TFocusMode.Line:
			PaintFocusLine(g, it, FocusModeColor ?? fore);
			break;
		case TFocusMode.Border:
			PaintFocusBorder(g, it, FocusModeColor ?? Colour.PrimaryBorder.Get(ColorScheme, "Menu", Name), radius);
			break;
		}
	}

	private void PaintFocusLine(Canvas g, MenuItem it, Color fill)
	{
		int lsize = (int)((float)FocusModeSize * Dpi);
		switch (FocusModeAlign)
		{
		case TAlignMini.None:
			if (IsModeHorizontal)
			{
				PaintFocusLineBottom(g, it, fill, lsize);
			}
			else
			{
				PaintFocusLineLeft(g, it, fill, lsize);
			}
			break;
		case TAlignMini.Top:
			PaintFocusLineTop(g, it, fill, lsize);
			break;
		case TAlignMini.Bottom:
			PaintFocusLineBottom(g, it, fill, lsize);
			break;
		case TAlignMini.Left:
			PaintFocusLineLeft(g, it, fill, lsize);
			break;
		case TAlignMini.Right:
			PaintFocusLineRight(g, it, fill, lsize);
			break;
		}
	}

	private void PaintFocusLineLeft(Canvas g, MenuItem it, Color fill, int lsize)
	{
		int num = (int)((float)it.rect.Height * (FocusModeSizeRatio ?? 0.6f));
		g.Fill(fill, new Rectangle(it.rect.X, it.rect.Y + (it.rect.Height - num) / 2, lsize, num));
	}

	private void PaintFocusLineRight(Canvas g, MenuItem it, Color fill, int lsize)
	{
		int num = (int)((float)it.rect.Height * (FocusModeSizeRatio ?? 0.6f));
		g.Fill(fill, new Rectangle(it.rect.Right - lsize, it.rect.Y + (it.rect.Height - num) / 2, lsize, num));
	}

	private void PaintFocusLineTop(Canvas g, MenuItem it, Color fill, int lsize)
	{
		int num = (int)((float)it.rect.Width * (FocusModeSizeRatio ?? 0.8f));
		g.Fill(fill, new Rectangle(it.rect.X + (it.rect.Width - num) / 2, it.rect.Y, num, lsize));
	}

	private void PaintFocusLineBottom(Canvas g, MenuItem it, Color fill, int lsize)
	{
		int num = (int)((float)it.rect.Width * (FocusModeSizeRatio ?? 0.8f));
		g.Fill(fill, new Rectangle(it.rect.X + (it.rect.Width - num) / 2, it.rect.Bottom - lsize, num, lsize));
	}

	private void PaintFocusBorder(Canvas g, MenuItem it, Color fill, float radius)
	{
		float num = (float)FocusModeSize * Dpi;
		float num2 = num * 2f;
		float num3 = radius + num;
		using GraphicsPath path = new RectangleF((float)it.rect.X - num, (float)it.rect.Y - num, (float)it.rect.Width + num2, (float)it.rect.Height + num2).RoundPath(num3);
		g.Draw(fill, num, path);
	}

	private void PaintTextIconExpand(Canvas g, MenuItem it, Color fore)
	{
		if (it.CanExpand)
		{
			if (mode == TMenuMode.Inline || mode == TMenuMode.InlineNoText)
			{
				using Pen pen = new Pen(fore, Dpi * 2f);
				LineCap startCap = (pen.EndCap = LineCap.Round);
				pen.StartCap = startCap;
				g.DrawLines(pen, it.arrow_rect.TriangleLinesVertical(it.GetArrowProg(), 0.4f));
			}
			else if (mode == TMenuMode.Vertical)
			{
				using Pen pen2 = new Pen(fore, Dpi * 2f);
				LineCap startCap = (pen2.EndCap = LineCap.Round);
				pen2.StartCap = startCap;
				g.DrawLines(pen2, TAlignMini.Right.TriangleLines(it.arrow_rect, 0.4f));
			}
			else if (mode == TMenuMode.Horizontal_Arrow)
			{
				using Pen pen3 = new Pen(fore, Dpi * 2f);
				LineCap startCap = (pen3.EndCap = LineCap.Round);
				pen3.StartCap = startCap;
				g.DrawLines(pen3, it.arrow_rect.TriangleLinesVertical(-1f, 0.4f));
			}
		}
		PaintTextIcon(g, it, fore, radius, focusMode: false);
	}

	private void PaintIcon(Canvas g, MenuItem it, Color fore)
	{
		if (it.Select)
		{
			int num = 0;
			if (it.IconActive != null)
			{
				g.Image(it.IconActive, it.ico_rect);
				num++;
			}
			if (it.IconActiveSvg != null && g.Svg(it.IconActiveSvg, it.ico_rect, fore))
			{
				num++;
			}
			if (num > 0)
			{
				return;
			}
		}
		if (it.Icon != null)
		{
			g.Image(it.Icon, it.ico_rect);
		}
		if (it.IconSvg != null)
		{
			g.Svg(it.IconSvg, it.ico_rect, fore);
		}
	}

	private void PaintCustomButton(Canvas g, MenuItem it, Color fore, Color fore_active)
	{
		if (it.Button == null)
		{
			return;
		}
		foreach (MenuButton item in it.Button)
		{
			if (item.AnimationHover)
			{
				PaintCustomButtonIcon(g, item, fore.BlendColors(item.AnimationHoverValue, fore_active));
			}
			else if (item.Hover)
			{
				PaintCustomButtonIcon(g, item, fore_active);
			}
			else
			{
				PaintCustomButtonIcon(g, item, fore);
			}
			item.PaintBadge(Font, item.rect, g, ColorScheme, Name);
		}
	}

	private void PaintCustomButtonIcon(Canvas g, MenuButton it, Color fore)
	{
		if (it.Icon != null)
		{
			g.Image(it.Icon, it.rect);
		}
		if (it.IconSvg != null)
		{
			g.Svg(it.IconSvg, it.rect, fore);
		}
	}

	private void PaintBack(Canvas g, Color color, Rectangle rect, float radius)
	{
		if (Round || radius > 0f)
		{
			using (GraphicsPath path = rect.RoundPath(radius, Round))
			{
				g.Fill(color, path);
				return;
			}
		}
		g.Fill(color, rect);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		mouseClicks = e.Clicks;
		if (FocusMode != TFocusMode.None)
		{
			Focus();
		}
		CloseTip();
		CloseDropDown();
		if ((e.Button == MouseButtons.Right && !MouseRightCtrl) || !ScrollBar.MouseDown(e.X, e.Y) || items == null || items.Count == 0 || (scroll_show && rect_r.Contains(e.X, e.Y)))
		{
			return;
		}
		OnTouchDown(e.X, e.Y);
		foreach (MenuItem item in items)
		{
			if (IMouseDown(items, item, e.X, e.Y))
			{
				break;
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if ((e.Button == MouseButtons.Right && !MouseRightCtrl) || !ScrollBar.MouseUp() || !OnTouchUp())
		{
			return;
		}
		if (items == null || items.Count == 0 || mDown == null)
		{
			OnNonItemClick(e);
			return;
		}
		foreach (MenuItem item in items)
		{
			List<MenuItem> list = new List<MenuItem> { item };
			if (IMouseUp(items, item, list, e.X, e.Y, mDown, e))
			{
				return;
			}
		}
		OnNonItemClick(e);
	}

	private bool IMouseDown(MenuItemCollection items, MenuItem item, int x, int y)
	{
		if (item.Visible)
		{
			if (item is MenuDividerItem)
			{
				return false;
			}
			bool canExpand = item.CanExpand;
			if (item.Enabled && item.Contains(x, y, 0, ScrollBar.Value, out MenuButton _))
			{
				mDown = item;
				return true;
			}
			if (canExpand && item.Expand && !collapsed)
			{
				foreach (MenuItem item2 in item.Sub)
				{
					if (IMouseDown(items, item2, x, y))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool IMouseUp(MenuItemCollection items, MenuItem item, List<MenuItem> list, int x, int y, MenuItem MDown, MouseEventArgs e)
	{
		if (item.Visible)
		{
			bool canExpand = item.CanExpand;
			if (MDown == item)
			{
				if (item.Enabled && item.Contains(x, y, 0, ScrollBar.Value, out MenuButton btn))
				{
					if (btn == null)
					{
						if (OnSelectChanging(item))
						{
							if (canExpand)
							{
								if (IsModeNeedDropDown && Trigger == Trigger.Click && item.items != null && item.items.Count > 0)
								{
									if (subForm == null)
									{
										OpenDropDown(item);
									}
									else
									{
										CloseDropDown();
									}
								}
								else
								{
									item.Expand = !item.Expand;
								}
							}
							else
							{
								IUSelect(items);
								if (list.Count > 1)
								{
									foreach (MenuItem item2 in list)
									{
										item2.Select = true;
									}
								}
								item.Select = true;
								SelectItem = item;
								OnItemClick(item, e);
								Invalidate();
							}
						}
					}
					else
					{
						OnCustomButtonClick(btn, item);
					}
				}
				return true;
			}
			if (item is MenuDividerItem)
			{
				return false;
			}
			if (canExpand && item.Expand && !collapsed)
			{
				foreach (MenuItem item3 in item.Sub)
				{
					List<MenuItem> list2 = new List<MenuItem>(list.Count + 1);
					list2.AddRange(list);
					list2.Add(item3);
					if (IMouseUp(items, item3, list2, x, y, MDown, e))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (ScrollBar.MouseMove(e.X, e.Y) && OnTouchMove(e.X, e.Y))
		{
			if (items == null || items.Count == 0)
			{
				return;
			}
			int count = 0;
			int hand = 0;
			if (scroll_show)
			{
				if (rect_r.Contains(e.X, e.Y))
				{
					if (!hover_r)
					{
						hover_r = true;
						Invalidate();
					}
					foreach (MenuItem item in items)
					{
						item.Hover = false;
					}
					SetCursor(val: true);
					return;
				}
				if (hover_r)
				{
					count++;
				}
				hover_r = false;
			}
			if (collapsed)
			{
				foreach (MenuItem item2 in items)
				{
					if (item2.show)
					{
						item2.Contains(e.X, e.Y, 0, ScrollBar.Value, ref hand, ref count);
					}
				}
			}
			else if (mode == TMenuMode.Inline)
			{
				foreach (MenuItem item3 in items)
				{
					IMouseMove(item3, e.X, e.Y, ref count, ref hand);
				}
			}
			else if (mode == TMenuMode.InlineNoText)
			{
				foreach (MenuItem item4 in items)
				{
					if (!item4.show)
					{
						continue;
					}
					item4.Contains(e.X, e.Y, 0, ScrollBar.Value, ref hand, ref count);
					if (item4.items == null || item4.items.Count <= 0)
					{
						continue;
					}
					foreach (MenuItem item5 in item4.items)
					{
						IMouseMove(item5, e.X, e.Y, ref count, ref hand);
					}
				}
			}
			else
			{
				foreach (MenuItem item6 in items)
				{
					if (item6.show)
					{
						item6.Contains(e.X, e.Y, 0, ScrollBar.Value, ref hand, ref count);
					}
				}
			}
			SetCursor(hand > 0);
			if (count > 0)
			{
				Invalidate();
			}
		}
		else
		{
			ILeave();
		}
	}

	private void IMouseMove(MenuItem it, int x, int y, ref int count, ref int hand)
	{
		if (!it.show || it.Contains(x, y, 0, ScrollBar.Value, ref hand, ref count) || it.items == null || it.items.Count <= 0)
		{
			return;
		}
		foreach (MenuItem item in it.items)
		{
			IMouseMove(item, x, y, ref count, ref hand);
		}
	}

	protected override bool OnMouseHover(int x, int y)
	{
		if (items == null || items.Count == 0)
		{
			CloseTip();
			CloseDropDown();
			return false;
		}
		if (scroll_show && rect_r.Contains(x, y))
		{
			tmpItem = null;
			CloseTip();
			if (subForm == null)
			{
				List<MenuItem> list = new List<MenuItem>(items.Count);
				foreach (MenuItem item in items)
				{
					if (item.rect.X > rect_r.X - item.rect.Width)
					{
						list.Add(item);
					}
				}
				subForm = new LayeredFormMenuDown(this, radius, rect_r, list);
				subForm.Show(this);
				return true;
			}
			return false;
		}
		int value = ScrollBar.Value;
		MenuItem itemMouseHover = GetItemMouseHover(items, x, y, value, out var drop);
		if (itemMouseHover == null)
		{
			tmpItem = null;
			CloseTip();
			CloseDropDown();
		}
		else
		{
			if (tmpItem == itemMouseHover)
			{
				return false;
			}
			CloseTip();
			CloseDropDown();
			tmpItem = itemMouseHover;
			switch (drop)
			{
			case 0:
				if (OpenDropDown(itemMouseHover))
				{
					OpenTip(itemMouseHover, itemMouseHover.Rect(0, ScrollBar.ValueY));
				}
				return true;
			case 1:
				OpenDropDown(itemMouseHover);
				return true;
			case 2:
				OpenTip(itemMouseHover, new Rectangle(itemMouseHover.rect.X, itemMouseHover.rect.Y + itemMouseHover.rect.Height / 2 - value, itemMouseHover.rect.Width, rect_r.Height));
				return true;
			}
		}
		return false;
	}

	private MenuItem? GetItemMouseHover(MenuItemCollection items, int x, int y, int sy, out int drop)
	{
		drop = 0;
		if (collapsed)
		{
			foreach (MenuItem item in items)
			{
				if (item.show && item.rect.Contains(x, y + sy))
				{
					return item;
				}
			}
		}
		else
		{
			if (mode == TMenuMode.Inline)
			{
				return null;
			}
			if (mode == TMenuMode.InlineNoText)
			{
				drop = 2;
				return GetItemMouseHover(items, x, y, sy);
			}
			if (Trigger == Trigger.Hover)
			{
				drop = 1;
				foreach (MenuItem item2 in items)
				{
					if (item2.show && item2.rect.Contains(x, y + sy))
					{
						return item2;
					}
				}
			}
		}
		return null;
	}

	private MenuItem? GetItemMouseHover(MenuItemCollection items, int x, int y, int sy)
	{
		foreach (MenuItem item in items)
		{
			if (item.show && item.rect.Contains(x, y + sy))
			{
				return item;
			}
			if (item.items != null && item.items.Count > 0)
			{
				MenuItem itemMouseHover = GetItemMouseHover(item.items, x, y, sy);
				if (itemMouseHover != null)
				{
					return itemMouseHover;
				}
			}
		}
		return null;
	}

	private void IMouseHover(MenuItem it, int x, int y, int sy)
	{
		if (it.show && it.rect.Contains(x, y + sy))
		{
			Rectangle rect = new Rectangle(it.rect.X, it.rect.Y + it.rect.Height / 2 - ScrollBar.Value, it.rect.Width, rect_r.Height);
			OpenTip(it, rect);
		}
		else
		{
			if (it.items == null || it.items.Count <= 0)
			{
				return;
			}
			foreach (MenuItem item in it.items)
			{
				IMouseHover(item, x, y, sy);
			}
		}
	}

	public void CloseTip()
	{
		toolTip?.IClose();
		toolTip = null;
	}

	private bool OpenTip(MenuItem it, Rectangle rect)
	{
		if (it.Text == null)
		{
			return true;
		}
		if (toolTip == null)
		{
			toolTip = new TooltipForm(this, rect, it.Text, TooltipConfig ?? new TooltipConfig
			{
				Font = (it.Font ?? Font),
				ArrowAlign = TAlign.Right
			});
			toolTip.Show(this);
		}
		else if (toolTip.SetText(rect, it.Text))
		{
			CloseTip();
			OpenTip(it, rect);
		}
		return false;
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		tmpItem = null;
		if (!RectangleToScreen(ClientRectangle).Contains(Control.MousePosition))
		{
			CloseTip();
			ScrollBar.Leave();
			ILeave();
		}
	}

	protected override void OnLeave(EventArgs e)
	{
		base.OnLeave(e);
		ScrollBar.Leave();
		ILeave();
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		ScrollBar.MouseWheel(e);
		base.OnMouseWheel(e);
	}

	protected override bool OnTouchScrollX(int x, int y, int value)
	{
		return ScrollBar.MouseWheelXCore(value);
	}

	protected override bool OnTouchScrollY(int x, int y, int value)
	{
		return ScrollBar.MouseWheelYCore(value);
	}

	private void ILeave()
	{
		SetCursor(val: false);
		if (items == null || items.Count == 0)
		{
			return;
		}
		int count = 0;
		foreach (MenuItem item in items)
		{
			ILeave(item, ref count);
		}
		if (count > 0)
		{
			Invalidate();
		}
	}

	private void ILeave(MenuItem it, ref int count)
	{
		if (it.Hover)
		{
			count++;
		}
		it.Hover = false;
		if (it.items == null || it.items.Count <= 0)
		{
			return;
		}
		foreach (MenuItem item in it.items)
		{
			ILeave(item, ref count);
		}
	}

	/// <summary>
	/// 取消全部选择
	/// </summary>
	public void USelect()
	{
		if (items != null && items.Count != 0)
		{
			IUSelect(items);
			Invalidate();
		}
	}

	private void IUSelect(MenuItemCollection items)
	{
		foreach (MenuItem item in items)
		{
			IUSelect(item);
		}
		SelectItem = null;
	}

	private void IUSelect(MenuItem it)
	{
		it.Select = false;
		if (it.items == null || it.items.Count <= 0)
		{
			return;
		}
		foreach (MenuItem item in it.items)
		{
			IUSelect(item);
		}
	}

	public MenuItem? HitTest(int x, int y)
	{
		if (items == null || items.Count == 0)
		{
			return null;
		}
		foreach (MenuItem item in items)
		{
			if (IHitTest(x, y, item, out MenuItem result))
			{
				return result;
			}
		}
		return null;
	}

	private bool IHitTest(int x, int y, MenuItem item, out MenuItem? mdown)
	{
		if (item.Visible)
		{
			bool canExpand = item.CanExpand;
			if (item.Enabled && item.Contains(x, y, 0, ScrollBar.Value, out MenuButton _))
			{
				mdown = item;
				return true;
			}
			if (canExpand && item.Expand && !collapsed)
			{
				foreach (MenuItem item2 in item.Sub)
				{
					if (IHitTest(x, y, item2, out mdown))
					{
						return true;
					}
				}
			}
		}
		mdown = null;
		return false;
	}

	protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
	{
		bool flag = base.ProcessCmdKey(ref msg, keyData);
		switch (keyData)
		{
		case Keys.Up:
			if (IsModeHorizontal)
			{
				return FKeyLeft(flag);
			}
			return FKeyUp(flag);
		case Keys.Down:
			if (IsModeHorizontal)
			{
				return FKeyRight(flag);
			}
			return FKeyDown(flag);
		case Keys.Left:
			if (IsModeHorizontal)
			{
				return FKeyUp(flag);
			}
			return FKeyLeft(flag);
		case Keys.Right:
			if (IsModeHorizontal)
			{
				return FKeyDown(flag);
			}
			return FKeyRight(flag);
		case Keys.Return:
			if (selectItem != null)
			{
				MenuItem menuItem = focusItem ?? selectItem;
				if (menuItem.CanExpand)
				{
					menuItem.Expand = !menuItem.Expand;
				}
				else
				{
					Select(menuItem);
				}
			}
			break;
		}
		return flag;
	}

	private bool FKeyUp(bool r)
	{
		if (selectItem == null)
		{
			if (items != null)
			{
				Select(items[0]);
				return true;
			}
		}
		else if (FindUp(focusItem ?? selectItem))
		{
			return true;
		}
		return r;
	}

	private bool FKeyDown(bool r)
	{
		if (selectItem == null)
		{
			if (items != null)
			{
				Select(items[items.Count - 1]);
				return true;
			}
		}
		else if (FindDown(focusItem ?? selectItem))
		{
			return true;
		}
		return r;
	}

	private bool FKeyLeft(bool r)
	{
		if (selectItem != null && FindLeft(focusItem ?? selectItem))
		{
			return true;
		}
		return r;
	}

	private bool FKeyRight(bool r)
	{
		if (selectItem != null && FindRight(focusItem ?? selectItem))
		{
			return true;
		}
		return r;
	}

	private bool FindUp(MenuItem item)
	{
		MenuItem parentItem = item.ParentItem;
		if (parentItem == null)
		{
			int num = items.IndexOf(item) - 1;
			if (num >= 0)
			{
				SetFocusItem(items[num]);
				return true;
			}
			return false;
		}
		int num2 = parentItem.items.IndexOf(item) - 1;
		if (num2 >= 0)
		{
			SetFocusItem(FindUpExpand(parentItem.items[num2]));
		}
		else
		{
			SetFocusItem(parentItem);
		}
		return true;
	}

	private MenuItem FindUpExpand(MenuItem it)
	{
		if (it.CanExpand && it.Expand)
		{
			return FindUpExpand(it.items[it.items.Count - 1]);
		}
		return it;
	}

	private bool FindDown(MenuItem item, bool canex = true)
	{
		if (canex && item.CanExpand && item.Expand)
		{
			SetFocusItem(item.items[0]);
			return true;
		}
		MenuItem parentItem = item.ParentItem;
		if (parentItem == null)
		{
			int num = items.IndexOf(item) + 1;
			if (num < items.Count)
			{
				SetFocusItem(items[num]);
				return true;
			}
		}
		else
		{
			int num2 = parentItem.items.IndexOf(item) + 1;
			if (num2 < parentItem.items.Count)
			{
				SetFocusItem(parentItem.items[num2]);
				return true;
			}
			if (parentItem.ParentItem == null)
			{
				MenuItemCollection menuItemCollection = items;
				if (menuItemCollection.Count + 1 < menuItemCollection.Count)
				{
					if (FindDown(menuItemCollection[menuItemCollection.Count + 1]))
					{
						return true;
					}
				}
				else if (FindDown(parentItem, canex: false))
				{
					return true;
				}
			}
			else
			{
				MenuItemCollection menuItemCollection2 = parentItem.ParentItem.items;
				int num3 = menuItemCollection2.Count + 1;
				if (num3 < menuItemCollection2.Count)
				{
					if (FindDown(menuItemCollection2[num3]))
					{
						return true;
					}
				}
				else if (FindDown(parentItem, canex: false))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool FindLeft(MenuItem item)
	{
		if (item.CanExpand)
		{
			if (IsModeNeedDropDown)
			{
				CloseDropDown();
			}
			else
			{
				item.Expand = false;
			}
		}
		return true;
	}

	private bool FindRight(MenuItem item)
	{
		if (item.CanExpand)
		{
			if (IsModeNeedDropDown)
			{
				if (subForm == null)
				{
					OpenDropDown(item);
				}
			}
			else
			{
				item.Expand = true;
			}
		}
		return true;
	}

	private void SetFocusItem(MenuItem item)
	{
		TFocusMode focusMode = FocusMode;
		if ((uint)(focusMode - 2) <= 1u)
		{
			focusItem = item;
			Invalidate();
			Focus(item);
		}
		else
		{
			Select(item);
		}
	}

	/// <summary>
	/// 获取选中项索引
	/// </summary>
	public int GetSelectIndex(MenuItem item)
	{
		if (items != null)
		{
			return items.IndexOf(item);
		}
		return -1;
	}

	/// <summary>
	/// 选中菜单
	/// </summary>
	/// <param name="item">项</param>
	/// <param name="focus">设置焦点</param>
	public void Select(MenuItem item, bool focus = true)
	{
		if (items != null && items.Count != 0)
		{
			IUSelect(items);
			Select(item, focus, items);
		}
	}

	private void Select(MenuItem item, bool focus, MenuItemCollection items)
	{
		foreach (MenuItem item2 in items)
		{
			if (item2 == item)
			{
				item2.Select = true;
				tmpAM = true;
				OnItemClick(item2, new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
				SelectItem = item2;
				if (SelectEx(item2.ParentItem) > 0)
				{
					ChangeList(print: true);
				}
				tmpAM = false;
				Invalidate();
				if (focus)
				{
					Focus(item2);
				}
				break;
			}
			if (item2.items != null && item2.items.Count > 0)
			{
				Select(item, focus, item2.items);
			}
		}
	}

	public void Focus(MenuItem menuItem, bool force = false)
	{
		if (ScrollBar.ShowY)
		{
			int valueY = ScrollBar.ValueY;
			if (force || menuItem.rect.Y < valueY || menuItem.rect.Bottom > valueY + Height)
			{
				ScrollBar.ValueY = menuItem.rect.Y - Padding.Top;
			}
		}
	}

	private int SelectEx(MenuItem? it)
	{
		int num = 0;
		if (it == null)
		{
			return num;
		}
		if (it.Expand)
		{
			it.Select = true;
		}
		else
		{
			num++;
			bool expand = (it.Select = true);
			it.Expand = expand;
		}
		return num + SelectEx(it.ParentItem);
	}

	/// <summary>
	/// 移除菜单
	/// </summary>
	/// <param name="item">项</param>
	public void Remove(MenuItem item)
	{
		Remove(item, items);
	}

	private void Remove(MenuItem item, MenuItemCollection? items)
	{
		if (items == null)
		{
			return;
		}
		foreach (MenuItem item2 in items)
		{
			if (item2 == item)
			{
				items.Remove(item2);
				break;
			}
			Remove(item, item2.items);
		}
	}

	public void SetCollapsed(bool value)
	{
		if (collapsed != value)
		{
			collapsed = value;
			ChangeList(print: true);
			OnPropertyChanged("Collapsed");
		}
	}

	/// <summary>
	/// 根据节点id查询节点
	/// </summary>
	public MenuItem? FindID(string id)
	{
		return FindID(items, id);
	}

	private MenuItem? FindID(MenuItemCollection? items, string id)
	{
		if (items == null)
		{
			return null;
		}
		foreach (MenuItem item in items)
		{
			if (item.ID == id)
			{
				return item;
			}
			MenuItem menuItem = FindID(item.items, id);
			if (menuItem != null)
			{
				return menuItem;
			}
		}
		return null;
	}

	/// <summary>
	/// 根据节点name查询节点
	/// </summary>
	public MenuItem? FindName(string id)
	{
		return FindName(items, id);
	}

	private MenuItem? FindName(MenuItemCollection? items, string id)
	{
		if (items == null)
		{
			return null;
		}
		foreach (MenuItem item in items)
		{
			if (item.Name == id)
			{
				return item;
			}
			MenuItem menuItem = FindName(item.items, id);
			if (menuItem != null)
			{
				return menuItem;
			}
		}
		return null;
	}

	protected virtual void OnSelectChanged(MenuItem item)
	{
		SelectChanged?.Invoke(this, new MenuSelectEventArgs(item));
	}

	protected virtual void OnItemClick(MenuItem item, MouseEventArgs e)
	{
		ItemClick?.Invoke(this, new MenuItemEventArgs(item, e.GenerateMouseEventArgs(mouseClicks)));
	}

	protected virtual void OnNonItemClick(MouseEventArgs e)
	{
		NonItemClick?.Invoke(this, e.GenerateMouseEventArgs(mouseClicks));
	}

	protected virtual bool OnSelectChanging(MenuItem it)
	{
		return SelectChanging?.Invoke(this, new MenuSelectEventArgs(it)) ?? true;
	}

	protected virtual void OnCustomButtonClick(MenuButton btn, MenuItem item)
	{
		CustomButtonClick?.Invoke(this, new MenuCustomButtonEventArgs(btn, item));
	}

	public ILayeredForm? SubForm()
	{
		return subForm;
	}

	internal void DropDownChange(MenuItem value)
	{
		select_x = 0;
		subForm = null;
		if (items == null || items.Count == 0)
		{
			return;
		}
		IUSelect(items);
		foreach (MenuItem item in items)
		{
			List<MenuItem> list = new List<MenuItem> { item };
			if (IDropDownChange(items, item, list, value))
			{
				return;
			}
		}
		Invalidate();
	}

	private bool IDropDownChange(MenuItemCollection items, MenuItem item, List<MenuItem> list, MenuItem value)
	{
		bool canExpand = item.CanExpand;
		if (item.Enabled && item == value)
		{
			if (OnSelectChanging(item))
			{
				if (canExpand)
				{
					item.Expand = !item.Expand;
				}
				else
				{
					IUSelect(items);
					if (list.Count > 1)
					{
						foreach (MenuItem item2 in list)
						{
							item2.Select = true;
						}
					}
					item.Select = true;
					SelectItem = item;
					OnItemClick(item, new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
					Invalidate();
				}
			}
			return true;
		}
		if (canExpand)
		{
			foreach (MenuItem item3 in item.Sub)
			{
				List<MenuItem> list2 = new List<MenuItem>(list.Count + 1);
				list2.AddRange(list);
				list2.Add(item3);
				if (IDropDownChange(items, item3, list2, value))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool OpenDropDown(MenuItem it)
	{
		if (it.items == null || it.items.Count == 0)
		{
			return true;
		}
		select_x = 0;
		subForm = new LayeredFormMenuDown(this, radius, it.Rect(0, ScrollBar.ValueY), it.items);
		subForm.Show(this);
		return false;
	}

	private void CloseDropDown()
	{
		subForm?.Close();
		subForm = null;
	}

	protected override void Dispose(bool disposing)
	{
		ScrollBar.Dispose();
		base.Dispose(disposing);
	}
}
