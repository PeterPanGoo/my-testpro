using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Segmented 分段控制器
/// </summary>
/// <remarks>分段控制器。</remarks>
[Description("Segmented 分段控制器")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("SelectIndexChanged")]
public class Segmented : IControl, IEventListener
{
	private TAlignMini barPosition;

	private bool barBg;

	private bool showBar;

	private Color? barColor;

	private float barsize = 3f;

	private int barpadding;

	private bool vertical;

	private bool full;

	private int radius = 6;

	private float? iconratio;

	private float icongap = 0.2f;

	private bool round;

	private TAlignMini iconalign = TAlignMini.Top;

	private int igap;

	private float itemGap = 0.6f;

	private Color? back;

	private Color? backactive;

	private Color? fore;

	private Color? foreactive;

	private TAlignFlow textAlign = TAlignFlow.Center;

	private float _borderWidth;

	private Color? _borderColor;

	private float _itemBorderWidth;

	private Color? _itemBorderColor;

	private int shadow;

	private Color? shadowColor;

	private int shadowOffsetX;

	private int shadowOffsetY;

	private float shadowOpacity = 0.2f;

	private RightToLeft rightToLeft;

	private SegmentedItemCollection? items;

	private int _select = -1;

	private bool AnimationBar;

	private RectangleF AnimationBarValue;

	private AnimationTask? ThreadBar;

	private RectangleF TabSelectRect;

	private bool pauseLayout;

	private Bitmap? shadow_temp;

	private Rectangle Rect;

	private SegmentedItem? tmp;

	private TooltipForm? toolTip;

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
	/// 线条位置
	/// </summary>
	[Description("线条位置")]
	[Category("条")]
	[DefaultValue(TAlignMini.None)]
	public TAlignMini BarPosition
	{
		get
		{
			return barPosition;
		}
		set
		{
			if (barPosition != value)
			{
				barPosition = value;
				showBar = barBg || barPosition != TAlignMini.None;
				Invalidate();
				OnPropertyChanged("BarPosition");
			}
		}
	}

	/// <summary>
	/// 显示条背景
	/// </summary>
	[Description("显示条背景")]
	[Category("条")]
	[DefaultValue(false)]
	public bool BarBg
	{
		get
		{
			return barBg;
		}
		set
		{
			if (barBg != value)
			{
				barBg = value;
				showBar = barBg || barPosition != TAlignMini.None;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 条背景色
	/// </summary>
	[Description("条背景色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BarColor
	{
		get
		{
			return barColor;
		}
		set
		{
			if (!(barColor == value))
			{
				barColor = value;
				Invalidate();
				OnPropertyChanged("BarColor");
			}
		}
	}

	/// <summary>
	/// 条大小
	/// </summary>
	[Description("条大小")]
	[Category("条")]
	[DefaultValue(3f)]
	public float BarSize
	{
		get
		{
			return barsize;
		}
		set
		{
			if (barsize != value)
			{
				barsize = value;
				if (showBar)
				{
					Invalidate();
				}
				OnPropertyChanged("BarSize");
			}
		}
	}

	/// <summary>
	/// 条边距
	/// </summary>
	[Description("条边距")]
	[Category("条")]
	[DefaultValue(0)]
	public int BarPadding
	{
		get
		{
			return barpadding;
		}
		set
		{
			if (barpadding != value)
			{
				barpadding = value;
				if (showBar)
				{
					Invalidate();
				}
				OnPropertyChanged("BarPadding");
			}
		}
	}

	/// <summary>
	/// 条圆角
	/// </summary>
	[Description("条圆角")]
	[Category("条")]
	[DefaultValue(0)]
	public int BarRadius { get; set; }

	/// <summary>
	/// 是否竖向
	/// </summary>
	[Description("是否竖向")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Vertical
	{
		get
		{
			return vertical;
		}
		set
		{
			if (vertical != value)
			{
				vertical = value;
				ChangeItems(print: true);
				OnPropertyChanged("Vertical");
			}
		}
	}

	/// <summary>
	/// 是否铺满
	/// </summary>
	[Description("是否铺满")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Full
	{
		get
		{
			return full;
		}
		set
		{
			if (full != value)
			{
				full = value;
				ChangeItems(print: true);
				OnPropertyChanged("Full");
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
				Invalidate();
				OnPropertyChanged("Radius");
			}
		}
	}

	/// <summary>
	/// 图标比例
	/// </summary>
	[Description("图标比例")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public float? IconRatio
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
				ChangeItems(print: true);
				OnPropertyChanged("IconRatio");
			}
		}
	}

	/// <summary>
	/// 图标与文字间距比例
	/// </summary>
	[Description("图标与文字间距比例")]
	[Category("Appearance")]
	[DefaultValue(0.2f)]
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
				ChangeItems(print: true);
				OnPropertyChanged("IconGap");
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

	[Description("图标对齐方向")]
	[Category("Appearance")]
	[DefaultValue(TAlignMini.Top)]
	public TAlignMini IconAlign
	{
		get
		{
			return iconalign;
		}
		set
		{
			if (iconalign != value)
			{
				iconalign = value;
				ChangeItems(print: true);
				OnPropertyChanged("IconAlign");
			}
		}
	}

	[Description("间距")]
	[Category("Appearance")]
	[DefaultValue(0)]
	public int Gap
	{
		get
		{
			return igap;
		}
		set
		{
			if (igap != value)
			{
				igap = value;
				ChangeItems(print: true);
				OnPropertyChanged("Gap");
			}
		}
	}

	[Description("项间距")]
	[Category("Appearance")]
	[DefaultValue(0.6f)]
	public float ItemGap
	{
		get
		{
			return itemGap;
		}
		set
		{
			if (itemGap != value)
			{
				itemGap = value;
				ChangeItems(print: true);
				OnPropertyChanged("ItemGap");
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
	public Color? BackActive
	{
		get
		{
			return backactive;
		}
		set
		{
			if (!(backactive == value))
			{
				backactive = value;
				Invalidate();
				OnPropertyChanged("BackActive");
			}
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
	public Color? ForeActive
	{
		get
		{
			return foreactive;
		}
		set
		{
			if (!(foreactive == value))
			{
				foreactive = value;
				Invalidate();
				OnPropertyChanged("ForeActive");
			}
		}
	}

	/// <summary>
	/// 文本水平对齐
	/// </summary>
	[Description("文本水平对齐")]
	[Category("Appearance")]
	[DefaultValue(TAlignFlow.Center)]
	public TAlignFlow TextAlign
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
				Invalidate();
				OnPropertyChanged("Type");
			}
		}
	}

	[Description("边框宽度")]
	[Category("Appearance")]
	[DefaultValue(0f)]
	public float BorderWidth
	{
		get
		{
			return _borderWidth;
		}
		set
		{
			if (_borderWidth != value)
			{
				_borderWidth = value;
				Invalidate();
				OnPropertyChanged("BorderWidth");
			}
		}
	}

	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	[Description("边框颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? BorderColor
	{
		get
		{
			return _borderColor;
		}
		set
		{
			if (!(_borderColor == value))
			{
				_borderColor = value;
				Invalidate();
				OnPropertyChanged("BorderColor");
			}
		}
	}

	[Description("项边框宽度")]
	[Category("Appearance")]
	[DefaultValue(0f)]
	public float ItemBorderWidth
	{
		get
		{
			return _itemBorderWidth;
		}
		set
		{
			if (_itemBorderWidth != value)
			{
				_itemBorderWidth = value;
				Invalidate();
				OnPropertyChanged("ItemBorderWidth");
			}
		}
	}

	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	[Description("项边框颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? ItemBorderColor
	{
		get
		{
			return _itemBorderColor;
		}
		set
		{
			if (!(_itemBorderColor == value))
			{
				_itemBorderColor = value;
				Invalidate();
				OnPropertyChanged("ItemBorderColor");
			}
		}
	}

	/// <summary>
	/// 阴影大小
	/// </summary>
	[Description("阴影")]
	[Category("Appearance")]
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
				shadow_temp?.Dispose();
				shadow_temp = null;
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
	public Color? ShadowColor
	{
		get
		{
			return shadowColor;
		}
		set
		{
			if (!(shadowColor == value))
			{
				shadowColor = value;
				shadow_temp?.Dispose();
				shadow_temp = null;
				Invalidate();
				OnPropertyChanged("ShadowColor");
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
				shadow_temp?.Dispose();
				shadow_temp = null;
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
				shadow_temp?.Dispose();
				shadow_temp = null;
				OnPropertyChanged("ShadowOffsetY");
			}
		}
	}

	/// <summary>
	/// 阴影透明度
	/// </summary>
	[Description("阴影透明度")]
	[Category("阴影")]
	[DefaultValue(0.2f)]
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

	[Description("反向")]
	[Category("Appearance")]
	[DefaultValue(RightToLeft.No)]
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
				if (!full)
				{
					ChangeItems(print: true);
					OnPropertyChanged("RightToLeft");
				}
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
	/// 获取列表中所有列表项的集合
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("Data")]
	[DefaultValue(null)]
	public SegmentedItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new SegmentedItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

	/// <summary>
	/// 选择序号
	/// </summary>
	[Description("选择序号")]
	[Category("Data")]
	[DefaultValue(-1)]
	public int SelectIndex
	{
		get
		{
			return _select;
		}
		set
		{
			if (_select != value)
			{
				int old = _select;
				_select = value;
				SetRect(old, _select);
				OnSelectIndexChanged(value);
				OnPropertyChanged("SelectIndex");
			}
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
					ChangeItems(print: true);
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	private FormatFlags s_f => GetTextAlign | FormatFlags.NoWrapEllipsis;

	private FormatFlags GetTextAlign => TextAlign switch
	{
		TAlignFlow.Left => FormatFlags.Left, 
		TAlignFlow.LeftCenter => FormatFlags.VerticalCenter | FormatFlags.Left, 
		TAlignFlow.Right => FormatFlags.Right, 
		TAlignFlow.RightCenter => FormatFlags.VerticalCenter | FormatFlags.Right, 
		_ => FormatFlags.Center, 
	};

	protected override bool CanMouseMove { get; set; } = true;

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
			if (base.AutoSize != value)
			{
				base.AutoSize = value;
				BeforeAutoSize();
			}
		}
	}

	/// <summary>
	/// SelectIndex 属性值更改时发生
	/// </summary>
	[Description("SelectIndex 属性值更改时发生")]
	[Category("Behavior")]
	public event IntEventHandler? SelectIndexChanged;

	/// <summary>
	/// SelectIndex 属性值更改前发生
	/// </summary>
	[Description("SelectIndex 属性值更改前发生")]
	[Category("Behavior")]
	public event IntBoolEventHandler? SelectIndexChanging;

	/// <summary>
	/// 点击项时发生
	/// </summary>
	[Description("点击项时发生")]
	[Category("Behavior")]
	public event SegmentedItemEventHandler? ItemClick;

	/// <summary>
	/// 空白项目点击事件（包含鼠标信息）
	/// </summary>
	[Description("空白项目点击事件")]
	[Category("Behavior")]
	public event MouseEventHandler? NonItemClick;

	public Segmented()
	{
		base.BackColor = Color.Transparent;
	}

	protected override void Dispose(bool disposing)
	{
		ThreadBar?.Dispose();
		shadow_temp?.Dispose();
		base.Dispose(disposing);
	}

	private void SetRect(int old, int value)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		if (items.ListExceed(value))
		{
			Invalidate();
			return;
		}
		SegmentedItem segmentedItem = items[value];
		if (items.ListExceed(old))
		{
			AnimationBarValue = (TabSelectRect = segmentedItem.Rect);
			Invalidate();
			return;
		}
		SegmentedItem segmentedItem2 = items[old];
		ThreadBar?.Dispose();
		RectangleF OldValue = segmentedItem2.Rect;
		RectangleF NewValue = segmentedItem.Rect;
		if (Config.HasAnimation("Segmented", Name))
		{
			if (vertical)
			{
				if (OldValue.X != NewValue.X)
				{
					return;
				}
				AnimationBar = true;
				TabSelectRect = NewValue;
				float p_val = Math.Abs(NewValue.Y - AnimationBarValue.Y) * 0.09f;
				float p_w_val = Math.Abs(NewValue.Height - AnimationBarValue.Height) * 0.1f;
				float p_val2 = (NewValue.Y - AnimationBarValue.Y) * 0.5f;
				ThreadBar = new AnimationTask(new AnimationLoopConfig(this, () =>
				{
					if (AnimationBarValue.Height != NewValue.Height)
					{
						if (NewValue.Height > OldValue.Height)
						{
							AnimationBarValue.Height += p_w_val;
							if (AnimationBarValue.Height > NewValue.Height)
							{
								AnimationBarValue.Height = NewValue.Height;
							}
						}
						else
						{
							AnimationBarValue.Height -= p_w_val;
							if (AnimationBarValue.Height < NewValue.Height)
							{
								AnimationBarValue.Height = NewValue.Height;
							}
						}
					}
					if (NewValue.Y > OldValue.Y)
					{
						if (AnimationBarValue.Y > p_val2)
						{
							AnimationBarValue.Y += p_val / 2f;
						}
						else
						{
							AnimationBarValue.Y += p_val;
						}
						if (AnimationBarValue.Y > NewValue.Y)
						{
							AnimationBarValue.Y = NewValue.Y;
							Invalidate();
							return false;
						}
					}
					else
					{
						AnimationBarValue.Y -= p_val;
						if (AnimationBarValue.Y < NewValue.Y)
						{
							AnimationBarValue.Y = NewValue.Y;
							Invalidate();
							return false;
						}
					}
					Invalidate();
					return true;
				}, 10).SetEnd(() =>
				{
					AnimationBarValue = NewValue;
					AnimationBar = false;
					Invalidate();
				}));
			}
			else
			{
				if (OldValue.Y != NewValue.Y)
				{
					return;
				}
				AnimationBar = true;
				TabSelectRect = NewValue;
				float p_val3 = Math.Abs(NewValue.X - AnimationBarValue.X) * 0.09f;
				float p_w_val2 = Math.Abs(NewValue.Width - AnimationBarValue.Width) * 0.1f;
				float p_val4 = (NewValue.X - AnimationBarValue.X) * 0.5f;
				ThreadBar = new AnimationTask(new AnimationLoopConfig(this, () =>
				{
					if (AnimationBarValue.Width != NewValue.Width)
					{
						if (NewValue.Width > OldValue.Width)
						{
							AnimationBarValue.Width += p_w_val2;
							if (AnimationBarValue.Width > NewValue.Width)
							{
								AnimationBarValue.Width = NewValue.Width;
							}
						}
						else
						{
							AnimationBarValue.Width -= p_w_val2;
							if (AnimationBarValue.Width < NewValue.Width)
							{
								AnimationBarValue.Width = NewValue.Width;
							}
						}
					}
					if (NewValue.X > OldValue.X)
					{
						if (AnimationBarValue.X > p_val4)
						{
							AnimationBarValue.X += p_val3 / 2f;
						}
						else
						{
							AnimationBarValue.X += p_val3;
						}
						if (AnimationBarValue.X > NewValue.X)
						{
							AnimationBarValue.X = NewValue.X;
							Invalidate();
							return false;
						}
					}
					else
					{
						AnimationBarValue.X -= p_val3;
						if (AnimationBarValue.X < NewValue.X)
						{
							AnimationBarValue.X = NewValue.X;
							Invalidate();
							return false;
						}
					}
					Invalidate();
					return true;
				}, 10).SetEnd(() =>
				{
					AnimationBarValue = NewValue;
					AnimationBar = false;
					Invalidate();
				}));
			}
		}
		else
		{
			TabSelectRect = (AnimationBarValue = NewValue);
			Invalidate();
		}
	}

	protected virtual void OnSelectIndexChanged(int e)
	{
		SelectIndexChanged?.Invoke(this, new IntEventArgs(e));
	}

	protected virtual bool OnSelectIndexChanging(int i)
	{
		return SelectIndexChanging?.Invoke(this, new IntEventArgs(i)) ?? true;
	}

	protected virtual void OnItemClick(SegmentedItem item, MouseEventArgs e)
	{
		ItemClick?.Invoke(this, new SegmentedItemEventArgs(item, e));
	}

	protected virtual void OnNonItemClick(MouseEventArgs e)
	{
		NonItemClick?.Invoke(this, e);
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		if (items == null || items.Count == 0)
		{
			base.OnDraw(e);
			return;
		}
		Canvas canvas = e.Canvas;
		float num = (float)radius * Dpi;
		using (GraphicsPath path = Rect.RoundPath(num, Round))
		{
			canvas.Fill(back ?? Colour.BgLayout.Get(ColorScheme, "Segmented", Name), path);
			if (_borderWidth > 0f)
			{
				canvas.Draw(_borderColor ?? Colour.BgLayout.Get(ColorScheme, "Segmented", Name), (int)(_borderWidth * Dpi), path);
			}
		}
		List<SegmentedItem> list = new List<SegmentedItem>(items.Count);
		int _hover = -1;
		for (int i = 0; i < items.Count; i++)
		{
			SegmentedItem segmentedItem = items[i];
			if (segmentedItem == null)
			{
				continue;
			}
			if (PaintItem(canvas, segmentedItem, i, num, ref _hover) && segmentedItem.Hover)
			{
				_hover = i;
				using GraphicsPath path2 = segmentedItem.Rect.RoundPath(num, Round);
				canvas.Fill(BackHover ?? Colour.HoverBg.Get(ColorScheme, "Segmented", Name), path2);
			}
			list.Add(segmentedItem);
		}
		if (AnimationBar)
		{
			if (barPosition == TAlignMini.None)
			{
				using GraphicsPath path3 = AnimationBarValue.RoundPath(num, Round);
				canvas.Fill(backactive ?? Colour.BgElevated.Get(ColorScheme, "Segmented", Name), path3);
			}
			else
			{
				float barSize = BarSize * Dpi;
				float num2 = (float)BarPadding * Dpi;
				float barPadding = num2 * 2f;
				RectangleF barRect = GetBarRect(AnimationBarValue, barSize, num2, barPadding);
				Color brush = barColor ?? backactive ?? Colour.BgElevated.Get(ColorScheme, "Segmented", Name);
				if (BarRadius > 0)
				{
					using GraphicsPath path4 = barRect.RoundPath((float)BarRadius * Dpi);
					canvas.Fill(brush, path4);
				}
				else
				{
					canvas.Fill(brush, barRect);
				}
			}
		}
		bool enabled = Enabled;
		using (SolidBrush solidBrush = new SolidBrush(Colour.TextQuaternary.GetSymbol(ColorScheme, "foreDisabled", "Segmented", Name)))
		{
			for (int j = 0; j < list.Count; j++)
			{
				SegmentedItem segmentedItem2 = list[j];
				using SolidBrush solidBrush2 = new SolidBrush(segmentedItem2.ForeColor ?? fore ?? Colour.TextSecondary.Get(ColorScheme, "Segmented", Name));
				if (j == _select)
				{
					if (enabled && segmentedItem2.Enabled)
					{
						Color color = foreactive ?? segmentedItem2.ForeColor ?? Colour.Text.Get(ColorScheme, "Segmented", Name);
						if (PaintImg(canvas, segmentedItem2, color, segmentedItem2.IconActiveSvg, segmentedItem2.IconActive))
						{
							PaintImg(canvas, segmentedItem2, color, segmentedItem2.IconSvg, segmentedItem2.Icon);
						}
						canvas.DrawText(segmentedItem2.Text, Font, color, segmentedItem2.RectText, s_f);
					}
					else
					{
						if (PaintImg(canvas, segmentedItem2, solidBrush.Color, segmentedItem2.IconActiveSvg, segmentedItem2.IconActive))
						{
							PaintImg(canvas, segmentedItem2, solidBrush.Color, segmentedItem2.IconSvg, segmentedItem2.Icon);
						}
						canvas.DrawText(segmentedItem2.Text, Font, solidBrush, segmentedItem2.RectText, s_f);
					}
				}
				else if (enabled && segmentedItem2.Enabled)
				{
					if (j == _hover)
					{
						Color color2 = ForeHover ?? segmentedItem2.ForeColor ?? Colour.HoverColor.Get(ColorScheme, "Segmented", Name);
						if (PaintImg(canvas, segmentedItem2, color2, segmentedItem2.IconHoverSvg ?? segmentedItem2.IconSvg, segmentedItem2.IconHover ?? segmentedItem2.Icon))
						{
							PaintImg(canvas, segmentedItem2, color2, segmentedItem2.IconSvg, segmentedItem2.Icon);
						}
						canvas.DrawText(segmentedItem2.Text, Font, color2, segmentedItem2.RectText, s_f);
					}
					else
					{
						PaintImg(canvas, segmentedItem2, solidBrush2.Color, segmentedItem2.IconSvg, segmentedItem2.Icon);
						canvas.DrawText(segmentedItem2.Text, Font, solidBrush2, segmentedItem2.RectText, s_f);
					}
				}
				else
				{
					PaintImg(canvas, segmentedItem2, solidBrush.Color, segmentedItem2.IconSvg, segmentedItem2.Icon);
					canvas.DrawText(segmentedItem2.Text, Font, solidBrush, segmentedItem2.RectText, s_f);
				}
				segmentedItem2.PaintBadge(Font, segmentedItem2.Rect, canvas, ColorScheme, Name);
			}
		}
		base.OnDraw(e);
	}

	private bool PaintItem(Canvas g, SegmentedItem it, int i, float _radius, ref int _hover)
	{
		if (i == _select)
		{
			if (barPosition == TAlignMini.None)
			{
				if (AnimationBar)
				{
					return true;
				}
				using GraphicsPath path = TabSelectRect.RoundPath(_radius, Round);
				DrawShadow(g, it.Rect, path);
				g.Fill(backactive ?? Colour.BgElevated.Get(ColorScheme, "Segmented", Name), path);
				if (_itemBorderWidth > 0f)
				{
					g.Draw(_itemBorderColor ?? Colour.BgElevated.Get(ColorScheme, "Segmented", Name), (int)(_itemBorderWidth * Dpi), path);
				}
			}
			else
			{
				if (AnimationBar)
				{
					if (barBg)
					{
						using (GraphicsPath path2 = TabSelectRect.RoundPath(_radius, Round))
						{
							g.Fill(backactive ?? Colour.BgElevated.Get(ColorScheme, "Segmented", Name), path2);
						}
						return false;
					}
					return true;
				}
				if (barBg)
				{
					using GraphicsPath path3 = TabSelectRect.RoundPath(_radius, Round);
					g.Fill(backactive ?? Colour.BgElevated.Get(ColorScheme, "Segmented", Name), path3);
				}
				Color brush = barColor ?? backactive ?? Colour.BgElevated.Get(ColorScheme, "Segmented", Name);
				float barSize = BarSize * Dpi;
				float num = (float)BarPadding * Dpi;
				float barPadding = num * 2f;
				RectangleF barRect = GetBarRect(TabSelectRect, barSize, num, barPadding);
				if (BarRadius > 0)
				{
					using GraphicsPath path4 = barRect.RoundPath((float)BarRadius * Dpi);
					g.Fill(brush, path4);
				}
				else
				{
					g.Fill(brush, barRect);
				}
			}
			return false;
		}
		return true;
	}

	private bool PaintImg(Canvas g, SegmentedItem it, Color color, string? svg, Image? bmp)
	{
		int num = 0;
		if (bmp != null)
		{
			g.Image(bmp, it.RectImg);
			num++;
		}
		if (svg != null && g.Svg(svg, it.RectImg, color))
		{
			num++;
		}
		return num == 0;
	}

	private RectangleF GetBarRect(RectangleF rect, float barSize, float barPadding, float barPadding2)
	{
		return barPosition switch
		{
			TAlignMini.Top => new RectangleF(rect.X + barPadding, rect.Y, rect.Width - barPadding2, barSize), 
			TAlignMini.Left => new RectangleF(rect.X, rect.Y + barPadding, barSize, rect.Height - barPadding2), 
			TAlignMini.Right => new RectangleF(rect.Right - barSize, rect.Y + barPadding, barSize, rect.Height - barPadding2), 
			_ => new RectangleF(rect.X + barPadding, rect.Bottom - barSize, rect.Width - barPadding2, barSize), 
		};
	}

	/// <summary>
	/// 绘制阴影
	/// </summary>
	private void DrawShadow(Canvas g, Rectangle rect_real, GraphicsPath path)
	{
		if (shadow <= 0)
		{
			return;
		}
		int num = (int)((float)Shadow * Dpi);
		int num2 = num * 2;
		int num3 = (int)((float)ShadowOffsetX * Dpi);
		int num4 = (int)((float)ShadowOffsetY * Dpi);
		Rectangle rectangle = new Rectangle(rect_real.X - num, rect_real.Y - num, rect_real.Width + num2, rect_real.Height + num2);
		if (shadow_temp == null || shadow_temp.PixelFormat == PixelFormat.Undefined || shadow_temp.Width != rectangle.Width || shadow_temp.Height != rectangle.Height)
		{
			shadow_temp?.Dispose();
			shadow_temp = new Bitmap(rectangle.Width, rectangle.Height);
			using (Canvas canvas = Graphics.FromImage(shadow_temp).HighLay(Dpi))
			{
				canvas.TranslateTransform(-rect_real.X + num, -rect_real.Y + num);
				canvas.Fill(backactive ?? Colour.BgElevated.Get(ColorScheme, "Segmented", Name), path);
			}
			Helper.Blur(shadow_temp, num);
		}
		g.Image(shadow_temp, new Rectangle(rectangle.X + num3, rectangle.Y + num4, rectangle.Width, rectangle.Height), shadowOpacity);
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
				_select = -1;
				return false;
			}
			return true;
		}
		canPrint = false;
		return false;
	}

	internal void ChangeItems(bool print = false)
	{
		if (CanLayout(out var canPrint))
		{
			if (_select >= items.Count)
			{
				_select = items.Count - 1;
			}
			Rectangle _rect = ClientRectangle.PaddingRect(Padding);
			Rectangle rect = _rect.PaddingRect(Margin);
			this.GDI((Canvas g) =>
			{
				Size size = g.MeasureString("龍Qq", Font);
				int num = size.Height;
				int num2 = (int)((float)num * icongap);
				int num3 = (int)((float)igap * Dpi);
				int num4 = (int)((float)size.Height * itemGap);
				int num5 = num4 * 2;
				if (Full)
				{
					int count = items.Count;
					if (Vertical)
					{
						float num6 = ((float)rect.Height * 1f - (float)(num3 * (count - 1))) / (float)count;
						float num7 = 0f;
						int num8 = (int)Math.Round(num6);
						switch (iconalign)
						{
						case TAlignMini.Top:
						{
							int imgsize4 = (int)((float)size.Height * (iconratio ?? 1.8f));
							foreach (SegmentedItem item in items)
							{
								item.PARENT = this;
								if (item.HasIcon && item.HasEmptyText)
								{
									item.SetIconNoText(new Rectangle(rect.X, (int)Math.Round((float)rect.Y + num7), rect.Width, num8), imgsize4);
								}
								else
								{
									item.SetRectTopFull(new Rectangle(rect.X, (int)Math.Round((float)rect.Y + num7), rect.Width, num8), imgsize4, num, num2, g, Font);
								}
								num7 += num6 + (float)num3;
							}
							break;
						}
						case TAlignMini.Bottom:
						{
							int imgsize3 = (int)((float)size.Height * (iconratio ?? 1.8f));
							foreach (SegmentedItem item2 in items)
							{
								item2.PARENT = this;
								if (item2.HasIcon && item2.HasEmptyText)
								{
									item2.SetIconNoText(new Rectangle(rect.X, (int)Math.Round((float)rect.Y + num7), rect.Width, num8), imgsize3);
								}
								else
								{
									item2.SetRectBottomFull(new Rectangle(rect.X, (int)Math.Round((float)rect.Y + num7), rect.Width, num8), imgsize3, num, num2, g, Font);
								}
								num7 += num6 + (float)num3;
							}
							break;
						}
						case TAlignMini.Left:
						{
							int imgsize2 = (int)((float)size.Height * (iconratio ?? 1.2f));
							foreach (SegmentedItem item3 in items)
							{
								item3.PARENT = this;
								if (item3.HasIcon && item3.HasEmptyText)
								{
									item3.SetIconNoText(new Rectangle(rect.X, (int)Math.Round((float)rect.Y + num7), rect.Width, num8), imgsize2);
								}
								else
								{
									item3.SetRectLeft(new Rectangle(rect.X, (int)Math.Round((float)rect.Y + num7), rect.Width, num8), imgsize2, num2, num4);
								}
								num7 += num6 + (float)num3;
							}
							break;
						}
						case TAlignMini.Right:
						{
							int imgsize = (int)((float)size.Height * (iconratio ?? 1.2f));
							foreach (SegmentedItem item4 in items)
							{
								item4.PARENT = this;
								if (item4.HasIcon && item4.HasEmptyText)
								{
									item4.SetIconNoText(new Rectangle(rect.X, (int)Math.Round((float)rect.Y + num7), rect.Width, num8), imgsize);
								}
								else
								{
									item4.SetRectRight(new Rectangle(rect.X, (int)Math.Round((float)rect.Y + num7), rect.Width, num8), imgsize, num2, num4);
								}
								num7 += num6 + (float)num3;
							}
							break;
						}
						default:
							foreach (SegmentedItem item5 in items)
							{
								item5.PARENT = this;
								item5.SetRectNone(new Rectangle(rect.X, (int)Math.Round((float)rect.Y + num7), rect.Width, num8));
								num7 += num6 + (float)num3;
							}
							break;
						}
					}
					else
					{
						float num9 = ((float)rect.Width * 1f - (float)(num3 * (count - 1))) / (float)count;
						float num10 = 0f;
						int num11 = (int)Math.Round(num9);
						switch (iconalign)
						{
						case TAlignMini.Top:
						{
							int imgsize8 = (int)((float)size.Height * (iconratio ?? 1.8f));
							foreach (SegmentedItem item6 in items)
							{
								item6.PARENT = this;
								if (item6.HasIcon && item6.HasEmptyText)
								{
									item6.SetIconNoText(new Rectangle((int)Math.Round((float)rect.X + num10), rect.Y, num11, rect.Height), imgsize8);
								}
								else
								{
									item6.SetRectTop(new Rectangle((int)Math.Round((float)rect.X + num10), rect.Y, num11, rect.Height), imgsize8, num, num2);
								}
								num10 += num9 + (float)num3;
							}
							break;
						}
						case TAlignMini.Bottom:
						{
							int imgsize7 = (int)((float)size.Height * (iconratio ?? 1.8f));
							foreach (SegmentedItem item7 in items)
							{
								item7.PARENT = this;
								if (item7.HasIcon && item7.HasEmptyText)
								{
									item7.SetIconNoText(new Rectangle((int)Math.Round((float)rect.X + num10), rect.Y, num11, rect.Height), imgsize7);
								}
								else
								{
									item7.SetRectBottom(new Rectangle((int)Math.Round((float)rect.X + num10), rect.Y, num11, rect.Height), imgsize7, num, num2);
								}
								num10 += num9 + (float)num3;
							}
							break;
						}
						case TAlignMini.Left:
						{
							int imgsize6 = (int)((float)size.Height * (iconratio ?? 1.2f));
							foreach (SegmentedItem item8 in items)
							{
								item8.PARENT = this;
								if (item8.HasIcon && item8.HasEmptyText)
								{
									item8.SetIconNoText(new Rectangle((int)Math.Round((float)rect.X + num10), rect.Y, num11, rect.Height), imgsize6);
								}
								else
								{
									item8.SetRectLeft(new Rectangle((int)Math.Round((float)rect.X + num10), rect.Y, num11, rect.Height), imgsize6, num2, num4);
								}
								num10 += num9 + (float)num3;
							}
							break;
						}
						case TAlignMini.Right:
						{
							int imgsize5 = (int)((float)size.Height * (iconratio ?? 1.2f));
							foreach (SegmentedItem item9 in items)
							{
								item9.PARENT = this;
								if (item9.HasIcon && item9.HasEmptyText)
								{
									item9.SetIconNoText(new Rectangle((int)Math.Round((float)rect.X + num10), rect.Y, num11, rect.Height), imgsize5);
								}
								else
								{
									item9.SetRectRight(new Rectangle((int)Math.Round((float)rect.X + num10), rect.Y, num11, rect.Height), imgsize5, num2, num4);
								}
								num10 += num9 + (float)num3;
							}
							break;
						}
						default:
							foreach (SegmentedItem item10 in items)
							{
								item10.PARENT = this;
								item10.SetRectNone(new Rectangle((int)Math.Round((float)rect.X + num10), rect.Y, num11, rect.Height));
								num10 += num9 + (float)num3;
							}
							break;
						}
					}
					Rect = _rect;
				}
				else
				{
					List<SegmentedItem> list = new List<SegmentedItem>(items.Count);
					if (Vertical)
					{
						int num12 = 0;
						int num13 = 0;
						switch (iconalign)
						{
						case TAlignMini.Top:
						{
							int imgsize12 = (int)((float)size.Height * (iconratio ?? 1.8f));
							int num18 = (int)Math.Ceiling((float)size.Height * 2.4f + (float)num5);
							foreach (SegmentedItem item11 in items)
							{
								item11.PARENT = this;
								if (item11.HasIcon && item11.HasEmptyText)
								{
									item11.SetIconNoText(new Rectangle(rect.X, rect.Y + num12, rect.Width, num18), imgsize12);
								}
								else
								{
									item11.SetRectTop(new Rectangle(rect.X, rect.Y + num12, rect.Width, num18), imgsize12, num, num2, g, Font);
								}
								if (item11.Reverse)
								{
									list.Add(item11);
									num13 += item11.Rect.Height + num3;
								}
								else
								{
									num12 += item11.Rect.Height + num3;
								}
							}
							break;
						}
						case TAlignMini.Bottom:
						{
							int imgsize11 = (int)((float)size.Height * (iconratio ?? 1.8f));
							int num17 = (int)Math.Ceiling((float)size.Height * 2.4f + (float)num5);
							foreach (SegmentedItem item12 in items)
							{
								item12.PARENT = this;
								if (item12.HasIcon && item12.HasEmptyText)
								{
									item12.SetIconNoText(new Rectangle(rect.X, rect.Y + num12, rect.Width, num17), imgsize11);
								}
								else
								{
									item12.SetRectBottom(new Rectangle(rect.X, rect.Y + num12, rect.Width, num17), imgsize11, num, num2, g, Font);
								}
								if (item12.Reverse)
								{
									list.Add(item12);
									num13 += item12.Rect.Height + num3;
								}
								else
								{
									num12 += item12.Rect.Height + num3;
								}
							}
							break;
						}
						case TAlignMini.Left:
						{
							int imgsize10 = (int)((float)size.Height * (iconratio ?? 1.2f));
							int num16 = size.Height + num5;
							foreach (SegmentedItem item13 in items)
							{
								item13.PARENT = this;
								if (item13.HasIcon && item13.HasEmptyText)
								{
									item13.SetIconNoText(new Rectangle(rect.X, rect.Y + num12, rect.Width, num16), imgsize10);
								}
								else
								{
									item13.SetRectLeft(new Rectangle(rect.X, rect.Y + num12, rect.Width, num16), imgsize10, num2, num4);
								}
								if (item13.Reverse)
								{
									list.Add(item13);
									num13 += item13.Rect.Height + num3;
								}
								else
								{
									num12 += item13.Rect.Height + num3;
								}
							}
							break;
						}
						case TAlignMini.Right:
						{
							int imgsize9 = (int)((float)size.Height * (iconratio ?? 1.2f));
							int num15 = size.Height + num5;
							foreach (SegmentedItem item14 in items)
							{
								item14.PARENT = this;
								if (item14.HasIcon && item14.HasEmptyText)
								{
									item14.SetIconNoText(new Rectangle(rect.X, rect.Y + num12, rect.Width, num15), imgsize9);
								}
								else
								{
									item14.SetRectRight(new Rectangle(rect.X, rect.Y + num12, rect.Width, num15), imgsize9, num2, num4);
								}
								if (item14.Reverse)
								{
									list.Add(item14);
									num13 += item14.Rect.Height + num3;
								}
								else
								{
									num12 += item14.Rect.Height + num3;
								}
							}
							break;
						}
						default:
						{
							int num14 = size.Height + num4;
							foreach (SegmentedItem item15 in items)
							{
								item15.PARENT = this;
								item15.SetRectNone(new Rectangle(rect.X, rect.Y + num12, rect.Width, num14));
								if (item15.Reverse)
								{
									list.Add(item15);
									num13 += item15.Rect.Height + num3;
								}
								else
								{
									num12 += item15.Rect.Height + num3;
								}
							}
							break;
						}
						}
						Rect = new Rectangle(_rect.X, _rect.Y, _rect.Width, num12 - num3 + Margin.Vertical);
						if (list.Count > 0)
						{
							if (Rect.Height < _rect.Height && rightToLeft == RightToLeft.Yes)
							{
								int num19 = _rect.Bottom - Rect.Height;
								int num20 = _rect.Y;
								Rect.Y = num19;
								{
									foreach (SegmentedItem item16 in items)
									{
										if (item16.Reverse)
										{
											item16.SetOffset(0, num20 - item16.Rect.Y);
											num20 += item16.Rect.Height + num3;
										}
										else
										{
											item16.SetOffset(0, num19);
										}
									}
									return;
								}
							}
							int num21 = _rect.Bottom - num13;
							{
								foreach (SegmentedItem item17 in list)
								{
									item17.SetOffset(0, num21 - item17.Rect.Y);
									num21 += item17.Rect.Height + num3;
								}
								return;
							}
						}
						if (Rect.Height < _rect.Height && rightToLeft == RightToLeft.Yes)
						{
							int num22 = _rect.Bottom - Rect.Height;
							Rect.Y = num22;
							foreach (SegmentedItem item18 in items)
							{
								item18.SetOffset(0, num22);
							}
						}
					}
					else
					{
						int num23 = 0;
						int num24 = 0;
						switch (iconalign)
						{
						case TAlignMini.Top:
						{
							int num28 = (int)((float)size.Height * (iconratio ?? 1.8f));
							foreach (SegmentedItem item19 in items)
							{
								item19.PARENT = this;
								if (item19.HasIcon && item19.HasEmptyText)
								{
									item19.SetIconNoText(new Rectangle(rect.X + num23, rect.Y, num28 + num5, rect.Height), num28);
								}
								else
								{
									Size size6 = g.MeasureText(item19.Text, Font);
									item19.SetRectTop(new Rectangle(rect.X + num23, rect.Y, size6.Width + num5, rect.Height), num28, size6.Height, num2);
								}
								if (item19.Reverse)
								{
									list.Add(item19);
									num24 += item19.Rect.Width + num3;
								}
								else
								{
									num23 += item19.Rect.Width + num3;
								}
							}
							break;
						}
						case TAlignMini.Bottom:
						{
							int num27 = (int)((float)size.Height * (iconratio ?? 1.8f));
							foreach (SegmentedItem item20 in items)
							{
								item20.PARENT = this;
								if (item20.HasIcon && item20.HasEmptyText)
								{
									item20.SetIconNoText(new Rectangle(rect.X + num23, rect.Y, num27 + num5, rect.Height), num27);
								}
								else
								{
									Size size5 = g.MeasureText(item20.Text, Font);
									item20.SetRectBottom(new Rectangle(rect.X + num23, rect.Y, size5.Width + num5, rect.Height), num27, size5.Height, num2);
								}
								if (item20.Reverse)
								{
									list.Add(item20);
									num24 += item20.Rect.Width + num3;
								}
								else
								{
									num23 += item20.Rect.Width + num3;
								}
							}
							break;
						}
						case TAlignMini.Left:
						{
							int num26 = (int)((float)size.Height * (iconratio ?? 1.2f));
							foreach (SegmentedItem item21 in items)
							{
								item21.PARENT = this;
								if (item21.HasIcon && item21.HasEmptyText)
								{
									item21.SetIconNoText(new Rectangle(rect.X + num23, rect.Y, num26 + num5, rect.Height), num26);
								}
								else
								{
									Size size4 = g.MeasureText(item21.Text, Font);
									item21.SetRectLeft(new Rectangle(rect.X + num23, rect.Y, size4.Width + num26 + num2 + num5, rect.Height), num26, num2, num4);
								}
								if (item21.Reverse)
								{
									list.Add(item21);
									num24 += item21.Rect.Width + num3;
								}
								else
								{
									num23 += item21.Rect.Width + num3;
								}
							}
							break;
						}
						case TAlignMini.Right:
						{
							int num25 = (int)((float)size.Height * (iconratio ?? 1.2f));
							foreach (SegmentedItem item22 in items)
							{
								item22.PARENT = this;
								if (item22.HasIcon && item22.HasEmptyText)
								{
									item22.SetIconNoText(new Rectangle(rect.X + num23, rect.Y, num25 + num5, rect.Height), num25);
								}
								else
								{
									Size size3 = g.MeasureText(item22.Text, Font);
									item22.SetRectRight(new Rectangle(rect.X + num23, rect.Y, size3.Width + num25 + num2 + num5, rect.Height), num25, num2, num4);
								}
								if (item22.Reverse)
								{
									list.Add(item22);
									num24 += item22.Rect.Width + num3;
								}
								else
								{
									num23 += item22.Rect.Width + num3;
								}
							}
							break;
						}
						default:
							foreach (SegmentedItem item23 in items)
							{
								item23.PARENT = this;
								Size size2 = g.MeasureText(item23.Text, Font);
								item23.SetRectNone(new Rectangle(rect.X + num23, rect.Y, size2.Width + num5, rect.Height));
								if (item23.Reverse)
								{
									list.Add(item23);
									num24 += item23.Rect.Width + num3;
								}
								else
								{
									num23 += item23.Rect.Width + num3;
								}
							}
							break;
						}
						Rect = new Rectangle(_rect.X, _rect.Y, num23 - num3 + Margin.Horizontal, _rect.Height);
						if (list.Count > 0)
						{
							if (Rect.Width < _rect.Width && rightToLeft == RightToLeft.Yes)
							{
								int num29 = _rect.Right - Rect.Width;
								int num30 = _rect.X;
								Rect.X = num29;
								{
									foreach (SegmentedItem item24 in items)
									{
										if (item24.Reverse)
										{
											item24.SetOffset(num30 - item24.Rect.X, 0);
											num30 += item24.Rect.Width + num3;
										}
										else
										{
											item24.SetOffset(num29, 0);
										}
									}
									return;
								}
							}
							int num31 = _rect.Right - num24;
							{
								foreach (SegmentedItem item25 in list)
								{
									item25.SetOffset(num31 - item25.Rect.X, 0);
									num31 += item25.Rect.Width + num3;
								}
								return;
							}
						}
						if (Rect.Width < _rect.Width && rightToLeft == RightToLeft.Yes)
						{
							int num32 = _rect.Right - Rect.Width;
							Rect.X = num32;
							foreach (SegmentedItem item26 in items)
							{
								item26.SetOffset(num32, 0);
							}
						}
					}
				}
			});
			if (_select > -1)
			{
				SegmentedItem segmentedItem = items[_select];
				AnimationBarValue = (TabSelectRect = segmentedItem.Rect);
			}
		}
		if (print & canPrint)
		{
			Invalidate();
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		ChangeItems();
		base.OnSizeChanged(e);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		ChangeItems();
		this.AddListener();
	}

	protected override void OnMarginChanged(EventArgs e)
	{
		ChangeItems();
		base.OnMarginChanged(e);
	}

	protected override void OnPaddingChanged(EventArgs e)
	{
		ChangeItems();
		base.OnPaddingChanged(e);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		ChangeItems();
		base.OnFontChanged(e);
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG)
		{
			ChangeItems();
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (SegmentedItem item in items)
		{
			bool flag = item.Enabled && item.Rect.Contains(e.X, e.Y);
			if (item.Hover != flag)
			{
				item.Hover = flag;
				num2++;
			}
			if (item.Hover)
			{
				num++;
			}
			num3++;
		}
		SetCursor(num > 0);
		if (num2 > 0)
		{
			Invalidate();
		}
	}

	protected override bool OnMouseHover(int x, int y)
	{
		SegmentedItem itemMouseHover = GetItemMouseHover(x, y);
		if (tmp == itemMouseHover)
		{
			return false;
		}
		tmp = itemMouseHover;
		CloseTip();
		if (itemMouseHover == null)
		{
			return false;
		}
		OpenTip(itemMouseHover);
		return true;
	}

	private SegmentedItem? GetItemMouseHover(int x, int y)
	{
		if (items == null || items.Count == 0)
		{
			return null;
		}
		foreach (SegmentedItem item in items)
		{
			if (item.Enabled && item.Rect.Contains(x, y))
			{
				return item;
			}
		}
		return null;
	}

	public void CloseTip()
	{
		toolTip?.IClose();
		toolTip = null;
	}

	private void OpenTip(SegmentedItem it)
	{
		if (it.Tooltip == null)
		{
			CloseTip();
		}
		else if (toolTip == null)
		{
			toolTip = new TooltipForm(this, it.Rect, it.Tooltip, TooltipConfig ?? new TooltipConfig
			{
				Font = Font,
				ArrowAlign = TAlign.Right
			});
			toolTip.Show(this);
		}
		else if (toolTip.SetText(it.Rect, it.Tooltip))
		{
			CloseTip();
			OpenTip(it);
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		SetCursor(val: false);
		tmp = null;
		CloseTip();
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (SegmentedItem item in items)
		{
			if (item.Hover)
			{
				item.Hover = false;
				num++;
			}
		}
		if (num > 0)
		{
			Invalidate();
		}
	}

	protected override void OnLeave(EventArgs e)
	{
		base.OnLeave(e);
		SetCursor(val: false);
		tmp = null;
		CloseTip();
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (SegmentedItem item in items)
		{
			if (item.Hover)
			{
				item.Hover = false;
				num++;
			}
		}
		if (num > 0)
		{
			Invalidate();
		}
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		if (items == null || items.Count == 0)
		{
			OnNonItemClick(e);
			return;
		}
		for (int i = 0; i < items.Count; i++)
		{
			SegmentedItem segmentedItem = items[i];
			if (segmentedItem != null && segmentedItem.Enabled && segmentedItem.Rect.Contains(e.X, e.Y))
			{
				if (OnSelectIndexChanging(i))
				{
					SelectIndex = i;
				}
				OnItemClick(segmentedItem, e);
				return;
			}
		}
		OnNonItemClick(e);
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		if (AutoSize)
		{
			if (Vertical)
			{
				return new Size(base.GetPreferredSize(proposedSize).Width, Rect.Height);
			}
			return new Size(Rect.Width, base.GetPreferredSize(proposedSize).Height);
		}
		return base.GetPreferredSize(proposedSize);
	}

	protected override void OnResize(EventArgs e)
	{
		BeforeAutoSize();
		base.OnResize(e);
	}

	private bool BeforeAutoSize()
	{
		if (AutoSize)
		{
			if (InvokeRequired)
			{
				return ITask.Invoke(this, BeforeAutoSize);
			}
			if (Vertical)
			{
				int num = Rect.Height;
				if (Height == num)
				{
					return true;
				}
				Height = num;
			}
			else
			{
				int num2 = Rect.Width;
				if (Width == num2)
				{
					return true;
				}
				Width = num2;
			}
			return false;
		}
		return true;
	}
}
