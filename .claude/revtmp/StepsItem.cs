using System;
using System.ComponentModel;
using System.Drawing;

namespace AntdUI;

public class StepsItem
{
	private Image? icon;

	private string? iconSvg;

	private int? iconsize;

	private string title = string.Empty;

	private string? subTitle;

	internal bool showSub;

	private string? description;

	internal bool showDescription;

	private DateTime? milestoneTime;

	private bool visible = true;

	private Color? back;

	private Color? fore;

	/// <summary>
	/// ID
	/// </summary>
	[Description("ID")]
	[Category("Data")]
	[DefaultValue(null)]
	public string? ID { get; set; }

	/// <summary>
	/// 图标，可选
	/// </summary>
	[Description("图标，可选")]
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
				PARENT?.Invalidate();
			}
		}
	}

	/// <summary>
	/// 图标SVG，可选
	/// </summary>
	[Description("图标SVG，可选")]
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
				PARENT?.Invalidate();
			}
		}
	}

	/// <summary>
	/// 图标的大小，可选
	/// </summary>
	[Description("图标的大小，可选")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? IconSize
	{
		get
		{
			return iconsize;
		}
		set
		{
			if (iconsize != value)
			{
				iconsize = value;
				Invalidate();
			}
		}
	}

	internal int ReadWidth { get; set; }

	/// <summary>
	/// 名称
	/// </summary>
	[Description("名称")]
	[Category("Data")]
	[DefaultValue(null)]
	public string? Name { get; set; }

	/// <summary>
	/// 标题
	/// </summary>
	[Description("标题")]
	[Category("Appearance")]
	[DefaultValue("")]
	public string Title
	{
		get
		{
			return Localization.GetLangIN(LocalizationTitle, title, new string[2] { "{id}", ID });
		}
		set
		{
			if (!(title == value))
			{
				title = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 里程碑模式下的标题
	/// </summary>
	public string GetTitle
	{
		get
		{
			string text = Localization.GetLangIN(LocalizationTitle, title, new string[2] { "{id}", ID });
			if (PARENT != null && PARENT.MilestoneMode && MilestoneTimePoint.HasValue)
			{
				if (!PARENT.MilestoneMode)
				{
					return text;
				}
				text = PARENT.MilestoneType switch
				{
					TMilestoneType.Time => MilestoneTimePoint?.ToString(string.IsNullOrEmpty(PARENT.MilestoneTimeFormat) ? "HH:mm:ss" : PARENT.MilestoneTimeFormat) + " " + text, 
					TMilestoneType.Full => MilestoneTimePoint?.ToString(string.IsNullOrEmpty(PARENT.MilestoneTimeFormat) ? "yy MMM dd HH:mm:ss" : PARENT.MilestoneTimeFormat) + " " + text, 
					_ => MilestoneTimePoint?.ToString(string.IsNullOrEmpty(PARENT.MilestoneTimeFormat) ? "yy MMM dd" : PARENT.MilestoneTimeFormat) + " " + text, 
				};
			}
			return text;
		}
	}

	[Description("标题")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationTitle { get; set; }

	internal Size TitleSize { get; set; }

	/// <summary>
	/// 子标题
	/// </summary>
	[Description("子标题")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? SubTitle
	{
		get
		{
			return Localization.GetLangI(LocalizationSubTitle, subTitle, new string[2] { "{id}", ID });
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = null;
			}
			if (!(subTitle == value))
			{
				subTitle = value;
				showSub = subTitle != null;
				Invalidate();
			}
		}
	}

	[Description("子标题")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationSubTitle { get; set; }

	internal Size SubTitleSize { get; set; }

	/// <summary>
	/// 详情描述，可选
	/// </summary>
	[Description("详情描述，可选")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public string? Description
	{
		get
		{
			return Localization.GetLangI(LocalizationDescription, description, new string[2] { "{id}", ID });
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = null;
			}
			if (!(description == value))
			{
				description = value;
				showDescription = description != null;
				Invalidate();
			}
		}
	}

	[Description("详情描述")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationDescription { get; set; }

	internal Size DescriptionSize { get; set; }

	/// <summary>
	/// 里程碑的时间点
	/// </summary>
	[Description("里程碑时间点")]
	[Category("Data")]
	[DefaultValue(null)]
	public DateTime? MilestoneTimePoint
	{
		get
		{
			return milestoneTime;
		}
		set
		{
			if (!(milestoneTime == value))
			{
				milestoneTime = value;
				Invalidate();
			}
		}
	}

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
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 时间轴背景色
	/// </summary>
	[Description("时间轴背景色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? BackColor
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
			}
		}
	}

	/// <summary>
	/// 时间轴前景色
	/// </summary>
	[Description("时间轴前景色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? ForeColor
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

	/// <summary>
	/// 用户定义数据
	/// </summary>
	[Description("用户定义数据")]
	[Category("Data")]
	[DefaultValue(null)]
	public object? Tag { get; set; }

	internal Steps? PARENT { get; set; }

	internal float pen_w { get; set; } = 3f;

	internal Rectangle rect { get; set; }

	internal Rectangle title_rect { get; set; }

	internal Rectangle subtitle_rect { get; set; }

	internal Rectangle description_rect { get; set; }

	internal Rectangle ico_rect { get; set; }

	public StepsItem()
	{
	}

	public StepsItem(string title)
	{
		Title = title;
	}

	public StepsItem(string title, string subTitle)
	{
		Title = title;
		SubTitle = subTitle;
	}

	public StepsItem(string title, string subTitle, string description)
	{
		Title = title;
		SubTitle = subTitle;
		Description = description;
	}

	private void Invalidate()
	{
		PARENT?.ChangeList(print: true);
	}

	public StepsItem SetIcon(Image? img)
	{
		icon = img;
		return this;
	}

	public StepsItem SetIcon(string? svg)
	{
		iconSvg = svg;
		return this;
	}

	public StepsItem SetID(string? value)
	{
		ID = value;
		return this;
	}

	public StepsItem SetName(string? value)
	{
		Name = value;
		return this;
	}

	public StepsItem SetTitle(string value, string? localization = null)
	{
		title = value;
		LocalizationTitle = localization;
		return this;
	}

	public StepsItem SetSubTitle(string? value, string? localization = null)
	{
		subTitle = value;
		LocalizationSubTitle = localization;
		return this;
	}

	public StepsItem SetDescription(string? value, string? localization = null)
	{
		description = value;
		LocalizationDescription = localization;
		showDescription = description != null;
		return this;
	}

	public StepsItem SetVisible(bool value = false)
	{
		visible = value;
		return this;
	}

	public StepsItem SetTag(object? value)
	{
		Tag = value;
		return this;
	}

	public override string ToString()
	{
		return Title + " " + SubTitle;
	}
}
