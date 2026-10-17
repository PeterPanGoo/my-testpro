using System.Collections.Generic;
using System.Drawing;

namespace AntdUI;

public class SelectItem : ISelectItem, SelectItemDraw
{
	private string _text;

	private string? subText;

	/// <summary>
	/// 在线状态
	/// </summary>
	public int? Online { get; set; }

	/// <summary>
	/// 在线自定义颜色
	/// </summary>
	public Color? OnlineCustom { get; set; }

	public Image? Icon { get; set; }

	public string? IconSvg { get; set; }

	/// <summary>
	/// 图标比例
	/// </summary>
	public float? IconRatio { get; set; }

	/// <summary>
	/// 文本
	/// </summary>
	public string Text
	{
		get
		{
			return Localization.GetLangIN(LocalizationText, _text);
		}
		set
		{
			_text = value;
		}
	}

	/// <summary>
	/// 国际化（文本）
	/// </summary>
	public string? LocalizationText { get; set; }

	/// <summary>
	/// 是否启用
	/// </summary>
	public bool Enable { get; set; } = true;

	/// <summary>
	/// 子文本
	/// </summary>
	public string? SubText
	{
		get
		{
			return Localization.GetLangI(LocalizationSubText, subText);
		}
		set
		{
			subText = value;
		}
	}

	/// <summary>
	/// 国际化（子文本）
	/// </summary>
	public string? LocalizationSubText { get; set; }

	/// <summary>
	/// 子选项
	/// </summary>
	public IList<object>? Sub { get; set; }

	public int MaxCount { get; set; }

	public object Tag { get; set; }

	internal int TextWidth { get; set; }

	internal int SubTextWidth { get; set; }

	/// <summary>
	/// 文字颜色
	/// </summary>
	public Color? Fore { get; set; }

	/// <summary>
	/// 子文字颜色
	/// </summary>
	public Color? ForeSub { get; set; }

	/// <summary>
	/// 激活背景颜色
	/// </summary>
	public Color? BackActive { get; set; }

	/// <summary>
	/// 激活背景渐变色
	/// </summary>
	public string? BackActiveExtend { get; set; }

	/// <summary>
	/// 标签文字颜色
	/// </summary>
	public Color? TagFore { get; set; }

	/// <summary>
	/// 标签背景颜色
	/// </summary>
	public Color? TagBack { get; set; }

	/// <summary>
	/// 标签背景渐变色
	/// </summary>
	public string? TagBackExtend { get; set; }

	public SelectItem(int online, Image? ico, string text, object tag)
		: this(text, tag)
	{
		Online = online;
		Icon = ico;
	}

	public SelectItem(int online, Image? ico, object tag)
		: this(tag)
	{
		Online = online;
		Icon = ico;
	}

	public SelectItem(Image? ico, string text, object tag)
		: this(text, tag)
	{
		Icon = ico;
	}

	public SelectItem(Image? ico, string text)
		: this(text)
	{
		Icon = ico;
	}

	public SelectItem(Image? ico, object tag)
		: this(tag)
	{
		Icon = ico;
	}

	public SelectItem(int online, string text, object tag)
		: this(text, tag)
	{
		Online = online;
	}

	public SelectItem(int online, object tag)
		: this(tag)
	{
		Online = online;
	}

	public SelectItem(object tag)
	{
		_text = tag.ToString() ?? string.Empty;
		Tag = tag;
	}

	public SelectItem(string text, object tag)
	{
		_text = text;
		Tag = tag;
	}

	public SelectItem SetText(string value, string? localization = null)
	{
		_text = value;
		LocalizationText = localization;
		return this;
	}

	public SelectItem SetSubText(string? value, string? localization = null)
	{
		subText = value;
		LocalizationSubText = localization;
		return this;
	}

	public SelectItem SetEnable(bool value = false)
	{
		Enable = value;
		return this;
	}

	public SelectItem SetOnline(int? value = 1)
	{
		Online = value;
		return this;
	}

	public SelectItem SetOnline(Color? value)
	{
		OnlineCustom = value;
		return this;
	}

	public SelectItem SetOnline(int? value, Color? color)
	{
		Online = value;
		OnlineCustom = color;
		return this;
	}

	public SelectItem SetIcon(Image? img)
	{
		Icon = img;
		return this;
	}

	public SelectItem SetIcon(string? svg)
	{
		IconSvg = svg;
		return this;
	}

	public SelectItem SetIconRatio(float? value)
	{
		IconRatio = value;
		return this;
	}

	public SelectItem SetSub(params object[] value)
	{
		Sub = value;
		return this;
	}

	public SelectItem SetSub(int value)
	{
		MaxCount = value;
		return this;
	}

	public SelectItem SetFore(Color? value)
	{
		Fore = value;
		return this;
	}

	public SelectItem SetForeSub(Color? value)
	{
		ForeSub = value;
		return this;
	}

	public SelectItem SetFore(Color? value, Color? sub)
	{
		Fore = value;
		ForeSub = sub;
		return this;
	}

	public SelectItem SetBackActive(Color? value)
	{
		BackActive = value;
		return this;
	}

	public SelectItem SetBackActive(string? value)
	{
		BackActiveExtend = value;
		return this;
	}

	public SelectItem SetTagFore(Color? value)
	{
		TagFore = value;
		return this;
	}

	public SelectItem SetTagBack(Color? value)
	{
		TagBack = value;
		return this;
	}

	public SelectItem SetTagBack(string? value)
	{
		TagBackExtend = value;
		return this;
	}

	public override string ToString()
	{
		return Text;
	}
}
