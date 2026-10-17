using System.Drawing;

namespace AntdUI;

/// <summary>
/// 超链接
/// </summary>
/// <seealso cref="T:AntdUI.ICell" />
public class CellLink : ICell
{
	private string? text;

	internal FormatFlags s_f = FormatFlags.Center | FormatFlags.NoWrap;

	private ContentAlignment textAlign = ContentAlignment.MiddleCenter;

	private bool enabled = true;

	internal AnimationTask? ThreadHover;

	internal AnimationTask? ThreadImageHover;

	internal bool _mouseDown;

	internal int AnimationHoverValue;

	internal bool AnimationHover;

	internal bool _mouseHover;

	internal bool AnimationImageHover;

	internal float AnimationImageHoverValue;

	internal AnimationTask? ThreadClick;

	internal bool AnimationClick;

	internal float AnimationClickValue;

	/// <summary>
	/// ID
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// 文本
	/// </summary>
	public string? Text
	{
		get
		{
			return Localization.GetLangI(LocalizationText, text, new string[2] { "{id}", Id });
		}
		set
		{
			if (!(text == value))
			{
				text = value;
				OnPropertyChanged(layout: true);
			}
		}
	}

	/// <summary>
	/// 国际化文本
	/// </summary>
	public string? LocalizationText { get; set; }

	/// <summary>
	/// 文本位置
	/// </summary>
	public ContentAlignment TextAlign
	{
		get
		{
			return textAlign;
		}
		set
		{
			if (textAlign != value)
			{
				textAlign = value;
				s_f = textAlign.SetAlignment(s_f);
				OnPropertyChanged();
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (enabled == value)
			{
				enabled = value;
			}
			enabled = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// 文本提示
	/// </summary>
	public string? Tooltip { get; set; }

	internal bool ExtraMouseDown
	{
		get
		{
			return _mouseDown;
		}
		set
		{
			if (_mouseDown != value)
			{
				_mouseDown = value;
				OnPropertyChanged();
			}
		}
	}

	internal virtual bool ExtraMouseHover
	{
		get
		{
			return _mouseHover;
		}
		set
		{
			if (_mouseHover != value)
			{
				_mouseHover = value;
				if (Enabled)
				{
					OnPropertyChanged();
				}
			}
		}
	}

	/// <summary>
	/// 超链接
	/// </summary>
	/// <param name="id">id</param>
	public CellLink(string id)
	{
		Id = id;
	}

	/// <summary>
	/// 超链接
	/// </summary>
	/// <param name="id">id</param>
	/// <param name="text">文本</param>
	public CellLink(string id, string? text)
	{
		Id = id;
		this.text = text;
	}

	public CellLink SetText(string? value, string? localization = null)
	{
		text = value;
		LocalizationText = localization;
		return this;
	}

	public CellLink SetLocalizationText(string? value)
	{
		LocalizationText = value;
		return this;
	}

	public CellLink SetTextAlign(ContentAlignment align = ContentAlignment.MiddleLeft)
	{
		textAlign = align;
		s_f = textAlign.SetAlignment(s_f);
		return this;
	}

	public CellLink SetEnabled(bool value = false)
	{
		enabled = value;
		return this;
	}

	public CellLink SetTooltip(string? tooltip)
	{
		Tooltip = tooltip;
		return this;
	}

	public override string? ToString()
	{
		return Text;
	}

	public override void PaintBack(Canvas g)
	{
	}

	public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
	{
		Table.PaintLink(g, font, Rect, this, enable, PARENT.PARENT.ColorScheme, PARENT.PARENT.Name);
	}

	public override Size GetSize(Canvas g, Font font, TableGaps gap)
	{
		Size size = g.MeasureText(Text ?? "龍Qq", font);
		return new Size(size.Width, size.Height);
	}

	public override void SetRect(Canvas g, Font font, Rectangle rect, Size size, int maxwidth, TableGaps gap)
	{
		Rect = new Rectangle(rect.X, rect.Y + (rect.Height - size.Height) / 2, rect.Width, size.Height);
	}

	internal virtual void Click()
	{
	}
}
