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
/// Breadcrumb 面包屑
/// </summary>
/// <remarks>显示当前页面在系统层级结构中的位置，并能向上返回。</remarks>
[Description("Breadcrumb 面包屑")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("ItemClick")]
public class Breadcrumb : IControl, IEventListener
{
	private int gap = 12;

	private Color? fore;

	private int radius = 4;

	private string separator = "/";

	private BreadcrumbItemCollection? items;

	private readonly FormatFlags s_f = FormatFlags.Center | FormatFlags.NoWrapEllipsis;

	private bool pauseLayout;

	private Rectangle[] hs = new Rectangle[0];

	[Description("间距")]
	[Category("Appearance")]
	[DefaultValue(12)]
	public int Gap
	{
		get
		{
			return gap;
		}
		set
		{
			if (gap != value)
			{
				gap = value;
				ChangeItems(print: true);
				OnPropertyChanged("Gap");
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
	/// 激活文字颜色
	/// </summary>
	[Description("激活文字颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeActive { get; set; }

	/// <summary>
	/// 圆角
	/// </summary>
	[Description("圆角")]
	[Category("Appearance")]
	[DefaultValue(4)]
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
	/// 获取或设置分割符
	/// </summary>
	[Description("获取或设置分割符")]
	[Category("Appearance")]
	[DefaultValue("/")]
	public string Separator
	{
		get
		{
			return separator;
		}
		set
		{
			if (!(separator == value))
			{
				separator = value;
				Invalidate();
				OnPropertyChanged("Separator");
			}
		}
	}

	/// <summary>
	/// 获取列表中所有列表项的集合
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("Data")]
	[DefaultValue(null)]
	public BreadcrumbItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new BreadcrumbItemCollection(this);
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
					ChangeItems(print: true);
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	/// <summary>
	/// 点击项时发生
	/// </summary>
	[Description("点击项时发生")]
	[Category("Behavior")]
	public event BreadcrumbItemEventHandler? ItemClick;

	/// <summary>
	/// 空白项目点击事件（包含鼠标信息）
	/// </summary>
	[Description("空白项目点击事件")]
	[Category("Behavior")]
	public event MouseEventHandler? NonItemClick;

	protected virtual void OnItemClick(BreadcrumbItem item, MouseEventArgs e)
	{
		ItemClick?.Invoke(this, new BreadcrumbItemEventArgs(item, e));
	}

	protected virtual void OnNonItemClick(MouseEventArgs e)
	{
		NonItemClick?.Invoke(this, e);
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

	protected override void OnDraw(DrawEventArgs e)
	{
		if (items == null || items.Count == 0)
		{
			base.OnDraw(e);
			return;
		}
		Canvas canvas = e.Canvas;
		float num = (float)radius * Dpi;
		using (SolidBrush brush = new SolidBrush(fore ?? Colour.TextSecondary.Get(ColorScheme, "Breadcrumb", Name)))
		{
			using SolidBrush brush2 = new SolidBrush(ForeActive ?? Colour.Text.Get(ColorScheme, "Breadcrumb", Name));
			for (int i = 0; i < hs.Length; i++)
			{
				canvas.DrawText(items[i].Separator ?? separator, Font, brush, hs[i], s_f);
			}
			for (int j = 0; j < items.Count; j++)
			{
				BreadcrumbItem breadcrumbItem = items[j];
				if (j == items.Count - 1)
				{
					PaintItemActive(canvas, breadcrumbItem, brush2);
				}
				else if (breadcrumbItem.Hover)
				{
					using (GraphicsPath path = breadcrumbItem.Rect.RoundPath(num))
					{
						canvas.Fill(Colour.FillSecondary.Get(ColorScheme, "Breadcrumb", Name), path);
					}
					PaintItemActive(canvas, breadcrumbItem, brush2);
				}
				else
				{
					PaintItem(canvas, breadcrumbItem, brush);
				}
			}
		}
		base.OnDraw(e);
	}

	private void PaintItemActive(Canvas g, BreadcrumbItem it, SolidBrush brush)
	{
		if (it.ForeActive.HasValue)
		{
			PaintImg(g, it, it.ForeActive.Value, it.IconSvg, it.Icon);
			g.DrawText(it.Text, Font, it.ForeActive.Value, it.RectText, s_f);
		}
		else if (it.Fore.HasValue)
		{
			PaintImg(g, it, it.Fore.Value, it.IconSvg, it.Icon);
			g.DrawText(it.Text, Font, it.Fore.Value, it.RectText, s_f);
		}
		else
		{
			PaintImg(g, it, brush.Color, it.IconSvg, it.Icon);
			g.DrawText(it.Text, Font, brush, it.RectText, s_f);
		}
	}

	private void PaintItem(Canvas g, BreadcrumbItem it, SolidBrush brush)
	{
		if (it.Fore.HasValue)
		{
			PaintImg(g, it, it.Fore.Value, it.IconSvg, it.Icon);
			g.DrawText(it.Text, Font, it.Fore.Value, it.RectText, s_f);
		}
		else
		{
			PaintImg(g, it, brush.Color, it.IconSvg, it.Icon);
			g.DrawText(it.Text, Font, brush, it.RectText, s_f);
		}
	}

	private bool PaintImg(Canvas g, BreadcrumbItem it, Color color, string? svg, Image? bmp)
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

	internal void ChangeItems(bool print = false)
	{
		if (CanLayout(out var canPrint))
		{
			Rectangle rect = ClientRectangle.PaddingRect(Padding).PaddingRect(Margin);
			hs = this.GDI((Canvas g) =>
			{
				List<Rectangle> list = new List<Rectangle>(items.Count);
				Size size = g.MeasureString("龍Qq", Font);
				int num = (int)(4f * Dpi);
				int num2 = num * 2;
				int num3 = (int)((float)size.Height * 0.8f);
				int num4 = size.Height + num;
				int num5 = rect.Y + (rect.Height - num4) / 2;
				int num6 = rect.Y + (rect.Height - num3) / 2;
				int num7 = (int)((float)gap * Dpi);
				int num8 = 0;
				int num9 = 0;
				foreach (BreadcrumbItem item in items)
				{
					item.PARENT = this;
					if (item.Text == null || string.IsNullOrEmpty(item.Text))
					{
						Rectangle rectangle = new Rectangle(rect.X + num8, num5, num3 + num2, num4);
						if (item.HasIcon)
						{
							Rectangle rect2 = (item.RectText = rectangle);
							item.Rect = rect2;
							item.RectImg = new Rectangle(rectangle.X + num, num6, num3, num3);
						}
						else
						{
							Rectangle rect2 = (item.RectText = rectangle);
							item.Rect = rect2;
						}
					}
					else
					{
						Size size2 = g.MeasureText(item.Text, Font);
						if (item.HasIcon)
						{
							Rectangle rectangle4 = (item.Rect = new Rectangle(rect.X + num8, num5, num3 + num + size2.Width + num2, num4));
							item.RectImg = new Rectangle(rectangle4.X + num, num6, num3, num3);
							item.RectText = new Rectangle(item.RectImg.Right + num, rectangle4.Y, size2.Width, rectangle4.Height);
						}
						else
						{
							Rectangle rect2 = (item.RectText = new Rectangle(rect.X + num8, num5, size2.Width + num2, num4));
							item.Rect = rect2;
						}
					}
					num8 += item.Rect.Width + num7;
					if (num9 > 0)
					{
						list.Add(new Rectangle(rect.X + num9 - num7, num5, num7, num4));
					}
					num9 = num8;
				}
				return list.ToArray();
			});
		}
		if (print & canPrint)
		{
			Invalidate();
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
		foreach (BreadcrumbItem item in items)
		{
			bool flag = item.Rect.Contains(e.X, e.Y);
			if (item.Hover != flag)
			{
				item.Hover = flag;
				num2++;
			}
			if (item.Hover)
			{
				num++;
			}
		}
		SetCursor(num > 0);
		if (num2 > 0)
		{
			Invalidate();
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		SetCursor(val: false);
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (BreadcrumbItem item in items)
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
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (BreadcrumbItem item in items)
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
			return;
		}
		for (int i = 0; i < items.Count; i++)
		{
			BreadcrumbItem breadcrumbItem = items[i];
			if (breadcrumbItem != null && breadcrumbItem.Rect.Contains(e.X, e.Y))
			{
				OnItemClick(breadcrumbItem, e);
				return;
			}
		}
		OnNonItemClick(e);
	}
}
