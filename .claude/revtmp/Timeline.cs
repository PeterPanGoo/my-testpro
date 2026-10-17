using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Timeline 时间轴
/// </summary>
/// <remarks>垂直展示的时间流信息。</remarks>
[Description("Timeline 时间轴")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("ItemClick")]
public class Timeline : IControl, IEventListener
{
	private Color? fore;

	private TimelineItemCollection? items;

	private bool pauseLayout;

	/// <summary>
	/// 滚动条
	/// </summary>
	[Browsable(false)]
	public ScrollBar ScrollBar;

	private RectangleF[] splits = new RectangleF[0];

	private readonly FormatFlags stringFormatLeft = FormatFlags.VerticalCenter | FormatFlags.Left;

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
			}
		}
	}

	[Description("描述字体")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Font? FontDescription { get; set; }

	/// <summary>
	/// 间距
	/// </summary>
	[Description("间距")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? Gap { get; set; }

	/// <summary>
	/// 集合
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("Data")]
	public TimelineItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new TimelineItemCollection(this);
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
	/// 点击项时发生
	/// </summary>
	[Description("点击项时发生")]
	[Category("Behavior")]
	public event TimelineEventHandler? ItemClick;

	protected override void OnSizeChanged(EventArgs e)
	{
		ChangeList();
		base.OnSizeChanged(e);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		ChangeList();
		this.AddListener();
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
			Rectangle rect = ClientRectangle.DeflateRect(Padding);
			int y = rect.Y;
			this.GDI((Canvas g) =>
			{
				int num = g.MeasureString("龍Qq", Font).Height;
				float num2 = (float)num * 0.136f;
				float num3 = num2 * 0.666f;
				float num4 = num3 * 2f;
				int num5 = (int)Math.Round(8f * Dpi);
				int num6 = (int)Math.Round((double)num * 1.1);
				int num7 = (int)Math.Round((double)num * 0.846);
				int num8 = (int)Math.Round(((double?)((float?)Gap * Dpi)) ?? ((double)num * 0.91));
				int num9 = (int)Math.Round((double)num * 0.636);
				int num10 = rect.Width - num9 - num7 - num6 * 2;
				y += num6;
				List<RectangleF> list = new List<RectangleF>(items.Count);
				int num11 = 0;
				Font font = FontDescription ?? Font;
				int num12 = num5 * 2;
				foreach (TimelineItem item in items)
				{
					item.PARENT = this;
					item.pen_w = num2;
					if (item.Visible)
					{
						Size size = g.MeasureText(item.Text, Font, num10);
						int num13 = y;
						int num14 = size.Height;
						item.ico_rect = new Rectangle(rect.X + num6, y + (num - num9) / 2, num9, num9);
						item.txt_rect = new Rectangle(item.ico_rect.Right + num7, y, size.Width, size.Height);
						if (!string.IsNullOrEmpty(item.Description))
						{
							Size size2 = g.MeasureText(item.Description, font, num10);
							item.description_rect = new Rectangle(item.txt_rect.X, item.txt_rect.Bottom + num5, size2.Width, size2.Height);
							y += num5 * 2 + size2.Height;
							num14 += size2.Height + num5;
						}
						item.rect = new Rectangle(item.ico_rect.X - num5, num13 - num5, item.txt_rect.Width + num9 + num7 + num12, num14 + num12);
						y += size.Height + num8;
						if (num11 > 0)
						{
							TimelineItem timelineItem = items[num11 - 1];
							if (timelineItem != null)
							{
								list.Add(new RectangleF((float)item.ico_rect.X + ((float)num9 - num3) / 2f, (float)timelineItem.ico_rect.Bottom + num4, num3, (float)(item.ico_rect.Y - timelineItem.ico_rect.Bottom) - num4 * 2f));
							}
						}
					}
					num11++;
				}
				splits = list.ToArray();
				y = y - num8 + num6;
			});
			ScrollBar.SetVrSize(y, rect);
		}
		if (print & canPrint)
		{
			Invalidate();
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		ScrollBar.MouseWheel(e);
		base.OnMouseWheel(e);
	}

	public Timeline()
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
		canvas.TranslateTransform(0f, -ScrollBar.Value);
		Color color = fore ?? Colour.Text.Get(ColorScheme, "Timeline", Name);
		using (SolidBrush brush = new SolidBrush(Colour.Split.Get(ColorScheme, "Timeline", Name)))
		{
			RectangleF[] array = splits;
			foreach (RectangleF rect in array)
			{
				canvas.Fill(brush, rect);
			}
		}
		Font font = FontDescription ?? Font;
		using (SolidBrush brush2 = new SolidBrush(color))
		{
			using SolidBrush brush3 = new SolidBrush(Colour.TextTertiary.Get(ColorScheme, "Timeline", Name));
			using SolidBrush brush4 = new SolidBrush(Colour.BgBase.Get(ColorScheme, "Timeline", Name));
			foreach (TimelineItem item in items)
			{
				if (item.Visible)
				{
					canvas.DrawText(item.Text, Font, brush2, item.txt_rect, stringFormatLeft);
					canvas.DrawText(item.Description, font, item.DescriptionFore, brush3, item.description_rect, stringFormatLeft);
					if (PaintIcon(canvas, item, color))
					{
						Color color2 = (item.Fill.HasValue ? item.Fill.Value : (item.Type switch
						{
							TTypeMini.Error => Colour.Error.Get(ColorScheme, "Timeline", Name), 
							TTypeMini.Success => Colour.Success.Get(ColorScheme, "Timeline", Name), 
							TTypeMini.Info => Colour.Info.Get(ColorScheme, "Timeline", Name), 
							TTypeMini.Warn => Colour.Warning.Get(ColorScheme, "Timeline", Name), 
							TTypeMini.Default => Colour.TextQuaternary.Get(ColorScheme, "Timeline", Name), 
							_ => Colour.Primary.Get(ColorScheme, "Timeline", Name), 
						}));
						canvas.FillEllipse(brush4, item.ico_rect);
						canvas.DrawEllipse(color2, item.pen_w, item.ico_rect);
					}
				}
			}
		}
		canvas.ResetTransform();
		ScrollBar.Paint(canvas, ColorScheme);
		base.OnDraw(e);
	}

	private bool PaintIcon(Canvas g, TimelineItem it, Color fore)
	{
		int num = 0;
		if (it.Icon != null)
		{
			g.Image(it.Icon, it.ico_rect);
			num++;
		}
		if (it.IconSvg != null && g.Svg(it.IconSvg, it.ico_rect, fore))
		{
			num++;
		}
		return num == 0;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (ScrollBar.MouseDown(e.X, e.Y))
		{
			OnTouchDown(e.X, e.Y);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		ScrollBar.MouseUp();
		OnTouchUp();
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (!ScrollBar.MouseMove(e.X, e.Y) || items == null || items.Count == 0 || ItemClick == null)
		{
			return;
		}
		if (OnTouchMove(e.X, e.Y))
		{
			for (int i = 0; i < items.Count; i++)
			{
				TimelineItem timelineItem = items[i];
				if (timelineItem != null && timelineItem.rect.Contains(e.X, e.Y + ScrollBar.Value))
				{
					SetCursor(val: true);
					return;
				}
			}
		}
		SetCursor(val: false);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		ScrollBar.Leave();
	}

	protected override void OnLeave(EventArgs e)
	{
		base.OnLeave(e);
		ScrollBar.Leave();
	}

	protected override bool OnTouchScrollX(int x, int y, int value)
	{
		return ScrollBar.MouseWheelXCore(value);
	}

	protected override bool OnTouchScrollY(int x, int y, int value)
	{
		return ScrollBar.MouseWheelYCore(value);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		if (items == null || items.Count == 0 || ItemClick == null)
		{
			return;
		}
		for (int i = 0; i < items.Count; i++)
		{
			TimelineItem timelineItem = items[i];
			if (timelineItem != null && timelineItem.rect.Contains(e.X, e.Y + ScrollBar.Value))
			{
				ItemClick(this, new TimelineItemEventArgs(timelineItem, e));
				break;
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		ScrollBar.Dispose();
		base.Dispose(disposing);
	}
}
