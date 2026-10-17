using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace AntdUI;

/// <summary>
/// 表头
/// </summary>
public class Column
{
	private string _title;

	private bool visible = true;

	private bool lineBreak;

	private bool colBreak;

	private bool wrap;

	private int wrapCount;

	private TableWrapGap wrapGapX;

	private TableWrapGap wrapGapY;

	private bool _fixed;

	private bool sortorder;

	private SortMode sortMode;

	/// <summary>
	/// 绑定名称
	/// </summary>
	public string Key { get; set; }

	/// <summary>
	/// 显示文字
	/// </summary>
	public string Title
	{
		get
		{
			return Localization.GetLangIN(LocalizationTitle, _title, new string[2] { "{id}", Key });
		}
		set
		{
			if (!(_title == value))
			{
				_title = value;
				Invalidates();
			}
		}
	}

	[Description("显示文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationTitle { get; set; }

	/// <summary>
	/// 是否显示
	/// </summary>
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
	/// 返回当前列的显示索引
	/// </summary>
	public int VisibleIndex
	{
		get
		{
			if (PARENT?.rows == null || PARENT.rows.Length == 0)
			{
				return -1;
			}
			Table.CELL[] cells = PARENT.rows.First.cells;
			foreach (Table.CELL cELL in cells)
			{
				if (cELL.COLUMN == this)
				{
					return cELL.INDEX;
				}
			}
			return INDEX;
		}
	}

	/// <summary>
	/// 对齐方式
	/// </summary>
	public ColumnAlign Align { get; set; }

	/// <summary>
	/// 表头对齐方式
	/// </summary>
	public ColumnAlign? ColAlign { get; set; }

	/// <summary>
	/// 列宽度
	/// </summary>
	public string? Width { get; set; }

	/// <summary>
	/// 宽度（像素）
	/// </summary>
	public int WidthPixel { get; internal set; }

	/// <summary>
	/// 列最大宽度
	/// </summary>
	public string? MaxWidth { get; set; }

	/// <summary>
	/// 列最小宽度
	/// </summary>
	public string? MinWidth { get; set; }

	/// <summary>
	/// 只读
	/// </summary>
	public bool ReadOnly { get; set; }

	/// <summary>
	/// 超过宽度将自动省略
	/// </summary>
	public bool Ellipsis { get; set; }

	/// <summary>
	/// 自动换行
	/// </summary>
	public bool LineBreak
	{
		get
		{
			return lineBreak;
		}
		set
		{
			if (lineBreak != value)
			{
				lineBreak = value;
				Invalidates();
			}
		}
	}

	/// <summary>
	/// 表头自动换行
	/// </summary>
	public bool ColBreak
	{
		get
		{
			return colBreak;
		}
		set
		{
			if (colBreak != value)
			{
				colBreak = value;
				Invalidates();
			}
		}
	}

	/// <summary>
	/// 单元格内子元素自动换行
	/// </summary>
	public bool Wrap
	{
		get
		{
			return wrap;
		}
		set
		{
			if (wrap != value)
			{
				wrap = value;
				Invalidates();
			}
		}
	}

	public int WrapCount
	{
		get
		{
			return wrapCount;
		}
		set
		{
			if (wrapCount != value)
			{
				wrapCount = value;
				Invalidates();
			}
		}
	}

	/// <summary>
	/// 行内水平间距模式
	/// </summary>
	public TableWrapGap WrapGapX
	{
		get
		{
			return wrapGapX;
		}
		set
		{
			if (wrapGapX != value)
			{
				wrapGapX = value;
				Invalidates();
			}
		}
	}

	/// <summary>
	/// 行间垂直间距模式
	/// </summary>
	public TableWrapGap WrapGapY
	{
		get
		{
			return wrapGapY;
		}
		set
		{
			if (wrapGapY != value)
			{
				wrapGapY = value;
				Invalidates();
			}
		}
	}

	/// <summary>
	/// 列是否固定
	/// </summary>
	public bool Fixed
	{
		get
		{
			return _fixed;
		}
		set
		{
			if (_fixed != value)
			{
				_fixed = value;
				Invalidates();
			}
		}
	}

	/// <summary>
	/// 列可编辑
	/// </summary>
	public bool Editable { get; set; } = true;

	/// <summary>
	/// 启用排序
	/// </summary>
	public bool SortOrder
	{
		get
		{
			return sortorder;
		}
		set
		{
			if (sortorder != value)
			{
				sortorder = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 排序模式
	/// </summary>
	public SortMode SortMode
	{
		get
		{
			return sortMode;
		}
		set
		{
			if (sortMode == value)
			{
				return;
			}
			if (PARENT == null || PARENT.rows == null)
			{
				sortMode = value;
				Invalidate();
				return;
			}
			Table.CELL[] cells = PARENT.rows.First.cells;
			foreach (Table.CELL cELL in cells)
			{
				if (cELL.COLUMN.SortOrder)
				{
					cELL.COLUMN.sortMode = SortMode.NONE;
				}
			}
			sortMode = value;
			Invalidate();
		}
	}

	/// <summary>
	/// 存在筛选
	/// </summary>
	public bool HasFilter { get; private set; }

	private FilterOption? filter { get; set; }

	/// <summary>
	/// 用户筛选选项
	/// </summary>
	public FilterOption? Filter
	{
		get
		{
			return filter;
		}
		set
		{
			filter = value;
			if (filter == null)
			{
				HasFilter = false;
			}
			else
			{
				HasFilter = true;
			}
			Invalidate();
		}
	}

	/// <summary>
	/// 格式化显示（如日期：D, yyyy-MM-dd, dd MMM yyyy..., 数字格式化：C, D5, P2, 0.###...）
	/// </summary>
	public string? DisplayFormat { get; set; }

	/// <summary>
	/// 汇总栏选项
	/// </summary>
	public SummaryItemOption? SummaryItem { get; set; }

	/// <summary>
	/// 列可拖拽
	/// </summary>
	public bool DragSort { get; set; } = true;

	/// <summary>
	/// 树形列
	/// </summary>
	public string? KeyTree { get; set; }

	/// <summary>
	/// 列样式
	/// </summary>
	public Table.CellStyleInfo? Style { get; set; }

	/// <summary>
	/// 标题列样式
	/// </summary>
	public Table.CellStyleInfo? ColStyle { get; set; }

	/// <summary>
	/// 用户定义数据
	/// </summary>
	public object? Tag { get; set; }

	internal Table? PARENT { get; set; }

	public int INDEX { get; internal set; }

	public int INDEX_REAL { get; internal set; }

	/// <summary>
	/// 插槽
	/// </summary>
	public Func<object?, object, int, object?>? Render { get; set; }

	/// <summary>
	/// 表头
	/// </summary>
	/// <param name="key">绑定名称</param>
	/// <param name="title">显示文字</param>
	public Column(string key, string title)
	{
		Key = key;
		_title = title;
	}

	/// <summary>
	/// 表头
	/// </summary>
	/// <param name="key">绑定名称</param>
	/// <param name="title">显示文字</param>
	/// <param name="align">对齐方式</param>
	public Column(string key, string title, ColumnAlign align)
	{
		Key = key;
		_title = title;
		Align = align;
	}

	/// <summary>
	/// 返回格式化的字符串
	/// </summary>
	/// <param name="value">数据</param>
	/// <returns></returns>
	public string? GetDisplayText(object? value)
	{
		return GetDisplayText(value, DisplayFormat);
	}

	/// <summary>
	/// 返回格式化的字符串
	/// </summary>
	/// <param name="value">数据</param>
	/// <param name="format">格式化</param>
	/// <returns></returns>
	internal static string? GetDisplayText(object? value, string? format)
	{
		if (value.IsNull())
		{
			return null;
		}
		if (format == null || string.IsNullOrEmpty(format))
		{
			if (value is IList<ICell> list)
			{
				List<string> list2 = new List<string>(list.Count);
				foreach (ICell item in list)
				{
					string text = item.ToString();
					if (text != null)
					{
						list2.Add(text);
					}
				}
				if (list2.Count > 0)
				{
					return string.Join(" ", list2);
				}
			}
			return value?.ToString();
		}
		try
		{
			if (format.Contains("{0:"))
			{
				return string.Format(format, value);
			}
			return string.Format("{0:" + format + "}", value);
		}
		catch
		{
			return value?.ToString();
		}
	}

	private void Invalidate()
	{
		if (PARENT != null && PARENT.LoadLayout())
		{
			PARENT.Invalidate();
		}
	}

	private void Invalidates()
	{
		if (PARENT != null)
		{
			PARENT.ExtractHeaderFixed();
			if (PARENT.LoadLayout())
			{
				PARENT.Invalidate();
			}
		}
	}

	internal bool SetSortMode(SortMode value)
	{
		if (sortMode == value)
		{
			return false;
		}
		sortMode = value;
		return true;
	}

	/// <summary>
	/// 设置显示文字
	/// </summary>
	public Column SetTitle(string value, string? localization = null)
	{
		_title = value;
		LocalizationTitle = localization;
		return this;
	}

	/// <summary>
	/// 设置国际化显示文本
	/// </summary>
	public Column SetLocalizationTitle(string? value)
	{
		LocalizationTitle = value;
		return this;
	}

	/// <summary>
	/// 设置国际化显示文本（后面插入id）
	/// </summary>
	public Column SetLocalizationTitleID(string value)
	{
		LocalizationTitle = value + "{id}";
		return this;
	}

	/// <summary>
	/// 设置是否显示
	/// </summary>
	public Column SetVisible(bool value = false)
	{
		Visible = value;
		return this;
	}

	/// <summary>
	/// 设置对齐方式
	/// </summary>
	public Column SetAlign(ColumnAlign value = ColumnAlign.Center)
	{
		Align = value;
		return this;
	}

	/// <summary>
	/// 设置表头对齐方式
	/// </summary>
	public Column SetColAlign(ColumnAlign value = ColumnAlign.Center)
	{
		ColAlign = value;
		return this;
	}

	/// <summary>
	/// 设置对齐方式
	/// </summary>
	/// <param name="value">内容对齐方式</param>
	/// <param name="col">表头对齐方式</param>
	public Column SetAligns(ColumnAlign value = ColumnAlign.Center, ColumnAlign col = ColumnAlign.Center)
	{
		Align = value;
		ColAlign = col;
		return this;
	}

	/// <summary>
	/// 设置列宽度
	/// </summary>
	public Column SetWidth(string? value = null)
	{
		Width = value;
		return this;
	}

	/// <summary>
	/// 设置列自动宽度
	/// </summary>
	public Column SetWidthAuto()
	{
		Width = "auto";
		return this;
	}

	/// <summary>
	/// 设置列填充剩余宽度
	/// </summary>
	public Column SetWidthFill()
	{
		Width = "fill";
		return this;
	}

	/// <summary>
	/// 设置列宽度
	/// </summary>
	public Column SetWidth(int value)
	{
		Width = value.ToString();
		return this;
	}

	/// <summary>
	/// 设置列宽度
	/// </summary>
	public Column SetWidth(float value)
	{
		Width = value.ToString();
		return this;
	}

	/// <summary>
	/// 设置列最大宽度
	/// </summary>
	public Column SetMaxWidth(string? value = null)
	{
		MaxWidth = value;
		return this;
	}

	/// <summary>
	/// 设置列最大宽度
	/// </summary>
	public Column SetMaxWidth(int value)
	{
		MaxWidth = value.ToString();
		return this;
	}

	/// <summary>
	/// 设置列最大宽度
	/// </summary>
	public Column SetMaxWidth(float value)
	{
		MaxWidth = value.ToString();
		return this;
	}

	/// <summary>
	/// 设置列最小宽度
	/// </summary>
	public Column SetMinWidth(string? value = null)
	{
		MinWidth = value;
		return this;
	}

	/// <summary>
	/// 设置列最小宽度
	/// </summary>
	public Column SetMinWidth(int value)
	{
		MinWidth = value.ToString();
		return this;
	}

	/// <summary>
	/// 设置列最小宽度
	/// </summary>
	public Column SetMinWidth(float value)
	{
		MinWidth = value.ToString();
		return this;
	}

	public Column SetReadOnly(bool value = true)
	{
		ReadOnly = value;
		return this;
	}

	/// <summary>
	/// 设置超过宽度将自动省略
	/// </summary>
	public Column SetEllipsis(bool value = true)
	{
		Ellipsis = value;
		return this;
	}

	/// <summary>
	/// 设置自动换行
	/// </summary>
	public Column SetLineBreak(bool value = true)
	{
		LineBreak = value;
		return this;
	}

	/// <summary>
	/// 设置表头自动换行
	/// </summary>
	public Column SetColumBreak(bool value = true)
	{
		ColBreak = value;
		return this;
	}

	/// <summary>
	/// 设置单元格内子元素换行：0 关闭换行，&gt;0 按数量换行，&lt;0 按宽度自适应换行
	/// </summary>
	public Column SetWrap(int count = 0)
	{
		if (count > 0)
		{
			Wrap = true;
			WrapCount = count;
		}
		else if (count < 0)
		{
			Wrap = true;
			WrapCount = 0;
		}
		else
		{
			Wrap = false;
			WrapCount = 0;
		}
		return this;
	}

	/// <summary>
	/// 设置行内水平间距模式
	/// </summary>
	public Column SetWrapGapX(TableWrapGap value)
	{
		WrapGapX = value;
		return this;
	}

	/// <summary>
	/// 设置行间垂直间距模式
	/// </summary>
	public Column SetWrapGapY(TableWrapGap value)
	{
		WrapGapY = value;
		return this;
	}

	/// <summary>
	/// 设置列是否固定
	/// </summary>
	public Column SetFixed(bool value = true)
	{
		Fixed = value;
		return this;
	}

	/// <summary>
	/// 设置列是否可编辑
	/// 注意：该方法必须配合EditMode使用控制某列在编辑模式下是否可编辑
	/// 默认设置为false
	/// </summary>
	public Column SetEditable(bool value = false)
	{
		Editable = value;
		return this;
	}

	/// <summary>
	/// 设置启用排序
	/// </summary>
	public Column SetSortOrder(bool value = true)
	{
		SortOrder = value;
		return this;
	}

	/// <summary>
	/// 设置默认筛选选项 (string)
	/// </summary>
	/// <returns></returns>
	public Column SetDefaultFilter()
	{
		return SetDefaultFilter(typeof(string));
	}

	/// <summary>
	/// 设置默认筛选选项
	/// </summary>
	/// <param name="type">数据类型</param>
	/// <returns></returns>
	public Column SetDefaultFilter(Type type)
	{
		Filter = new FilterOption(type);
		return this;
	}

	/// <summary>
	/// 设置格式化显示
	/// 建议非string类型需要时设置
	/// </summary>
	/// <param name="format">string.Format格式化。如日期：D, yyyy-MM-dd, dd MMM yyyy..., 数字格式化：C, D5, P2, 0.###...</param>
	public Column SetDisplayFormat(string format)
	{
		DisplayFormat = format;
		return this;
	}

	public Column SetSummaryItem(TSummaryType summaryType)
	{
		SummaryItem = new SummaryItemOption(summaryType);
		return this;
	}

	public Column SetSummaryItem(TSummaryType summaryType, string format)
	{
		SummaryItem = new SummaryItemOption(summaryType, format);
		return this;
	}

	public Column SetSummaryItem(string text)
	{
		SummaryItem = new SummaryItemOption(TSummaryType.Text)
		{
			DisplayText = text
		};
		return this;
	}

	/// <summary>
	/// 设置列可拖拽
	/// </summary>
	public Column SetDragSort(bool value = false)
	{
		DragSort = value;
		return this;
	}

	/// <summary>
	/// 设置树形列
	/// </summary>
	public Column SetTree(string? key)
	{
		KeyTree = key;
		return this;
	}

	/// <summary>
	/// 设置列样式
	/// </summary>
	public Column SetStyle(Color? back, Color? fore = null)
	{
		return SetStyle(new Table.CellStyleInfo
		{
			BackColor = back,
			ForeColor = fore
		});
	}

	/// <summary>
	/// 设置列样式
	/// </summary>
	public Column SetStyle(Table.CellStyleInfo? style)
	{
		Style = style;
		return this;
	}

	/// <summary>
	/// 设置标题列样式
	/// </summary>
	public Column SetColStyle(Color? back, Color? fore = null)
	{
		return SetColStyle(new Table.CellStyleInfo
		{
			BackColor = back,
			ForeColor = fore
		});
	}

	/// <summary>
	/// 设置标题列样式
	/// </summary>
	public Column SetColStyle(Table.CellStyleInfo? style)
	{
		ColStyle = style;
		return this;
	}

	public Column SetRender(Func<object?, object, int, object?>? value)
	{
		Render = value;
		return this;
	}

	public Column SetTag(object? value)
	{
		Tag = value;
		return this;
	}

	internal bool EqualsKeyTree(string? value)
	{
		if (value == null)
		{
			return false;
		}
		return KeyTree == value;
	}
}
