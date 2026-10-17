using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using AntdUI.Design;

namespace AntdUI;

public class TimelineItem
{
	private string? text;

	private string? description;

	private bool visible = true;

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
	public Image? Icon { get; set; }

	/// <summary>
	/// 图标SVG
	/// </summary>
	[Description("图标SVG")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? IconSvg { get; set; }

	/// <summary>
	/// 名称
	/// </summary>
	[Description("名称")]
	[Category("Data")]
	[DefaultValue(null)]
	public string? Name { get; set; }

	/// <summary>
	/// 文本
	/// </summary>
	[Description("文本")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
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
	/// 描述，可选
	/// </summary>
	[Description("描述，可选")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
	public string? Description
	{
		get
		{
			return Localization.GetLangI(LocalizationDescription, description, new string[2] { "{id}", ID });
		}
		set
		{
			if (!(description == value))
			{
				description = value;
				Invalidates();
			}
		}
	}

	[Description("详情描述")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationDescription { get; set; }

	/// <summary>
	/// 描述文本颜色
	/// </summary>
	[Description("描述文本颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? DescriptionFore { get; set; }

	[Description("颜色类型")]
	[Category("Appearance")]
	[DefaultValue(TTypeMini.Primary)]
	public TTypeMini Type { get; set; } = TTypeMini.Primary;

	[Description("填充颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Fill { get; set; }

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

	internal Timeline? PARENT { get; set; }

	internal float pen_w { get; set; } = 3f;

	internal Rectangle rect { get; set; }

	internal Rectangle txt_rect { get; set; }

	internal Rectangle description_rect { get; set; }

	internal Rectangle ico_rect { get; set; }

	public TimelineItem()
	{
	}

	public TimelineItem(string text)
	{
		Text = text;
	}

	public TimelineItem(string text, string description)
	{
		Text = text;
		Description = description;
	}

	public TimelineItem(string text, string description, Image? icon)
	{
		Text = text;
		Description = description;
		Icon = icon;
	}

	public TimelineItem(string text, Image? icon)
	{
		Text = text;
		Icon = icon;
	}

	private void Invalidates()
	{
		if (PARENT != null)
		{
			PARENT.ChangeList();
			PARENT.Invalidate();
		}
	}

	public TimelineItem SetType(TTypeMini value = TTypeMini.Success)
	{
		Type = value;
		return this;
	}

	public TimelineItem SetFill(Color? img)
	{
		Fill = img;
		return this;
	}

	public TimelineItem SetIcon(Image? img)
	{
		Icon = img;
		return this;
	}

	public TimelineItem SetIcon(string? svg)
	{
		IconSvg = svg;
		return this;
	}

	public TimelineItem SetID(string? value)
	{
		ID = value;
		return this;
	}

	public TimelineItem SetName(string? value)
	{
		Name = value;
		return this;
	}

	public TimelineItem SetText(string? value, string? localization = null)
	{
		text = value;
		LocalizationText = localization;
		return this;
	}

	public TimelineItem SetDescription(string? value, string? localization = null)
	{
		description = value;
		LocalizationDescription = localization;
		return this;
	}

	public TimelineItem SetVisible(bool value = false)
	{
		visible = value;
		return this;
	}

	public TimelineItem SetTag(object? value)
	{
		Tag = value;
		return this;
	}

	public override string? ToString()
	{
		return Text;
	}
}
