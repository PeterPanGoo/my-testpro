using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI;

/// <summary>
/// 进度条
/// </summary>
/// <seealso cref="T:AntdUI.ICell" />
public class CellProgress : ICell
{
	private Color? back;

	private Color? fill;

	private int radius = 6;

	private TShape shape = TShape.Round;

	private float _value;

	/// <summary>
	/// 背景颜色
	/// </summary>
	public Color? Back
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
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// 进度条颜色
	/// </summary>
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
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// 圆角
	/// </summary>
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
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// 形状
	/// </summary>
	public TShape Shape
	{
		get
		{
			return shape;
		}
		set
		{
			if (shape != value)
			{
				shape = value;
				OnPropertyChanged(layout: true);
			}
		}
	}

	/// <summary>
	/// 进度条
	/// </summary>
	public float Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				else if (value > 1f)
				{
					value = 1f;
				}
				_value = value;
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// 大小
	/// </summary>
	public Size? Size { get; set; }

	/// <summary>
	/// 进度条
	/// </summary>
	/// <param name="value">进度</param>
	public CellProgress(float value)
	{
		_value = value;
	}

	public CellProgress SetBack(Color? value)
	{
		back = value;
		return this;
	}

	public CellProgress SetFill(Color? value)
	{
		fill = value;
		return this;
	}

	public CellProgress SetRadius(int value = 0)
	{
		radius = value;
		return this;
	}

	public CellProgress SetShape(TShape value = TShape.Default)
	{
		shape = value;
		return this;
	}

	public CellProgress SetValue(float value = 1f)
	{
		if (value < 0f)
		{
			value = 0f;
		}
		else if (value > 1f)
		{
			value = 1f;
		}
		_value = value;
		return this;
	}

	public CellProgress SetSize(Size? value)
	{
		Size = value;
		return this;
	}

	public CellProgress SetSize(int width, int height)
	{
		Size = new Size(width, height);
		return this;
	}

	public CellProgress SetSize(int size)
	{
		Size = new Size(size, size);
		return this;
	}

	public override string ToString()
	{
		return _value * 100f + "%";
	}

	public override void PaintBack(Canvas g)
	{
	}

	public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
	{
		Color color = Fill ?? Colour.Primary.Get(PARENT.PARENT.ColorScheme, "Progress", PARENT.PARENT.Name);
		Color color2 = Back ?? Colour.FillSecondary.Get(PARENT.PARENT.ColorScheme, "Progress", PARENT.PARENT.Name);
		if (Shape == TShape.Circle)
		{
			float width = (float)Radius * g.Dpi;
			g.DrawEllipse(color2, width, Rect);
			if (Value > 0f)
			{
				int num = (int)(360f * Value);
				using Pen pen = new Pen(color, width);
				LineCap startCap = (pen.EndCap = LineCap.Round);
				pen.StartCap = startCap;
				g.DrawArc(pen, Rect, -90f, num);
				return;
			}
			return;
		}
		float num2 = (float)Radius * g.Dpi;
		if (Shape == TShape.Round)
		{
			num2 = Rect.Height;
		}
		using GraphicsPath path = Rect.RoundPath(num2);
		g.Fill(color2, path);
		if (!(Value > 0f))
		{
			return;
		}
		float num3 = (float)Rect.Width * Value;
		if (num3 > num2)
		{
			using (GraphicsPath path2 = new RectangleF(Rect.X, Rect.Y, num3, Rect.Height).RoundPath(num2))
			{
				g.Fill(color, path2);
				return;
			}
		}
		using Bitmap image = new Bitmap(Rect.Width, Rect.Height);
		using (Canvas canvas = Graphics.FromImage(image).High(g.Dpi))
		{
			using SolidBrush brush = new SolidBrush(color);
			canvas.FillEllipse(brush, new RectangleF(0f, 0f, num3, Rect.Height));
		}
		using TextureBrush textureBrush = new TextureBrush(image, WrapMode.Clamp);
		textureBrush.TranslateTransform(Rect.X, Rect.Y);
		g.Fill(textureBrush, path);
	}

	public override Size GetSize(Canvas g, Font font, TableGaps gap)
	{
		if (Size.HasValue)
		{
			return new Size((int)((float)Size.Value.Width * g.Dpi), (int)((float)Size.Value.Height * g.Dpi));
		}
		int height = g.MeasureString("龍Qq", font).Height;
		if (Shape == TShape.Circle)
		{
			int num = gap.x2 + height;
			return new Size(num, num);
		}
		return new Size(height * 2, height / 2);
	}

	public override void SetRect(Canvas g, Font font, Rectangle rect, Size size, int maxwidth, TableGaps gap)
	{
		int num = rect.Width;
		int num2 = size.Height;
		if (Shape == TShape.Circle)
		{
			num = size.Width - gap.x2;
			num2 = size.Height - gap.y2;
		}
		Rect = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y + (rect.Height - num2) / 2, num, num2);
	}
}
