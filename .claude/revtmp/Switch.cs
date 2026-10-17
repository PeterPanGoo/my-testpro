using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Switch 开关
/// </summary>
/// <remarks>开关选择器。</remarks>
[Description("Switch 开关")]
[ToolboxItem(true)]
[DefaultProperty("Checked")]
[DefaultEvent("CheckedChanged")]
public class Switch : IControl
{
	private Color? fore;

	private Color? fill;

	private bool AnimationCheck;

	private float AnimationCheckValue;

	private bool _checked;

	private string? _checkedText;

	private string? _unCheckedText;

	private bool loading;

	private AnimationTask? ThreadLoading;

	internal float LineWidth = 6f;

	internal float LineAngle;

	private bool init;

	private float AnimationHoverValue;

	private bool AnimationHover;

	private bool _mouseHover;

	private AnimationTask? ThreadHover;

	private AnimationTask? ThreadCheck;

	private AnimationTask? ThreadClick;

	private bool AnimationClick;

	private float AnimationClickValue;

	private bool hasFocus;

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
	/// 选中状态
	/// </summary>
	[Description("选中状态")]
	[Category("Data")]
	[DefaultValue(false)]
	public bool Checked
	{
		get
		{
			return _checked;
		}
		set
		{
			if (_checked == value)
			{
				return;
			}
			_checked = value;
			ThreadCheck?.Dispose();
			if (IsHandleCreated && Config.HasAnimation("Switch", Name))
			{
				AnimationCheck = true;
				ThreadCheck = new AnimationTask(new AnimationLinearFConfig(this, (float i) =>
				{
					AnimationCheckValue = i;
					Invalidate();
					return true;
				}, 10).SetValue(AnimationCheckValue, value, 0.1f).SetEnd(() =>
				{
					AnimationCheck = false;
				}));
			}
			else
			{
				AnimationCheckValue = (value ? 1f : 0f);
			}
			Invalidate();
			OnCheckedChanged(value);
			OnPropertyChanged("Checked");
		}
	}

	/// <summary>
	/// 点击时自动改变选中状态
	/// </summary>
	[Description("点击时自动改变选中状态")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool AutoCheck { get; set; } = true;

	/// <summary>
	/// 波浪大小
	/// </summary>
	[Description("波浪大小")]
	[Category("Appearance")]
	[DefaultValue(4)]
	public int WaveSize { get; set; } = 4;

	[Description("间距")]
	[Category("Appearance")]
	[DefaultValue(2)]
	public int Gap { get; set; } = 2;

	[Description("选中时显示的文本")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? CheckedText
	{
		get
		{
			return this.GetLangI(LocalizationCheckedText, _checkedText);
		}
		set
		{
			if (!(_checkedText == value))
			{
				_checkedText = value;
				if (_checked)
				{
					Invalidate();
				}
				OnPropertyChanged("CheckedText");
			}
		}
	}

	[Description("选中时显示的文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationCheckedText { get; set; }

	[Description("未选中时显示的文本")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? UnCheckedText
	{
		get
		{
			return this.GetLangI(LocalizationUnCheckedText, _unCheckedText);
		}
		set
		{
			if (!(_unCheckedText == value))
			{
				_unCheckedText = value;
				if (!_checked)
				{
					Invalidate();
				}
				OnPropertyChanged("UnCheckedText");
			}
		}
	}

	[Description("未选中时显示的文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationUnCheckedText { get; set; }

	/// <summary>
	/// 加载中
	/// </summary>
	[Description("加载中")]
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
			if (IsHandleCreated)
			{
				if (loading)
				{
					bool ProgState = false;
					ThreadLoading = new AnimationTask(new AnimationLoopConfig(this, () =>
					{
						if (ProgState)
						{
							LineAngle = LineAngle.Calculate(9f);
							LineWidth = LineWidth.Calculate(0.6f);
							if (LineWidth > 75f)
							{
								ProgState = false;
							}
						}
						else
						{
							LineAngle = LineAngle.Calculate(9.6f);
							LineWidth = LineWidth.Calculate(-0.6f);
							if (LineWidth < 6f)
							{
								ProgState = true;
							}
						}
						if (LineAngle >= 360f)
						{
							LineAngle = 0f;
						}
						Invalidate();
						return true;
					}, 10).SetPriority());
				}
				else
				{
					ThreadLoading?.Dispose();
				}
			}
			Invalidate();
		}
	}

	public override Rectangle ReadRectangle => ClientRectangle.PaddingRect(Padding, Dpi, WaveSize);

	public override GraphicsPath RenderRegion
	{
		get
		{
			Rectangle readRectangle = ReadRectangle;
			return readRectangle.RoundPath(readRectangle.Height);
		}
	}

	private bool ExtraMouseHover
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
			SetCursor(value & enabled);
			if (!enabled)
			{
				return;
			}
			if (Config.HasAnimation("Switch", Name))
			{
				ThreadHover?.Dispose();
				AnimationHover = true;
				ThreadHover = new AnimationTask(new AnimationLinearFConfig(this, (float i) =>
				{
					AnimationHoverValue = i;
					Invalidate();
					return true;
				}, 10).SetValue(AnimationHoverValue, value, 0.1f).SetEnd(() =>
				{
					AnimationHover = false;
				}));
			}
			else
			{
				AnimationHoverValue = 255f;
			}
			Invalidate();
		}
	}

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
			if (value && _mouseHover)
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

	/// <summary>
	/// Checked 属性值更改时发生
	/// </summary>
	[Description("Checked 属性值更改时发生")]
	[Category("Behavior")]
	public event BoolEventHandler? CheckedChanged;

	public Switch()
		: base(ControlType.Select)
	{
	}

	protected virtual void OnCheckedChanged(bool e)
	{
		CheckedChanged?.Invoke(this, new BoolEventArgs(e));
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		init = true;
		Canvas canvas = e.Canvas;
		Rectangle rect = e.Rect.PaddingRect(Padding);
		Rectangle readRectangle = ReadRectangle;
		bool enabled = Enabled;
		using (GraphicsPath path = readRectangle.RoundPath(readRectangle.Height))
		{
			Color color = fill ?? Colour.Primary.Get(ColorScheme, "Switch", Name);
			PaintClick(canvas, path, rect, readRectangle, color);
			if (enabled && hasFocus && Config.FocusBorderEnabled && WaveSize > 0)
			{
				float num = (float)WaveSize * Dpi / 2f;
				float num2 = num * 2f;
				using GraphicsPath path2 = new RectangleF((float)readRectangle.X - num, (float)readRectangle.Y - num, (float)readRectangle.Width + num2, (float)readRectangle.Height + num2).RoundPath(0f, TShape.Round);
				canvas.Draw(Colour.PrimaryBorder.Get(ColorScheme, "Switch", Name), num, path2);
			}
			using (SolidBrush solidBrush = new SolidBrush(Colour.TextQuaternary.Get(ColorScheme, "Switch", Name)))
			{
				canvas.Fill(solidBrush, path);
				if (AnimationHover)
				{
					canvas.Fill(Helper.ToColorN(AnimationHoverValue, solidBrush.Color), path);
				}
				else if (ExtraMouseHover)
				{
					canvas.Fill(solidBrush, path);
				}
			}
			int num3 = (int)((float)Gap * Dpi);
			int num4 = num3 * 2;
			if (AnimationCheck)
			{
				float alpha = 255f * AnimationCheckValue;
				canvas.Fill(Helper.ToColor(alpha, color), path);
				RectangleF rectangleF = new RectangleF((float)(readRectangle.X + num3) + (float)(readRectangle.Width - readRectangle.Height) * AnimationCheckValue, readRectangle.Y + num3, readRectangle.Height - num4, readRectangle.Height - num4);
				canvas.FillEllipse(enabled ? Colour.SwitchHandleBg.Get(ColorScheme, "Switch", Name) : Helper.ToColorN(0.65f, Colour.SwitchHandleBg.Get(ColorScheme, "Switch", Name)), rectangleF);
				if (loading)
				{
					PaintLoading(canvas, readRectangle, rectangleF, num3, num4, color);
				}
				PaintText(canvas, null, readRectangle, rectangleF);
			}
			else if (_checked)
			{
				if (enabled)
				{
					PaintChecked(canvas, readRectangle, path, num3, num4, Colour.SwitchHandleBg.Get(ColorScheme, "Switch", Name), color);
				}
				else
				{
					PaintChecked(canvas, readRectangle, path, num3, num4, Helper.ToColorN(0.8f, Colour.SwitchHandleBg.Get(ColorScheme, "Switch", Name)), Helper.ToColorN(0.65f, color));
				}
			}
			else if (enabled)
			{
				PaintUnChecked(canvas, readRectangle, num3, num4, Colour.SwitchHandleBg.Get(ColorScheme, "Switch", Name), color);
			}
			else
			{
				PaintUnChecked(canvas, readRectangle, num3, num4, Helper.ToColorN(0.8f, Colour.SwitchHandleBg.Get(ColorScheme, "Switch", Name)), Helper.ToColorN(0.65f, color));
			}
		}
		base.OnDraw(e);
	}

	private void PaintChecked(Canvas g, Rectangle rect, GraphicsPath path, int gap, int gap2, Color handBg, Color color)
	{
		Color color2 = FillHover ?? Colour.PrimaryHover.Get(ColorScheme, "Switch", Name);
		g.Fill(color, path);
		if (AnimationHover)
		{
			g.Fill(Helper.ToColorN(AnimationHoverValue, color2), path);
		}
		else if (ExtraMouseHover)
		{
			g.Fill(color2, path);
		}
		RectangleF rectangleF = new RectangleF(rect.X + gap + rect.Width - rect.Height, rect.Y + gap, rect.Height - gap2, rect.Height - gap2);
		g.FillEllipse(handBg, rectangleF);
		if (loading)
		{
			PaintLoading(g, rect, rectangleF, gap, gap2, color);
		}
		PaintText(g, true, rect, rectangleF);
	}

	private void PaintUnChecked(Canvas g, Rectangle rect, int gap, int gap2, Color handBg, Color color)
	{
		RectangleF rectangleF = new RectangleF(rect.X + gap, rect.Y + gap, rect.Height - gap2, rect.Height - gap2);
		g.FillEllipse(handBg, rectangleF);
		if (loading)
		{
			PaintLoading(g, rect, rectangleF, gap, gap2, color);
		}
		PaintText(g, false, rect, rectangleF);
	}

	private void PaintLoading(Canvas g, Rectangle rect, RectangleF dot_rect, int gap, int gap2, Color color)
	{
		RectangleF rect2 = new RectangleF(dot_rect.X + (float)gap, dot_rect.Y + (float)gap, dot_rect.Height - (float)gap2, dot_rect.Height - (float)gap2);
		float num = (float)rect.Height * 0.1f;
		using Pen pen = new Pen(color, num);
		LineCap startCap = (pen.EndCap = LineCap.Round);
		pen.StartCap = startCap;
		g.DrawArc(pen, rect2, LineAngle, LineWidth * 3.6f);
	}

	/// <summary>
	/// 绘制文本
	/// </summary>
	private void PaintText(Canvas g, bool? check, Rectangle rect, RectangleF dot_rect)
	{
		if (check.HasValue)
		{
			if (check.Value)
			{
				string checkedText = CheckedText;
				if (checkedText != null)
				{
					int num = (int)(dot_rect.Height * 1.1f);
					g.DrawText(checkedText, Font, fore ?? Colour.PrimaryColor.Get(ColorScheme, "Switch", Name), new Rectangle(rect.X, rect.Y, rect.Width - num, rect.Height));
				}
			}
			else
			{
				string unCheckedText = UnCheckedText;
				if (unCheckedText != null)
				{
					int num2 = (int)(dot_rect.Height * 1.1f);
					g.DrawText(unCheckedText, Font, fore ?? Colour.PrimaryColor.Get(ColorScheme, "Switch", Name), new Rectangle(rect.X + num2, rect.Y, rect.Width - num2, rect.Height));
				}
			}
			return;
		}
		g.SetClip(rect);
		string checkedText2 = CheckedText;
		string unCheckedText2 = UnCheckedText;
		if (checkedText2 != null || unCheckedText2 != null)
		{
			int num3 = (int)(dot_rect.Height * 1.1f);
			int num4 = (int)((float)rect.Width * AnimationCheckValue);
			using (SolidBrush brush = new SolidBrush(fore ?? Colour.PrimaryColor.Get(ColorScheme, "Switch", Name)))
			{
				int num5 = rect.Width - num3;
				g.DrawText(unCheckedText2, Font, brush, new Rectangle(rect.X + num3 + num4, rect.Y, num5, rect.Height));
				g.DrawText(checkedText2, Font, brush, new Rectangle(rect.X + num4 - rect.Width, rect.Y, num5, rect.Height));
			}
			g.ResetClip();
		}
	}

	internal void PaintClick(Canvas g, GraphicsPath path, Rectangle rect, RectangleF rect_read, Color color)
	{
		_ = AnimationClick;
		float alpha = 100f * (1f - AnimationClickValue);
		float num = rect_read.Width + ((float)rect.Width - rect_read.Width) * AnimationClickValue;
		float num2 = rect_read.Height + ((float)rect.Height - rect_read.Height) * AnimationClickValue;
		using GraphicsPath graphicsPath = new RectangleF((float)rect.X + ((float)rect.Width - num) / 2f, (float)rect.Y + ((float)rect.Height - num2) / 2f, num, num2).RoundPath(num2);
		graphicsPath.AddPath(path, connect: false);
		g.Fill(Helper.ToColor(alpha, color), graphicsPath);
	}

	protected override void OnClick(EventArgs e)
	{
		if (AutoCheck)
		{
			Checked = !_checked;
		}
		base.OnClick(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		init = false;
		Focus();
		base.OnMouseDown(e);
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		base.OnKeyUp(e);
		if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Return)
		{
			OnClick(EventArgs.Empty);
			e.Handled = true;
		}
	}

	protected override void Dispose(bool disposing)
	{
		ThreadClick?.Dispose();
		ThreadCheck?.Dispose();
		ThreadHover?.Dispose();
		ThreadLoading?.Dispose();
		base.Dispose(disposing);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (!Config.HasAnimation("Switch", Name) || e.Button != MouseButtons.Left)
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
				AnimationClickValue = AnimationClickValue.Calculate(0.1f);
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

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		ExtraMouseHover = true;
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
}
