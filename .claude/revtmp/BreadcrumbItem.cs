using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using AntdUI.Design;

namespace AntdUI;

public class BreadcrumbItem
{
	private Image? icon;

	private string? iconSvg;

	private string? text;

	private string? separator;

	/// <summary>
	/// ID
	/// </summary>
	[Description("ID")]
	[Category("Data")]
	[DefaultValue(null)]
	public string? ID { get; set; }

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
				Invalidates();
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
			if (IconSvg == null)
			{
				return Icon != null;
			}
			return true;
		}
	}

	/// <summary>
	/// 文本
	/// </summary>
	[Description("文本")]
	[Category("Appearance")]
	[DefaultValue(null)]
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

	/// <summary>
	/// 获取或设置尾部的自定义分割符
	/// </summary>
	[Description("获取或设置尾部的自定义分割符")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? Separator
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
				Invalidates();
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
	public Color? Fore { get; set; }

	/// <summary>
	/// 激活文字颜色
	/// </summary>
	[Description("激活文字颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeActive { get; set; }

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	/// <summary>
	/// 用户定义数据
	/// </summary>
	[Description("用户定义数据")]
	[Category("Data")]
	[DefaultValue(null)]
	public object? Tag { get; set; }

	internal bool Hover { get; set; }

	internal Rectangle Rect { get; set; }

	internal Rectangle RectImg { get; set; }

	internal Rectangle RectText { get; set; }

	internal Breadcrumb? PARENT { get; set; }

	private void Invalidates()
	{
		if (PARENT != null)
		{
			PARENT.ChangeItems();
			PARENT.Invalidate();
		}
	}

	public BreadcrumbItem SetIcon(Image? img)
	{
		icon = img;
		return this;
	}

	public BreadcrumbItem SetIcon(string? svg)
	{
		iconSvg = svg;
		return this;
	}

	public BreadcrumbItem SetID(string? value)
	{
		ID = value;
		return this;
	}

	public BreadcrumbItem SetText(string? value, string? localization = null)
	{
		text = value;
		LocalizationText = localization;
		return this;
	}

	public BreadcrumbItem SetFore(Color? value)
	{
		Fore = value;
		return this;
	}

	public BreadcrumbItem SetForeActive(Color? value)
	{
		ForeActive = value;
		return this;
	}

	public BreadcrumbItem SetSeparatorChar(string? value)
	{
		Separator = value;
		return this;
	}

	public BreadcrumbItem SetTag(object? value)
	{
		Tag = value;
		return this;
	}

	public override string? ToString()
	{
		return Text;
	}
}
