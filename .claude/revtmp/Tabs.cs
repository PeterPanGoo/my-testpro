using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Tabs 标签页
/// </summary>
/// <remarks>选项卡切换组件。</remarks>
[Description("Tabs 标签页")]
[ToolboxItem(true)]
[DefaultEvent("SelectedIndexChanged")]
[DefaultProperty("Pages")]
[Designer(typeof(TabControlDesigner))]
public class Tabs : IControl, IEventListener
{
	/// <summary>
	/// 卡片样式
	/// </summary>
	public class StyleCard : IStyle
	{
		private Tabs? owner;

		private int radius = 6;

		private int bordersize = 1;

		private Color? border;

		private Color? fill;

		private int gap = 2;

		private bool closable;

		private TabPageRect[] rects = new TabPageRect[0];

		private int onetmp;

		/// <summary>
		/// 卡片圆角
		/// </summary>
		[Description("卡片圆角")]
		[Category("卡片")]
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
					owner?.Invalidate();
				}
			}
		}

		/// <summary>
		/// 边框大小
		/// </summary>
		[Description("边框大小")]
		[Category("卡片")]
		[DefaultValue(1)]
		public int Border
		{
			get
			{
				return bordersize;
			}
			set
			{
				if (bordersize != value)
				{
					bordersize = value;
					owner?.LoadLayout(print: true);
				}
			}
		}

		/// <summary>
		/// 卡片边框颜色
		/// </summary>
		[Description("卡片边框颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? BorderColor
		{
			get
			{
				return border;
			}
			set
			{
				if (!(border == value))
				{
					border = value;
					owner?.Invalidate();
				}
			}
		}

		/// <summary>
		/// 卡片边框激活颜色
		/// </summary>
		[Description("卡片边框激活颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? BorderActive { get; set; }

		/// <summary>
		/// 卡片颜色
		/// </summary>
		[Description("卡片颜色")]
		[Category("卡片")]
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
					owner?.Invalidate();
				}
			}
		}

		/// <summary>
		/// 卡片悬停颜色
		/// </summary>
		[Description("卡片悬停颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? FillHover { get; set; }

		/// <summary>
		/// 卡片激活颜色
		/// </summary>
		[Description("卡片激活颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? FillActive { get; set; }

		/// <summary>
		/// 卡片间距
		/// </summary>
		[Description("卡片间距")]
		[Category("卡片")]
		[DefaultValue(2)]
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
					owner?.LoadLayout(print: true);
				}
			}
		}

		/// <summary>
		/// 可关闭
		/// </summary>
		[Description("可关闭")]
		[Category("卡片")]
		[DefaultValue(false)]
		public bool Closable
		{
			get
			{
				return closable;
			}
			set
			{
				if (closable != value)
				{
					closable = value;
					owner?.LoadLayout(print: true);
				}
			}
		}

		public StyleCard()
		{
		}

		public StyleCard(Tabs tabs)
		{
			owner = tabs;
		}

		public void LoadLayout(Tabs tabs, Rectangle rect, TabCollection items)
		{
			owner = tabs;
			rects = tabs.GDI((Canvas g) =>
			{
				int num = (int)((float)tabs.Gap * owner.Dpi);
				int num2 = num * 2;
				int num3 = 0;
				int num4 = (int)((float)Gap * owner.Dpi);
				List<TabPageRect> list = new List<TabPageRect>(items.Count);
				Dictionary<TabPage, Size> dir = GetDir(tabs, g, items, num, num2, out var ico_size, out var ico_gap, out var close_size, out var sizewh, out var rw);
				int? num5 = (tabs.TextCenter ? new int?(rw) : ((int?)null));
				switch (tabs.Alignment)
				{
				case TabAlignment.Bottom:
				{
					int y5 = rect.Bottom - sizewh;
					foreach (KeyValuePair<TabPage, Size> item in dir)
					{
						if (item.Key.Visible)
						{
							bool flag8 = closable && !item.Key.ReadOnly;
							int num19 = item.Value.Width;
							if (item.Key.HasIcon && num19 == 0)
							{
								num19 = ico_size + ico_gap;
								if (flag8)
								{
									num19 += close_size + ico_gap;
								}
							}
							else
							{
								if (item.Key.HasIcon)
								{
									num19 += ico_size + ico_gap * 2;
								}
								if (flag8)
								{
									num19 += close_size + ico_gap;
								}
							}
							Rectangle rect5 = new Rectangle(rect.X + num3, y5, num19 + num2, sizewh);
							if (flag8)
							{
								if (item.Key.HasIcon)
								{
									list.Add(new TabPageRect(rect5, item.Value, num, ico_size, close_size, ico_gap, left: true));
								}
								else
								{
									list.Add(new TabPageRect(rect5, test: false, item.Value, num, close_size, ico_gap, left: true));
								}
							}
							else if (item.Key.HasIcon)
							{
								list.Add(new TabPageRect(rect5, item.Value, num, ico_size, ico_gap, left: true));
							}
							else
							{
								list.Add(new TabPageRect(rect5, item.Value, num, left: true));
							}
							item.Key.SetRect(rect5);
							num3 += rect5.Width + num4;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					num3 -= num4;
					tabs.SetPadding(0, 0, 0, sizewh + tabs.Margin.Top);
					owner.scroll_max = num3 - rect.Width;
					owner.scroll_show = num3 > rect.Width;
					break;
				}
				case TabAlignment.Left:
					foreach (KeyValuePair<TabPage, Size> item2 in dir)
					{
						if (item2.Key.Visible)
						{
							bool flag2 = closable && !item2.Key.ReadOnly;
							int height = item2.Value.Height;
							int height2;
							if (!owner.IsRotate)
							{
								height2 = ((!item2.Key.HasIcon || height != 0) ? (height + num) : (ico_size + num));
							}
							else
							{
								int num7 = (item2.Key.HasIcon ? (ico_size + ico_gap) : 0);
								int num8 = (flag2 ? (close_size + ico_gap) : 0);
								height2 = num + num7 + height + num8 + num;
							}
							Rectangle rect3 = new Rectangle(rect.X, rect.Y + num3, sizewh, height2);
							TabPageRect tabPageRect;
							if (!owner.IsRotate)
							{
								tabPageRect = (flag2 ? ((!item2.Key.HasIcon) ? new TabPageRect(rect3, test: false, item2.Value, num, close_size, ico_gap, left: true, num5, rw) : new TabPageRect(rect3, item2.Value, num, ico_size, close_size, ico_gap, left: true, num5, rw)) : ((!item2.Key.HasIcon) ? new TabPageRect(rect3, item2.Value, num, left: true, num5) : new TabPageRect(rect3, item2.Value, num, ico_size, ico_gap, left: true, num5)));
							}
							else
							{
								tabPageRect = new TabPageRect(rect3, item2.Value, num, left: true, num5);
								bool flag3 = owner.Rotate == TRotate.Clockwise_90;
								bool hasIcon = item2.Key.HasIcon;
								bool flag4 = flag2;
								if (hasIcon)
								{
									int x = rect3.X + (rect3.Width - ico_size) / 2;
									int y = (flag3 ? (rect3.Y + num) : (rect3.Bottom - num - ico_size));
									tabPageRect.Rect_Ico = new Rectangle(x, y, ico_size, ico_size);
								}
								else
								{
									tabPageRect.Rect_Ico = Rectangle.Empty;
								}
								if (flag4)
								{
									int num9 = close_size;
									int x2 = rect3.X + (rect3.Width - num9) / 2;
									int y2 = (flag3 ? (rect3.Bottom - num - num9) : (rect3.Y + num));
									tabPageRect.Rect_Close = new Rectangle(x2, y2, num9, num9);
								}
								else
								{
									tabPageRect.Rect_Close = Rectangle.Empty;
								}
								int num10 = rect3.Y + num;
								int num11 = rect3.Bottom - num;
								if (hasIcon & flag3)
								{
									num10 += ico_size + ico_gap;
								}
								if (flag4 && !flag3)
								{
									num10 += close_size + ico_gap;
								}
								if (flag4 & flag3)
								{
									num11 -= close_size + ico_gap;
								}
								if (hasIcon && !flag3)
								{
									num11 -= ico_size + ico_gap;
								}
								int height3 = Math.Max(0, num11 - num10);
								int num12 = num5 ?? item2.Value.Width;
								int x3 = rect3.X + Math.Max(0, (rect3.Width - num12) / 2);
								tabPageRect.Rect_Text = new Rectangle(x3, num10, num12, height3);
							}
							list.Add(tabPageRect);
							item2.Key.SetRect(rect3);
							num3 += rect3.Height + num4;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					num3 -= num4;
					tabs.SetPadding(sizewh + tabs.Margin.Left, 0, 0, 0);
					owner.scroll_max = num3 - rect.Height;
					owner.scroll_show = num3 > rect.Height;
					break;
				case TabAlignment.Right:
				{
					int x4 = rect.Right - sizewh;
					foreach (KeyValuePair<TabPage, Size> item3 in dir)
					{
						if (item3.Key.Visible)
						{
							bool flag5 = closable && !item3.Key.ReadOnly;
							int height4 = item3.Value.Height;
							int height5;
							if (!owner.IsRotate)
							{
								height5 = ((!item3.Key.HasIcon || height4 != 0) ? (height4 + num) : (ico_size + num));
							}
							else
							{
								int num13 = (item3.Key.HasIcon ? (ico_size + ico_gap) : 0);
								int num14 = (flag5 ? (close_size + ico_gap) : 0);
								height5 = num + num13 + height4 + num14 + num;
							}
							Rectangle rect4 = new Rectangle(x4, rect.Y + num3, sizewh, height5);
							TabPageRect tabPageRect2;
							if (!owner.IsRotate)
							{
								tabPageRect2 = (flag5 ? ((!item3.Key.HasIcon) ? new TabPageRect(rect4, test: false, item3.Value, num, close_size, ico_gap, left: false, num5, rw) : new TabPageRect(rect4, item3.Value, num, ico_size, close_size, ico_gap, left: false, num5, rw)) : ((!item3.Key.HasIcon) ? new TabPageRect(rect4, item3.Value, num, left: false, num5) : new TabPageRect(rect4, item3.Value, num, ico_size, ico_gap, left: false, num5)));
							}
							else
							{
								tabPageRect2 = new TabPageRect(rect4, item3.Value, num, left: true, num5);
								bool flag6 = owner.Rotate == TRotate.Clockwise_90;
								bool hasIcon2 = item3.Key.HasIcon;
								bool flag7 = flag5;
								if (hasIcon2)
								{
									int x5 = rect4.X + (rect4.Width - ico_size) / 2;
									int y3 = (flag6 ? (rect4.Y + num) : (rect4.Bottom - num - ico_size));
									tabPageRect2.Rect_Ico = new Rectangle(x5, y3, ico_size, ico_size);
								}
								else
								{
									tabPageRect2.Rect_Ico = Rectangle.Empty;
								}
								if (flag7)
								{
									int num15 = close_size;
									int x6 = rect4.X + (rect4.Width - num15) / 2;
									int y4 = (flag6 ? (rect4.Bottom - num - num15) : (rect4.Y + num));
									tabPageRect2.Rect_Close = new Rectangle(x6, y4, num15, num15);
								}
								else
								{
									tabPageRect2.Rect_Close = Rectangle.Empty;
								}
								int num16 = rect4.Y + num;
								int num17 = rect4.Bottom - num;
								if (hasIcon2 & flag6)
								{
									num16 += ico_size + ico_gap;
								}
								if (flag7 && !flag6)
								{
									num16 += close_size + ico_gap;
								}
								if (flag7 & flag6)
								{
									num17 -= close_size + ico_gap;
								}
								if (hasIcon2 && !flag6)
								{
									num17 -= ico_size + ico_gap;
								}
								int height6 = Math.Max(0, num17 - num16);
								int num18 = num5 ?? item3.Value.Width;
								int x7 = rect4.X + Math.Max(0, (rect4.Width - num18) / 2);
								tabPageRect2.Rect_Text = new Rectangle(x7, num16, num18, height6);
							}
							list.Add(tabPageRect2);
							item3.Key.SetRect(rect4);
							num3 += rect4.Height + num4;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					num3 -= num4;
					tabs.SetPadding(0, 0, sizewh + tabs.Margin.Left, 0);
					owner.scroll_max = num3 - rect.Height;
					owner.scroll_show = num3 > rect.Height;
					break;
				}
				default:
					foreach (KeyValuePair<TabPage, Size> item4 in dir)
					{
						if (item4.Key.Visible)
						{
							bool flag = closable && !item4.Key.ReadOnly;
							int num6 = item4.Value.Width;
							if (item4.Key.HasIcon && num6 == 0)
							{
								num6 = ico_size + ico_gap;
								if (flag)
								{
									num6 += close_size + ico_gap;
								}
							}
							else
							{
								if (item4.Key.HasIcon)
								{
									num6 += ico_size + ico_gap * 2;
								}
								if (flag)
								{
									num6 += close_size + ico_gap;
								}
							}
							Rectangle rect2 = new Rectangle(rect.X + num3, rect.Y, num6 + num2, sizewh);
							if (flag)
							{
								if (item4.Key.HasIcon)
								{
									list.Add(new TabPageRect(rect2, item4.Value, num, ico_size, close_size, ico_gap, left: true));
								}
								else
								{
									list.Add(new TabPageRect(rect2, test: false, item4.Value, num, close_size, ico_gap, left: true));
								}
							}
							else if (item4.Key.HasIcon)
							{
								list.Add(new TabPageRect(rect2, item4.Value, num, ico_size, ico_gap, left: true));
							}
							else
							{
								list.Add(new TabPageRect(rect2, item4.Value, num, left: true));
							}
							item4.Key.SetRect(rect2);
							num3 += rect2.Width + num4;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					num3 -= num4;
					tabs.SetPadding(0, sizewh + tabs.Margin.Top, 0, 0);
					owner.scroll_max = num3 - rect.Width;
					owner.scroll_show = num3 > rect.Width;
					break;
				}
				if (owner.scroll_show)
				{
					owner.scroll_max += owner.SizeExceed(owner.ClientRectangle, list[0].Rect, list[list.Count - 1].Rect);
				}
				else
				{
					Tabs? tabs2 = owner;
					int scroll_x = (owner.scroll_y = 0);
					tabs2.scroll_x = scroll_x;
					if (tabs.centered)
					{
						TabAlignment alignment = tabs.Alignment;
						if ((uint)alignment > 1u && (uint)(alignment - 2) <= 1u)
						{
							int y6 = (rect.Height - num3) / 2;
							foreach (KeyValuePair<TabPage, Size> item5 in dir)
							{
								item5.Key.SetOffset(0, y6);
							}
							foreach (TabPageRect item6 in list)
							{
								item6.Rect.Offset(0, y6);
								item6.Rect_Text.Offset(0, y6);
								item6.Rect_Ico.Offset(0, y6);
								item6.Rect_Close.Offset(0, y6);
							}
						}
						else
						{
							int x8 = (rect.Width - num3) / 2;
							foreach (KeyValuePair<TabPage, Size> item7 in dir)
							{
								item7.Key.SetOffset(x8, 0);
							}
							foreach (TabPageRect item8 in list)
							{
								item8.Rect.Offset(x8, 0);
								item8.Rect_Text.Offset(x8, 0);
								item8.Rect_Ico.Offset(x8, 0);
								item8.Rect_Close.Offset(x8, 0);
							}
						}
					}
				}
				return list.ToArray();
			});
		}

		private Dictionary<TabPage, Size> GetDir(Tabs owner, Canvas g, TabCollection items, int gap, int gap2, out int ico_size, out int ico_gap, out int close_size, out int sizewh, out int rw)
		{
			sizewh = (rw = 0);
			Size size = g.MeasureString("龍Qq", owner.Font);
			Dictionary<TabPage, Size> dictionary = new Dictionary<TabPage, Size>(items.Count);
			int num = 0;
			int? num2 = null;
			foreach (TabPage item in items)
			{
				Size value = g.MeasureString(item.Text, owner.Font);
				if (owner.IsRotate)
				{
					value = new Size(value.Height, value.Width);
				}
				dictionary.Add(item, value);
				if (rw < value.Width)
				{
					rw = value.Width;
				}
				if (item.Visible && !num2.HasValue)
				{
					num2 = num;
				}
				num++;
			}
			onetmp = num2.GetValueOrDefault();
			ico_size = (int)((float)size.Height * owner.iconratio);
			ico_gap = (int)((float)size.Height * owner.icongap);
			close_size = (int)((float)size.Height * (owner.iconratio * 0.8f));
			TabAlignment alignment = owner.Alignment;
			if ((uint)alignment > 1u && (uint)(alignment - 2) <= 1u)
			{
				if (closable)
				{
					foreach (KeyValuePair<TabPage, Size> item2 in dictionary)
					{
						if (item2.Key.Visible)
						{
							int num3;
							if (item2.Key.HasIcon)
							{
								num3 = (owner.IsRotate ? (Math.Max(rw, ico_size) + gap2) : (item2.Value.Width + gap2 + ico_size + ico_gap));
							}
							else
							{
								num3 = (owner.IsRotate ? (rw + gap2) : (item2.Value.Width + gap2));
							}
							num3 += close_size + ico_gap;
							if (sizewh < num3)
							{
								sizewh = num3;
							}
						}
					}
				}
				else
				{
					foreach (KeyValuePair<TabPage, Size> item3 in dictionary)
					{
						if (item3.Key.Visible)
						{
							int num4;
							if (item3.Key.HasIcon)
							{
								num4 = (owner.IsRotate ? (Math.Max(rw, ico_size) + gap2) : (item3.Value.Width + gap2 + ico_size + ico_gap));
							}
							else
							{
								num4 = (owner.IsRotate ? (rw + gap2) : (item3.Value.Width + gap2));
							}
							if (sizewh < num4)
							{
								sizewh = num4;
							}
						}
					}
				}
			}
			else
			{
				foreach (KeyValuePair<TabPage, Size> item4 in dictionary)
				{
					if (item4.Key.Visible)
					{
						int num5 = ((item4.Value.Height > 0) ? (item4.Value.Height + gap) : (ico_size + gap));
						if (sizewh < num5)
						{
							sizewh = num5;
						}
					}
				}
			}
			return owner.HandItemSize(dictionary, ref sizewh);
		}

		public void Paint(Tabs owner, Canvas g, TabCollection items)
		{
			if (rects.Length != items.Count)
			{
				return;
			}
			using SolidBrush solidBrush = new SolidBrush(owner.ForeColor ?? Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush brush = new SolidBrush(owner.Fill ?? Colour.Primary.Get(owner.ColorScheme, "Tabs", owner.Name));
			Rectangle clientRectangle = owner.ClientRectangle;
			int num = (int)((float)Radius * owner.Dpi);
			int num2 = (int)((float)bordersize * owner.Dpi);
			int num3 = num2 * 6;
			int num4 = num2 / 2;
			TabPage tabPage = null;
			int selectedIndex = owner.SelectedIndex;
			Rectangle rect = rects[onetmp].Rect;
			switch (owner.Alignment)
			{
			case TabAlignment.Bottom:
			{
				int num9 = rect.Height + rect.X;
				Rectangle rectangle4 = new Rectangle(clientRectangle.X, clientRectangle.Bottom - num9, clientRectangle.Width, num9);
				if (owner.scroll_show)
				{
					g.SetClip(owner.PaintExceedPre(rectangle4, rect));
					g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
				}
				else
				{
					g.SetClip(rectangle4);
				}
				tabPage = PaintTABS(owner, g, items, num, solidBrush, num2, TL: false, TR: false, BR: true, BL: true);
				g.ResetTransform();
				if (tabPage == null)
				{
					break;
				}
				GraphicsState state4 = g.Save();
				if (owner.pageMove == tabPage)
				{
					g.TranslateTransform(-owner.offsetx, -owner.offsety);
				}
				Rectangle rect5 = tabPage.Rect;
				if (num2 > 0)
				{
					float num10 = rect5.Y + num4;
					PaintTABS(rect_card: new RectangleF(rect5.X + num4, rect5.Y - num4, rect5.Width - num2, rect5.Height + num4), rect_line: new Rectangle(rect5.X - num2, rect5.Y + num2, rect5.Width + num3, rect5.Height + num2), owner: owner, gg: g, rect_t: clientRectangle, rect_page: rect5, radius: num, x: clientRectangle.X, y: num10, x2: clientRectangle.Right, y2: num10, bor: num2, TL: false, TR: false, BR: true, BL: true);
					if (owner.scroll_show)
					{
						g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
					}
				}
				else
				{
					if (owner.scroll_show)
					{
						g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
					}
					using GraphicsPath path4 = rect5.RoundPath(num, roundTL: false, roundTR: false, roundBR: true, roundBL: true);
					using SolidBrush brush5 = new SolidBrush(FillActive ?? Colour.BgContainer.Get(owner.ColorScheme, "Tabs", owner.Name));
					g.Fill(brush5, path4);
				}
				PaintText(g, rects[selectedIndex], owner, tabPage, brush);
				g.Restore(state4);
				break;
			}
			case TabAlignment.Left:
			{
				Rectangle rectangle2 = new Rectangle(clientRectangle.X, clientRectangle.Y, rect.Right, clientRectangle.Height);
				if (owner.scroll_show)
				{
					g.SetClip(owner.PaintExceedPre(rectangle2, rect));
					g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
				}
				else
				{
					g.SetClip(rectangle2);
				}
				tabPage = PaintTABS(owner, g, items, num, solidBrush, num2, TL: true, TR: false, BR: false, BL: true);
				g.ResetTransform();
				if (tabPage == null)
				{
					break;
				}
				GraphicsState state2 = g.Save();
				if (owner.pageMove == tabPage)
				{
					g.TranslateTransform(-owner.offsetx, -owner.offsety);
				}
				Rectangle rect3 = tabPage.Rect;
				if (num2 > 0)
				{
					float num6 = rect3.Right - num4;
					PaintTABS(rect_card: new RectangleF(rect3.X - num4, rect3.Y + num4, rect3.Width + num4, rect3.Height - num2), rect_line: new Rectangle(rect3.X - num4, rect3.Y - num2, rect3.Width, rect3.Height + num3), owner: owner, gg: g, rect_t: clientRectangle, rect_page: rect3, radius: num, x: num6, y: clientRectangle.Y, x2: num6, y2: clientRectangle.Bottom, bor: num2, TL: true, TR: false, BR: false, BL: true);
					if (owner.scroll_show)
					{
						g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
					}
				}
				else
				{
					if (owner.scroll_show)
					{
						g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
					}
					using GraphicsPath path2 = rect3.RoundPath(num, roundTL: true, roundTR: false, roundBR: false, roundBL: true);
					using SolidBrush brush3 = new SolidBrush(FillActive ?? Colour.BgContainer.Get(owner.ColorScheme, "Tabs", owner.Name));
					g.Fill(brush3, path2);
				}
				PaintText(g, rects[selectedIndex], owner, tabPage, brush);
				g.Restore(state2);
				break;
			}
			case TabAlignment.Right:
			{
				int num7 = rect.Width + rect.Y;
				Rectangle rectangle3 = new Rectangle(clientRectangle.Right - num7, clientRectangle.Y, num7, clientRectangle.Height);
				if (owner.scroll_show)
				{
					g.SetClip(owner.PaintExceedPre(rectangle3, rect));
					g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
				}
				else
				{
					g.SetClip(rectangle3);
				}
				tabPage = PaintTABS(owner, g, items, num, solidBrush, num2, TL: false, TR: true, BR: true, BL: false);
				g.ResetTransform();
				if (tabPage == null)
				{
					break;
				}
				GraphicsState state3 = g.Save();
				if (owner.pageMove == tabPage)
				{
					g.TranslateTransform(-owner.offsetx, -owner.offsety);
				}
				Rectangle rect4 = tabPage.Rect;
				if (num2 > 0)
				{
					float num8 = rect4.X + num4;
					PaintTABS(rect_card: new RectangleF(rect4.X - num4, rect4.Y + num4, rect4.Width + num4, rect4.Height - num2), rect_line: new Rectangle(rect4.X + num2, rect4.Y - num2, rect4.Width + num2, rect4.Height + num3), owner: owner, gg: g, rect_t: clientRectangle, rect_page: rect4, radius: num, x: num8, y: clientRectangle.Y, x2: num8, y2: clientRectangle.Bottom, bor: num2, TL: false, TR: true, BR: true, BL: false);
					if (owner.scroll_show)
					{
						g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
					}
				}
				else
				{
					if (owner.scroll_show)
					{
						g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
					}
					using GraphicsPath path3 = rect4.RoundPath(num, roundTL: false, roundTR: true, roundBR: true, roundBL: false);
					using SolidBrush brush4 = new SolidBrush(FillActive ?? Colour.BgContainer.Get(owner.ColorScheme, "Tabs", owner.Name));
					g.Fill(brush4, path3);
				}
				PaintText(g, rects[selectedIndex], owner, tabPage, brush);
				g.Restore(state3);
				break;
			}
			default:
			{
				Rectangle rectangle = new Rectangle(clientRectangle.X, clientRectangle.Y, clientRectangle.Width, rect.Bottom);
				if (owner.scroll_show)
				{
					g.SetClip(owner.PaintExceedPre(rectangle, rect));
					g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
				}
				else
				{
					g.SetClip(rectangle);
				}
				tabPage = PaintTABS(owner, g, items, num, solidBrush, num2, TL: true, TR: true, BR: false, BL: false);
				g.ResetTransform();
				if (tabPage == null)
				{
					break;
				}
				GraphicsState state = g.Save();
				if (owner.pageMove == tabPage)
				{
					g.TranslateTransform(-owner.offsetx, -owner.offsety);
				}
				Rectangle rect2 = tabPage.Rect;
				if (num2 > 0)
				{
					float num5 = rect2.Bottom - num4;
					PaintTABS(rect_card: new RectangleF(rect2.X + num4, rect2.Y - num4, rect2.Width - num2, rect2.Height + num4), rect_line: new Rectangle(rect2.X - num2, rect2.Y - num2, rect2.Width + num3, rect2.Height), owner: owner, gg: g, rect_t: clientRectangle, rect_page: rect2, radius: num, x: clientRectangle.X, y: num5, x2: clientRectangle.Right, y2: num5, bor: num2, TL: true, TR: true, BR: false, BL: false);
					if (owner.scroll_show)
					{
						g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
					}
				}
				else
				{
					if (owner.scroll_show)
					{
						g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
					}
					using GraphicsPath path = rect2.RoundPath(num, roundTL: true, roundTR: true, roundBR: false, roundBL: false);
					using SolidBrush brush2 = new SolidBrush(FillActive ?? Colour.BgContainer.Get(owner.ColorScheme, "Tabs", owner.Name));
					g.Fill(brush2, path);
				}
				PaintText(g, rects[selectedIndex], owner, tabPage, brush);
				g.Restore(state);
				break;
			}
			}
			g.ResetClip();
			if (owner.scroll_show)
			{
				owner.PaintExceed(g, solidBrush.Color, num, clientRectangle, rect, rects[rects.Length - 1].Rect, full: true);
			}
		}

		private TabPage? PaintTABS(Tabs owner, Canvas g, TabCollection items, int radius, SolidBrush brush_fore, int bor, bool TL, bool TR, bool BR, bool BL)
		{
			int num = 0;
			TabPage result = null;
			using SolidBrush solidBrush = new SolidBrush(owner.FillActive ?? Colour.PrimaryActive.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush solidBrush2 = new SolidBrush(owner.FillHover ?? Colour.PrimaryHover.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush solidBrush3 = new SolidBrush(Fill ?? Colour.FillQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush solidBrush4 = new SolidBrush(FillHover ?? Colour.FillQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
			foreach (TabPage item in items)
			{
				if (item.Visible)
				{
					if (owner.SelectedIndex == num)
					{
						result = item;
					}
					else
					{
						GraphicsState state = g.Save();
						if (owner.pageMove == item)
						{
							g.TranslateTransform(-owner.offsetx, -owner.offsety);
						}
						using (GraphicsPath path = item.Rect.RoundPath(radius, TL, TR, BR, BL))
						{
							g.Fill((owner.hover_i == num) ? solidBrush4 : solidBrush3, path);
							if (bor > 0)
							{
								g.Draw(border ?? Colour.BorderSecondary.Get(owner.ColorScheme, "Tabs", owner.Name), bor, path);
							}
							if (owner.hover_i == num)
							{
								PaintText(g, rects[num], owner, item, (owner.pageDown == item) ? solidBrush : solidBrush2);
							}
							else
							{
								PaintText(g, rects[num], owner, item, brush_fore);
							}
						}
						g.Restore(state);
					}
				}
				num++;
			}
			return result;
		}

		private void PaintTABS(Tabs owner, Canvas gg, Rectangle rect_t, Rectangle rect_page, int radius, RectangleF rect_card, Rectangle rect_line, float x, float y, float x2, float y2, int bor, bool TL, bool TR, bool BR, bool BL)
		{
			try
			{
				using Bitmap image = new Bitmap(rect_t.Width, rect_t.Height);
				using (Canvas canvas = Graphics.FromImage(image).High(owner.Dpi))
				{
					using GraphicsPath path = rect_page.RoundPath(radius, TL, TR, BR, BL);
					using Pen pen = new Pen(BorderActive ?? Colour.BorderColor.Get(owner.ColorScheme, "Tabs", owner.Name), bor);
					using SolidBrush brush = new SolidBrush(FillActive ?? Colour.BgContainer.Get(owner.ColorScheme, "Tabs", owner.Name));
					canvas.DrawLine(pen, x, y, x2, y2);
					if (owner.scroll_show)
					{
						canvas.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
					}
					using (GraphicsPath path2 = rect_card.RoundPath(radius, TL, TR, BR, BL))
					{
						canvas.Fill(brush, path2);
					}
					canvas.SetClip(rect_line);
					canvas.Draw(pen, path);
				}
				gg.Image(image, rect_t);
			}
			catch
			{
			}
		}

		private void PaintText(Canvas g, TabPageRect rects, Tabs owner, TabPage page, SolidBrush brush)
		{
			if (page.Enabled)
			{
				if (page.HasIcon)
				{
					if (page.Icon != null)
					{
						g.Image(page.Icon, rects.Rect_Ico);
					}
					if (page.IconSvg != null)
					{
						g.Svg(page.IconSvg, rects.Rect_Ico, brush.Color);
					}
				}
				if (closable)
				{
					if (rects.hover_close == null)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else if (rects.hover_close.Animation)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Helper.ToColor(rects.hover_close.Value + Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name).A, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name)));
					}
					else if (rects.hover_close.Switch)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
				}
				PaintTextCore(g, rects, owner, page, brush);
			}
			else
			{
				if (page.HasIcon)
				{
					if (page.Icon != null)
					{
						g.Image(page.Icon, rects.Rect_Ico);
					}
					if (page.IconSvg != null)
					{
						g.Svg(page.IconSvg, rects.Rect_Ico, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
				}
				if (closable)
				{
					if (rects.hover_close == null)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else if (rects.hover_close.Animation)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Helper.ToColor(rects.hover_close.Value + Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name).A, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name)));
					}
					else if (rects.hover_close.Switch)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
				}
				using SolidBrush brush2 = new SolidBrush(Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
				PaintTextCore(g, rects, owner, page, brush2);
			}
			owner.PaintBadge(g, page, rects.Rect_Text);
		}

		private void PaintTextCore(Canvas g, TabPageRect rects, Tabs owner, TabPage page, SolidBrush brush)
		{
			if (owner.IsRotate)
			{
				GraphicsState state = g.Save();
				float dx = (float)rects.Rect_Text.X + (float)rects.Rect_Text.Width / 2f;
				float dy = (float)rects.Rect_Text.Y + (float)rects.Rect_Text.Height / 2f;
				g.TranslateTransform(dx, dy);
				float angle = ((owner.Rotate == TRotate.Clockwise_90) ? 90f : (-90f));
				g.RotateTransform(angle);
				g.String(rect: new RectangleF((float)(-rects.Rect_Text.Height) / 2f, (float)(-rects.Rect_Text.Width) / 2f, rects.Rect_Text.Height, rects.Rect_Text.Width), text: page.Text, font: owner.Font, brush: brush, format: owner.s_c);
				g.Restore(state);
			}
			else
			{
				g.String(page.Text, owner.Font, brush, rects.Rect_Text, owner.s_c);
			}
		}

		public TabPageRect GetTabRect(int i)
		{
			return rects[i];
		}

		public void SelectedIndexChanged(int i, int old)
		{
			if (owner == null)
			{
				return;
			}
			if (old > -1 && i > -1)
			{
				TabPageRect oldTab = rects[old];
				TabPageRect newTab = rects[i];
				if (owner.TabFocusMove(oldTab, newTab, i, rects.Length))
				{
					return;
				}
			}
			owner.Invalidate();
		}

		public void Dispose()
		{
			TabPageRect[] array = rects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].hover_close?.Dispose();
			}
		}

		public bool MouseClick(TabPage page, int i, int x, int y)
		{
			if (owner == null)
			{
				return false;
			}
			if (closable && !page.ReadOnly && rects[i].Rect_Close.Contains(x, y))
			{
				if (owner.IOnClosingPage(page))
				{
					owner.Pages.Remove(page);
					if (owner.CloseDisposePage)
					{
						page.Dispose();
					}
				}
				return true;
			}
			return false;
		}

		public void MouseMove(int x, int y)
		{
			if (owner == null || !closable)
			{
				return;
			}
			int num = 0;
			TabPageRect[] array = rects;
			foreach (TabPageRect tabPageRect in array)
			{
				TabPageRect tabPageRect2 = tabPageRect;
				if (tabPageRect2.hover_close == null)
				{
					tabPageRect2.hover_close = new ITaskOpacity("Tabs", owner);
				}
				if (num == owner.hover_i)
				{
					tabPageRect.hover_close.MaxValue = Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name).A - Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name).A;
					tabPageRect.hover_close.Switch = tabPageRect.Rect_Close.Contains(x, y);
				}
				else
				{
					tabPageRect.hover_close.Switch = false;
				}
				num++;
			}
		}

		public void MouseLeave()
		{
			if (!closable)
			{
				return;
			}
			TabPageRect[] array = rects;
			foreach (TabPageRect tabPageRect in array)
			{
				if (tabPageRect.hover_close != null)
				{
					tabPageRect.hover_close.Switch = false;
				}
			}
		}
	}

	/// <summary>
	/// 卡片样式2
	/// </summary>
	public class StyleCard2 : IStyle
	{
		public enum CloseType
		{
			none,
			always,
			activate
		}

		private Tabs? owner;

		private int radius = 6;

		private int bordersize = 1;

		private Color? border;

		private Color? fill;

		private int gap = 2;

		private CloseType closable;

		private TabPageRect[] rects = new TabPageRect[0];

		private int onetmp;

		/// <summary>
		/// 卡片圆角
		/// </summary>
		[Description("卡片圆角")]
		[Category("卡片")]
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
					owner?.Invalidate();
				}
			}
		}

		/// <summary>
		/// 边框大小
		/// </summary>
		[Description("边框大小")]
		[Category("卡片")]
		[DefaultValue(1)]
		public int Border
		{
			get
			{
				return bordersize;
			}
			set
			{
				if (bordersize != value)
				{
					bordersize = value;
					owner?.LoadLayout(print: true);
				}
			}
		}

		/// <summary>
		/// 卡片边框颜色
		/// </summary>
		[Description("卡片边框颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? BorderColor
		{
			get
			{
				return border;
			}
			set
			{
				if (!(border == value))
				{
					border = value;
					owner?.Invalidate();
				}
			}
		}

		/// <summary>
		/// 卡片边框激活颜色
		/// </summary>
		[Description("卡片边框激活颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? BorderActive { get; set; }

		/// <summary>
		/// 卡片颜色
		/// </summary>
		[Description("卡片颜色")]
		[Category("卡片")]
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
					owner?.Invalidate();
				}
			}
		}

		/// <summary>
		/// 卡片悬停颜色
		/// </summary>
		[Description("卡片悬停颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? FillHover { get; set; }

		/// <summary>
		/// 卡片激活颜色
		/// </summary>
		[Description("卡片激活颜色")]
		[Category("卡片")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? FillActive { get; set; }

		/// <summary>
		/// 卡片间距
		/// </summary>
		[Description("卡片间距")]
		[Category("卡片")]
		[DefaultValue(2)]
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
					owner?.LoadLayout(print: true);
				}
			}
		}

		/// <summary>
		/// 可关闭
		/// </summary>
		[Description("可关闭")]
		[Category("卡片")]
		[DefaultValue(false)]
		public CloseType Closable
		{
			get
			{
				return closable;
			}
			set
			{
				if (closable != value)
				{
					closable = value;
					owner?.LoadLayout(print: true);
				}
			}
		}

		public StyleCard2()
		{
		}

		public StyleCard2(Tabs tabs)
		{
			owner = tabs;
		}

		public void LoadLayout(Tabs tabs, Rectangle rect, TabCollection items)
		{
			owner = tabs;
			rects = tabs.GDI((Canvas g) =>
			{
				int num = (int)((float)tabs.Gap * tabs.Dpi);
				int num2 = num * 2;
				int num3 = 0;
				int num4 = (int)((float)Gap * tabs.Dpi);
				List<TabPageRect> list = new List<TabPageRect>(items.Count);
				Dictionary<TabPage, Size> dir = GetDir(tabs, g, items, num, num2, out var ico_size, out var ico_gap, out var close_size, out var sizewh, out var rw);
				int? num5 = (tabs.TextCenter ? new int?(rw) : ((int?)null));
				if (closable != CloseType.none)
				{
					switch (tabs.Alignment)
					{
					case TabAlignment.Bottom:
					{
						int y5 = rect.Bottom - sizewh;
						foreach (KeyValuePair<TabPage, Size> item in dir)
						{
							if (item.Key.Visible)
							{
								Rectangle rectangle2 = default;
								bool readOnly2 = item.Key.ReadOnly;
								bool hasIcon4 = item.Key.HasIcon;
								int num19 = item.Value.Width;
								if (hasIcon4 && num19 == 0)
								{
									num19 = ico_size + ico_gap;
									if (!readOnly2)
									{
										num19 += close_size + ico_gap;
									}
								}
								else
								{
									if (hasIcon4)
									{
										num19 += ico_size + ico_gap * 2;
									}
									if (!readOnly2)
									{
										num19 += close_size + ico_gap;
									}
								}
								rectangle2 = new Rectangle(rect.X + num3, y5, num19 + num2, sizewh);
								if (readOnly2)
								{
									if (hasIcon4)
									{
										list.Add(new TabPageRect(rectangle2, item.Value, num, ico_size, ico_gap, left: true));
									}
									else
									{
										list.Add(new TabPageRect(rectangle2, item.Value, num, left: true));
									}
								}
								else if (hasIcon4)
								{
									list.Add(new TabPageRect(rectangle2, item.Value, num, ico_size, close_size, ico_gap, left: true));
								}
								else
								{
									list.Add(new TabPageRect(rectangle2, test: false, item.Value, num, close_size, ico_gap, left: true));
								}
								item.Key.SetRect(rectangle2);
								num3 += rectangle2.Width + num4;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num3 -= num4;
						tabs.SetPadding(0, 0, 0, sizewh + 2 + bordersize + tabs.Margin.Top);
						owner.scroll_max = num3 - rect.Width;
						owner.scroll_show = num3 > rect.Width;
						break;
					}
					case TabAlignment.Left:
						foreach (KeyValuePair<TabPage, Size> item2 in dir)
						{
							if (item2.Key.Visible)
							{
								int height = item2.Value.Height;
								int height2;
								if (!owner.IsRotate)
								{
									height2 = ((!item2.Key.HasIcon || height != 0) ? (height + num) : (ico_size + num));
								}
								else
								{
									int num7 = (item2.Key.HasIcon ? (ico_size + ico_gap) : 0);
									int num8 = ((!item2.Key.ReadOnly) ? (close_size + ico_gap) : 0);
									height2 = num + num7 + height + num8 + num;
								}
								Rectangle rect2 = new Rectangle(rect.X, rect.Y + num3, sizewh, height2);
								TabPageRect tabPageRect;
								if (!owner.IsRotate)
								{
									tabPageRect = (item2.Key.ReadOnly ? ((!item2.Key.HasIcon) ? new TabPageRect(rect2, item2.Value, num, left: true, num5) : new TabPageRect(rect2, item2.Value, num, ico_size, ico_gap, left: true, num5)) : ((!item2.Key.HasIcon) ? new TabPageRect(rect2, test: false, item2.Value, num, close_size, ico_gap, left: true, num5, rw) : new TabPageRect(rect2, item2.Value, num, ico_size, close_size, ico_gap, left: true, num5, rw)));
								}
								else
								{
									tabPageRect = new TabPageRect(rect2, item2.Value, num, left: true, num5);
									bool flag = owner.Rotate == TRotate.Clockwise_90;
									bool hasIcon2 = item2.Key.HasIcon;
									bool flag2 = !item2.Key.ReadOnly;
									if (hasIcon2)
									{
										int x = rect2.X + (rect2.Width - ico_size) / 2;
										int y = (flag ? (rect2.Y + num) : (rect2.Bottom - num - ico_size));
										tabPageRect.Rect_Ico = new Rectangle(x, y, ico_size, ico_size);
									}
									else
									{
										tabPageRect.Rect_Ico = Rectangle.Empty;
									}
									if (flag2)
									{
										int num9 = close_size;
										int x2 = rect2.X + (rect2.Width - num9) / 2;
										int y2 = (flag ? (rect2.Bottom - num - num9) : (rect2.Y + num));
										tabPageRect.Rect_Close = new Rectangle(x2, y2, num9, num9);
									}
									else
									{
										tabPageRect.Rect_Close = Rectangle.Empty;
									}
									int num10 = rect2.Y + num;
									int num11 = rect2.Bottom - num;
									if (hasIcon2 & flag)
									{
										num10 += ico_size + ico_gap;
									}
									if (flag2 && !flag)
									{
										num10 += close_size + ico_gap;
									}
									if (flag2 & flag)
									{
										num11 -= close_size + ico_gap;
									}
									if (hasIcon2 && !flag)
									{
										num11 -= ico_size + ico_gap;
									}
									int height3 = Math.Max(0, num11 - num10);
									int num12 = num5 ?? item2.Value.Width;
									int x3 = rect2.X + Math.Max(0, (rect2.Width - num12) / 2);
									tabPageRect.Rect_Text = new Rectangle(x3, num10, num12, height3);
								}
								list.Add(tabPageRect);
								item2.Key.SetRect(rect2);
								num3 += rect2.Height + num4;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num3 -= num4;
						tabs.SetPadding(sizewh + 2 + bordersize + tabs.Margin.Left, 0, 0, 0);
						owner.scroll_max = num3 - rect.Height;
						owner.scroll_show = num3 > rect.Height;
						break;
					case TabAlignment.Right:
					{
						int x4 = rect.Right - sizewh;
						foreach (KeyValuePair<TabPage, Size> item3 in dir)
						{
							if (item3.Key.Visible)
							{
								int height4 = item3.Value.Height;
								int height5;
								if (!owner.IsRotate)
								{
									height5 = ((!item3.Key.HasIcon || height4 != 0) ? (height4 + num) : (ico_size + num));
								}
								else
								{
									int num13 = (item3.Key.HasIcon ? (ico_size + ico_gap) : 0);
									int num14 = ((!item3.Key.ReadOnly) ? (close_size + ico_gap) : 0);
									height5 = num + num13 + height4 + num14 + num;
								}
								Rectangle rect3 = new Rectangle(x4, rect.Y + num3, sizewh, height5);
								TabPageRect tabPageRect2;
								if (!owner.IsRotate)
								{
									tabPageRect2 = (item3.Key.ReadOnly ? ((!item3.Key.HasIcon) ? new TabPageRect(rect3, item3.Value, num, left: false, num5) : new TabPageRect(rect3, item3.Value, num, ico_size, ico_gap, left: false, num5)) : ((!item3.Key.HasIcon) ? new TabPageRect(rect3, test: false, item3.Value, num, close_size, ico_gap, left: false, num5, rw) : new TabPageRect(rect3, item3.Value, num, ico_size, close_size, ico_gap, left: false, num5, rw)));
								}
								else
								{
									tabPageRect2 = new TabPageRect(rect3, item3.Value, num, left: true, num5);
									bool flag3 = owner.Rotate == TRotate.Clockwise_90;
									bool hasIcon3 = item3.Key.HasIcon;
									bool flag4 = !item3.Key.ReadOnly;
									if (hasIcon3)
									{
										int x5 = rect3.X + (rect3.Width - ico_size) / 2;
										int y3 = (flag3 ? (rect3.Y + num) : (rect3.Bottom - num - ico_size));
										tabPageRect2.Rect_Ico = new Rectangle(x5, y3, ico_size, ico_size);
									}
									else
									{
										tabPageRect2.Rect_Ico = Rectangle.Empty;
									}
									if (flag4)
									{
										int num15 = close_size;
										int x6 = rect3.X + (rect3.Width - num15) / 2;
										int y4 = (flag3 ? (rect3.Bottom - num - num15) : (rect3.Y + num));
										tabPageRect2.Rect_Close = new Rectangle(x6, y4, num15, num15);
									}
									else
									{
										tabPageRect2.Rect_Close = Rectangle.Empty;
									}
									int num16 = rect3.Y + num;
									int num17 = rect3.Bottom - num;
									if (hasIcon3 & flag3)
									{
										num16 += ico_size + ico_gap;
									}
									if (flag4 && !flag3)
									{
										num16 += close_size + ico_gap;
									}
									if (flag4 & flag3)
									{
										num17 -= close_size + ico_gap;
									}
									if (hasIcon3 && !flag3)
									{
										num17 -= ico_size + ico_gap;
									}
									int height6 = Math.Max(0, num17 - num16);
									int num18 = num5 ?? item3.Value.Width;
									int x7 = rect3.X + Math.Max(0, (rect3.Width - num18) / 2);
									tabPageRect2.Rect_Text = new Rectangle(x7, num16, num18, height6);
								}
								list.Add(tabPageRect2);
								item3.Key.SetRect(rect3);
								num3 += rect3.Height + num4;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num3 -= num4;
						tabs.SetPadding(0, 0, sizewh + 2 + bordersize + tabs.Margin.Left, 0);
						owner.scroll_max = num3 - rect.Height;
						owner.scroll_show = num3 > rect.Height;
						break;
					}
					default:
						foreach (KeyValuePair<TabPage, Size> item4 in dir)
						{
							if (item4.Key.Visible)
							{
								Rectangle rectangle = default;
								bool readOnly = item4.Key.ReadOnly;
								bool hasIcon = item4.Key.HasIcon;
								int num6 = item4.Value.Width;
								if (hasIcon && num6 == 0)
								{
									num6 = ico_size + ico_gap;
									if (!readOnly)
									{
										num6 += close_size + ico_gap;
									}
								}
								else
								{
									if (hasIcon)
									{
										num6 += ico_size + ico_gap * 2;
									}
									if (!readOnly)
									{
										num6 += close_size + ico_gap;
									}
								}
								rectangle = new Rectangle(rect.X + num3, rect.Y, num6 + num2, sizewh);
								if (readOnly)
								{
									if (hasIcon)
									{
										list.Add(new TabPageRect(rectangle, item4.Value, num, ico_size, ico_gap, left: true));
									}
									else
									{
										list.Add(new TabPageRect(rectangle, item4.Value, num, left: true));
									}
								}
								else if (hasIcon)
								{
									list.Add(new TabPageRect(rectangle, item4.Value, num, ico_size, close_size, ico_gap, left: true));
								}
								else
								{
									list.Add(new TabPageRect(rectangle, test: false, item4.Value, num, close_size, ico_gap, left: true));
								}
								item4.Key.SetRect(rectangle);
								num3 += rectangle.Width + num4;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num3 -= num4;
						tabs.SetPadding(0, sizewh + 2 + bordersize + tabs.Margin.Top, 0, 0);
						owner.scroll_max = num3 - rect.Width;
						owner.scroll_show = num3 > rect.Width;
						break;
					}
				}
				else
				{
					switch (tabs.Alignment)
					{
					case TabAlignment.Bottom:
					{
						int y8 = rect.Bottom - sizewh;
						foreach (KeyValuePair<TabPage, Size> item5 in dir)
						{
							if (item5.Key.Visible)
							{
								bool hasIcon8 = item5.Key.HasIcon;
								int num29 = item5.Value.Width;
								if (hasIcon8 && num29 == 0)
								{
									num29 = ico_size + ico_gap;
								}
								else if (hasIcon8)
								{
									num29 += ico_size + ico_gap * 2;
								}
								Rectangle rect7 = new Rectangle(rect.X + num3, y8, num29 + num2, sizewh);
								if (hasIcon8)
								{
									list.Add(new TabPageRect(rect7, item5.Value, num, ico_size, ico_gap, left: true, num5));
								}
								else
								{
									list.Add(new TabPageRect(rect7, item5.Value, num, left: true, num5));
								}
								item5.Key.SetRect(rect7);
								num3 += rect7.Width + num4;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num3 -= num4;
						tabs.SetPadding(0, 0, 0, sizewh + 2 + bordersize + tabs.Margin.Top);
						owner.scroll_max = num3 - rect.Width;
						owner.scroll_show = num3 > rect.Width;
						break;
					}
					case TabAlignment.Left:
						foreach (KeyValuePair<TabPage, Size> item6 in dir)
						{
							if (item6.Key.Visible)
							{
								int height7 = item6.Value.Height;
								int height8;
								if (!owner.IsRotate)
								{
									height8 = ((!item6.Key.HasIcon || height7 != 0) ? (height7 + num) : (ico_size + num));
								}
								else
								{
									int num21 = (item6.Key.HasIcon ? (ico_size + ico_gap) : 0);
									height8 = num + num21 + height7 + num;
								}
								Rectangle rect5 = new Rectangle(rect.X, rect.Y + num3, sizewh, height8);
								TabPageRect tabPageRect3;
								if (!owner.IsRotate)
								{
									tabPageRect3 = ((!item6.Key.HasIcon) ? new TabPageRect(rect5, item6.Value, num, left: true, num5) : new TabPageRect(rect5, item6.Value, num, ico_size, ico_gap, left: true, num5));
								}
								else
								{
									tabPageRect3 = new TabPageRect(rect5, item6.Value, num, left: true, num5);
									bool flag5 = owner.Rotate == TRotate.Clockwise_90;
									bool hasIcon6 = item6.Key.HasIcon;
									if (hasIcon6)
									{
										int x8 = rect5.X + (rect5.Width - ico_size) / 2;
										int y6 = (flag5 ? (rect5.Y + num) : (rect5.Bottom - num - ico_size));
										tabPageRect3.Rect_Ico = new Rectangle(x8, y6, ico_size, ico_size);
									}
									else
									{
										tabPageRect3.Rect_Ico = Rectangle.Empty;
									}
									int num22 = rect5.Y + num;
									int num23 = rect5.Bottom - num;
									if (hasIcon6 & flag5)
									{
										num22 += ico_size + ico_gap;
									}
									if (hasIcon6 && !flag5)
									{
										num23 -= ico_size + ico_gap;
									}
									int height9 = Math.Max(0, num23 - num22);
									int num24 = num5 ?? item6.Value.Width;
									int x9 = rect5.X + Math.Max(0, (rect5.Width - num24) / 2);
									tabPageRect3.Rect_Text = new Rectangle(x9, num22, num24, height9);
								}
								list.Add(tabPageRect3);
								item6.Key.SetRect(rect5);
								num3 += rect5.Height + num4;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num3 -= num4;
						tabs.SetPadding(sizewh + 2 + bordersize + tabs.Margin.Left, 0, 0, 0);
						owner.scroll_max = num3 - rect.Height;
						owner.scroll_show = num3 > rect.Height;
						break;
					case TabAlignment.Right:
					{
						int x10 = rect.Right - sizewh;
						foreach (KeyValuePair<TabPage, Size> item7 in dir)
						{
							if (item7.Key.Visible)
							{
								int height10 = item7.Value.Height;
								int height11;
								if (!owner.IsRotate)
								{
									height11 = ((!item7.Key.HasIcon || height10 != 0) ? (height10 + num) : (ico_size + num));
								}
								else
								{
									int num25 = (item7.Key.HasIcon ? (ico_size + ico_gap) : 0);
									height11 = num + num25 + height10 + num;
								}
								Rectangle rect6 = new Rectangle(x10, rect.Y + num3, sizewh, height11);
								TabPageRect tabPageRect4;
								if (!owner.IsRotate)
								{
									tabPageRect4 = ((!item7.Key.HasIcon) ? new TabPageRect(rect6, item7.Value, num, left: false, num5) : new TabPageRect(rect6, item7.Value, num, ico_size, ico_gap, left: false, num5));
								}
								else
								{
									tabPageRect4 = new TabPageRect(rect6, item7.Value, num, left: true, num5);
									bool flag6 = owner.Rotate == TRotate.Clockwise_90;
									bool hasIcon7 = item7.Key.HasIcon;
									if (hasIcon7)
									{
										int x11 = rect6.X + (rect6.Width - ico_size) / 2;
										int y7 = (flag6 ? (rect6.Y + num) : (rect6.Bottom - num - ico_size));
										tabPageRect4.Rect_Ico = new Rectangle(x11, y7, ico_size, ico_size);
									}
									else
									{
										tabPageRect4.Rect_Ico = Rectangle.Empty;
									}
									int num26 = rect6.Y + num;
									int num27 = rect6.Bottom - num;
									if (hasIcon7 & flag6)
									{
										num26 += ico_size + ico_gap;
									}
									if (hasIcon7 && !flag6)
									{
										num27 -= ico_size + ico_gap;
									}
									int height12 = Math.Max(0, num27 - num26);
									int num28 = num5 ?? item7.Value.Width;
									int x12 = rect6.X + Math.Max(0, (rect6.Width - num28) / 2);
									tabPageRect4.Rect_Text = new Rectangle(x12, num26, num28, height12);
								}
								list.Add(tabPageRect4);
								item7.Key.SetRect(rect6);
								num3 += rect6.Height + num4;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num3 -= num4;
						tabs.SetPadding(0, 0, sizewh + 2 + bordersize + tabs.Margin.Left, 0);
						owner.scroll_max = num3 - rect.Height;
						owner.scroll_show = num3 > rect.Height;
						break;
					}
					default:
						foreach (KeyValuePair<TabPage, Size> item8 in dir)
						{
							if (item8.Key.Visible)
							{
								bool hasIcon5 = item8.Key.HasIcon;
								int num20 = item8.Value.Width;
								if (hasIcon5 && num20 == 0)
								{
									num20 = ico_size + ico_gap;
								}
								else if (hasIcon5)
								{
									num20 += ico_size + ico_gap * 2;
								}
								Rectangle rect4 = new Rectangle(rect.X + num3, rect.Y, num20 + num2, sizewh);
								if (hasIcon5)
								{
									list.Add(new TabPageRect(rect4, item8.Value, num, ico_size, ico_gap, left: true, num5));
								}
								else
								{
									list.Add(new TabPageRect(rect4, item8.Value, num, left: true, num5));
								}
								item8.Key.SetRect(rect4);
								num3 += rect4.Width + num4;
							}
							else
							{
								list.Add(new TabPageRect());
							}
						}
						num3 -= num4;
						tabs.SetPadding(0, sizewh + 2 + bordersize + tabs.Margin.Top, 0, 0);
						owner.scroll_max = num3 - rect.Width;
						owner.scroll_show = num3 > rect.Width;
						break;
					}
				}
				if (owner.scroll_show)
				{
					owner.scroll_max += owner.SizeExceed(owner.ClientRectangle, list[0].Rect, list[list.Count - 1].Rect);
				}
				else
				{
					Tabs? tabs2 = owner;
					int scroll_x = (owner.scroll_y = 0);
					tabs2.scroll_x = scroll_x;
					if (tabs.centered)
					{
						TabAlignment alignment = tabs.Alignment;
						if ((uint)alignment > 1u && (uint)(alignment - 2) <= 1u)
						{
							int y9 = (rect.Height - num3) / 2;
							foreach (KeyValuePair<TabPage, Size> item9 in dir)
							{
								item9.Key.SetOffset(0, y9);
							}
							foreach (TabPageRect item10 in list)
							{
								item10.Rect.Offset(0, y9);
								item10.Rect_Text.Offset(0, y9);
								item10.Rect_Ico.Offset(0, y9);
								item10.Rect_Close.Offset(0, y9);
							}
						}
						else
						{
							int x13 = (rect.Width - num3) / 2;
							foreach (KeyValuePair<TabPage, Size> item11 in dir)
							{
								item11.Key.SetOffset(x13, 0);
							}
							foreach (TabPageRect item12 in list)
							{
								item12.Rect.Offset(x13, 0);
								item12.Rect_Text.Offset(x13, 0);
								item12.Rect_Ico.Offset(x13, 0);
								item12.Rect_Close.Offset(x13, 0);
							}
						}
					}
				}
				return list.ToArray();
			});
		}

		private Dictionary<TabPage, Size> GetDir(Tabs owner, Canvas g, TabCollection items, int gap, int gap2, out int ico_size, out int ico_gap, out int close_size, out int sizewh, out int rw)
		{
			sizewh = (rw = 0);
			Size size = g.MeasureString("龍Qq", owner.Font);
			Dictionary<TabPage, Size> dictionary = new Dictionary<TabPage, Size>(items.Count);
			int num = 0;
			int? num2 = null;
			foreach (TabPage item in items)
			{
				Size value = g.MeasureString(item.Text, owner.Font);
				if (owner.IsRotate)
				{
					value = new Size(value.Height, value.Width);
				}
				dictionary.Add(item, value);
				if (rw < value.Width)
				{
					rw = value.Width;
				}
				if (item.Visible && !num2.HasValue)
				{
					num2 = num;
				}
				num++;
			}
			onetmp = num2.GetValueOrDefault();
			ico_size = (int)((float)size.Height * owner.iconratio);
			ico_gap = (int)((float)size.Height * owner.icongap);
			close_size = (int)((float)size.Height * (owner.iconratio * 0.8f));
			TabAlignment alignment = owner.Alignment;
			if ((uint)alignment > 1u && (uint)(alignment - 2) <= 1u)
			{
				if (closable != CloseType.none)
				{
					foreach (KeyValuePair<TabPage, Size> item2 in dictionary)
					{
						if (item2.Key.Visible)
						{
							int num3;
							if (item2.Key.HasIcon)
							{
								num3 = (owner.IsRotate ? (Math.Max(rw, ico_size) + gap2) : (item2.Value.Width + gap2 + ico_size + ico_gap));
							}
							else
							{
								num3 = (owner.IsRotate ? (rw + gap2) : (item2.Value.Width + gap2));
							}
							num3 += close_size + ico_gap;
							if (sizewh < num3)
							{
								sizewh = num3;
							}
						}
					}
				}
				else
				{
					foreach (KeyValuePair<TabPage, Size> item3 in dictionary)
					{
						if (item3.Key.Visible)
						{
							int num4;
							if (item3.Key.HasIcon)
							{
								num4 = (owner.IsRotate ? (Math.Max(rw, ico_size) + gap2) : (item3.Value.Width + gap2 + ico_size + ico_gap));
							}
							else
							{
								num4 = (owner.IsRotate ? (rw + gap2) : (item3.Value.Width + gap2));
							}
							if (sizewh < num4)
							{
								sizewh = num4;
							}
						}
					}
				}
			}
			else
			{
				foreach (KeyValuePair<TabPage, Size> item4 in dictionary)
				{
					if (item4.Key.Visible)
					{
						int num5 = ((item4.Value.Height > 0) ? (item4.Value.Height + gap) : (ico_size + gap));
						if (sizewh < num5)
						{
							sizewh = num5;
						}
					}
				}
			}
			return owner.HandItemSize(dictionary, ref sizewh);
		}

		public void Paint(Tabs owner, Canvas g, TabCollection items)
		{
			if (rects.Length != items.Count)
			{
				return;
			}
			using SolidBrush solidBrush = new SolidBrush(owner.ForeColor ?? Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush brush = new SolidBrush(owner.Fill ?? Colour.Primary.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush solidBrush2 = new SolidBrush(owner.FillActive ?? Colour.PrimaryActive.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush solidBrush3 = new SolidBrush(owner.FillHover ?? Colour.PrimaryHover.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush solidBrush4 = new SolidBrush(Fill ?? Colour.FillQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush solidBrush5 = new SolidBrush(FillHover ?? Colour.FillQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush brush2 = new SolidBrush(FillActive ?? Colour.BgContainer.Get(owner.ColorScheme, "Tabs", owner.Name));
			Rectangle clientRectangle = owner.ClientRectangle;
			int num = (int)((float)Radius * owner.Dpi);
			int num2 = (int)((float)bordersize * owner.Dpi);
			_ = num2 * 6;
			float num3 = (float)num2 / 2f;
			TabPage tabPage = null;
			int num4 = 0;
			int selectedIndex = owner.SelectedIndex;
			Rectangle rect = rects[onetmp].Rect;
			switch (owner.Alignment)
			{
			case TabAlignment.Bottom:
			{
				int num5 = rect.Height + rect.X;
				Rectangle rectangle2 = new Rectangle(clientRectangle.X, clientRectangle.Bottom - num5 - num2, clientRectangle.Width, num5 + num2);
				if (owner.scroll_show)
				{
					g.SetClip(owner.PaintExceedPre(rectangle2, rect));
					g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
				}
				else
				{
					g.SetClip(rectangle2);
				}
				foreach (TabPage item in items)
				{
					if (item.Visible)
					{
						if (selectedIndex == num4)
						{
							tabPage = item;
						}
						else
						{
							GraphicsState state3 = g.Save();
							if (owner.pageMove == item)
							{
								g.TranslateTransform(-owner.offsetx, -owner.offsety);
							}
							using (GraphicsPath path4 = item.Rect.RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
							{
								g.Fill((owner.hover_i == num4) ? solidBrush5 : solidBrush4, path4);
								if (num2 > 0)
								{
									g.Draw(border ?? Colour.BorderSecondary.Get(owner.ColorScheme, "Tabs", owner.Name), num2, path4);
								}
								if (owner.hover_i == num4)
								{
									PaintText(g, rects[num4], owner, item, (owner.pageDown == item) ? solidBrush2 : solidBrush3, closshow: true);
								}
								else
								{
									PaintText(g, rects[num4], owner, item, solidBrush, closable == CloseType.always);
								}
							}
							g.Restore(state3);
						}
					}
					num4++;
				}
				g.ResetTransform();
				if (tabPage == null)
				{
					break;
				}
				GraphicsState state4 = g.Save();
				if (owner.pageMove == tabPage)
				{
					g.TranslateTransform(-owner.offsetx, -owner.offsety);
				}
				Rectangle rect3 = tabPage.Rect;
				using (GraphicsPath path5 = rect3.RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
				{
					if (num2 > 0)
					{
						_ = rect3.Y;
						if (owner.scroll_show)
						{
							g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
						}
						using (GraphicsPath path6 = new RectangleF((float)rect3.X + num3, (float)rect3.Y - num3, rect3.Width - num2, (float)rect3.Height + num3).RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
						{
							g.Fill(brush2, path6);
						}
						g.Draw(BorderActive ?? Colour.BorderColor.Get(owner.ColorScheme, "Tabs", owner.Name), num2, path5);
					}
					else
					{
						if (owner.scroll_show)
						{
							g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
						}
						g.Fill(brush2, path5);
					}
					PaintText(g, rects[selectedIndex], owner, tabPage, brush, closshow: true);
				}
				g.Restore(state4);
				break;
			}
			case TabAlignment.Left:
			{
				Rectangle rectangle3 = new Rectangle(clientRectangle.X, clientRectangle.Y, rect.Right + num2, clientRectangle.Height);
				if (owner.scroll_show)
				{
					g.SetClip(owner.PaintExceedPre(rectangle3, rect));
					g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
				}
				else
				{
					g.SetClip(rectangle3);
				}
				foreach (TabPage item2 in items)
				{
					if (item2.Visible)
					{
						if (owner.SelectedIndex == num4)
						{
							tabPage = item2;
						}
						else
						{
							GraphicsState state5 = g.Save();
							if (owner.pageMove == item2)
							{
								g.TranslateTransform(-owner.offsetx, -owner.offsety);
							}
							using (GraphicsPath path7 = item2.Rect.RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
							{
								g.Fill((owner.hover_i == num4) ? solidBrush5 : solidBrush4, path7);
								if (num2 > 0)
								{
									g.Draw(border ?? Colour.BorderSecondary.Get(owner.ColorScheme, "Tabs", owner.Name), num2, path7);
								}
								if (owner.hover_i == num4)
								{
									PaintText(g, rects[num4], owner, item2, (owner.pageDown == item2) ? solidBrush2 : solidBrush3, closshow: true);
								}
								else
								{
									PaintText(g, rects[num4], owner, item2, solidBrush, closable == CloseType.always);
								}
							}
							g.Restore(state5);
						}
					}
					num4++;
				}
				g.ResetTransform();
				if (tabPage == null)
				{
					break;
				}
				GraphicsState state6 = g.Save();
				if (owner.pageMove == tabPage)
				{
					g.TranslateTransform(-owner.offsetx, -owner.offsety);
				}
				Rectangle rect4 = tabPage.Rect;
				using (GraphicsPath path8 = rect4.RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
				{
					if (num2 > 0)
					{
						_ = rect4.Right;
						if (owner.scroll_show)
						{
							g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
						}
						using (GraphicsPath path9 = new RectangleF((float)rect4.X - num3, (float)rect4.Y + num3, (float)rect4.Width + num3, rect4.Height - num2).RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
						{
							g.Fill(brush2, path9);
						}
						g.Draw(BorderActive ?? Colour.BorderColor.Get(owner.ColorScheme, "Tabs", owner.Name), num2, path8);
					}
					else
					{
						if (owner.scroll_show)
						{
							g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
						}
						g.Fill(brush2, path8);
					}
					PaintText(g, rects[selectedIndex], owner, tabPage, brush, closshow: true);
				}
				g.Restore(state6);
				break;
			}
			case TabAlignment.Right:
			{
				int num6 = rect.Width + rect.Y;
				Rectangle rectangle4 = new Rectangle(clientRectangle.Right - num6 - num2, clientRectangle.Y, num6 + num2, clientRectangle.Height);
				if (owner.scroll_show)
				{
					g.SetClip(owner.PaintExceedPre(rectangle4, rect));
					g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
				}
				else
				{
					g.SetClip(rectangle4);
				}
				foreach (TabPage item3 in items)
				{
					if (item3.Visible)
					{
						if (owner.SelectedIndex == num4)
						{
							tabPage = item3;
						}
						else
						{
							GraphicsState state7 = g.Save();
							if (owner.pageMove == item3)
							{
								g.TranslateTransform(-owner.offsetx, -owner.offsety);
							}
							using (GraphicsPath path10 = item3.Rect.RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
							{
								g.Fill((owner.hover_i == num4) ? solidBrush5 : solidBrush4, path10);
								if (num2 > 0)
								{
									g.Draw(border ?? Colour.BorderSecondary.Get(owner.ColorScheme, "Tabs", owner.Name), num2, path10);
								}
								if (owner.hover_i == num4)
								{
									PaintText(g, rects[num4], owner, item3, (owner.pageDown == item3) ? solidBrush2 : solidBrush3, closshow: true);
								}
								else
								{
									PaintText(g, rects[num4], owner, item3, solidBrush, closable == CloseType.always);
								}
							}
							g.Restore(state7);
						}
					}
					num4++;
				}
				g.ResetTransform();
				if (tabPage == null)
				{
					break;
				}
				GraphicsState state8 = g.Save();
				if (owner.pageMove == tabPage)
				{
					g.TranslateTransform(-owner.offsetx, -owner.offsety);
				}
				Rectangle rect5 = tabPage.Rect;
				using (GraphicsPath path11 = rect5.RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
				{
					if (num2 > 0)
					{
						_ = rect5.X;
						if (owner.scroll_show)
						{
							g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
						}
						using (GraphicsPath path12 = new RectangleF((float)rect5.X - num3, (float)rect5.Y + num3, (float)rect5.Width + num3, rect5.Height - num2).RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
						{
							g.Fill(brush2, path12);
						}
						g.Draw(BorderActive ?? Colour.BorderColor.Get(owner.ColorScheme, "Tabs", owner.Name), num2, path11);
					}
					else
					{
						if (owner.scroll_show)
						{
							g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
						}
						g.Fill(brush2, path11);
					}
					PaintText(g, rects[selectedIndex], owner, tabPage, brush, closshow: true);
				}
				g.Restore(state8);
				break;
			}
			default:
			{
				Rectangle rectangle = new Rectangle(clientRectangle.X, clientRectangle.Y, clientRectangle.Width, rect.Bottom + num2);
				if (owner.scroll_show)
				{
					g.SetClip(owner.PaintExceedPre(rectangle, rect));
					g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
				}
				else
				{
					g.SetClip(rectangle);
				}
				foreach (TabPage item4 in items)
				{
					if (item4.Visible)
					{
						if (owner.SelectedIndex == num4)
						{
							tabPage = item4;
						}
						else
						{
							GraphicsState state = g.Save();
							if (owner.pageMove == item4)
							{
								g.TranslateTransform(-owner.offsetx, -owner.offsety);
							}
							using (GraphicsPath path = item4.Rect.RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
							{
								g.Fill((owner.hover_i == num4) ? solidBrush5 : solidBrush4, path);
								if (num2 > 0)
								{
									g.Draw(border ?? Colour.BorderSecondary.Get(owner.ColorScheme, "Tabs", owner.Name), num2, path);
								}
								if (owner.hover_i == num4)
								{
									PaintText(g, rects[num4], owner, item4, (owner.pageDown == item4) ? solidBrush2 : solidBrush3, closshow: true);
								}
								else
								{
									PaintText(g, rects[num4], owner, item4, solidBrush, closable == CloseType.always && !item4.ReadOnly);
								}
							}
							g.Restore(state);
						}
					}
					num4++;
				}
				g.ResetTransform();
				if (tabPage == null)
				{
					break;
				}
				GraphicsState state2 = g.Save();
				if (owner.pageMove == tabPage)
				{
					g.TranslateTransform(-owner.offsetx, -owner.offsety);
				}
				Rectangle rect2 = tabPage.Rect;
				using (GraphicsPath path2 = rect2.RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
				{
					if (num2 > 0)
					{
						_ = rect2.Bottom;
						if (owner.scroll_show)
						{
							g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
						}
						using (GraphicsPath path3 = new RectangleF((float)rect2.X + num3, (float)rect2.Y - num3, rect2.Width - num2, (float)rect2.Height + num3).RoundPath(num, roundTL: true, roundTR: true, roundBR: true, roundBL: true))
						{
							g.Fill(brush2, path3);
						}
						g.Draw(BorderActive ?? Colour.BorderColor.Get(owner.ColorScheme, "Tabs", owner.Name), num2, path2);
					}
					else
					{
						if (owner.scroll_show)
						{
							g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
						}
						g.Fill(brush2, path2);
					}
					PaintText(g, rects[selectedIndex], owner, tabPage, brush, !tabPage.ReadOnly);
				}
				g.Restore(state2);
				break;
			}
			}
			if (owner.scroll_show)
			{
				owner.PaintExceed(g, solidBrush.Color, num, clientRectangle, rect, rects[rects.Length - 1].Rect, full: true);
			}
		}

		private void PaintText(Canvas g, TabPageRect rects, Tabs owner, TabPage page, SolidBrush brush, bool closshow = false)
		{
			if (page.Enabled)
			{
				if (page.HasIcon)
				{
					if (page.Icon != null)
					{
						g.Image(page.Icon, rects.Rect_Ico);
					}
					if (page.IconSvg != null)
					{
						g.Svg(page.IconSvg, rects.Rect_Ico, brush.Color);
					}
				}
				if ((closable != CloseType.none) & closshow)
				{
					if (rects.hover_close == null)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else if (rects.hover_close.Animation)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Helper.ToColor(rects.hover_close.Value + Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name).A, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name)));
					}
					else if (rects.hover_close.Switch)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
				}
				PaintTextCore(g, rects, owner, page, brush);
			}
			else
			{
				if (page.HasIcon)
				{
					if (page.Icon != null)
					{
						g.Image(page.Icon, rects.Rect_Ico);
					}
					if (page.IconSvg != null)
					{
						g.Svg(page.IconSvg, rects.Rect_Ico, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
				}
				if ((closable != CloseType.none) & closshow)
				{
					if (rects.hover_close == null)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else if (rects.hover_close.Animation)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Helper.ToColor(rects.hover_close.Value + Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name).A, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name)));
					}
					else if (rects.hover_close.Switch)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
				}
				using SolidBrush brush2 = new SolidBrush(Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
				PaintTextCore(g, rects, owner, page, brush2);
			}
			owner.PaintBadge(g, page, rects.Rect_Text);
		}

		private void PaintTextCore(Canvas g, TabPageRect rects, Tabs owner, TabPage page, SolidBrush brush)
		{
			if (owner.IsRotate)
			{
				GraphicsState state = g.Save();
				float dx = (float)rects.Rect_Text.X + (float)rects.Rect_Text.Width / 2f;
				float dy = (float)rects.Rect_Text.Y + (float)rects.Rect_Text.Height / 2f;
				g.TranslateTransform(dx, dy);
				float angle = ((owner.Rotate == TRotate.Clockwise_90) ? 90f : (-90f));
				g.RotateTransform(angle);
				g.String(rect: new RectangleF((float)(-rects.Rect_Text.Height) / 2f, (float)(-rects.Rect_Text.Width) / 2f, rects.Rect_Text.Height, rects.Rect_Text.Width), text: page.Text, font: owner.Font, brush: brush, format: owner.s_c);
				g.Restore(state);
			}
			else
			{
				g.String(page.Text, owner.Font, brush, rects.Rect_Text, owner.s_c);
			}
		}

		public TabPageRect GetTabRect(int i)
		{
			return rects[i];
		}

		public void SelectedIndexChanged(int i, int old)
		{
			if (owner == null)
			{
				return;
			}
			if (old > -1 && i > -1)
			{
				TabPageRect oldTab = rects[old];
				TabPageRect newTab = rects[i];
				if (owner.TabFocusMove(oldTab, newTab, i, rects.Length))
				{
					return;
				}
			}
			owner.Invalidate();
		}

		public void Dispose()
		{
			TabPageRect[] array = rects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].hover_close?.Dispose();
			}
		}

		public bool MouseClick(TabPage page, int i, int x, int y)
		{
			if (owner == null)
			{
				return false;
			}
			if (closable != CloseType.none && rects[i].Rect_Close.Contains(x, y))
			{
				if (owner.IOnClosingPage(page))
				{
					owner.Pages.Remove(page);
					if (owner.CloseDisposePage)
					{
						page.Dispose();
					}
				}
				return true;
			}
			return false;
		}

		public void MouseMove(int x, int y)
		{
			if (owner == null || closable == CloseType.none)
			{
				return;
			}
			int num = 0;
			TabPageRect[] array = rects;
			foreach (TabPageRect tabPageRect in array)
			{
				TabPageRect tabPageRect2 = tabPageRect;
				if (tabPageRect2.hover_close == null)
				{
					tabPageRect2.hover_close = new ITaskOpacity("Tabs", owner);
				}
				if (num == owner.hover_i)
				{
					tabPageRect.hover_close.MaxValue = Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name).A - Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name).A;
					tabPageRect.hover_close.Switch = tabPageRect.Rect_Close.Contains(x, y);
				}
				else
				{
					tabPageRect.hover_close.Switch = false;
				}
				num++;
			}
		}

		public bool MouseMovePre(int x, int y)
		{
			return false;
		}

		public void MouseLeave()
		{
			if (closable == CloseType.none)
			{
				return;
			}
			TabPageRect[] array = rects;
			foreach (TabPageRect tabPageRect in array)
			{
				if (tabPageRect.hover_close != null)
				{
					tabPageRect.hover_close.Switch = false;
				}
			}
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface IStyle
	{
		void LoadLayout(Tabs owner, Rectangle rect, TabCollection items);

		void Paint(Tabs owner, Canvas g, TabCollection items);

		void SelectedIndexChanged(int i, int old);

		bool MouseClick(TabPage page, int i, int x, int y);

		void MouseMove(int x, int y);

		void MouseLeave();

		void Dispose();

		TabPageRect GetTabRect(int i);
	}

	/// <summary>
	/// 线条样式
	/// </summary>
	public class StyleLine : IStyle
	{
		private Tabs? owner;

		private int size = 3;

		private int padding = 8;

		private int radius;

		private bool closable;

		private int backsize = 1;

		private Rectangle rect_ful;

		private Rectangle rect_line_top;

		private TabPageRect[] rects = new TabPageRect[0];

		private int onetmp;

		private bool AnimationBar;

		private RectangleF AnimationBarValue;

		private AnimationTask? ThreadBar;

		/// <summary>
		/// 条大小
		/// </summary>
		[Description("条大小")]
		[Category("样式")]
		[DefaultValue(3)]
		public int Size
		{
			get
			{
				return size;
			}
			set
			{
				if (size != value)
				{
					size = value;
					owner?.LoadLayout(print: true);
				}
			}
		}

		/// <summary>
		/// 条边距
		/// </summary>
		[Description("条边距")]
		[Category("样式")]
		[DefaultValue(8)]
		public int Padding
		{
			get
			{
				return padding;
			}
			set
			{
				if (padding != value)
				{
					padding = value;
					owner?.LoadLayout(print: true);
				}
			}
		}

		/// <summary>
		/// 条圆角
		/// </summary>
		[Description("条圆角")]
		[Category("样式")]
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
					owner?.Invalidate(invalidateChildren: true);
				}
			}
		}

		/// <summary>
		/// 可关闭
		/// </summary>
		[Description("可关闭")]
		[Category("卡片")]
		[DefaultValue(false)]
		public bool Closable
		{
			get
			{
				return closable;
			}
			set
			{
				if (closable != value)
				{
					closable = value;
					owner?.LoadLayout(print: true);
				}
			}
		}

		/// <summary>
		/// 条背景大小
		/// </summary>
		[Description("条背景大小")]
		[Category("样式")]
		[DefaultValue(1)]
		public int BackSize
		{
			get
			{
				return backsize;
			}
			set
			{
				if (backsize != value)
				{
					backsize = value;
					owner?.LoadLayout(print: true);
				}
			}
		}

		/// <summary>
		/// 条背景
		/// </summary>
		[Description("条背景")]
		[Category("样式")]
		[DefaultValue(null)]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color? Back { get; set; }

		public StyleLine()
		{
		}

		public StyleLine(Tabs tabs)
		{
			owner = tabs;
		}

		public void LoadLayout(Tabs tabs, Rectangle rect, TabCollection items)
		{
			rect_ful = rect;
			owner = tabs;
			rects = tabs.GDI((Canvas g) =>
			{
				int num = (int)((float)tabs.Gap * tabs.Dpi);
				int num2 = num * 2;
				int num3 = 0;
				int num4 = (int)((float)Size * tabs.Dpi);
				int num5 = (int)((float)Padding * tabs.Dpi);
				int num6 = num5 * 2;
				List<TabPageRect> list = new List<TabPageRect>(items.Count);
				Dictionary<TabPage, Size> dir = GetDir(tabs, g, items, num, num2, out var ico_size, out var ico_gap, out var close_size, out var sizewh, out var rw);
				int? num7 = (tabs.TextCenter ? new int?(rw) : ((int?)null));
				switch (tabs.Alignment)
				{
				case TabAlignment.Bottom:
				{
					int y3 = rect.Bottom - sizewh;
					foreach (KeyValuePair<TabPage, Size> item in dir)
					{
						if (item.Key.Visible)
						{
							bool flag5 = closable && !item.Key.ReadOnly;
							int num16 = item.Value.Width;
							if (item.Key.HasIcon && num16 == 0)
							{
								num16 = ico_size + ico_gap;
							}
							else
							{
								if (item.Key.HasIcon)
								{
									num16 += ico_size + ico_gap * 2;
								}
								if (flag5)
								{
									num16 += close_size + ico_gap;
								}
							}
							Rectangle rect4 = new Rectangle(rect.X + num3, y3, num16 + num2, sizewh);
							if (flag5)
							{
								if (item.Key.HasIcon)
								{
									list.Add(new TabPageRect(rect4, item.Value, num, ico_size, close_size, ico_gap, left: true).SetLine(num5, num6, num4));
								}
								else
								{
									list.Add(new TabPageRect(rect4, test: false, item.Value, num, close_size, ico_gap, left: true).SetLine(num5, num6, num4));
								}
							}
							else if (item.Key.HasIcon)
							{
								list.Add(new TabPageRect(rect4, item.Value, num, ico_size, ico_gap, left: true).SetLine(num5, num6, num4));
							}
							else
							{
								list.Add(new TabPageRect(rect4, item.Value, num, left: true).SetLine(num5, num6, num4));
							}
							item.Key.SetRect(rect4);
							num3 += rect4.Width;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					tabs.SetPadding(0, 0, 0, sizewh + tabs.Margin.Horizontal);
					if (BackSize > 0)
					{
						int height4 = (int)((float)BackSize * tabs.Dpi);
						rect_line_top = new Rectangle(0, rect.Bottom - sizewh, rect.Width + tabs.Margin.Horizontal, height4);
					}
					owner.scroll_max = num3 - rect.Width;
					owner.scroll_show = num3 > rect.Width;
					break;
				}
				case TabAlignment.Left:
					foreach (KeyValuePair<TabPage, Size> item2 in dir)
					{
						if (item2.Key.Visible)
						{
							bool flag6 = closable && !item2.Key.ReadOnly;
							int height5 = item2.Value.Height;
							int height6;
							if (!owner.IsRotate)
							{
								height6 = ((!item2.Key.HasIcon || height5 != 0) ? (height5 + num) : (ico_size + num));
							}
							else
							{
								int num17 = (item2.Key.HasIcon ? (ico_size + ico_gap) : 0);
								int num18 = (flag6 ? (close_size + ico_gap) : 0);
								height6 = num + num17 + height5 + num18 + num;
							}
							Rectangle rect5 = new Rectangle(rect.X, rect.Y + num3, sizewh, height6);
							Rectangle line2 = new Rectangle(rect5.X + sizewh - num4, rect5.Y + num5, num4, rect5.Height - num6);
							TabPageRect tabPageRect2;
							if (!owner.IsRotate)
							{
								tabPageRect2 = (flag6 ? ((!item2.Key.HasIcon) ? new TabPageRect(rect5, test: false, item2.Value, num, close_size, ico_gap, left: true, num7, rw).SetLine(line2) : new TabPageRect(rect5, item2.Value, num, ico_size, close_size, ico_gap, left: true, num7, rw).SetLine(line2)) : ((!item2.Key.HasIcon) ? new TabPageRect(rect5, item2.Value, num, left: true, num7).SetLine(line2) : new TabPageRect(rect5, item2.Value, num, ico_size, ico_gap, left: true, num7).SetLine(line2)));
							}
							else
							{
								tabPageRect2 = new TabPageRect(rect5, item2.Value, num, left: true, num7).SetLine(line2);
								bool flag7 = owner.Rotate == TRotate.Clockwise_90;
								bool hasIcon2 = item2.Key.HasIcon;
								bool flag8 = flag6;
								if (hasIcon2)
								{
									int x5 = rect5.X + (rect5.Width - ico_size) / 2;
									int y4 = (flag7 ? (rect5.Y + num) : (rect5.Bottom - num - ico_size));
									tabPageRect2.Rect_Ico = new Rectangle(x5, y4, ico_size, ico_size);
								}
								else
								{
									tabPageRect2.Rect_Ico = Rectangle.Empty;
								}
								if (flag8)
								{
									int num19 = close_size;
									int x6 = rect5.X + (rect5.Width - num19) / 2;
									int y5 = (flag7 ? (rect5.Bottom - num - num19) : (rect5.Y + num));
									tabPageRect2.Rect_Close = new Rectangle(x6, y5, num19, num19);
								}
								else
								{
									tabPageRect2.Rect_Close = Rectangle.Empty;
								}
								int num20 = rect5.Y + num;
								int num21 = rect5.Bottom - num;
								if (hasIcon2 & flag7)
								{
									num20 += ico_size + ico_gap;
								}
								if (flag8 && !flag7)
								{
									num20 += close_size + ico_gap;
								}
								if (flag8 & flag7)
								{
									num21 -= close_size + ico_gap;
								}
								if (hasIcon2 && !flag7)
								{
									num21 -= ico_size + ico_gap;
								}
								int height7 = Math.Max(0, num21 - num20);
								int num22 = num7 ?? item2.Value.Width;
								int x7 = rect5.X + Math.Max(0, (rect5.Width - num22) / 2);
								tabPageRect2.Rect_Text = new Rectangle(x7, num20, num22, height7);
							}
							list.Add(tabPageRect2);
							item2.Key.SetRect(rect5);
							num3 += rect5.Height;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					tabs.SetPadding(sizewh + tabs.Margin.Vertical, 0, 0, 0);
					if (BackSize > 0)
					{
						int num23 = (int)((float)BackSize * tabs.Dpi);
						rect_line_top = new Rectangle(rect.X + sizewh - num23, 0, num23, rect.Height + tabs.Margin.Vertical);
					}
					owner.scroll_max = num3 - rect.Height;
					owner.scroll_show = num3 > rect.Height;
					break;
				case TabAlignment.Right:
				{
					int x = rect.Right - sizewh;
					foreach (KeyValuePair<TabPage, Size> item3 in dir)
					{
						if (item3.Key.Visible)
						{
							bool flag2 = closable && !item3.Key.ReadOnly;
							int height = item3.Value.Height;
							int height2;
							if (!owner.IsRotate)
							{
								height2 = ((!item3.Key.HasIcon || height != 0) ? (height + num) : (ico_size + num));
							}
							else
							{
								int num10 = (item3.Key.HasIcon ? (ico_size + ico_gap) : 0);
								int num11 = (flag2 ? (close_size + ico_gap) : 0);
								height2 = num + num10 + height + num11 + num;
							}
							Rectangle rect3 = new Rectangle(x, rect.Y + num3, sizewh, height2);
							Rectangle line = new Rectangle(rect3.X, rect3.Y + num5, num4, rect3.Height - num6);
							TabPageRect tabPageRect;
							if (!owner.IsRotate)
							{
								tabPageRect = (flag2 ? ((!item3.Key.HasIcon) ? new TabPageRect(rect3, test: false, item3.Value, num, close_size, ico_gap, left: false, num7, rw).SetLine(line) : new TabPageRect(rect3, item3.Value, num, ico_size, close_size, ico_gap, left: false, num7, rw).SetLine(line)) : ((!item3.Key.HasIcon) ? new TabPageRect(rect3, item3.Value, num, left: false, num7).SetLine(line) : new TabPageRect(rect3, item3.Value, num, ico_size, ico_gap, left: false, num7).SetLine(line)));
							}
							else
							{
								tabPageRect = new TabPageRect(rect3, item3.Value, num, left: true, num7).SetLine(line);
								bool flag3 = owner.Rotate == TRotate.Clockwise_90;
								bool hasIcon = item3.Key.HasIcon;
								bool flag4 = flag2;
								if (hasIcon)
								{
									int x2 = rect3.X + (rect3.Width - ico_size) / 2;
									int y = (flag3 ? (rect3.Y + num) : (rect3.Bottom - num - ico_size));
									tabPageRect.Rect_Ico = new Rectangle(x2, y, ico_size, ico_size);
								}
								else
								{
									tabPageRect.Rect_Ico = Rectangle.Empty;
								}
								if (flag4)
								{
									int num12 = close_size;
									int x3 = rect3.X + (rect3.Width - num12) / 2;
									int y2 = (flag3 ? (rect3.Bottom - num - num12) : (rect3.Y + num));
									tabPageRect.Rect_Close = new Rectangle(x3, y2, num12, num12);
								}
								else
								{
									tabPageRect.Rect_Close = Rectangle.Empty;
								}
								int num13 = rect3.Y + num;
								int num14 = rect3.Bottom - num;
								if (hasIcon & flag3)
								{
									num13 += ico_size + ico_gap;
								}
								if (flag4 && !flag3)
								{
									num13 += close_size + ico_gap;
								}
								if (flag4 & flag3)
								{
									num14 -= close_size + ico_gap;
								}
								if (hasIcon && !flag3)
								{
									num14 -= ico_size + ico_gap;
								}
								int height3 = Math.Max(0, num14 - num13);
								int num15 = num7 ?? item3.Value.Width;
								int x4 = rect3.X + Math.Max(0, (rect3.Width - num15) / 2);
								tabPageRect.Rect_Text = new Rectangle(x4, num13, num15, height3);
							}
							list.Add(tabPageRect);
							item3.Key.SetRect(rect3);
							num3 += rect3.Height;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					tabs.SetPadding(0, 0, sizewh + tabs.Margin.Vertical, 0);
					if (BackSize > 0)
					{
						int width = (int)((float)BackSize * tabs.Dpi);
						rect_line_top = new Rectangle(x, 0, width, rect.Height + tabs.Margin.Vertical);
					}
					owner.scroll_max = num3 - rect.Height;
					owner.scroll_show = num3 > rect.Height;
					break;
				}
				default:
					foreach (KeyValuePair<TabPage, Size> item4 in dir)
					{
						if (item4.Key.Visible)
						{
							bool flag = closable && !item4.Key.ReadOnly;
							int num8 = item4.Value.Width;
							if (item4.Key.HasIcon && num8 == 0)
							{
								num8 = ico_size + ico_gap;
							}
							else
							{
								if (item4.Key.HasIcon)
								{
									num8 += ico_size + ico_gap * 2;
								}
								if (flag)
								{
									num8 += close_size + ico_gap;
								}
							}
							Rectangle rect2 = new Rectangle(rect.X + num3, rect.Y, num8 + num2, sizewh);
							if (flag)
							{
								if (item4.Key.HasIcon)
								{
									list.Add(new TabPageRect(rect2, item4.Value, num, ico_size, close_size, ico_gap, left: true).SetLineB(num5, num6, num4));
								}
								else
								{
									list.Add(new TabPageRect(rect2, test: false, item4.Value, num, close_size, ico_gap, left: true).SetLineB(num5, num6, num4));
								}
							}
							else if (item4.Key.HasIcon)
							{
								list.Add(new TabPageRect(rect2, item4.Value, num, ico_size, ico_gap, left: true).SetLineB(num5, num6, num4));
							}
							else
							{
								list.Add(new TabPageRect(rect2, item4.Value, num, left: true).SetLineB(num5, num6, num4));
							}
							item4.Key.SetRect(rect2);
							num3 += rect2.Width;
						}
						else
						{
							list.Add(new TabPageRect());
						}
					}
					tabs.SetPadding(0, sizewh + tabs.Margin.Horizontal, 0, 0);
					if (BackSize > 0)
					{
						int num9 = (int)((float)BackSize * tabs.Dpi);
						rect_line_top = new Rectangle(0, rect.Y + sizewh - num9, rect.Width + tabs.Margin.Horizontal, num9);
					}
					owner.scroll_max = num3 - rect.Width;
					owner.scroll_show = num3 > rect.Width;
					break;
				}
				if (owner.scroll_show)
				{
					owner.scroll_max += owner.SizeExceed(rect, list[0].Rect, list[list.Count - 1].Rect);
				}
				else
				{
					Tabs? tabs2 = owner;
					int scroll_x = (owner.scroll_y = 0);
					tabs2.scroll_x = scroll_x;
					if (tabs.centered)
					{
						TabAlignment alignment = tabs.Alignment;
						if ((uint)alignment > 1u && (uint)(alignment - 2) <= 1u)
						{
							int y6 = (rect.Height - num3) / 2;
							foreach (KeyValuePair<TabPage, Size> item5 in dir)
							{
								item5.Key.SetOffset(0, y6);
							}
							foreach (TabPageRect item6 in list)
							{
								item6.Rect.Offset(0, y6);
								item6.Rect_Text.Offset(0, y6);
								item6.Rect_Ico.Offset(0, y6);
								item6.Rect_Line.Offset(0, y6);
							}
						}
						else
						{
							int x8 = (rect.Width - num3) / 2;
							foreach (KeyValuePair<TabPage, Size> item7 in dir)
							{
								item7.Key.SetOffset(x8, 0);
							}
							foreach (TabPageRect item8 in list)
							{
								item8.Rect.Offset(x8, 0);
								item8.Rect_Text.Offset(x8, 0);
								item8.Rect_Ico.Offset(x8, 0);
								item8.Rect_Line.Offset(x8, 0);
							}
						}
					}
				}
				return list.ToArray();
			});
		}

		private Dictionary<TabPage, Size> GetDir(Tabs owner, Canvas g, TabCollection items, int gap, int gap2, out int ico_size, out int ico_gap, out int close_size, out int sizewh, out int rw)
		{
			sizewh = (rw = 0);
			Size size = g.MeasureString("龍Qq", owner.Font);
			Dictionary<TabPage, Size> dictionary = new Dictionary<TabPage, Size>(items.Count);
			int num = 0;
			int? num2 = null;
			foreach (TabPage item in items)
			{
				Size value = g.MeasureString(item.Text, owner.Font);
				if (owner.IsRotate)
				{
					value = new Size(value.Height, value.Width);
				}
				dictionary.Add(item, value);
				if (rw < value.Width)
				{
					rw = value.Width;
				}
				if (item.Visible && !num2.HasValue)
				{
					num2 = num;
				}
				num++;
			}
			onetmp = num2.GetValueOrDefault();
			ico_size = (int)((float)size.Height * owner.iconratio);
			ico_gap = (int)((float)size.Height * owner.icongap);
			close_size = (int)((float)size.Height * (owner.iconratio * 0.8f));
			TabAlignment alignment = owner.Alignment;
			if ((uint)alignment > 1u && (uint)(alignment - 2) <= 1u)
			{
				if (closable)
				{
					foreach (KeyValuePair<TabPage, Size> item2 in dictionary)
					{
						if (item2.Key.Visible)
						{
							int num3;
							if (item2.Key.HasIcon)
							{
								num3 = (owner.IsRotate ? (Math.Max(rw, ico_size) + gap2) : (item2.Value.Width + gap2 + ico_size + ico_gap));
							}
							else
							{
								num3 = (owner.IsRotate ? (rw + gap2) : (item2.Value.Width + gap2));
							}
							num3 += close_size + ico_gap;
							if (sizewh < num3)
							{
								sizewh = num3;
							}
						}
					}
				}
				else
				{
					foreach (KeyValuePair<TabPage, Size> item3 in dictionary)
					{
						if (item3.Key.Visible)
						{
							int num4;
							if (item3.Key.HasIcon)
							{
								num4 = (owner.IsRotate ? (Math.Max(rw, ico_size) + gap2) : (item3.Value.Width + gap2 + ico_size + ico_gap));
							}
							else
							{
								num4 = (owner.IsRotate ? (rw + gap2) : (item3.Value.Width + gap2));
							}
							if (sizewh < num4)
							{
								sizewh = num4;
							}
						}
					}
				}
			}
			else
			{
				foreach (KeyValuePair<TabPage, Size> item4 in dictionary)
				{
					if (item4.Key.Visible)
					{
						int num5 = ((!item4.Key.HasIcon || item4.Value.Height != 0) ? (item4.Value.Height + gap) : (ico_size + gap));
						if (sizewh < num5)
						{
							sizewh = num5;
						}
					}
				}
			}
			return owner.HandItemSize(dictionary, ref sizewh);
		}

		public void Paint(Tabs owner, Canvas g, TabCollection items)
		{
			if (rects.Length == 0)
			{
				return;
			}
			if (BackSize > 0)
			{
				using SolidBrush brush = new SolidBrush(Back ?? Colour.BorderSecondary.Get(owner.ColorScheme, "Tabs", owner.Name));
				g.Fill(brush, rect_line_top);
			}
			if (owner.scroll_show)
			{
				g.SetClip(owner.PaintExceedPre(rect_ful, rects[rects.Length - 1].Rect));
			}
			else
			{
				g.SetClip(rect_ful);
			}
			using SolidBrush solidBrush = new SolidBrush(owner.ForeColor ?? Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush solidBrush2 = new SolidBrush(owner.Fill ?? Colour.Primary.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush solidBrush3 = new SolidBrush(owner.FillActive ?? Colour.PrimaryActive.Get(owner.ColorScheme, "Tabs", owner.Name));
			using SolidBrush solidBrush4 = new SolidBrush(owner.FillHover ?? Colour.PrimaryHover.Get(owner.ColorScheme, "Tabs", owner.Name));
			if (owner.scroll_show)
			{
				g.TranslateTransform(-owner.scroll_x, -owner.scroll_y);
			}
			if (AnimationBar)
			{
				PaintBar(g, AnimationBarValue, solidBrush2);
				int num = 0;
				foreach (TabPage item in items)
				{
					if (item.Visible)
					{
						if (owner.SelectedIndex == num)
						{
							PaintText(g, rects[num], owner, item, solidBrush2);
						}
						else if (owner.hover_i == num)
						{
							PaintText(g, rects[num], owner, item, solidBrush4);
						}
						else
						{
							PaintText(g, rects[num], owner, item, solidBrush);
						}
					}
					num++;
				}
			}
			else
			{
				int num2 = 0;
				if (owner.pageMove == null)
				{
					foreach (TabPage item2 in items)
					{
						if (item2.Visible)
						{
							if (owner.SelectedIndex == num2)
							{
								PaintBar(g, rects[num2].Rect_Line, solidBrush2);
								PaintText(g, rects[num2], owner, item2, solidBrush2);
							}
							else if (owner.hover_i == num2)
							{
								PaintText(g, rects[num2], owner, item2, (owner.pageDown == item2) ? solidBrush3 : solidBrush4);
							}
							else
							{
								PaintText(g, rects[num2], owner, item2, solidBrush);
							}
						}
						num2++;
					}
				}
				else
				{
					int i = 0;
					foreach (TabPage item3 in items)
					{
						if (item3.Visible)
						{
							if (owner.pageMove == item3)
							{
								i = num2;
							}
							else
							{
								PaintItem(owner, g, num2, item3, solidBrush, solidBrush2, solidBrush4, solidBrush3);
							}
						}
						num2++;
					}
					GraphicsState state = g.Save();
					g.TranslateTransform(-owner.offsetx, -owner.offsety);
					PaintItem(owner, g, i, owner.pageMove, solidBrush, solidBrush2, solidBrush4, solidBrush3);
					g.Restore(state);
				}
			}
			if (owner.scroll_show)
			{
				owner.PaintExceed(g, solidBrush.Color, (int)((float)radius * owner.Dpi), rect_ful, rects[onetmp].Rect, rects[rects.Length - 1].Rect, full: false);
			}
		}

		private void PaintItem(Tabs owner, Canvas g, int i, TabPage page, SolidBrush brush_fore, SolidBrush brush_fill, SolidBrush brush_hover, SolidBrush brush_active)
		{
			if (owner.SelectedIndex == i)
			{
				PaintBar(g, rects[i].Rect_Line, brush_fill);
				PaintText(g, rects[i], owner, page, brush_fill);
			}
			else if (owner.hover_i == i)
			{
				PaintText(g, rects[i], owner, page, (owner.pageDown == page) ? brush_active : brush_hover);
			}
			else
			{
				PaintText(g, rects[i], owner, page, brush_fore);
			}
		}

		private void PaintText(Canvas g, TabPageRect rects, Tabs owner, TabPage page, SolidBrush brush)
		{
			if (page.Enabled)
			{
				if (page.HasIcon)
				{
					if (page.Icon != null)
					{
						g.Image(page.Icon, rects.Rect_Ico);
					}
					if (page.IconSvg != null)
					{
						g.Svg(page.IconSvg, rects.Rect_Ico, brush.Color);
					}
				}
				if (closable)
				{
					if (rects.hover_close == null)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else if (rects.hover_close.Animation)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Helper.ToColor(rects.hover_close.Value + Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name).A, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name)));
					}
					else if (rects.hover_close.Switch)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
				}
				PaintTextCore(g, rects, owner, page, brush);
			}
			else
			{
				if (page.HasIcon)
				{
					if (page.Icon != null)
					{
						g.Image(page.Icon, rects.Rect_Ico);
					}
					if (page.IconSvg != null)
					{
						g.Svg(page.IconSvg, rects.Rect_Ico, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
				}
				if (closable)
				{
					if (rects.hover_close == null)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else if (rects.hover_close.Animation)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Helper.ToColor(rects.hover_close.Value + Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name).A, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name)));
					}
					else if (rects.hover_close.Switch)
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
					else
					{
						g.Svg(SvgDb.IcoErrorGhost, rects.Rect_Close, Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
					}
				}
				using SolidBrush brush2 = new SolidBrush(Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name));
				PaintTextCore(g, rects, owner, page, brush2);
			}
			owner.PaintBadge(g, page, rects.Rect_Text);
		}

		private void PaintTextCore(Canvas g, TabPageRect rects, Tabs owner, TabPage page, SolidBrush brush)
		{
			if (owner.IsRotate)
			{
				GraphicsState state = g.Save();
				float dx = (float)rects.Rect_Text.X + (float)rects.Rect_Text.Width / 2f;
				float dy = (float)rects.Rect_Text.Y + (float)rects.Rect_Text.Height / 2f;
				g.TranslateTransform(dx, dy);
				float angle = ((owner.Rotate == TRotate.Clockwise_90) ? 90f : (-90f));
				g.RotateTransform(angle);
				g.String(rect: new RectangleF((float)(-rects.Rect_Text.Height) / 2f, (float)(-rects.Rect_Text.Width) / 2f, rects.Rect_Text.Height, rects.Rect_Text.Width), text: page.Text, font: owner.Font, brush: brush, format: owner.s_c);
				g.Restore(state);
			}
			else
			{
				g.String(page.Text, owner.Font, brush, rects.Rect_Text, owner.s_c);
			}
		}

		private void PaintBar(Canvas g, RectangleF rect, SolidBrush brush)
		{
			if (radius > 0)
			{
				using (GraphicsPath path = rect.RoundPath((float)radius * g.Dpi))
				{
					g.Fill(brush, path);
					return;
				}
			}
			g.Fill(brush, rect);
		}

		private void PaintBar(Canvas g, Rectangle rect, SolidBrush brush)
		{
			if (radius > 0)
			{
				using (GraphicsPath path = rect.RoundPath((float)radius * g.Dpi))
				{
					g.Fill(brush, path);
					return;
				}
			}
			g.Fill(brush, rect);
		}

		public TabPageRect GetTabRect(int i)
		{
			return rects[i];
		}

		private void SetRect(int old, int value)
		{
			if (owner == null || owner.items == null || rects.Length == 0 || owner.items.ListExceed(value))
			{
				return;
			}
			if (owner.items.ListExceed(old))
			{
				AnimationBarValue = rects[value].Rect_Line;
				return;
			}
			ThreadBar?.Dispose();
			TabPageRect oldTab = rects[old];
			TabPageRect newTab = rects[value];
			owner.TabFocusMove(oldTab, newTab, value, rects.Length);
			owner.GDI((Canvas g) =>
			{
				RectangleF OldValue = oldTab.Rect_Line;
				RectangleF NewValue = newTab.Rect_Line;
				if (AnimationBarValue.Height == 0f)
				{
					AnimationBarValue = OldValue;
				}
				if (Config.HasAnimation("Tabs"))
				{
					if (owner.alignment == TabAlignment.Left || owner.alignment == TabAlignment.Right)
					{
						if (OldValue.X == NewValue.X)
						{
							AnimationBarValue.X = OldValue.X;
							AnimationBar = true;
							float p_val = Math.Abs(NewValue.Y - AnimationBarValue.Y) * 0.09f;
							float p_w_val = Math.Abs(NewValue.Height - AnimationBarValue.Height) * 0.1f;
							float p_val2 = (NewValue.Y - AnimationBarValue.Y) * 0.5f;
							ThreadBar = new AnimationTask(new AnimationLoopConfig(owner, () =>
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
										owner.Invalidate();
										return false;
									}
								}
								else
								{
									AnimationBarValue.Y -= p_val;
									if (AnimationBarValue.Y < NewValue.Y)
									{
										AnimationBarValue.Y = NewValue.Y;
										owner.Invalidate();
										return false;
									}
								}
								owner.Invalidate();
								return true;
							}, 10).SetEnd(() =>
							{
								AnimationBarValue = NewValue;
								AnimationBar = false;
								owner.Invalidate();
							}));
							return;
						}
					}
					else if (OldValue.Y == NewValue.Y)
					{
						AnimationBarValue.Y = OldValue.Y;
						AnimationBar = true;
						float p_val3 = Math.Abs(NewValue.X - AnimationBarValue.X) * 0.09f;
						float p_w_val2 = Math.Abs(NewValue.Width - AnimationBarValue.Width) * 0.1f;
						float p_val4 = (NewValue.X - AnimationBarValue.X) * 0.5f;
						ThreadBar = new AnimationTask(new AnimationLoopConfig(owner, () =>
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
									owner.Invalidate();
									return false;
								}
							}
							else
							{
								AnimationBarValue.X -= p_val3;
								if (AnimationBarValue.X < NewValue.X)
								{
									AnimationBarValue.X = NewValue.X;
									owner.Invalidate();
									return false;
								}
							}
							owner.Invalidate();
							return true;
						}, 10).SetEnd(() =>
						{
							AnimationBarValue = NewValue;
							AnimationBar = false;
							owner.Invalidate();
						}));
						return;
					}
				}
				AnimationBarValue = NewValue;
				owner.Invalidate();
			});
		}

		public void SelectedIndexChanged(int i, int old)
		{
			SetRect(old, i);
		}

		public void Dispose()
		{
			ThreadBar?.Dispose();
		}

		public bool MouseClick(TabPage page, int i, int x, int y)
		{
			if (owner == null)
			{
				return false;
			}
			if (closable && !page.ReadOnly && rects[i].Rect_Close.Contains(x, y))
			{
				if (owner.IOnClosingPage(page))
				{
					owner.Pages.Remove(page);
					if (owner.CloseDisposePage)
					{
						page.Dispose();
					}
				}
				return true;
			}
			return false;
		}

		public void MouseMove(int x, int y)
		{
			if (owner == null || !closable)
			{
				return;
			}
			int num = 0;
			TabPageRect[] array = rects;
			foreach (TabPageRect tabPageRect in array)
			{
				TabPageRect tabPageRect2 = tabPageRect;
				if (tabPageRect2.hover_close == null)
				{
					tabPageRect2.hover_close = new ITaskOpacity("Tabs", owner);
				}
				if (num == owner.hover_i)
				{
					tabPageRect.hover_close.MaxValue = Colour.Text.Get(owner.ColorScheme, "Tabs", owner.Name).A - Colour.TextQuaternary.Get(owner.ColorScheme, "Tabs", owner.Name).A;
					tabPageRect.hover_close.Switch = tabPageRect.Rect_Close.Contains(x, y);
				}
				else
				{
					tabPageRect.hover_close.Switch = false;
				}
				num++;
			}
		}

		public void MouseLeave()
		{
			if (!closable)
			{
				return;
			}
			TabPageRect[] array = rects;
			foreach (TabPageRect tabPageRect in array)
			{
				if (tabPageRect.hover_close != null)
				{
					tabPageRect.hover_close.Switch = false;
				}
			}
		}
	}

	internal class TabControlDesigner : ParentControlDesigner
	{
		private DesignerActionListCollection? actionLists;

		public new Tabs Control => (Tabs)base.Control;

		public IDesignerHost? DesignerHost { get; private set; }

		public ISelectionService? SelectionService { get; private set; }

		public override DesignerActionListCollection ActionLists
		{
			get
			{
				if (actionLists == null)
				{
					actionLists = base.ActionLists;
					actionLists.Add(new TabControlActionList(Control));
				}
				return actionLists;
			}
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			DesignerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
			SelectionService = (ISelectionService)GetService(typeof(ISelectionService));
		}

		protected override bool GetHitTest(Point point)
		{
			Point point2 = Control.PointToClient(point);
			foreach (TabPage page in Control.Pages)
			{
				if (page.Contains(point2.X, point2.Y))
				{
					return true;
				}
			}
			return base.GetHitTest(point);
		}
	}

	internal class TabControlActionList : DesignerActionList
	{
		/// <summary>
		/// Gets the associated <see cref="T:System.Windows.Forms.TabControl" />.
		/// </summary>
		public Tabs Control { get; private set; }

		/// <summary>
		/// Gets the designer host.
		/// </summary>
		public IDesignerHost DesignerHost { get; private set; }

		/// <summary>
		/// Gets the selection service.
		/// </summary>
		public ISelectionService SelectionService { get; private set; }

		public TabAlignment Alignment
		{
			get
			{
				return Control.Alignment;
			}
			set
			{
				GetPropertyByName("Alignment").SetValue(Control, value);
			}
		}

		public void AddTab()
		{
			if (DesignerHost != null)
			{
				TabPage tabPage = (TabPage)DesignerHost.CreateComponent(typeof(TabPage));
				string value = (string)TypeDescriptor.GetProperties(tabPage)["Name"]?.GetValue(tabPage);
				TypeDescriptor.GetProperties(tabPage)["Text"]?.SetValue(tabPage, value);
				Control.Pages.Add(tabPage);
				Control.SelectedIndex = Control.Pages.IndexOf(tabPage);
			}
		}

		protected void RemoveTab()
		{
			if (DesignerHost != null && Control.Pages.Count > 1)
			{
				int selectedIndex = Control.SelectedIndex;
				Control.SelectedIndex = selectedIndex;
			}
		}

		public TabControlActionList(IComponent component)
			: base(component)
		{
			Control = (Tabs)component;
			DesignerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
			SelectionService = (ISelectionService)GetService(typeof(ISelectionService));
		}

		private PropertyDescriptor GetPropertyByName(string propName)
		{
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(Control)[propName];
			if (propertyDescriptor == null)
			{
				throw new ArgumentException("Unknown property.", propName);
			}
			return propertyDescriptor;
		}

		public override DesignerActionItemCollection GetSortedActionItems()
		{
			return new DesignerActionItemCollection
			{
				new DesignerActionHeaderItem("外观"),
				new DesignerActionHeaderItem("数据"),
				new DesignerActionPropertyItem("Alignment", "选项卡位置", "外观", "确定选项卡是否显示在控件的顶部、底部、左侧或右侧(在左侧或右侧将隐式地分为多行)。"),
				new DesignerActionMethodItem(this, "AddTab", "添加选项卡", "数据", "向控件添加新选项卡"),
				new DesignerActionMethodItem(this, "RemoveTab", "移除选项卡", "数据", "删除当前选项卡")
			};
		}
	}

	private Color? fore;

	private Color? fill;

	private TabAlignment alignment;

	private bool centered;

	private bool textCenter;

	private TabTypExceed typExceed = TabTypExceed.Button;

	private Color? scrollback;

	private Color? scrollfore;

	private IStyle style;

	private TabType type;

	private int _gap = 8;

	private float iconratio = 0.7f;

	private float icongap = 0.25f;

	private bool _tabMenuVisible = true;

	private int? _itemSize;

	private TRotate rotate;

	internal bool IsRotate;

	private TabCollection? items;

	private int _select;

	private bool showok;

	private Padding _padding = new Padding(0);

	private readonly FormatFlags s_c = FormatFlags.Center | FormatFlags.NoWrapEllipsis;

	private readonly FormatFlags s_f = FormatFlags.Center | FormatFlags.NoWrap;

	private int clicks = 1;

	private TabPage? pageDown;

	private TabPage? pageMove;

	private int oldXY;

	private int offsetx;

	private int offsety;

	private int hover_i = -1;

	internal bool scroll_show;

	private int _scroll_x;

	private int _scroll_y;

	private int scroll_max;

	private LayeredFormSelectDown? subForm;

	private Bitmap? bitblock_l;

	private Bitmap? bitblock_r;

	private Rectangle rect_l;

	private Rectangle rect_r;

	private bool hover_l;

	private bool hover_r;

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
	/// 颜色
	/// </summary>
	[Description("颜色")]
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
	/// 悬停颜色
	/// </summary>
	[Description("悬停颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? FillHover { get; set; }

	/// <summary>
	/// 激活颜色
	/// </summary>
	[Description("激活颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? FillActive { get; set; }

	/// <summary>
	/// 位置
	/// </summary>
	[Description("位置")]
	[Category("Appearance")]
	[DefaultValue(TabAlignment.Top)]
	public TabAlignment Alignment
	{
		get
		{
			return alignment;
		}
		set
		{
			if (alignment != value)
			{
				alignment = value;
				SetIsRotate();
				LoadLayout(print: true);
				OnPropertyChanged("Alignment");
			}
		}
	}

	/// <summary>
	/// 标签居中展示
	/// </summary>
	[Description("标签居中展示")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Centered
	{
		get
		{
			return centered;
		}
		set
		{
			if (centered != value)
			{
				centered = value;
				LoadLayout(print: true);
				OnPropertyChanged("Centered");
			}
		}
	}

	/// <summary>
	/// 文本是否居中对齐（仅在Left/Right方向生效）
	/// </summary>
	[Description("文本是否居中对齐（仅在Left/Right方向生效）")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool TextCenter
	{
		get
		{
			return textCenter;
		}
		set
		{
			if (textCenter != value)
			{
				textCenter = value;
				LoadLayout(print: true);
				OnPropertyChanged("TextCenter");
			}
		}
	}

	/// <summary>
	/// 超出UI类型
	/// </summary>
	[Description("超出UI类型")]
	[Category("Appearance")]
	[DefaultValue(TabTypExceed.Button)]
	public TabTypExceed TypExceed
	{
		get
		{
			return typExceed;
		}
		set
		{
			if (typExceed != value)
			{
				typExceed = value;
				bitblock_l?.Dispose();
				bitblock_r?.Dispose();
				bitblock_l = (bitblock_r = null);
				LoadLayout(print: true);
				OnPropertyChanged("TypExceed");
			}
		}
	}

	/// <summary>
	/// 切换使能
	/// </summary>
	[Description("切换使能")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool EnableSwitch { get; set; } = true;

	/// <summary>
	/// 鼠标滚轮切换焦点页使能
	/// </summary>
	[Description("鼠标滚轮切换焦点页使能")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool EnablePageScrolling { get; set; } = true;

	/// <summary>
	/// 鼠标中键关闭使能
	/// </summary>
	[Description("鼠标中键关闭使能")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool EnablePageCloseByMouseMiddle { get; set; } = true;

	/// <summary>
	/// 鼠标双击关闭使能
	/// </summary>
	[Description("鼠标双击关闭使能")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool EnablePageCloseByMouseDoubleClick { get; set; } = true;

	/// <summary>
	/// 关闭页面后释放
	/// </summary>
	[Description("关闭页面后释放")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool CloseDisposePage { get; set; }

	/// <summary>
	/// 滚动条颜色
	/// </summary>
	[Description("滚动条颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ScrollBack
	{
		get
		{
			return scrollback;
		}
		set
		{
			if (!(scrollback == value))
			{
				scrollback = value;
				bitblock_l?.Dispose();
				bitblock_r?.Dispose();
				bitblock_l = (bitblock_r = null);
				Invalidate();
				OnPropertyChanged("ScrollBack");
			}
		}
	}

	/// <summary>
	/// 滚动条文本颜色
	/// </summary>
	[Description("滚动条文本颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ScrollFore
	{
		get
		{
			return scrollfore;
		}
		set
		{
			if (!(scrollfore == value))
			{
				scrollfore = value;
				Invalidate();
				OnPropertyChanged("ScrollFore");
			}
		}
	}

	/// <summary>
	/// 滚动条悬停文本颜色
	/// </summary>
	[Description("滚动条悬停文本颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ScrollForeHover { get; set; }

	/// <summary>
	/// 滚动条悬停颜色
	/// </summary>
	[Description("滚动条悬停颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ScrollBackHover { get; set; }

	/// <summary>
	/// 样式
	/// </summary>
	[Description("样式")]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public IStyle Style
	{
		get
		{
			return style;
		}
		set
		{
			style = value;
		}
	}

	/// <summary>
	/// 样式类型
	/// </summary>
	[Description("样式类型")]
	[Category("Appearance")]
	[DefaultValue(TabType.Line)]
	public TabType Type
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
				style = SetType(value);
				LoadLayout(print: true);
				OnPropertyChanged("Type");
			}
		}
	}

	/// <summary>
	/// 间距
	/// </summary>
	[Description("间距")]
	[Category("Appearance")]
	[DefaultValue(8)]
	public int Gap
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
				LoadLayout(print: true);
				OnPropertyChanged("Gap");
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
				LoadLayout(print: true);
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

	[Description("是否显示头")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool TabMenuVisible
	{
		get
		{
			return _tabMenuVisible;
		}
		set
		{
			_tabMenuVisible = value;
			LoadLayout(print: true);
			OnPropertyChanged("TabMenuVisible");
		}
	}

	/// <summary>
	/// 自定义项宽度
	/// </summary>
	[Description("自定义项大小")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? ItemSize
	{
		get
		{
			return _itemSize;
		}
		set
		{
			if (_itemSize != value)
			{
				_itemSize = value;
				LoadLayout(print: true);
				OnPropertyChanged("ItemSize");
			}
		}
	}

	/// <summary>
	/// 拖动顺序
	/// </summary>
	[Description("拖动顺序")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool DragOrder { get; set; }

	/// <summary>
	/// 旋转（用于 Left/Right 时竖排显示）
	/// </summary>
	[Description("旋转（用于 Left/Right 时竖排显示）")]
	[Category("Appearance")]
	[DefaultValue(TRotate.None)]
	public TRotate Rotate
	{
		get
		{
			return rotate;
		}
		set
		{
			if (rotate != value)
			{
				rotate = value;
				SetIsRotate();
				LoadLayout(print: true);
				OnPropertyChanged("Rotate");
			}
		}
	}

	public override Rectangle DisplayRectangle => ClientRectangle.PaddingRect(Padding, _padding);

	/// <summary>
	/// 数据
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("Data")]
	public TabCollection Pages
	{
		get
		{
			if (items == null)
			{
				items = new TabCollection(this);
			}
			return items;
		}
		set
		{
			items?.Clear();
			items = value.BindData(this);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public TabPage? SelectedTab
	{
		get
		{
			if (items == null || items.Count <= _select || _select < 0)
			{
				return null;
			}
			return items[_select];
		}
		set
		{
			if (items != null && value != null)
			{
				int selectedIndex = items.IndexOf(value);
				SelectedIndex = selectedIndex;
				OnPropertyChanged("SelectedTab");
			}
		}
	}

	[Description("选中序号")]
	[Category("Data")]
	[DefaultValue(0)]
	public int SelectedIndex
	{
		get
		{
			return _select;
		}
		set
		{
			if (_select != value && OnSelectedIndexChanging(value))
			{
				int old = _select;
				_select = value;
				style.SelectedIndexChanged(value, old);
				Invalidate();
				ShowPage(_select);
				OnSelectedIndexChanged(value);
				OnPropertyChanged("SelectedIndex");
			}
		}
	}

	private int Hover_i
	{
		get
		{
			return hover_i;
		}
		set
		{
			if (hover_i != value)
			{
				hover_i = value;
				Invalidate();
			}
		}
	}

	private int scroll_x
	{
		get
		{
			return _scroll_x;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			else if (scroll_max > 0 && value > scroll_max)
			{
				value = scroll_max;
			}
			if (value != _scroll_x)
			{
				_scroll_x = value;
				Invalidate();
			}
		}
	}

	private int scroll_y
	{
		get
		{
			return _scroll_y;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			else if (scroll_max > 0 && value > scroll_max)
			{
				value = scroll_max;
			}
			if (value != _scroll_y)
			{
				_scroll_y = value;
				Invalidate();
			}
		}
	}

	private bool Hover_l
	{
		get
		{
			return hover_l;
		}
		set
		{
			if (hover_l != value)
			{
				hover_l = value;
				Invalidate();
			}
		}
	}

	private bool Hover_r
	{
		get
		{
			return hover_r;
		}
		set
		{
			if (hover_r != value)
			{
				hover_r = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// SelectedIndex 属性值更改时发生
	/// </summary>
	[Description("SelectedIndex 属性值更改时发生")]
	[Category("Behavior")]
	public event IntEventHandler? SelectedIndexChanged;

	/// <summary>
	/// SelectedIndex 属性值更改前发生
	/// </summary>
	[Description("SelectedIndex 属性值更改前发生")]
	[Category("Behavior")]
	public event TabsIndexChangingEventHandler? SelectedIndexChanging;

	/// <summary>
	/// 关闭页面前发生
	/// </summary>
	[Description("关闭页面前发生")]
	[Category("Behavior")]
	public event ClosingPageEventHandler? ClosingPage;

	/// <summary>
	/// 点击标签时发生
	/// </summary>
	[Description("点击标签时发生")]
	[Category("Behavior")]
	public event TabsItemEventHandler? TabClick;

	/// <summary>
	/// 点击空白标签时发生
	/// </summary>
	[Description("点击空白标签时发生")]
	[Category("Behavior")]
	public event MouseEventHandler? NonTabClick;

	public Tabs()
	{
		style = SetType(type);
	}

	private IStyle SetType(TabType type)
	{
		switch (type)
		{
		case TabType.Card:
			if (style is StyleCard result2)
			{
				return result2;
			}
			return new StyleCard(this);
		case TabType.Card2:
			if (style is StyleCard2 result3)
			{
				return result3;
			}
			return new StyleCard2(this);
		default:
			if (style is StyleLine result)
			{
				return result;
			}
			return new StyleLine(this);
		}
	}

	internal Dictionary<TabPage, Size> HandItemSize(Dictionary<TabPage, Size> rect_dir, ref int sizewh)
	{
		if (_itemSize.HasValue)
		{
			int num = (int)Math.Ceiling((float)_itemSize.Value * Dpi);
			Dictionary<TabPage, Size> dictionary = new Dictionary<TabPage, Size>(rect_dir.Count);
			foreach (KeyValuePair<TabPage, Size> item in rect_dir)
			{
				dictionary.Add(item.Key, new Size(num, item.Value.Height));
			}
			if (alignment == TabAlignment.Left || alignment == TabAlignment.Right)
			{
				sizewh = num;
			}
			return dictionary;
		}
		return rect_dir;
	}

	private void SetIsRotate()
	{
		IsRotate = (alignment == TabAlignment.Left || alignment == TabAlignment.Right) && rotate != TRotate.None;
	}

	public void SelectTab(string tabPageName)
	{
		if (items == null)
		{
			return;
		}
		foreach (TabPage item in items)
		{
			if (item.Name == tabPageName)
			{
				SelectedTab = item;
				break;
			}
		}
	}

	public void SelectTab(TabPage tabPage)
	{
		SelectedTab = tabPage;
	}

	public void SelectTab(int index)
	{
		SelectedIndex = index;
	}

	/// <summary>
	/// 添加页面并设置激活
	/// </summary>
	public void AddTabSelect(TabPage page)
	{
		if (items == null)
		{
			items = new TabCollection(this);
		}
		int index = (_select = items.Count);
		items.Add(page);
		if (IsHandleCreated)
		{
			ShowPage(index);
		}
	}

	internal void ShowPage(int index)
	{
		if (!showok || items == null)
		{
			return;
		}
		try
		{
			BeginInvoke(() =>
			{
				if (items.Count > index && index >= 0)
				{
					for (int i = 0; i < items.Count; i++)
					{
						TabPage tabPage = items[i];
						bool showed = i == index;
						tabPage.Showed = showed;
					}
				}
			});
		}
		catch
		{
		}
	}

	protected override void Dispose(bool disposing)
	{
		style.Dispose();
		bitblock_l?.Dispose();
		bitblock_r?.Dispose();
		items?.Dispose();
		base.Dispose(disposing);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		this.AddListener();
		LoadLayout();
		showok = true;
		ShowPage(_select);
	}

	protected override void OnMarginChanged(EventArgs e)
	{
		LoadLayout();
		base.OnMarginChanged(e);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		LoadLayout();
		base.OnFontChanged(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		LoadLayout();
		base.OnSizeChanged(e);
	}

	private bool SetPadding(int x, int y, int r, int b)
	{
		if (_padding.Left == x && _padding.Top == y && _padding.Right == r && _padding.Bottom == b)
		{
			return true;
		}
		_padding.Left = x;
		_padding.Top = y;
		_padding.Right = r;
		_padding.Bottom = b;
		base.OnSizeChanged(EventArgs.Empty);
		return false;
	}

	private bool CanLayout(out bool canPrint)
	{
		if (IsHandleCreated)
		{
			Rectangle clientRectangle = ClientRectangle;
			if (clientRectangle.Width == 0 || clientRectangle.Height == 0)
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

	internal void LoadLayout(bool print = false)
	{
		if (CanLayout(out var canPrint))
		{
			if (_tabMenuVisible)
			{
				style.LoadLayout(this, ClientRectangle.DeflateRect(Margin), items);
			}
			else
			{
				SetPadding(0, 0, 0, 0);
			}
		}
		if (print & canPrint)
		{
			Invalidate();
		}
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		if (items != null && items.Count != 0 && _tabMenuVisible)
		{
			Canvas canvas = e.Canvas;
			style.Paint(this, canvas, items);
			base.OnDraw(e);
		}
	}

	private void PaintBadge(Canvas g, TabPage page, Rectangle rect)
	{
		if (page.Badge == null)
		{
			return;
		}
		Color color = page.BadgeBack ?? Colour.Error.Get(ColorScheme, "Tabs", Name);
		using SolidBrush solidBrush = new SolidBrush(Colour.ErrorColor.Get(ColorScheme, "Tabs", Name));
		using Font font = new Font(Font.FontFamily, Font.Size * page.BadgeSize);
		if (string.IsNullOrEmpty(page.Badge) || page.Badge == "" || page.Badge == " ")
		{
			int num = g.MeasureString("龍Qq", font).Width / 2;
			RectangleF rect2 = new RectangleF((float)(rect.Right - num) - (float)page.BadgeOffsetX * Dpi, (float)rect.Y + (float)page.BadgeOffsetY * Dpi, num, num);
			using SolidBrush brush = new SolidBrush(color);
			g.FillEllipse(brush, rect2);
			g.DrawEllipse(solidBrush.Color, Dpi, rect2);
			return;
		}
		Size size = g.MeasureString(page.Badge, font);
		int num2 = (int)((float)size.Height * 1.2f);
		if (size.Height > size.Width)
		{
			Rectangle rect3 = new Rectangle(rect.Right - num2 - (int)((float)page.BadgeOffsetX * Dpi), rect.Y + (int)((float)page.BadgeOffsetY * Dpi), num2, num2);
			using (SolidBrush brush2 = new SolidBrush(color))
			{
				g.FillEllipse(brush2, rect3);
				g.DrawEllipse(solidBrush.Color, Dpi, rect3);
			}
			g.String(page.Badge, font, solidBrush, rect3, s_f);
			return;
		}
		int num3 = size.Width + (num2 - size.Height);
		Rectangle rect4 = new Rectangle(rect.Right - num3 - (int)((float)page.BadgeOffsetX * Dpi), rect.Y + (int)((float)page.BadgeOffsetY * Dpi), num3, num2);
		using (SolidBrush brush3 = new SolidBrush(color))
		{
			using GraphicsPath path = rect4.RoundPath(rect4.Height);
			g.Fill(brush3, path);
			g.Draw(solidBrush.Color, Dpi, path);
		}
		g.String(page.Badge, font, solidBrush, rect4, s_f);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		pageDown = (pageMove = null);
		offsetx = (offsety = 0);
		base.OnMouseDown(e);
		clicks = e.Clicks;
		if (items == null || MouseDownPre(e.X, e.Y) || !_tabMenuVisible || !EnableSwitch)
		{
			return;
		}
		int num = 0;
		int num2 = e.X + scroll_x;
		int num3 = e.Y + scroll_y;
		foreach (TabPage item in items)
		{
			if (item.Visible && item.Contains(num2, num3))
			{
				if (item.Enabled)
				{
					pageDown = item;
					Invalidate();
				}
				break;
			}
			num++;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (items == null)
		{
			return;
		}
		if (pageMove != null)
		{
			if (alignment == TabAlignment.Top || alignment == TabAlignment.Bottom)
			{
				int num = oldXY - e.X;
				offsetx += num;
				Invalidate();
				oldXY = e.X;
			}
			else
			{
				int num2 = oldXY - e.Y;
				offsety += num2;
				Invalidate();
				oldXY = e.Y;
			}
			return;
		}
		if (DragOrder && pageDown != null)
		{
			if (alignment == TabAlignment.Top || alignment == TabAlignment.Bottom)
			{
				int num3 = Math.Abs(oldXY - e.X);
				int num4 = (int)((float)Config.TouchThreshold * Dpi);
				if (num3 > num4)
				{
					oldXY = e.X;
					pageMove = pageDown;
					return;
				}
			}
			else
			{
				int num5 = Math.Abs(oldXY - e.Y);
				int num6 = (int)((float)Config.TouchThreshold * Dpi);
				if (num5 > num6)
				{
					oldXY = e.Y;
					pageMove = pageDown;
					return;
				}
			}
		}
		if (MouseMovePre(e.X, e.Y))
		{
			Hover_i = -1;
			SetCursor(val: true);
			return;
		}
		int num7 = 0;
		int num8 = e.X + scroll_x;
		int num9 = e.Y + scroll_y;
		foreach (TabPage item in items)
		{
			if (item.Visible && item.Contains(num8, num9))
			{
				if (item.Enabled && EnableSwitch)
				{
					SetCursor(val: true);
					Hover_i = num7;
					style.MouseMove(num8, num9);
				}
				else
				{
					SetCursor(val: false);
				}
				return;
			}
			num7++;
		}
		style.MouseMove(num8, num9);
		SetCursor(val: false);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		TabPage tabPage = pageDown;
		TabPage tabPage2 = pageMove;
		pageDown = (pageMove = null);
		base.OnMouseUp(e);
		if (items == null)
		{
			return;
		}
		Helper.GenerateMouseEventArgs(ref e, clicks);
		if (!_tabMenuVisible)
		{
			return;
		}
		if (tabPage2 != null)
		{
			Rectangle currentRect = ((alignment != TabAlignment.Top && alignment != TabAlignment.Bottom) ? tabPage2.GetRect(0, scroll_y - offsety) : tabPage2.GetRect(scroll_x - offsetx, 0));
			TabPage tabPage3 = FindNearestPage(currentRect, items, tabPage2);
			if (tabPage3 != tabPage2)
			{
				int num = items.IndexOf(tabPage2);
				int num2 = items.IndexOf(tabPage3);
				items.InsertAntRemove(num2, tabPage2);
				if (_select == num2)
				{
					SelectedIndex = num;
				}
				else if (_select == num)
				{
					SelectedIndex = num2;
				}
				return;
			}
			Invalidate();
		}
		if (tabPage == null)
		{
			OnNonTabClick(e);
			return;
		}
		int num3 = 0;
		int num4 = e.X + scroll_x;
		int num5 = e.Y + scroll_y;
		foreach (TabPage item in items)
		{
			if (item == tabPage)
			{
				if (tabPage.Contains(num4, num5))
				{
					if (!style.MouseClick(tabPage, num3, num4, num5))
					{
						OnTabClick(tabPage, num3, style, e);
					}
				}
				else
				{
					Invalidate();
				}
				break;
			}
			num3++;
		}
	}

	private TabPage FindNearestPage(Rectangle currentRect, TabCollection items, TabPage page)
	{
		double num = 2147483647.0;
		TabPage tabPage = null;
		foreach (TabPage item in items)
		{
			double num2 = CalculateDistance(currentRect, item.Rect);
			if (num2 < num)
			{
				num = num2;
				tabPage = item;
			}
		}
		return tabPage ?? page;
	}

	private double CalculateDistance(Rectangle rect1, Rectangle rect2)
	{
		int num = rect1.X + rect1.Width / 2 - (rect2.X + rect2.Width / 2);
		int num2 = rect1.Y + rect1.Height / 2 - (rect2.Y + rect2.Height / 2);
		return Math.Sqrt(num * num + num2 * num2);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		style.MouseLeave();
		bool flag = (Hover_r = false);
		Hover_l = flag;
		Hover_i = -1;
		SetCursor(val: false);
		base.OnMouseLeave(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		MouseWheelCore(e);
		base.OnMouseWheel(e);
	}

	private void MouseWheelCore(MouseEventArgs e)
	{
		if (!scroll_show && EnablePageScrolling && ContainsTabPage(e.X, e.Y) != null)
		{
			int num = SelectedIndex + ((e.Delta <= 0) ? 1 : (-1));
			if (num > -1 && num < Pages.Count)
			{
				SelectedIndex = num;
				return;
			}
		}
		if (MouseWheelCore(e.Delta) && e is HandledMouseEventArgs e2)
		{
			e2.Handled = true;
		}
	}

	private bool MouseWheelCore(int delta)
	{
		if (scroll_show)
		{
			if (delta == 0)
			{
				return false;
			}
			TabAlignment tabAlignment = alignment;
			if ((uint)tabAlignment > 1u && (uint)(tabAlignment - 2) <= 1u)
			{
				int num = _scroll_y;
				scroll_x = 0;
				scroll_y -= delta;
				if (num == _scroll_y)
				{
					return false;
				}
				return true;
			}
			int num2 = _scroll_x;
			scroll_y = 0;
			scroll_x -= delta;
			if (num2 == _scroll_x)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// 判断鼠标在不在标签上
	/// </summary>
	public TabPage? ContainsTabPage(int x, int y)
	{
		if (items == null)
		{
			return null;
		}
		int num = x + scroll_x;
		int num2 = y + scroll_y;
		foreach (TabPage item in items)
		{
			if (item.Visible && item.Contains(num, num2))
			{
				return item;
			}
		}
		return null;
	}

	private bool TabFocusMove(TabPageRect oldTab, TabPageRect newTab, int value, int max)
	{
		if (scroll_show)
		{
			TabAlignment tabAlignment = alignment;
			if ((uint)tabAlignment > 1u && (uint)(tabAlignment - 2) <= 1u)
			{
				int num = scroll_y;
				if (newTab.Rect.Y > num && newTab.Rect.Bottom < num + ClientRectangle.Height)
				{
					return false;
				}
				if (value == 0)
				{
					scroll_y = 0;
				}
				else if (value == max - 1)
				{
					scroll_y = scroll_max;
				}
				else
				{
					scroll_y = newTab.Rect.Y;
				}
				return true;
			}
			int num2 = scroll_x;
			if (newTab.Rect.X > num2 && newTab.Rect.Right < num2 + ClientRectangle.Width)
			{
				return false;
			}
			if (value == 0)
			{
				scroll_x = 0;
			}
			else if (value == max - 1)
			{
				scroll_x = scroll_max;
			}
			else
			{
				scroll_x = newTab.Rect.X;
			}
			return true;
		}
		return false;
	}

	private bool MouseMovePre(int x, int y)
	{
		if (items == null)
		{
			return false;
		}
		switch (typExceed)
		{
		case TabTypExceed.Button:
			if (scroll_show && rect_r.Contains(x, y))
			{
				if (subForm == null)
				{
					List<SelectItem> list = new List<SelectItem>(items.Count);
					foreach (TabPage item in items)
					{
						list.Add(new SelectItem(item.Text, item));
					}
					subForm = new LayeredFormSelectDown(this, list.ToArray(), 6, SelectedTab, rect_r);
					subForm.Disposed += (object a, EventArgs b) =>
					{
						subForm = null;
					};
					subForm.MouseLeave += (object a, EventArgs b) =>
					{
						if (a is LayeredFormSelectDown layeredFormSelectDown)
						{
							layeredFormSelectDown.IClose();
						}
					};
					subForm.Leave += (object a, EventArgs b) =>
					{
						if (a is LayeredFormSelectDown layeredFormSelectDown)
						{
							layeredFormSelectDown.IClose();
						}
					};
					subForm.Show(this);
				}
				return true;
			}
			subForm?.IClose();
			subForm = null;
			break;
		case TabTypExceed.LR:
		case TabTypExceed.LR_Shadow:
			if (scroll_show)
			{
				if (MouseDownLRL(x, y, out var _))
				{
					Hover_l = true;
					return true;
				}
				Hover_l = false;
				if (MouseDownLRR(x, y, out var _))
				{
					Hover_r = true;
					return true;
				}
				Hover_r = false;
			}
			break;
		}
		return false;
	}

	private bool MouseDownPre(int x, int y)
	{
		if (items == null)
		{
			return false;
		}
		if (scroll_show && (typExceed == TabTypExceed.LR || typExceed == TabTypExceed.LR_Shadow))
		{
			if (MouseDownLRL(x, y, out var lr))
			{
				if (lr)
				{
					scroll_y -= 120;
				}
				else
				{
					scroll_x -= 120;
				}
				return true;
			}
			if (MouseDownLRR(x, y, out var lr2))
			{
				if (lr2)
				{
					scroll_y += 120;
				}
				else
				{
					scroll_x += 120;
				}
				return true;
			}
		}
		return false;
	}

	private bool MouseDownLRL(int x, int y, out bool lr)
	{
		TabAlignment tabAlignment = alignment;
		if ((uint)tabAlignment > 1u && (uint)(tabAlignment - 2) <= 1u)
		{
			lr = true;
			if (scroll_y > 0 && rect_l.Contains(x, y))
			{
				return true;
			}
			return false;
		}
		lr = false;
		if (scroll_x > 0 && rect_l.Contains(x, y))
		{
			return true;
		}
		return false;
	}

	private bool MouseDownLRR(int x, int y, out bool lr)
	{
		TabAlignment tabAlignment = alignment;
		if ((uint)tabAlignment > 1u && (uint)(tabAlignment - 2) <= 1u)
		{
			lr = true;
			if (scroll_max != scroll_y && rect_r.Contains(x, y))
			{
				return true;
			}
			return false;
		}
		lr = false;
		if (scroll_max != scroll_x && rect_r.Contains(x, y))
		{
			return true;
		}
		return false;
	}

	public virtual int SizeExceed(Rectangle rect, Rectangle first, Rectangle last)
	{
		TabTypExceed tabTypExceed = typExceed;
		if (tabTypExceed != TabTypExceed.None && tabTypExceed == TabTypExceed.Button)
		{
			int num = last.Height;
			TabAlignment tabAlignment = alignment;
			if ((uint)tabAlignment > 1u && (uint)(tabAlignment - 2) <= 1u)
			{
				if (IsRotate)
				{
					num = last.Width;
				}
				rect_r = new Rectangle(last.X, rect.Bottom - num, last.Width, num);
			}
			else
			{
				rect_r = new Rectangle(rect.Right - num, last.Y, num, num);
			}
			return num;
		}
		rect_r = Rectangle.Empty;
		return 0;
	}

	public virtual Rectangle PaintExceedPre(Rectangle rect, Rectangle rect_btn)
	{
		int num = (IsRotate ? rect_btn.Width : rect_btn.Height);
		switch (typExceed)
		{
		case TabTypExceed.Button:
		{
			TabAlignment tabAlignment = alignment;
			if ((uint)tabAlignment > 1u && (uint)(tabAlignment - 2) <= 1u)
			{
				if (scroll_max != scroll_y)
				{
					return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height - num);
				}
				return rect;
			}
			if (scroll_max != scroll_x)
			{
				return new Rectangle(rect.X, rect.Y, rect.Width - num, rect.Height);
			}
			return rect;
		}
		case TabTypExceed.LR:
		case TabTypExceed.LR_Shadow:
		{
			TabAlignment tabAlignment = alignment;
			if ((uint)tabAlignment > 1u && (uint)(tabAlignment - 2) <= 1u)
			{
				int num2 = (int)((float)num * 0.6f);
				if (scroll_max != scroll_y)
				{
					if (scroll_y > 0)
					{
						return new Rectangle(rect.X, rect.Y + num2, rect.Width, rect.Height - num2 * 2);
					}
					return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height - num2);
				}
				if (scroll_y > 0)
				{
					return new Rectangle(rect.X, rect.Y + num2, rect.Width, rect.Height - num2);
				}
				return rect;
			}
			int num3 = (int)((float)num * 0.6f);
			if (scroll_max != scroll_x)
			{
				if (scroll_x > 0)
				{
					return new Rectangle(rect.X + num3, rect.Y, rect.Width - num3 * 2, rect.Height);
				}
				return new Rectangle(rect.X, rect.Y, rect.Width - num3, rect.Height);
			}
			if (scroll_x > 0)
			{
				return new Rectangle(rect.X + num3, rect.Y, rect.Width - num3, rect.Height);
			}
			return rect;
		}
		default:
			return rect;
		}
	}

	public virtual void PaintExceed(Canvas g, Color color, int radius, Rectangle rect, Rectangle first, Rectangle last, bool full)
	{
		switch (typExceed)
		{
		case TabTypExceed.Button:
			PaintExceedButton(g, color, radius, rect, first, last, full);
			break;
		case TabTypExceed.LR:
			PaintExceedLR(g, color, radius, rect, first, last, full);
			break;
		case TabTypExceed.LR_Shadow:
			PaintExceedLR_Shadow(g, color, radius, rect, first, last, full);
			break;
		case TabTypExceed.None:
			break;
		}
	}

	public virtual void PaintExceedButton(Canvas g, Color color, int radius, Rectangle rect, Rectangle first, Rectangle last, bool full)
	{
		g.ResetClip();
		g.ResetTransform();
		TabAlignment tabAlignment = alignment;
		if ((uint)tabAlignment > 1u && (uint)(tabAlignment - 2) <= 1u)
		{
			if (scroll_y <= 0 && scroll_max == scroll_y)
			{
				return;
			}
			int num = (int)((float)_gap * Dpi);
			int num2 = num * 2;
			int num3 = (IsRotate ? last.Width : last.Height);
			int num4 = (int)((float)num3 * 0.4f);
			Rectangle rectangle = new Rectangle(last.X, rect.Bottom - num3, last.Width, num3);
			if (scroll_y > 0)
			{
				Rectangle rect2 = new Rectangle(first.X, first.Y, rectangle.Width, num2);
				if (full)
				{
					rect2.Y = 0;
				}
				if (bitblock_l == null || bitblock_l.Width != rect2.Width || bitblock_l.Height != rect2.Height)
				{
					bitblock_l?.Dispose();
					bitblock_l = new Bitmap(rect2.Width, rect2.Height);
					using (Canvas canvas = Graphics.FromImage(bitblock_l).HighLay(Dpi))
					{
						using SolidBrush brush = new SolidBrush(color);
						using GraphicsPath path = new Rectangle(0, 0, bitblock_l.Width, num).RoundPath(num, roundTL: false, roundTR: false, roundBR: true, roundBL: true);
						canvas.Fill(brush, path);
					}
					Helper.Blur(bitblock_l, num);
				}
				g.Image(bitblock_l, rect2, 0.1f);
			}
			if (scroll_max != scroll_y)
			{
				Rectangle rect3 = new Rectangle(rectangle.X, rectangle.Y - num2, rectangle.Width, num2);
				if (bitblock_r == null || bitblock_r.Width != rect3.Width || bitblock_r.Height != rect3.Height)
				{
					bitblock_r?.Dispose();
					bitblock_r = new Bitmap(rect3.Width, rect3.Height);
					using (Canvas canvas2 = Graphics.FromImage(bitblock_r).HighLay(Dpi))
					{
						using SolidBrush brush2 = new SolidBrush(color);
						using GraphicsPath path2 = new Rectangle(0, num, bitblock_r.Width, num).RoundPath(num, roundTL: true, roundTR: true, roundBR: false, roundBL: false);
						canvas2.Fill(brush2, path2);
					}
					Helper.Blur(bitblock_r, num);
				}
				g.Image(bitblock_r, rect3, 0.1f);
			}
			g.Svg(rect: new Rectangle(rectangle.X + (rectangle.Width - num4) / 2, rectangle.Y + (rectangle.Height - num4) / 2, num4, num4), svg: SvgDb.IcoMore, color: color);
		}
		else
		{
			if (scroll_x <= 0 && scroll_max == scroll_x)
			{
				return;
			}
			int num5 = (int)((float)_gap * Dpi);
			int num6 = num5 * 2;
			int num7 = last.Height;
			int num8 = (int)((float)num7 * 0.4f);
			Rectangle rectangle2 = new Rectangle(rect.Right - num7, last.Y, num7, num7);
			if (scroll_x > 0)
			{
				Rectangle rect4 = new Rectangle(first.X, first.Y, num6, num7);
				if (full)
				{
					rect4.X = 0;
				}
				if (bitblock_l == null || bitblock_l.Width != rect4.Width || bitblock_l.Height != rect4.Height)
				{
					bitblock_l?.Dispose();
					bitblock_l = new Bitmap(rect4.Width, rect4.Height);
					using (Canvas canvas3 = Graphics.FromImage(bitblock_l).HighLay(Dpi))
					{
						using SolidBrush brush3 = new SolidBrush(color);
						using GraphicsPath path3 = new Rectangle(0, 0, num5, bitblock_l.Height).RoundPath(num5, roundTL: false, roundTR: true, roundBR: true, roundBL: false);
						canvas3.Fill(brush3, path3);
					}
					Helper.Blur(bitblock_l, num5);
				}
				g.Image(bitblock_l, rect4, 0.1f);
			}
			if (scroll_max != scroll_x)
			{
				Rectangle rect5 = new Rectangle(rectangle2.X - num6, rectangle2.Y, num6, num7);
				if (bitblock_r == null || bitblock_r.Width != rect5.Width || bitblock_r.Height != rect5.Height)
				{
					bitblock_r?.Dispose();
					bitblock_r = new Bitmap(rect5.Width, rect5.Height);
					using (Canvas canvas4 = Graphics.FromImage(bitblock_r).HighLay(Dpi))
					{
						using SolidBrush brush4 = new SolidBrush(color);
						using GraphicsPath path4 = new Rectangle(num5, 0, num5, bitblock_r.Height).RoundPath(num5, roundTL: true, roundTR: false, roundBR: false, roundBL: true);
						canvas4.Fill(brush4, path4);
					}
					Helper.Blur(bitblock_r, num5);
				}
				g.Image(bitblock_r, rect5, 0.1f);
			}
			g.Svg(rect: new Rectangle(rectangle2.X + (rectangle2.Width - num8) / 2, rectangle2.Y + (rectangle2.Height - num8) / 2, num8, num8), svg: SvgDb.IcoMore, color: color);
		}
	}

	public virtual void PaintExceedLR(Canvas g, Color color, int radius, Rectangle rect, Rectangle first, Rectangle last, bool full)
	{
		g.ResetClip();
		g.ResetTransform();
		TabAlignment tabAlignment = alignment;
		if ((uint)tabAlignment > 1u && (uint)(tabAlignment - 2) <= 1u)
		{
			if (scroll_y <= 0 && scroll_max == scroll_y)
			{
				return;
			}
			int num = (int)((float)(IsRotate ? last.Width : last.Height) * 0.6f);
			using SolidBrush brush = new SolidBrush(scrollback ?? Colour.FillSecondary.Get(ColorScheme, "Tabs", Name));
			using SolidBrush brush2 = new SolidBrush(ScrollBackHover ?? Colour.Primary.Get(ColorScheme, "Tabs", Name));
			using Pen pen = new Pen(scrollfore ?? color, 2f * Dpi);
			using Pen pen2 = new Pen(ScrollForeHover ?? Colour.PrimaryColor.Get(ColorScheme, "Tabs", Name), 2f * Dpi);
			if (scroll_y > 0)
			{
				rect_l = new Rectangle(last.X, rect.Y, last.Width, num);
				using GraphicsPath path = rect_l.RoundPath(radius);
				if (hover_l)
				{
					g.Fill(brush2, path);
					g.DrawLines(pen2, TAlignMini.Top.TriangleLines(rect_l, 0.5f));
				}
				else
				{
					g.Fill(brush, path);
					g.DrawLines(pen, TAlignMini.Top.TriangleLines(rect_l, 0.5f));
				}
			}
			if (scroll_max == scroll_y)
			{
				return;
			}
			rect_r = new Rectangle(last.X, rect.Y + rect.Height - num, last.Width, num);
			using GraphicsPath path2 = rect_r.RoundPath(radius);
			if (hover_r)
			{
				g.Fill(brush2, path2);
				g.DrawLines(pen2, TAlignMini.Bottom.TriangleLines(rect_r, 0.5f));
			}
			else
			{
				g.Fill(brush, path2);
				g.DrawLines(pen, TAlignMini.Bottom.TriangleLines(rect_r, 0.5f));
			}
			return;
		}
		if (scroll_x <= 0 && scroll_max == scroll_x)
		{
			return;
		}
		int num2 = (int)((float)last.Height * 0.6f);
		using SolidBrush brush3 = new SolidBrush(scrollback ?? Colour.FillSecondary.Get(ColorScheme, "Tabs", Name));
		using SolidBrush brush4 = new SolidBrush(ScrollBackHover ?? Colour.Primary.Get(ColorScheme, "Tabs", Name));
		using Pen pen3 = new Pen(scrollfore ?? color, 2f * Dpi);
		using Pen pen4 = new Pen(ScrollForeHover ?? Colour.PrimaryColor.Get(ColorScheme, "Tabs", Name), 2f * Dpi);
		if (scroll_x > 0)
		{
			rect_l = new Rectangle(rect.X, last.Y, num2, last.Height);
			using GraphicsPath path3 = rect_l.RoundPath(radius);
			if (hover_l)
			{
				g.Fill(brush4, path3);
				g.DrawLines(pen4, TAlignMini.Left.TriangleLines(rect_l, 0.5f));
			}
			else
			{
				g.Fill(brush3, path3);
				g.DrawLines(pen3, TAlignMini.Left.TriangleLines(rect_l, 0.5f));
			}
		}
		if (scroll_max == scroll_x)
		{
			return;
		}
		rect_r = new Rectangle(rect.X + rect.Width - num2, last.Y, num2, last.Height);
		using GraphicsPath path4 = rect_r.RoundPath(radius);
		if (hover_r)
		{
			g.Fill(brush4, path4);
			g.DrawLines(pen4, TAlignMini.Right.TriangleLines(rect_r, 0.5f));
		}
		else
		{
			g.Fill(brush3, path4);
			g.DrawLines(pen3, TAlignMini.Right.TriangleLines(rect_r, 0.5f));
		}
	}

	public virtual void PaintExceedLR_Shadow(Canvas g, Color color, int radius, Rectangle rect, Rectangle first, Rectangle last, bool full)
	{
		g.ResetClip();
		g.ResetTransform();
		TabAlignment tabAlignment = alignment;
		if ((uint)tabAlignment > 1u && (uint)(tabAlignment - 2) <= 1u)
		{
			if (scroll_y <= 0 && scroll_max == scroll_y)
			{
				return;
			}
			int num = (int)((float)_gap * Dpi);
			int num2 = num * 2;
			int num3 = (int)((float)(IsRotate ? last.Width : last.Height) * 0.6f);
			using SolidBrush brush = new SolidBrush(scrollback ?? Colour.FillSecondary.Get(ColorScheme, "Tabs", Name));
			using SolidBrush brush2 = new SolidBrush(ScrollBackHover ?? Colour.Primary.Get(ColorScheme, "Tabs", Name));
			using Pen pen = new Pen(scrollfore ?? color, 2f * Dpi);
			using Pen pen2 = new Pen(ScrollForeHover ?? Colour.PrimaryColor.Get(ColorScheme, "Tabs", Name), 2f * Dpi);
			if (scroll_y > 0)
			{
				rect_l = new Rectangle(last.X, rect.Y, last.Width, num3);
				Rectangle rect2 = new Rectangle(rect_l.X, rect_l.Bottom, rect_l.Width, num2);
				if (bitblock_l == null || bitblock_l.Width != rect2.Width || bitblock_l.Height != rect2.Height)
				{
					bitblock_l?.Dispose();
					bitblock_l = new Bitmap(rect2.Width, rect2.Height);
					using (Canvas canvas = Graphics.FromImage(bitblock_l).HighLay(Dpi))
					{
						using GraphicsPath path = new Rectangle(0, 0, bitblock_l.Width, num).RoundPath(num, roundTL: false, roundTR: false, roundBR: true, roundBL: true);
						canvas.Fill(brush, path);
					}
					Helper.Blur(bitblock_l, num);
				}
				g.Image(bitblock_l, rect2, 0.1f);
				using GraphicsPath path2 = rect_l.RoundPath(radius, roundTL: true, roundTR: true, roundBR: false, roundBL: false);
				if (hover_l)
				{
					g.Fill(brush2, path2);
					g.DrawLines(pen2, TAlignMini.Top.TriangleLines(rect_l, 0.5f));
				}
				else
				{
					g.Fill(brush, path2);
					g.DrawLines(pen, TAlignMini.Top.TriangleLines(rect_l, 0.5f));
				}
			}
			if (scroll_max == scroll_y)
			{
				return;
			}
			rect_r = new Rectangle(last.X, rect.Y + rect.Height - num3, last.Width, num3);
			Rectangle rect3 = new Rectangle(rect_r.X, rect_r.Y - num2, rect_r.Width, num2);
			if (bitblock_r == null || bitblock_r.Width != rect3.Width || bitblock_r.Height != rect3.Height)
			{
				bitblock_r?.Dispose();
				bitblock_r = new Bitmap(rect3.Width, rect3.Height);
				using (Canvas canvas2 = Graphics.FromImage(bitblock_r).HighLay(Dpi))
				{
					using GraphicsPath path3 = new Rectangle(0, num, bitblock_r.Width, num).RoundPath(num, roundTL: true, roundTR: true, roundBR: false, roundBL: false);
					canvas2.Fill(brush, path3);
				}
				Helper.Blur(bitblock_r, num);
			}
			g.Image(bitblock_r, rect3, 0.1f);
			using GraphicsPath path4 = rect_r.RoundPath(radius, roundTL: false, roundTR: false, roundBR: true, roundBL: true);
			if (hover_r)
			{
				g.Fill(brush2, path4);
				g.DrawLines(pen2, TAlignMini.Bottom.TriangleLines(rect_r, 0.5f));
			}
			else
			{
				g.Fill(brush, path4);
				g.DrawLines(pen, TAlignMini.Bottom.TriangleLines(rect_r, 0.5f));
			}
			return;
		}
		if (scroll_x <= 0 && scroll_max == scroll_x)
		{
			return;
		}
		int num4 = (int)((float)_gap * Dpi);
		int num5 = num4 * 2;
		int num6 = (int)((float)last.Height * 0.6f);
		using SolidBrush brush3 = new SolidBrush(scrollback ?? Colour.FillSecondary.Get(ColorScheme, "Tabs", Name));
		using SolidBrush brush4 = new SolidBrush(ScrollBackHover ?? Colour.Primary.Get(ColorScheme, "Tabs", Name));
		using Pen pen3 = new Pen(scrollfore ?? color, 2f * Dpi);
		using Pen pen4 = new Pen(ScrollForeHover ?? Colour.PrimaryColor.Get(ColorScheme, "Tabs", Name), 2f * Dpi);
		if (scroll_x > 0)
		{
			rect_l = new Rectangle(rect.X, last.Y, num6, last.Height);
			Rectangle rect4 = new Rectangle(rect_l.Right, rect_l.Y, num5, rect_l.Height);
			if (bitblock_l == null || bitblock_l.Width != rect4.Width || bitblock_l.Height != rect4.Height)
			{
				bitblock_l?.Dispose();
				bitblock_l = new Bitmap(rect4.Width, rect4.Height);
				using (Canvas canvas3 = Graphics.FromImage(bitblock_l).HighLay(Dpi))
				{
					using GraphicsPath path5 = new Rectangle(0, 0, num4, bitblock_l.Height).RoundPath(num4, roundTL: false, roundTR: true, roundBR: true, roundBL: false);
					canvas3.Fill(brush3, path5);
				}
				Helper.Blur(bitblock_l, num4);
			}
			g.Image(bitblock_l, rect4, 0.1f);
			using GraphicsPath path6 = rect_l.RoundPath(radius, roundTL: true, roundTR: false, roundBR: false, roundBL: true);
			if (hover_l)
			{
				g.Fill(brush4, path6);
				g.DrawLines(pen4, TAlignMini.Left.TriangleLines(rect_l, 0.5f));
			}
			else
			{
				g.Fill(brush3, path6);
				g.DrawLines(pen3, TAlignMini.Left.TriangleLines(rect_l, 0.5f));
			}
		}
		if (scroll_max == scroll_x)
		{
			return;
		}
		rect_r = new Rectangle(rect.X + rect.Width - num6, last.Y, num6, last.Height);
		Rectangle rect5 = new Rectangle(rect_r.X - num5, rect_r.Y, num5, rect_r.Height);
		if (bitblock_r == null || bitblock_r.Width != rect5.Width || bitblock_r.Height != rect5.Height)
		{
			bitblock_r?.Dispose();
			bitblock_r = new Bitmap(rect5.Width, rect5.Height);
			using (Canvas canvas4 = Graphics.FromImage(bitblock_r).HighLay(Dpi))
			{
				using GraphicsPath path7 = new Rectangle(num4, 0, num4, bitblock_r.Height).RoundPath(num4, roundTL: true, roundTR: false, roundBR: false, roundBL: true);
				canvas4.Fill(brush3, path7);
			}
			Helper.Blur(bitblock_r, num4);
		}
		g.Image(bitblock_r, rect5, 0.1f);
		using GraphicsPath path8 = rect_r.RoundPath(radius, roundTL: false, roundTR: true, roundBR: true, roundBL: false);
		if (hover_r)
		{
			g.Fill(brush4, path8);
			g.DrawLines(pen4, TAlignMini.Right.TriangleLines(rect_r, 0.5f));
		}
		else
		{
			g.Fill(brush3, path8);
			g.DrawLines(pen3, TAlignMini.Right.TriangleLines(rect_r, 0.5f));
		}
	}

	protected virtual void OnSelectedIndexChanged(int e)
	{
		SelectedIndexChanged?.Invoke(this, new IntEventArgs(e));
	}

	protected virtual bool OnSelectedIndexChanging(int e)
	{
		if (SelectedIndexChanging == null)
		{
			return true;
		}
		TabsIndexChangingEventArgs e2 = new TabsIndexChangingEventArgs(e);
		SelectedIndexChanging(this, e2);
		if (e2.Cancel)
		{
			return false;
		}
		return true;
	}

	internal void MouseChangeIndex(TabPage page)
	{
		if (items != null)
		{
			int index = items.IndexOf(page);
			OnTabClick(items[index], index, style, new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
		}
	}

	protected virtual bool OnClosingPage(TabPage item)
	{
		if (ClosingPage == null)
		{
			return true;
		}
		ClosingPageEventArgs e = new ClosingPageEventArgs(item);
		return ClosingPage(this, e);
	}

	internal bool IOnClosingPage(TabPage item)
	{
		return OnClosingPage(item);
	}

	protected virtual bool OnTabClick(TabPage item, int index, IStyle style, MouseEventArgs e)
	{
		if (TabClick == null)
		{
			RunTabClick(item, index, style, e);
			return true;
		}
		TabsItemEventArgs e2 = new TabsItemEventArgs(item, index, style, e);
		TabClick(this, e2);
		if (e2.Cancel)
		{
			return false;
		}
		RunTabClick(item, index, style, e);
		return true;
	}

	private void RunTabClick(TabPage item, int index, IStyle style, MouseEventArgs e)
	{
		if (((EnablePageCloseByMouseMiddle && e.Button == MouseButtons.Middle) || (EnablePageCloseByMouseDoubleClick && e.Button == MouseButtons.Left && e.Clicks > 1)) && CanClose(item, style))
		{
			items.Remove(item);
			if (CloseDisposePage)
			{
				item.Dispose();
			}
		}
		else
		{
			SelectedIndex = index;
		}
	}

	private bool CanClose(TabPage item, IStyle style)
	{
		if (item.ReadOnly)
		{
			return false;
		}
		bool flag = false;
		if (style is StyleLine styleLine)
		{
			if (styleLine.Closable)
			{
				flag = true;
			}
		}
		else if (style is StyleCard styleCard)
		{
			if (styleCard.Closable)
			{
				flag = true;
			}
		}
		else if (style is StyleCard2 { Closable: not StyleCard2.CloseType.none })
		{
			flag = true;
		}
		if (flag)
		{
			return OnClosingPage(item);
		}
		return false;
	}

	protected virtual void OnNonTabClick(MouseEventArgs e)
	{
		NonTabClick?.Invoke(this, e);
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG)
		{
			LoadLayout(print: true);
		}
	}
}
