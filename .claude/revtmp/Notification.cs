using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

/// <summary>
/// Notification 通知提醒框
/// </summary>
/// <remarks>全局展示通知提醒信息。</remarks>
public static class Notification
{
	/// <summary>
	/// 配置
	/// </summary>
	public class Config
	{
		private string? title;

		private string? text;

		/// <summary>
		/// ID
		/// </summary>
		public string? ID { get; set; }

		/// <summary>
		/// 所属目标
		/// </summary>
		public Target Target { get; set; }

		/// <summary>
		/// 所属窗口
		/// </summary>
		[Obsolete("use Target")]
		public Control Form => Target.GetForm;

		/// <summary>
		/// 标题
		/// </summary>
		public string? Title
		{
			get
			{
				return Localization.GetLangI(LocalizationTitle, title, new string[2] { "{id}", ID });
			}
			set
			{
				title = value;
			}
		}

		/// <summary>
		/// 国际化（标题）
		/// </summary>
		public string? LocalizationTitle { get; set; }

		/// <summary>
		/// 标题字体
		/// </summary>
		public Font? FontTitle { get; set; }

		/// <summary>
		/// 标题字体样式
		/// </summary>
		public FontStyle? FontStyleTitle { get; set; }

		/// <summary>
		/// 文本
		/// </summary>
		public string? Text
		{
			get
			{
				return Localization.GetLangI(LocalizationText, text, new string[2] { "{id}", ID });
			}
			set
			{
				text = value;
			}
		}

		/// <summary>
		/// 国际化（文本）
		/// </summary>
		public string? LocalizationText { get; set; }

		/// <summary>
		/// 图标
		/// </summary>
		public TType Icon { get; set; }

		public IconInfo? IconCustom { get; set; }

		/// <summary>
		/// 字体
		/// </summary>
		public Font? Font { get; set; }

		/// <summary>
		/// 方向
		/// </summary>
		public TAlignFrom Align { get; set; }

		/// <summary>
		/// 圆角
		/// </summary>
		public int Radius { get; set; } = 10;

		/// <summary>
		/// 自动关闭时间（秒）0等于不关闭
		/// </summary>
		public int AutoClose { get; set; } = 6;

		/// <summary>
		/// 是否可以点击关闭
		/// </summary>
		public bool ClickClose { get; set; } = true;

		/// <summary>
		/// 是否显示关闭图标
		/// </summary>
		public bool CloseIcon { get; set; } = true;

		/// <summary>
		/// 是否置顶
		/// </summary>
		public bool TopMost { get; set; }

		/// <summary>
		/// 超链接
		/// </summary>
		public IConfigControl? Link
		{
			get
			{
				if (Links == null || Links.Length == 0)
				{
					return null;
				}
				return Links[0];
			}
			set
			{
				if (value == null)
				{
					Links = null;
				}
				else if (Links == null || Links.Length == 0)
				{
					Links = new IConfigControl[1] { value };
				}
				else
				{
					Links[0] = value;
				}
			}
		}

		/// <summary>
		/// 超链接组合
		/// </summary>
		public IConfigControl[]? Links { get; set; }

		/// <summary>
		/// 关闭回调
		/// </summary>
		public Action? OnClose { get; set; }

		/// <summary>
		/// 边距
		/// </summary>
		public Size Padding { get; set; } = new Size(24, 20);

		/// <summary>
		/// 弹出在窗口
		/// </summary>
		public bool? ShowInWindow { get; set; }

		/// <summary>
		/// 是否启用声音
		/// </summary>
		public bool EnableSound { get; set; }

		/// <summary>
		/// 自定义背景色
		/// </summary>
		public Color? Back { get; set; }

		/// <summary>
		/// 自定义前景色
		/// </summary>
		public Color? Fore { get; set; }

		public Config(Target target)
		{
			Target = target;
		}

		public Config(Target target, TType icon, TAlignFrom _align)
		{
			Target = target;
			Align = _align;
			Icon = icon;
		}

		public Config(Target target, string title, string text, TType icon, TAlignFrom align)
		{
			Target = target;
			Title = title;
			Text = text;
			Align = align;
			Icon = icon;
		}

		public Config(Target target, string title, string text, TType icon, TAlignFrom align, Font? font)
		{
			Target = target;
			Font = font;
			Title = title;
			Text = text;
			Align = align;
			Icon = icon;
		}

		public Config(Target target, string title, string text, TType icon, TAlignFrom align, Font? font, int? autoClose)
		{
			Target = target;
			Font = font;
			Title = title;
			Text = text;
			Align = align;
			Icon = icon;
			if (autoClose.HasValue)
			{
				AutoClose = autoClose.Value;
			}
		}

		public Config(Form form)
			: this(new Target(form))
		{
		}

		public Config(Form form, TType icon, TAlignFrom align)
			: this(new Target(form), icon, align)
		{
		}

		public Config(Form form, string title, string text, TType icon, TAlignFrom align)
			: this(new Target(form), title, text, icon, align)
		{
		}

		public Config(Form form, string title, string text, TType icon, TAlignFrom align, Font? font)
			: this(new Target(form), title, text, icon, align, font)
		{
		}

		public Config(Form form, string title, string text, TType icon, TAlignFrom align, Font? font, int? autoClose)
			: this(new Target(form), title, text, icon, align, font, autoClose)
		{
		}

		public Config SetID(string? value)
		{
			ID = value;
			return this;
		}

		public Config SetTitle(string? value, string? localization = null)
		{
			title = value;
			LocalizationTitle = localization;
			return this;
		}

		public Config SetTitle(Font? value)
		{
			FontTitle = value;
			return this;
		}

		public Config SetTitle(FontStyle? value)
		{
			FontStyleTitle = value;
			return this;
		}

		public Config SetText(string? value, string? localization = null)
		{
			text = value;
			LocalizationText = localization;
			return this;
		}

		public Config SetBack(Color? value)
		{
			Back = value;
			return this;
		}

		public Config SetFore(Color? value)
		{
			Fore = value;
			return this;
		}

		public Config SetIcon(TType icon = TType.Success)
		{
			IconCustom = null;
			Icon = icon;
			return this;
		}

		public Config SetIcon(string svg)
		{
			return SetIcon(new IconInfo(svg));
		}

		public Config SetIcon(string svg, Color? fill)
		{
			return SetIcon(new IconInfo(svg, fill));
		}

		public Config SetIcon(string svg, Color back, bool round)
		{
			return SetIcon(new IconInfo(svg)
			{
				Back = back,
				Round = round
			});
		}

		public Config SetIcon(string svg, Color back, int radius)
		{
			return SetIcon(new IconInfo(svg)
			{
				Back = back,
				Radius = radius
			});
		}

		public Config SetIcon(string svg, Color? fill, Color back, bool round)
		{
			return SetIcon(new IconInfo(svg, fill)
			{
				Back = back,
				Round = round
			});
		}

		public Config SetIcon(string svg, Color? fill, Color back, int radius)
		{
			return SetIcon(new IconInfo(svg, fill)
			{
				Back = back,
				Radius = radius
			});
		}

		public Config SetIcon(IconInfo iconInfo)
		{
			IconCustom = iconInfo;
			return this;
		}

		public Config SetFont(Font? value)
		{
			Font = value;
			return this;
		}

		public Config SetRadius(int value = 0)
		{
			Radius = value;
			return this;
		}

		public Config SetAutoClose(int value = 0)
		{
			AutoClose = value;
			return this;
		}

		public Config SetClickClose(bool value = false)
		{
			ClickClose = value;
			return this;
		}

		public Config SetCloseIcon(bool value = false)
		{
			CloseIcon = value;
			return this;
		}

		public Config SetTopMost(bool value = true)
		{
			TopMost = value;
			return this;
		}

		public Config SetAlign(TAlignFrom value = TAlignFrom.TR)
		{
			Align = value;
			return this;
		}

		public Config SetPadding(int x, int y)
		{
			Padding = new Size(x, y);
			return this;
		}

		public Config SetPadding(int size)
		{
			Padding = new Size(size, size);
			return this;
		}

		public Config SetPadding(Size size)
		{
			Padding = size;
			return this;
		}

		public Config SetShowInWindow(bool? value = true)
		{
			ShowInWindow = value;
			return this;
		}

		public Config SetOnClose(Action? value)
		{
			OnClose = value;
			return this;
		}

		public Config SetEnableSound(bool value = true)
		{
			EnableSound = value;
			return this;
		}

		public Config SetLink(string text, Func<bool> call)
		{
			Links = new IConfigControl[1]
			{
				new ConfigLink(text, call)
			};
			return this;
		}

		public Config SetLink(string text, object tag, Func<bool> call)
		{
			Links = new IConfigControl[1] { new ConfigLink(text, call).SetTag(tag) };
			return this;
		}

		public Config SetLink(params IConfigControl[] value)
		{
			Links = value;
			return this;
		}
	}

	public class ConfigLink : IConfigControl
	{
		private string? _text;

		/// <summary>
		/// 文本
		/// </summary>
		public string? Text
		{
			get
			{
				return Localization.GetLangI(LocalizationText, _text);
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
		/// 文本颜色
		/// </summary>
		public Color? Fore { get; set; }

		/// <summary>
		/// 是否显示标签
		/// </summary>
		public bool Link { get; set; } = true;

		/// <summary>
		/// 点击回调
		/// </summary>
		public Func<bool>? Call { get; set; }

		/// <summary>
		/// 用户定义数据
		/// </summary>
		public object? Tag { get; set; }

		internal Rectangle Rect { get; set; }

		internal Rectangle RectText { get; set; }

		internal Rectangle RectArrow { get; set; }

		public ConfigLink()
		{
		}

		public ConfigLink(string text)
		{
			_text = text;
		}

		public ConfigLink(string text, Func<bool> call)
		{
			_text = text;
			Call = call;
		}

		public ConfigLink SetText(string? value, string? localization = null)
		{
			_text = value;
			LocalizationText = localization;
			return this;
		}

		public ConfigLink SetFore(Color? value)
		{
			Fore = value;
			return this;
		}

		public ConfigLink SetLink(bool value = false)
		{
			Link = value;
			return this;
		}

		public ConfigLink SetTag(object? value)
		{
			Tag = value;
			return this;
		}

		public bool Contains(int x, int y)
		{
			return Rect.Contains(x, y);
		}
	}

	public interface IConfigControl
	{
		/// <summary>
		/// 文本
		/// </summary>
		string? Text { get; set; }

		/// <summary>
		/// 文本颜色
		/// </summary>
		Color? Fore { get; set; }

		/// <summary>
		/// 点击回调
		/// </summary>
		Func<bool>? Call { get; set; }

		/// <summary>
		/// 用户定义数据
		/// </summary>
		object? Tag { get; set; }

		bool Contains(int x, int y);
	}

	/// <summary>
	/// 最大显示数量
	/// </summary>
	public static int? MaxCount { get; set; }

	/// <summary>
	/// 成功通知
	/// </summary>
	/// <param name="form">窗口</param>
	/// <param name="title">标题</param>
	/// <param name="text">内容</param>
	/// <param name="align">位置</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void success(Form form, string title, string text, TAlignFrom align = TAlignFrom.TR, Font? font = null, int? autoClose = null)
	{
		new Config(form, title, text, TType.Success, align, font, autoClose).open();
	}

	/// <summary>
	/// 信息通知
	/// </summary>
	/// <param name="form">窗口</param>
	/// <param name="title">标题</param>
	/// <param name="text">内容</param>
	/// <param name="align">位置</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void info(Form form, string title, string text, TAlignFrom align = TAlignFrom.TR, Font? font = null, int? autoClose = null)
	{
		new Config(form, title, text, TType.Info, align, font, autoClose).open();
	}

	/// <summary>
	/// 警告通知
	/// </summary>
	/// <param name="form">窗口</param>
	/// <param name="title">标题</param>
	/// <param name="text">内容</param>
	/// <param name="align">位置</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void warn(Form form, string title, string text, TAlignFrom align = TAlignFrom.TR, Font? font = null, int? autoClose = null)
	{
		new Config(form, title, text, TType.Warn, align, font, autoClose).open();
	}

	/// <summary>
	/// 失败通知
	/// </summary>
	/// <param name="form">窗口</param>
	/// <param name="title">标题</param>
	/// <param name="text">内容</param>
	/// <param name="align">位置</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void error(Form form, string title, string text, TAlignFrom align = TAlignFrom.TR, Font? font = null, int? autoClose = null)
	{
		new Config(form, title, text, TType.Error, align, font, autoClose).open();
	}

	/// <summary>
	/// 普通通知
	/// </summary>
	/// <param name="form">窗口</param>
	/// <param name="title">标题</param>
	/// <param name="text">内容</param>
	/// <param name="align">位置</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void open(Form form, string title, string text, TAlignFrom align = TAlignFrom.TR, Font? font = null, int? autoClose = null)
	{
		new Config(form, title, text, TType.None, align, font, autoClose).open();
	}

	/// <summary>
	/// Notification 通知提醒框
	/// </summary>
	/// <param name="config">配置</param>
	public static void open(this Config config)
	{
		MsgQueue.Add(config);
	}

	/// <summary>
	/// 关闭全部
	/// </summary>
	public static void close_all()
	{
		List<NotificationFrm> list = new List<NotificationFrm>(10);
		foreach (KeyValuePair<string, List<ILayeredFormAnimate>> item2 in ILayeredFormAnimate.list)
		{
			foreach (ILayeredFormAnimate item3 in item2.Value)
			{
				if (item3 is NotificationFrm item)
				{
					list.Add(item);
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		foreach (NotificationFrm item4 in list)
		{
			item4.CloseMe();
		}
	}

	/// <summary>
	/// 关闭指定id
	/// </summary>
	public static void close_id(string id)
	{
		if (ILayeredFormAnimate.list.Count <= 0 && MsgQueue.queue.Count <= 0)
		{
			return;
		}
		bool flag = true;
		List<NotificationFrm> list = new List<NotificationFrm>();
		foreach (KeyValuePair<string, List<ILayeredFormAnimate>> item in ILayeredFormAnimate.list)
		{
			foreach (ILayeredFormAnimate item2 in item.Value)
			{
				if (item2 is NotificationFrm notificationFrm && notificationFrm.config.ID == id)
				{
					list.Add(notificationFrm);
					flag = false;
				}
			}
		}
		if (flag)
		{
			MsgQueue.volley.Add("N" + id);
		}
		if (list.Count == 0)
		{
			return;
		}
		foreach (NotificationFrm item3 in list)
		{
			item3.CloseMe();
		}
	}

	/// <summary>
	/// 判断通知ID是否存在队列
	/// </summary>
	/// <param name="id">ID</param>
	/// <param name="time_expand">是否延长时间</param>
	public static bool contains(string id, bool time_expand = false)
	{
		return MsgQueue.contains("N" + id, time_expand);
	}
}
