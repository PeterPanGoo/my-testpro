using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

[ToolboxItem(false)]
[Designer(typeof(IControlDesigner))]
public class TabPage : ScrollableControl
{
	private Image? icon;

	private string? iconSvg;

	private bool readOnly;

	private string? badge;

	private float badgeSize = 0.6f;

	private Color? badgeback;

	private string text = "";

	internal Rectangle Rect = new Rectangle(-10, -10, 0, 0);

	internal Tabs? PARENT;

	private DockStyle olddock = DockStyle.Fill;

	private bool? showed;

	/// <summary>
	/// 图标
	/// </summary>
	[Category("Appearance")]
	[Description("图标")]
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
				PARENT?.LoadLayout(print: true);
			}
		}
	}

	/// <summary>
	/// 图标
	/// </summary>
	[Category("Appearance")]
	[Description("图标SVG")]
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
				PARENT?.LoadLayout(print: true);
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
	/// 只读
	/// </summary>
	[Description("只读")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool ReadOnly
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
				PARENT?.LoadLayout(print: true);
			}
		}
	}

	[Description("徽标内容")]
	[Category("徽标")]
	[DefaultValue(null)]
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
				PARENT?.Invalidate();
			}
		}
	}

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
				if (badge != null)
				{
					PARENT?.Invalidate();
				}
			}
		}
	}

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
			if (!(badgeback == value) && badgeback != value)
			{
				badgeback = value;
				if (badge != null)
				{
					PARENT?.Invalidate();
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
	public int BadgeOffsetX { get; set; } = 1;

	/// <summary>
	/// 徽标偏移Y
	/// </summary>
	[Description("徽标偏移Y")]
	[Category("徽标")]
	[DefaultValue(1)]
	public int BadgeOffsetY { get; set; } = 1;

	/// <summary>
	/// 文本
	/// </summary>
	[Description("文本")]
	[Category("Appearance")]
	[DefaultValue("")]
	public override string Text
	{
		get
		{
			return this.GetLangIN(LocalizationText, text);
		}
		set
		{
			if (!(text == value))
			{
				base.Text = (text = value);
				PARENT?.LoadLayout(print: true);
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	/// <summary>
	/// 定义要绑定到容器的控件边框
	/// </summary>
	[Category("Layout")]
	[Description("定义要绑定到容器的控件边框")]
	[DefaultValue(DockStyle.Fill)]
	public new DockStyle Dock
	{
		get
		{
			return olddock;
		}
		set
		{
			if (olddock != value)
			{
				olddock = value;
				if (Showed)
				{
					base.Dock = value;
				}
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("显示的")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Showed
	{
		get
		{
			return showed == true;
		}
		internal set
		{
			if (showed != value)
			{
				showed = value;
				if (value)
				{
					base.Dock = olddock;
					BringToFront();
				}
				else
				{
					int num = Width;
					int num2 = Height;
					base.Dock = DockStyle.None;
					Bounds = new Rectangle(-num * 2, -num2 * 2, num, num2);
				}
				ShowedChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	public event EventHandler? ShowedChanged;

	public TabPage()
	{
		SetStyle(ControlStyles.ContainerControl | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		UpdateStyles();
	}

	internal bool Contains(int x, int y)
	{
		return Rect.Contains(x, y);
	}

	internal Rectangle GetRect(int offsetx, int offsety)
	{
		return new Rectangle(Rect.X + offsetx, Rect.Y + offsety, Rect.Width, Rect.Height);
	}

	internal Rectangle SetRect(Rectangle rect)
	{
		Rect = rect;
		return Rect;
	}

	internal Rectangle SetOffset(int x, int y)
	{
		Rect.Offset(x, y);
		return Rect;
	}

	protected override void OnTextChanged(EventArgs e)
	{
		PARENT?.LoadLayout(print: true);
		base.OnTextChanged(e);
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		PARENT?.LoadLayout(print: true);
		base.OnVisibleChanged(e);
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public IAsyncResult BeginInvoke(Action method)
	{
		return BeginInvoke(method, null);
	}

	public void Invoke(Action method)
	{
		Invoke(method, null);
	}

	public T Invoke<T>(Func<T> method)
	{
		return (T)Invoke(method, null);
	}

	public override string ToString()
	{
		return Text;
	}
}
