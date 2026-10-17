using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AntdUI;

/// <summary>
/// Message 全局提示
/// </summary>
/// <remarks>全局展示操作反馈信息。</remarks>
public static class Message
{
	/// <summary>
	/// 配置
	/// </summary>
	public class Config
	{
		private string? text;

		internal Action? refresh;

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
		/// 加载回调
		/// </summary>
		public object? Call { get; set; }

		/// <summary>
		/// 字体
		/// </summary>
		public Font? Font { get; set; }

		/// <summary>
		/// 圆角
		/// </summary>
		public int Radius { get; set; } = 6;

		/// <summary>
		/// 自动关闭时间（秒）0等于不关闭
		/// </summary>
		public int AutoClose { get; set; } = 6;

		/// <summary>
		/// 是否可以点击关闭
		/// </summary>
		public bool ClickClose { get; set; } = true;

		/// <summary>
		/// 是否置顶
		/// </summary>
		public bool TopMost { get; set; }

		/// <summary>
		/// 方向
		/// </summary>
		public TAlignFrom Align { get; set; } = TAlignFrom.Top;

		/// <summary>
		/// 边距
		/// </summary>
		public Size Padding { get; set; } = new Size(12, 9);

		/// <summary>
		/// 弹出在窗口
		/// </summary>
		public bool? ShowInWindow { get; set; }

		/// <summary>
		/// 是否启用声音
		/// </summary>
		public bool EnableSound { get; set; }

		/// <summary>
		/// 最大宽度
		/// </summary>
		public int? MaxWidth { get; set; }

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

		public Config(Target target, string text)
		{
			Target = target;
			Text = text;
		}

		public Config(Target target, string text, TType icon)
		{
			Target = target;
			Text = text;
			Icon = icon;
		}

		public Config(Target target, string text, Font? font)
		{
			Target = target;
			Font = font;
			Text = text;
		}

		public Config(Target target, string text, TType icon, Font? font)
		{
			Target = target;
			Font = font;
			Text = text;
			Icon = icon;
		}

		public Config(Target target, string text, TType icon, Font? font, int? autoClose)
		{
			Target = target;
			Font = font;
			Text = text;
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

		public Config(Form form, string text)
			: this(new Target(form), text)
		{
		}

		public Config(Form form, string text, TType icon)
			: this(new Target(form), text, icon)
		{
		}

		public Config(Form form, string text, Font? font)
			: this(new Target(form), text, font)
		{
		}

		public Config(Form form, string text, TType icon, Font? font)
			: this(new Target(form), text, icon, font)
		{
		}

		public Config(Form form, string text, TType icon, Font? font, int? autoClose)
			: this(new Target(form), text, icon, font, autoClose)
		{
		}

		public void OK(string text)
		{
			Icon = TType.Success;
			Text = text;
			Refresh();
		}

		public void Error(string text)
		{
			Icon = TType.Error;
			Text = text;
			Refresh();
		}

		public void Warn(string text)
		{
			Icon = TType.Warn;
			Text = text;
			Refresh();
		}

		public void Info(string text)
		{
			Icon = TType.Info;
			Text = text;
			Refresh();
		}

		public void Refresh()
		{
			refresh?.Invoke();
		}

		public Config SetID(string? value)
		{
			ID = value;
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

		public Config SetCall(Action? value)
		{
			Call = value;
			return this;
		}

		public Config SetCall(Action<Config>? value)
		{
			Call = value;
			return this;
		}

		public Config SetCall(Func<Config, Task>? value)
		{
			Call = value;
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

		public Config SetTopMost(bool value = true)
		{
			TopMost = value;
			return this;
		}

		public Config SetAlign(TAlignFrom value = TAlignFrom.Bottom)
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

		public Config SetEnableSound(bool value = true)
		{
			EnableSound = value;
			return this;
		}

		public Config SetMaxWidth(int? value)
		{
			MaxWidth = value;
			return this;
		}
	}

	/// <summary>
	/// 最大显示数量
	/// </summary>
	public static int? MaxCount { get; set; }

	/// <summary>
	/// 成功提示
	/// </summary>
	/// <param name="target">目标</param>
	/// <param name="text">提示内容</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void success(Target target, string text, Font? font = null, int? autoClose = null)
	{
		new Config(target, text, TType.Success, font, autoClose).open();
	}

	/// <summary>
	/// 信息提示
	/// </summary>
	/// <param name="target">目标</param>
	/// <param name="text">提示内容</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void info(Target target, string text, Font? font = null, int? autoClose = null)
	{
		new Config(target, text, TType.Info, font, autoClose).open();
	}

	/// <summary>
	/// 警告提示
	/// </summary>
	/// <param name="target">目标</param>
	/// <param name="text">提示内容</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void warn(Target target, string text, Font? font = null, int? autoClose = null)
	{
		new Config(target, text, TType.Warn, font, autoClose).open();
	}

	/// <summary>
	/// 失败提示
	/// </summary>
	/// <param name="target">目标</param>
	/// <param name="text">提示内容</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void error(Target target, string text, Font? font = null, int? autoClose = null)
	{
		new Config(target, text, TType.Error, font, autoClose).open();
	}

	/// <summary>
	/// 加载提示
	/// </summary>
	/// <param name="target">目标</param>
	/// <param name="text">提示内容</param>
	/// <param name="call">耗时任务</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void loading(Target target, string text, Action<Config> call, Font? font = null, int? autoClose = null)
	{
		new Config(target, text, TType.None, font, autoClose).SetCall(call).open();
	}

	/// <summary>
	/// 加载提示
	/// </summary>
	/// <param name="target">目标</param>
	/// <param name="text">提示内容</param>
	/// <param name="call">耗时任务</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void loading(Target target, string text, Func<Config, Task> call, Font? font = null, int? autoClose = null)
	{
		new Config(target, text, TType.None, font, autoClose).SetCall(call).open();
	}

	public static void open(Target target, string text, Font? font = null, int? autoClose = null)
	{
		new Config(target, text, TType.None, font, autoClose).open();
	}

	/// <summary>
	/// 成功提示
	/// </summary>
	/// <param name="form">窗口</param>
	/// <param name="text">提示内容</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void success(Form form, string text, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.Success, font, autoClose).open();
	}

	/// <summary>
	/// 信息提示
	/// </summary>
	/// <param name="form">窗口</param>
	/// <param name="text">提示内容</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void info(Form form, string text, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.Info, font, autoClose).open();
	}

	/// <summary>
	/// 警告提示
	/// </summary>
	/// <param name="form">窗口</param>
	/// <param name="text">提示内容</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void warn(Form form, string text, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.Warn, font, autoClose).open();
	}

	/// <summary>
	/// 失败提示
	/// </summary>
	/// <param name="form">窗口</param>
	/// <param name="text">提示内容</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void error(Form form, string text, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.Error, font, autoClose).open();
	}

	/// <summary>
	/// 加载提示
	/// </summary>
	/// <param name="form">窗口</param>
	/// <param name="text">提示内容</param>
	/// <param name="call">耗时任务</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void loading(Form form, string text, Action<Config> call, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.None, font, autoClose).SetCall(call).open();
	}

	/// <summary>
	/// 加载提示
	/// </summary>
	/// <param name="form">窗口</param>
	/// <param name="text">提示内容</param>
	/// <param name="call">耗时任务</param>
	/// <param name="font">字体</param>
	/// <param name="autoClose">自动关闭时间（秒）0等于不关闭</param>
	public static void loading(Form form, string text, Func<Config, Task> call, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.None, font, autoClose).SetCall(call).open();
	}

	public static void open(Form form, string text, Font? font = null, int? autoClose = null)
	{
		new Config(form, text, TType.None, font, autoClose).open();
	}

	/// <summary>
	/// Message 全局提示
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
		List<MessageFrm> list = new List<MessageFrm>(10);
		foreach (KeyValuePair<string, List<ILayeredFormAnimate>> item2 in ILayeredFormAnimate.list)
		{
			foreach (ILayeredFormAnimate item3 in item2.Value)
			{
				if (item3 is MessageFrm item)
				{
					list.Add(item);
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		foreach (MessageFrm item4 in list)
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
		List<MessageFrm> list = new List<MessageFrm>();
		foreach (KeyValuePair<string, List<ILayeredFormAnimate>> item in ILayeredFormAnimate.list)
		{
			foreach (ILayeredFormAnimate item2 in item.Value)
			{
				if (item2 is MessageFrm messageFrm && messageFrm.config.ID == id)
				{
					list.Add(messageFrm);
					flag = false;
				}
			}
		}
		if (flag)
		{
			MsgQueue.volley.Add("M" + id);
		}
		if (list.Count == 0)
		{
			return;
		}
		foreach (MessageFrm item3 in list)
		{
			item3.CloseMe();
		}
	}

	/// <summary>
	/// 判断提示ID是否存在队列
	/// </summary>
	/// <param name="id">ID</param>
	/// <param name="time_expand">是否延长时间</param>
	public static bool contains(string id, bool time_expand = false)
	{
		return MsgQueue.contains("M" + id, time_expand);
	}
}
