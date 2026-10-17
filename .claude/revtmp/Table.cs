using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

/// <summary>
/// Table 表格
/// </summary>
/// <remarks>展示行列数据。</remarks>
[Description("Table 表格")]
[DefaultEvent("CellClick")]
[ToolboxItem(true)]
public class Table : IControl, IEventListener, IScrollBar
{
	internal class TableSubValue
	{
		public object record { get; set; }

		public PropertyDescriptor prop { get; set; }

		public TableSubValue(object value, PropertyDescriptor p)
		{
			record = value;
			prop = p;
		}
	}

	internal class TempTable
	{
		private IRow[] _rowsCache;

		/// <summary>
		/// 表头 - 列
		/// </summary>
		public TempiColumn[] columns { get; set; }

		/// <summary>
		/// 数据 - 行
		/// </summary>
		public IRow[] rows
		{
			get
			{
				return rowsFilter ?? _rowsCache;
			}
			set
			{
				lock (_rowsCache)
				{
					_rowsCache = value;
				}
			}
		}

		/// <summary>
		/// 所有行缓存
		/// </summary>
		public IRow[] RowsCache => _rowsCache;

		/// <summary>
		/// 数据 - 行已筛选
		/// </summary>
		public IRow[]? rowsFilter { get; set; }

		/// <summary>
		/// 数据 - 行
		/// </summary>
		public IRow[]? summary { get; set; }

		public TempTable(TempiColumn[] _columns, IRow[] _rows, IRow[]? _summary)
		{
			columns = _columns;
			_rowsCache = _rows;
			summary = _summary;
		}

		public void ClearFilter()
		{
			rowsFilter = null;
		}

		public void ForRowI(Action<int> action)
		{
			lock (_rowsCache)
			{
				IRow[] array = rowsFilter ?? _rowsCache;
				for (int i = 0; i < array.Length; i++)
				{
					action(i);
				}
			}
		}

		public void ForRow(Action<IRow> action)
		{
			lock (_rowsCache)
			{
				IRow[] array = rowsFilter ?? _rowsCache;
				for (int i = 0; i < array.Length; i++)
				{
					action(array[i]);
				}
			}
		}

		public int[] GetInts(int count = 0)
		{
			lock (_rowsCache)
			{
				IRow[] array = rowsFilter ?? _rowsCache;
				int[] array2 = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array2[i] = i + count;
				}
				return array2;
			}
		}

		public object[] GetRecord()
		{
			lock (_rowsCache)
			{
				IRow[] array = rowsFilter ?? _rowsCache;
				object[] array2 = new object[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array2[i] = array[i].record;
				}
				return array2;
			}
		}

		public int FindRecord(object record)
		{
			lock (_rowsCache)
			{
				IRow[] array = rowsFilter ?? _rowsCache;
				_ = new object[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].record == record)
					{
						return i;
					}
				}
			}
			return -1;
		}

		public object[] GetRecord(int[] sortData)
		{
			lock (_rowsCache)
			{
				IRow[] array = rowsFilter ?? _rowsCache;
				List<object> list = new List<object>(array.Length);
				foreach (int num in sortData)
				{
					try
					{
						list.Add(array[num].record);
					}
					catch
					{
					}
				}
				return list.ToArray();
			}
		}

		public int FindTreeIndexFid(int index, IList list)
		{
			if (list.Count > 0 && index > 0)
			{
				object record = list[index - 1];
				int num = FindRecord(record);
				if (num == -1)
				{
					return index;
				}
				List<object> list2 = new List<object>(rows.Length) { rows[num].record };
				for (int i = num + 1; i < rows.Length; i++)
				{
					IRow row = rows[i];
					if (list2.Contains(row.fid))
					{
						list2.Add(row.record);
						continue;
					}
					return i;
				}
				return rows.Length;
			}
			return index;
		}
	}

	internal class TempiColumn
	{
		/// <summary>
		/// 表头名称
		/// </summary>
		public string key { get; set; }

		public string? text { get; set; }

		/// <summary>
		/// 列序号
		/// </summary>
		public int i { get; set; }

		public TempiColumn(int index, DataColumn dataColumn)
		{
			i = index;
			key = dataColumn.ColumnName;
			if (!string.IsNullOrEmpty(dataColumn.Caption))
			{
				text = dataColumn.Caption;
			}
		}

		public TempiColumn(int index, string name)
		{
			i = index;
			key = name;
		}
	}

	public class IRow
	{
		/// <summary>
		/// 行序号
		/// </summary>
		public int i { get; set; }

		public int i_virtual { get; set; } = -1;

		/// <summary>
		/// 行原始数据
		/// </summary>
		public object record { get; set; }

		public bool add { get; set; }

		/// <summary>
		/// 父级原始数据
		/// </summary>
		public object? fid { get; set; }

		/// <summary>
		/// 是否包含子数据
		/// </summary>
		public bool has_expand { get; set; }

		/// <summary>
		/// 是否可以展开（默认）
		/// </summary>
		public bool can_expand { get; set; }

		public bool expand { get; set; }

		/// <summary>
		/// 树深度
		/// </summary>
		public int depth { get; set; }

		public Dictionary<string, object?> cells { get; set; }

		public object? this[int index]
		{
			get
			{
				if (cells == null)
				{
					return null;
				}
				int num = 0;
				foreach (KeyValuePair<string, object> cell in cells)
				{
					if (num == index)
					{
						if (cell.Value is PropertyDescriptor propertyDescriptor)
						{
							return propertyDescriptor.GetValue(record);
						}
						if (cell.Value is AntItem antItem)
						{
							return antItem.value;
						}
						return cell.Value;
					}
					num++;
				}
				return null;
			}
		}

		public object? this[string key]
		{
			get
			{
				if (cells == null)
				{
					return null;
				}
				if (cells.TryGetValue(key, out object value))
				{
					if (value is PropertyDescriptor propertyDescriptor)
					{
						return propertyDescriptor.GetValue(record);
					}
					if (value is AntItem antItem)
					{
						return antItem.value;
					}
					return value;
				}
				return null;
			}
		}

		public IRow(int index, object _record, Dictionary<string, object?> _cells, int _depth, object? f)
		{
			i = index;
			record = _record;
			cells = _cells;
			fid = f;
			depth = _depth;
		}

		public IRow(int index, object _record, IList<AntItem> _cells, int _depth, object? f)
		{
			i = index;
			record = _record;
			depth = _depth;
			fid = f;
			cells = new Dictionary<string, object>(_cells.Count);
			foreach (AntItem _cell in _cells)
			{
				cells.Add(_cell.key, _cell);
			}
		}

		public void SetValue(int index, object? value)
		{
			if (cells == null)
			{
				return;
			}
			int num = 0;
			foreach (KeyValuePair<string, object> cell in cells)
			{
				if (num == index)
				{
					cells[cell.Key] = value;
					break;
				}
				num++;
			}
		}

		public bool SetValue(string key, object? value)
		{
			if (cells == null)
			{
				return false;
			}
			if (cells.TryGetValue(key, out object value2))
			{
				if (value2 is PropertyDescriptor propertyDescriptor)
				{
					propertyDescriptor.SetValue(record, value);
				}
				else if (value2 is AntItem antItem)
				{
					antItem.value = value;
				}
				else
				{
					cells[key] = value;
				}
				return true;
			}
			return false;
		}
	}

	private class EditControlTag
	{
		public object? value { get; set; }

		public Action<bool, object?>? call { get; set; }

		public EditControlTag(object? v, Action<bool, object?> c)
		{
			value = v;
			call = c;
		}
	}

	/// <summary>
	/// 选中改变事件
	/// </summary>
	public delegate void CheckEventHandler(object sender, TableCheckEventArgs e);

	/// <summary>
	/// 点击事件
	/// </summary>
	public delegate void ClickEventHandler(object sender, TableClickEventArgs e);

	/// <summary>
	/// 点击之前事件
	/// </summary>
	public delegate void ClickBeginEventHandler(object sender, TableClickBeginEventArgs e);

	/// <summary>
	/// 移动事件
	/// </summary>
	public delegate void HoverEventHandler(object sender, TableHoverEventArgs e);

	/// <summary>
	/// 按钮点击事件
	/// </summary>
	public delegate void ClickButtonEventHandler(object sender, TableButtonEventArgs e);

	public class CheckStateEventArgs : EventArgs
	{
		/// <summary>
		/// 触发表头对象
		/// </summary>
		public ColumnCheck Column { get; private set; }

		/// <summary>
		/// 数值
		/// </summary>
		public CheckState Value { get; private set; }

		public CheckStateEventArgs(ColumnCheck column, CheckState value)
		{
			Column = column;
			Value = value;
		}
	}

	/// <summary>
	/// CheckState类型事件
	/// </summary>
	public delegate void CheckStateEventHandler(object sender, CheckStateEventArgs e);

	/// <summary>
	/// 单元格焦点变更事件
	/// </summary>
	public delegate void CellFocusedEventHandler(object sender, TableCellFocusedEventArgs e);

	/// <summary>
	/// 编辑前事件
	/// </summary>
	public delegate bool BeginEditEventHandler(object sender, TableEventArgs e);

	/// <summary>
	/// 编辑前事件文本框样式
	/// </summary>
	public delegate void BeginEditInputStyleEventHandler(object sender, TableBeginEditInputStyleEventArgs e);

	/// <summary>
	/// 编辑后事件
	/// </summary>
	public delegate bool EndEditEventHandler(object sender, TableEndEditEventArgs e);

	/// <summary>
	/// 编辑后事件
	/// </summary>
	public delegate bool EndValueEditEventHandler(object sender, TableEndValueEditEventArgs e);

	/// <summary>
	/// 编辑后事件
	/// </summary>
	public delegate void EndEditCompleteEventHandler(object sender, ITableEventArgs e);

	/// <summary>
	/// 单元格输入模式下按下回车键时发生
	/// </summary>
	public delegate void CellEditEnterEventHandler(object sender, TableCellEditEnterEventArgs e);

	/// <summary>
	/// 绘制单元格时发生
	/// </summary>
	public delegate void CellPaintEventHandler(object sender, TablePaintEventArgs e);

	public delegate void CellPaintRowEventHandler(object sender, TablePaintRowEventArgs e);

	/// <summary>
	/// 绘制单元格之前发生
	/// </summary>
	public delegate void CellPaintBeginEventHandler(object sender, TablePaintBeginEventArgs e);

	public delegate CellStyleInfo? SetRowStyleEventHandler(object sender, TableSetRowStyleEventArgs e);

	public class CellStyleInfo
	{
		/// <summary>
		/// 背景颜色
		/// </summary>
		public Color? BackColor { get; set; }

		/// <summary>
		/// 文字颜色
		/// </summary>
		public Color? ForeColor { get; set; }

		/// <summary>
		/// 字体
		/// </summary>
		public Font? Font { get; set; }

		/// <summary>
		/// 字体粗体
		/// </summary>
		public bool? FontBold { get; set; }

		public CellStyleInfo SetBackColor(Color? value)
		{
			BackColor = value;
			return this;
		}

		public CellStyleInfo SetForeColor(Color? value)
		{
			ForeColor = value;
			return this;
		}

		public CellStyleInfo SetFont(Font? value)
		{
			Font = value;
			return this;
		}

		public CellStyleInfo SetFontBold(bool? value = true)
		{
			FontBold = value;
			return this;
		}
	}

	public delegate void SortTreeEventHandler(object sender, TableSortTreeEventArgs e);

	public delegate bool SortModeEventHandler(object sender, TableSortModeEventArgs e);

	/// <summary>
	/// 列拖放新位置前事件
	/// </summary>
	public delegate void ColumnIndexChangingEventHandler(object sender, TableColumnIndexChangingEventArgs e);

	/// <summary>
	/// 列拖放新位置后事件
	/// </summary>
	public delegate void ColumnIndexChangedEventHandler(object sender, TableColumnIndexChangedEventArgs e);

	/// <summary>
	/// 展开事件
	/// </summary>
	public delegate void ExpandEventHandler(object sender, TableExpandEventArgs e);

	/// <summary>
	/// Table的列筛选关闭前的处理事件
	/// </summary>
	/// <param name="sender">Table</param>
	/// <param name="e">事件参数</param>
	public delegate void TableFilterPopupEndEventHandler(object sender, TableFilterPopupEndEventArgs e);

	/// <summary>
	/// Table的列筛选弹出前的数据处理事件
	/// </summary>
	/// <param name="sender">Table</param>
	/// <param name="e">事件参数</param>
	public delegate void TableFilterPopupBeginEventHandler(object sender, TableFilterPopupBeginEventArgs e);

	/// <summary>
	/// Table的筛选数据变更事件
	/// </summary>
	/// <param name="sender">Table</param>
	/// <param name="e">事件参数</param>
	public delegate void TableFilterDataChangedEventHandler(object sender, TableFilterDataChangedEventArgs e);

	/// <summary>
	/// 内置汇总功能切换
	/// </summary>
	public delegate void SummaryCustomizeChangedEventHandler(object sender, BoolEventArgs e);

	public class CELLDB
	{
		public CELL cell { get; set; }

		public int x { get; set; }

		public int y { get; set; }

		public int offset_x { get; set; }

		public int offset_xi { get; set; }

		public int offset_y { get; set; }

		public int i_row { get; set; }

		public int i_cel { get; set; }

		public Column col { get; set; }

		public CELLDBMode mode { get; set; }

		public CELLDB(CELL _cell, int _r_x, int _r_y, int _offset_x, int _offset_xi, int _offset_y, int _i_row, int _i_cel, Column _col)
		{
			cell = _cell;
			x = _r_x;
			y = _r_y;
			offset_x = _offset_x;
			offset_xi = _offset_xi;
			offset_y = _offset_y;
			i_row = _i_row;
			i_cel = _i_cel;
			col = _col;
		}
	}

	public enum CELLDBMode
	{
		None,
		/// <summary>
		/// 表头
		/// </summary>
		Column,
		/// <summary>
		/// 浮动表头
		/// </summary>
		ColumnFixed,
		/// <summary>
		/// 总结栏
		/// </summary>
		Summary
	}

	public class NavigationConfig
	{
		/// <summary>
		/// 焦点跳转配置字典 - 定义字段间的跳转顺序
		/// </summary>
		public Dictionary<string, string> Map { get; set; }

		public string FirstFieldKey { get; set; }

		public string LastFieldKey { get; set; }

		public bool SelectAll { get; set; }

		public bool LineBreak { get; set; }

		public NavigationConfig(string[] fieldSequence, bool selectAll, bool lineBreak)
		{
			SelectAll = selectAll;
			LineBreak = lineBreak;
			int num = fieldSequence.Length - 1;
			FirstFieldKey = fieldSequence[0];
			LastFieldKey = fieldSequence[num];
			Map = new Dictionary<string, string>(num);
			for (int i = 0; i < num; i++)
			{
				Map.Add(fieldSequence[i], fieldSequence[i + 1]);
			}
		}

		public bool Contains(string key, out string? next)
		{
			if (Map.TryGetValue(key, out next))
			{
				return true;
			}
			next = null;
			if (!(FirstFieldKey == key))
			{
				return LastFieldKey == key;
			}
			return true;
		}
	}

	private class DownCellTMP<T>
	{
		public T cell { get; set; }

		public RowTemplate row { get; set; }

		public int i_row { get; set; }

		public int i_cel { get; set; }

		public int offset_x { get; set; }

		public int offset_xi { get; set; }

		public int offset_y { get; set; }

		public Column col { get; set; }

		public DownCellTMP(RowTemplate _row, T _cell, CELLDB db)
		{
			row = _row;
			cell = _cell;
			i_row = db.i_row;
			i_cel = db.i_cel;
			offset_x = db.offset_x;
			offset_xi = db.offset_xi;
			offset_y = db.offset_y;
			col = db.col;
		}
	}

	internal class RowList
	{
		private Dictionary<int, RowTemplate> dir_index;

		private Dictionary<int, RowTemplate> dir_index_r;

		private Dictionary<object, RowTemplate> dir_r;

		public RowTemplate[] List { get; }

		public RowTemplate? this[int index]
		{
			get
			{
				if (dir_index.TryGetValue(index, out RowTemplate value))
				{
					return value;
				}
				return null;
			}
		}

		public RowTemplate First => List[0];

		public int HeaderHeight => List[0].Height;

		public int Length => List.Length;

		public RowList(List<RowTemplate> data)
		{
			dir_index = new Dictionary<int, RowTemplate>(data.Count);
			dir_index_r = new Dictionary<int, RowTemplate>(data.Count);
			dir_r = new Dictionary<object, RowTemplate>(data.Count);
			foreach (RowTemplate datum in data)
			{
				dir_index.Add(datum.INDEX, datum);
				if (!dir_r.ContainsKey(datum.RECORD))
				{
					dir_r.Add(datum.RECORD, datum);
				}
				if (!dir_index_r.ContainsKey(datum.INDEX_REAL))
				{
					dir_index_r.Add(datum.INDEX_REAL, datum);
				}
			}
			List = data.ToArray();
		}

		public RowTemplate? Real(int index)
		{
			if (dir_index_r.TryGetValue(index, out RowTemplate value))
			{
				return value;
			}
			return null;
		}

		public RowTemplate? Record(object record)
		{
			if (dir_r.TryGetValue(record, out RowTemplate value))
			{
				return value;
			}
			return null;
		}

		public Dictionary<int, object> RealDir()
		{
			Dictionary<int, object> dictionary = new Dictionary<int, object>(List.Length);
			RowTemplate[] list = List;
			foreach (RowTemplate rowTemplate in list)
			{
				if (rowTemplate.INDEX_REAL > -1)
				{
					dictionary.Add(rowTemplate.INDEX_REAL, rowTemplate.RECORD);
				}
			}
			return dictionary;
		}

		public void SetSelect(bool value)
		{
			RowTemplate[] list = List;
			for (int i = 0; i < list.Length; i++)
			{
				list[i].Select = value;
			}
		}

		public void SetHover(int row)
		{
			for (int i = 0; i < List.Length; i++)
			{
				List[i].hover = i == row;
			}
		}

		public void HoverLeave()
		{
			RowTemplate[] list = List;
			foreach (RowTemplate row in list)
			{
				HoverLeave(row);
			}
		}

		public int HoverLeave(CELLDB db)
		{
			int num = 0;
			for (int i = 1; i < List.Length; i++)
			{
				if (List[i].INDEX == db.i_row)
				{
					if (db.cell is TCellSort tCellSort)
					{
						tCellSort.Hover = tCellSort.Contains(db.x, db.y);
						if (tCellSort.Hover)
						{
							num++;
						}
					}
					List[i].Hover = true;
				}
				else
				{
					HoverLeave(List[i]);
				}
			}
			return num;
		}

		private void HoverLeave(RowTemplate row)
		{
			row.Hover = false;
			CELL[] cells = row.cells;
			foreach (CELL cELL in cells)
			{
				if (cELL is TCellSort tCellSort)
				{
					tCellSort.Hover = false;
				}
				else if (cELL is Template template)
				{
					ILeave(template);
				}
			}
		}

		private void ILeave(Template template)
		{
			foreach (ICell item in template.Value)
			{
				if (item is CellLink cellLink)
				{
					cellLink.ExtraMouseHover = false;
				}
				else if (item is CellCheckbox cellCheckbox)
				{
					cellCheckbox.ExtraMouseHover = false;
				}
				else if (item is CellRadio cellRadio)
				{
					cellRadio.ExtraMouseHover = false;
				}
				else if (item is CellSwitch cellSwitch)
				{
					cellSwitch.ExtraMouseHover = false;
				}
			}
		}
	}

	/// <summary>
	/// 行数据
	/// </summary>
	internal class RowTemplate : IROW
	{
		private Table PARENT;

		internal bool hover;

		internal float AnimationHoverValue;

		internal bool AnimationHover;

		internal AnimationTask? ThreadHover;

		/// <summary>
		/// 内部判断脏渲染
		/// </summary>
		internal bool SHOW { get; set; }

		/// <summary>
		/// 行区域
		/// </summary>
		public Rectangle RECT { get; set; }

		/// <summary>
		/// 原始行数据
		/// </summary>
		public object RECORD { get; set; }

		/// <summary>
		/// 使能
		/// </summary>
		public bool ENABLE { get; set; } = true;

		public int INDEX { get; set; }

		public int INDEX_REAL { get; set; }

		/// <summary>
		/// 列数据
		/// </summary>
		public CELL[] cells { get; set; }

		/// <summary>
		/// 行高度
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// 表类型
		/// </summary>
		public RowType Type { get; set; }

		public bool IsColumn => Type == RowType.Column;

		public bool IsOther
		{
			get
			{
				if (Type != RowType.None)
				{
					return Type == RowType.Summary;
				}
				return true;
			}
		}

		public bool IsSummary => Type == RowType.Summary;

		/// <summary>
		/// 是否移动
		/// </summary>
		internal bool Hover
		{
			get
			{
				return hover;
			}
			set
			{
				if (hover == value)
				{
					return;
				}
				hover = value;
				if (!SHOW || (PARENT.RowHoverBg ?? Colour.FillSecondary.Get(PARENT.ColorScheme, "Table", PARENT.Name)).A <= 0)
				{
					return;
				}
				if (Config.HasAnimation("Table") && PARENT.AnimationTime > 0)
				{
					ThreadHover?.Dispose();
					AnimationHover = true;
					ThreadHover = new AnimationTask(new AnimationFixedConfig((float i) =>
					{
						AnimationHoverValue = i;
						PARENT.Invalidate();
					}, 20, Animation.TotalFrames(20, PARENT.AnimationTime), value).SetEnd(() =>
					{
						AnimationHover = false;
					}));
				}
				else
				{
					PARENT.Invalidate();
				}
			}
		}

		public bool Select { get; set; }

		public bool CanExpand { get; set; }

		public bool Expand { get; set; }

		public int Depth { get; set; }

		public IRow? RD { get; set; }

		internal int INDEX_REAL_KEY { get; set; }

		internal Rectangle RectExpand { get; set; }

		public RowTemplate(Table table, CELL[] cell, int i, object record)
		{
			PARENT = table;
			cells = cell;
			RECORD = record;
			INDEX_REAL = i;
		}

		public RowTemplate(Table table, CELL[] cell, IRow data)
		{
			PARENT = table;
			cells = cell;
			RD = data;
			RECORD = data.record;
			INDEX_REAL = data.i;
			Depth = data.depth;
			CanExpand = data.can_expand;
			Expand = data.expand;
		}

		internal bool Contains(int x, int y, bool sethover)
		{
			if (ENABLE)
			{
				if (sethover)
				{
					if (CONTAINS(x, y))
					{
						Hover = true;
						return true;
					}
					Hover = false;
					return false;
				}
				return CONTAINS(x, y);
			}
			return false;
		}

		internal bool CONTAINS(int x, int y)
		{
			if (ENABLE)
			{
				return RECT.Contains(x, y);
			}
			return false;
		}
	}

	public interface IROW
	{
		/// <summary>
		/// 行区域
		/// </summary>
		Rectangle RECT { get; }

		/// <summary>
		/// 原始行数据
		/// </summary>
		object RECORD { get; }

		/// <summary>
		/// 使能
		/// </summary>
		bool ENABLE { get; }

		int INDEX { get; }

		int INDEX_REAL { get; }

		/// <summary>
		/// 列数据
		/// </summary>
		CELL[] cells { get; }

		/// <summary>
		/// 行高度
		/// </summary>
		int Height { get; }

		/// <summary>
		/// 表类型
		/// </summary>
		RowType Type { get; }

		bool IsColumn { get; }

		bool IsOther { get; }

		bool CanExpand { get; }

		bool Expand { get; }
	}

	/// <summary>
	/// 复选框
	/// </summary>
	private class TCellCheck : CELL_CHECK
	{
		public bool AnimationCheck;

		public float AnimationCheckValue;

		private AnimationTask? ThreadCheck;

		private bool _checked;

		[Description("选中状态")]
		[Category("Behavior")]
		[DefaultValue(false)]
		public override bool Checked
		{
			get
			{
				return _checked;
			}
			set
			{
				if (_checked != value)
				{
					_checked = value;
					OnCheck();
				}
			}
		}

		public bool NoTitle { get; set; }

		/// <summary>
		/// 复选框
		/// </summary>
		/// <param name="table">表格</param>
		/// <param name="column">表头</param>
		/// <param name="prop">反射</param>
		/// <param name="ov">行数据</param>
		/// <param name="value">值</param>
		public TCellCheck(Table table, ColumnCheck column, PropertyDescriptor? prop, object? ov, bool value, bool valint)
			: base(table, column, prop, ov, valint)
		{
			_checked = value;
			AnimationCheckValue = (_checked ? 1f : 0f);
			NoTitle = column.NoTitle;
			AutoCheck = column.AutoCheck;
		}

		private void OnCheck()
		{
			ThreadCheck?.Dispose();
			if (!ROW.SHOW || !PARENT.IsHandleCreated)
			{
				return;
			}
			if (Config.HasAnimation("Table"))
			{
				AnimationCheck = true;
				ThreadCheck = new AnimationTask(new AnimationLinearFConfig(PARENT, (float i) =>
				{
					AnimationCheckValue = i;
					PARENT.Invalidate();
					return true;
				}, 20).SetValue(AnimationCheckValue, _checked, 0.2f).SetEnd(() =>
				{
					AnimationCheck = false;
				}));
			}
			else
			{
				AnimationCheckValue = (_checked ? 1f : 0f);
				PARENT.Invalidate();
			}
		}

		public override void SetSize(Canvas g, Font font, Size font_size, Rectangle _rect, int ox, TableGaps gap)
		{
		}

		public void SetSize(Rectangle _rect, int check_size)
		{
			RECT = _rect;
			RECT_REAL = new Rectangle(_rect.X + (_rect.Width - check_size) / 2, _rect.Y + (_rect.Height - check_size) / 2, check_size, check_size);
		}

		public override Size GetSize(Canvas g, Font font, Size font_size, int width, TableGaps gap)
		{
			MinWidth = font_size.Width;
			return font_size;
		}

		public void Print(Canvas g, TAMode colorScheme, Font font, SolidBrush fore, bool enable)
		{
			using GraphicsPath path = RECT_REAL.RoundPath(PARENT.check_radius);
			if (enable)
			{
				if (AnimationCheck)
				{
					g.Fill(Colour.BgBase.Get(colorScheme, "Checkbox", PARENT.Name), path);
					float alpha = 255f * AnimationCheckValue;
					g.Fill(Helper.ToColor(alpha, Colour.Primary.Get(colorScheme, "Checkbox", PARENT.Name)), path);
					using (Pen pen = new Pen(Helper.ToColor(alpha, Colour.BgBase.Get(colorScheme, "Checkbox", PARENT.Name)), PARENT.check_border * 2f))
					{
						g.DrawLines(pen, PaintArrow(RECT_REAL));
					}
					if (Checked)
					{
						float num = (float)RECT_REAL.Height + (float)RECT_REAL.Height * AnimationCheckValue;
						using SolidBrush brush = new SolidBrush(Helper.ToColor(100f * (1f - AnimationCheckValue), Colour.Primary.Get(colorScheme, "Checkbox", PARENT.Name)));
						g.FillEllipse(brush, new RectangleF((float)RECT_REAL.X + ((float)RECT_REAL.Width - num) / 2f, (float)RECT_REAL.Y + ((float)RECT_REAL.Height - num) / 2f, num, num));
					}
					g.Draw(Colour.Primary.Get(colorScheme, "Checkbox", PARENT.Name), PARENT.check_border, path);
					return;
				}
				if (Checked)
				{
					g.Fill(Colour.Primary.Get(colorScheme, "Checkbox", PARENT.Name), path);
					using Pen pen2 = new Pen(Colour.BgBase.Get(colorScheme, "Checkbox", PARENT.Name), PARENT.check_border * 2f);
					g.DrawLines(pen2, PaintArrow(RECT_REAL));
					return;
				}
				g.Fill(Colour.BgBase.Get(colorScheme, "Checkbox", PARENT.Name), path);
				g.Draw(Colour.BorderColor.Get(colorScheme, "Checkbox", PARENT.Name), PARENT.check_border, path);
			}
			else
			{
				g.Fill(Colour.FillQuaternary.Get(colorScheme, "Checkbox", PARENT.Name), path);
				if (Checked)
				{
					g.DrawLines(Colour.TextQuaternary.Get(colorScheme, "Checkbox", PARENT.Name), PARENT.check_border * 2f, PaintArrow(RECT_REAL));
				}
				g.Draw(Colour.BorderColorDisable.Get(colorScheme, "Checkbox", PARENT.Name), PARENT.check_border, path);
			}
		}

		public override string ToString()
		{
			return Checked.ToString();
		}
	}

	/// <summary>
	/// 单选框
	/// </summary>
	private class TCellRadio : CELL_CHECK
	{
		public bool AnimationCheck;

		public float AnimationCheckValue;

		private AnimationTask? ThreadCheck;

		private bool _checked;

		[Description("选中状态")]
		[Category("Behavior")]
		[DefaultValue(false)]
		public override bool Checked
		{
			get
			{
				return _checked;
			}
			set
			{
				if (_checked != value)
				{
					_checked = value;
					OnCheck();
				}
			}
		}

		/// <summary>
		/// 单选框
		/// </summary>
		/// <param name="table">表格</param>
		/// <param name="column">表头</param>
		/// <param name="prop">反射</param>
		/// <param name="ov">行数据</param>
		/// <param name="value">值</param>
		public TCellRadio(Table table, ColumnRadio column, PropertyDescriptor? prop, object? ov, bool value, bool valint)
			: base(table, column, prop, ov, valint)
		{
			_checked = value;
			AnimationCheckValue = (_checked ? 1f : 0f);
			AutoCheck = column.AutoCheck;
		}

		private void OnCheck()
		{
			ThreadCheck?.Dispose();
			if (!ROW.SHOW || !PARENT.IsHandleCreated)
			{
				return;
			}
			if (Config.HasAnimation("Table"))
			{
				AnimationCheck = true;
				ThreadCheck = new AnimationTask(new AnimationLinearFConfig(PARENT, (float i) =>
				{
					AnimationCheckValue = i;
					PARENT.Invalidate();
					return true;
				}, 20).SetValue(AnimationCheckValue, _checked, 0.2f).SetEnd(() =>
				{
					AnimationCheck = false;
				}));
			}
			else
			{
				AnimationCheckValue = (_checked ? 1f : 0f);
				PARENT.Invalidate();
			}
		}

		public override void SetSize(Canvas g, Font font, Size font_size, Rectangle _rect, int ox, TableGaps gap)
		{
		}

		public void SetSize(Rectangle _rect, int check_size)
		{
			RECT = _rect;
			RECT_REAL = new Rectangle(_rect.X + (_rect.Width - check_size) / 2, _rect.Y + (_rect.Height - check_size) / 2, check_size, check_size);
		}

		public override Size GetSize(Canvas g, Font font, Size font_size, int width, TableGaps gap)
		{
			MinWidth = font_size.Width;
			return font_size;
		}

		public void Print(Canvas g, TAMode colorScheme, Font font, SolidBrush fore, bool enable)
		{
			int height = RECT_REAL.Height;
			if (enable)
			{
				g.FillEllipse(Colour.BgBase.Get(colorScheme, "Radio", PARENT.Name), RECT_REAL);
				if (AnimationCheck)
				{
					float num = (float)height * 0.3f;
					using (GraphicsPath graphicsPath = new GraphicsPath())
					{
						float num2 = (float)height - num * AnimationCheckValue;
						float num3 = num2 / 2f;
						float alpha = 255f * AnimationCheckValue;
						graphicsPath.AddEllipse(RECT_REAL);
						graphicsPath.AddEllipse(new RectangleF((float)RECT_REAL.X + num3, (float)RECT_REAL.Y + num3, (float)RECT_REAL.Width - num2, (float)RECT_REAL.Height - num2));
						g.Fill(Helper.ToColor(alpha, Colour.Primary.Get(colorScheme, "Radio", PARENT.Name)), graphicsPath);
					}
					if (Checked)
					{
						float num4 = (float)RECT_REAL.Height + (float)RECT_REAL.Height * AnimationCheckValue;
						float alpha2 = 100f * (1f - AnimationCheckValue);
						g.FillEllipse(Helper.ToColor(alpha2, Colour.Primary.Get(colorScheme, "Radio", PARENT.Name)), new RectangleF((float)RECT_REAL.X + ((float)RECT_REAL.Width - num4) / 2f, (float)RECT_REAL.Y + ((float)RECT_REAL.Height - num4) / 2f, num4, num4));
					}
					g.DrawEllipse(Colour.Primary.Get(colorScheme, "Radio", PARENT.Name), PARENT.check_border, RECT_REAL);
				}
				else if (Checked)
				{
					float num5 = (float)height * 0.3f;
					float num6 = num5 / 2f;
					g.DrawEllipse(Color.FromArgb(250, Colour.Primary.Get(colorScheme, "Radio", PARENT.Name)), num5, new RectangleF((float)RECT_REAL.X + num6, (float)RECT_REAL.Y + num6, (float)RECT_REAL.Width - num5, (float)RECT_REAL.Height - num5));
					g.DrawEllipse(Colour.Primary.Get(colorScheme, "Radio", PARENT.Name), PARENT.check_border, RECT_REAL);
				}
				else
				{
					g.DrawEllipse(Colour.BorderColor.Get(colorScheme, "Radio", PARENT.Name), PARENT.check_border, RECT_REAL);
				}
			}
			else
			{
				g.FillEllipse(Colour.FillQuaternary.Get(colorScheme, "Radio", PARENT.Name), RECT_REAL);
				if (Checked)
				{
					float num7 = (float)height / 2f;
					float num8 = num7 / 2f;
					g.FillEllipse(Colour.TextQuaternary.Get(colorScheme, "Radio", PARENT.Name), new RectangleF((float)RECT_REAL.X + num8, (float)RECT_REAL.Y + num8, (float)RECT_REAL.Width - num7, (float)RECT_REAL.Height - num7));
				}
				g.DrawEllipse(Colour.BorderColorDisable.Get(colorScheme, "Radio", PARENT.Name), PARENT.check_border, RECT_REAL);
			}
		}

		public override string ToString()
		{
			return Checked.ToString();
		}
	}

	/// <summary>
	/// 开关
	/// </summary>
	private class TCellSwitch : CELL_CHECK
	{
		internal bool AnimationCheck;

		internal float AnimationCheckValue;

		private AnimationTask? ThreadCheck;

		private bool _checked;

		private AnimationTask? ThreadHover;

		internal float AnimationHoverValue;

		internal bool AnimationHover;

		internal bool _mouseHover;

		private bool loading;

		private AnimationTask? ThreadLoading;

		internal float LineWidth = 6f;

		internal float LineAngle;

		[Description("选中状态")]
		[Category("Behavior")]
		[DefaultValue(false)]
		public override bool Checked
		{
			get
			{
				return _checked;
			}
			set
			{
				if (_checked != value)
				{
					_checked = value;
					OnCheck();
				}
			}
		}

		internal bool ExtraMouseHover
		{
			get
			{
				return _mouseHover;
			}
			set
			{
				if (_mouseHover == value)
				{
					return;
				}
				_mouseHover = value;
				if (ROW.SHOW && PARENT.IsHandleCreated && Config.HasAnimation("Table"))
				{
					ThreadHover?.Dispose();
					AnimationHover = true;
					ThreadHover = new AnimationTask(new AnimationLinearFConfig(PARENT, (float i) =>
					{
						AnimationHoverValue = i;
						PARENT.Invalidate();
						return true;
					}, 10).SetValue(AnimationHoverValue, value, 0.1f).SetEnd(() =>
					{
						AnimationHover = false;
						PARENT.Invalidate();
					}));
				}
				else
				{
					AnimationHoverValue = 255f;
				}
				PARENT.Invalidate();
			}
		}

		public bool Loading
		{
			get
			{
				return loading;
			}
			set
			{
				if (loading == value)
				{
					return;
				}
				loading = value;
				if (ROW.SHOW && PARENT.IsHandleCreated)
				{
					if (loading)
					{
						bool ProgState = false;
						ThreadLoading = new AnimationTask(new AnimationLoopConfig(PARENT, () =>
						{
							if (ProgState)
							{
								LineAngle = LineAngle.Calculate(9f);
								LineWidth = LineWidth.Calculate(0.6f);
								if (LineWidth > 75f)
								{
									ProgState = false;
								}
							}
							else
							{
								LineAngle = LineAngle.Calculate(9.6f);
								LineWidth = LineWidth.Calculate(-0.6f);
								if (LineWidth < 6f)
								{
									ProgState = true;
								}
							}
							if (LineAngle >= 360f)
							{
								LineAngle = 0f;
							}
							PARENT.Invalidate();
							return true;
						}, 10).SetPriority());
					}
					else
					{
						ThreadLoading?.Dispose();
					}
				}
				PARENT.Invalidate();
			}
		}

		/// <summary>
		/// 开关
		/// </summary>
		/// <param name="table">表格</param>
		/// <param name="column">表头</param>
		/// <param name="prop">反射</param>
		/// <param name="ov">行数据</param>
		/// <param name="value">值</param>
		public TCellSwitch(Table table, ColumnSwitch column, PropertyDescriptor? prop, object? ov, bool value, bool valint)
			: base(table, column, prop, ov, valint)
		{
			_checked = value;
			AnimationCheckValue = (_checked ? 1f : 0f);
			AutoCheck = column.AutoCheck;
		}

		private void OnCheck()
		{
			ThreadCheck?.Dispose();
			if (!ROW.SHOW || !PARENT.IsHandleCreated)
			{
				return;
			}
			if (Config.HasAnimation("Table"))
			{
				AnimationCheck = true;
				ThreadCheck = new AnimationTask(new AnimationLinearFConfig(PARENT, (float i) =>
				{
					AnimationCheckValue = i;
					PARENT.Invalidate();
					return true;
				}, 10).SetValue(AnimationCheckValue, _checked, 0.1f).SetEnd(() =>
				{
					AnimationCheck = false;
				}));
			}
			else
			{
				AnimationCheckValue = (_checked ? 1f : 0f);
				PARENT.Invalidate();
			}
		}

		public override void SetSize(Canvas g, Font font, Size font_size, Rectangle _rect, int ox, TableGaps gap)
		{
		}

		public void SetSize(Rectangle _rect, int check_size)
		{
			int num = check_size * 2;
			RECT = _rect;
			RECT_REAL = new Rectangle(_rect.X + (_rect.Width - num) / 2, _rect.Y + (_rect.Height - check_size) / 2, num, check_size);
		}

		public override Size GetSize(Canvas g, Font font, Size font_size, int width, TableGaps gap)
		{
			MinWidth = font_size.Width;
			return font_size;
		}

		public void Print(Canvas g, TAMode colorScheme, Font font, SolidBrush fore, bool enable)
		{
			string name = PARENT.Name;
			Color color = Colour.Primary.Get(colorScheme, "Switch", name);
			using GraphicsPath path = RECT_REAL.RoundPath(RECT_REAL.Height);
			using (SolidBrush solidBrush = new SolidBrush(Colour.TextQuaternary.Get(colorScheme, "Switch", name)))
			{
				g.Fill(solidBrush, path);
				if (AnimationHover)
				{
					g.Fill(Helper.ToColorN(AnimationHoverValue, solidBrush.Color), path);
				}
				else if (ExtraMouseHover)
				{
					g.Fill(solidBrush, path);
				}
			}
			int num = (int)(2f * g.Dpi);
			int num2 = num * 2;
			if (AnimationCheck)
			{
				float alpha = 255f * AnimationCheckValue;
				g.Fill(Helper.ToColor(alpha, color), path);
				RectangleF rectangleF = new RectangleF((float)(RECT_REAL.X + num) + (float)(RECT_REAL.Width - RECT_REAL.Height) * AnimationCheckValue, RECT_REAL.Y + num, RECT_REAL.Height - num2, RECT_REAL.Height - num2);
				g.FillEllipse(enable ? Colour.SwitchHandleBg.Get(colorScheme, "Switch", name) : Helper.ToColorN(0.65f, Colour.SwitchHandleBg.Get(colorScheme, "Switch", name)), rectangleF);
				if (loading)
				{
					PaintLoading(g, RECT_REAL, rectangleF, num, num2, color);
				}
			}
			else if (Checked)
			{
				if (enable)
				{
					PaintChecked(g, colorScheme, name, RECT_REAL, path, num, num2, Colour.SwitchHandleBg.Get(colorScheme, "Switch", name), color);
				}
				else
				{
					PaintChecked(g, colorScheme, name, RECT_REAL, path, num, num2, Helper.ToColorN(0.8f, Colour.SwitchHandleBg.Get(colorScheme, "Switch", name)), Helper.ToColorN(0.65f, color));
				}
			}
			else if (enable)
			{
				PaintUnChecked(g, colorScheme, name, RECT_REAL, num, num2, Colour.SwitchHandleBg.Get(colorScheme, "Switch", name), color);
			}
			else
			{
				PaintUnChecked(g, colorScheme, name, RECT_REAL, num, num2, Helper.ToColorN(0.8f, Colour.SwitchHandleBg.Get(colorScheme, "Switch", name)), Helper.ToColorN(0.65f, color));
			}
		}

		private void PaintChecked(Canvas g, TAMode colorScheme, string name, Rectangle rect, GraphicsPath path, int gap, int gap2, Color handBg, Color color)
		{
			Color color2 = Colour.PrimaryHover.Get(colorScheme, "Switch", name);
			g.Fill(color, path);
			if (AnimationHover)
			{
				g.Fill(Helper.ToColorN(AnimationHoverValue, color2), path);
			}
			else if (ExtraMouseHover)
			{
				g.Fill(color2, path);
			}
			RectangleF rectangleF = new RectangleF(rect.X + gap + rect.Width - rect.Height, rect.Y + gap, rect.Height - gap2, rect.Height - gap2);
			g.FillEllipse(handBg, rectangleF);
			if (loading)
			{
				PaintLoading(g, rect, rectangleF, gap, gap2, color);
			}
		}

		private void PaintUnChecked(Canvas g, TAMode colorScheme, string name, Rectangle rect, int gap, int gap2, Color handBg, Color color)
		{
			RectangleF rectangleF = new RectangleF(rect.X + gap, rect.Y + gap, rect.Height - gap2, rect.Height - gap2);
			g.FillEllipse(handBg, rectangleF);
			if (loading)
			{
				PaintLoading(g, rect, rectangleF, gap, gap2, color);
			}
		}

		private void PaintLoading(Canvas g, Rectangle rect, RectangleF dot_rect, int gap, int gap2, Color color)
		{
			RectangleF rect2 = new RectangleF(dot_rect.X + (float)gap, dot_rect.Y + (float)gap, dot_rect.Height - (float)gap2, dot_rect.Height - (float)gap2);
			float width = (float)rect.Height * 0.1f;
			using Pen pen = new Pen(color, width);
			LineCap startCap = (pen.EndCap = LineCap.Round);
			pen.StartCap = startCap;
			g.DrawArc(pen, rect2, LineAngle, LineWidth * 3.6f);
		}

		public override string ToString()
		{
			return Checked.ToString();
		}
	}

	private abstract class CELL_CHECK : CELL
	{
		public abstract bool Checked { get; set; }

		public bool AutoCheck { get; set; }

		public bool ValInt { get; set; }

		public CELL_CHECK(Table table, Column column, PropertyDescriptor? prop, object? ov, bool val_int)
			: base(table, column, prop, ov)
		{
			ValInt = val_int;
		}
	}

	/// <summary>
	/// 拖拽手柄
	/// </summary>
	private class TCellSort : CELL
	{
		private bool hover;

		private float AnimationHoverValue;

		private bool AnimationHover;

		private AnimationTask? ThreadHover;

		public Rectangle rect_ico;

		/// <summary>
		/// 是否移动
		/// </summary>
		public bool Hover
		{
			get
			{
				return hover;
			}
			set
			{
				if (hover == value)
				{
					return;
				}
				hover = value;
				if (Config.HasAnimation("Table") && PARENT.AnimationTime > 0)
				{
					ThreadHover?.Dispose();
					AnimationHover = true;
					ThreadHover = new AnimationTask(new AnimationFixedConfig((float i) =>
					{
						AnimationHoverValue = i;
						PARENT.Invalidate();
					}, 20, Animation.TotalFrames(20, PARENT.AnimationTime), value).SetEnd(() =>
					{
						AnimationHover = false;
					}));
				}
				else
				{
					PARENT.Invalidate();
				}
			}
		}

		/// <summary>
		/// 拖拽手柄
		/// </summary>
		/// <param name="table">表格</param>
		/// <param name="column">表头</param>
		public TCellSort(Table table, ColumnSort column)
			: base(table, column, null, null)
		{
		}

		public bool Contains(int x, int y)
		{
			if (RECT_REAL.Contains(x, y))
			{
				Hover = true;
				return true;
			}
			Hover = false;
			return false;
		}

		public override void SetSize(Canvas g, Font font, Size font_size, Rectangle _rect, int ox, TableGaps gap)
		{
		}

		public void SetSize(Rectangle _rect, int sort_size, int sort_ico_size)
		{
			RECT = _rect;
			RECT_REAL = new Rectangle(_rect.X + (_rect.Width - sort_size) / 2, _rect.Y + (_rect.Height - sort_size) / 2, sort_size, sort_size);
			rect_ico = new Rectangle(_rect.X + (_rect.Width - sort_ico_size) / 2, _rect.Y + (_rect.Height - sort_ico_size) / 2, sort_ico_size, sort_ico_size);
		}

		public override Size GetSize(Canvas g, Font font, Size font_size, int width, TableGaps gap)
		{
			MinWidth = font_size.Width;
			return font_size;
		}

		public void Print(Canvas g, TAMode colorScheme, Font font, SolidBrush fore, bool enable)
		{
			if (AnimationHover)
			{
				using SolidBrush brush = new SolidBrush(Helper.ToColorN(AnimationHoverValue, Colour.FillTertiary.Get(colorScheme, "Table", PARENT.Name)));
				using GraphicsPath path = RECT_REAL.RoundPath(PARENT.check_radius);
				g.Fill(brush, path);
			}
			else if (Hover)
			{
				using GraphicsPath path2 = RECT_REAL.RoundPath(PARENT.check_radius);
				g.Fill(Colour.FillTertiary.Get(colorScheme, "Table", PARENT.Name), path2);
			}
			g.Svg(SvgDb.IcoTableColumnSort, rect_ico, fore.Color);
		}
	}

	/// <summary>
	/// 普通文本
	/// </summary>
	private class TCellText : CELL
	{
		/// <summary>
		/// 值
		/// </summary>
		public string? value { get; set; }

		/// <summary>
		/// 普通文本
		/// </summary>
		/// <param name="table">表格</param>
		/// <param name="column">表头</param>
		/// <param name="prop">反射</param>
		/// <param name="ov">行数据</param>
		/// <param name="txt">文本</param>
		public TCellText(Table table, Column column, PropertyDescriptor? prop, object? ov, object? txt)
			: base(table, column, prop, ov)
		{
			value = column.GetDisplayText(txt);
		}

		public void SetValue(object? txt)
		{
			value = COLUMN.GetDisplayText(txt);
		}

		public override void SetSize(Canvas g, Font font, Size font_size, Rectangle _rect, int ox, TableGaps gap)
		{
			RECT = _rect;
			RECT_REAL = new Rectangle(_rect.X + gap.x + ox, _rect.Y + gap.y, _rect.Width - gap.x2, _rect.Height - gap.y2);
		}

		public override Size GetSize(Canvas g, Font font, Size font_size, int width, TableGaps gap)
		{
			if (COLUMN.LineBreak)
			{
				if (COLUMN.Width != null)
				{
					if (PARENT.tmpcol_width.TryGetValue(INDEX, out var num))
					{
						Size size = g.MeasureText(value, font, num - gap.x2, FormatFlags.Center);
						MinWidth = size.Width;
						return new Size(size.Width + gap.x2, size.Height);
					}
					if (COLUMN.Width.EndsWith("%") && float.TryParse(COLUMN.Width.TrimEnd('%'), out var result))
					{
						Size size2 = g.MeasureText(value, font, (int)Math.Ceiling((float)width * (result / 100f)) - gap.x2, FormatFlags.Center);
						MinWidth = size2.Width;
						return new Size(size2.Width + gap.x2, size2.Height);
					}
					if (int.TryParse(COLUMN.Width, out var result2))
					{
						Size size3 = g.MeasureText(value, font, (int)Math.Ceiling((float)result2 * g.Dpi) - gap.x2, FormatFlags.Center);
						MinWidth = size3.Width;
						return new Size(size3.Width + gap.x2, size3.Height);
					}
					Size size4 = g.MeasureText(value, font, width - gap.x2, FormatFlags.Center);
					MinWidth = size4.Width;
					return new Size(size4.Width + gap.x2, size4.Height);
				}
				Size size5 = g.MeasureText(value, font, 0, FormatFlags.Center);
				MinWidth = size5.Width;
				return new Size(size5.Width + gap.x2, size5.Height);
			}
			Size size6 = g.MeasureText(value, font);
			MinWidth = size6.Width;
			return new Size(size6.Width + gap.x2, size6.Height);
		}

		public void Print(Canvas g, TAMode colorScheme, Font font, SolidBrush fore, bool enable)
		{
			g.DrawText(value, font, fore, RECT_REAL, StringFormat(COLUMN));
		}

		public override string? ToString()
		{
			return value;
		}
	}

	/// <summary>
	/// 图标文本
	/// </summary>
	private class TCellSelect : CELL
	{
		private Rectangle rect_icon;

		private Rectangle rect_text;

		public SelectItem? value { get; set; }

		public new ColumnSelect COLUMN { get; private set; }

		/// <summary>
		/// 普通文本
		/// </summary>
		/// <param name="table">表格</param>
		/// <param name="column">表头</param>
		/// <param name="prop">反射</param>
		/// <param name="tag">行数据</param>
		public TCellSelect(Table table, ColumnSelect column, PropertyDescriptor? prop, object? ov, object? tag)
			: base(table, column, prop, ov)
		{
			COLUMN = column;
			foreach (SelectItem item in column.Items)
			{
				if (item.Tag == tag || item.Tag.Equals(tag))
				{
					value = item;
					break;
				}
			}
		}

		private int GetIconSize(int height, int gap)
		{
			if (value == null)
			{
				return 0;
			}
			if (COLUMN.CellType == SelectCellType.Text || (value.Icon == null && value.IconSvg == null))
			{
				return 0;
			}
			return COLUMN.IconMaxSize ?? ((int)((float)(height - gap) * (value.IconRatio ?? 0.75f)));
		}

		public override void SetSize(Canvas g, Font font, Size font_size, Rectangle _rect, int ox, TableGaps gap)
		{
			if (value == null)
			{
				return;
			}
			RECT = _rect;
			RECT_REAL = new Rectangle(_rect.X + gap.x + ox, _rect.Y + gap.y, _rect.Width - gap.x2, _rect.Height - gap.y2);
			if (COLUMN.CellType == SelectCellType.Text || (value.Icon == null && value.IconSvg == null))
			{
				rect_text = RECT_REAL;
				return;
			}
			bool flag = COLUMN.CellType == SelectCellType.Icon || string.IsNullOrEmpty(value.Text);
			int num = gap.x / 2;
			int iconSize = GetIconSize(_rect.Height, gap.x);
			rect_icon = new Rectangle(_rect.X + (flag ? ((_rect.Width - iconSize) / 2) : gap.x), _rect.Y + (_rect.Height - iconSize) / 2, iconSize, iconSize);
			if (COLUMN.CellType != SelectCellType.Text)
			{
				rect_text = new Rectangle(rect_icon.X + num + iconSize, rect_icon.Y, RECT_REAL.Width - rect_icon.Width + num, rect_icon.Height);
			}
		}

		public override Size GetSize(Canvas g, Font font, Size font_size, int width, TableGaps gap)
		{
			if (value == null || string.IsNullOrEmpty(value.Text))
			{
				return RECT.Size;
			}
			int num = gap.x2;
			if (value.Icon != null || value.IconSvg != null)
			{
				num += RECT.Height - gap.x;
			}
			if (COLUMN.LineBreak && COLUMN.Width != null)
			{
				if (PARENT.tmpcol_width.TryGetValue(INDEX, out var num2))
				{
					int num3 = gap.x * 2;
					int iconSize = GetIconSize(RECT.Height, gap.x);
					Size size = g.MeasureText(value.Text, font, num2 - num);
					MinWidth = size.Width + iconSize;
					return new Size(size.Width + iconSize + num + num3, size.Height);
				}
				if (COLUMN.Width.EndsWith("%") && float.TryParse(COLUMN.Width.TrimEnd('%'), out var result))
				{
					Size size2 = g.MeasureText(value.Text, font, (int)Math.Ceiling((float)width * (result / 100f)) - num);
					MinWidth = size2.Width;
					return new Size(size2.Width + num, size2.Height);
				}
				if (int.TryParse(COLUMN.Width, out var result2))
				{
					Size size3 = g.MeasureText(value.Text, font, (int)Math.Ceiling((float)result2 * g.Dpi) - num);
					MinWidth = size3.Width;
					return new Size(size3.Width + num, size3.Height);
				}
			}
			Size size4 = g.MeasureText(value.Text, font);
			int num4 = gap.x * 2;
			int iconSize2 = GetIconSize(PARENT.RowHeight ?? (size4.Height * 2), gap.x);
			MinWidth = size4.Width + iconSize2;
			return new Size(size4.Width + iconSize2 + num + num4, size4.Height);
		}

		public void Print(Canvas g, TAMode colorScheme, Font font, SolidBrush fore, bool enable)
		{
			if (value != null)
			{
				Color color = value.TagFore ?? PARENT.ForeColor ?? Colour.Text.Get(colorScheme, "Select", PARENT.Name);
				if (value.IconSvg != null)
				{
					g.Svg(value.IconSvg, rect_icon, color);
				}
				else if (value.Icon != null)
				{
					g.Image(value.Icon, rect_icon);
				}
				if (COLUMN.CellType != SelectCellType.Icon && rect_text != Rectangle.Empty)
				{
					g.DrawText(value.Text, font, color, rect_text, FormatFlags.Center | FormatFlags.NoWrap);
				}
			}
		}

		public override string? ToString()
		{
			return value?.Text;
		}
	}

	/// <summary>
	/// 表头
	/// </summary>
	internal class TCellColumn : CELL
	{
		public Rectangle rect_up;

		public Rectangle rect_down;

		public Rectangle rect_filter;

		public int SFWidth;

		/// <summary>
		/// 值
		/// </summary>
		public string value { get; set; }

		public TCellColumn(Table table, Column column)
			: base(table, column)
		{
			value = column.Title;
		}

		public override void SetSize(Canvas g, Font font, Size font_size, Rectangle _rect, int ox, TableGaps gap)
		{
			RECT = _rect;
			if (COLUMN.SortOrder || COLUMN.HasFilter)
			{
				int num = ((!PARENT.SortOrderSize.HasValue) ? ((int)((float)font_size.Height * 0.6f)) : ((int)((float)PARENT.SortOrderSize.Value * g.Dpi)));
				int num2 = num * 2;
				int num3 = (int)((float)num * 0.34f);
				int num4 = 0;
				int num5 = _rect.Right - ((gap.x > 0) ? gap.x : num3);
				if (COLUMN.HasFilter)
				{
					rect_filter = new Rectangle(num5 - num4 - num, _rect.Y + (_rect.Height - num) / 2, num, num);
					num4 = num + num3;
				}
				if (COLUMN.SortOrder)
				{
					int y = _rect.Y + (_rect.Height - num2 + num3) / 2;
					rect_up = new Rectangle(num5 - num4 - num, y, num, num);
					rect_down = new Rectangle(rect_up.X, rect_up.Bottom - num3, num, num);
				}
			}
		}

		public override Size GetSize(Canvas g, Font font, Size font_size, int width, TableGaps gap)
		{
			if (COLUMN.ColBreak && COLUMN.Width != null)
			{
				if (COLUMN.Width.EndsWith("%") && float.TryParse(COLUMN.Width.TrimEnd('%'), out var result))
				{
					Size size = g.MeasureText(value, font, (int)Math.Ceiling((float)width * (result / 100f)));
					MinWidth = size.Width;
					return new Size(size.Width + gap.x2, size.Height);
				}
				if (int.TryParse(COLUMN.Width, out var result2))
				{
					Size size2 = g.MeasureText(value, font, (int)Math.Ceiling((float)result2 * g.Dpi));
					MinWidth = size2.Width;
					return new Size(size2.Width + gap.x2, size2.Height);
				}
			}
			Size size3 = g.MeasureText(value, font);
			if (COLUMN.SortOrder || COLUMN.HasFilter)
			{
				int num = (int)((float)size3.Height * 0.86f);
				if (COLUMN.SortOrder && COLUMN.HasFilter)
				{
					SFWidth = num * 2;
				}
				else
				{
					SFWidth = num;
				}
			}
			MinWidth = size3.Width + gap.x2 + SFWidth;
			return new Size(size3.Width + gap.x2 + SFWidth, size3.Height);
		}

		public void Print(Canvas g, TAMode colorScheme, Font font, SolidBrush fore)
		{
			if (COLUMN.SortOrder)
			{
				g.Svg(SvgDb.IcoTableSortUp, rect_up, (COLUMN.SortMode == SortMode.ASC) ? Colour.Primary.Get(colorScheme, "Table", PARENT.Name) : Colour.TextQuaternary.Get(colorScheme, "Table", PARENT.Name));
				g.Svg(SvgDb.IcoTableSortDown, rect_down, (COLUMN.SortMode == SortMode.DESC) ? Colour.Primary.Get(colorScheme, "Table", PARENT.Name) : Colour.TextQuaternary.Get(colorScheme, "Table", PARENT.Name));
			}
			if (COLUMN.HasFilter)
			{
				g.Svg(SvgDb.IcoTableFilter, rect_filter, COLUMN.Filter.Enabled ? Colour.Primary.Get(colorScheme, "Table", PARENT.Name) : Colour.TextQuaternary.Get(colorScheme, "Table", PARENT.Name));
			}
			if (COLUMN is ColumnCheck { NoTitle: not false } columnCheck)
			{
				PaintCheck(g, colorScheme, columnCheck);
			}
			else
			{
				PrintText(g, font, fore, COLUMN.ColStyle);
			}
		}

		private void PrintText(Canvas g, Font defaultFont, Brush defaultFore, CellStyleInfo? style)
		{
			if (style == null)
			{
				g.DrawText(value, defaultFont, defaultFore, RECT_REAL, StringFormat(COLUMN, isColumn: true));
				return;
			}
			Font font = style.Font ?? defaultFont;
			Font font2 = null;
			if (style.FontBold.HasValue)
			{
				FontStyle style2 = font.Style;
				style2 = ((!style.FontBold.Value) ? (style2 & ~FontStyle.Bold) : (style2 | FontStyle.Bold));
				if (style2 != font.Style)
				{
					font2 = new Font(font.FontFamily, font.Size, style2);
				}
			}
			using (font2)
			{
				Font font3 = font2 ?? font;
				using Brush brush = (style.ForeColor.HasValue ? new SolidBrush(style.ForeColor.Value) : null);
				Brush brush2 = brush ?? defaultFore;
				g.DrawText(value, font3, brush2, RECT_REAL, StringFormat(COLUMN, isColumn: true));
			}
		}

		private void PaintCheck(Canvas g, TAMode colorScheme, ColumnCheck columnCheck)
		{
			using GraphicsPath path = RECT_REAL.RoundPath(PARENT.check_radius);
			if (columnCheck.AnimationCheck)
			{
				g.Fill(Colour.BgBase.Get(colorScheme, "Checkbox", PARENT.Name), path);
				float alpha = 255f * columnCheck.AnimationCheckValue;
				if (columnCheck.CheckState == CheckState.Indeterminate || (columnCheck.checkStateOld == CheckState.Indeterminate && !columnCheck.Checked))
				{
					g.Draw(Colour.BorderColor.Get(colorScheme, "Checkbox", PARENT.Name), PARENT.check_border, path);
					g.Fill(Helper.ToColor(alpha, Colour.Primary.Get(colorScheme, "Checkbox", PARENT.Name)), Checkbox.PaintBlock(RECT_REAL));
					return;
				}
				g.Fill(Helper.ToColor(alpha, Colour.Primary.Get(colorScheme, "Checkbox", PARENT.Name)), path);
				using (Pen pen = new Pen(Helper.ToColor(alpha, Colour.BgBase.Get(colorScheme, "Checkbox", PARENT.Name)), PARENT.check_border * 2f))
				{
					g.DrawLines(pen, PaintArrow(RECT_REAL));
				}
				if (columnCheck.Checked)
				{
					float num = (float)RECT_REAL.Height + (float)RECT_REAL.Height * columnCheck.AnimationCheckValue;
					using SolidBrush brush = new SolidBrush(Helper.ToColor(100f * (1f - columnCheck.AnimationCheckValue), Colour.Primary.Get(colorScheme, "Checkbox", PARENT.Name)));
					g.FillEllipse(brush, new RectangleF((float)RECT_REAL.X + ((float)RECT_REAL.Width - num) / 2f, (float)RECT_REAL.Y + ((float)RECT_REAL.Height - num) / 2f, num, num));
				}
				g.Draw(Colour.Primary.Get(colorScheme, "Checkbox", PARENT.Name), PARENT.check_border, path);
				return;
			}
			if (columnCheck.CheckState == CheckState.Indeterminate)
			{
				g.Fill(Colour.BgBase.Get(colorScheme, "Checkbox", PARENT.Name), path);
				g.Draw(Colour.BorderColor.Get(colorScheme, "Checkbox", PARENT.Name), PARENT.check_border, path);
				g.Fill(Colour.Primary.Get(colorScheme, "Checkbox", PARENT.Name), Checkbox.PaintBlock(RECT_REAL));
				return;
			}
			if (columnCheck.Checked)
			{
				g.Fill(Colour.Primary.Get(colorScheme, "Checkbox", PARENT.Name), path);
				using Pen pen2 = new Pen(Colour.BgBase.Get(colorScheme, "Checkbox", PARENT.Name), PARENT.check_border * 2f);
				g.DrawLines(pen2, PaintArrow(RECT_REAL));
				return;
			}
			g.Fill(Colour.BgBase.Get(colorScheme, "Checkbox", PARENT.Name), path);
			g.Draw(Colour.BorderColor.Get(colorScheme, "Checkbox", PARENT.Name), PARENT.check_border, path);
		}

		public override string ToString()
		{
			return value;
		}
	}

	/// <summary>
	/// 单元格
	/// </summary>
	public abstract class CELL
	{
		private RowTemplate? _ROW;

		internal int offsetx;

		internal int offsety;

		internal int mergeW = -1;

		/// <summary>
		/// 表对象
		/// </summary>
		public Table PARENT { get; set; }

		public Column COLUMN { get; set; }

		/// <summary>
		/// 列对象
		/// </summary>
		public int INDEX { get; set; }

		public PropertyDescriptor? PROPERTY { get; set; }

		public object? VALUE { get; set; }

		/// <summary>
		/// 行对象
		/// </summary>
		internal RowTemplate ROW
		{
			get
			{
				if (_ROW == null)
				{
					throw new ArgumentNullException();
				}
				return _ROW;
			}
		}

		/// <summary>
		/// 行
		/// </summary>
		public IROW Row => ROW;

		/// <summary>
		/// 区域
		/// </summary>
		public Rectangle RECT { get; set; }

		/// <summary>
		/// 显示区域
		/// </summary>
		public Rectangle RECT_REAL { get; set; }

		/// <summary>
		/// 最小宽度
		/// </summary>
		public int MinWidth { get; set; }

		public CELL(Table table, Column column)
		{
			COLUMN = column;
			PARENT = table;
		}

		public CELL(Table table, Column column, PropertyDescriptor? prop, object? ov)
		{
			COLUMN = column;
			PARENT = table;
			PROPERTY = prop;
			VALUE = ov;
		}

		internal void SetROW(RowTemplate row)
		{
			_ROW = row;
		}

		public bool CONTAIN(int x, int y)
		{
			return RECT.Contains(x, y);
		}

		public bool CONTAIN_REAL(int x, int y)
		{
			return RECT_REAL.Contains(x - offsetx, y - offsety);
		}

		public abstract void SetSize(Canvas g, Font font, Size font_size, Rectangle _rect, int ox, TableGaps gap);

		public abstract Size GetSize(Canvas g, Font font, Size font_size, int width, TableGaps gap);
	}

	/// <summary>
	/// 包裹容器
	/// </summary>
	internal class Template : CELL
	{
		private struct CellLine
		{
			public int Start;

			public int End;

			public int VisibleCount;

			public int ContentWidth;
		}

		private Size[] SIZES = new Size[0];

		/// <summary>
		/// 值
		/// </summary>
		public IList<ICell> Value { get; set; }

		public Template(Table table, Column column, PropertyDescriptor? prop, object? ov, ref int processing, IList<ICell> cels)
			: base(table, column, prop, ov)
		{
			Value = cels;
			foreach (ICell cel in cels)
			{
				cel.SetCELL(this);
				if (cel is CellBadge { State: TState.Processing })
				{
					processing++;
				}
			}
		}

		public override void SetSize(Canvas g, Font font, Size font_size, Rectangle _rect, int ox, TableGaps gap)
		{
			Rectangle rECT = (RECT_REAL = _rect);
			RECT = rECT;
			int num = _rect.X + ox;
			int num2 = gap.x / 2;
			int num3 = _rect.Width - gap.x2;
			if (COLUMN.Wrap)
			{
				List<CellLine> list = GroupLines(SIZES, COLUMN.WrapCount, num3, num2);
				if (list.Count == 0)
				{
					return;
				}
				int[] array = new int[list.Count];
				int num4 = 0;
				for (int i = 0; i < list.Count; i++)
				{
					CellLine cellLine = list[i];
					int num5 = 0;
					for (int j = cellLine.Start; j < cellLine.End; j++)
					{
						if (num5 < SIZES[j].Height)
						{
							num5 = SIZES[j].Height;
						}
					}
					array[i] = num5;
					if (num4 < cellLine.ContentWidth)
					{
						num4 = cellLine.ContentWidth;
					}
				}
				int[] array2 = LineGapY(list, array, _rect.Height, num2, COLUMN.WrapGapY, out var leadingGap);
				int num6 = 0;
				for (int k = 0; k < list.Count; k++)
				{
					num6 += array[k];
				}
				for (int l = 0; l < array2.Length; l++)
				{
					num6 += array2[l];
				}
				int num7 = _rect.Y + leadingGap + ((leadingGap == 0 && num6 < _rect.Height) ? ((_rect.Height - num6) / 2) : 0);
				for (int m = 0; m < list.Count; m++)
				{
					CellLine line = list[m];
					int num8 = array[m];
					int num9 = LineGapX(line, num4, num2, COLUMN.WrapGapX, out var renderWidth);
					int num10 = COLUMN.Align switch
					{
						ColumnAlign.Center => num + gap.x + (num3 - renderWidth) / 2, 
						ColumnAlign.Right => num + gap.x + (num3 - renderWidth), 
						_ => num + gap.x, 
					};
					int num11 = num3;
					int num12 = 0;
					for (int n = line.Start; n < line.End; n++)
					{
						ICell cell = Value[n];
						Size size = SIZES[n];
						if (size.Width <= 0)
						{
							cell.SetRect(g, font, new Rectangle(num10, num7, 0, num8), size, num11, gap);
							continue;
						}
						cell.SetRect(g, font, new Rectangle(num10, num7, size.Width, num8), size, num11, gap);
						num12++;
						int num13 = cell.Rect.Width;
						if (num12 < line.VisibleCount)
						{
							num13 += num9;
						}
						num10 += num13;
						num11 -= num13;
					}
					num7 += num8 + ((m < array2.Length) ? array2[m] : 0);
				}
			}
			else
			{
				int num14 = COLUMN.Align switch
				{
					ColumnAlign.Center => num + (_rect.Width - MinWidth + gap.x2) / 2, 
					ColumnAlign.Right => _rect.Right - MinWidth + gap.x, 
					_ => num + gap.x, 
				};
				for (int num15 = 0; num15 < Value.Count; num15++)
				{
					ICell cell2 = Value[num15];
					Size size2 = SIZES[num15];
					cell2.SetRect(g, font, new Rectangle(num14, _rect.Y, size2.Width, _rect.Height), size2, num3, gap);
					int num16 = size2.Width + num2;
					num14 += num16;
					num3 -= num16;
				}
			}
		}

		private static List<CellLine> GroupLines(Size[] sizes, int count, int maxwidth, int sp)
		{
			List<CellLine> list = new List<CellLine>();
			int num = sizes.Length;
			if (num == 0)
			{
				return list;
			}
			if (count > 0)
			{
				for (int i = 0; i < num; i += count)
				{
					int end = Math.Min(i + count, num);
					list.Add(MakeLine(sizes, i, end));
				}
			}
			else
			{
				if (maxwidth <= 0)
				{
					list.Add(MakeLine(sizes, 0, num));
					return list;
				}
				int start = 0;
				int num2 = 0;
				bool flag = true;
				for (int j = 0; j < num; j++)
				{
					int width = sizes[j].Width;
					if (width > 0)
					{
						int num3 = (flag ? width : (sp + width));
						if (!flag && num2 + num3 > maxwidth)
						{
							list.Add(MakeLine(sizes, start, j));
							start = j;
							num2 = width;
							flag = false;
						}
						else
						{
							num2 += num3;
							flag = false;
						}
					}
				}
				if (!flag)
				{
					list.Add(MakeLine(sizes, start, num));
				}
			}
			return list;
		}

		private static CellLine MakeLine(Size[] sizes, int start, int end)
		{
			int num = 0;
			int num2 = 0;
			for (int i = start; i < end; i++)
			{
				int width = sizes[i].Width;
				if (width > 0)
				{
					num++;
					num2 += width;
				}
			}
			return new CellLine
			{
				Start = start,
				End = end,
				VisibleCount = num,
				ContentWidth = num2
			};
		}

		private static int LineGapX(CellLine line, int widthMax, int sp, TableWrapGap gapX, out int renderWidth)
		{
			if (gapX == TableWrapGap.SpaceBetween)
			{
				if (line.VisibleCount >= 2)
				{
					int num = (widthMax - line.ContentWidth) / (line.VisibleCount - 1);
					if (num < 0)
					{
						renderWidth = line.ContentWidth + (line.VisibleCount - 1) * sp;
						return sp;
					}
					renderWidth = widthMax;
					return num;
				}
				renderWidth = line.ContentWidth;
				return 0;
			}
			renderWidth = line.ContentWidth + ((line.VisibleCount > 0) ? ((line.VisibleCount - 1) * sp) : 0);
			return sp;
		}

		private static int[] LineGapY(List<CellLine> lines, int[] lineHeights, int targetHeight, int sp, TableWrapGap gapY, out int leadingGap)
		{
			leadingGap = 0;
			int count = lines.Count;
			int[] array = new int[(count > 1) ? (count - 1) : 0];
			if (count < 2)
			{
				return array;
			}
			if (gapY == TableWrapGap.SpaceBetween)
			{
				int num = 0;
				for (int i = 0; i < lineHeights.Length; i++)
				{
					num += lineHeights[i];
				}
				int num2 = (targetHeight - num) / (count + 1);
				if (num2 >= sp)
				{
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = num2;
					}
					leadingGap = num2;
					return array;
				}
			}
			for (int k = 0; k < array.Length; k++)
			{
				array[k] = sp;
			}
			return array;
		}

		public override Size GetSize(Canvas g, Font font, Size font_size, int width, TableGaps gap)
		{
			int num = gap.x / 2;
			List<Size> list = new List<Size>(Value.Count);
			foreach (ICell item in Value)
			{
				list.Add(item.GetSize(g, font, gap));
			}
			Size result;
			if (COLUMN.Wrap)
			{
				List<CellLine> list2 = GroupLines(list.ToArray(), COLUMN.WrapCount, width - gap.x2, num);
				int num2 = 0;
				for (int i = 0; i < list2.Count; i++)
				{
					if (num2 < list2[i].ContentWidth)
					{
						num2 = list2[i].ContentWidth;
					}
				}
				int[] array = new int[list2.Count];
				int num3 = 0;
				for (int j = 0; j < list2.Count; j++)
				{
					int num4 = 0;
					for (int k = list2[j].Start; k < list2[j].End; k++)
					{
						if (num4 < list[k].Height)
						{
							num4 = list[k].Height;
						}
					}
					array[j] = num4;
					num3 += num4;
				}
				int targetHeight = num3 + ((list2.Count > 1) ? ((list2.Count - 1) * num) : 0);
				int[] array2 = LineGapY(list2, array, targetHeight, num, COLUMN.WrapGapY, out var leadingGap);
				int num5 = num3 + leadingGap;
				for (int l = 0; l < array2.Length; l++)
				{
					num5 += array2[l];
				}
				if (leadingGap > 0)
				{
					num5 += leadingGap;
				}
				int num6 = 0;
				for (int m = 0; m < list2.Count; m++)
				{
					LineGapX(list2[m], num2, num, COLUMN.WrapGapX, out var renderWidth);
					if (num6 < renderWidth)
					{
						num6 = renderWidth;
					}
				}
				MinWidth = num6 + gap.x2;
				result = new Size(MinWidth, num5);
			}
			else
			{
				int num7 = 0;
				int num8 = 0;
				bool flag = PARENT.CellImpactHeight ?? true;
				for (int n = 0; n < Value.Count; n++)
				{
					num7 += list[n].Width + num;
					if ((Value[n].ImpactHeight ?? flag) && num8 < list[n].Height)
					{
						num8 = list[n].Height;
					}
				}
				MinWidth = num7 + gap.x + num;
				result = new Size(MinWidth, num8);
			}
			SIZES = list.ToArray();
			return result;
		}

		public override string? ToString()
		{
			List<string> list = new List<string>(Value.Count);
			foreach (ICell item in Value)
			{
				string text = item.ToString();
				if (text != null && !string.IsNullOrEmpty(text))
				{
					list.Add(text);
				}
			}
			return string.Join(" ", list);
		}
	}

	internal class StyleRow
	{
		public RowTemplate row { get; set; }

		public CellStyleInfo? style { get; set; }

		public StyleRow(RowTemplate _row, CellStyleInfo? _style)
		{
			row = _row;
			style = _style;
		}
	}

	internal class AutoWidth
	{
		/// <summary>
		/// 自动值
		/// </summary>
		public int value { get; set; }

		/// <summary>
		/// 最小值
		/// </summary>
		public int minvalue { get; set; }

		/// <summary>
		/// Minimum width configured by the column.
		/// </summary>
		public int? MinWidth { get; set; }

		/// <summary>
		/// Maximum width configured by the column.
		/// </summary>
		public int? MaxWidth { get; set; }
	}

	internal class MoveHeader
	{
		public Rectangle rect { get; set; }

		public bool MouseDown { get; set; }

		public int x { get; set; }

		public int i { get; set; }

		public int width { get; set; }

		public int min_width { get; set; }

		public MoveHeader(Dictionary<int, MoveHeader> dir, Rectangle r, int index, int w, int min)
		{
			rect = r;
			i = index;
			min_width = min;
			if (dir.TryGetValue(index, out MoveHeader value) && value.MouseDown)
			{
				x = value.x;
				MouseDown = value.MouseDown;
				width = value.width;
			}
			else
			{
				width = w;
			}
		}
	}

	internal class DividerList
	{
		public int a { get; set; }

		public int b { get; set; }

		public int c { get; set; }

		public bool h { get; set; } = true;

		public DividerList(int A, int B, int C)
		{
			a = A;
			b = B;
			c = C;
		}
	}

	internal class DragHeader
	{
		public int _ox { get; set; }

		public int _oy { get; set; }

		public bool enable { get; set; }

		public int x { get; set; }

		public int xr { get; set; }

		public int i { get; set; }

		public int im { get; set; } = -1;

		public bool last { get; set; }

		public bool hand { get; set; }

		public DragHeader(int ox, int oy, int _i, int _x)
		{
			_ox = ox;
			_oy = oy;
			i = _i;
			x = _x;
		}

		public void SetEnable(int x, int y)
		{
			if (x != _ox || y != _oy)
			{
				enable = true;
			}
		}
	}

	private ColumnCollection? columns;

	private object? dataSource;

	private object? summary;

	private Color? fore;

	private bool fixedHeader = true;

	private bool visibleHeader = true;

	private bool enableHeaderResizing;

	private bool bordered;

	private int radius;

	private bool scrollBarAvoidHeader;

	private Size _gap = new Size(12, 12);

	private int _gapTree = 12;

	private int? rowHeight;

	private int? rowHeightHeader;

	private int _checksize = 16;

	private int _switchsize = 16;

	private int[]? focusedxy;

	private bool empty = true;

	private string? emptyText;

	private bool emptyHeader;

	private Color? rowHoverBg;

	private Color? rowSelectedBg;

	private Color? rowSelectedFore;

	private float borderWidth = 1f;

	private Color? borderColor;

	private bool? borderHigh;

	private Font? columnfont;

	private Color? columnback;

	private Color? columnfore;

	private int[] selectedIndex = new int[0];

	private bool summaryCustomize;

	/// <summary>
	/// 滚动条
	/// </summary>
	[Browsable(false)]
	public ScrollBar ScrollBar;

	private TEditMode editmode;

	private bool pauseLayout;

	private List<object> enableDir = new List<object>();

	private CellRange[]? CellRanges;

	internal TempTable? dataTmp;

	private bool dataOne = true;

	private int hovers = -1;

	private int dataLen;

	private bool showFixedColumnL;

	private bool showFixedColumnR;

	private int sFixedR;

	private List<int>? fixedColumnL;

	private List<int>? fixedColumnR;

	private string? TreeKey;

	private bool inEditMode;

	private ConcurrentDictionary<Input, EditControlTag> _editControls = new ConcurrentDictionary<Input, EditControlTag>();

	private TableCellEditEnterEventArgs? _currentEdit;

	private NavigationConfig? navigationConfig;

	internal RowList? rows;

	private int rowSummary;

	private DividerList[] dividers = new DividerList[0];

	private DividerList[] dividerHs = new DividerList[0];

	private MoveHeader[] moveheaders = new MoveHeader[0];

	private bool has_check;

	private Rectangle rect_read;

	private Rectangle rect_divider;

	private int? _RowHeightHeader;

	private int? _RowHeight;

	/// <summary>
	/// 虚拟模式滚动基准偏移（RECT 坐标 = 绝对坐标 - 基准偏移，随滚动窗口整体平移）
	/// </summary>
	internal int virtualMode_Y;

	private Dictionary<int, int> tmpcol_width = new Dictionary<int, int>(0);

	private AnimationTask? ThreadState;

	internal float AnimationStateValue;

	private float check_radius;

	private float check_border = 1f;

	private bool isMVVM;

	private bool pauseLayoutByCheckAll;

	private int shift_index = -1;

	private int mouseClicks = 1;

	private LayeredFormSelectDown? subForm;

	private object? tmp;

	private TooltipForm? toolTip;

	private DownCellTMP<CELL>? cellMouseDown;

	private DownCellTMP<CellLink>? btnMouseDown;

	private DragHeader? dragHeader;

	private DragHeader? dragBody;

	private int[]? SortHeader;

	private int[]? SortData;

	private Rectangle? rect_fixed_L;

	private Rectangle? rect_fixed_R;

	private Rectangle? rect_fixed_B;

	private int sFixedB = -1;

	protected const string SvgSummaryMIN = "<svg viewBox=\"0 0 1194 1024\"><path d=\"M682.666667 256h170.666666v768H682.666667z\"></path><path d=\"M0 768h170.666667v256H0z\" fill=\"#FF0000\"></path><path d=\"M341.333333 512h170.666667v512H341.333333zM1024 0h170.666667v1024h-170.666667z\"></path></svg>";

	protected const string SvgSummaryMAX = "<svg viewBox=\"0 0 1194 1024\"><path d=\"M682.666667 256h170.666666v768H682.666667zM0 768h170.666667v256H0zM341.333333 512h170.666667v512H341.333333z\"></path><path d=\"M1024 0h170.666667v1024h-170.666667z\" fill=\"#FF0000\"></path></svg>";

	protected const string SvgSummaryAVG = "<svg viewBox=\"0 0 1024 1024\"><path d=\"M89.6 0m38.4 0l0 0q38.4 0 38.4 38.4l0 947.2q0 38.4-38.4 38.4l0 0q-38.4 0-38.4-38.4l0-947.2q0-38.4 38.4-38.4Z\"></path><path d=\"M428.736 256m83.2 0l0 0q83.2 0 83.2 83.2l0 345.6q0 83.2-83.2 83.2l0 0q-83.2 0-83.2-83.2l0-345.6q0-83.2 83.2-83.2Z\" fill=\"#d81e06\"></path><path d=\"M857.6 0m38.4 0l0 0q38.4 0 38.4 38.4l0 947.2q0 38.4-38.4 38.4l0 0q-38.4 0-38.4-38.4l0-947.2q0-38.4 38.4-38.4Z\"></path></svg>";

	protected const string SvgSummarySUM = "<svg viewBox=\"0 0 1024 1024\"><path d=\"M843.776 205.312v-88.064H266.24l339.968 362.496c21.504 22.528 20.992 57.856-0.512 80.384L269.824 906.752h574.464v-53.248c0-32.256 26.624-58.368 58.88-58.368 32.768 0 58.88 26.112 58.88 58.368V965.12c0 32.256-26.624 58.368-58.88 58.368H131.072c-23.552 0-45.056-13.824-54.272-35.84-9.216-21.504-4.608-46.592 11.776-63.488l393.728-406.016L88.064 97.792C71.68 81.408 67.072 56.32 76.8 35.328 86.016 13.824 107.52 0 131.072 0h772.096c32.768 0 58.88 26.112 58.88 58.368v146.944c0 32.256-26.624 58.368-58.88 58.368-32.768 0-59.392-26.112-59.392-58.368z\"></path></svg>";

	protected const string SvgSummaryCNT = "<svg viewBox=\"0 0 1024 1024\"><path d=\"M609.792 877.568H166.4l283.136-346.624-282.624-427.52h419.328c84.992 0 157.184 56.832 216.576 169.984h14.336l-30.208-224.768H14.848L353.28 531.456 14.848 966.144h772.608l30.208-225.28h-14.336c-56.832 91.136-121.344 136.704-193.536 136.704z\" fill=\"#13227a\"></path><path d=\"M593.408 439.296c16.384 0 30.72 2.56 43.008 8.192V395.776c-11.776-4.608-30.72-7.168-49.664-7.168-59.904 0-95.232 25.6-95.232 118.272 0 92.16 35.328 118.272 95.232 118.784 17.408 0 39.936-3.072 51.2-8.704v-50.176c-13.824 6.144-28.16 8.192-43.52 8.192-35.328 0-50.176-14.848-50.176-67.584 0-52.736 14.848-68.096 49.152-68.096zM792.064 388.608c-28.16 0-50.176 10.24-60.416 31.744v-27.648h-51.712v228.352h51.712V470.016c5.632-23.552 19.456-31.232 40.96-31.232 25.088 0 31.232 11.776 31.232 42.496v139.264h51.712V464.896c0-50.176-15.872-76.288-63.488-76.288zM1002.496 441.344v-48.128H962.56V338.944h-52.224v53.76h-25.6v48.128h25.6v106.496c0 59.904 16.896 75.776 64.512 75.776 8.704 0 17.408-1.024 25.088-2.048v-47.104c-4.096 1.024-9.216 1.536-15.36 1.536-17.92 0-22.016-6.144-22.016-32.768V441.344h39.936z\"></path></svg>";

	protected const string SvgSummaryNONE = "<svg viewBox=\"0 0 1024 1024\"><path d=\"M832 460.8l128-128c12.8-12.8 38.4-12.8 51.2 0 12.8 12.8 12.8 38.4 0 51.2l-128 128 128 128c12.8 12.8 12.8 38.4 0 51.2s-38.4 12.8-51.2 0l-128-128-128 128c-12.8 12.8-38.4 12.8-51.2 0s-12.8-38.4 0-51.2l128-128-128-128c-12.8-12.8-12.8-38.4 0-51.2 12.8-12.8 38.4-12.8 51.2 0l128 128z\" fill=\"#d81e06\"></path><path d=\"M736 64H38.4c-12.8 0-19.2 0-25.6 6.4-12.8 12.8-12.8 38.4 0 51.2l38.4 38.4 313.6 313.6c6.4 6.4 6.4 25.6 0 32L64 806.4l-64 64v51.2c0 19.2 19.2 38.4 38.4 38.4h704c19.2 0 32-12.8 32-32v-64c0-19.2-12.8-32-32-32s-38.4 12.8-38.4 32v32H70.4l345.6-345.6c32-32 32-89.6 0-121.6L108.8 128H704v32c0 19.2 12.8 32 32 32s32-12.8 32-32v-64c0-19.2-12.8-32-32-32z\"></path></svg>";

	protected const string SvgSummarySelectionMode = "<svg viewBox=\"0 0 1024 1024\"><path d=\"M921.6 0 102.4 0C46.08 0 0 46.08 0 102.4l0 819.2c0 56.32 46.08 102.4 102.4 102.4l819.2 0c56.32 0 102.4-46.08 102.4-102.4L1024 102.4C1024 46.08 977.92 0 921.6 0zM972.8 921.6c0 28.2368-22.9632 51.2-51.2 51.2L102.4 972.8c-28.2368 0-51.2-22.9632-51.2-51.2L51.2 102.4c0-28.2368 22.9632-51.2 51.2-51.2l819.2 0c28.2368 0 51.2 22.9632 51.2 51.2L972.8 921.6z\"></path><path d=\"M128 422.4l768 0 0 179.2-768 0 0-179.2Z\"></path><path d=\"M128 704l768 0 0 179.2-768 0 0-179.2Z\" fill=\"#d81e06\"></path><path d=\"M128 140.8l768 0 0 179.2-768 0 0-179.2Z\" fill=\"#d81e06\"></path></svg>";

	protected static string SvgSummaryAllMode = "<svg viewBox=\"0 0 1024 1024\"><path d=\"M921.6 0 102.4 0C46.08 0 0 46.08 0 102.4l0 819.2c0 56.32 46.08 102.4 102.4 102.4l819.2 0c56.32 0 102.4-46.08 102.4-102.4L1024 102.4C1024 46.08 977.92 0 921.6 0zM972.8 921.6c0 28.2368-22.9632 51.2-51.2 51.2L102.4 972.8c-28.2368 0-51.2-22.9632-51.2-51.2L51.2 102.4c0-28.2368 22.9632-51.2 51.2-51.2l819.2 0c28.2368 0 51.2 22.9632 51.2 51.2L972.8 921.6z\"></path><path d=\"M128 422.4l768 0 0 179.2-768 0 0-179.2Z\"></path><path d=\"M128 704l768 0 0 179.2-768 0 0-179.2Z\" fill=\"#d81e06\"></path><path d=\"M128 140.8l768 0 0 179.2-768 0 0-179.2Z\" fill=\"#d81e06\"></path></svg>".Replace("fill=\"#d81e06\"", "");

	private Form? FMenuStrip;

	/// <summary>
	/// 表格列的配置
	/// </summary>
	[Browsable(false)]
	[Description("表格列的配置")]
	[Category("Data")]
	[DefaultValue(null)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ColumnCollection Columns
	{
		get
		{
			if (columns == null)
			{
				columns = new ColumnCollection();
			}
			columns.table = this;
			return columns;
		}
		set
		{
			if (columns != value)
			{
				SortHeader = null;
				columns = value;
				if (LoadLayout())
				{
					Invalidate();
				}
				if (value != null)
				{
					value.table = this;
					ExtractHeaderFixed();
					OnPropertyChanged("Columns");
				}
			}
		}
	}

	/// <summary>
	/// 数据数组
	/// </summary>
	[Browsable(false)]
	[Description("数据数组")]
	[Category("Data")]
	[DefaultValue(null)]
	public object? DataSource
	{
		get
		{
			return dataSource;
		}
		set
		{
			enableDir.Clear();
			EditModeClose();
			CellRanges = null;
			dataSource = value;
			SortData = null;
			focusedxy = null;
			hovers = -1;
			selectedIndex = new int[0];
			ExtractHeaderFixed();
			ExtractData();
			OnUpdateSummaries();
			if (LoadLayout())
			{
				Invalidate();
			}
			OnPropertyChanged("DataSource");
		}
	}

	/// <summary>
	/// 总结栏
	/// </summary>
	[Browsable(false)]
	[Description("总结栏")]
	[Category("Data")]
	[DefaultValue(null)]
	public object? Summary
	{
		get
		{
			return summary;
		}
		set
		{
			summary = value;
			if (ExtractDataSummary())
			{
				ExtractData();
			}
			if (LoadLayout())
			{
				Invalidate();
			}
			OnPropertyChanged("Summary");
		}
	}

	/// <summary>
	/// 文字颜色
	/// </summary>
	[Description("文字颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public new Color? ForeColor
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
				OnPropertyChanged("ForeColor");
			}
		}
	}

	/// <summary>
	/// 固定表头
	/// </summary>
	[Description("固定表头")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool FixedHeader
	{
		get
		{
			return fixedHeader;
		}
		set
		{
			if (fixedHeader != value)
			{
				fixedHeader = value;
				SetSAH();
				Invalidate();
				OnPropertyChanged("FixedHeader");
			}
		}
	}

	/// <summary>
	/// 显示表头
	/// </summary>
	[Description("显示表头")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool VisibleHeader
	{
		get
		{
			return visibleHeader;
		}
		set
		{
			if (visibleHeader != value)
			{
				visibleHeader = value;
				ScrollBar.RB = !value;
				SetSAH();
				if (LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("VisibleHeader");
			}
		}
	}

	/// <summary>
	/// 手动调整列头宽度
	/// </summary>
	[Description("手动调整列头宽度")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool EnableHeaderResizing
	{
		get
		{
			return enableHeaderResizing;
		}
		set
		{
			if (enableHeaderResizing != value)
			{
				enableHeaderResizing = value;
				if (LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("EnableHeaderResizing");
			}
		}
	}

	/// <summary>
	/// 列拖拽排序
	/// </summary>
	[Description("列拖拽排序")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool ColumnDragSort { get; set; }

	/// <summary>
	/// 焦点离开清空选中
	/// </summary>
	[Description("焦点离开清空选中")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool LostFocusClearSelection { get; set; }

	/// <summary>
	/// 鼠标点击穿透
	/// </summary>
	[Description("鼠标点击穿透")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool MouseClickPenetration { get; set; } = true;

	/// <summary>
	/// 显示列边框
	/// </summary>
	[Description("显示列边框")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool Bordered
	{
		get
		{
			return bordered;
		}
		set
		{
			if (bordered != value)
			{
				bordered = value;
				if (LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("Bordered");
			}
		}
	}

	/// <summary>
	/// 圆角
	/// </summary>
	[Description("圆角")]
	[Category("Appearance")]
	[DefaultValue(0)]
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
				ScrollBar.Radius = (radius = value);
				Invalidate();
				OnPropertyChanged("Radius");
			}
		}
	}

	/// <summary>
	/// 滚动条从表头下方开始绘制（避免遮挡表头）
	/// </summary>
	[Description("滚动条从表头下方开始绘制")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool ScrollBarAvoidHeader
	{
		get
		{
			return scrollBarAvoidHeader;
		}
		set
		{
			if (scrollBarAvoidHeader != value)
			{
				scrollBarAvoidHeader = value;
				SetSAH();
				if (LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("ScrollBarAvoidHeader");
			}
		}
	}

	/// <summary>
	/// 行复制
	/// </summary>
	[Description("行/列复制")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool ClipboardCopy { get; set; } = true;

	/// <summary>
	/// 是否启用单元格复制
	/// </summary>
	[Description("是否启用单元格复制")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool ClipboardCopyFocusedCell { get; set; }

	/// <summary>
	/// 列宽自动调整模式
	/// </summary>
	[Description("列宽自动调整模式")]
	[Category("Behavior")]
	[DefaultValue(ColumnsMode.Auto)]
	public ColumnsMode AutoSizeColumnsMode { get; set; }

	/// <summary>
	/// 虚拟模式
	/// </summary>
	[Description("虚拟模式")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool VirtualMode { get; set; }

	/// <summary>
	/// 间距
	/// </summary>
	[Description("间距")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? Gap
	{
		get
		{
			if (_gap.Width == _gap.Height)
			{
				return _gap.Width;
			}
			return null;
		}
		set
		{
			int num = value ?? 12;
			Gaps = new Size(num, num);
		}
	}

	/// <summary>
	/// 间距
	/// </summary>
	[Description("间距")]
	[Category("Appearance")]
	[DefaultValue(typeof(Size), "12, 12")]
	public Size Gaps
	{
		get
		{
			return _gap;
		}
		set
		{
			if (!(_gap == value))
			{
				_gap = value;
				if (LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("Gaps");
			}
		}
	}

	/// <summary>
	/// 单元格内间距
	/// </summary>
	[Description("单元格内间距")]
	[Category("Appearance")]
	[DefaultValue(6)]
	public int? GapCell { get; set; } = 6;

	[Description("单元格调整高度")]
	[Category("边框")]
	[DefaultValue(null)]
	public bool? CellImpactHeight { get; set; }

	/// <summary>
	/// 树间距
	/// </summary>
	[Description("树间距")]
	[Category("Appearance")]
	[DefaultValue(12)]
	public int GapTree
	{
		get
		{
			return _gapTree;
		}
		set
		{
			if (_gapTree != value)
			{
				_gapTree = value;
				if (LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("GapTree");
			}
		}
	}

	/// <summary>
	/// 行高
	/// </summary>
	[Description("行高")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? RowHeight
	{
		get
		{
			return rowHeight;
		}
		set
		{
			if (rowHeight != value)
			{
				rowHeight = value;
				if (LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("RowHeight");
			}
		}
	}

	/// <summary>
	/// 表头行高
	/// </summary>
	[Description("表头行高")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? RowHeightHeader
	{
		get
		{
			return rowHeightHeader;
		}
		set
		{
			if (rowHeightHeader != value)
			{
				rowHeightHeader = value;
				if (LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("RowHeightHeader");
			}
		}
	}

	/// <summary>
	/// 复选框大小
	/// </summary>
	[Description("复选框大小")]
	[Category("Appearance")]
	[DefaultValue(16)]
	public int CheckSize
	{
		get
		{
			return _checksize;
		}
		set
		{
			if (_checksize != value)
			{
				_checksize = value;
				if (LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("CheckSize");
			}
		}
	}

	/// <summary>
	/// 开关大小
	/// </summary>
	[Description("开关大小")]
	[Category("Appearance")]
	[DefaultValue(16)]
	public int SwitchSize
	{
		get
		{
			return _switchsize;
		}
		set
		{
			if (_switchsize != value)
			{
				_switchsize = value;
				if (LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("SwitchSize");
			}
		}
	}

	/// <summary>
	/// 树开关按钮大小
	/// </summary>
	[Description("树开关按钮大小")]
	[Category("Appearance")]
	[DefaultValue(16)]
	public int TreeButtonSize { get; set; } = 16;

	/// <summary>
	/// 拖拽手柄大小
	/// </summary>
	[Description("拖拽手柄大小")]
	[Category("Appearance")]
	[DefaultValue(24)]
	public int DragHandleSize { get; set; } = 24;

	/// <summary>
	/// 拖拽手柄图标大小
	/// </summary>
	[Description("拖拽手柄图标大小")]
	[Category("Appearance")]
	[DefaultValue(14)]
	public int DragHandleIconSize { get; set; } = 14;

	/// <summary>
	/// 排序大小
	/// </summary>
	[Description("排序大小")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public int? SortOrderSize { get; set; }

	/// <summary>
	/// 焦点列样式
	/// </summary>
	[Description("焦点列样式")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public TableCellFocusedStyle? CellFocusedStyle { get; set; }

	/// <summary>
	/// 焦点列背景色
	/// </summary>
	[Description("焦点列背景色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? CellFocusedBg { get; set; }

	/// <summary>
	/// 焦点列边框色
	/// </summary>
	[Description("焦点列边框色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Color? CellFocusedBorder { get; set; }

	/// <summary>
	/// 当前获得焦点的列
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Column? FocusedColumn => FocusedCell?.COLUMN;

	/// <summary>
	/// 当前获得焦点的行
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public object? FocusedRow => FocusedCell?.ROW.RECORD;

	/// <summary>
	/// 当前获得焦点的单元格
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public CELL? FocusedCell
	{
		get
		{
			if (focusedxy == null)
			{
				return null;
			}
			RowTemplate rowTemplate = rows?[focusedxy[1]];
			if (rowTemplate == null)
			{
				return null;
			}
			return rowTemplate.cells[focusedxy[0]];
		}
	}

	[Description("是否显示空样式")]
	[Category("Appearance")]
	[DefaultValue(true)]
	public bool Empty
	{
		get
		{
			return empty;
		}
		set
		{
			if (empty != value)
			{
				empty = value;
				Invalidate();
				OnPropertyChanged("Empty");
			}
		}
	}

	[Description("数据为空显示文字")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? EmptyText
	{
		get
		{
			return emptyText;
		}
		set
		{
			if (!(emptyText == value))
			{
				emptyText = value;
				Invalidate();
				OnPropertyChanged("EmptyText");
			}
		}
	}

	[Description("数据为空显示图片")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Image? EmptyImage { get; set; }

	/// <summary>
	/// 空是否显示表头
	/// </summary>
	[Description("空是否显示表头")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool EmptyHeader
	{
		get
		{
			return emptyHeader;
		}
		set
		{
			if (emptyHeader != value)
			{
				emptyHeader = value;
				if (LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("EmptyHeader");
			}
		}
	}

	/// <summary>
	/// 表格行悬浮背景色
	/// </summary>
	[Description("表格行悬浮背景色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? RowHoverBg
	{
		get
		{
			return rowHoverBg;
		}
		set
		{
			if (!(rowHoverBg == value))
			{
				rowHoverBg = value;
				OnPropertyChanged("RowHoverBg");
			}
		}
	}

	/// <summary>
	/// 表格行选中背景色
	/// </summary>
	[Description("表格行选中背景色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? RowSelectedBg
	{
		get
		{
			return rowSelectedBg;
		}
		set
		{
			if (!(rowSelectedBg == value))
			{
				rowSelectedBg = value;
				if (selectedIndex.Length != 0)
				{
					Invalidate();
				}
				OnPropertyChanged("RowSelectedBg");
			}
		}
	}

	/// <summary>
	/// 表格行选中字色
	/// </summary>
	[Description("表格行选中字色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? RowSelectedFore
	{
		get
		{
			return rowSelectedFore;
		}
		set
		{
			if (!(rowSelectedFore == value))
			{
				rowSelectedFore = value;
				if (selectedIndex.Length != 0)
				{
					Invalidate();
				}
				OnPropertyChanged("RowSelectedFore");
			}
		}
	}

	/// <summary>
	/// 边框宽度
	/// </summary>
	[Description("边框宽度")]
	[Category("边框")]
	[DefaultValue(1f)]
	public float BorderWidth
	{
		get
		{
			return borderWidth;
		}
		set
		{
			if (borderWidth != value)
			{
				borderWidth = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 表格边框颜色
	/// </summary>
	[Description("表格边框颜色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			if (!(borderColor == value))
			{
				borderColor = value;
				Invalidate();
				OnPropertyChanged("BorderColor");
			}
		}
	}

	/// <summary>
	/// 单元格边框宽度
	/// </summary>
	[Description("单元格边框宽度")]
	[Category("边框")]
	[DefaultValue(1f)]
	public float BorderCellWidth { get; set; } = 1f;

	/// <summary>
	/// 高精度边框
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("use BorderRenderMode")]
	[Description("高精度边框")]
	[Category("边框")]
	[DefaultValue(null)]
	public bool? BorderHigh
	{
		get
		{
			return borderHigh;
		}
		set
		{
			if (borderHigh != value)
			{
				borderHigh = value;
				if (borderHigh.HasValue)
				{
					BorderRenderMode = ((!borderHigh.Value) ? TableBorderMode.Pen : TableBorderMode.High);
				}
				else
				{
					BorderRenderMode = TableBorderMode.None;
				}
			}
		}
	}

	[Description("边框渲染模式")]
	[Category("边框")]
	[DefaultValue(TableBorderMode.None)]
	public TableBorderMode BorderRenderMode { get; set; }

	/// <summary>
	/// 表头字体
	/// </summary>
	[Description("表头字体")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public Font? ColumnFont
	{
		get
		{
			return columnfont;
		}
		set
		{
			if (columnfont != value)
			{
				columnfont = value;
				Invalidate();
				OnPropertyChanged("ColumnFont");
			}
		}
	}

	/// <summary>
	/// 表头背景色
	/// </summary>
	[Description("表头背景色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ColumnBack
	{
		get
		{
			return columnback;
		}
		set
		{
			if (!(columnback == value))
			{
				columnback = value;
				Invalidate();
				OnPropertyChanged("ColumnBack");
			}
		}
	}

	/// <summary>
	/// 表头文本色
	/// </summary>
	[Description("表头文本色")]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ColumnFore
	{
		get
		{
			return columnfore;
		}
		set
		{
			if (!(columnfore == value))
			{
				columnfore = value;
				Invalidate();
				OnPropertyChanged("ColumnFore");
			}
		}
	}

	/// <summary>
	/// 选中行（1开始）
	/// </summary>
	[Browsable(false)]
	[Description("选中行（1开始）")]
	[Category("Data")]
	[DefaultValue(-1)]
	public int SelectedIndex
	{
		get
		{
			if (selectedIndex.Length != 0)
			{
				return selectedIndex[0];
			}
			return -1;
		}
		set
		{
			if (SetIndex(value))
			{
				Invalidate();
				OnUpdateSummaries();
				OnPropertyChanged("SelectedIndex");
				OnSelectIndexChanged();
			}
		}
	}

	/// <summary>
	/// 选中多行
	/// </summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor", typeof(UITypeEditor))]
	[Browsable(false)]
	[Description("选中多行")]
	[Category("Data")]
	public int[] SelectedIndexs
	{
		get
		{
			return selectedIndex;
		}
		set
		{
			if (selectedIndex != value)
			{
				selectedIndex = value;
				Invalidate();
				OnUpdateSummaries();
				OnSelectIndexChanged();
				OnPropertyChanged("SelectedIndexs");
			}
		}
	}

	/// <summary>
	/// 多选行
	/// </summary>
	[Description("多选行")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool MultipleRows { get; set; }

	/// <summary>
	/// 当前视图的数据行数
	/// </summary>
	public int DisplayRowCount
	{
		get
		{
			if (dataTmp == null || dataTmp.rows.Length == 0)
			{
				return 0;
			}
			int num = 0;
			IRow[] array = dataTmp.rows;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].add)
				{
					num++;
				}
			}
			return num;
		}
	}

	[Description("是否启用内置汇总定制功能（可右键菜单自定义切换)")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool SummaryCustomize
	{
		get
		{
			return summaryCustomize;
		}
		set
		{
			if (summaryCustomize == value)
			{
				return;
			}
			summaryCustomize = value;
			if (value)
			{
				Column[] summaryColumns = SummaryColumns;
				if ((summaryColumns == null || summaryColumns.Length == 0) && columns != null && columns.Count > 0)
				{
					columns[0].SetSummaryItem(string.Empty);
				}
				OnUpdateSummaries();
			}
			else
			{
				Summary = null;
			}
			SummaryCustomizeChanged?.Invoke(this, new BoolEventArgs(value));
		}
	}

	/// <summary>
	/// 默认是否展开
	/// </summary>
	[Description("默认是否展开")]
	[Category("Appearance")]
	[DefaultValue(false)]
	public bool DefaultExpand { get; set; }

	/// <summary>
	/// 处理快捷键
	/// </summary>
	[Description("处理快捷键")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool HandShortcutKeys { get; set; } = true;

	/// <summary>
	/// 显示复选背景
	/// </summary>
	[Description("显示复选背景")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool ShowCheckBg { get; set; } = true;

	/// <summary>
	/// 省略文字提示
	/// </summary>
	[Description("省略文字提示")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool ShowTip { get; set; } = true;

	/// <summary>
	/// 筛选实时生效
	/// </summary>
	[Description("筛选实时生效")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool FilterRealTime { get; set; }

	/// <summary>
	/// 筛选显示复选背景
	/// </summary>
	[Description("筛选显示复选背景")]
	[Category("Behavior")]
	[DefaultValue(null)]
	public bool? FilterShowCheckBg { get; set; }

	/// <summary>
	/// 筛选启用排序
	/// </summary>
	[Description("筛选启用排序")]
	[Category("Behavior")]
	[DefaultValue(null)]
	public bool? FilterSortOrder { get; set; }

	/// <summary>
	/// 超出文字提示配置
	/// </summary>
	[Browsable(false)]
	[Description("超出文字提示配置")]
	[Category("Behavior")]
	[DefaultValue(null)]
	public TooltipConfig? TooltipConfig { get; set; }

	/// <summary>
	/// 编辑模式
	/// </summary>
	[Description("编辑模式")]
	[Category("Behavior")]
	[DefaultValue(TEditMode.None)]
	public TEditMode EditMode
	{
		get
		{
			return editmode;
		}
		set
		{
			if (editmode != value)
			{
				editmode = value;
				Invalidate();
				OnPropertyChanged("EditMode");
			}
		}
	}

	/// <summary>
	/// 编辑模式下的默认文本选择动作
	/// </summary>
	[Description("编辑模式下的默认文本选择动作")]
	[Category("Behavior")]
	[DefaultValue(TEditSelection.None)]
	public TEditSelection EditSelection { get; set; }

	/// <summary>
	/// 编辑模式输入框样式
	/// </summary>
	[Description("编辑模式输入框样式")]
	[Category("Behavior")]
	[DefaultValue(TEditInputStyle.Default)]
	public TEditInputStyle EditInputStyle { get; set; }

	/// <summary>
	/// 编辑模式自动高度
	/// </summary>
	[Description("编辑模式自动高度")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool EditAutoHeight { get; set; }

	/// <summary>
	/// 失去焦点退出编辑模式
	/// </summary>
	[Description("失去焦点退出编辑模式")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool EditLostFocus { get; set; } = true;

	/// <summary>
	/// 树表格的箭头样式
	/// </summary>
	[Description("树表格的箭头样式")]
	[Category("Behavior")]
	[DefaultValue(TableTreeStyle.Button)]
	public TableTreeStyle TreeArrowStyle { get; set; }

	/// <summary>
	/// 动画时长（ms）
	/// </summary>
	[Description("动画时长（ms）")]
	[Category("Behavior")]
	[DefaultValue(100)]
	public int AnimationTime { get; set; } = 100;

	[Browsable(false)]
	[Description("暂停布局")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool PauseLayout
	{
		get
		{
			return pauseLayout;
		}
		set
		{
			if (pauseLayout != value)
			{
				pauseLayout = value;
				if (!value && LoadLayout())
				{
					Invalidate();
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	private bool IsPauseLayout
	{
		get
		{
			if (!pauseLayout)
			{
				return pauseLayoutByCheckAll;
			}
			return true;
		}
	}

	/// <summary>
	/// 获取指定行的数据
	/// </summary>
	/// <param name="index">序号</param>
	/// <returns>行</returns>
	public IRow? this[int index] => GetRow(index);

	/// <summary>
	/// 行总数
	/// </summary>
	public int RowCount => dataLen;

	/// <summary>
	/// 已启用筛选的列
	/// </summary>
	public Column[]? FilterColumns
	{
		get
		{
			if (columns == null)
			{
				return null;
			}
			List<Column> list = new List<Column>(columns.Count);
			foreach (Column column in columns)
			{
				if (column.Filter != null && column.Filter.Enabled)
				{
					list.Add(column);
				}
			}
			if (list.Count > 0)
			{
				return list.ToArray();
			}
			return null;
		}
	}

	/// <summary>
	/// 按当前顺序返回可见的列集合
	/// </summary>
	public Column[]? VisibleColumns
	{
		get
		{
			if (columns == null)
			{
				return null;
			}
			List<Column> list = new List<Column>(columns.Count);
			foreach (Column column in columns)
			{
				if (column.Visible)
				{
					list.Add(column);
				}
			}
			return list.ToArray();
		}
	}

	/// <summary>
	/// 是否启用焦点自动跳转
	/// </summary>
	[Description("是否启用焦点自动跳转")]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool EnableFocusNavigation { get; set; }

	/// <summary>
	/// 焦点跳转时自动选中行（行背景色跟随）
	/// </summary>
	[Description("焦点跳转时自动选中行")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool FocusNavigationAutoSelectRow { get; set; } = true;

	/// <summary>
	/// 焦点跳转时自动滚动到新行
	/// </summary>
	[Description("焦点跳转时自动滚动到新行")]
	[Category("Behavior")]
	[DefaultValue(true)]
	public bool FocusNavigationAutoScroll { get; set; } = true;

	public Rectangle RectRead => rect_read;

	/// <summary>
	/// 滚动条真实纵向滚动值（仅虚拟模式；已扣除基准偏移，始终为小数值，避免 GDI+ 大数平移导致模糊）
	/// </summary>
	public int ScrollBarRealY => ScrollBar.ValueY - virtualMode_Y;

	protected override bool CanMouseMove { get; set; } = true;

	public override Rectangle ReadRectangle => ClientRectangle.PaddingRect(Padding, Dpi, borderWidth / 2f);

	public override GraphicsPath RenderRegion
	{
		get
		{
			if (rows == null)
			{
				GraphicsPath graphicsPath = new GraphicsPath();
				graphicsPath.AddRectangle(ReadRectangle);
				return graphicsPath;
			}
			if (radius > 0)
			{
				if (visibleHeader)
				{
					return rect_divider.RoundPath((float)radius * Dpi, roundTL: true, roundTR: true, roundBR: false, roundBL: false);
				}
				return rect_divider.RoundPath((float)radius * Dpi);
			}
			GraphicsPath graphicsPath2 = new GraphicsPath();
			graphicsPath2.AddRectangle(rect_divider);
			return graphicsPath2;
		}
	}

	/// <summary>
	/// 堆叠表头行
	/// </summary>
	[Browsable(false)]
	[Description("堆叠表头行")]
	[Category("Data")]
	[DefaultValue(null)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public StackedHeaderRow[]? StackedHeaderRows { get; set; }

	public bool SummaryRowFocused => FMenuStrip != null;

	/// <summary>
	/// 已启用汇总的列
	/// </summary>
	public Column[]? SummaryColumns
	{
		get
		{
			if (columns == null)
			{
				return null;
			}
			List<Column> list = new List<Column>(columns.Count);
			foreach (Column column in columns)
			{
				if (column.SummaryItem != null && column.SummaryItem.SummaryType != TSummaryType.None)
				{
					list.Add(column);
				}
			}
			if (list.Count > 0)
			{
				return list.ToArray();
			}
			return null;
		}
	}

	/// <summary>
	/// Checked 属性值更改时发生
	/// </summary>
	[Description("Checked 属性值更改时发生")]
	[Category("Behavior")]
	public event CheckEventHandler? CheckedChanged;

	/// <summary>
	/// 全局 CheckState 属性值更改时发生
	/// </summary>
	[Description("全局 CheckState 属性值更改时发生")]
	[Category("Behavior")]
	public event CheckStateEventHandler? CheckedOverallChanged;

	/// <summary>
	/// 单击时发生
	/// </summary>
	[Description("单击时发生")]
	[Category("Behavior")]
	public event ClickEventHandler? CellClick;

	/// <summary>
	/// 双击时发生
	/// </summary>
	[Description("双击时发生")]
	[Category("Behavior")]
	public event ClickEventHandler? CellDoubleClick;

	/// <summary>
	/// 单击空白时发生
	/// </summary>
	[Description("单击空白时发生")]
	[Category("Behavior")]
	public event MouseEventHandler? NonCellClick;

	/// <summary>
	/// 双击空白时发生
	/// </summary>
	[Description("双击空白时发生")]
	[Category("Behavior")]
	public event MouseEventHandler? NonCellDoubleClick;

	/// <summary>
	/// 单击之前发生
	/// </summary>
	[Description("单击之前发生")]
	[Category("Behavior")]
	public event ClickBeginEventHandler? CellClickBegin;

	/// <summary>
	/// 滑动时发生
	/// </summary>
	[Description("滑动时发生")]
	[Category("Behavior")]
	public event HoverEventHandler? CellHover;

	/// <summary>
	/// 单击按钮时发生
	/// </summary>
	[Description("单击按钮时发生")]
	[Category("Behavior")]
	public event ClickButtonEventHandler? CellButtonClick;

	/// <summary>
	/// 按下按钮时发生
	/// </summary>
	[Description("按下按钮时发生")]
	[Category("Behavior")]
	public event ClickButtonEventHandler? CellButtonDown;

	/// <summary>
	/// 放下按钮时发生
	/// </summary>
	[Description("放下按钮时发生")]
	[Category("Behavior")]
	public event ClickButtonEventHandler? CellButtonUp;

	/// <summary>
	/// 单元格焦点变更后发生
	/// </summary>
	[Description("单元格焦点变更后发生")]
	[Category("Behavior")]
	public event CellFocusedEventHandler? CellFocused;

	/// <summary>
	/// 编辑前发生
	/// </summary>
	[Description("编辑前发生")]
	[Category("Behavior")]
	public event BeginEditEventHandler? CellBeginEdit;

	/// <summary>
	/// 编辑前文本框样式发生
	/// </summary>
	[Description("编辑前文本框样式发生")]
	[Category("Behavior")]
	public event BeginEditInputStyleEventHandler? CellBeginEditInputStyle;

	/// <summary>
	/// 编辑后发生
	/// </summary>
	[Description("编辑后发生")]
	[Category("Behavior")]
	public event EndEditEventHandler? CellEndEdit;

	/// <summary>
	/// 编辑后发生
	/// </summary>
	[Description("编辑后发生")]
	[Category("Behavior")]
	public event EndValueEditEventHandler? CellEndValueEdit;

	/// <summary>
	/// 编辑完成后发生
	/// </summary>
	[Description("编辑完成后发生")]
	[Category("Behavior")]
	public event EndEditCompleteEventHandler? CellEditComplete;

	/// <summary>
	/// 单元格按下回车键时发生
	/// </summary>
	[Description("单元格输入模式下按下回车键时发生")]
	[Category("Behavior")]
	public event CellEditEnterEventHandler? CellEditEnter;

	/// <summary>
	/// 绘制行时发生
	/// </summary>
	[Description("绘制行时发生")]
	[Category("Behavior")]
	public event CellPaintRowEventHandler? RowPaint;

	/// <summary>
	/// 绘制行前发生
	/// </summary>
	[Description("绘制行前发生")]
	[Category("Behavior")]
	public event CellPaintRowEventHandler? RowPaintBegin;

	/// <summary>
	/// 绘制单元格时发生
	/// </summary>
	[Description("绘制单元格时发生")]
	[Category("Behavior")]
	public event CellPaintEventHandler? CellPaint;

	/// <summary>
	/// 绘制单元格之前发生
	/// </summary>
	[Description("绘制单元格之前发生")]
	[Category("Behavior")]
	public event CellPaintBeginEventHandler? CellPaintBegin;

	/// <summary>
	/// 设置行样式
	/// </summary>
	public event SetRowStyleEventHandler? SetRowStyle;

	/// <summary>
	/// 选中变化后发生
	/// </summary>
	[Description("选中变化后发生")]
	[Category("Behavior")]
	public event EventHandler? SelectIndexChanged;

	/// <summary>
	/// 行排序时发生
	/// </summary>
	[Description("行排序时发生")]
	[Category("Behavior")]
	public event IntEventHandler? SortRows;

	/// <summary>
	/// 树行排序时发生
	/// </summary>
	[Description("树行排序时发生")]
	[Category("Behavior")]
	public event SortTreeEventHandler? SortRowsTree;

	/// <summary>
	/// 点击排序后发生
	/// </summary>
	[Description("点击排序后发生")]
	[Category("Behavior")]
	public event SortModeEventHandler? SortModeChanged;

	/// <summary>
	/// 列拖放到新位置时发生 (Cancel=true时取消)
	/// </summary>
	[Description("列拖放到新位置时发生 (Cancel=true时取消)")]
	[Category("Behavior")]
	public event ColumnIndexChangingEventHandler? ColumnIndexChanging;

	/// <summary>
	/// 列拖放到新位置后发生
	/// </summary>
	[Description("列拖放到新位置后发生")]
	[Category("Behavior")]
	public event ColumnIndexChangedEventHandler? ColumnIndexChanged;

	/// <summary>
	/// 自定义排序
	/// </summary>
	[Description("自定义排序")]
	[Category("Behavior")]
	public event Comparison<string>? CustomSort;

	/// <summary>
	/// 展开改变时发生
	/// </summary>
	[Description("展开改变时发生")]
	[Category("Behavior")]
	public event ExpandEventHandler? ExpandChanged;

	[Description("筛选窗口弹出前发生")]
	[Category("Behavior")]
	public event TableFilterPopupBeginEventHandler? FilterPopupBegin;

	/// <summary>
	/// 筛选窗口关闭前发生
	/// </summary>
	[Description("筛选窗口关闭前发生")]
	[Category("Behavior")]
	public event TableFilterPopupEndEventHandler? FilterPopupEnd;

	/// <summary>
	/// 筛选数据变更后发生
	/// </summary>
	[Description("筛选数据变更后发生")]
	[Category("Behavior")]
	public event TableFilterDataChangedEventHandler? FilterDataChanged;

	/// <summary>
	/// 每行或行自定义汇总计算结束时发生
	/// </summary>
	[Description("每行或行自定义汇总计算结束时发生")]
	[Category("Data")]
	public event CustomSummaryEventHandler? CustomSummaryCalculate;

	/// <summary>
	/// 内置/外部汇总栏切换后发生
	/// </summary>
	[Description("内置/外部汇总栏切换后发生")]
	[Category("Behavior")]
	public event SummaryCustomizeChangedEventHandler? SummaryCustomizeChanged;

	private void SetSAH()
	{
		if (scrollBarAvoidHeader && visibleHeader && fixedHeader)
		{
			ScrollBar.RT = false;
		}
		else
		{
			ScrollBar.RT = true;
		}
	}

	public void SetFocusedCell(CELL? value)
	{
		if (value == null)
		{
			if (focusedxy != null)
			{
				focusedxy = null;
				Invalidate();
			}
		}
		else if (focusedxy == null || focusedxy[0] != value.INDEX || focusedxy[1] != value.ROW.INDEX)
		{
			focusedxy = new int[2]
			{
				value.INDEX,
				value.ROW.INDEX
			};
			OnCellFocused(value.ROW.RECORD, value.ROW.Type, value.ROW.INDEX, value.INDEX, value.COLUMN, value.RECT);
			Invalidate();
		}
	}

	private bool SetIndex(int value)
	{
		if (selectedIndex.Length < 1 && value == -1)
		{
			return false;
		}
		if (value == -1)
		{
			if (selectedIndex.Length == 0 || string.Join("", selectedIndex) == value.ToString())
			{
				return false;
			}
			selectedIndex = new int[0];
			return true;
		}
		if (string.Join("", selectedIndex) == value.ToString())
		{
			return false;
		}
		selectedIndex = new int[1] { value };
		if (focusedxy != null && dataLen >= value)
		{
			focusedxy[1] = value;
		}
		return true;
	}

	private int[] SetIndexs(int value)
	{
		List<int> list = new List<int>(selectedIndex.Length + 1);
		list.AddRange(selectedIndex);
		if (list.Contains(value))
		{
			list.Remove(value);
		}
		else
		{
			list.Add(value);
		}
		return list.ToArray();
	}

	private int[] SetIndexs(int start, int end)
	{
		List<int> list = new List<int>(end - start + 1);
		for (int i = start; i <= end; i++)
		{
			list.Add(i);
		}
		return list.ToArray();
	}

	public Table()
		: base(ControlType.Select)
	{
		ScrollBar = new ScrollBar(this, enabledY: true, enabledX: true, radius, !visibleHeader);
	}

	protected override void Dispose(bool disposing)
	{
		ThreadState?.Dispose();
		ScrollBar.Dispose();
		base.Dispose(disposing);
	}

	/// <summary>
	/// 选中真实行
	/// </summary>
	public int[] SelectedIndexsReal()
	{
		if (SortData == null || selectedIndex.Length < 1)
		{
			return selectedIndex;
		}
		if (selectedIndex.Length == 1 && selectedIndex[0] == 0)
		{
			return selectedIndex;
		}
		List<int> list = new List<int>(SortData.Length);
		int[] array = selectedIndex;
		foreach (int num in array)
		{
			list.Add(SortData[num - 1] + 1);
		}
		return list.ToArray();
	}

	/// <summary>
	/// 选中真实行数据
	/// </summary>
	public object[] SelectedsReal()
	{
		if (rows == null || selectedIndex.Length < 1)
		{
			return new object[0];
		}
		if (selectedIndex.Length == 1 && selectedIndex[0] == 0)
		{
			return new object[0];
		}
		Dictionary<int, object> dictionary = rows.RealDir();
		List<object> list = new List<object>(dictionary.Count);
		if (SortData == null)
		{
			int[] array = selectedIndex;
			foreach (int num in array)
			{
				if (dictionary.TryGetValue(num - 1, out var value))
				{
					list.Add(value);
				}
			}
		}
		else
		{
			int[] array = selectedIndex;
			foreach (int num2 in array)
			{
				if (dictionary.TryGetValue(SortData[num2 - 1], out var value2))
				{
					list.Add(value2);
				}
			}
		}
		return list.ToArray();
	}

	/// <summary>
	/// 获取指定行的数据
	/// </summary>
	/// <param name="index">序号</param>
	/// <returns>行</returns>
	public IRow? GetRow(int index)
	{
		if (dataTmp == null || dataTmp.rows.Length == 0)
		{
			return null;
		}
		if (index < 0 || dataTmp.rows.Length - 1 < index)
		{
			return null;
		}
		if (SortData == null || SortData.Length - 1 < index)
		{
			return dataTmp.rows[index];
		}
		return dataTmp.rows[SortData[index]];
	}

	public override void Refresh()
	{
		ExtractHeaderFixed();
		if (dataSource == null || dataSource is DataTable || dataSource is IList)
		{
			ExtractData();
		}
		base.Refresh();
		if (LoadLayout())
		{
			Invalidate();
		}
	}

	public void Refresh<T>(AntList<T>? list = null)
	{
		if (list == null)
		{
			Refresh();
			return;
		}
		IBinding(list);
		base.Refresh();
	}

	/// <summary>
	/// 设置选中行
	/// </summary>
	/// <param name="record">行数据</param>
	/// <param name="expand">是否展开关联父级</param>
	public void SetSelected(object record, bool expand = false)
	{
		if (rows == null)
		{
			return;
		}
		for (int i = 0; i < rows.Length; i++)
		{
			RowTemplate rowTemplate = rows.Record(record);
			if (rowTemplate == null)
			{
				break;
			}
			SelectedIndex = rowTemplate.INDEX;
			if (expand && !rowTemplate.SHOW)
			{
				ExpandUp(rows, i);
				if (LoadLayout())
				{
					Invalidate();
				}
			}
		}
	}

	private void ExpandUp(RowList rows, int i)
	{
		RowTemplate rowTemplate = rows[i];
		if (rowTemplate == null)
		{
			return;
		}
		int num = rowTemplate.Depth - 1;
		while (i >= 1)
		{
			i--;
			rowTemplate = rows[i];
			if (rowTemplate == null)
			{
				break;
			}
			if (rowTemplate.CanExpand && rowTemplate.Depth == num)
			{
				if (rowTemplate.RD != null)
				{
					rowTemplate.RD.expand = true;
					rowTemplate.RD.SetValue("__EXPAND__", true);
				}
				num = rowTemplate.Depth - 1;
				if (num < 0)
				{
					break;
				}
			}
		}
	}

	/// <summary>
	/// 获取行使能
	/// </summary>
	/// <param name="i">行</param>
	/// <returns>是否禁用</returns>
	public bool GetRowEnable(int i)
	{
		RowTemplate rowTemplate = rows?[i];
		if (rowTemplate == null)
		{
			return true;
		}
		return GetRowEnable(rowTemplate.RECORD);
	}

	/// <summary>
	/// 获取行使能
	/// </summary>
	/// <param name="record">行对象</param>
	/// <returns>是否禁用</returns>
	public bool GetRowEnable(object record)
	{
		if (enableDir.Contains(record))
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// 设置行使能
	/// </summary>
	/// <param name="i">行</param>
	/// <param name="value">值</param>
	/// <param name="ui">是否刷新ui</param>
	public void SetRowEnable(int i, bool value = true, bool ui = true)
	{
		RowTemplate rowTemplate = rows?[i];
		if (rowTemplate != null)
		{
			SetRowEnable(rowTemplate.RECORD, value, ui);
		}
	}

	/// <summary>
	/// 设置行使能
	/// </summary>
	/// <param name="record">行对象</param>
	/// <param name="value">值</param>
	/// <param name="ui">是否刷新ui</param>
	public void SetRowEnable(object record, bool value = true, bool ui = true)
	{
		if (value)
		{
			if (!enableDir.Contains(record))
			{
				return;
			}
			enableDir.Remove(record);
		}
		else
		{
			if (enableDir.Contains(record))
			{
				return;
			}
			enableDir.Add(record);
		}
		try
		{
			RowTemplate rowTemplate = rows?.Record(record);
			if (rowTemplate != null && rowTemplate.RECORD == record)
			{
				rowTemplate.ENABLE = value;
				if (ui)
				{
					Invalidate();
				}
			}
		}
		catch
		{
		}
	}

	/// <summary>
	/// 滚动到指定行
	/// </summary>
	/// <param name="i">行</param>
	/// <param name="force">是否强制滚动</param>
	/// <returns>返回滚动量</returns>
	public int ScrollLine(int i, bool force = false)
	{
		if (rows == null || !ScrollBar.ShowY)
		{
			return 0;
		}
		return ScrollLine(i, rows, force);
	}

	/// <summary>
	/// 滚动到指定行
	/// </summary>
	/// <param name="record">行对象</param>
	/// <param name="force">是否强制滚动</param>
	/// <returns>返回滚动量</returns>
	public int ScrollLine(object record, bool force = false)
	{
		if (rows == null)
		{
			return 0;
		}
		RowTemplate rowTemplate = rows.Record(record);
		if (rowTemplate == null)
		{
			if (VirtualMode && _RowHeight.HasValue)
			{
				if (dataTmp == null)
				{
					return 0;
				}
				int num = dataTmp.FindRecord(record);
				int num2 = dataTmp.rows.Length * _RowHeight.Value;
				float num3 = (float)(_RowHeight.Value * num) * 1f / (float)num2;
				int num4 = (int)Math.Round((float)num2 * num3);
				return ScrollLine(num4, num4 + _RowHeight.Value, RealRegionRow(rows.List), force);
			}
			return 0;
		}
		return ScrollLine(rowTemplate.INDEX, rows, force);
	}

	/// <summary>
	/// 内容滚动到最下面
	/// </summary>
	public void ScrollToEnd()
	{
		if (ScrollBar.ShowY)
		{
			ScrollBar.Value = ScrollBar.Max;
		}
	}

	private int ScrollLine(int i, RowList rows, bool force = false)
	{
		if (VirtualMode && _RowHeight.HasValue)
		{
			if (dataTmp == null)
			{
				return 0;
			}
			int num = dataTmp.rows.Length * _RowHeight.Value;
			float num2 = (float)(_RowHeight.Value * i) * 1f / (float)num;
			int num3 = (int)Math.Round((float)num * num2);
			return ScrollLine(num3, num3 + _RowHeight.Value, RealRegionRow(rows.List), force);
		}
		RowTemplate rowTemplate = rows[i];
		if (rowTemplate == null)
		{
			return 0;
		}
		return ScrollLine(rowTemplate.RECT, RealRegionRow(rows.List), force);
	}

	private Rectangle RealRegionRow(RowTemplate[] rows)
	{
		int num = rect_read.X;
		int num2 = rect_read.Width;
		int bottom = rect_read.Y;
		int num3 = rect_read.Height;
		if (fixedHeader && visibleHeader)
		{
			Rectangle rECT = rows[0].RECT;
			bottom = rECT.Bottom;
			num3 -= rECT.Height;
		}
		if (sFixedB != -1 && rect_fixed_B.HasValue)
		{
			num3 -= rect_fixed_B.Value.Height;
		}
		return new Rectangle(num, bottom, num2, num3);
	}

	private int ScrollLine(Rectangle rect_cel, Rectangle rect, bool force = false)
	{
		return ScrollLine(rect_cel.Y + virtualMode_Y, rect_cel.Bottom + virtualMode_Y, rect, force);
	}

	private int ScrollLine(int y, int b, Rectangle rect, bool force = false)
	{
		if (ScrollBar.ShowY)
		{
			int valueY = ScrollBar.ValueY;
			int num = valueY + rect.Y;
			int num2 = num + rect.Height;
			if (force || y < num || b > num2)
			{
				ScrollBar.ValueY = y - rect.Y;
				return valueY - ScrollBar.ValueY;
			}
		}
		return 0;
	}

	/// <summary>
	/// 滚动到指定列
	/// </summary>
	/// <param name="i">列</param>
	/// <param name="force">是否强制滚动</param>
	/// <returns>返回滚动量</returns>
	public int ScrollColumn(int i, bool force = false)
	{
		if (rows == null)
		{
			return 0;
		}
		CELL cELL = rows.First.cells[i];
		Rectangle rECT = cELL.RECT;
		return ScrollColumn(rECT, RealRegionFixedColumn(cELL.COLUMN.INDEX_REAL), force);
	}

	/// <summary>
	/// 滚动到指定列
	/// </summary>
	/// <param name="column">表头 key</param>
	/// <param name="force">是否强制滚动</param>
	/// <param name="selectCell">是否选中为焦点单元格</param>
	/// <returns>返回滚动量</returns>
	public int ScrollColumn(string column, bool force = false, bool select = false)
	{
		if (select)
		{
			Column columnByFieldKey = GetColumnByFieldKey(column);
			if (columnByFieldKey == null)
			{
				return 0;
			}
			return ScrollColumn(columnByFieldKey, force, select);
		}
		if (rows == null)
		{
			return 0;
		}
		CELL[] cells = rows.First.cells;
		foreach (CELL cELL in cells)
		{
			if (cELL.COLUMN.Key == column)
			{
				return ScrollColumn(cELL.RECT, RealRegionFixedColumn(cELL.COLUMN.INDEX_REAL), force);
			}
		}
		return 0;
	}

	/// <summary>
	/// 滚动到指定列
	/// </summary>
	/// <param name="column">表头</param>
	/// <param name="force">是否强制滚动</param>
	/// <param name="select">是否选中为焦点单元格</param>
	/// <returns>返回滚动量</returns>
	public int ScrollColumn(Column column, bool force = false, bool select = false)
	{
		CELL[] cells;
		if (select)
		{
			if (rows == null || rows.Length == 0)
			{
				return 0;
			}
			int index = ((focusedxy != null && focusedxy.Length >= 2) ? focusedxy[1] : 0);
			RowTemplate rowTemplate = rows[index];
			if (rowTemplate == null)
			{
				return 0;
			}
			cells = rowTemplate.cells;
			foreach (CELL cELL in cells)
			{
				if (cELL.COLUMN == column)
				{
					int result = ScrollColumn(cELL.RECT, RealRegionFixedColumn(column.INDEX_REAL), force);
					if (select)
					{
						SetFocusedCell(cELL);
					}
					return result;
				}
			}
			return 0;
		}
		if (rows == null)
		{
			return 0;
		}
		cells = rows.First.cells;
		foreach (CELL cELL2 in cells)
		{
			if (cELL2.COLUMN == column)
			{
				return ScrollColumn(cELL2.RECT, RealRegionFixedColumn(column.INDEX_REAL), force);
			}
		}
		return 0;
	}

	private Rectangle RealRegionFixedColumn(int col)
	{
		int right = rect_read.X;
		int num = rect_read.Width;
		int num2 = rect_read.Y;
		int num3 = rect_read.Height;
		if (showFixedColumnL && rect_fixed_L.HasValue && !fixedColumnL.Contains(col))
		{
			right = rect_fixed_L.Value.Right;
			num -= rect_fixed_L.Value.Width;
		}
		if (showFixedColumnR && rect_fixed_R.HasValue && !fixedColumnR.Contains(col))
		{
			num -= rect_fixed_R.Value.Width;
		}
		return new Rectangle(right, num2, num, num3);
	}

	private int ScrollColumn(Rectangle rect_cel, Rectangle rect, bool force = false)
	{
		return ScrollColumn(rect_cel.X, rect_cel.Right, rect, force);
	}

	private int ScrollColumn(int x, int r, Rectangle rect, bool force = false)
	{
		if (ScrollBar.ShowX)
		{
			int valueX = ScrollBar.ValueX;
			int num = valueX + rect.X;
			int num2 = num + rect.Width;
			if (force || x < num || r > num2)
			{
				ScrollBar.ValueX = x - rect.X;
				return valueX - ScrollBar.ValueX;
			}
		}
		return 0;
	}

	/// <summary>
	/// 复制表格数据
	/// </summary>
	/// <param name="row">行</param>
	public bool CopyData(int row)
	{
		try
		{
			RowTemplate rowTemplate = rows?[row];
			if (rowTemplate == null)
			{
				return false;
			}
			List<string> list = new List<string>(rowTemplate.cells.Length);
			CELL[] cells = rowTemplate.cells;
			foreach (CELL cELL in cells)
			{
				list.Add(cELL.ToString());
			}
			this.ClipboardSetText(string.Join("\t", list));
			return true;
		}
		catch
		{
		}
		return false;
	}

	/// <summary>
	/// 复制表格数据
	/// </summary>
	/// <param name="row">行</param>
	public bool CopyData(int[] row)
	{
		if (rows != null)
		{
			try
			{
				List<string> list = new List<string>(row.Length);
				foreach (int index in row)
				{
					RowTemplate rowTemplate = rows[index];
					if (rowTemplate != null)
					{
						List<string> list2 = new List<string>(rowTemplate.cells.Length);
						CELL[] cells = rowTemplate.cells;
						foreach (CELL cELL in cells)
						{
							list2.Add(cELL.ToString());
						}
						list.Add(string.Join("\t", list2));
					}
				}
				this.ClipboardSetText(string.Join("\n", list));
				return true;
			}
			catch
			{
			}
		}
		return false;
	}

	/// <summary>
	/// 复制表格数据
	/// </summary>
	/// <param name="row">行</param>
	/// <param name="column">列</param>
	public bool CopyData(int row, int column)
	{
		RowTemplate rowTemplate = rows?[row];
		if (rowTemplate == null)
		{
			return false;
		}
		string text = rowTemplate.cells[column].ToString();
		if (text == null)
		{
			return false;
		}
		this.ClipboardSetText(text);
		return true;
	}

	/// <summary>
	/// 复制表格数据
	/// </summary>
	public bool CopyData(CELL cell)
	{
		if (cell != null)
		{
			try
			{
				string text = cell.ToString();
				if (text == null)
				{
					return false;
				}
				this.ClipboardSetText(text);
				return true;
			}
			catch
			{
			}
		}
		return false;
	}

	/// <summary>
	/// 获取排序序号
	/// </summary>
	public int[] SortIndex()
	{
		if (SortData == null)
		{
			if (dataTmp == null || dataTmp.rows.Length == 0)
			{
				return new int[0];
			}
			return dataTmp.GetInts();
		}
		return SortData;
	}

	/// <summary>
	/// 设置排序序号
	/// </summary>
	/// <param name="data">序号数据</param>
	public void SetSortIndex(int[]? data)
	{
		SortData = data;
		if (LoadLayout())
		{
			Invalidate();
		}
	}

	/// <summary>
	/// 获取排序数据
	/// </summary>
	public object[] SortList()
	{
		if (dataTmp == null || dataTmp.rows.Length == 0)
		{
			return new object[0];
		}
		if (SortData == null)
		{
			return dataTmp.GetRecord();
		}
		return dataTmp.GetRecord(SortData);
	}

	/// <summary>
	/// 获取排序数据
	/// </summary>
	public T[] SortList<T>()
	{
		if (dataTmp == null || dataTmp.rows.Length == 0)
		{
			return new T[0];
		}
		object[] array = ((SortData == null) ? dataTmp.GetRecord() : dataTmp.GetRecord(SortData));
		List<T> list = new List<T>(array.Length);
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i] is T item)
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	/// <summary>
	/// 设置排序数据
	/// </summary>
	public void SetSortList(IList<object>? data)
	{
		if (data == null)
		{
			SortData = null;
			if (LoadLayout())
			{
				Invalidate();
			}
		}
		else
		{
			if (dataTmp == null)
			{
				return;
			}
			lock (dataTmp)
			{
				IRow[] array = dataTmp.rows;
				if (array.Length == 0)
				{
					return;
				}
				List<int> list = new List<int>(array.Length);
				foreach (object datum in data)
				{
					int? rowIndex = GetRowIndex(datum, array);
					if (rowIndex.HasValue)
					{
						list.Add(rowIndex.Value);
					}
				}
				SortData = list.ToArray();
			}
			if (LoadLayout())
			{
				Invalidate();
			}
		}
	}

	/// <summary>
	/// 按列SortMode排序
	/// </summary>
	/// <param name="col">列</param>
	public void Sort(Column col)
	{
		bool flag = SortData == null;
		if (col.SortMode == SortMode.ASC)
		{
			SortDataASC(col);
		}
		else if (col.SortMode == SortMode.DESC)
		{
			SortDataDESC(col);
		}
		else
		{
			SortData = null;
		}
		if (!flag || SortData != null)
		{
			LoadLayout();
			Invalidate();
			OnSortRows(col.INDEX);
		}
	}

	private int? GetRowIndex(object it, IRow[] rows)
	{
		for (int i = 0; i < rows.Length; i++)
		{
			if (rows[i].record == it)
			{
				return i;
			}
		}
		return null;
	}

	private int? GetRowIndexReal(object record, IRow[] rows)
	{
		foreach (IRow row in rows)
		{
			if (row.record == record)
			{
				return row.i;
			}
		}
		return null;
	}

	/// <summary>
	/// 获取表头排序序号
	/// </summary>
	public int[] SortColumnsIndex()
	{
		if (SortHeader == null)
		{
			if (columns == null || columns.Count == 0)
			{
				return new int[0];
			}
			int[] array = new int[columns.Count];
			for (int i = 0; i < columns.Count; i++)
			{
				array[i] = i;
			}
			return array;
		}
		return SortHeader;
	}

	/// <summary>
	/// 设置表头排序序号
	/// </summary>
	/// <param name="data">序号数据</param>
	public void SetSortColumnsIndex(int[]? data)
	{
		SortHeader = data;
		if (LoadLayout())
		{
			Invalidate();
		}
	}

	/// <summary>
	/// 获取表头排序数据
	/// </summary>
	public Column[] SortColumnsList()
	{
		if (columns == null || columns.Count == 0)
		{
			return new Column[0];
		}
		if (SortHeader == null)
		{
			Column[] array = new Column[columns.Count];
			for (int i = 0; i < columns.Count; i++)
			{
				array[i] = columns[i];
			}
			return array;
		}
		List<Column> list = new List<Column>(columns.Count);
		int[] sortHeader = SortHeader;
		foreach (int index in sortHeader)
		{
			list.Add(columns[index]);
		}
		return list.ToArray();
	}

	/// <summary>
	/// 获取表头真实索引
	/// </summary>
	public int GetColumnRealIndex(Column column)
	{
		return column.INDEX_REAL;
	}

	/// <summary>
	/// 获取区域
	/// </summary>
	/// <param name="row">行</param>
	/// <param name="column">列</param>
	public Rectangle CellRectangle(int row, int column)
	{
		try
		{
			RowTemplate rowTemplate = rows?[row];
			if (rowTemplate == null)
			{
				return Rectangle.Empty;
			}
			CELL cELL = rowTemplate.cells[column];
			CELLDB cELLDB = CellContains(rows.List, sethover: false, 0, 0);
			if (cELLDB == null)
			{
				return RealRect(cELL.RECT, 0, 0);
			}
			return RealRect(cELL.RECT, cELLDB.offset_xi, cELLDB.offset_y);
		}
		catch
		{
		}
		return Rectangle.Empty;
	}

	/// <summary>
	/// 导出表格数据
	/// </summary>
	/// <param name="enableRender">启用插槽</param>
	/// <param name="toString">使用toString</param>
	public DataTable? ToDataTable(bool enableRender = true, bool toString = true)
	{
		if (dataTmp == null || dataTmp.rows.Length == 0)
		{
			return null;
		}
		DataTable dt = new DataTable();
		Dictionary<string, Column> dir_columns;
		if (rows == null)
		{
			dir_columns = new Dictionary<string, Column>(0);
			TempiColumn[] array = dataTmp.columns;
			foreach (TempiColumn tempiColumn in array)
			{
				dt.Columns.Add(new DataColumn(tempiColumn.key)
				{
					Caption = tempiColumn.text
				});
			}
		}
		else
		{
			dir_columns = new Dictionary<string, Column>(dataTmp.columns.Length);
			Dictionary<string, DataColumn> dictionary = new Dictionary<string, DataColumn>(dataTmp.columns.Length);
			TempiColumn[] array = dataTmp.columns;
			foreach (TempiColumn tempiColumn2 in array)
			{
				dictionary.Add(tempiColumn2.key, new DataColumn(tempiColumn2.key)
				{
					Caption = tempiColumn2.text
				});
			}
			CELL[] cells = rows.List[0].cells;
			for (int i = 0; i < cells.Length; i++)
			{
				Column cOLUMN = cells[i].COLUMN;
				dir_columns.Add(cOLUMN.Key, cOLUMN);
				if (!string.IsNullOrWhiteSpace(cOLUMN.Title) && dictionary.TryGetValue(cOLUMN.Key, out var value))
				{
					value.Caption = cOLUMN.Title;
				}
			}
			foreach (KeyValuePair<string, DataColumn> item in dictionary)
			{
				dt.Columns.Add(item.Value);
			}
		}
		if (toString)
		{
			dataTmp.ForRow((IRow row) =>
			{
				List<object> list = new List<object>(row.cells.Count);
				foreach (KeyValuePair<string, object> cell in row.cells)
				{
					object obj = row[cell.Key];
					if (enableRender && dir_columns.TryGetValue(cell.Key, out Column value2) && value2.Render != null)
					{
						obj = value2.Render(obj, row.record, row.i);
					}
					if (obj is IList<ICell> list2)
					{
						List<string> list3 = new List<string>(list2.Count);
						foreach (ICell item2 in list2)
						{
							string text = item2.ToString();
							if (text != null)
							{
								list3.Add(text);
							}
						}
						if (list3.Count > 0)
						{
							list.Add(string.Join(" ", list3));
						}
						else
						{
							list.Add(null);
						}
					}
					else
					{
						list.Add(obj);
					}
				}
				dt.Rows.Add(list.ToArray());
			});
		}
		else
		{
			dataTmp.ForRow((IRow row) =>
			{
				List<object> list = new List<object>(row.cells.Count);
				foreach (KeyValuePair<string, object> cell2 in row.cells)
				{
					object obj = row[cell2.Key];
					if (enableRender && dir_columns.TryGetValue(cell2.Key, out Column value2) && value2.Render != null)
					{
						obj = value2.Render(obj, row.record, row.i);
					}
					list.Add(obj);
				}
				dt.Rows.Add(list.ToArray());
			});
		}
		return dt;
	}

	/// <summary>
	/// 展开全部
	/// </summary>
	/// <param name="value">展开或折叠</param>
	public void ExpandAll(bool value = true)
	{
		if (dataTmp == null)
		{
			return;
		}
		int num = 0;
		IRow[] rowsCache = dataTmp.RowsCache;
		foreach (IRow row in rowsCache)
		{
			if (row.expand != value && OnExpandChanged(row.record, value))
			{
				row.expand = value;
				if (!row.SetValue("__EXPAND__", value) || !isMVVM)
				{
					num++;
				}
			}
		}
		if (num > 0 && LoadLayout())
		{
			Invalidate();
		}
	}

	/// <summary>
	/// 展开或折叠
	/// </summary>
	/// <param name="record">元数据</param>
	/// <param name="value">展开或折叠</param>
	public void Expand(object record, bool value = true)
	{
		if (dataTmp == null || !OnExpandChanged(record, value))
		{
			return;
		}
		IRow[] rowsCache = dataTmp.RowsCache;
		foreach (IRow row in rowsCache)
		{
			if (row.record == record)
			{
				row.expand = value;
				if ((!row.SetValue("__EXPAND__", value) || !isMVVM) && LoadLayout())
				{
					Invalidate();
				}
				break;
			}
		}
	}

	public CELL? HitTest(int x, int y)
	{
		if (rows == null)
		{
			return null;
		}
		return CellContains(rows.List, sethover: false, x, y)?.cell;
	}

	public CELL? HitTest(int x, int y, out int i_row, out int i_cel)
	{
		if (rows == null)
		{
			i_row = (i_cel = 0);
			return null;
		}
		CELLDB cELLDB = CellContains(rows.List, sethover: false, x, y);
		if (cELLDB == null)
		{
			i_row = (i_cel = 0);
			return null;
		}
		i_row = cELLDB.i_row;
		i_cel = cELLDB.i_cel;
		return cELLDB.cell;
	}

	public CELL? HitTest(int x, int y, out int r_x, out int r_y, out int offset_x, out int offset_xi, out int offset_y, out int i_row, out int i_cel)
	{
		if (rows == null)
		{
			r_x = (r_y = (offset_x = (offset_xi = (offset_y = (i_row = (i_cel = 0))))));
			return null;
		}
		CELLDB cELLDB = CellContains(rows.List, sethover: false, x, y);
		if (cELLDB == null)
		{
			r_x = (r_y = (offset_x = (offset_xi = (offset_y = (i_row = (i_cel = 0))))));
			return null;
		}
		r_x = cELLDB.x;
		r_y = cELLDB.y;
		offset_x = cELLDB.offset_x;
		offset_xi = cELLDB.offset_xi;
		offset_y = cELLDB.offset_y;
		i_row = cELLDB.i_row;
		i_cel = cELLDB.i_cel;
		return cELLDB.cell;
	}

	public CELL? HitTest(int x, int y, out int r_x, out int r_y, out int offset_x, out int offset_xi, out int offset_y, out int i_row, out int i_cel, out CELLDBMode mode)
	{
		if (rows == null)
		{
			mode = CELLDBMode.None;
			r_x = (r_y = (offset_x = (offset_xi = (offset_y = (i_row = (i_cel = 0))))));
			return null;
		}
		CELLDB cELLDB = CellContains(rows.List, sethover: false, x, y);
		if (cELLDB == null)
		{
			mode = CELLDBMode.None;
			r_x = (r_y = (offset_x = (offset_xi = (offset_y = (i_row = (i_cel = 0))))));
			return null;
		}
		mode = cELLDB.mode;
		r_x = cELLDB.x;
		r_y = cELLDB.y;
		offset_x = cELLDB.offset_x;
		offset_xi = cELLDB.offset_xi;
		offset_y = cELLDB.offset_y;
		i_row = cELLDB.i_row;
		i_cel = cELLDB.i_cel;
		return cELLDB.cell;
	}

	public Bitmap? DrawBitmap(bool full)
	{
		if (full && (ScrollBar.ShowX || ScrollBar.ShowY))
		{
			Rectangle rectangle = new Rectangle(0, 0, rect_read.Width, rect_read.Height);
			if (ScrollBar.ShowX)
			{
				rectangle.Width = ScrollBar.MaxX;
			}
			if (ScrollBar.ShowY)
			{
				rectangle.Height = ScrollBar.MaxY;
			}
			if (rectangle.Width == 0 || rectangle.Height == 0)
			{
				return null;
			}
			LoadLayout(rectangle);
			Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height);
			using (Canvas canvas = Graphics.FromImage(bitmap).High(Dpi))
			{
				try
				{
					DrawEventArgs e = new DrawEventArgs(canvas, rectangle);
					OnDrawBg(e);
					OnDraw(e);
				}
				catch
				{
				}
				this.PaintBadge(canvas);
			}
			LoadLayout();
			return bitmap;
		}
		return DrawBitmap();
	}

	public void Invalidate(int row)
	{
		if (ThreadState == null && rows != null)
		{
			RowTemplate rowTemplate = rows[row];
			if (rowTemplate != null)
			{
				Rectangle rECT = rowTemplate.RECT;
				int scrollBarRealY = ScrollBarRealY;
				Invalidate(new Rectangle(rECT.X, rECT.Y - scrollBarRealY, rECT.Width, rECT.Height));
			}
		}
	}

	public void Invalidate(int row, int column)
	{
		if (ThreadState == null && rows != null)
		{
			RowTemplate rowTemplate = rows[row];
			if (rowTemplate != null)
			{
				Rectangle rECT = rowTemplate.cells[column].RECT;
				int valueX = ScrollBar.ValueX;
				int scrollBarRealY = ScrollBarRealY;
				Invalidate(new Rectangle(rECT.X - valueX, rECT.Y - scrollBarRealY, rECT.Width, rECT.Height));
			}
		}
	}

	public void AddMergedRegion(CellRange range)
	{
		if (CellRanges == null)
		{
			CellRanges = new CellRange[1] { range };
		}
		else
		{
			List<CellRange> list = new List<CellRange>(CellRanges.Length + 1);
			list.AddRange(CellRanges);
			list.Add(range);
			CellRanges = list.ToArray();
		}
		Invalidate();
	}

	public void AddMergedRegion(CellRange[] ranges)
	{
		if (CellRanges == null)
		{
			CellRanges = ranges;
		}
		else
		{
			List<CellRange> list = new List<CellRange>(CellRanges.Length + ranges.Length);
			list.AddRange(CellRanges);
			list.AddRange(ranges);
			CellRanges = list.ToArray();
		}
		Invalidate();
	}

	public bool ContainsMergedRegion(CellRange range)
	{
		if (CellRanges == null)
		{
			return false;
		}
		List<int> list = new List<int>(CellRanges.Length);
		CellRange[] cellRanges = CellRanges;
		foreach (CellRange cellRange in cellRanges)
		{
			list.Add(cellRange.GetHashCode());
		}
		return list.Contains(range.GetHashCode());
	}

	public void ClearMergedRegion()
	{
		CellRanges = null;
		Invalidate();
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		this.AddListener();
		if (dataOne)
		{
			LoadLayout();
		}
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG && ColumnsHasLocalization() && LoadLayout())
		{
			Invalidate();
		}
	}

	private bool ColumnsHasLocalization()
	{
		if (columns == null)
		{
			return false;
		}
		foreach (Column column in columns)
		{
			if (column.LocalizationTitle != null)
			{
				return true;
			}
		}
		return false;
	}

	private void ExtractData()
	{
		dataLen = 0;
		dataOne = true;
		dataTmp = null;
		if (columns != null)
		{
			foreach (Column column in columns)
			{
				if (column is ColumnCheck columnCheck)
				{
					columnCheck.CheckState = CheckState.Unchecked;
				}
			}
		}
		if (dataSource == null)
		{
			ScrollBar.Clear();
		}
		else if (dataSource is BindingSource bindingSource)
		{
			bindingSource.ListChanged -= Binding_ListChanged;
			bindingSource.ListChanged += Binding_ListChanged;
			ExtractData(bindingSource.DataSource);
		}
		else
		{
			ExtractData(dataSource);
		}
	}

	private bool ExtractDataSummary()
	{
		if (dataTmp == null || dataTmp.rowsFilter == null)
		{
			return true;
		}
		if (summary == null)
		{
			dataTmp.summary = null;
			return false;
		}
		if (dataSource is DataTable dataTable)
		{
			if (dataTable.Columns.Count > 0 && dataTable.Rows.Count > 0)
			{
				dataTmp.summary = ExtractSummary(ExtractColumn(dataTable));
			}
		}
		else if (dataSource is IList list)
		{
			TempiColumn[] array = ExtractColumn(list);
			List<IRow> list2 = new List<IRow>(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				GetRowAuto(ref list2, list[i], i, array, 0);
			}
			dataTmp.summary = ExtractSummary(array);
		}
		return false;
	}

	private bool ExtractData(object? dataSource)
	{
		if (dataSource is DataTable dataTable)
		{
			if (dataTable.Columns.Count > 0 && dataTable.Rows.Count > 0)
			{
				TempiColumn[] array = ExtractColumn(dataTable);
				List<IRow> list = new List<IRow>(dataTable.Rows.Count + 1);
				for (int i = 0; i < dataTable.Rows.Count; i++)
				{
					DataRow dataRow = dataTable.Rows[i];
					if (dataRow != null)
					{
						Dictionary<string, object> dictionary = new Dictionary<string, object>(array.Length);
						TempiColumn[] array2 = array;
						foreach (TempiColumn tempiColumn in array2)
						{
							dictionary.Add(tempiColumn.key, dataRow[tempiColumn.key]);
						}
						if (dictionary.Count > 0)
						{
							list.Add(new IRow(i, dataRow, dictionary, 0, null));
						}
					}
				}
				dataTmp = new TempTable(array, list.ToArray(), ExtractSummary(array));
				return true;
			}
		}
		else if (dataSource is IList { Count: >0 } list2)
		{
			TempiColumn[] array3 = ExtractColumn(list2);
			List<IRow> list3 = new List<IRow>(list2.Count);
			for (int k = 0; k < list2.Count; k++)
			{
				GetRowAuto(ref list3, list2[k], k, array3, 0);
			}
			dataTmp = new TempTable(array3, list3.ToArray(), ExtractSummary(array3));
			return true;
		}
		return false;
	}

	private TempiColumn[] ExtractColumn(DataTable table)
	{
		List<TempiColumn> list = new List<TempiColumn>(table.Columns.Count);
		for (int i = 0; i < table.Columns.Count; i++)
		{
			list.Add(new TempiColumn(i, table.Columns[i]));
		}
		return list.ToArray();
	}

	private TempiColumn[] ExtractColumn<T>(IList<T> list)
	{
		foreach (T item in list)
		{
			if (item != null)
			{
				return ExtractColumn(item);
			}
		}
		return new TempiColumn[0];
	}

	private TempiColumn[] ExtractColumn(IList list)
	{
		foreach (object item in list)
		{
			if (item != null)
			{
				return ExtractColumn(item);
			}
		}
		return new TempiColumn[0];
	}

	private TempiColumn[] ExtractColumn(object row)
	{
		if (row is IList<AntItem> list)
		{
			TempiColumn[] array = new TempiColumn[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = new TempiColumn(i, list[i].key);
			}
			return array;
		}
		int num = 0;
		if (row is ExpandoObject expandoObject)
		{
			List<TempiColumn> list2 = new List<TempiColumn>(((ICollection<KeyValuePair<string, object>>)expandoObject).Count);
			foreach (KeyValuePair<string, object> item in (IEnumerable<KeyValuePair<string, object>>)expandoObject)
			{
				list2.Add(new TempiColumn(num, item.Key));
				num++;
			}
			return list2.ToArray();
		}
		if (row is IDictionary<string, object> dictionary)
		{
			List<TempiColumn> list3 = new List<TempiColumn>(dictionary.Count);
			foreach (KeyValuePair<string, object> item2 in dictionary)
			{
				list3.Add(new TempiColumn(num, item2.Key));
				num++;
			}
			return list3.ToArray();
		}
		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(row);
		List<TempiColumn> list4 = new List<TempiColumn>(properties.Count);
		foreach (PropertyDescriptor item3 in properties)
		{
			list4.Add(new TempiColumn(num, item3.DisplayName ?? item3.Name));
			num++;
		}
		return list4.ToArray();
	}

	private IRow[]? ExtractSummary(TempiColumn[] columns)
	{
		if (summary == null)
		{
			return null;
		}
		if (summary is DataTable dataTable)
		{
			if (dataTable.Columns.Count > 0 && dataTable.Rows.Count > 0)
			{
				List<IRow> list = new List<IRow>(dataTable.Rows.Count);
				for (int i = 0; i < dataTable.Rows.Count; i++)
				{
					DataRow dataRow = dataTable.Rows[i];
					if (dataRow != null)
					{
						Dictionary<string, object> dictionary = new Dictionary<string, object>(columns.Length);
						foreach (TempiColumn tempiColumn in columns)
						{
							dictionary.Add(tempiColumn.key, dataRow[tempiColumn.key]);
						}
						if (dictionary.Count > 0)
						{
							list.Add(new IRow(i, dataRow, dictionary, 0, null));
						}
					}
				}
				return list.ToArray();
			}
		}
		else
		{
			if (summary is IList list2)
			{
				List<IRow> list3 = new List<IRow>(list2.Count);
				for (int k = 0; k < list2.Count; k++)
				{
					GetRowAuto(ref list3, list2[k], k, columns, 0);
				}
				return list3.ToArray();
			}
			List<IRow> list4 = new List<IRow>(1);
			GetRowAuto(ref list4, summary, 0, columns, 0);
			if (list4.Count > 0)
			{
				return list4.ToArray();
			}
		}
		return null;
	}

	public void Binding<T>(AntList<T> list)
	{
		dataOne = true;
		if (list == null)
		{
			return;
		}
		dataSource = list;
		list.action = (string code, object obj) =>
		{
			if (dataTmp != null)
			{
				try
				{
					switch (code)
					{
					case "add":
						if (obj is int num)
						{
							T val = list[num];
							if (val != null)
							{
								Dictionary<string, object> row = GetRow(val, dataTmp.columns.Length);
								if (row.Count != 0)
								{
									int num2 = dataTmp.rows.Length + 1;
									if (num2 >= num)
									{
										List<IRow> list2 = new List<IRow>(num2);
										list2.AddRange(dataTmp.rows);
										list2.Insert(num, new IRow(num, val, row, 0, null));
										dataTmp.rows = ChangeList(list2);
										if (LoadLayout())
										{
											Invalidate();
										}
									}
								}
							}
						}
						else if (obj is int[] array)
						{
							List<IRow> list3 = new List<IRow>(array.Length);
							int[] array2 = array;
							foreach (int index in array2)
							{
								T val2 = list[index];
								if (val2 != null)
								{
									Dictionary<string, object> row2 = GetRow(val2, dataTmp.columns.Length);
									if (row2.Count > 0)
									{
										list3.Add(new IRow(index, val2, row2, 0, null));
									}
								}
							}
							List<IRow> list4 = new List<IRow>(dataTmp.rows.Length + list3.Count);
							list4.AddRange(dataTmp.rows);
							list4.InsertRange(array[0], list3);
							dataTmp.rows = ChangeList(list4);
							if (LoadLayout())
							{
								Invalidate();
							}
						}
						break;
					case "edit":
						if (LoadLayout())
						{
							Invalidate();
						}
						break;
					case "del":
						if (obj is int i)
						{
							DelData(i);
						}
						else if (obj is string)
						{
							EditModeClose();
							dataTmp.rows = new IRow[0];
							SortData = null;
							if (LoadLayout())
							{
								Invalidate();
							}
						}
						break;
					}
				}
				catch
				{
					IBinding(list);
				}
			}
		};
		IBinding(list);
	}

	private void IBinding<T>(AntList<T> list)
	{
		ExtractHeaderFixed();
		TempiColumn[] array = ExtractColumn(list);
		List<IRow> list2 = new List<IRow>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			GetRowAuto(ref list2, list[i], i, array, 0);
		}
		dataTmp = new TempTable(array, list2.ToArray(), ExtractSummary(array));
		LoadLayout();
		Invalidate();
	}

	public void Binding<T>(BindingList<T> list)
	{
		dataOne = true;
		if (list != null)
		{
			list.ListChanged -= Binding_ListChanged;
			list.ListChanged += Binding_ListChanged;
			DataSource = list;
		}
	}

	private void Binding_ListChanged(object? sender, ListChangedEventArgs e)
	{
		if (sender != dataSource)
		{
			return;
		}
		switch (e.ListChangedType)
		{
		case ListChangedType.ItemAdded:
			BindingItemAdded(sender, e.NewIndex);
			break;
		case ListChangedType.ItemDeleted:
			BindingItemDeleted(sender, e.NewIndex);
			break;
		case ListChangedType.ItemChanged:
			BindingItemChanged(sender, e.NewIndex, e.NewIndex == e.OldIndex);
			break;
		case ListChangedType.Reset:
			if (dataTmp != null)
			{
				EditModeClose();
				dataTmp.rows = new IRow[0];
				SortData = null;
				if (LoadLayout())
				{
					Invalidate();
				}
			}
			break;
		default:
			if (sender is IList list)
			{
				DataSource = list;
			}
			break;
		}
	}

	private void BindingItemAdded(object? sender, int i)
	{
		if (dataTmp == null)
		{
			if (ExtractData(dataSource) && LoadLayout())
			{
				Invalidate();
			}
		}
		else
		{
			if (!(sender is IList list))
			{
				return;
			}
			object obj = list[i];
			if (obj == null)
			{
				return;
			}
			if (dataTmp.columns.Length == 0)
			{
				ExtractData();
			}
			else
			{
				List<IRow> list2;
				if (TreeKey == null)
				{
					Dictionary<string, object> row = GetRow(obj, dataTmp.columns.Length);
					if (row.Count == 0)
					{
						return;
					}
					int num = dataTmp.rows.Length + 1;
					if (num < i)
					{
						return;
					}
					list2 = new List<IRow>(num);
					list2.AddRange(dataTmp.rows);
					list2.Insert(i, new IRow(i, obj, row, 0, null));
					dataTmp.rows = ChangeList(list2);
				}
				else
				{
					List<IRow> list3 = new List<IRow>(1);
					GetRowAuto(ref list3, obj, i, dataTmp.columns, 0);
					if (list3.Count == 0)
					{
						return;
					}
					int num2 = dataTmp.rows.Length + list3.Count;
					i = dataTmp.FindTreeIndexFid(i, list);
					if (num2 < i)
					{
						return;
					}
					list2 = new List<IRow>(num2);
					list2.AddRange(dataTmp.rows);
					list2.InsertRange(i, list3);
				}
				dataTmp.rows = ChangeList(list2);
			}
			if (LoadLayout())
			{
				Invalidate();
			}
		}
	}

	private void BindingItemChanged(object? sender, int i, bool eq)
	{
		if (dataTmp == null || pauseLayoutByCheckAll || !(sender is IList list))
		{
			return;
		}
		object obj = list[i];
		if (obj == null)
		{
			return;
		}
		if (eq)
		{
			int? rowIndex = GetRowIndex(obj, dataTmp.rows);
			if (rowIndex.HasValue)
			{
				i = rowIndex.Value;
			}
		}
		Dictionary<string, object> row = GetRow(obj, dataTmp.columns.Length);
		if (row.Count != 0)
		{
			List<IRow> list2 = new List<IRow>(dataTmp.rows.Length);
			list2.AddRange(dataTmp.rows);
			list2[i] = new IRow(i, obj, row, 0, null);
			dataTmp.rows = ChangeList(list2);
			if (LoadLayout())
			{
				Invalidate();
			}
		}
	}

	private void BindingItemDeleted(object? sender, int i)
	{
		DelData(i);
	}

	public int[]? SortDelList(int i)
	{
		if (dataTmp == null)
		{
			return null;
		}
		if (i >= 0 && i < dataTmp.rows.Length)
		{
			DelSortData(dataTmp.rows, i);
		}
		return SortData;
	}

	public int[]? SortDelList(object record)
	{
		if (dataTmp == null)
		{
			return null;
		}
		int? rowIndexReal = GetRowIndexReal(record, dataTmp.rows);
		if (rowIndexReal.HasValue)
		{
			DelSortData(dataTmp.rows, rowIndexReal.Value);
		}
		return SortData;
	}

	private void DelData(int i)
	{
		if (dataTmp != null && i >= 0 && i < dataTmp.rows.Length)
		{
			List<IRow> list = new List<IRow>(dataTmp.rows.Length);
			list.AddRange(dataTmp.rows);
			list.RemoveAt(i);
			DelSortData(dataTmp.rows, i);
			dataTmp.rows = ChangeList(list);
			if (LoadLayout())
			{
				Invalidate();
			}
		}
	}

	private void DelSortData(IRow[] rows, int i)
	{
		if (SortData == null || i < 0 || i >= rows.Length)
		{
			return;
		}
		List<int> list = new List<int>(rows.Length - 1);
		int[] sortData = SortData;
		foreach (int num in sortData)
		{
			if (num != i)
			{
				int num2 = ((num > i) ? (num - 1) : num);
				if (num2 >= 0 && num2 < rows.Length - 1)
				{
					list.Add(num2);
				}
			}
		}
		SortData = ((list.Count > 0) ? list.ToArray() : null);
	}

	private IRow[] ChangeList(List<IRow> rows)
	{
		for (int i = 0; i < rows.Count; i++)
		{
			rows[i].i = i;
		}
		return rows.ToArray();
	}

	private void GetRowAuto(ref List<IRow> rows, object? row, int i, TempiColumn[] columns, int depth, object? fid = null)
	{
		if (row == null)
		{
			return;
		}
		PropertyDescriptor property;
		object value2;
		if (row is IList<AntItem> cells)
		{
			IRow row2 = new IRow(i, row, cells, depth, fid);
			rows.Add(row2);
			if (TreeKey == null || !row2.cells.TryGetValue(TreeKey, out object value))
			{
				return;
			}
			object[] rowTree = GetRowTree(OGetValue(value, row, out property, out value2));
			if (rowTree != null)
			{
				row2.has_expand = true;
				List<IRow> collection = new List<IRow>(rowTree.Length);
				for (int j = 0; j < rowTree.Length; j++)
				{
					GetRowAuto(ref collection, rowTree[j], j, columns, depth + 1, row);
				}
				rows.AddRange(collection);
			}
			return;
		}
		Dictionary<string, object> row3 = GetRow(row, columns.Length);
		if (row3.Count <= 0)
		{
			return;
		}
		IRow row4 = new IRow(i, row, row3, depth, fid);
		rows.Add(row4);
		if (TreeKey == null || !row4.cells.TryGetValue(TreeKey, out object value3))
		{
			return;
		}
		object[] rowTree2 = GetRowTree(OGetValue(value3, row, out property, out value2));
		if (rowTree2 != null)
		{
			row4.has_expand = true;
			List<IRow> collection2 = new List<IRow>(rowTree2.Length);
			for (int k = 0; k < rowTree2.Length; k++)
			{
				GetRowAuto(ref collection2, rowTree2[k], k, columns, depth + 1, row);
			}
			rows.AddRange(collection2);
		}
	}

	private static object[]? GetRowTree(object? value)
	{
		if (value == null)
		{
			return null;
		}
		if (value is IList<AntItem[]> { Count: >0 } list)
		{
			List<object> list2 = new List<object>(list.Count);
			foreach (AntItem[] item in list)
			{
				list2.Add(item);
			}
			return list2.ToArray();
		}
		if (value is IList<object> { Count: >0 } list3)
		{
			return list3.ToArray();
		}
		if (value is IEnumerable<object> source)
		{
			List<object> list4 = source.ToList();
			if (list4.Count <= 0)
			{
				return null;
			}
			return list4.ToArray();
		}
		return null;
	}

	private Dictionary<string, object?> GetRow(object row, int len)
	{
		if (row is IList<AntItem> list)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>(list.Count);
			{
				foreach (AntItem item in list)
				{
					dictionary.Add(item.key, item);
				}
				return dictionary;
			}
		}
		if (row is ExpandoObject expandoObject)
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>(len);
			{
				foreach (KeyValuePair<string, object> item2 in (IEnumerable<KeyValuePair<string, object>>)expandoObject)
				{
					dictionary2.Add(item2.Key, item2.Value);
				}
				return dictionary2;
			}
		}
		if (row is IDictionary<string, object> dictionary3)
		{
			Dictionary<string, object> dictionary4 = new Dictionary<string, object>(len);
			{
				foreach (KeyValuePair<string, object> item3 in dictionary3)
				{
					dictionary4.Add(item3.Key, item3.Value);
				}
				return dictionary4;
			}
		}
		Dictionary<string, object> cells = new Dictionary<string, object>(len);
		foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(row))
		{
			cells.Add(property.Name, property);
			GetRow(row, property, ref cells);
		}
		return cells;
	}

	private void GetRow(object row, PropertyDescriptor it, ref Dictionary<string, object?> cells)
	{
		if (!it.PropertyType.IsClass || it.PropertyType.IsArray || it.PropertyType.FullName == null || it.PropertyType.FullName.StartsWith("System.") || it.PropertyType.FullName.StartsWith("AntdUI."))
		{
			return;
		}
		object value = it.GetValue(row);
		if (value == null)
		{
			return;
		}
		foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value))
		{
			cells.Add(it.Name + "." + property.Name, new TableSubValue(value, property));
		}
	}

	internal void ExtractHeaderFixed()
	{
		if (columns == null)
		{
			TreeKey = null;
			fixedColumnL = (fixedColumnR = null);
			return;
		}
		string treekey = null;
		List<int> dir = new List<int>();
		int index = 0;
		ForColumnI(columns, (Column column) =>
		{
			if (column.Fixed)
			{
				dir.Add(index);
			}
			if (column.KeyTree != null)
			{
				treekey = column.KeyTree;
			}
			index++;
		});
		TreeKey = treekey;
		if (dir.Count > 0)
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			foreach (int item in dir)
			{
				if (item == 0)
				{
					list.Add(item);
				}
				else if (list.Count > 0 && list[list.Count - 1] + 1 == item)
				{
					list.Add(item);
				}
			}
			foreach (int item2 in list)
			{
				dir.Remove(item2);
			}
			if (dir.Count > 0)
			{
				dir.Reverse();
				foreach (int item3 in dir)
				{
					if (item3 == index - 1)
					{
						list2.Add(item3);
					}
					else if (list2.Count > 0 && list2[list2.Count - 1] - 1 == item3)
					{
						list2.Add(item3);
					}
				}
			}
			if (list.Count > 0)
			{
				fixedColumnL = list;
			}
			else
			{
				fixedColumnL = null;
			}
			if (list2.Count > 0)
			{
				fixedColumnR = list2;
			}
			else
			{
				fixedColumnR = null;
			}
		}
		else
		{
			fixedColumnL = (fixedColumnR = null);
		}
	}

	/// <summary>
	/// 进入编辑模式
	/// </summary>
	/// <param name="row">行</param>
	/// <param name="column">列</param>
	public bool EnterEditMode(int row, int column)
	{
		if (rows != null)
		{
			try
			{
				RowTemplate rowTemplate = rows[row];
				if (rowTemplate == null)
				{
					return false;
				}
				CELL cELL = RealCELL(rowTemplate.cells[column], rows.List, row, column, out var rect);
				EditModeClose();
				if (CanEditMode(cELL))
				{
					ScrollLine(rect, RealRegionRow(rows.List));
					if (showFixedColumnL && fixedColumnL != null && fixedColumnL.Contains(column))
					{
						OnEditMode(rowTemplate, cELL, rect, row, column, cELL.COLUMN, 0, ScrollBarRealY, region: false);
					}
					else if (showFixedColumnR && fixedColumnR != null && fixedColumnR.Contains(column))
					{
						OnEditMode(rowTemplate, cELL, rect, row, column, cELL.COLUMN, sFixedR, ScrollBarRealY, region: false);
					}
					else
					{
						OnEditMode(rowTemplate, cELL, rect, row, column, cELL.COLUMN, ScrollBar.ValueX, ScrollBarRealY, region: true);
					}
					return true;
				}
			}
			catch
			{
			}
		}
		return false;
	}

	/// <summary>
	/// 关闭编辑模式
	/// </summary>
	public void EditModeClose()
	{
		if (!inEditMode && _editControls.Count <= 0)
		{
			return;
		}
		Input[] array = _editControls.Keys.ToArray();
		foreach (Input input in array)
		{
			if (_editControls.TryRemove(input, out EditControlTag value))
			{
				input.LostFocus -= InputEdit_LostFocus;
				if (input is Select obj)
				{
					obj.SelectedValueChanged -= InputEdit_SelectedValueChanged;
					obj.ClosedItem -= InputEdit_SelectedValueChanged;
					value.call?.Invoke(value.value == obj.SelectedValue, obj.SelectedValue);
				}
				else
				{
					input.KeyPress -= InputEdit_KeyPress;
					value.call?.Invoke((string)value.value == input.Text, input.Text);
				}
				value.call = null;
				Focus();
				input.Dispose();
				Controls.Remove(input);
			}
		}
		inEditMode = false;
	}

	private bool CanEditMode(CELL cell)
	{
		if (rows == null)
		{
			return false;
		}
		if (cell.COLUMN.Editable)
		{
			if (cell is TCellText)
			{
				return true;
			}
			if (cell is TCellSelect)
			{
				return true;
			}
			if (cell is Template template)
			{
				foreach (ICell item in template.Value)
				{
					if (item is CellText)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void OnEditMode(RowTemplate it, CELL cell, Rectangle rect, int i_row, int i_col, Column column, int sx, int sy, bool region)
	{
		if (rows == null)
		{
			return;
		}
		if (it.AnimationHover)
		{
			it.ThreadHover?.Dispose();
			it.ThreadHover = null;
		}
		_currentEdit = new TableCellEditEnterEventArgs(it.RECORD, i_row, i_col, column);
		bool multiline = column.LineBreak;
		if (column is ColumnSelect)
		{
			object value = null;
			object val = null;
			if (cell is TCellSelect tCellSelect)
			{
				value = tCellSelect.value?.Tag;
				val = tCellSelect.value;
			}
			if (!OnCellBeginEdit(value, it.RECORD, i_row, i_col, column))
			{
				return;
			}
			inEditMode = true;
			BeginInvoke(() =>
			{
				rows.SetHover(i_row);
				Input input = CreateInput(cell, sx, sy, multiline, val, rect);
				input.Name = "edit_" + i_col + "_" + i_row;
				if (column.Align == ColumnAlign.Center)
				{
					input.TextAlign = HorizontalAlignment.Center;
				}
				else if (column.Align == ColumnAlign.Right)
				{
					input.TextAlign = HorizontalAlignment.Right;
				}
				TableBeginEditInputStyleEventArgs e = new TableBeginEditInputStyleEventArgs(value, it.RECORD, i_row, i_col, column, input);
				OnCellBeginEditInputStyle(e);
				if (e.Input is Select obj)
				{
					ShowSelect(obj, region, column.INDEX_REAL, (bool echo, object? r_value) =>
					{
						if (!echo && OnCellEndValueEdit(r_value, it.RECORD, i_row, i_col, column))
						{
							if (cell is TCellSelect tCellSelect2)
							{
								tCellSelect2.value = tCellSelect2.COLUMN[r_value];
								SetValue(cell, r_value);
								if (multiline)
								{
									LoadLayout();
								}
							}
							else
							{
								SetValue(cell, r_value);
								LoadLayout();
							}
							OnCellEditComplete(it.RECORD, i_row, i_col, column);
						}
					});
				}
				else
				{
					e.Input.Dispose();
				}
			});
			return;
		}
		TCellText cellText = cell as TCellText;
		if (cellText != null)
		{
			object value2 = null;
			if (cell.PROPERTY != null && cell.VALUE != null)
			{
				value2 = cell.PROPERTY.GetValue(cell.VALUE);
			}
			else if (cell.VALUE is AntItem antItem)
			{
				value2 = antItem.value;
			}
			else
			{
				value2 = cell.VALUE;
			}
			if (!OnCellBeginEdit(value2, it.RECORD, i_row, i_col, column))
			{
				return;
			}
			inEditMode = true;
			BeginInvoke(() =>
			{
				rows.SetHover(i_row);
				Input input = CreateInput(cell, sx, sy, multiline, value2, rect);
				input.Name = "edit_" + i_col + "_" + i_row;
				if (column.Align == ColumnAlign.Center)
				{
					input.TextAlign = HorizontalAlignment.Center;
				}
				else if (column.Align == ColumnAlign.Right)
				{
					input.TextAlign = HorizontalAlignment.Right;
				}
				TableBeginEditInputStyleEventArgs arge = new TableBeginEditInputStyleEventArgs(value2, it.RECORD, i_row, i_col, column, input);
				OnCellBeginEditInputStyle(arge);
				ShowInput(arge.Input, region, column.INDEX_REAL, (bool echo, object? r_value) =>
				{
					if (r_value is string value4)
					{
						arge.Call?.Invoke(new TableEndEditEventArgs(value4, it.RECORD, i_row, i_col, column));
						if (!echo && OnCellEndEdit(value4, it.RECORD, i_row, i_col, column))
						{
							if (GetValue(value2, value4, cell.PROPERTY, out object read))
							{
								cellText.SetValue(read);
								SetValue(cell, read);
								if (multiline)
								{
									LoadLayout();
								}
							}
							OnCellEditComplete(it.RECORD, i_row, i_col, column);
						}
					}
					else if (!echo && OnCellEndValueEdit(r_value, it.RECORD, i_row, i_col, column))
					{
						SetValue(cell, r_value);
						LoadLayout();
						OnCellEditComplete(it.RECORD, i_row, i_col, column);
					}
				});
			});
		}
		else
		{
			if (!(cell is Template template))
			{
				return;
			}
			foreach (ICell item in template.Value)
			{
				CellText text = item as CellText;
				if (text == null)
				{
					continue;
				}
				object value3 = null;
				if (cell.PROPERTY != null && cell.VALUE != null)
				{
					value3 = cell.PROPERTY.GetValue(cell.VALUE);
				}
				else if (cell.VALUE is AntItem antItem2)
				{
					value3 = antItem2.value;
				}
				else
				{
					value3 = cell.VALUE;
				}
				if (!OnCellBeginEdit(value3, it.RECORD, i_row, i_col, column))
				{
					break;
				}
				inEditMode = true;
				BeginInvoke(() =>
				{
					rows.SetHover(i_row);
					Input input = CreateInput(cell, sx, sy, multiline, value3, rect);
					input.Name = "edit_" + i_col + "_" + i_row;
					if (column.Align == ColumnAlign.Center)
					{
						input.TextAlign = HorizontalAlignment.Center;
					}
					else if (column.Align == ColumnAlign.Right)
					{
						input.TextAlign = HorizontalAlignment.Right;
					}
					TableBeginEditInputStyleEventArgs arge = new TableBeginEditInputStyleEventArgs(value3, it.RECORD, i_row, i_col, column, input);
					OnCellBeginEditInputStyle(arge);
					ShowInput(arge.Input, region, column.INDEX_REAL, (bool echo, object? r_value) =>
					{
						if (r_value is string value4)
						{
							arge.Call?.Invoke(new TableEndEditEventArgs(value4, it.RECORD, i_row, i_col, column));
							if (!echo && OnCellEndEdit(value4, it.RECORD, i_row, i_col, column))
							{
								if (value3 is CellText cellText2)
								{
									cellText2.Text = value4;
									SetValue(cell, cellText2);
								}
								else
								{
									text.Text = value4;
									if (GetValue(value3, value4, cell.PROPERTY, out object read))
									{
										SetValue(cell, read);
									}
								}
								OnCellEditComplete(it.RECORD, i_row, i_col, column);
							}
						}
						else if (!echo && OnCellEndValueEdit(r_value, it.RECORD, i_row, i_col, column))
						{
							SetValue(cell, r_value);
							LoadLayout();
							OnCellEditComplete(it.RECORD, i_row, i_col, column);
						}
					});
				});
				break;
			}
		}
	}

	private bool GetValue(object? value, string _value, PropertyDescriptor? Property, out object? read)
	{
		if (Property == null)
		{
			read = GetValue(value, _value);
			return true;
		}
		try
		{
			read = Property.Converter.ConvertFrom(_value);
			return true;
		}
		catch
		{
			if (Property.PropertyType.IsGenericType && Property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				read = GetValue(value, _value);
				return true;
			}
			return GetValue(value, _value, out read);
		}
	}

	private object? GetValue(object? value, string _value)
	{
		if (value is int?)
		{
			if (int.TryParse(_value, out var result))
			{
				return result;
			}
			return null;
		}
		if (value is uint?)
		{
			if (uint.TryParse(_value, out var result2))
			{
				return result2;
			}
			return null;
		}
		if (value is short?)
		{
			if (short.TryParse(_value, out var result3))
			{
				return result3;
			}
			return null;
		}
		if (value is ushort?)
		{
			if (ushort.TryParse(_value, out var result4))
			{
				return result4;
			}
			return null;
		}
		if (value is long?)
		{
			if (long.TryParse(_value, out var result5))
			{
				return result5;
			}
			return null;
		}
		if (value is ulong?)
		{
			if (ulong.TryParse(_value, out var result6))
			{
				return result6;
			}
			return null;
		}
		if (value is double?)
		{
			if (double.TryParse(_value, out var result7))
			{
				return result7;
			}
			return null;
		}
		if (value is decimal?)
		{
			if (decimal.TryParse(_value, out var result8))
			{
				return result8;
			}
			return null;
		}
		if (value is float?)
		{
			if (float.TryParse(_value, out var result9))
			{
				return result9;
			}
			return null;
		}
		if (value is DateTime?)
		{
			if (DateTime.TryParse(_value, out var result10))
			{
				return result10;
			}
			return null;
		}
		return _value;
	}

	private bool GetValue(object? value, string _value, out object? read)
	{
		if (value is int)
		{
			if (int.TryParse(_value, out var result))
			{
				read = result;
				return true;
			}
		}
		else if (value is uint)
		{
			if (uint.TryParse(_value, out var result2))
			{
				read = result2;
				return true;
			}
		}
		else if (value is short)
		{
			if (short.TryParse(_value, out var result3))
			{
				read = result3;
				return true;
			}
		}
		else if (value is ushort)
		{
			if (ushort.TryParse(_value, out var result4))
			{
				read = result4;
				return true;
			}
		}
		else if (value is long)
		{
			if (long.TryParse(_value, out var result5))
			{
				read = result5;
				return true;
			}
		}
		else if (value is ulong)
		{
			if (ulong.TryParse(_value, out var result6))
			{
				read = result6;
				return true;
			}
		}
		else if (value is double)
		{
			if (double.TryParse(_value, out var result7))
			{
				read = result7;
				return true;
			}
		}
		else if (value is decimal)
		{
			if (decimal.TryParse(_value, out var result8))
			{
				read = result8;
				return true;
			}
		}
		else if (value is float)
		{
			if (float.TryParse(_value, out var result9))
			{
				read = result9;
				return true;
			}
		}
		else
		{
			if (!(value is DateTime))
			{
				read = _value;
				return true;
			}
			if (DateTime.TryParse(_value, out var result10))
			{
				read = result10;
				return true;
			}
		}
		read = null;
		return false;
	}

	private Input CreateInput(CELL cell, int sx, int sy, bool multiline, object? value, Rectangle rect)
	{
		switch (EditInputStyle)
		{
		case TEditInputStyle.Full:
		{
			Input input2 = CreateInput(multiline, value, cell.COLUMN, RectInput(cell, rect, sx, sy, 1f, 4));
			input2.Radius = 0;
			return input2;
		}
		case TEditInputStyle.Excel:
		{
			Input input = CreateInput(multiline, value, cell.COLUMN, RectInput(cell, rect, sx, sy, 2.5f, 0));
			input.WaveSize = 0;
			input.Radius = 0;
			input.BorderWidth = 2.5f;
			return input;
		}
		default:
			return CreateInput(multiline, value, cell.COLUMN, RectInputDefault(cell, rect, sx, sy, 1f, 4));
		}
	}

	private Rectangle RectInput(CELL cell, Rectangle rect, int sx, int sy, float borwidth, int wavesize)
	{
		int num = (int)(((float)wavesize + borwidth / 2f) * Dpi);
		int num2 = num * 2;
		int num3 = rect.Y;
		int num4 = rect.Height;
		if (EditAutoHeight)
		{
			int num5 = this.GDI((Canvas g) => g.MeasureString("龍Qq", Font).Height);
			int num6 = (int)((float)num5 * 0.4f) * 2;
			int num7 = num5 + num6 + num2;
			if (num7 > rect.Height)
			{
				num4 = num7 - num2;
				num3 = rect.Y + (rect.Height - num4) / 2;
				if (num3 + num7 - sy > rect_read.Bottom)
				{
					num3 = rect_read.Bottom + sy - num4 - num;
				}
			}
		}
		return new Rectangle(rect.X - sx - num, num3 - sy - num, rect.Width + num2, num4 + num2);
	}

	private Rectangle RectInputDefault(CELL cell, Rectangle rect, int sx, int sy, float borwidth, int wavesize)
	{
		int num = (int)(((float)wavesize + borwidth / 2f) * Dpi) * 2;
		int num2 = rect.Y;
		int num3 = rect.Height;
		if (EditAutoHeight)
		{
			int num4 = this.GDI((Canvas g) => g.MeasureString("龍Qq", Font).Height);
			int num5 = (int)((float)num4 * 0.4f) * 2;
			int num6 = num4 + num5 + num;
			if (num6 > rect.Height)
			{
				num3 = num6;
				num2 = rect.Y + (rect.Height - num3) / 2;
				if (num2 + num6 - sy > rect_read.Bottom)
				{
					num2 = rect_read.Bottom + sy - num3;
				}
			}
		}
		return new Rectangle(rect.X - sx, num2 - sy, rect.Width, num3);
	}

	private Input CreateInput(bool multiline, object? value, Column column, Rectangle rect)
	{
		Input input;
		if (column is ColumnSelect column2)
		{
			Select obj = CreateInputBySelect(column2);
			obj.Multiline = multiline;
			obj.IconRatio = 1f;
			obj.ReadOnly = column.ReadOnly;
			obj.Bounds = rect;
			if (value is SelectItem selectItem)
			{
				obj.Text = selectItem.Text;
				obj.SelectedValue = selectItem.Tag;
				obj.ShowIcon = selectItem.Icon != null || selectItem.IconSvg != null;
			}
			input = obj;
			obj.SelectedValue = value;
		}
		else
		{
			input = ((!(value is CellText cellText)) ? new Input
			{
				AcceptsEscape = true,
				Multiline = multiline,
				Bounds = rect,
				Text = (column.GetDisplayText(value) ?? string.Empty),
				ReadOnly = column.ReadOnly
			} : new Input
			{
				AcceptsEscape = true,
				Multiline = multiline,
				Bounds = rect,
				Text = (cellText.Text ?? ""),
				ReadOnly = column.ReadOnly
			});
		}
		if (input.ReadOnly)
		{
			input.BackColor = Colour.BorderSecondary.Get("Table");
		}
		switch (EditSelection)
		{
		case TEditSelection.First:
		{
			Input input2 = input;
			int selectionStart = (input.SelectionLength = 0);
			input2.SelectionStart = selectionStart;
			break;
		}
		case TEditSelection.Last:
			input.SelectLast();
			break;
		case TEditSelection.All:
			input.SelectAll();
			break;
		}
		return input;
	}

	internal Select CreateInputBySelect(ColumnSelect column)
	{
		Select obj = new Select
		{
			AcceptsEscape = true,
			List = true
		};
		if (column.MaxCount.HasValue)
		{
			obj.MaxCount = column.MaxCount.Value;
		}
		BaseCollection items = obj.Items;
		object[] items2 = column.Items.ToArray();
		items.AddRange(items2);
		return obj;
	}

	private void ShowInput(Input input, bool region, int column, Action<bool, object?> call)
	{
		if (input is Select obj)
		{
			ShowSelect(obj, region, column, call);
		}
		else if (AddEditInput(input, input.Text, call, region, column))
		{
			input.KeyPress += InputEdit_KeyPress;
			input.Focus();
		}
	}

	private void ShowSelect(Select select, bool region, int column, Action<bool, object?> call)
	{
		if (AddEditInput(select, select.SelectedValue, call, region, column))
		{
			select.SelectedValueChanged += InputEdit_SelectedValueChanged;
			select.ClosedItem += InputEdit_SelectedValueChanged;
			select.Focus();
		}
	}

	private void InputEdit_KeyPress(object? sender, KeyPressEventArgs e)
	{
		if (sender is Input && e.KeyChar == '\r')
		{
			e.Handled = true;
			EditModeClose();
			if (_currentEdit != null)
			{
				OnCellEditEnter(sender, _currentEdit);
			}
		}
	}

	private void InputEdit_SelectedValueChanged(object? sender, ObjectNEventArgs e)
	{
		EditModeClose();
	}

	private void InputEdit_LostFocus(object? sender, EventArgs e)
	{
		if (EditLostFocus)
		{
			EditModeClose();
		}
	}

	/// <summary>
	/// 添加空间到编辑
	/// </summary>
	private bool AddEditInput(Input input, object? txt, Action<bool, object?> action, bool region, int column)
	{
		if (_editControls.TryAdd(input, new EditControlTag(txt, action)))
		{
			if (region)
			{
				EditInputRegion(input, column);
			}
			Controls.Add(input);
			if (OS.Win7OrLower)
			{
				return true;
			}
			input.LostFocus += InputEdit_LostFocus;
			return true;
		}
		input.Dispose();
		return false;
	}

	private void EditInputRegion(Input input, int col)
	{
		if (showFixedColumnR && rect_fixed_R.HasValue && showFixedColumnL && rect_fixed_L.HasValue)
		{
			if (!fixedColumnL.Contains(col) && !fixedColumnR.Contains(col))
			{
				input.Region = EditInputRegion(input.Bounds, rect_fixed_R.Value, rect_fixed_L.Value);
			}
		}
		else if (showFixedColumnR && rect_fixed_R.HasValue)
		{
			if (!fixedColumnR.Contains(col))
			{
				input.Region = EditInputRegion(input.Bounds, rect_fixed_R.Value);
			}
		}
		else if (showFixedColumnL && rect_fixed_L.HasValue && !fixedColumnL.Contains(col))
		{
			input.Region = EditInputRegion(input.Bounds, rect_fixed_L.Value);
		}
	}

	private Region? EditInputRegion(Rectangle rect, params Rectangle[] rect_fixeds)
	{
		Region region = null;
		foreach (Rectangle rectangle in rect_fixeds)
		{
			if (rect.IntersectsWith(rectangle))
			{
				Rectangle rectangle2 = Rectangle.Intersect(rect, rectangle);
				if (region == null)
				{
					region = new Region(new Rectangle(0, 0, rect.Width, rect.Height));
				}
				region.Exclude(new Rectangle(rectangle2.X - rect.X, rectangle2.Y - rect.Y, rectangle2.Width, rectangle2.Height));
			}
		}
		return region;
	}

	protected virtual void OnCheckedChanged(bool value, object record, int rowIndex, int columnIndex, Column column)
	{
		CheckedChanged?.Invoke(this, new TableCheckEventArgs(value, record, rowIndex, columnIndex, column));
	}

	internal void OnICheckedOverallChanged(ColumnCheck column, CheckState checkState)
	{
		OnCheckedOverallChanged(column, checkState);
	}

	protected virtual void OnCheckedOverallChanged(ColumnCheck column, CheckState checkState)
	{
		CheckedOverallChanged?.Invoke(this, new CheckStateEventArgs(column, checkState));
	}

	protected virtual void OnCellClick(object record, RowType rowType, int rowIndex, int columnIndex, Column? column, Rectangle rect, MouseEventArgs e)
	{
		CellClick?.Invoke(this, new TableClickEventArgs(record, rowType, rowIndex, columnIndex, column, rect, e));
	}

	protected virtual void OnCellDoubleClick(object record, RowType rowType, int rowIndex, int columnIndex, Column? column, Rectangle rect, MouseEventArgs e)
	{
		CellDoubleClick?.Invoke(this, new TableClickEventArgs(record, rowType, rowIndex, columnIndex, column, rect, e));
	}

	protected virtual void OnNonCellClick(MouseEventArgs e)
	{
		NonCellClick?.Invoke(this, e);
	}

	protected virtual void OnNonCellDoubleClick(MouseEventArgs e)
	{
		NonCellDoubleClick?.Invoke(this, e);
	}

	protected virtual bool OnCellClickBegin(object record, RowType rowType, int rowIndex, int columnIndex, Column? column, Rectangle rect, MouseEventArgs e)
	{
		if (CellClickBegin == null)
		{
			return true;
		}
		TableClickBeginEventArgs e2 = new TableClickBeginEventArgs(record, rowType, rowIndex, columnIndex, column, rect, e);
		CellClickBegin(this, e2);
		if (e2.Handled)
		{
			return false;
		}
		return true;
	}

	protected virtual void OnCellHover(object record, RowType rowType, int rowIndex, int columnIndex, Column? column, Rectangle rect, MouseEventArgs e)
	{
		CellHover?.Invoke(this, new TableHoverEventArgs(record, rowType, rowIndex, columnIndex, column, rect, e));
	}

	protected virtual void OnCellHover()
	{
		CellHover?.Invoke(this, new TableHoverEventArgs(new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0)));
	}

	protected virtual void OnCellButtonClick(CellLink btn, object record, RowType rowType, int rowIndex, int columnIndex, Column column, Rectangle rect, MouseEventArgs e)
	{
		CellButtonClick?.Invoke(this, new TableButtonEventArgs(btn, record, rowType, rowIndex, columnIndex, column, rect, e));
	}

	protected virtual void OnCellButtonDown(CellLink btn, object record, RowType rowType, int rowIndex, int columnIndex, Column column, Rectangle rect, MouseEventArgs e)
	{
		CellButtonDown?.Invoke(this, new TableButtonEventArgs(btn, record, rowType, rowIndex, columnIndex, column, rect, e));
	}

	protected virtual void OnCellButtonUp(CellLink btn, object record, RowType rowType, int rowIndex, int columnIndex, Column column, Rectangle rect, MouseEventArgs e)
	{
		CellButtonUp?.Invoke(this, new TableButtonEventArgs(btn, record, rowType, rowIndex, columnIndex, column, rect, e));
	}

	protected virtual void OnCellFocused(object record, RowType rowType, int rowIndex, int columnIndex, Column column, Rectangle rect)
	{
		CellFocused?.Invoke(this, new TableCellFocusedEventArgs(record, rowType, rowIndex, columnIndex, column, rect));
	}

	protected virtual bool OnCellBeginEdit(object? value, object record, int rowIndex, int columnIndex, Column column)
	{
		return CellBeginEdit?.Invoke(this, new TableEventArgs(value, record, rowIndex, columnIndex, column)) ?? true;
	}

	protected virtual void OnCellBeginEditInputStyle(TableBeginEditInputStyleEventArgs e)
	{
		CellBeginEditInputStyle?.Invoke(this, e);
	}

	protected virtual bool OnCellEndEdit(string value, object record, int rowIndex, int columnIndex, Column column)
	{
		return CellEndEdit?.Invoke(this, new TableEndEditEventArgs(value, record, rowIndex, columnIndex, column)) ?? true;
	}

	protected virtual bool OnCellEndValueEdit(object? value, object record, int rowIndex, int columnIndex, Column column)
	{
		return CellEndValueEdit?.Invoke(this, new TableEndValueEditEventArgs(value, record, rowIndex, columnIndex, column)) ?? true;
	}

	protected virtual void OnCellEditComplete(object record, int rowIndex, int columnIndex, Column column)
	{
		CellEditComplete?.Invoke(this, new ITableEventArgs(record, rowIndex, columnIndex, column));
	}

	protected virtual void OnCellEditEnter(object sender, TableCellEditEnterEventArgs e)
	{
		CellEditEnter?.Invoke(sender, e);
		if (EnableFocusNavigation)
		{
			FocusNavigation(e);
		}
	}

	protected virtual void OnRowPaint(Canvas canvas, Rectangle rect, object record, RowType rowType, int rowIndex)
	{
		RowPaint?.Invoke(this, new TablePaintRowEventArgs(canvas, rect, record, rowType, rowIndex));
	}

	protected virtual void OnRowPaintBegin(Canvas canvas, Rectangle rect, object record, RowType rowType, int rowIndex)
	{
		RowPaintBegin?.Invoke(this, new TablePaintRowEventArgs(canvas, rect, record, rowType, rowIndex));
	}

	protected virtual void OnCellPaint(Canvas canvas, Rectangle rect, Rectangle rectreal, object record, RowType rowType, int rowIndex, int index, Column column)
	{
		CellPaint?.Invoke(this, new TablePaintEventArgs(canvas, rect, rectreal, record, rowType, rowIndex, index, column));
	}

	protected virtual bool OnCellPaintBegin(Canvas canvas, Rectangle rect, Rectangle rectreal, object record, RowType rowType, int rowIndex, int index, Column column, out SolidBrush? CellFore, out Brush? CellBack, out Font? CellFont)
	{
		CellFore = null;
		CellBack = null;
		CellFont = null;
		if (CellPaintBegin == null)
		{
			return true;
		}
		TablePaintBeginEventArgs e = new TablePaintBeginEventArgs(canvas, rect, rectreal, record, rowType, rowIndex, index, column);
		CellPaintBegin(this, e);
		CellFore = e.CellFore;
		CellBack = e.CellBack;
		CellFont = e.CellFont;
		if (e.Handled)
		{
			return false;
		}
		return true;
	}

	protected virtual CellStyleInfo? OnSetRowStyle(object record, RowType rowType, int rowIndex, int index)
	{
		return SetRowStyle?.Invoke(this, new TableSetRowStyleEventArgs(record, rowType, rowIndex, index));
	}

	protected virtual void OnSelectIndexChanged()
	{
		SelectIndexChanged?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnSortRows(int e)
	{
		SortRows?.Invoke(this, new IntEventArgs(e));
	}

	protected virtual bool OnSortRowsTree(object record, int from, int to)
	{
		if (SortRowsTree == null)
		{
			return false;
		}
		TableSortTreeEventArgs e = new TableSortTreeEventArgs(record, from, to);
		SortRowsTree(this, e);
		return e.Handled;
	}

	protected virtual bool OnSortModeChanged(SortMode sortMode, Column column)
	{
		return SortModeChanged?.Invoke(this, new TableSortModeEventArgs(sortMode, column)) ?? false;
	}

	protected virtual bool OnExpandChanged(object record, bool value)
	{
		ExpandChanged?.Invoke(this, new TableExpandEventArgs(record, value));
		return true;
	}

	protected virtual bool OnFilterPopupBegin(Column column, out IList<object>? customSource, out Font? font, out int? height)
	{
		if (FilterPopupBegin == null)
		{
			customSource = null;
			font = null;
			height = null;
			return true;
		}
		TableFilterPopupBeginEventArgs e = new TableFilterPopupBeginEventArgs(column);
		FilterPopupBegin(this, e);
		customSource = e.CustomSource;
		font = e.Font;
		height = e.Height;
		if (e.Cancel)
		{
			return false;
		}
		return true;
	}

	protected virtual bool OnFilterPopupEnd(FilterOption option)
	{
		if (FilterPopupEnd == null)
		{
			return true;
		}
		TableFilterPopupEndEventArgs e = new TableFilterPopupEndEventArgs(option, FilterList());
		FilterPopupEnd(this, e);
		if (e.Cancel)
		{
			return false;
		}
		inEditMode = false;
		OnMouseLeave(EventArgs.Empty);
		return true;
	}

	protected virtual void OnFilterDataChanged(object[] records)
	{
		FilterDataChanged?.Invoke(this, new TableFilterDataChangedEventArgs(records));
	}

	/// <summary>
	/// 获取指定的列
	/// </summary>
	/// <param name="key">字段ID</param>
	/// <param name="ignoreCase">是否忽略大小写</param>
	public Column? GetColumnByFieldKey(string key, bool ignoreCase = false)
	{
		if (columns == null)
		{
			return null;
		}
		foreach (Column column in columns)
		{
			if (column.Key.Equals(key, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
				return column;
			}
		}
		return null;
	}

	/// <summary>
	/// 获取筛选数据 (无筛选数据时返回当前视图数据)
	/// </summary>
	/// <returns>筛选后的数据对象数组（注意：返回的是原始数据对象的引用）</returns>
	public object[] FilterList()
	{
		return FilterList(IFilterList());
	}

	internal object[] FilterList(IRow[]? row)
	{
		IRow[] rowsCache;
		if (row == null)
		{
			if (dataTmp == null)
			{
				return new object[0];
			}
			List<object> list = new List<object>(dataTmp.RowsCache.Length);
			rowsCache = dataTmp.RowsCache;
			foreach (IRow row2 in rowsCache)
			{
				list.Add(row2.record);
			}
			return list.ToArray();
		}
		List<object> list2 = new List<object>(row.Length);
		rowsCache = row;
		foreach (IRow row3 in rowsCache)
		{
			list2.Add(row3.record);
		}
		return list2.ToArray();
	}

	public IRow[]? IFilterList()
	{
		if (dataTmp == null)
		{
			return null;
		}
		bool filteredSub;
		return IFilterList(dataTmp, out filteredSub);
	}

	internal IRow[]? IFilterList(TempTable dataTmp, out bool filteredSub)
	{
		if (columns == null)
		{
			filteredSub = false;
			return null;
		}
		Dictionary<string, List<object>> dictionary = new Dictionary<string, List<object>>(columns.Count);
		foreach (Column column in columns)
		{
			if (column.Visible && column.Filter != null && column.Filter.Enabled)
			{
				dictionary.Add(column.Key, column.Filter.FilterValues);
			}
		}
		if (dictionary.Count > 0)
		{
			bool flag = false;
			List<IRow> list = new List<IRow>(dataTmp.RowsCache.Length);
			IRow[] rowsCache = dataTmp.RowsCache;
			foreach (IRow row in rowsCache)
			{
				if (MatchFilter(row, dictionary, out var filteredSub2))
				{
					list.Add(row);
					if (filteredSub2)
					{
						flag = true;
					}
				}
			}
			filteredSub = flag;
			return list.ToArray();
		}
		filteredSub = false;
		return null;
	}

	internal IRow[] IFilterList(string columns_skip)
	{
		if (dataTmp == null || columns == null)
		{
			return new IRow[0];
		}
		Dictionary<string, List<object>> dictionary = new Dictionary<string, List<object>>(columns.Count);
		foreach (Column column in columns)
		{
			if (column.Visible && column.Filter != null && column.Filter.Enabled && !(columns_skip == column.Key))
			{
				dictionary.Add(column.Key, column.Filter.FilterValues);
			}
		}
		if (dictionary.Count > 0)
		{
			List<IRow> list = new List<IRow>(dataTmp.RowsCache.Length);
			IRow[] rowsCache = dataTmp.RowsCache;
			foreach (IRow row in rowsCache)
			{
				if (MatchFilter(row, dictionary, out var _))
				{
					list.Add(row);
				}
			}
			return list.ToArray();
		}
		return dataTmp.RowsCache;
	}

	/// <summary>
	/// 更新筛选视图
	/// </summary>
	public void UpdateFilter()
	{
		if (dataTmp == null)
		{
			return;
		}
		if (columns == null)
		{
			dataTmp.ClearFilter();
			return;
		}
		IRow[] array = IFilterList(dataTmp, out var filteredSub);
		dataTmp.rowsFilter = array;
		OnFilterDataChanged(FilterList(array));
		OnUpdateSummaries();
		if (LoadLayout())
		{
			if (filteredSub)
			{
				ExpandAll();
			}
			Invalidate();
		}
	}

	/// <summary>
	/// 检查行是否符合列的筛选条件
	/// </summary>
	/// <param name="row">要检查的行</param>
	/// <param name="dir">筛选集合</param>
	/// <returns>是否符合条件</returns>
	private bool MatchFilter(IRow row, Dictionary<string, List<object?>> dir, out bool filteredSub)
	{
		bool filteredSub2 = false;
		foreach (KeyValuePair<string, List<object>> item in dir)
		{
			if (!MatchFilter(row, item.Key, item.Value, out filteredSub2))
			{
				filteredSub = false;
				return false;
			}
		}
		filteredSub = filteredSub2;
		return true;
	}

	/// <summary>
	/// 检查行是否符合列的筛选条件
	/// </summary>
	/// <param name="row">要检查的行</param>
	/// <param name="column">筛选列</param>
	/// <returns>是否符合条件</returns>
	private bool MatchFilter(IRow row, string key, List<object?> filterValues, out bool filteredSub)
	{
		object obj = row[key];
		foreach (object filterValue in filterValues)
		{
			if (filterValue == null && obj == null)
			{
				filteredSub = false;
				return true;
			}
			if (filterValue != null && filterValue.Equals(obj))
			{
				filteredSub = false;
				return true;
			}
		}
		Column column = GetColumnByFieldKey(key, ignoreCase: true);
		if (column == null || column.KeyTree == null)
		{
			column = Columns.Find((Column c) => c.KeyTree != null);
		}
		if (column != null && column.KeyTree != null)
		{
			string keyTree = column.KeyTree;
			object obj2 = row[keyTree];
			if (obj2 != null)
			{
				bool result = MatchFilterTree(obj2, key, filterValues);
				filteredSub = true;
				return result;
			}
		}
		filteredSub = false;
		return false;
	}

	private bool MatchFilterTree(object treeValue, string key, List<object?> filterValues)
	{
		object[] rowTree = GetRowTree(treeValue);
		if (rowTree == null || rowTree.Length == 0)
		{
			return false;
		}
		object[] array = rowTree;
		foreach (object row in array)
		{
			object rowValue = GetRowValue(row, key);
			foreach (object filterValue in filterValues)
			{
				if (filterValue == null && rowValue == null)
				{
					return true;
				}
				if (filterValue != null && filterValue.Equals(rowValue))
				{
					return true;
				}
			}
			object rowValue2 = GetRowValue(row, TreeKey);
			if (rowValue2 != null && MatchFilterTree(rowValue2, key, filterValues))
			{
				return true;
			}
		}
		return false;
	}

	private object? GetRowValue(object row, string key)
	{
		if (row is IList<AntItem> list)
		{
			foreach (AntItem item in list)
			{
				if (item.key == key)
				{
					return item.value;
				}
			}
		}
		else if (row is ExpandoObject expandoObject)
		{
			if (((IDictionary<string, object>)expandoObject).TryGetValue(key, out object value))
			{
				return value;
			}
		}
		else if (row is IDictionary<string, object> dictionary)
		{
			if (dictionary.TryGetValue(key, out var value2))
			{
				return value2;
			}
		}
		else
		{
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(row)[key];
			if (propertyDescriptor != null)
			{
				return propertyDescriptor.GetValue(row);
			}
		}
		return null;
	}

	/// <summary>
	/// 配置焦点跳转顺序  
	/// </summary>
	/// <param name="fieldSequence">字段顺序数组</param>
	/// <param name="selectAll">是否选中文本</param>
	/// <param name="lineBreak">是否换行，不换行本行则回到第一个设置的字段</param>
	public void ConfigureFocusNavigation(string[] fieldSequence, bool selectAll = true, bool lineBreak = true)
	{
		if (fieldSequence.Length != 0)
		{
			navigationConfig = new NavigationConfig(fieldSequence, selectAll, lineBreak);
			EnableFocusNavigation = true;
		}
		else
		{
			navigationConfig = null;
			EnableFocusNavigation = false;
		}
	}

	/// <summary>
	/// 焦点跳转
	/// </summary>
	/// <param name="e">当前编辑的列</param>
	public void FocusNavigation(TableCellEditEnterEventArgs e)
	{
		if (navigationConfig == null || !navigationConfig.Contains(e.Column.Key, out string next))
		{
			return;
		}
		if (next == null)
		{
			if (navigationConfig.LineBreak)
			{
				int num = 0;
				if (rows != null && rows.Length > 0)
				{
					num = rows.Length - rowSummary;
				}
				else if (dataSource is BindingList<object> bindingList)
				{
					num = bindingList.Count;
				}
				else if (dataSource is IList list)
				{
					num = list.Count;
				}
				else if (dataSource is DataTable dataTable)
				{
					num = dataTable.Rows.Count;
				}
				if (num > 0)
				{
					int num2 = e.RowIndex + 1;
					if (num2 < num)
					{
						MoveToNextEditableCell(navigationConfig, num2, navigationConfig.FirstFieldKey);
					}
				}
			}
			else
			{
				MoveToNextEditableCell(navigationConfig, e.RowIndex, navigationConfig.FirstFieldKey);
			}
		}
		else
		{
			MoveToNextEditableCell(navigationConfig, e.RowIndex, next);
		}
	}

	/// <summary>
	/// 移动到下一个可编辑单元格
	/// </summary>
	/// <param name="rowIndex">行索引</param>
	/// <param name="columnKey">列键名</param>
	private void MoveToNextEditableCell(NavigationConfig navigationConfig, int rowIndex, string columnKey)
	{
		if (rows == null)
		{
			return;
		}
		CELL[] cells = rows.First.cells;
		foreach (CELL cELL in cells)
		{
			if (cELL.COLUMN.Key == columnKey)
			{
				if (cELL.COLUMN.Editable && MoveToNextEditableCell(rowIndex, cELL) && navigationConfig.SelectAll)
				{
					BeginInvoke(MoveToNextEditableCellSelectAll);
				}
				break;
			}
		}
	}

	private bool MoveToNextEditableCell(int rowIndex, CELL cell)
	{
		try
		{
			if (FocusNavigationAutoSelectRow && (selectedIndex.Length == 0 || selectedIndex[0] != rowIndex))
			{
				SelectedIndex = rowIndex;
			}
			if (FocusNavigationAutoScroll)
			{
				ScrollLine(cell.ROW.RECORD);
				ScrollColumn(cell.COLUMN);
			}
			if (cell.INDEX >= 0)
			{
				SetFocusedCell(cell);
				EnterEditMode(rowIndex, cell.INDEX);
				return true;
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	private void MoveToNextEditableCellSelectAll()
	{
		try
		{
			using IEnumerator<KeyValuePair<Input, EditControlTag>> enumerator = _editControls.GetEnumerator();
			if (enumerator.MoveNext())
			{
				KeyValuePair<Input, EditControlTag> current = enumerator.Current;
				current.Key.SelectAll();
				current.Key.Focus();
			}
		}
		catch
		{
		}
	}

	protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
	{
		switch (keyData)
		{
		case Keys.A | Keys.Control:
			if (MultipleRows && dataTmp != null)
			{
				SelectedIndexs = dataTmp.GetInts(1);
				if (HandShortcutKeys)
				{
					return true;
				}
			}
			break;
		case Keys.C | Keys.Control:
			if (ClipboardCopy && rows != null && !inEditMode && selectedIndex.Length != 0)
			{
				if (ClipboardCopyFocusedCell && FocusedCell != null)
				{
					CopyData(FocusedCell);
				}
				else
				{
					CopyData(selectedIndex);
				}
				if (HandShortcutKeys)
				{
					return true;
				}
			}
			break;
		case Keys.Down:
			if (IKeyDown() && HandShortcutKeys)
			{
				return true;
			}
			break;
		case Keys.Up:
			if (IKeyUp() && HandShortcutKeys)
			{
				return true;
			}
			break;
		case Keys.Prior:
			if (ScrollBar.ShowY)
			{
				ScrollBar.ValueY -= rect_read.Height;
				if (HandShortcutKeys)
				{
					return true;
				}
			}
			break;
		case Keys.Next:
			if (ScrollBar.ShowY)
			{
				ScrollBar.ValueY += rect_read.Height;
				if (HandShortcutKeys)
				{
					return true;
				}
			}
			break;
		case Keys.Left:
			if (IKeyLeft() && HandShortcutKeys)
			{
				return true;
			}
			break;
		case Keys.Right:
			if (IKeyRight() && HandShortcutKeys)
			{
				return true;
			}
			break;
		case Keys.Return:
		case Keys.Space:
			IKeyEnter();
			break;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private bool IKeyUp()
	{
		if (rows == null || _editControls.Count > 0)
		{
			return false;
		}
		if (selectedIndex.Length == 0)
		{
			ScrollBar.ValueY -= (int)(60f * Dpi);
		}
		else if (selectedIndex[0] > 1)
		{
			int num = selectedIndex[0] - 1;
			if (num > 0)
			{
				if (dataLen < num)
				{
					num = dataLen - 1;
				}
			}
			else
			{
				num = 0;
			}
			SelectedIndex = num;
			ScrollLine(num, rows);
		}
		return true;
	}

	private bool IKeyDown()
	{
		if (rows == null || _editControls.Count > 0)
		{
			return false;
		}
		if (selectedIndex.Length == 0)
		{
			ScrollBar.ValueY += (int)(60f * Dpi);
		}
		else if (selectedIndex[selectedIndex.Length - 1] < dataLen)
		{
			int i = (SelectedIndex = selectedIndex[selectedIndex.Length - 1] + 1);
			ScrollLine(i, rows);
		}
		else if (selectedIndex.Length > 1)
		{
			int i2 = (SelectedIndex = selectedIndex[selectedIndex.Length - 1]);
			ScrollLine(i2, rows);
		}
		return true;
	}

	private bool IKeyLeft()
	{
		if (rows == null || _editControls.Count > 0)
		{
			return false;
		}
		if (focusedxy == null || focusedxy[0] <= 0)
		{
			if (ScrollBar.ShowX)
			{
				ScrollBar.ValueX -= (int)(60f * Dpi);
			}
			return false;
		}
		try
		{
			RowTemplate rowTemplate = rows[focusedxy[1]];
			if (rowTemplate == null)
			{
				if (ScrollBar.ShowX)
				{
					ScrollBar.ValueX -= (int)(60f * Dpi);
				}
				return false;
			}
			CELL cELL = rowTemplate.cells[focusedxy[0] - 1];
			SetFocusedCell(cELL);
			ScrollColumn(cELL.COLUMN);
			return true;
		}
		catch
		{
		}
		if (ScrollBar.ShowX)
		{
			ScrollBar.ValueX -= (int)(60f * Dpi);
		}
		return false;
	}

	private bool IKeyRight()
	{
		if (rows == null || _editControls.Count > 0)
		{
			return false;
		}
		if (focusedxy == null)
		{
			if (ScrollBar.ShowX)
			{
				ScrollBar.ValueX += (int)(60f * Dpi);
			}
			return false;
		}
		int num = focusedxy[0] + 1;
		try
		{
			RowTemplate rowTemplate = rows[focusedxy[1]];
			if (rowTemplate == null)
			{
				if (ScrollBar.ShowX)
				{
					ScrollBar.ValueX += (int)(60f * Dpi);
				}
				return false;
			}
			if (num < rowTemplate.cells.Length)
			{
				CELL cELL = rowTemplate.cells[num];
				SetFocusedCell(cELL);
				ScrollColumn(cELL.COLUMN);
				return true;
			}
		}
		catch
		{
		}
		if (ScrollBar.ShowX)
		{
			ScrollBar.ValueX += (int)(60f * Dpi);
		}
		return false;
	}

	private void IKeyEnter()
	{
		try
		{
			if (selectedIndex.Length == 0)
			{
				return;
			}
			int num = selectedIndex[0];
			if (num > dataLen)
			{
				return;
			}
			RowTemplate rowTemplate = rows?[num];
			if (rowTemplate == null)
			{
				return;
			}
			ItemMouseClick(rowTemplate.RECORD, rowTemplate.Type, num, 0, FocusedColumn, RealRect(rowTemplate.RECT, ScrollBar.ValueX, ScrollBarRealY), new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
			if (editmode == TEditMode.None || focusedxy == null)
			{
				return;
			}
			if (inEditMode)
			{
				string text = "edit_" + focusedxy[0] + "_" + focusedxy[1];
				foreach (KeyValuePair<Input, EditControlTag> editControl in _editControls)
				{
					if (editControl.Key.Name == text)
					{
						return;
					}
				}
				if (EnableFocusNavigation)
				{
					NavigationConfig? navigationConfig = this.navigationConfig;
					if (navigationConfig != null && navigationConfig.Contains(rows.First.cells[focusedxy[0]].COLUMN.Key, out string _))
					{
						return;
					}
				}
			}
			_currentEdit = null;
			EnterEditMode(num, focusedxy[0]);
		}
		catch
		{
		}
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		if (LoadLayout())
		{
			Invalidate();
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		Rectangle clientRectangle = ClientRectangle;
		if (IsHandleCreated && clientRectangle.Width > 1 && clientRectangle.Height > 1)
		{
			LoadLayout(clientRectangle);
			EditModeClose();
			base.OnSizeChanged(e);
		}
	}

	public bool LoadLayout()
	{
		if (IsHandleCreated)
		{
			if (IsPauseLayout)
			{
				return false;
			}
			Rectangle clientRectangle = ClientRectangle;
			if (clientRectangle.Width > 1 && clientRectangle.Height > 1)
			{
				LoadLayout(clientRectangle);
			}
			return true;
		}
		return false;
	}

	private void LoadLayout(Rectangle rect_t)
	{
		LayoutDesign(rect_t.PaddingRect(Padding, Dpi, borderWidth / 2f));
	}

	private void LayoutDesign(Rectangle rect)
	{
		virtualMode_Y = (rowSummary = 0);
		has_check = false;
		if (dataTmp == null)
		{
			ThreadState?.Dispose();
			ThreadState = null;
			if (visibleHeader && emptyHeader && columns != null && columns.Count > 0)
			{
				List<RowTemplate> list = LayoutDesign(rect, new TempTable(new TempiColumn[0], new IRow[0], null), out bool _, out List<Column> Columns, out Dictionary<int, object> ColWidth, out int LastRowIndex);
				rows = LayoutDesign(rect, list, Columns, ColWidth, LastRowIndex, out var _x, out var _y, out var _is_exceed);
				ScrollBar.SetVrSize(_is_exceed ? _x : 0, _y, rect);
				return;
			}
		}
		else
		{
			List<RowTemplate> list2 = LayoutDesign(rect, dataTmp, out bool Processing2, out List<Column> Columns2, out Dictionary<int, object> ColWidth2, out int LastRowIndex2);
			if (Columns2.Count > 0)
			{
				if (visibleHeader && EmptyHeader && list2.Count == 0)
				{
					rows = LayoutDesign(rect, list2, Columns2, ColWidth2, LastRowIndex2, out var _x2, out var _y2, out var _is_exceed2);
					ScrollBar.SetVrSize(_is_exceed2 ? _x2 : 0, _y2, rect);
					ThreadState?.Dispose();
					ThreadState = null;
					return;
				}
				if (list2.Count > 0)
				{
					rows = LayoutDesign(rect, list2, Columns2, ColWidth2, LastRowIndex2, out var _x3, out var _y3, out var _is_exceed3);
					if (scrollBarAvoidHeader && visibleHeader && fixedHeader)
					{
						int headerHeight = rows.HeaderHeight;
						if (headerHeight > 0 && rect.Height > headerHeight)
						{
							_y3 -= headerHeight;
							rect = new Rectangle(rect.X, rect.Y + headerHeight, rect.Width, rect.Height - headerHeight);
						}
					}
					ScrollBar.SetVrSize(_is_exceed3 ? _x3 : 0, _y3, rect);
					if (Processing2 && Config.HasAnimation("Table"))
					{
						if (ThreadState == null)
						{
							ThreadState = new AnimationTask(new AnimationLinearConfig(this, (int i) =>
							{
								AnimationStateValue = (float)i / 100f;
								Invalidate();
								return true;
							}, 50, 100, 5));
						}
					}
					else
					{
						ThreadState?.Dispose();
						ThreadState = null;
					}
					return;
				}
			}
		}
		ThreadState?.Dispose();
		ThreadState = null;
		ScrollBar.SetVrSize(0, 0, Rectangle.Empty);
		dividers = new DividerList[0];
		rows = null;
	}

	private List<RowTemplate?> LayoutDesign(Rectangle rect, TempTable dataTmp, out bool Processing, out List<Column> Columns, out Dictionary<int, object> ColWidth, out int LastRowIndex)
	{
		int processing = 0;
		Dictionary<int, object> col_width = new Dictionary<int, object>();
		List<Column> _columns = new List<Column>(dataTmp.columns.Length);
		if (columns == null || columns.Count == 0)
		{
			ForColumn(dataTmp.columns, (Column it) =>
			{
				_columns.Add(it);
			});
		}
		else
		{
			int x = 0;
			ForColumn(columns, (Column it) =>
			{
				int count = _columns.Count;
				_columns.Add(it);
				if (it.Width != null)
				{
					col_width.Add(x, ColumnWidth(it.Width));
				}
				x++;
				return count;
			});
		}
		int start = 0;
		int end = dataTmp.rows.Length;
		List<RowTemplate?> _rows = new List<RowTemplate>(end);
		if (VirtualMode)
		{
			float dpi = Dpi;
			this.GDI((Canvas g) =>
			{
				int num2 = (int)((float)_gap.Height * Dpi) * 2;
				int num3 = g.MeasureString("龍Qq", Font).Height + num2;
				int value = num3;
				if (rowHeight.HasValue)
				{
					value = (num3 = (int)((float)rowHeight.Value * dpi));
				}
				if (rowHeightHeader.HasValue)
				{
					value = (int)((float)rowHeightHeader.Value * dpi);
				}
				_RowHeightHeader = value;
				_RowHeight = num3;
				int valueY = ScrollBar.ValueY;
				int num4 = (int)Math.Ceiling((double)rect.Height / (double)num3);
				start = (int)Math.Floor((double)valueY / (double)num3);
				end = start + num4;
				if (start > 0 && visibleHeader && !fixedHeader)
				{
					int num5 = start;
					start = num5 - 1;
				}
				virtualMode_Y = ((dataTmp.rows.Length != 0) ? (start * num3) : 0);
			});
		}
		else
		{
			_RowHeightHeader = (_RowHeight = null);
		}
		dataLen = ForRow(dataTmp);
		isMVVM = false;
		int lastRowIndex = 0;
		ForRow(dataTmp, start, end, (IRow? row2) =>
		{
			if (row2 == null)
			{
				_rows.Add(null);
			}
			else if (row2.add)
			{
				List<CELL> cells2 = new List<CELL>(_columns.Count);
				int check2 = 0;
				foreach (Column item in _columns)
				{
					AddRows(ref cells2, ref processing, ref check2, item, row2, item.Key);
				}
				if (cells2.Count > 0)
				{
					AddRows(ref _rows, cells2.ToArray(), row2, check2);
					lastRowIndex = _rows.Count;
				}
			}
		});
		if (dataTmp.summary != null)
		{
			IRow[] array = dataTmp.summary;
			foreach (IRow row in array)
			{
				List<CELL> cells = new List<CELL>(_columns.Count);
				int check = 0;
				foreach (Column item2 in _columns)
				{
					AddRows(ref cells, ref processing, ref check, item2, row, item2.Key, summary: true);
				}
				if (cells.Count > 0)
				{
					rowSummary++;
					AddRows(ref _rows, cells.ToArray(), row, 0).Type = RowType.Summary;
					lastRowIndex = _rows.Count;
				}
			}
		}
		dataOne = false;
		LastRowIndex = lastRowIndex;
		Processing = processing > 0;
		Columns = _columns;
		ColWidth = col_width;
		return _rows;
	}

	private RowList LayoutDesign(Rectangle rect, List<RowTemplate?> _rows, List<Column> _columns, Dictionary<int, object> col_width, int lastRowIndex, out int _x, out int _y, out bool _is_exceed)
	{
		List<TCellColumn> list = new List<TCellColumn>(_columns.Count);
		foreach (Column _column in _columns)
		{
			list.Add(new TCellColumn(this, _column));
		}
		AddRowsHeader(ref _rows, list.ToArray(), dataSource ?? _rows);
		int x = 0;
		int y = 0;
		bool is_exceed = false;
		Rectangle rect_real = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
		List<RowTemplate> rowlist = new List<RowTemplate>(_rows.Count);
		this.GDI((Canvas g) =>
		{
			float num = Dpi;
			Size font_size = g.MeasureString("龍Qq", Font);
			TableGaps tableGaps = new TableGaps(_gap, num);
			int num2 = (int)((float)_checksize * num);
			int check_size = (int)((float)_switchsize * num);
			int num3 = (int)((float)TreeButtonSize * num);
			int num4 = (int)((float)_gapTree * num);
			int num5 = num4 * 2;
			int sort_size = (int)((float)DragHandleSize * num);
			int sort_ico_size = (int)((float)DragHandleIconSize * num);
			int num6 = (int)(6f * num);
			check_radius = (float)num2 * 0.12f * num;
			check_border = (float)num2 * 0.04f * num;
			RowTemplate rowTemplate = _rows[0];
			Dictionary<int, AutoWidth> dictionary = new Dictionary<int, AutoWidth>(rowTemplate.cells.Length);
			for (int i = 0; i < rowTemplate.cells.Length; i++)
			{
				dictionary.Add(i, new AutoWidth());
			}
			Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
			int num7 = 0;
			for (int j = 0; j < _rows.Count; j++)
			{
				RowTemplate rowTemplate2 = _rows[j];
				if (rowTemplate2 != null)
				{
					rowTemplate2.INDEX = j;
					if (j == hovers)
					{
						rowTemplate2.hover = true;
					}
					int num8 = 0;
					if (rowTemplate2.IsColumn)
					{
						for (int k = 0; k < rowTemplate2.cells.Length; k++)
						{
							CELL cELL = rowTemplate2.cells[k];
							cELL.INDEX = k;
							Size size = cELL.GetSize(g, columnfont ?? Font, font_size, rect.Width, tableGaps);
							AutoWidth autoWidth = dictionary[k];
							if (cELL.COLUMN is ColumnSort)
							{
								autoWidth.value = -2;
							}
							else if (cELL.COLUMN is ColumnCheck { NoTitle: not false })
							{
								autoWidth.value = -1;
							}
							else
							{
								int num9 = size.Width;
								if (autoWidth.value < num9)
								{
									autoWidth.value = num9;
								}
								if (autoWidth.minvalue < cELL.MinWidth)
								{
									autoWidth.minvalue = cELL.MinWidth;
								}
							}
							if (num8 < size.Height)
							{
								num8 = size.Height;
							}
						}
						if (_RowHeightHeader.HasValue)
						{
							num8 = (rowTemplate2.Height = _RowHeightHeader.Value);
						}
						else if (rowHeightHeader.HasValue)
						{
							rowTemplate2.Height = (int)((float)rowHeightHeader.Value * num);
						}
						else if (rowHeight.HasValue)
						{
							rowTemplate2.Height = (int)((float)rowHeight.Value * num);
						}
						else
						{
							rowTemplate2.Height = num8 + tableGaps.y2;
						}
						if (visibleHeader)
						{
							num7 += rowTemplate2.Height;
						}
						dictionary2 = CalculateWidth(rect, VHeight(_rows.Count, num, rowTemplate2.Height), change: false, ref rect_real, col_width, dictionary, tableGaps.x2, num2, sort_size, ref is_exceed);
						List<int> list2 = new List<int>(dictionary2.Count);
						foreach (KeyValuePair<int, int> item in dictionary2)
						{
							if (col_width.TryGetValue(item.Key, out object value2) && value2 is float)
							{
								list2.Add(item.Key);
							}
						}
						if (list2.Count > 0)
						{
							foreach (int item2 in list2)
							{
								dictionary2.Remove(item2);
							}
						}
					}
					else
					{
						for (int l = 0; l < rowTemplate2.cells.Length; l++)
						{
							CELL cELL2 = rowTemplate2.cells[l];
							cELL2.INDEX = l;
							if (cELL2.COLUMN is ColumnSort || cELL2 is TCellCheck { NoTitle: not false })
							{
								if (num8 < tableGaps.y2)
								{
									num8 = tableGaps.y2;
								}
							}
							else
							{
								int num10 = rect.Width;
								if (dictionary2.TryGetValue(l, out var value3))
								{
									num10 = value3;
								}
								Size size2 = cELL2.GetSize(g, Font, font_size, num10, tableGaps);
								int num11 = size2.Width;
								if (cELL2.ROW.CanExpand && rowTemplate.cells[l].COLUMN.EqualsKeyTree(TreeKey))
								{
									num11 += (num3 + num5) * (cELL2.ROW.Depth + 1) - num3 / 2;
								}
								if (num8 < size2.Height)
								{
									num8 = size2.Height;
								}
								if (dictionary[l].value < num11)
								{
									dictionary[l].value = num11;
								}
							}
						}
						if (_RowHeight.HasValue)
						{
							num8 = (rowTemplate2.Height = _RowHeight.Value);
						}
						else if (rowHeight.HasValue)
						{
							rowTemplate2.Height = (int)((float)rowHeight.Value * num);
						}
						else
						{
							rowTemplate2.Height = num8 + tableGaps.y2;
						}
						num7 += rowTemplate2.Height;
					}
				}
			}
			CalculateWidth(rect, _columns, dictionary, col_width, rowTemplate);
			Dictionary<int, int> dictionary3 = CalculateWidth(rect, num7, change: true, ref rect_real, col_width, dictionary, tableGaps.x2, num2, sort_size, ref is_exceed);
			for (int m = 0; m < rowTemplate.cells.Length; m++)
			{
				rowTemplate.cells[m].COLUMN.WidthPixel = dictionary3[m];
			}
			if (StackedHeaderRows != null)
			{
				rowTemplate.Height += rowTemplate.Height * StackedHeaderRows.Length;
			}
			int num12 = ((!visibleHeader) ? (rect.Y - rowTemplate.Height) : rect.Y);
			bool flag = VirtualMode && virtualMode_Y != 0;
			bool flag2 = fixedHeader && visibleHeader;
			if (flag && !flag2)
			{
				num12 -= virtualMode_Y;
			}
			foreach (RowTemplate _row in _rows)
			{
				if (_row == null)
				{
					num12 += _RowHeight.Value;
				}
				else
				{
					if ((flag & flag2) && !_row.IsColumn)
					{
						num12 -= virtualMode_Y;
						flag = false;
					}
					rowlist.Add(_row);
					int num13 = rect.X;
					_row.RECT = new Rectangle(rect.X, num12, rect_real.Width, _row.Height);
					for (int n = 0; n < _row.cells.Length; n++)
					{
						CELL cELL3 = _row.cells[n];
						Rectangle rect2 = new Rectangle(num13, num12, dictionary3[n], _row.RECT.Height);
						int ox = 0;
						if (_row.INDEX > 0 && rowTemplate.cells[n].COLUMN.EqualsKeyTree(TreeKey))
						{
							int num14 = num4 * _row.Depth;
							ox = num14 + (num4 + num3);
							_row.RectExpand = new Rectangle(num13 + num14 + num6, num12 + (_row.Height - num3) / 2, num3, num3);
						}
						if (cELL3 is TCellCheck tCellCheck2)
						{
							tCellCheck2.SetSize(rect2, num2);
						}
						else if (cELL3 is TCellRadio tCellRadio)
						{
							tCellRadio.SetSize(rect2, num2);
						}
						else if (cELL3 is TCellSwitch tCellSwitch)
						{
							tCellSwitch.SetSize(rect2, check_size);
						}
						else if (cELL3 is TCellSort tCellSort)
						{
							tCellSort.SetSize(rect2, sort_size, sort_ico_size);
						}
						else if (cELL3 is TCellSelect tCellSelect)
						{
							tCellSelect.SetSize(g, Font, font_size, rect2, ox, tableGaps);
						}
						else if (cELL3 is TCellColumn tCellColumn)
						{
							cELL3.SetSize(g, Font, font_size, rect2, ox, tableGaps);
							if (tCellColumn.COLUMN is ColumnCheck { NoTitle: not false } columnCheck2)
							{
								tCellColumn.COLUMN.SortOrder = false;
								columnCheck2.PARENT = this;
								tCellColumn.RECT_REAL = new Rectangle(rect2.X + (rect2.Width - num2) / 2, rect2.Y + (rect2.Height - num2) / 2, num2, num2);
							}
							else
							{
								tCellColumn.RECT_REAL = new Rectangle(rect2.X + tableGaps.x, rect2.Y, rect2.Width - tableGaps.x2 - tCellColumn.SFWidth, rect2.Height);
								if (x < tCellColumn.RECT_REAL.Right)
								{
									x = tCellColumn.RECT_REAL.Right;
								}
							}
						}
						else
						{
							cELL3.SetSize(g, Font, font_size, rect2, ox, tableGaps);
						}
						if (x < rect2.Right)
						{
							x = rect2.Right;
						}
						if (y < rect2.Bottom)
						{
							y = rect2.Bottom;
						}
						num13 += dictionary3[n];
					}
					num12 += _row.Height;
				}
			}
			x -= rect_real.X;
			y -= rect_real.Y;
			y += virtualMode_Y;
			RowTemplate rowTemplate3 = LastRow(_rows, lastRowIndex);
			CELL obj = rowTemplate3.cells[rowTemplate3.cells.Length - 1];
			bool flag3 = emptyHeader && _rows.Count == 1;
			int num15 = obj.RECT.Bottom + virtualMode_Y;
			if (!flag3 && rect.Y + rect.Height > num15)
			{
				rect_real.Height = num15 - rect.Y + (int)Math.Ceiling(borderWidth / 2f * num);
			}
			rect_divider = new Rectangle(rect_real.X, rect_real.Y, rect_real.Width, rect_real.Height);
			List<MoveHeader> list3 = new List<MoveHeader>();
			Dictionary<int, MoveHeader> dictionary4 = new Dictionary<int, MoveHeader>(moveheaders.Length);
			MoveHeader[] array = moveheaders;
			foreach (MoveHeader moveHeader in array)
			{
				dictionary4.Add(moveHeader.i, moveHeader);
			}
			if (BorderCellWidth > 0f)
			{
				List<DividerList> list4 = new List<DividerList>(rowTemplate.cells.Length);
				List<DividerList> list5 = new List<DividerList>(_rows.Count);
				foreach (RowTemplate item3 in rowlist)
				{
					if (item3.IsColumn)
					{
						if (EnableHeaderResizing)
						{
							int num17 = num6 / 2;
							for (int num18 = 0; num18 < item3.cells.Length; num18++)
							{
								CELL cELL4 = item3.cells[num18];
								list3.Add(new MoveHeader(dictionary4, new Rectangle(cELL4.RECT.Right - num17, rect.Y, num6, cELL4.RECT.Height), num18, cELL4.RECT.Width, cELL4.MinWidth));
							}
						}
						if (bordered)
						{
							if (flag3)
							{
								for (int num19 = 0; num19 < item3.cells.Length - 1; num19++)
								{
									CELL cELL5 = item3.cells[num19];
									list4.Add(new DividerList(cELL5.RECT.Right, rect.Y, cELL5.RECT.Height));
								}
							}
							else
							{
								for (int num20 = 0; num20 < item3.cells.Length - 1; num20++)
								{
									CELL cELL6 = item3.cells[num20];
									list4.Add(new DividerList(cELL6.RECT.Right, rect.Y, rect_real.Height));
								}
							}
						}
						else
						{
							for (int num21 = 0; num21 < item3.cells.Length - 1; num21++)
							{
								CELL cELL7 = item3.cells[num21];
								list4.Add(new DividerList(cELL7.RECT.Right, cELL7.RECT.Y + tableGaps.y, cELL7.RECT.Height - tableGaps.y2));
							}
						}
						if (visibleHeader)
						{
							list5.Add(new DividerList(item3.RECT.Bottom, rect.X, rect_real.Width));
						}
					}
					else if (bordered)
					{
						list5.Add(new DividerList(item3.RECT.Bottom, rect.X, rect_real.Width));
					}
					else
					{
						list5.Add(new DividerList(item3.RECT.Bottom, item3.RECT.X, item3.RECT.Width));
					}
				}
				if (bordered && !flag3)
				{
					list5.RemoveAt(list5.Count - 1);
				}
				dividerHs = list4.ToArray();
				dividers = list5.ToArray();
			}
			else
			{
				dividers = new DividerList[0];
				dividerHs = new DividerList[0];
			}
			moveheaders = list3.ToArray();
		});
		_x = x;
		_y = y;
		rect_read = rect_real;
		_is_exceed = is_exceed;
		return new RowList(rowlist);
	}

	private RowTemplate LastRow(List<RowTemplate?> _rows, int index)
	{
		RowTemplate rowTemplate;
		for (rowTemplate = _rows[index]; rowTemplate == null; rowTemplate = _rows[index])
		{
			index--;
			if (index < 0)
			{
				throw new Exception("Data is empty");
			}
		}
		return rowTemplate;
	}

	private int VHeight(int rows, float dpi, int colHeight)
	{
		int num = 0;
		int num2 = rows - 1;
		if (visibleHeader)
		{
			num += colHeight;
		}
		if (_RowHeight.HasValue)
		{
			return num + _RowHeight.Value * num2;
		}
		if (rowHeight.HasValue)
		{
			return num + (int)((float)rowHeight.Value * dpi) * num2;
		}
		return num + colHeight * num2;
	}

	public virtual void OnShowXChanged(bool value)
	{
	}

	public virtual void OnShowYChanged(bool value)
	{
	}

	public virtual void OnValueXChanged(int value)
	{
		EditModeClose();
	}

	public virtual void OnValueYChanged(int value)
	{
		if (VirtualMode)
		{
			LoadLayout();
		}
		EditModeClose();
	}

	private void ForColumn(TempiColumn[] columns, Action<Column> action)
	{
		if (SortHeader == null)
		{
			foreach (TempiColumn tempiColumn in columns)
			{
				int num = 0;
				action(new Column(tempiColumn.key, tempiColumn.text ?? tempiColumn.key)
				{
					INDEX = num,
					INDEX_REAL = num
				});
				num++;
			}
		}
		else
		{
			int[] sortHeader = SortHeader;
			foreach (int num2 in sortHeader)
			{
				TempiColumn tempiColumn2 = columns[num2];
				action(new Column(tempiColumn2.key, tempiColumn2.text ?? tempiColumn2.key)
				{
					INDEX = num2,
					INDEX_REAL = num2
				});
			}
		}
	}

	private void ForColumn(ColumnCollection columns, Func<Column, int> action)
	{
		int num = 0;
		if (SortHeader == null)
		{
			foreach (Column column2 in columns)
			{
				column2.PARENT = this;
				column2.INDEX_REAL = num;
				if (column2.Visible)
				{
					column2.INDEX = action(column2);
				}
				num++;
			}
			return;
		}
		Dictionary<int, Column> dictionary = new Dictionary<int, Column>();
		foreach (Column column3 in columns)
		{
			column3.INDEX_REAL = num;
			dictionary.Add(dictionary.Count, column3);
			num++;
		}
		try
		{
			int[] sortHeader = SortHeader;
			foreach (int key in sortHeader)
			{
				Column column = dictionary[key];
				column.PARENT = this;
				if (column.Visible)
				{
					column.INDEX = action(column);
				}
			}
		}
		catch
		{
		}
	}

	private void ForColumnI(ColumnCollection columns, Action<Column> action)
	{
		if (SortHeader == null)
		{
			foreach (Column column2 in columns)
			{
				if (column2.Visible)
				{
					action(column2);
				}
			}
			return;
		}
		Dictionary<int, Column> dictionary = new Dictionary<int, Column>();
		foreach (Column column3 in columns)
		{
			dictionary.Add(dictionary.Count, column3);
		}
		try
		{
			int[] sortHeader = SortHeader;
			foreach (int key in sortHeader)
			{
				Column column = dictionary[key];
				if (column.Visible)
				{
					action(column);
				}
			}
		}
		catch
		{
		}
	}

	private void ForRow(TempTable data_temp, int i_start, int i_end, Action<IRow?> action)
	{
		int num = data_temp.rows.Length;
		int num2 = num - 1;
		if (SortData == null || SortData.Length != num)
		{
			IRow[] array = data_temp.rows;
			foreach (IRow row in array)
			{
				if (row.i == 0 || row.i == num2)
				{
					action(row);
				}
				else if (row.add && row.i_virtual > -1)
				{
					if (row.i_virtual >= i_start && row.i_virtual < i_end)
					{
						action(row);
					}
					else
					{
						action(null);
					}
				}
			}
			return;
		}
		for (int j = 0; j < num; j++)
		{
			if (j == 0 || j == num2 || (j >= i_start && j < i_end))
			{
				action(data_temp.rows[SortData[j]]);
			}
			else
			{
				action(null);
			}
		}
	}

	private int ForRow(TempTable data_temp)
	{
		Dictionary<object, IRow> dictionary = new Dictionary<object, IRow>(data_temp.rows.Length);
		int num = 0;
		IRow[] array = data_temp.rows;
		foreach (IRow row in array)
		{
			row.add = false;
			IRow value;
			if (row.fid == null)
			{
				row.add = true;
			}
			else if (dictionary.TryGetValue(row.fid, out value))
			{
				value.can_expand = true;
				if (value["__EXPAND__"] is bool expand)
				{
					value.expand = expand;
				}
				else if (DefaultExpand && dataOne && OnExpandChanged(row.fid, value: true))
				{
					value.expand = true;
				}
				if (value.expand)
				{
					row.add = true;
				}
			}
			if (row.add)
			{
				row.i_virtual = num;
				num++;
				if (row.has_expand)
				{
					dictionary.Add(row.record, row);
				}
			}
			else
			{
				row.i_virtual = -1;
			}
		}
		return num;
	}

	private void CalculateWidth(Rectangle rect, List<Column> _columns, Dictionary<int, AutoWidth> read_width_cell, Dictionary<int, object> col_width, RowTemplate firstrow)
	{
		foreach (KeyValuePair<int, AutoWidth> item in read_width_cell)
		{
			AutoWidth value = item.Value;
			value.MinWidth = ColumnWidth(_columns[item.Key].MinWidth, rect.Width);
			value.MaxWidth = ColumnWidth(_columns[item.Key].MaxWidth, rect.Width);
			if (value.MinWidth.HasValue && value.MaxWidth.HasValue && value.MaxWidth.Value < value.MinWidth.Value)
			{
				value.MaxWidth = value.MinWidth;
			}
			if (value.MinWidth.HasValue && value.MinWidth.Value > firstrow.cells[item.Key].MinWidth)
			{
				firstrow.cells[item.Key].MinWidth = value.MinWidth.Value;
			}
		}
	}

	private int ApplyWidthConstraints(int width, AutoWidth info)
	{
		if (info.MinWidth.HasValue && width < info.MinWidth.Value)
		{
			width = info.MinWidth.Value;
		}
		if (info.MaxWidth.HasValue && width > info.MaxWidth.Value)
		{
			width = info.MaxWidth.Value;
		}
		return width;
	}

	private int GetFillMinimumWidth(AutoWidth info)
	{
		int num = Math.Max(info.value, info.minvalue);
		if (info.MinWidth.HasValue)
		{
			num = Math.Max(num, info.MinWidth.Value);
		}
		if (info.MaxWidth.HasValue)
		{
			num = Math.Min(num, info.MaxWidth.Value);
		}
		return num;
	}

	/// <summary>
	/// 计算宽度
	/// </summary>
	/// <param name="rect">区域</param>
	/// <param name="heightEs">预估高度</param>
	/// <param name="col_width">表头宽度代码</param>
	/// <param name="read_width">每列的当前值和最大值</param>
	/// <param name="gap2">边距2</param>
	/// <param name="check_size">复选框大小</param>
	/// <param name="sort_size">拖拽大小</param>
	/// <param name="is_exceed">是否超出容器宽度</param>
	private Dictionary<int, int> CalculateWidth(Rectangle rect, int heightEs, bool change, ref Rectangle rect_read, Dictionary<int, object> col_width, Dictionary<int, AutoWidth> read_width, int gap2, int check_size, int sort_size, ref bool is_exceed)
	{
		int num = rect.Width;
		float num2 = 0f;
		Dictionary<int, float> dictionary = new Dictionary<int, float>(col_width.Count);
		foreach (KeyValuePair<int, AutoWidth> item in read_width)
		{
			object value2;
			if (tmpcol_width.TryGetValue(item.Key, out var value))
			{
				num2 += (float)value;
				dictionary.Add(item.Key, value);
			}
			else if (col_width.TryGetValue(item.Key, out value2))
			{
				float num3 = 0f;
				if (value2 is int num4)
				{
					num3 = num4 switch
					{
						-1 => (float)ApplyWidthConstraints(item.Value.value, item.Value), 
						-2 => GetFillMinimumWidth(item.Value), 
						_ => ApplyWidthConstraints(num4, item.Value), 
					};
				}
				else if (value2 is float num5)
				{
					int num6 = (int)Math.Ceiling((float)rect.Width * num5);
					num3 = ApplyWidthConstraints(num6, item.Value);
				}
				num2 += num3;
				dictionary.Add(item.Key, num3);
			}
			else if ((float)item.Value.value == -1f)
			{
				int num7 = check_size * 2;
				num2 += (float)num7;
				num -= num7;
			}
			else if ((float)item.Value.value == -2f)
			{
				int num8 = sort_size + gap2;
				num2 += (float)num8;
				num -= num8;
			}
			else
			{
				num2 += (float)item.Value.value;
			}
		}
		Dictionary<int, int> dictionary2;
		if (num2 > (float)rect.Width)
		{
			is_exceed = true;
			dictionary2 = new Dictionary<int, int>(read_width.Count);
			{
				foreach (KeyValuePair<int, AutoWidth> item2 in read_width)
				{
					object value4;
					if (tmpcol_width.TryGetValue(item2.Key, out var value3))
					{
						dictionary2.Add(item2.Key, value3);
					}
					else if (col_width.TryGetValue(item2.Key, out value4))
					{
						if (value4 is int num9)
						{
							switch (num9)
							{
							case -1:
								dictionary2.Add(item2.Key, ApplyWidthConstraints(item2.Value.value, item2.Value));
								break;
							case -2:
								dictionary2.Add(item2.Key, GetFillMinimumWidth(item2.Value));
								break;
							default:
								dictionary2.Add(item2.Key, ApplyWidthConstraints(num9, item2.Value));
								break;
							}
						}
						else if (value4 is float num10)
						{
							int num11 = (int)Math.Ceiling((float)rect.Width * num10);
							dictionary2.Add(item2.Key, ApplyWidthConstraints(num11, item2.Value));
						}
					}
					else if ((float)item2.Value.value == -1f)
					{
						dictionary2.Add(item2.Key, check_size * 2);
					}
					else if ((float)item2.Value.value == -2f)
					{
						dictionary2.Add(item2.Key, sort_size + gap2);
					}
					else
					{
						dictionary2.Add(item2.Key, item2.Value.value);
					}
				}
				return dictionary2;
			}
		}
		bool flag = heightEs > rect_read.Height;
		if (AutoSizeColumnsMode == ColumnsMode.Fill)
		{
			int num12 = (flag ? ScrollBar.SIZE : 0);
			num2 += (float)num12;
			dictionary2 = CalculateWidth(rect, col_width, read_width, num, num2, gap2, check_size, sort_size, out var sum_width);
			if (rect_read.Width > sum_width)
			{
				int num13 = rect_read.Width - num12;
				int num14 = 0;
				foreach (KeyValuePair<int, float> item3 in dictionary)
				{
					num14 += (int)Math.Round(item3.Value);
				}
				sum_width -= num14;
				int num15 = num13 - num14;
				Dictionary<int, int> dictionary3 = new Dictionary<int, int>(dictionary2.Count);
				foreach (KeyValuePair<int, int> item4 in dictionary2)
				{
					if (dictionary.TryGetValue(item4.Key, out var _))
					{
						dictionary3.Add(item4.Key, item4.Value);
					}
					else
					{
						dictionary3.Add(item4.Key, (int)Math.Round((double)num15 * ((double)item4.Value * 1.0 / (double)sum_width)));
					}
				}
				dictionary2 = dictionary3;
			}
			return dictionary2;
		}
		dictionary2 = ((!flag || !(num2 + (float)ScrollBar.SIZE > (float)num)) ? CalculateWidth(rect, col_width, read_width, num, null, gap2, check_size, sort_size, out var sum_width2) : CalculateWidth(rect, col_width, read_width, num, num2 += (float)ScrollBar.SIZE, gap2, check_size, sort_size, out sum_width2));
		if (change && rect_read.Width > sum_width2)
		{
			rect_read.Width = sum_width2;
		}
		return dictionary2;
	}

	private Dictionary<int, int> CalculateWidth(Rectangle rect, Dictionary<int, object> col_width, Dictionary<int, AutoWidth> read_width, int use_width, float? max_width, int gap2, int check_size, int sort_size, out int sum_width)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(read_width.Count);
		List<int> list = new List<int>(col_width.Count);
		foreach (KeyValuePair<int, AutoWidth> item in read_width)
		{
			object value2;
			if (tmpcol_width.TryGetValue(item.Key, out var value))
			{
				dictionary.Add(item.Key, value);
			}
			else if (col_width.TryGetValue(item.Key, out value2))
			{
				if (value2 is int num)
				{
					switch (num)
					{
					case -1:
						dictionary.Add(item.Key, ApplyWidthConstraints(item.Value.value, item.Value));
						break;
					case -2:
						list.Add(item.Key);
						break;
					default:
						dictionary.Add(item.Key, ApplyWidthConstraints(num, item.Value));
						break;
					}
				}
				else if (value2 is float num2)
				{
					int num3 = (int)Math.Ceiling((float)rect.Width * num2);
					dictionary.Add(item.Key, ApplyWidthConstraints(num3, item.Value));
				}
			}
			else if ((float)item.Value.value == -1f)
			{
				dictionary.Add(item.Key, check_size * 2);
			}
			else if ((float)item.Value.value == -2f)
			{
				dictionary.Add(item.Key, sort_size + gap2);
			}
			else if (max_width.HasValue)
			{
				dictionary.Add(item.Key, (int)Math.Ceiling((float)use_width * ((float)item.Value.value / max_width.Value)));
			}
			else
			{
				dictionary.Add(item.Key, item.Value.value);
			}
		}
		sum_width = 0;
		foreach (KeyValuePair<int, int> item2 in dictionary)
		{
			sum_width += item2.Value;
		}
		if (list.Count > 0)
		{
			DistributeFillWidths(rect.Width - sum_width, list, read_width, dictionary);
			sum_width = 0;
			foreach (KeyValuePair<int, int> item3 in dictionary)
			{
				sum_width += item3.Value;
			}
		}
		return dictionary;
	}

	private void DistributeFillWidths(int availableWidth, List<int> fillColumns, Dictionary<int, AutoWidth> readWidth, Dictionary<int, int> result)
	{
		if (fillColumns.Count == 0)
		{
			return;
		}
		List<int> list = new List<int>(fillColumns);
		Dictionary<int, int> dictionary = new Dictionary<int, int>(fillColumns.Count);
		Dictionary<int, int?> dictionary2 = new Dictionary<int, int?>(fillColumns.Count);
		foreach (int fillColumn in fillColumns)
		{
			AutoWidth autoWidth = readWidth[fillColumn];
			int fillMinimumWidth = GetFillMinimumWidth(autoWidth);
			int? value = autoWidth.MaxWidth;
			if (value.HasValue && value.Value < fillMinimumWidth)
			{
				value = fillMinimumWidth;
			}
			dictionary[fillColumn] = fillMinimumWidth;
			dictionary2[fillColumn] = value;
		}
		int num = 0;
		foreach (int value2 in dictionary.Values)
		{
			num += value2;
		}
		if (availableWidth <= num)
		{
			foreach (int fillColumn2 in fillColumns)
			{
				result[fillColumn2] = dictionary[fillColumn2];
			}
			return;
		}
		int num2 = availableWidth;
		while (list.Count > 0)
		{
			int num3 = num2 / list.Count;
			bool flag = false;
			for (int num4 = list.Count - 1; num4 >= 0; num4--)
			{
				int key = list[num4];
				int num5 = dictionary[key];
				int? num6 = dictionary2[key];
				if (num3 < num5)
				{
					result[key] = num5;
					num2 -= num5;
					list.RemoveAt(num4);
					flag = true;
				}
				else if (num6.HasValue && num3 > num6.Value)
				{
					result[key] = num6.Value;
					num2 -= num6.Value;
					list.RemoveAt(num4);
					flag = true;
				}
			}
			if (!flag)
			{
				int num7 = num2 / list.Count;
				int num8 = num2 % list.Count;
				for (int i = 0; i < list.Count; i++)
				{
					result[list[i]] = num7 + ((i < num8) ? 1 : 0);
				}
				break;
			}
		}
	}

	private int? ColumnWidth(string? max, int width)
	{
		if (max == null)
		{
			return null;
		}
		if (max.EndsWith("%") && float.TryParse(max.TrimEnd('%'), out var result))
		{
			return (int)((float)width * result / 100f);
		}
		if (int.TryParse(max, out var result2))
		{
			return (int)((float)result2 * Dpi);
		}
		return null;
	}

	/// <summary>
	/// 通过表头属性获取列宽/代码
	/// </summary>
	/// <param name="width">宽度属性</param>
	/// <returns>float 百分比/int 固定值/-2 FILL/-1 AUTO</returns>
	private object ColumnWidth(string width)
	{
		if (width.EndsWith("%") && float.TryParse(width.TrimEnd('%'), out var result))
		{
			return result / 100f;
		}
		if (int.TryParse(width, out var result2))
		{
			return (int)((float)result2 * Dpi);
		}
		if (width.Contains("fill"))
		{
			return -2;
		}
		return -1;
	}

	private void AddRows(ref List<CELL> cells, ref int processing, ref int check, Column column, IRow row, string key, bool summary = false)
	{
		object value4;
		if (summary)
		{
			if (row.cells.TryGetValue(key, out object value))
			{
				object value2 = OGetValue(value, row.record, out PropertyDescriptor property, out object value3);
				AddRows(ref cells, ref processing, ref check, column, value3, value2, property, summary);
			}
			else
			{
				AddRows(ref cells, ref processing, ref check, column, null, null, null, summary);
			}
		}
		else if (column is ColumnSort column2)
		{
			cells.Add(new TCellSort(this, column2));
		}
		else if (row.cells.TryGetValue(key, out value4))
		{
			object obj = OGetValue(value4, row.record, out PropertyDescriptor property2, out object value5);
			if (column.Render == null)
			{
				AddRows(ref cells, ref processing, ref check, column, value5, obj, property2, summary);
			}
			else
			{
				AddRows(ref cells, ref processing, ref check, column, value5, column.Render(obj, row.record, row.i), property2, summary);
			}
		}
		else
		{
			AddRows(ref cells, ref processing, ref check, column, null, column.Render?.Invoke(null, row.record, row.i), null, summary);
		}
	}

	/// <summary>
	/// 添加行
	/// </summary>
	/// <param name="cells">列</param>
	/// <param name="processing">动画</param>
	/// <param name="column">表头</param>
	/// <param name="ov">原始值</param>
	/// <param name="value">真值</param>
	/// <param name="prop">反射</param>
	private void AddRows(ref List<CELL> cells, ref int processing, ref int check, Column column, object? ov, object? value, PropertyDescriptor? prop, bool summary)
	{
		if (value == null)
		{
			cells.Add(new TCellText(this, column, prop, ov, null));
		}
		else
		{
			cells.Add(GetCELL(column, ref processing, ref check, ov, value, prop, summary));
		}
		if (ov is INotifyPropertyChanged notifyPropertyChanged)
		{
			isMVVM = true;
			notifyPropertyChanged.PropertyChanged -= Notify_PropertyChanged;
			notifyPropertyChanged.PropertyChanged += Notify_PropertyChanged;
		}
	}

	private CELL GetCELL(Column column, ref int processing, ref int check, object? ov, object value, PropertyDescriptor? prop, bool summary)
	{
		if (summary)
		{
			return AddCells(column, ref processing, ov, value, prop);
		}
		if (column is ColumnCheck column2)
		{
			has_check = true;
			bool checkValue = GetCheckValue(column2, value, out var val_int);
			if (checkValue)
			{
				check++;
			}
			return new TCellCheck(this, column2, prop, ov, checkValue, val_int);
		}
		if (column is ColumnRadio column3)
		{
			has_check = true;
			bool checkValue2 = GetCheckValue(column3, value, out var val_int2);
			if (checkValue2)
			{
				check++;
			}
			return new TCellRadio(this, column3, prop, ov, checkValue2, val_int2);
		}
		if (column is ColumnSwitch column4)
		{
			bool checkValue3 = GetCheckValue(column4, value, out var val_int3);
			return new TCellSwitch(this, column4, prop, ov, checkValue3, val_int3);
		}
		if (column is ColumnSelect column5)
		{
			return new TCellSelect(this, column5, prop, ov, value);
		}
		return AddCells(column, ref processing, ov, value, prop);
	}

	private CELL AddCells(Column column, ref int processing, object? ov, object value, PropertyDescriptor? prop)
	{
		if (value is IList<ICell> cels)
		{
			Template template = new Template(this, column, prop, ov, ref processing, cels);
			{
				foreach (ICell item in template.Value)
				{
					item.Changed = (bool layout) =>
					{
						if (layout)
						{
							LoadLayout();
						}
						Invalidate();
					};
				}
				return template;
			}
		}
		if (value is ICell cell)
		{
			Template template2 = new Template(this, column, prop, ov, ref processing, new ICell[1] { cell });
			{
				foreach (ICell item2 in template2.Value)
				{
					item2.Changed = (bool layout) =>
					{
						if (layout)
						{
							LoadLayout();
						}
						Invalidate();
					};
				}
				return template2;
			}
		}
		if (column is TemplateColumn templateColumn)
		{
			return templateColumn.CreateCell(this, templateColumn, prop, ov, ref processing, value);
		}
		return new TCellText(this, column, prop, ov, value);
	}

	private RowTemplate AddRows(ref List<RowTemplate?> rows, CELL[] cells, IRow row_data, int check_count)
	{
		RowTemplate rowTemplate = new RowTemplate(this, cells, row_data);
		if (enableDir.Contains(row_data.record))
		{
			rowTemplate.ENABLE = false;
		}
		CELL[] cells2 = rowTemplate.cells;
		for (int i = 0; i < cells2.Length; i++)
		{
			cells2[i].SetROW(rowTemplate);
		}
		if (check_count > 0)
		{
			rowTemplate.Select = true;
		}
		rows.Add(rowTemplate);
		return rowTemplate;
	}

	private RowTemplate AddRowsHeader(ref List<RowTemplate?> rows, TCellColumn[] cells, object record)
	{
		RowTemplate rowTemplate = new RowTemplate(this, cells, -1, record)
		{
			Type = RowType.Column
		};
		for (int i = 0; i < rowTemplate.cells.Length; i++)
		{
			CELL cELL = rowTemplate.cells[i];
			Column cOLUMN = cELL.COLUMN;
			ColumnCheck checkColumn = cOLUMN as ColumnCheck;
			if (checkColumn != null && checkColumn.NoTitle)
			{
				if (rows.Count > 0)
				{
					int num = rows.Count;
					int check_count = 0;
					if (VirtualMode && dataTmp != null)
					{
						num = dataTmp.rows.Length;
						ForRow(dataTmp, 0, dataTmp.rows.Length, (IRow? row) =>
						{
							if (row != null && GetCheckValue(checkColumn, row[checkColumn.Key], out var _))
							{
								check_count++;
							}
						});
					}
					else
					{
						for (int num2 = 0; num2 < rows.Count; num2++)
						{
							RowTemplate rowTemplate2 = rows[num2];
							if (rowTemplate2 != null)
							{
								if (rowTemplate2.Type == RowType.Summary)
								{
									num--;
								}
								else if (rowTemplate2.cells[i] is TCellCheck { Checked: not false })
								{
									check_count++;
								}
							}
						}
					}
					if (num == check_count)
					{
						checkColumn.CheckState = CheckState.Checked;
					}
					else if (check_count > 0)
					{
						checkColumn.CheckState = CheckState.Indeterminate;
					}
					else
					{
						checkColumn.CheckState = CheckState.Unchecked;
					}
				}
				else
				{
					checkColumn.CheckState = CheckState.Unchecked;
				}
			}
			cELL.SetROW(rowTemplate);
		}
		rows.Insert(0, rowTemplate);
		return rowTemplate;
	}

	private string? OGetValue(object record, object? value)
	{
		if (value is AntItem antItem)
		{
			object value2 = antItem.value;
			if (value2 is IList<ICell> list)
			{
				List<string> list2 = new List<string>(list.Count);
				foreach (ICell item in list)
				{
					string text = item.ToString();
					if (!string.IsNullOrEmpty(text))
					{
						list2.Add(text);
					}
				}
				return string.Join(" ", list2);
			}
			return value2?.ToString();
		}
		if (value is PropertyDescriptor propertyDescriptor)
		{
			object value3 = propertyDescriptor.GetValue(record);
			if (value3 is IList<ICell> list3)
			{
				List<string> list4 = new List<string>(list3.Count);
				foreach (ICell item2 in list3)
				{
					string text2 = item2.ToString();
					if (!string.IsNullOrEmpty(text2))
					{
						list4.Add(text2);
					}
				}
				return string.Join(" ", list4);
			}
			return value3?.ToString();
		}
		return value?.ToString();
	}

	private object? OGetValue(object? ov, object record, out PropertyDescriptor? property, out object? value)
	{
		value = ov;
		property = null;
		if (ov is AntItem antItem)
		{
			return antItem.value;
		}
		if (ov is PropertyDescriptor propertyDescriptor)
		{
			value = record;
			property = propertyDescriptor;
			return propertyDescriptor.GetValue(record);
		}
		if (ov is TableSubValue tableSubValue)
		{
			value = tableSubValue.record;
			property = tableSubValue.prop;
			return property.GetValue(value);
		}
		return ov;
	}

	internal bool GetCheckValue(ColumnICheck column, object? value, out bool val_int)
	{
		if (column._in == null)
		{
			return GetCheckValue(value, out val_int);
		}
		val_int = false;
		return column._in(value);
	}

	internal bool GetCheckValueAuto(Column column, object? value, out bool val_int)
	{
		if (column is ColumnICheck column2)
		{
			return GetCheckValue(column2, value, out val_int);
		}
		return GetCheckValue(value, out val_int);
	}

	internal bool GetCheckValue(object? value, out bool val_int)
	{
		val_int = false;
		if (value is bool)
		{
			return (bool)value;
		}
		if (value is int num)
		{
			val_int = true;
			return num > 0;
		}
		return false;
	}

	private void Notify_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (sender != null && e.PropertyName != null)
		{
			PropertyChanged(sender, e.PropertyName);
		}
	}

	private void PropertyChanged(object data, string key)
	{
		if (rows == null || key == null)
		{
			return;
		}
		if (key == "__EXPAND__")
		{
			LoadLayout();
			Invalidate();
			return;
		}
		for (int i = 0; i < rows.First.cells.Length; i++)
		{
			CELL cELL = rows.First.cells[i];
			if (key == cELL.COLUMN.Key)
			{
				RowTemplate[] list;
				if (data is AntItem antItem)
				{
					list = rows.List;
					foreach (RowTemplate rowTemplate in list)
					{
						if (rowTemplate.RECORD is IList<AntItem> list2 && list2.Contains(antItem))
						{
							RefreshItem(rows.List, rowTemplate, rowTemplate.cells[i], i, antItem.value);
							break;
						}
					}
					break;
				}
				list = rows.List;
				foreach (RowTemplate rowTemplate2 in list)
				{
					if (rowTemplate2.RECORD == data)
					{
						CELL cELL2 = rowTemplate2.cells[i];
						if (cELL2.PROPERTY != null)
						{
							RefreshItem(rows.List, rowTemplate2, cELL2, i, cELL2.PROPERTY.GetValue(data));
						}
						break;
					}
				}
				break;
			}
			if (key == cELL.COLUMN.KeyTree)
			{
				int length = rows.Length;
				LoadLayout();
				int length2 = rows.Length;
				if (selectedIndex.Length != 0 && length2 < length)
				{
					SetIndex(selectedIndex[0] - length - length2);
				}
				Invalidate();
				break;
			}
		}
	}

	private void RefreshItem(RowTemplate[] rows, RowTemplate row, CELL cel, int cel_i, object? value)
	{
		bool val_int2;
		bool val_int;
		if (cel is Template template)
		{
			int num = 0;
			if (value == null)
			{
				num++;
			}
			else if (value is IList<ICell> list)
			{
				if (template.Value.Count == list.Count)
				{
					for (int i = 0; i < template.Value.Count; i++)
					{
						template.Value[i] = list[i];
						num++;
					}
				}
				else
				{
					num++;
				}
			}
			else if (value is ICell value2)
			{
				if (template.Value.Count == 1)
				{
					template.Value[0] = value2;
					num++;
				}
				else
				{
					num++;
				}
			}
			else
			{
				num++;
			}
			if (num > 0)
			{
				LoadLayout();
				Invalidate();
			}
		}
		else if (cel is TCellText tCellText)
		{
			if (value is IList<ICell> || value is ICell)
			{
				LoadLayout();
			}
			else
			{
				tCellText.SetValue(value);
			}
			Invalidate();
		}
		else if (cel is TCellCheck tCellCheck)
		{
			if (cel.COLUMN is ColumnCheck columnCheck)
			{
				bool checkValue = GetCheckValue(columnCheck, value, out val_int);
				if (tCellCheck.Checked != checkValue)
				{
					OnCheckedChanged(checkValue, row.RECORD, row.INDEX, cel_i, columnCheck);
				}
				val_int = (tCellCheck.Checked = checkValue);
				row.Select = val_int;
				if (IsPauseLayout)
				{
					return;
				}
				if (columnCheck.NoTitle)
				{
					IsCheckAll(cel_i, columnCheck);
				}
			}
			else
			{
				val_int = (tCellCheck.Checked = GetCheckValue(value, out val_int2));
				row.Select = val_int;
			}
			Invalidate();
		}
		else if (cel is TCellRadio tCellRadio)
		{
			val_int = (tCellRadio.Checked = GetCheckValueAuto(cel.COLUMN, value, out val_int2));
			row.Select = val_int;
			Invalidate();
		}
		else if (cel is TCellSwitch tCellSwitch)
		{
			tCellSwitch.Checked = GetCheckValueAuto(cel.COLUMN, value, out val_int);
			Invalidate();
		}
		else
		{
			LoadLayout();
			Invalidate();
		}
	}

	internal void CheckAll(int i_cel, ColumnCheck columnCheck, bool value)
	{
		if (rows == null)
		{
			return;
		}
		int t_count = 0;
		pauseLayoutByCheckAll = true;
		if (columnCheck._out == null)
		{
			if (VirtualMode)
			{
				if (dataTmp == null)
				{
					return;
				}
				List<int> index = new List<int>(dataTmp.rows.Length);
				int value_int = (value ? 1 : 0);
				ForRow(dataTmp, 0, dataTmp.rows.Length, (IRow? row) =>
				{
					if (row != null)
					{
						int num3 = t_count;
						t_count = num3 + 1;
						object obj = row[columnCheck.Key];
						if (obj is bool flag4)
						{
							if (flag4 == value)
							{
								index.Add(row.i);
								return;
							}
							row.SetValue(columnCheck.Key, value);
							OnCheckedChanged(value, row.record, row.i, i_cel, columnCheck);
						}
						else if (obj is int num4)
						{
							if (num4 == value_int)
							{
								index.Add(row.i);
								return;
							}
							row.SetValue(columnCheck.Key, value_int);
							OnCheckedChanged(value, row.record, row.i, i_cel, columnCheck);
						}
						index.Add(row.i);
					}
				});
				rows.SetSelect(value);
			}
			else
			{
				for (int num = 1; num < rows.Length; num++)
				{
					CELL cELL = rows.List[num].cells[i_cel];
					if (cELL.ROW.Type == RowType.Summary || !(cELL is TCellCheck tCellCheck))
					{
						continue;
					}
					t_count++;
					if (tCellCheck.Checked != value)
					{
						RowTemplate rOW = cELL.ROW;
						bool flag = (tCellCheck.Checked = value);
						rOW.Select = flag;
						if (tCellCheck.ValInt)
						{
							SetValue(cELL, tCellCheck.Checked ? 1 : 0);
							OnCheckedChanged(value, rows.List[num].RECORD, num, i_cel, cELL.COLUMN);
						}
						else
						{
							SetValue(cELL, tCellCheck.Checked);
							OnCheckedChanged(value, rows.List[num].RECORD, num, i_cel, cELL.COLUMN);
						}
					}
				}
			}
		}
		else if (VirtualMode)
		{
			if (dataTmp == null)
			{
				return;
			}
			List<int> index2 = new List<int>(dataTmp.rows.Length);
			ForRow(dataTmp, 0, dataTmp.rows.Length, (IRow? row) =>
			{
				if (row != null)
				{
					int num3 = t_count;
					t_count = num3 + 1;
					row.SetValue(columnCheck.Key, columnCheck._out(value));
					OnCheckedChanged(value, row.record, row.i, i_cel, columnCheck);
					index2.Add(row.i);
				}
			});
			rows.SetSelect(value);
		}
		else
		{
			for (int num2 = 1; num2 < rows.Length; num2++)
			{
				CELL cELL2 = rows.List[num2].cells[i_cel];
				if (cELL2.ROW.Type != RowType.Summary && cELL2 is TCellCheck tCellCheck2)
				{
					t_count++;
					if (tCellCheck2.Checked != value)
					{
						RowTemplate rOW2 = cELL2.ROW;
						bool flag = (tCellCheck2.Checked = value);
						rOW2.Select = flag;
						SetValue(cELL2, columnCheck._out(value));
						OnCheckedChanged(value, rows.List[num2].RECORD, num2, i_cel, cELL2.COLUMN);
					}
				}
			}
		}
		pauseLayoutByCheckAll = false;
		if (t_count > 0)
		{
			if (value)
			{
				columnCheck.CheckState = CheckState.Checked;
			}
			else
			{
				columnCheck.CheckState = CheckState.Unchecked;
			}
		}
	}

	internal void IsCheckAll(int cel, ColumnCheck column)
	{
		int t_count = 0;
		int check_count = 0;
		if (VirtualMode)
		{
			if (dataTmp == null)
			{
				return;
			}
			ForRow(dataTmp, 0, dataTmp.rows.Length, (IRow? row) =>
			{
				if (row != null)
				{
					t_count++;
					if (GetCheckValue(column, row[column.Key], out var _))
					{
						check_count++;
					}
				}
			});
		}
		else
		{
			if (rows == null)
			{
				return;
			}
			t_count = rows.Length - 1;
			for (int num = 1; num < rows.Length; num++)
			{
				RowTemplate rowTemplate = rows.List[num];
				if (rowTemplate.Type == RowType.Summary)
				{
					t_count--;
				}
				else if (rowTemplate.cells[cel] is TCellCheck { Checked: not false })
				{
					check_count++;
				}
			}
		}
		if (t_count == check_count)
		{
			column.CheckState = CheckState.Checked;
		}
		else if (check_count > 0)
		{
			column.CheckState = CheckState.Indeterminate;
		}
		else
		{
			column.CheckState = CheckState.Unchecked;
		}
	}

	private void SetValueCheckRow(RowTemplate row)
	{
		int num = 0;
		CELL[] cells = row.cells;
		foreach (CELL cELL in cells)
		{
			if (cELL is TCellCheck tCellCheck)
			{
				if (tCellCheck.Checked)
				{
					num++;
				}
			}
			else if (cELL is TCellRadio { Checked: not false })
			{
				num++;
			}
		}
		row.Select = num > 0;
	}

	private void SetValueCheck(CELL_CHECK cel)
	{
		SetValueCheck(cel, !cel.Checked);
	}

	private void SetValueCheck(CELL_CHECK cel, bool value)
	{
		cel.Checked = value;
		SetValueCheckRow(cel.ROW);
		if (cel.COLUMN is ColumnICheck { _out: not null } columnICheck)
		{
			SetValue(cel, columnICheck._out(value));
		}
		else if (cel.ValInt)
		{
			SetValue(cel, value ? 1 : 0);
		}
		else
		{
			SetValue(cel, value);
		}
	}

	private void SetValue(CELL cel, object? value)
	{
		if (cel.PROPERTY == null)
		{
			if (cel.VALUE is AntItem antItem)
			{
				antItem.value = value;
			}
			else if (cel.ROW.RECORD is DataRow dataRow)
			{
				int iNDEX_REAL = cel.COLUMN.INDEX_REAL;
				int iNDEX_REAL2 = cel.ROW.INDEX_REAL;
				if (dataRow.Table.Columns.Contains(cel.COLUMN.Key))
				{
					string key = cel.COLUMN.Key;
					object value2 = (cel.VALUE = value);
					dataRow[key] = value2;
				}
				else
				{
					object value2 = (cel.VALUE = value);
					dataRow[iNDEX_REAL] = value2;
				}
				if (dataTmp != null)
				{
					dataTmp.rows[iNDEX_REAL2].SetValue(cel.COLUMN.Key, value);
				}
			}
		}
		else
		{
			cel.PROPERTY.SetValue(cel.VALUE, value);
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		cellMouseDown = null;
		btnMouseDown = null;
		if (ClipboardCopy)
		{
			Focus();
		}
		subForm?.IClose();
		subForm = null;
		CloseTip();
		mouseClicks = e.Clicks;
		bool flag = mouseClicks > 1;
		if (!ScrollBar.MouseDownY(e.X, e.Y) || !ScrollBar.MouseDownX(e.X, e.Y))
		{
			return;
		}
		base.OnMouseDown(e);
		if (rows == null)
		{
			return;
		}
		OnTouchDown(e.X, e.Y);
		CELLDB cELLDB = CellContains(rows.List, sethover: true, e.X, e.Y);
		if (cELLDB == null || cELLDB.mode == CELLDBMode.Summary)
		{
			SetFocusedCell(null);
		}
		else
		{
			if (!OnCellClickBegin(cELLDB.cell.ROW.RECORD, cELLDB.cell.ROW.Type, cELLDB.i_row, cELLDB.i_cel, cELLDB.col, RealRect(cELLDB.cell.RECT, cELLDB.offset_xi, cELLDB.offset_y), e))
			{
				return;
			}
			if ((CellFocusedStyle ?? Config.DefaultCellFocusedStyle) == TableCellFocusedStyle.None)
			{
				SetFocusedCell(null);
			}
			else
			{
				SetFocusedCell(cELLDB.cell);
			}
			if (dataSource is BindingSource bindingSource)
			{
				bindingSource.Position = cELLDB.i_row - 1;
			}
			RowTemplate rOW = cELLDB.cell.ROW;
			if (cELLDB.mode > CELLDBMode.None)
			{
				if (moveheaders.Length != 0)
				{
					MoveHeader[] array = moveheaders;
					foreach (MoveHeader moveHeader in array)
					{
						if (moveHeader.rect.Contains(cELLDB.x, cELLDB.y))
						{
							moveHeader.x = e.X;
							Window.CanHandMessage = false;
							moveHeader.MouseDown = true;
							return;
						}
					}
				}
				cellMouseDown = new DownCellTMP<CELL>(rOW, cELLDB.cell, cELLDB);
				if (!flag && cELLDB.col is ColumnCheck { NoTitle: not false } columnCheck && e.Button == MouseButtons.Left && cELLDB.cell.CONTAIN_REAL(cELLDB.x, cELLDB.y))
				{
					CheckAll(cELLDB.i_cel, columnCheck, !columnCheck.Checked);
				}
				else if ((!(cELLDB.cell is TCellColumn tCellColumn) || (!tCellColumn.rect_up.Contains(cELLDB.x - cELLDB.offset_x, cELLDB.y - cELLDB.offset_xi) && !tCellColumn.rect_down.Contains(cELLDB.x - cELLDB.offset_x, cELLDB.y - cELLDB.offset_xi) && (cELLDB.col.Filter == null || !tCellColumn.rect_filter.Contains(cELLDB.x - cELLDB.offset_x, cELLDB.y - cELLDB.offset_xi)))) && ColumnDragSort && cELLDB.col.DragSort)
				{
					dragHeader = new DragHeader(e.X, e.Y, cELLDB.col.INDEX_REAL, e.X);
				}
			}
			else if (cELLDB.col is ColumnSort && cELLDB.cell.CONTAIN_REAL(cELLDB.x, cELLDB.y))
			{
				dragBody = new DragHeader(e.X, e.Y, cELLDB.cell.ROW.INDEX, e.Y);
			}
			else if (cELLDB.cell.ROW.CanExpand && cELLDB.cell.ROW.RectExpand.Contains(cELLDB.x, cELLDB.y))
			{
				bool flag2 = !cELLDB.cell.ROW.Expand;
				if (!OnExpandChanged(cELLDB.cell.ROW.RECORD, flag2))
				{
					return;
				}
				if (cELLDB.cell.ROW.RD == null)
				{
					cELLDB.cell.ROW.Expand = flag2;
				}
				else
				{
					RowTemplate rOW2 = cELLDB.cell.ROW;
					bool expand = (cELLDB.cell.ROW.RD.expand = flag2);
					rOW2.Expand = expand;
					if (cELLDB.cell.ROW.RD.SetValue("__EXPAND__", flag2) && isMVVM)
					{
						return;
					}
				}
				if (LoadLayout())
				{
					Invalidate();
				}
			}
			else
			{
				MouseDownRow(e, rOW, cELLDB);
			}
		}
	}

	private void MouseDownRow(MouseEventArgs e, RowTemplate it, CELLDB db)
	{
		cellMouseDown = new DownCellTMP<CELL>(it, db.cell, db);
		if (!(db.cell is Template template))
		{
			return;
		}
		if (e.Button == MouseButtons.Left)
		{
			foreach (ICell item in template.Value)
			{
				if (item is CellLink { Enabled: not false, Rect: var rect } cellLink && rect.Contains(db.x, db.y))
				{
					btnMouseDown = new DownCellTMP<CellLink>(it, cellLink, db);
					cellLink.ExtraMouseDown = true;
					OnCellButtonDown(cellLink, it.RECORD, it.Type, db.i_row, db.i_cel, db.col, RealRect(cellLink.Rect, db.offset_xi, db.offset_y), e);
					break;
				}
			}
			return;
		}
		foreach (ICell item2 in template.Value)
		{
			if (item2 is CellLink { Enabled: not false, Rect: var rect2 } cellLink2 && rect2.Contains(db.x, db.y))
			{
				btnMouseDown = new DownCellTMP<CellLink>(it, cellLink2, db);
				OnCellButtonDown(cellLink2, it.RECORD, it.Type, db.i_row, db.i_cel, db.col, RealRect(cellLink2.Rect, db.offset_xi, db.offset_y), e);
				break;
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		DownCellTMP<CELL> downCellTMP = cellMouseDown;
		DownCellTMP<CellLink> downCellTMP2 = btnMouseDown;
		cellMouseDown = null;
		btnMouseDown = null;
		if (moveheaders.Length != 0)
		{
			MoveHeader[] array = moveheaders;
			foreach (MoveHeader moveHeader in array)
			{
				if (moveHeader.MouseDown)
				{
					int num = moveHeader.width + e.X - moveHeader.x;
					if (num < moveHeader.min_width)
					{
						num = moveHeader.min_width;
					}
					if (tmpcol_width.ContainsKey(moveHeader.i))
					{
						tmpcol_width[moveHeader.i] = num;
					}
					else
					{
						tmpcol_width.Add(moveHeader.i, num);
					}
					moveHeader.MouseDown = false;
					Window.CanHandMessage = true;
					LoadLayout();
					Invalidate();
					OnTouchCancel();
					return;
				}
			}
		}
		if (dragHeader != null)
		{
			bool hand = dragHeader.hand;
			if (hand && dragHeader.enable && dragHeader.im != -1)
			{
				if (columns == null)
				{
					return;
				}
				List<int> list = new List<int>(columns.Count);
				if (SortHeader == null)
				{
					foreach (Column column in columns)
					{
						list.Add(column.INDEX_REAL);
					}
				}
				else
				{
					list.AddRange(SortHeader);
				}
				int num2 = list.IndexOf(dragHeader.i);
				int num3 = list.IndexOf(dragHeader.im);
				int num4 = list[num2];
				if (ColumnIndexChanging != null)
				{
					TableColumnIndexChangingEventArgs e2 = new TableColumnIndexChangingEventArgs(num2, num4, num3);
					ColumnIndexChanging(this, e2);
					if (e2.Cancel)
					{
						Invalidate();
						OnTouchCancel();
						return;
					}
				}
				list.RemoveAt(num2);
				list.Insert(num3, num4);
				SortHeader = list.ToArray();
				ExtractHeaderFixed();
				LoadLayout();
				ColumnIndexChanged?.Invoke(this, new TableColumnIndexChangedEventArgs(num2, num4, num3));
			}
			dragHeader = null;
			if (hand)
			{
				Invalidate();
				OnTouchCancel();
				return;
			}
		}
		if (dragBody != null)
		{
			bool hand2 = dragBody.hand;
			if (hand2 && dragBody.enable && dragBody.im != -1)
			{
				if (rows == null)
				{
					return;
				}
				List<int> list2 = new List<int>(rows.Length);
				int num5 = dragBody.im;
				int num6 = dragBody.i;
				RowTemplate[] list3 = rows.List;
				foreach (RowTemplate rowTemplate in list3)
				{
					rowTemplate.hover = false;
					if (dragBody.im == rowTemplate.INDEX)
					{
						rowTemplate.hover = true;
						num5 = rowTemplate.INDEX_REAL;
					}
					if (dragBody.i == rowTemplate.INDEX)
					{
						num6 = rowTemplate.INDEX_REAL;
					}
				}
				if (num5 == num6)
				{
					RowTemplate rowTemplate2 = DragBodyTree(rows.List, dragBody);
					if (rowTemplate2 != null)
					{
						int iNDEX_REAL_KEY = rows.List[dragBody.i].INDEX_REAL_KEY;
						int iNDEX_REAL_KEY2 = rows.List[dragBody.im].INDEX_REAL_KEY;
						if (OnSortRowsTree(rowTemplate2.RECORD, iNDEX_REAL_KEY, iNDEX_REAL_KEY2) && LoadLayout())
						{
							Invalidate();
						}
					}
				}
				else
				{
					SetIndex(dragBody.im);
					list3 = rows.List;
					for (int i = 0; i < list3.Length; i++)
					{
						int iNDEX_REAL = list3[i].INDEX_REAL;
						if (iNDEX_REAL <= -1)
						{
							continue;
						}
						if (iNDEX_REAL == num5)
						{
							if (dragBody.last)
							{
								list2.Add(iNDEX_REAL);
							}
							if (list2.Contains(num6))
							{
								list2.Remove(num6);
							}
							list2.Add(num6);
						}
						if (!list2.Contains(iNDEX_REAL))
						{
							list2.Add(iNDEX_REAL);
						}
					}
					SortData = list2.ToArray();
					LoadLayout();
					OnSortRows(-1);
				}
			}
			dragBody = null;
			if (hand2)
			{
				Invalidate();
				OnTouchCancel();
				return;
			}
		}
		if (!ScrollBar.MouseUpY() || !ScrollBar.MouseUpX() || rows == null)
		{
			return;
		}
		if (OnTouchUp())
		{
			if (downCellTMP == null)
			{
				EditModeClose();
				if (summaryCustomize && e.Button == MouseButtons.Right)
				{
					CELLDB cELLDB = CellContains(rows.List, sethover: false, e.X, e.Y);
					if (cELLDB != null && cELLDB.mode == CELLDBMode.Summary)
					{
						Summary_RClick(cELLDB);
					}
				}
				ItemMouseClick(e);
			}
			else
			{
				MouseUpRow(rows.List, downCellTMP, downCellTMP2, e);
			}
		}
		else if (downCellTMP2 == null)
		{
			EditModeClose();
		}
		else if (downCellTMP2.cell.ExtraMouseDown)
		{
			OnCellButtonUp(downCellTMP2.cell, downCellTMP2.row.RECORD, downCellTMP2.row.Type, downCellTMP2.i_row, downCellTMP2.i_cel, downCellTMP2.cell.PARENT.COLUMN, RealRect(downCellTMP2), e);
			downCellTMP2.cell.ExtraMouseDown = false;
		}
	}

	private RowTemplate? DragBodyTree(RowTemplate[] rows, DragHeader dragBody)
	{
		int num = rows[dragBody.i].Depth - 1;
		for (int num2 = dragBody.i - 1; num2 > 1; num2--)
		{
			if (rows[num2].Depth == num)
			{
				return rows[num2];
			}
		}
		return null;
	}

	internal bool Filter_PopupEndEventMethod(FilterOption option)
	{
		return OnFilterPopupEnd(option);
	}

	private void MouseUpRow(RowTemplate[] rows, DownCellTMP<CELL> it, DownCellTMP<CellLink>? btn, MouseEventArgs e)
	{
		CELLDB cELLDB = CellContains(rows, sethover: true, e.X, e.Y);
		if (cELLDB == null || cELLDB.mode == CELLDBMode.Summary)
		{
			shift_index = -1;
			MouseUpBtn(it, btn, e);
			ItemMouseClick(e);
			return;
		}
		if (it.i_row != cELLDB.i_row || it.i_cel != cELLDB.i_cel)
		{
			MouseUpBtn(it, btn, e, cELLDB);
			return;
		}
		if (MouseClickPenetration || MouseUpRowBefore(rows, it, btn, e, cELLDB))
		{
			if (e.Button == MouseButtons.Left)
			{
				if (MultipleRows && Control.ModifierKeys.HasFlag(Keys.Shift))
				{
					if (shift_index == -1)
					{
						SelectedIndexs = SetIndexs(cELLDB.i_row);
					}
					else if (shift_index > cELLDB.i_row)
					{
						SelectedIndexs = SetIndexs(cELLDB.i_row, shift_index);
					}
					else
					{
						SelectedIndexs = SetIndexs(shift_index, cELLDB.i_row);
					}
				}
				else if (MultipleRows && Control.ModifierKeys.HasFlag(Keys.Control))
				{
					SelectedIndexs = SetIndexs(cELLDB.i_row);
				}
				else
				{
					SelectedIndex = cELLDB.i_row;
				}
			}
			shift_index = cELLDB.i_row;
		}
		if (MouseUpBtn(it, btn, e, cELLDB))
		{
			return;
		}
		if (e.Button == MouseButtons.Left)
		{
			if (it.cell is TCellCheck tCellCheck)
			{
				if (tCellCheck.CONTAIN_REAL(cELLDB.x, cELLDB.y))
				{
					if (tCellCheck.COLUMN is ColumnCheck { Call: not null } columnCheck)
					{
						bool flag = columnCheck.Call(!tCellCheck.Checked, it.row.RECORD, it.i_row, it.i_cel);
						if (tCellCheck.Checked != flag)
						{
							SetValueCheck(tCellCheck, flag);
							OnCheckedChanged(tCellCheck.Checked, it.row.RECORD, it.i_row, it.i_cel, cELLDB.col);
						}
					}
					else if (tCellCheck.AutoCheck)
					{
						SetValueCheck(tCellCheck);
						OnCheckedChanged(tCellCheck.Checked, it.row.RECORD, it.i_row, it.i_cel, cELLDB.col);
					}
				}
			}
			else if (it.cell is TCellRadio tCellRadio)
			{
				if (tCellRadio.CONTAIN_REAL(cELLDB.x, cELLDB.y) && !tCellRadio.Checked)
				{
					bool flag2 = false;
					if (tCellRadio.COLUMN is ColumnRadio { Call: not null } columnRadio)
					{
						if (columnRadio.Call(arg1: true, it.row.RECORD, it.i_row, it.i_cel))
						{
							flag2 = true;
						}
					}
					else if (tCellRadio.AutoCheck)
					{
						flag2 = true;
					}
					if (flag2)
					{
						for (int i = 0; i < rows.Length; i++)
						{
							if (i != it.i_row && rows[i].cells[it.i_cel] is TCellRadio { Checked: not false } tCellRadio2)
							{
								SetValueCheck(tCellRadio2, value: false);
							}
						}
						SetValueCheck(tCellRadio, value: true);
						OnCheckedChanged(tCellRadio.Checked, it.row.RECORD, it.i_row, it.i_cel, cELLDB.col);
					}
				}
			}
			else
			{
				CELL cell = it.cell;
				TCellSwitch switchCell = cell as TCellSwitch;
				if (switchCell != null)
				{
					if (switchCell.CONTAIN_REAL(cELLDB.x, cELLDB.y) && !switchCell.Loading)
					{
						Column cOLUMN = switchCell.COLUMN;
						ColumnSwitch columnSwitch = cOLUMN as ColumnSwitch;
						if (columnSwitch != null && columnSwitch.Call != null)
						{
							switchCell.Loading = true;
							ITask.Run(() =>
							{
								bool flag4 = columnSwitch.Call(!switchCell.Checked, it.row.RECORD, it.i_row, it.i_cel);
								if (switchCell.Checked != flag4)
								{
									SetValueCheck(switchCell, flag4);
								}
							}).ContinueWith((Task action) => switchCell.Loading = false);
						}
						else if (switchCell.AutoCheck)
						{
							SetValueCheck(switchCell);
							OnCheckedChanged(switchCell.Checked, it.row.RECORD, it.i_row, it.i_cel, cELLDB.col);
						}
					}
				}
				else if (it.cell is Template template)
				{
					foreach (ICell item in template.Value)
					{
						if (item is CellCheckbox { Rect: var rect } cellCheckbox)
						{
							if (rect.Contains(cELLDB.x, cELLDB.y) && cellCheckbox.Enabled && cellCheckbox.AutoCheck)
							{
								cellCheckbox.Checked = !cellCheckbox.Checked;
							}
						}
						else if (item is CellRadio { Rect: var rect2 } cellRadio)
						{
							if (rect2.Contains(cELLDB.x, cELLDB.y) && cellRadio.Enabled && cellRadio.AutoCheck)
							{
								cellRadio.Checked = !cellRadio.Checked;
							}
						}
						else if (item is CellSwitch { Rect: var rect3 } cellSwitch && rect3.Contains(cELLDB.x, cELLDB.y) && cellSwitch.Enabled && cellSwitch.AutoCheck)
						{
							cellSwitch.Checked = !cellSwitch.Checked;
						}
					}
				}
				else if (it.row.IsColumn && it.cell is TCellColumn tCellColumn)
				{
					if (it.cell.COLUMN.Filter != null && tCellColumn.rect_filter.Contains(cELLDB.x - tCellColumn.offsetx, cELLDB.y - tCellColumn.offsety))
					{
						Column cOLUMN2 = it.cell.COLUMN;
						if (OnFilterPopupBegin(cOLUMN2, out IList<object> customSource, out Font font, out int? num))
						{
							if (font == null)
							{
								font = Font;
							}
							int num2 = tCellColumn.offsetx - cELLDB.offset_xi;
							FilterControl filterControl = new FilterControl(this, font, cOLUMN2, customSource);
							if (num.HasValue)
							{
								filterControl.Height = num.Value;
							}
							filterControl.Set(new Popover.Config(this, filterControl)
							{
								Dpi = font.Size / 10f * Dpi,
								Tag = cOLUMN2.Filter,
								ArrowAlign = TAlign.Bottom,
								Font = font,
								Offset = new Rectangle(tCellColumn.rect_filter.X + num2, tCellColumn.rect_filter.Y + tCellColumn.offsety, tCellColumn.rect_filter.Width, tCellColumn.rect_filter.Height),
								Padding = new Size(6, 6)
							}.open());
						}
						return;
					}
					if (it.cell.COLUMN.SortOrder)
					{
						SortMode sortMode = SortMode.NONE;
						int num3 = cELLDB.x - tCellColumn.offsetx;
						int num4 = cELLDB.y - tCellColumn.offsety;
						if (tCellColumn.rect_up.Contains(num3, num4))
						{
							sortMode = SortMode.ASC;
						}
						else if (tCellColumn.rect_down.Contains(num3, num4))
						{
							sortMode = SortMode.DESC;
						}
						else
						{
							sortMode = tCellColumn.COLUMN.SortMode + 1;
							if (sortMode > SortMode.DESC)
							{
								sortMode = SortMode.NONE;
							}
						}
						if (tCellColumn.COLUMN.SetSortMode(sortMode))
						{
							CELL[] cells = it.row.cells;
							foreach (CELL cELL in cells)
							{
								if (cELL.COLUMN.SortOrder && cELL.INDEX != it.i_cel)
								{
									cELL.COLUMN.SetSortMode(SortMode.NONE);
								}
							}
							if (OnSortModeChanged(sortMode, tCellColumn.COLUMN))
							{
								Invalidate();
							}
							else
							{
								Invalidate();
								switch (sortMode)
								{
								case SortMode.ASC:
									SortDataASC(tCellColumn.COLUMN);
									break;
								case SortMode.DESC:
									SortDataDESC(tCellColumn.COLUMN);
									break;
								default:
									SortData = null;
									break;
								}
								LoadLayout();
								OnSortRows(it.i_cel);
							}
						}
					}
				}
			}
		}
		bool flag3 = ItemMouseClick(it.row.RECORD, it.row.Type, cELLDB.i_row, cELLDB.i_cel, cELLDB.col, RealRect(cELLDB.cell.RECT, cELLDB.offset_xi, cELLDB.offset_y), e);
		if (e.Button == MouseButtons.Left && editmode == (TEditMode)((!flag3) ? 1 : 2))
		{
			EditModeClose();
			int i_row = cELLDB.i_row;
			int i_cel = cELLDB.i_cel;
			cELLDB.cell = RealCELL(cELLDB.cell, rows, ref i_row, ref i_cel, ref it, out var rect4);
			if (CanEditMode(cELLDB.cell))
			{
				int num6 = ScrollLine(rect4, RealRegionRow(rows));
				OnEditMode(it.row, cELLDB.cell, rect4, i_row, i_cel, cELLDB.col, cELLDB.offset_xi, cELLDB.offset_y - num6, region: true);
			}
		}
	}

	private bool MouseUpRowBefore(RowTemplate[] rows, DownCellTMP<CELL> it, DownCellTMP<CellLink>? btn, MouseEventArgs e, CELLDB db)
	{
		if (e.Button == MouseButtons.Left)
		{
			if (btn != null && btn.cell.Rect.Contains(db.x, db.y))
			{
				return true;
			}
			if (it.cell is TCellCheck tCellCheck)
			{
				if (tCellCheck.CONTAIN_REAL(db.x, db.y))
				{
					return false;
				}
			}
			else if (it.cell is TCellRadio tCellRadio)
			{
				if (tCellRadio.CONTAIN_REAL(db.x, db.y) && !tCellRadio.Checked)
				{
					return false;
				}
			}
			else if (it.cell is TCellSwitch tCellSwitch)
			{
				if (tCellSwitch.CONTAIN_REAL(db.x, db.y) && !tCellSwitch.Loading)
				{
					return false;
				}
			}
			else if (it.cell is Template template)
			{
				foreach (ICell item in template.Value)
				{
					if (item is CellCheckbox { Rect: var rect } cellCheckbox)
					{
						if (rect.Contains(db.x, db.y) && cellCheckbox.Enabled)
						{
							return false;
						}
					}
					else if (item is CellRadio { Rect: var rect2 } cellRadio)
					{
						if (rect2.Contains(db.x, db.y) && cellRadio.Enabled)
						{
							return false;
						}
					}
					else if (item is CellSwitch { Rect: var rect3 } cellSwitch && rect3.Contains(db.x, db.y) && cellSwitch.Enabled)
					{
						return false;
					}
				}
			}
			else if (it.row.IsColumn && it.cell is TCellColumn tCellColumn)
			{
				if (it.cell.COLUMN.Filter != null)
				{
					if (tCellColumn.rect_filter.Contains(db.x - tCellColumn.offsetx, db.y - tCellColumn.offsety))
					{
						return false;
					}
				}
				else if (it.cell.COLUMN.SortOrder)
				{
					int num = db.x - tCellColumn.offsetx;
					int num2 = db.y - tCellColumn.offsety;
					if (tCellColumn.rect_up.Contains(num, num2) || tCellColumn.rect_down.Contains(num, num2))
					{
						return false;
					}
				}
			}
			if (editmode == (TEditMode)((mouseClicks <= 1) ? 1 : 2) && CanEditMode(db.cell))
			{
				return false;
			}
		}
		return true;
	}

	private bool MouseUpBtn(DownCellTMP<CELL> it, DownCellTMP<CellLink>? btn, MouseEventArgs e, CELLDB db)
	{
		if (btn == null)
		{
			return false;
		}
		btn.cell.ExtraMouseDown = false;
		if (e.Button == MouseButtons.Left && btn.cell.Rect.Contains(db.x, db.y))
		{
			btn.cell.Click();
			if (btn.cell.DropDownItems != null && btn.cell.DropDownItems.Count > 0)
			{
				subForm?.IClose();
				subForm = null;
				Rectangle rect = btn.cell.Rect;
				rect.Offset(-db.offset_xi, -db.offset_y);
				subForm = new LayeredFormSelectDown(this, btn.cell.DropDownItems, btn.cell, rect);
				subForm.Show(this);
			}
			OnCellButtonUp(btn.cell, it.row.RECORD, it.row.Type, it.i_row, it.i_cel, db.col, RealRect(btn.cell.Rect, db.offset_xi, db.offset_y), e);
			OnCellButtonClick(btn.cell, it.row.RECORD, it.row.Type, it.i_row, it.i_cel, db.col, RealRect(btn.cell.Rect, db.offset_xi, db.offset_y), e);
			return true;
		}
		OnCellButtonUp(btn.cell, it.row.RECORD, it.row.Type, it.i_row, it.i_cel, db.col, RealRect(btn.cell.Rect, db.offset_xi, db.offset_y), e);
		return false;
	}

	private bool MouseUpBtn(DownCellTMP<CELL> it, DownCellTMP<CellLink>? btn, MouseEventArgs e)
	{
		if (btn == null)
		{
			return false;
		}
		btn.cell.ExtraMouseDown = false;
		OnCellButtonUp(btn.cell, it.row.RECORD, it.row.Type, it.i_row, it.i_cel, btn.col, RealRect(btn.cell.Rect, it.offset_xi, it.offset_y), e);
		return false;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (moveheaders.Length != 0)
		{
			MoveHeader[] array = moveheaders;
			foreach (MoveHeader moveHeader in array)
			{
				if (!moveHeader.MouseDown)
				{
					continue;
				}
				int num = moveHeader.width + e.X - moveHeader.x;
				if (num >= moveHeader.min_width)
				{
					if (tmpcol_width.ContainsKey(moveHeader.i))
					{
						tmpcol_width[moveHeader.i] = num;
					}
					else
					{
						tmpcol_width.Add(moveHeader.i, num);
					}
					if (LoadLayout())
					{
						Invalidate();
					}
					SetCursor(CursorType.VSplit);
				}
				return;
			}
		}
		if (dragHeader != null)
		{
			dragHeader.SetEnable(e.X, e.Y);
			SetCursor(CursorType.SizeAll);
			dragHeader.hand = true;
			dragHeader.xr = e.X - dragHeader.x;
			if (rows == null)
			{
				return;
			}
			int ex = dragHeader.x + dragHeader.xr;
			CELL[] cells = rows.First.cells;
			dragHeader.last = e.X > dragHeader.x;
			CELLDB cELLDB = CellContains(rows.List, sethover: false, ex, e.Y);
			if (cELLDB != null)
			{
				if (cELLDB.col.INDEX_REAL == dragHeader.i)
				{
					dragHeader.im = -1;
				}
				else
				{
					dragHeader.im = cELLDB.col.INDEX_REAL;
				}
				Invalidate();
				return;
			}
			int iNDEX_REAL = cells[cells.Length - 1].COLUMN.INDEX_REAL;
			if (iNDEX_REAL == dragHeader.i)
			{
				dragHeader.im = -1;
			}
			else
			{
				dragHeader.im = iNDEX_REAL;
			}
			Invalidate();
		}
		else if (dragBody != null)
		{
			dragBody.SetEnable(e.X, e.Y);
			SetCursor(CursorType.SizeAll);
			dragBody.hand = true;
			dragBody.xr = e.Y - dragBody.x;
			if (rows == null)
			{
				return;
			}
			int ey = dragBody.x + dragBody.xr;
			dragBody.last = e.Y > dragBody.x;
			CELLDB cELLDB2 = CellContains(rows.List, sethover: false, e.X, ey);
			if (cELLDB2 != null)
			{
				if (cELLDB2.i_row == dragBody.i)
				{
					dragBody.im = -1;
				}
				else
				{
					dragBody.im = cELLDB2.i_row;
				}
				Invalidate();
				return;
			}
			int num2 = rows.Length - 1 - rowSummary;
			RowTemplate rowTemplate = rows.List[num2];
			if (rowTemplate.INDEX == dragBody.i)
			{
				dragBody.im = -1;
			}
			else
			{
				dragBody.im = rowTemplate.INDEX;
			}
			Invalidate();
		}
		else if (ScrollBar.MouseMoveY(e.X, e.Y) && ScrollBar.MouseMoveX(e.X, e.Y) && OnTouchMove(e.X, e.Y))
		{
			if (rows == null || inEditMode)
			{
				return;
			}
			CELLDB cELLDB3 = CellContains(rows.List, sethover: true, e.X, e.Y);
			if (cELLDB3 == null || cELLDB3.mode == CELLDBMode.Summary)
			{
				hovers = -1;
				rows.HoverLeave();
				SetCursor(val: false);
				return;
			}
			hovers = cELLDB3.cell.ROW.INDEX;
			if (cELLDB3.mode > CELLDBMode.None)
			{
				hovers = -1;
				rows.HoverLeave();
				TCellColumn tCellColumn = (TCellColumn)cELLDB3.cell;
				if (moveheaders.Length != 0)
				{
					MoveHeader[] array = moveheaders;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].rect.Contains(cELLDB3.x, cELLDB3.y))
						{
							SetCursor(CursorType.VSplit);
							return;
						}
					}
				}
				if (has_check && tCellColumn.COLUMN is ColumnCheck { NoTitle: not false } && tCellColumn.CONTAIN_REAL(cELLDB3.x, cELLDB3.y))
				{
					SetCursor(val: true);
				}
				else if (tCellColumn.COLUMN.SortOrder)
				{
					SetCursor(val: true);
				}
				else if (tCellColumn.COLUMN.Filter != null && tCellColumn.rect_filter.Contains(cELLDB3.x - tCellColumn.offsetx, cELLDB3.y - tCellColumn.offsety))
				{
					SetCursor(val: true);
				}
				else if (ColumnDragSort && tCellColumn.COLUMN.DragSort)
				{
					SetCursor(CursorType.SizeAll);
				}
				else
				{
					SetCursor(val: false);
				}
			}
			else if (rows.HoverLeave(cELLDB3) > 0)
			{
				SetCursor(CursorType.SizeAll);
			}
			else if (cELLDB3.cell.ROW.CanExpand && cELLDB3.cell.ROW.RectExpand.Contains(cELLDB3.x, cELLDB3.y))
			{
				SetCursor(val: true);
			}
			else
			{
				SetCursor(MouseMoveRow(cELLDB3, e));
			}
		}
		else
		{
			ILeave();
		}
	}

	protected override bool OnMouseHover(int x, int y)
	{
		if (rows == null || inEditMode)
		{
			return false;
		}
		CELLDB cELLDB = CellContains(rows.List, sethover: false, x, y);
		if (cELLDB == null || cELLDB.mode == CELLDBMode.Summary)
		{
			tmp = null;
			CloseTip();
			OnCellHover();
		}
		else
		{
			OnCellHover(cELLDB.cell.ROW.RECORD, cELLDB.cell.ROW.Type, cELLDB.i_row, cELLDB.i_cel, cELLDB.col, RealRect(cELLDB.cell.RECT, cELLDB.offset_xi, cELLDB.offset_y), new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
			if (cELLDB.mode == CELLDBMode.None)
			{
				object obj = MouseHoverRow(cELLDB);
				if (tmp == obj)
				{
					return false;
				}
				tmp = obj;
				CloseTip();
				if (obj == null)
				{
					return false;
				}
				if (obj is CellLink cellLink)
				{
					if (cellLink.Tooltip != null)
					{
						OpenTip(RealRect(cellLink.Rect, cELLDB.offset_xi, cELLDB.offset_y), cellLink.Tooltip);
						return true;
					}
				}
				else if (obj is CellImage cellImage)
				{
					if (cellImage.Tooltip != null)
					{
						OpenTip(RealRect(cellImage.Rect, cELLDB.offset_xi, cELLDB.offset_y), cellImage.Tooltip);
						return true;
					}
				}
				else if (obj is CELL cELL)
				{
					string text = cELL.ToString();
					if (!string.IsNullOrEmpty(text) && !cELLDB.col.LineBreak && cELL.MinWidth > cELL.RECT_REAL.Width + 1)
					{
						OpenTip(RealRect(cELL.RECT_REAL, cELLDB.offset_xi, cELLDB.offset_y), text);
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool MouseMoveRow(CELLDB db, MouseEventArgs e)
	{
		if (db.cell is TCellCheck tCellCheck)
		{
			if (tCellCheck.AutoCheck && tCellCheck.CONTAIN_REAL(db.x, db.y))
			{
				return true;
			}
			return false;
		}
		if (db.cell is TCellRadio tCellRadio)
		{
			if (tCellRadio.AutoCheck && tCellRadio.CONTAIN_REAL(db.x, db.y))
			{
				return true;
			}
			return false;
		}
		if (db.cell is TCellSwitch tCellSwitch)
		{
			if (tCellSwitch.AutoCheck || tCellSwitch.COLUMN is ColumnSwitch { Call: not null })
			{
				tCellSwitch.ExtraMouseHover = tCellSwitch.CONTAIN_REAL(db.x, db.y);
				if (tCellSwitch.ExtraMouseHover)
				{
					return true;
				}
			}
			else
			{
				tCellSwitch.ExtraMouseHover = false;
			}
			return false;
		}
		if (db.cell is Template template)
		{
			int num = 0;
			foreach (ICell item in template.Value)
			{
				if (item is CellLink cellLink)
				{
					if (cellLink.Enabled)
					{
						cellLink.ExtraMouseHover = cellLink.Rect.Contains(db.x, db.y);
						if (cellLink.ExtraMouseHover)
						{
							num++;
						}
					}
					else
					{
						cellLink.ExtraMouseHover = false;
					}
				}
				else if (item is CellCheckbox cellCheckbox)
				{
					if (cellCheckbox.Enabled && cellCheckbox.AutoCheck)
					{
						cellCheckbox.ExtraMouseHover = cellCheckbox.Rect.Contains(db.x, db.y);
						if (cellCheckbox.ExtraMouseHover)
						{
							num++;
						}
					}
					else
					{
						cellCheckbox.ExtraMouseHover = false;
					}
				}
				else if (item is CellRadio cellRadio)
				{
					if (cellRadio.Enabled && cellRadio.AutoCheck)
					{
						cellRadio.ExtraMouseHover = cellRadio.Rect.Contains(db.x, db.y);
						if (cellRadio.ExtraMouseHover)
						{
							num++;
						}
					}
					else
					{
						cellRadio.ExtraMouseHover = false;
					}
				}
				else
				{
					if (!(item is CellSwitch cellSwitch))
					{
						continue;
					}
					if (cellSwitch.Enabled && cellSwitch.AutoCheck)
					{
						cellSwitch.ExtraMouseHover = cellSwitch.Rect.Contains(db.x, db.y);
						if (cellSwitch.ExtraMouseHover)
						{
							num++;
						}
					}
					else
					{
						cellSwitch.ExtraMouseHover = false;
					}
				}
			}
			return num > 0;
		}
		return false;
	}

	private object? MouseHoverRow(CELLDB db)
	{
		if (db.cell is TCellCheck)
		{
			return null;
		}
		if (db.cell is TCellRadio)
		{
			return null;
		}
		if (db.cell is TCellSwitch)
		{
			return null;
		}
		if (db.cell is Template template)
		{
			ICell cell = MouseHoverCell(template, db.x, db.y);
			if (cell == null)
			{
				return null;
			}
			if (cell is CellLink result)
			{
				return result;
			}
			if (cell is CellImage result2)
			{
				return result2;
			}
		}
		else if (ShowTip)
		{
			return db.cell;
		}
		return null;
	}

	private ICell? MouseHoverCell(Template template, int x, int y)
	{
		foreach (ICell item in template.Value)
		{
			if (item is CellLink cellLink)
			{
				if (cellLink.Enabled && cellLink.Rect.Contains(x, y))
				{
					return cellLink;
				}
			}
			else if (item is CellImage { Rect: var rect } cellImage && rect.Contains(x, y))
			{
				return cellImage;
			}
		}
		return null;
	}

	public void CloseTip()
	{
		toolTip?.IClose();
		toolTip = null;
	}

	public void OpenTip(Rectangle rect, string tooltip, TooltipConfig? config = null)
	{
		if (toolTip == null)
		{
			toolTip = new TooltipForm(this, rect, tooltip, config ?? TooltipConfig ?? new TooltipConfig
			{
				Font = Font,
				ArrowAlign = TAlign.Top
			}, hasmax: true);
			toolTip.Show(this);
		}
		else if (toolTip.SetText(rect, tooltip))
		{
			CloseTip();
			OpenTip(rect, tooltip);
		}
	}

	private CELLDB? CellContains(RowTemplate[] rows, bool sethover, int ex, int ey)
	{
		int valueX = ScrollBar.ValueX;
		int scrollBarRealY = ScrollBarRealY;
		int px = ex + valueX;
		int py = ey + scrollBarRealY;
		if (summary == null)
		{
			return CellContainsCore(rows, sethover, ex, ey, valueX, scrollBarRealY, px, py);
		}
		List<RowTemplate> list = new List<RowTemplate>(rows.Length);
		foreach (RowTemplate rowTemplate in rows)
		{
			if (rowTemplate.IsSummary)
			{
				if (sFixedB == -1)
				{
					if (rowTemplate.CONTAINS(ex, py) && CellContains(rowTemplate, ex, ey, valueX, scrollBarRealY, px, py, out CELLDB cell))
					{
						cell.mode = CELLDBMode.Summary;
						return cell;
					}
					continue;
				}
				int eyb = ey + sFixedB;
				if (rowTemplate.CONTAINS(ex, eyb) && CellContains(rowTemplate, ex, ey, eyb, valueX, scrollBarRealY, px, py, out CELLDB cell2))
				{
					cell2.mode = CELLDBMode.Summary;
					return cell2;
				}
			}
			else
			{
				list.Add(rowTemplate);
			}
		}
		return CellContainsCore(list, sethover, ex, ey, valueX, scrollBarRealY, px, py);
	}

	private CELLDB? CellContainsCore(IList<RowTemplate> rows, bool sethover, int ex, int ey, int sx, int sy, int px, int py)
	{
		foreach (RowTemplate row in rows)
		{
			if (row.IsColumn)
			{
				CELLDB cell2;
				if (fixedHeader)
				{
					if (CellContainsFixed(row, ex, ey, sx, sy, px, py, out CELLDB cell))
					{
						cell.mode = CELLDBMode.ColumnFixed;
						return cell;
					}
				}
				else if (row.CONTAINS(ex, py) && CellContains(row, ex, ey, sx, sy, px, py, out cell2))
				{
					cell2.mode = CELLDBMode.Column;
					return cell2;
				}
			}
			else if (!row.IsSummary && row.Contains(ex, py, sethover))
			{
				if (sethover)
				{
					hovers = row.INDEX;
				}
				if (CellContains(row, ex, ey, sx, sy, px, py, out CELLDB cell3))
				{
					cell3.mode = CELLDBMode.None;
					return cell3;
				}
			}
		}
		return null;
	}

	private bool CellContainsFixed(RowTemplate it, int ex, int ey, int sx, int sy, int px, int py, out CELLDB? cell)
	{
		List<int> list = new List<int>();
		if (showFixedColumnL && fixedColumnL != null)
		{
			foreach (int item in fixedColumnL)
			{
				list.Add(item);
				CELL cELL = it.cells[item];
				if (cELL.CONTAIN(ex, ey))
				{
					cell = new CELLDB(cELL, ex, ey, 0, 0, sy, it.INDEX, item, cELL.COLUMN);
					return true;
				}
			}
		}
		if (showFixedColumnR && fixedColumnR != null)
		{
			foreach (int item2 in fixedColumnR)
			{
				list.Add(item2);
				CELL cELL2 = it.cells[item2];
				if (cELL2.CONTAIN(ex + sFixedR, ey))
				{
					cell = new CELLDB(cELL2, ex + sFixedR, ey, -sFixedR, sFixedR, sy, it.INDEX, item2, cELL2.COLUMN);
					return true;
				}
			}
		}
		for (int i = 0; i < it.cells.Length; i++)
		{
			if (!list.Contains(i))
			{
				CELL cELL3 = it.cells[i];
				if (cELL3.CONTAIN(px, ey))
				{
					cell = new CELLDB(cELL3, px, ey, sx, sx, sy, it.INDEX, i, cELL3.COLUMN);
					return true;
				}
			}
		}
		cell = null;
		return false;
	}

	private bool CellContains(RowTemplate it, int ex, int ey, int sx, int sy, int px, int py, out CELLDB? cell)
	{
		List<int> list = new List<int>();
		if (showFixedColumnL && fixedColumnL != null)
		{
			foreach (int item in fixedColumnL)
			{
				list.Add(item);
				CELL cELL = it.cells[item];
				if (cELL.CONTAIN(ex, py))
				{
					cell = new CELLDB(cELL, ex, py, 0, 0, sy, it.INDEX, item, cELL.COLUMN);
					return true;
				}
			}
		}
		if (showFixedColumnR && fixedColumnR != null)
		{
			foreach (int item2 in fixedColumnR)
			{
				list.Add(item2);
				CELL cELL2 = it.cells[item2];
				if (cELL2.CONTAIN(ex + sFixedR, py))
				{
					cell = new CELLDB(cELL2, ex + sFixedR, py, -sFixedR, sFixedR, sy, it.INDEX, item2, cELL2.COLUMN);
					return true;
				}
			}
		}
		for (int i = 0; i < it.cells.Length; i++)
		{
			if (!list.Contains(i))
			{
				CELL cELL3 = it.cells[i];
				if (cELL3.CONTAIN(px, py))
				{
					cell = new CELLDB(cELL3, px, py, sx, sx, sy, it.INDEX, i, cELL3.COLUMN);
					return true;
				}
			}
		}
		cell = null;
		return false;
	}

	private bool CellContains(RowTemplate it, int ex, int ey, int eyb, int sx, int sy, int px, int py, out CELLDB? cell)
	{
		List<int> list = new List<int>();
		if (showFixedColumnL && fixedColumnL != null)
		{
			foreach (int item in fixedColumnL)
			{
				list.Add(item);
				CELL cELL = it.cells[item];
				if (cELL.CONTAIN(ex, eyb))
				{
					cell = new CELLDB(cELL, ex, eyb, 0, 0, sy, it.INDEX, item, cELL.COLUMN);
					return true;
				}
			}
		}
		if (showFixedColumnR && fixedColumnR != null)
		{
			foreach (int item2 in fixedColumnR)
			{
				list.Add(item2);
				CELL cELL2 = it.cells[item2];
				if (cELL2.CONTAIN(ex + sFixedR, eyb))
				{
					cell = new CELLDB(cELL2, ex + sFixedR, eyb, -sFixedR, sFixedR, sy, it.INDEX, item2, cELL2.COLUMN);
					return true;
				}
			}
		}
		for (int i = 0; i < it.cells.Length; i++)
		{
			if (!list.Contains(i))
			{
				CELL cELL3 = it.cells[i];
				if (cELL3.CONTAIN(px, eyb))
				{
					cell = new CELLDB(cELL3, px, eyb, sx, sx, sy, it.INDEX, i, cELL3.COLUMN);
					return true;
				}
			}
		}
		cell = null;
		return false;
	}

	internal int GetCellOffsetX(CELL cell)
	{
		if (showFixedColumnL && fixedColumnL != null && fixedColumnL.Contains(cell.INDEX))
		{
			return 0;
		}
		if (showFixedColumnR && fixedColumnR != null && fixedColumnR.Contains(cell.INDEX))
		{
			try
			{
				RowTemplate rowTemplate = rows[0];
				Rectangle rectangle = ClientRectangle.PaddingRect(Padding, Dpi, borderWidth / 2f);
				int valueX = ScrollBar.ValueX;
				int num = rectangle.Right - (ScrollBar.ShowY ? ScrollBar.SIZE : 0);
				CELL cELL = rowTemplate.cells[fixedColumnR[0]];
				if (valueX + num < cELL.RECT.Right)
				{
					showFixedColumnR = true;
					sFixedR = cELL.RECT.Right - num;
				}
				else
				{
					showFixedColumnR = false;
				}
			}
			catch
			{
			}
			return sFixedR;
		}
		return ScrollBar.ValueX;
	}

	private bool ItemMouseClick(object record, RowType rowType, int rowIndex, int columnIndex, Column? column, Rectangle rect, MouseEventArgs e)
	{
		if (mouseClicks > 1)
		{
			OnCellDoubleClick(record, rowType, rowIndex, columnIndex, column, rect, e.GenerateMouseEventArgs(mouseClicks));
			return true;
		}
		OnCellClick(record, rowType, rowIndex, columnIndex, column, rect, e.GenerateMouseEventArgs(mouseClicks));
		return false;
	}

	private void ItemMouseClick(MouseEventArgs e)
	{
		if (mouseClicks > 1)
		{
			OnNonCellDoubleClick(e.GenerateMouseEventArgs(mouseClicks));
		}
		else
		{
			OnNonCellClick(e.GenerateMouseEventArgs(mouseClicks));
		}
	}

	private Rectangle RealRect(DownCellTMP<CellLink> link)
	{
		return RealRect(link.cell.Rect, link.offset_xi, link.offset_y);
	}

	private Rectangle RealRect(Rectangle rect, int ox, int oy)
	{
		return new Rectangle(rect.X - ox, rect.Y - oy, rect.Width, rect.Height);
	}

	private CELL RealCELL(CELL cell, RowTemplate[] rows, ref int i_row, ref int i_cel, ref DownCellTMP<CELL> it, out Rectangle rect)
	{
		if (CellRanges == null || CellRanges.Length == 0)
		{
			rect = cell.RECT;
			return cell;
		}
		CellRange[] cellRanges = CellRanges;
		foreach (CellRange cellRange in cellRanges)
		{
			if (cellRange.IsInRange(i_row - 1, i_cel))
			{
				try
				{
					RowTemplate rowTemplate = rows[cellRange.FirstRow + 1];
					RowTemplate obj = rows[cellRange.LastRow + 1];
					CELL cELL = rowTemplate.cells[cellRange.FirstColumn];
					CELL last = obj.cells[cellRange.LastColumn];
					rect = RectMergeCells(cELL, last, out var _);
					i_row = cellRange.FirstRow + 1;
					i_cel = cellRange.FirstColumn;
					it.row = rowTemplate;
					return cELL;
				}
				catch
				{
				}
			}
		}
		rect = cell.RECT;
		return cell;
	}

	private CELL RealCELL(CELL cell, RowTemplate[] rows, int i_row, int i_cel, out Rectangle rect)
	{
		if (CellRanges == null || CellRanges.Length == 0)
		{
			rect = cell.RECT;
			return cell;
		}
		CellRange[] cellRanges = CellRanges;
		foreach (CellRange cellRange in cellRanges)
		{
			if (cellRange.IsInRange(i_row - 1, i_cel))
			{
				try
				{
					RowTemplate rowTemplate = rows[cellRange.FirstRow + 1];
					RowTemplate obj = rows[cellRange.LastRow + 1];
					CELL cELL = rowTemplate.cells[cellRange.FirstColumn];
					CELL last = obj.cells[cellRange.LastColumn];
					rect = RectMergeCells(cELL, last, out var _);
					return cELL;
				}
				catch
				{
				}
			}
		}
		rect = cell.RECT;
		return cell;
	}

	private List<SortModel> SortDatas(Column column, out int count)
	{
		count = 0;
		if (dataTmp == null || dataTmp.rows.Length == 0)
		{
			return new List<SortModel>(0);
		}
		List<SortModel> list = new List<SortModel>(dataTmp.rows.Length);
		if (dataTmp.rows[0].cells.ContainsKey(column.Key))
		{
			Dictionary<object, List<SortModel>> dictionary = new Dictionary<object, List<SortModel>>(dataTmp.rows.Length);
			Dictionary<object, SortModel> dictionary2 = new Dictionary<object, SortModel>(dataTmp.rows.Length);
			if (column.Render == null)
			{
				for (int i = 0; i < dataTmp.rows.Length; i++)
				{
					IRow row = dataTmp.rows[i];
					if (!row.cells.TryGetValue(column.Key, out object value))
					{
						continue;
					}
					SortModel sortModel = new SortModel(i, OGetValue(row.record, value));
					if (row.fid == null)
					{
						list.Add(sortModel);
					}
					else if (dictionary.ContainsKey(row.fid))
					{
						if (dictionary[row.fid] == null)
						{
							object? fid = row.fid;
							List<SortModel> value2 = (dictionary2[row.fid].children = new List<SortModel>(dataTmp.rows.Length));
							dictionary[fid] = value2;
						}
						dictionary[row.fid].Add(sortModel);
					}
					dictionary.Add(row.record, null);
					dictionary2.Add(row.record, sortModel);
					count++;
				}
			}
			else
			{
				for (int j = 0; j < dataTmp.rows.Length; j++)
				{
					IRow row2 = dataTmp.rows[j];
					SortModel sortModel2 = new SortModel(j, column.Render(row2[column.Key], dataTmp.rows[j].record, j)?.ToString());
					if (row2.fid == null)
					{
						list.Add(sortModel2);
					}
					else if (dictionary.ContainsKey(row2.fid))
					{
						if (dictionary[row2.fid] == null)
						{
							object? fid2 = row2.fid;
							List<SortModel> value2 = (dictionary2[row2.fid].children = new List<SortModel>(dataTmp.rows.Length));
							dictionary[fid2] = value2;
						}
						dictionary[row2.fid].Add(sortModel2);
					}
					dictionary.Add(row2.record, null);
					dictionary2.Add(row2.record, sortModel2);
					count++;
				}
			}
		}
		return list;
	}

	private void SortDataASC(Column column)
	{
		List<SortModel> list = SortDatas(column, out var count);
		SortDataASC(list);
		List<int> list2 = new List<int>(count);
		SortChildren(ref list2, list);
		SortData = list2.ToArray();
	}

	private void SortDataDESC(Column column)
	{
		List<SortModel> list = SortDatas(column, out var count);
		SortDataDESC(list);
		List<int> list2 = new List<int>(count);
		SortChildren(ref list2, list);
		SortData = list2.ToArray();
	}

	private void SortDataASC(List<SortModel> list)
	{
		if (CustomSort == null)
		{
			list.Sort((SortModel x, SortModel y) => FilesNameComparerClass.Compare(x.v, y.v));
		}
		else
		{
			list.Sort((SortModel x, SortModel y) => CustomSort(x.v, y.v));
		}
		foreach (SortModel item in list)
		{
			if (item.children != null)
			{
				SortDataASC(item.children);
			}
		}
	}

	private void SortDataDESC(List<SortModel> list)
	{
		if (CustomSort == null)
		{
			list.Sort((SortModel y, SortModel x) => FilesNameComparerClass.Compare(x.v, y.v));
		}
		else
		{
			list.Sort((SortModel y, SortModel x) => CustomSort(x.v, y.v));
		}
		foreach (SortModel item in list)
		{
			if (item.children != null)
			{
				SortDataDESC(item.children);
			}
		}
	}

	private void SortChildren(ref List<int> list, List<SortModel> data)
	{
		foreach (SortModel datum in data)
		{
			list.Add(datum.i);
			if (datum.children != null)
			{
				SortChildren(ref list, datum.children);
			}
		}
	}

	protected override void OnLostFocus(EventArgs e)
	{
		if (LostFocusClearSelection)
		{
			SelectedIndex = -1;
		}
		CloseTip();
		base.OnLostFocus(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (!RectangleToScreen(ClientRectangle).Contains(Control.MousePosition))
		{
			ScrollBar.Leave();
			ILeave();
		}
	}

	protected override void OnLeave(EventArgs e)
	{
		base.OnLeave(e);
		ScrollBar.Leave();
		ILeave();
	}

	private void ILeave()
	{
		SetCursor(val: false);
		if (rows != null && !inEditMode)
		{
			hovers = -1;
			rows.HoverLeave();
			CloseTip();
			OnCellHover();
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		subForm?.IClose();
		subForm = null;
		CloseTip();
		ScrollBar.MouseWheel(e);
		base.OnMouseWheel(e);
	}

	protected override bool OnTouchScrollX(int x, int y, int value)
	{
		return ScrollBar.MouseWheelXCore(value);
	}

	protected override bool OnTouchScrollY(int x, int y, int value)
	{
		return ScrollBar.MouseWheelYCore(value);
	}

	internal static void PaintButton(Canvas g, Font font, Size gap, Rectangle rect_read, CellButton btn, bool enable, TAMode colorScheme, string cname)
	{
		float num = ((btn.Shape == TShape.Round || btn.Shape == TShape.Circle) ? ((float)rect_read.Height) : ((float)btn.Radius * g.Dpi));
		if (btn.Type == TTypeMini.Default)
		{
			Color color = Colour.DefaultColor.Get(colorScheme, "Button", cname);
			Color color2 = Colour.Primary.Get(colorScheme, "Button", cname);
			Color color3;
			Color color4;
			if (btn.BorderWidth > 0f)
			{
				color3 = Colour.PrimaryHover.Get(colorScheme, "Button", cname);
				color4 = Colour.PrimaryActive.Get(colorScheme, "Button", cname);
			}
			else
			{
				color3 = Colour.FillSecondary.Get(colorScheme, "Button", cname);
				color4 = Colour.Fill.Get(colorScheme, "Button", cname);
			}
			if (btn.Fore.HasValue)
			{
				color = btn.Fore.Value;
			}
			if (btn.BackHover.HasValue)
			{
				color3 = btn.BackHover.Value;
			}
			if (btn.BackActive.HasValue)
			{
				color4 = btn.BackActive.Value;
			}
			using GraphicsPath graphicsPath = PathButton(rect_read, btn, num);
			if (btn.AnimationClick)
			{
				int num2 = (int)((float)Math.Max(gap.Height, gap.Width) * g.Dpi);
				float num3 = (float)rect_read.Width + (float)num2 * btn.AnimationClickValue;
				float num4 = (float)rect_read.Height + (float)num2 * btn.AnimationClickValue;
				float alpha = 100f * (1f - btn.AnimationClickValue);
				using GraphicsPath graphicsPath2 = new RectangleF((float)rect_read.X + ((float)rect_read.Width - num3) / 2f, (float)rect_read.Y + ((float)rect_read.Height - num4) / 2f, num3, num4).RoundPath(num, btn.Shape);
				graphicsPath2.AddPath(graphicsPath, connect: false);
				g.Fill(Helper.ToColor(alpha, color2), graphicsPath2);
			}
			if (enable && btn.Enabled)
			{
				if (!btn.Ghost)
				{
					using (GraphicsPath graphicsPath3 = new RectangleF(rect_read.X, rect_read.Y + 3, rect_read.Width, rect_read.Height).RoundPath(num))
					{
						graphicsPath3.AddPath(graphicsPath, connect: false);
						g.Fill(Colour.FillQuaternary.Get(colorScheme, "Button", cname), graphicsPath3);
					}
					g.Fill(btn.DefaultBack ?? Colour.DefaultBg.Get(colorScheme, "Button", cname), graphicsPath);
				}
				if (btn.BorderWidth > 0f)
				{
					float num5 = btn.BorderWidth * g.Dpi;
					if (btn.ExtraMouseDown)
					{
						g.Draw(color4, num5, graphicsPath);
						PaintButton(g, font, btn, color4, rect_read);
					}
					else if (btn.AnimationHover)
					{
						Color overlay = Helper.ToColor(btn.AnimationHoverValue, color3);
						g.Draw(Colour.DefaultBorder.Get(colorScheme, "Button", cname).BlendColors(overlay), num5, graphicsPath);
						PaintButton(g, font, btn, color.BlendColors(overlay), rect_read);
					}
					else if (btn.ExtraMouseHover)
					{
						g.Draw(color3, num5, graphicsPath);
						PaintButton(g, font, btn, color3, rect_read);
					}
					else
					{
						g.Draw(btn.DefaultBorderColor ?? Colour.DefaultBorder.Get(colorScheme, "Button", cname), num5, graphicsPath);
						PaintButton(g, font, btn, color, rect_read);
					}
				}
				else
				{
					if (btn.ExtraMouseDown)
					{
						g.Fill(color4, graphicsPath);
					}
					else if (btn.AnimationHover)
					{
						g.Fill(Helper.ToColor(btn.AnimationHoverValue, color3), graphicsPath);
					}
					else if (btn.ExtraMouseHover)
					{
						g.Fill(color3, graphicsPath);
					}
					PaintButton(g, font, btn, color, rect_read);
				}
			}
			else
			{
				if (btn.BorderWidth > 0f)
				{
					g.Fill(Colour.FillTertiary.Get(colorScheme, "Button", cname), graphicsPath);
				}
				PaintButton(g, font, btn, Colour.TextQuaternary.Get(colorScheme, "Button", cname), rect_read);
			}
			return;
		}
		Color color5;
		Color color6;
		Color color7;
		Color color8;
		switch (btn.Type)
		{
		case TTypeMini.Error:
			color5 = Colour.Error.Get(colorScheme, "Button", cname);
			color6 = Colour.ErrorColor.Get(colorScheme, "Button", cname);
			color7 = Colour.ErrorHover.Get(colorScheme, "Button", cname);
			color8 = Colour.ErrorActive.Get(colorScheme, "Button", cname);
			break;
		case TTypeMini.Success:
			color5 = Colour.Success.Get(colorScheme, "Button", cname);
			color6 = Colour.SuccessColor.Get(colorScheme, "Button", cname);
			color7 = Colour.SuccessHover.Get(colorScheme, "Button", cname);
			color8 = Colour.SuccessActive.Get(colorScheme, "Button", cname);
			break;
		case TTypeMini.Info:
			color5 = Colour.Info.Get(colorScheme, "Button", cname);
			color6 = Colour.InfoColor.Get(colorScheme, "Button", cname);
			color7 = Colour.InfoHover.Get(colorScheme, "Button", cname);
			color8 = Colour.InfoActive.Get(colorScheme, "Button", cname);
			break;
		case TTypeMini.Warn:
			color5 = Colour.Warning.Get(colorScheme, "Button", cname);
			color6 = Colour.WarningColor.Get(colorScheme, "Button", cname);
			color7 = Colour.WarningHover.Get(colorScheme, "Button", cname);
			color8 = Colour.WarningActive.Get(colorScheme, "Button", cname);
			break;
		default:
			color5 = Colour.Primary.Get(colorScheme, "Button", cname);
			color6 = Colour.PrimaryColor.Get(colorScheme, "Button", cname);
			color7 = Colour.PrimaryHover.Get(colorScheme, "Button", cname);
			color8 = Colour.PrimaryActive.Get(colorScheme, "Button", cname);
			break;
		}
		if (btn.Fore.HasValue)
		{
			color6 = btn.Fore.Value;
		}
		if (btn.Back.HasValue)
		{
			color5 = btn.Back.Value;
		}
		if (btn.BackHover.HasValue)
		{
			color7 = btn.BackHover.Value;
		}
		if (btn.BackActive.HasValue)
		{
			color8 = btn.BackActive.Value;
		}
		using GraphicsPath graphicsPath4 = PathButton(rect_read, btn, num);
		if (btn.AnimationClick)
		{
			int num6 = (int)((float)Math.Max(gap.Height, gap.Width) * g.Dpi);
			float num7 = (float)rect_read.Width + (float)num6 * btn.AnimationClickValue;
			float num8 = (float)rect_read.Height + (float)num6 * btn.AnimationClickValue;
			float alpha2 = 100f * (1f - btn.AnimationClickValue);
			using GraphicsPath graphicsPath5 = new RectangleF((float)rect_read.X + ((float)rect_read.Width - num7) / 2f, (float)rect_read.Y + ((float)rect_read.Height - num8) / 2f, num7, num8).RoundPath(num, btn.Shape);
			graphicsPath5.AddPath(graphicsPath4, connect: false);
			g.Fill(Helper.ToColor(alpha2, color5), graphicsPath5);
		}
		if (btn.Ghost)
		{
			if (btn.BorderWidth > 0f)
			{
				float num9 = btn.BorderWidth * g.Dpi;
				if (btn.ExtraMouseDown)
				{
					g.Draw(color8, num9, graphicsPath4);
					PaintButton(g, font, btn, color8, rect_read);
					return;
				}
				if (btn.AnimationHover)
				{
					Color overlay2 = Helper.ToColor(btn.AnimationHoverValue, color7);
					g.Draw(((enable && btn.Enabled) ? color5 : Colour.FillTertiary.Get(colorScheme, "Button", cname)).BlendColors(overlay2), num9, graphicsPath4);
					PaintButton(g, font, btn, color5.BlendColors(overlay2), rect_read);
					return;
				}
				if (btn.ExtraMouseHover)
				{
					g.Draw(color7, num9, graphicsPath4);
					PaintButton(g, font, btn, color7, rect_read);
					return;
				}
				if (enable && btn.Enabled)
				{
					using Brush brush = btn.BackExtend.BrushEx(rect_read, color5);
					g.Draw(brush, num9, graphicsPath4);
				}
				else
				{
					g.Draw(Colour.FillTertiary.Get(colorScheme, "Button", cname), num9, graphicsPath4);
				}
				PaintButton(g, font, btn, (enable && btn.Enabled) ? color5 : Colour.TextQuaternary.Get(colorScheme, "Button", cname), rect_read);
			}
			else
			{
				PaintButton(g, font, btn, (enable && btn.Enabled) ? color5 : Colour.TextQuaternary.Get(colorScheme, "Button", cname), rect_read);
			}
			return;
		}
		if (enable && btn.Enabled)
		{
			using GraphicsPath graphicsPath6 = new RectangleF(rect_read.X, rect_read.Y + 3, rect_read.Width, rect_read.Height).RoundPath(num);
			graphicsPath6.AddPath(graphicsPath4, connect: false);
			g.Fill(color5.rgba((Config.Mode == TMode.Dark) ? 0.15f : 0.1f), graphicsPath6);
		}
		if (enable && btn.Enabled)
		{
			using Brush brush2 = btn.BackExtend.BrushEx(rect_read, color5);
			g.Fill(brush2, graphicsPath4);
		}
		else
		{
			g.Fill(Colour.FillTertiary.Get(colorScheme, "Button", cname), graphicsPath4);
		}
		if (btn.ExtraMouseDown)
		{
			g.Fill(color8, graphicsPath4);
		}
		else if (btn.AnimationHover)
		{
			g.Fill(Helper.ToColor(btn.AnimationHoverValue, color7), graphicsPath4);
		}
		else if (btn.ExtraMouseHover)
		{
			g.Fill(color7, graphicsPath4);
		}
		PaintButton(g, font, btn, (enable && btn.Enabled) ? color6 : Colour.TextQuaternary.Get(colorScheme, "Button", cname), rect_read);
	}

	private static GraphicsPath PathButton(RectangleF rect_read, CellButton btn, float _radius)
	{
		if (btn.Shape == TShape.Circle)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(rect_read);
			return graphicsPath;
		}
		return rect_read.RoundPath(_radius);
	}

	private static Size MeasureText(Canvas g, string? text, Font font, Rectangle rect, out int txt_height, out bool multiLine)
	{
		Size result = g.MeasureText("龍Qq", font);
		txt_height = result.Height;
		multiLine = false;
		if (text == null)
		{
			return result;
		}
		Size result2 = g.MeasureText(text, font);
		if (result2.Width > rect.Width && text.Contains("\n"))
		{
			multiLine = true;
			return g.MeasureText(text, font, rect.Width);
		}
		return result2;
	}

	private static void PaintButton(Canvas g, Font font, CellButton btn, Color color, Rectangle rect_read)
	{
		bool flag = btn.Loading && btn.LoadingValue > -1f;
		if (string.IsNullOrEmpty(btn.Text))
		{
			Size font_size = g.MeasureString("龍Qq", font);
			Rectangle rect = PaintButtonImageRectCenter(btn, font_size, rect_read);
			if (flag)
			{
				g.PaintLoading(btn.LoadingIcon, btn.LoadingSvg, rect, btn.AnimationLoadingValue, btn.LoadingValue, color, rect_read.Height, 0.06f);
			}
			else if (PaintButtonImageNoText(g, btn, color, rect) && btn.ShowArrow)
			{
				int num = (int)((float)font_size.Height * btn.IconRatio);
				Rectangle rect2 = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				PaintButtonTextArrow(g, btn, rect2, color);
			}
			return;
		}
		Size font_size2 = MeasureText(g, btn.Text, font, rect_read, out var txt_height, out var multiLine);
		bool flag2 = flag || btn.HasIcon;
		bool showArrow = btn.ShowArrow;
		Rectangle rect_text;
		if (flag2 | showArrow)
		{
			if (flag2 & showArrow)
			{
				rect_text = Button.RectAlignLR(g, txt_height, multiLine, font, btn.IconPosition, btn.IconRatio, btn.IconGap, font_size2, rect_read, out var rect_l, out var rect_r);
				if (flag)
				{
					g.PaintLoading(btn.LoadingIcon, btn.LoadingSvg, rect_l, btn.AnimationLoadingValue, btn.LoadingValue, color, rect_l.Height, 0.14f);
				}
				else
				{
					PaintButtonPaintImage(g, btn, color, rect_l);
				}
				PaintButtonTextArrow(g, btn, rect_r, color);
			}
			else if (flag2)
			{
				rect_text = Button.RectAlignL(g, txt_height, multiLine, textCenter: false, font, btn.IconPosition, btn.IconRatio, btn.IconGap, font_size2, rect_read, out var rect_l2);
				if (flag)
				{
					g.PaintLoading(btn.LoadingIcon, btn.LoadingSvg, rect_l2, btn.AnimationLoadingValue, btn.LoadingValue, color, rect_l2.Height, 0.14f);
				}
				else
				{
					PaintButtonPaintImage(g, btn, color, rect_l2);
				}
			}
			else
			{
				rect_text = Button.RectAlignR(g, txt_height, multiLine, font, btn.IconPosition, btn.IconRatio, btn.IconGap, font_size2, rect_read, out var rect_r2);
				PaintButtonTextArrow(g, btn, rect_r2, color);
			}
		}
		else
		{
			int num2 = (int)((float)font_size2.Height * 0.4f);
			int num3 = num2 * 2;
			rect_text = new Rectangle(rect_read.X + num2, rect_read.Y + num2, rect_read.Width - num3, rect_read.Height - num3);
			PaintButtonTextAlign(btn, rect_read, ref rect_text);
		}
		using SolidBrush brush = new SolidBrush(color);
		g.DrawText(btn.Text, font, brush, rect_text, btn.s_f);
	}

	private static void PaintButtonTextArrow(Canvas g, CellButton btn, Rectangle rect, Color color)
	{
		using Pen pen = new Pen(color, 2f * g.Dpi);
		LineCap startCap = (pen.EndCap = LineCap.Round);
		pen.StartCap = startCap;
		if (btn.IsLink)
		{
			g.DrawLines(pen, rect.TriangleLinesHorizontal(btn.ArrowProg));
		}
		else
		{
			g.DrawLines(pen, rect.TriangleLinesVertical(btn.ArrowProg));
		}
	}

	private static void PaintButtonTextAlign(CellButton btn, Rectangle rect_read, ref Rectangle rect_text)
	{
		switch (btn.TextAlign)
		{
		case ContentAlignment.MiddleLeft:
		case ContentAlignment.MiddleCenter:
		case ContentAlignment.MiddleRight:
			rect_text.Y = rect_read.Y;
			rect_text.Height = rect_read.Height;
			break;
		case ContentAlignment.TopLeft:
		case ContentAlignment.TopCenter:
		case ContentAlignment.TopRight:
			rect_text.Height = rect_read.Height - rect_text.Y;
			break;
		}
	}

	/// <summary>
	/// 渲染图片（没有文字）
	/// </summary>
	/// <param name="g">GDI</param>
	/// <param name="color">颜色</param>
	/// <param name="rect">区域</param>
	private static bool PaintButtonImageNoText(Canvas g, CellButton btn, Color? color, Rectangle rect)
	{
		if (btn.AnimationImageHover)
		{
			PaintButtonCoreImage(g, btn, rect, color, 1f - btn.AnimationImageHoverValue);
			PaintButtonCoreImageHover(g, btn, rect, color, btn.AnimationImageHoverValue);
			return false;
		}
		if (btn.ExtraMouseHover && PaintButtonCoreImageHover(g, btn, rect, color))
		{
			return false;
		}
		if (PaintButtonCoreImage(g, btn, rect, color))
		{
			return false;
		}
		return true;
	}

	private static bool PaintButtonCoreImage(Canvas g, CellButton btn, Rectangle rect, Color? color, float opacity = 1f)
	{
		int num = 0;
		if (btn.Icon != null)
		{
			g.Image(btn.Icon, rect, opacity);
			num++;
		}
		if (btn.IconSvg != null && g.Svg(btn.IconSvg, rect, opacity, color))
		{
			num++;
		}
		return num > 0;
	}

	private static bool PaintButtonCoreImageHover(Canvas g, CellButton btn, Rectangle rect, Color? color, float opacity = 1f)
	{
		int num = 0;
		if (btn.IconHover != null)
		{
			g.Image(btn.IconHover, rect, opacity);
			num++;
		}
		if (btn.IconHoverSvg != null && g.Svg(btn.IconHoverSvg, rect, opacity, color))
		{
			num++;
		}
		return num > 0;
	}

	private static Rectangle PaintButtonImageRectCenter(CellButton btn, Size font_size, Rectangle rect_read)
	{
		int num = (int)((float)font_size.Height * btn.IconRatio * 1.125f);
		return new Rectangle(rect_read.X + (rect_read.Width - num) / 2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
	}

	/// <summary>
	/// 渲染图片
	/// </summary>
	/// <param name="g">GDI</param>
	/// <param name="color">颜色</param>
	/// <param name="rectl">图标区域</param>
	private static void PaintButtonPaintImage(Canvas g, CellButton btn, Color? color, Rectangle rectl)
	{
		if (btn.AnimationImageHover)
		{
			PaintButtonCoreImage(g, btn, rectl, color, 1f - btn.AnimationImageHoverValue);
			PaintButtonCoreImageHover(g, btn, rectl, color, btn.AnimationImageHoverValue);
		}
		else if (!btn.ExtraMouseHover || !PaintButtonCoreImageHover(g, btn, rectl, color))
		{
			PaintButtonCoreImage(g, btn, rectl, color);
		}
	}

	internal static void PaintLink(Canvas g, Font font, Rectangle rect_read, CellLink link, bool enable, TAMode colorScheme, string cname)
	{
		if (link.ExtraMouseDown)
		{
			g.DrawText(link.Text, font, Colour.PrimaryActive.Get(colorScheme, "Button", cname), rect_read, link.s_f);
		}
		else if (link.AnimationHover)
		{
			g.DrawText(link.Text, font, Colour.Primary.Get(colorScheme, "Button", cname).BlendColors(link.AnimationHoverValue, Colour.PrimaryHover.Get(colorScheme, "Button", cname)), rect_read, link.s_f);
		}
		else if (link.ExtraMouseHover)
		{
			g.DrawText(link.Text, font, Colour.PrimaryHover.Get(colorScheme, "Button", cname), rect_read, link.s_f);
		}
		else
		{
			g.DrawText(link.Text, font, (enable && link.Enabled) ? Colour.Primary.Get(colorScheme, "Button", cname) : Colour.TextQuaternary.Get(colorScheme, "Button", cname), rect_read, link.s_f);
		}
	}

	protected override void OnDraw(DrawEventArgs e)
	{
		Canvas canvas = e.Canvas;
		if (rows == null)
		{
			if (Empty)
			{
				PaintEmpty(canvas, e.Rect, 0);
			}
			base.OnDraw(e);
			return;
		}
		if (columnfont == null)
		{
			using Font column_font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold);
			PaintTable(canvas, rows.List, e.Rect, e.Rect.PaddingRect(Padding, Dpi, borderWidth / 2f), column_font);
		}
		else
		{
			PaintTable(canvas, rows.List, e.Rect, e.Rect.PaddingRect(Padding, Dpi, borderWidth / 2f), columnfont);
		}
		if (emptyHeader && Empty && rows.Length == 1)
		{
			PaintEmpty(canvas, e.Rect, rows.First.RECT.Height);
		}
		ScrollBar.Paint(canvas, ColorScheme);
		base.OnDraw(e);
	}

	private void PaintTable(Canvas g, RowTemplate[] rows, Rectangle rect, Rectangle rect_real, Font column_font)
	{
		float num = (float)radius * Dpi;
		int valueX = ScrollBar.ValueX;
		int scrollBarRealY = ScrollBarRealY;
		using SolidBrush solidBrush = new SolidBrush(fore ?? Colour.Text.Get(ColorScheme, "Table", Name));
		using SolidBrush foreEnable = new SolidBrush(fore ?? Colour.TextQuaternary.Get(ColorScheme, "Table", Name));
		using SolidBrush forecolumn = new SolidBrush(columnfore ?? fore ?? Colour.Text.Get(ColorScheme, "Table", Name));
		GraphicsPath graphicsPath = null;
		StyleRow[] d_shows;
		StyleRow[] d_summarys;
		if (visibleHeader)
		{
			if (num > 0f)
			{
				graphicsPath = rect_divider.RoundPath(num, roundTL: true, roundTR: true, roundBR: false, roundBL: false);
				g.SetClip(graphicsPath);
			}
			else
			{
				g.SetClip(rect_divider);
			}
			if (fixedHeader)
			{
				DirtyByVisibleHeaderFixedHeader(rows, scrollBarRealY, out d_shows, out d_summarys);
				g.TranslateTransform(0f, -scrollBarRealY);
				StyleRow[] array = d_shows;
				foreach (StyleRow row in array)
				{
					PaintBgRowFront(g, row);
				}
				g.ResetTransform();
				g.TranslateTransform(-valueX, -scrollBarRealY);
				array = d_shows;
				foreach (StyleRow styleRow in array)
				{
					PaintItemBgRowStyle(g, styleRow.row);
				}
				g.ResetTransform();
				g.TranslateTransform(0f, -scrollBarRealY);
				array = d_shows;
				foreach (StyleRow styleRow2 in array)
				{
					PaintBg(g, styleRow2.row);
				}
				g.TranslateTransform(-valueX, 0f);
				array = d_shows;
				foreach (StyleRow row2 in array)
				{
					PaintForeItem(g, row2, solidBrush, foreEnable);
				}
				g.ResetTransform();
				g.TranslateTransform(0f, -scrollBarRealY);
				if (dividers.Length != 0)
				{
					PaintBorder(g, dividers, 1);
				}
				g.ResetTransform();
				PaintTableBgHeader(g, rows[0], num, valueX);
				g.TranslateTransform(-valueX, 0f);
				PaintTableHeader(g, rows[0], forecolumn, column_font, num);
				if (dividers.Length != 0)
				{
					g.ResetTransform();
					PaintBorder(g, dividers[0]);
					g.ResetTransform();
					g.TranslateTransform(-valueX, 0f);
				}
				if (dividerHs.Length != 0)
				{
					PaintBorderH(g, dividerHs);
				}
			}
			else
			{
				DirtyByVisibleHeader(rows, scrollBarRealY, out d_shows, out d_summarys);
				g.TranslateTransform(0f, -scrollBarRealY);
				List<StyleRow> list = new List<StyleRow>(d_shows.Length);
				StyleRow[] array = d_shows;
				foreach (StyleRow styleRow3 in array)
				{
					if (styleRow3.row.IsOther)
					{
						list.Add(styleRow3);
						PaintBgRowFront(g, styleRow3);
					}
				}
				g.TranslateTransform(-valueX, 0f);
				foreach (StyleRow item in list)
				{
					PaintItemBgRowStyle(g, item.row);
				}
				g.ResetTransform();
				g.TranslateTransform(0f, -scrollBarRealY);
				array = d_shows;
				foreach (StyleRow styleRow4 in array)
				{
					if (styleRow4.row.IsColumn)
					{
						PaintTableBgHeader(g, styleRow4.row, num, valueX);
					}
					else
					{
						PaintBg(g, styleRow4.row);
					}
				}
				g.TranslateTransform(-valueX, 0f);
				array = d_shows;
				foreach (StyleRow styleRow5 in array)
				{
					if (styleRow5.row.IsColumn)
					{
						PaintTableHeader(g, styleRow5.row, forecolumn, column_font, num);
					}
					else
					{
						PaintForeItem(g, styleRow5, solidBrush, foreEnable);
					}
				}
				g.ResetTransform();
				g.TranslateTransform(0f, -scrollBarRealY);
				if (dividers.Length != 0)
				{
					PaintBorder(g, dividers);
				}
				g.ResetTransform();
				if (bordered)
				{
					g.TranslateTransform(-valueX, 0f);
				}
				else
				{
					g.TranslateTransform(-valueX, -scrollBarRealY);
				}
				if (dividerHs.Length != 0)
				{
					PaintBorderH(g, dividerHs);
				}
			}
		}
		else
		{
			if (num > 0f)
			{
				graphicsPath = rect_divider.RoundPath(num);
				g.SetClip(graphicsPath);
			}
			else
			{
				g.SetClip(rect_divider);
			}
			DirtyByNone(rows, scrollBarRealY, out d_shows, out d_summarys);
			g.TranslateTransform(0f, -scrollBarRealY);
			StyleRow[] array = d_shows;
			foreach (StyleRow row3 in array)
			{
				PaintBgRowFront(g, row3);
			}
			g.ResetTransform();
			g.TranslateTransform(-valueX, -scrollBarRealY);
			array = d_shows;
			foreach (StyleRow styleRow6 in array)
			{
				PaintItemBgRow(g, styleRow6.row);
			}
			g.ResetTransform();
			g.TranslateTransform(0f, -scrollBarRealY);
			array = d_shows;
			foreach (StyleRow styleRow7 in array)
			{
				PaintBg(g, styleRow7.row);
			}
			g.TranslateTransform(-valueX, 0f);
			array = d_shows;
			foreach (StyleRow row4 in array)
			{
				PaintForeItem(g, row4, solidBrush, foreEnable);
			}
			g.ResetTransform();
			g.TranslateTransform(0f, -scrollBarRealY);
			if (dividers.Length != 0)
			{
				PaintBorder(g, dividers);
			}
			g.ResetTransform();
			if (bordered)
			{
				g.TranslateTransform(-valueX, 0f);
			}
			else
			{
				g.TranslateTransform(0f, -scrollBarRealY);
			}
			if (dividerHs.Length != 0)
			{
				PaintBorderH(g, dividerHs);
			}
		}
		g.ResetClip();
		g.ResetTransform();
		PaintMergeCells(g, rows, valueX, scrollBarRealY, solidBrush, foreEnable);
		if (d_shows.Length != 0 && ScrollBar.ShowX && (fixedColumnL != null || fixedColumnR != null))
		{
			PaintFixedColumnL(g, rect, rect_read, rows, d_shows, solidBrush, foreEnable, forecolumn, column_font, valueX, scrollBarRealY, num);
			PaintFixedColumnR(g, rect, rect_read, rows, d_shows, solidBrush, foreEnable, forecolumn, column_font, valueX, scrollBarRealY, num);
		}
		else
		{
			showFixedColumnL = (showFixedColumnR = false);
		}
		if (d_summarys.Length != 0)
		{
			sFixedB = PaintFixedSummary(g, rect, rect_read, d_summarys, solidBrush, foreEnable, forecolumn, column_font, valueX, scrollBarRealY, num);
		}
		else
		{
			sFixedB = -1;
		}
		if (bordered)
		{
			g.ResetClip();
			Color color = borderColor ?? Colour.BorderColor.Get(ColorScheme, "Table", Name);
			if (graphicsPath == null)
			{
				g.Draw(color, borderWidth * Dpi, rect_divider);
			}
			else
			{
				g.Draw(color, borderWidth * Dpi, graphicsPath);
			}
		}
		graphicsPath?.Dispose();
	}

	private void DirtyByVisibleHeaderFixedHeader(RowTemplate[] rows, int sy, out StyleRow[] d_shows, out StyleRow[] d_summarys)
	{
		int num = 0;
		List<StyleRow> list = new List<StyleRow>(rows.Length);
		List<StyleRow> list2 = new List<StyleRow>(1);
		foreach (RowTemplate rowTemplate in rows)
		{
			if (rowTemplate.Type == RowType.Summary)
			{
				rowTemplate.SHOW = true;
				StyleRow item = new StyleRow(rowTemplate, OnSetRowStyle(rowTemplate.RECORD, rowTemplate.Type, rowTemplate.INDEX, num));
				list.Add(item);
				list2.Add(item);
			}
			else
			{
				rowTemplate.SHOW = rowTemplate.Type == RowType.None && rect_read.IsItemVisible(sy, rowTemplate.RECT);
				if (rowTemplate.SHOW)
				{
					list.Add(new StyleRow(rowTemplate, OnSetRowStyle(rowTemplate.RECORD, rowTemplate.Type, rowTemplate.INDEX, num)));
				}
			}
			num++;
		}
		d_shows = list.ToArray();
		d_summarys = list2.ToArray();
	}

	private void DirtyByVisibleHeader(RowTemplate[] rows, int sy, out StyleRow[] d_shows, out StyleRow[] d_summarys)
	{
		int num = 0;
		List<StyleRow> list = new List<StyleRow>(rows.Length);
		List<StyleRow> list2 = new List<StyleRow>(1);
		foreach (RowTemplate rowTemplate in rows)
		{
			if (rowTemplate.Type == RowType.Summary)
			{
				rowTemplate.SHOW = true;
				StyleRow item = new StyleRow(rowTemplate, OnSetRowStyle(rowTemplate.RECORD, rowTemplate.Type, rowTemplate.INDEX, num));
				list.Add(item);
				list2.Add(item);
			}
			else
			{
				rowTemplate.SHOW = (rowTemplate.Type == RowType.None || rowTemplate.Type == RowType.Column) && rect_read.IsItemVisible(sy, rowTemplate.RECT);
				if (rowTemplate.SHOW)
				{
					list.Add(new StyleRow(rowTemplate, OnSetRowStyle(rowTemplate.RECORD, rowTemplate.Type, rowTemplate.INDEX, num)));
				}
			}
			num++;
		}
		d_shows = list.ToArray();
		d_summarys = list2.ToArray();
	}

	private void DirtyByNone(RowTemplate[] rows, int sy, out StyleRow[] d_shows, out StyleRow[] d_summarys)
	{
		int num = 0;
		rows[0].SHOW = false;
		List<StyleRow> list = new List<StyleRow>(rows.Length);
		List<StyleRow> list2 = new List<StyleRow>(1);
		for (int i = 1; i < rows.Length; i++)
		{
			RowTemplate rowTemplate = rows[i];
			if (rowTemplate.Type == RowType.Summary)
			{
				rowTemplate.SHOW = true;
				StyleRow item = new StyleRow(rowTemplate, OnSetRowStyle(rowTemplate.RECORD, rowTemplate.Type, rowTemplate.INDEX, num));
				list.Add(item);
				list2.Add(item);
			}
			else
			{
				rowTemplate.SHOW = rect_read.IsItemVisible(sy, rowTemplate.RECT);
				if (rowTemplate.SHOW)
				{
					list.Add(new StyleRow(rowTemplate, OnSetRowStyle(rowTemplate.RECORD, rowTemplate.Type, rowTemplate.INDEX, num)));
				}
			}
			num++;
		}
		d_shows = list.ToArray();
		d_summarys = list2.ToArray();
	}

	private void PaintTableBgHeader(Canvas g, RowTemplate row, float radius, int sx)
	{
		GraphicsState graphicsState = g.Save();
		using (SolidBrush brush = new SolidBrush(columnback ?? Colour.TagDefaultBg.Get(ColorScheme, "Table", Name)))
		{
			if (radius > 0f)
			{
				using GraphicsPath path = row.RECT.RoundPath(radius, roundTL: true, roundTR: true, roundBR: false, roundBL: false);
				g.Fill(brush, path);
				g.SetClip(path, CombineMode.Intersect);
				g.TranslateTransform(-sx, 0f);
				CELL[] cells = row.cells;
				foreach (CELL cELL in cells)
				{
					if (cELL.COLUMN.ColStyle != null && cELL.COLUMN.ColStyle.BackColor.HasValue)
					{
						g.Fill(cELL.COLUMN.ColStyle.BackColor.Value, cELL.RECT);
					}
				}
			}
			else
			{
				g.Fill(brush, row.RECT);
				g.TranslateTransform(-sx, 0f);
				CELL[] cells = row.cells;
				foreach (CELL cELL2 in cells)
				{
					if (cELL2.COLUMN.ColStyle != null && cELL2.COLUMN.ColStyle.BackColor.HasValue)
					{
						g.Fill(cELL2.COLUMN.ColStyle.BackColor.Value, cELL2.RECT);
					}
				}
			}
		}
		g.Restore(graphicsState);
	}

	private void PaintTableHeader(Canvas g, RowTemplate row, SolidBrush fore, Font column_font, float radius)
	{
		CELL[] cells;
		if (StackedHeaderRows == null)
		{
			cells = row.cells;
			for (int i = 0; i < cells.Length; i++)
			{
				((TCellColumn)cells[i]).Print(g, ColorScheme, column_font, fore);
			}
			if (dragHeader == null || !dragHeader.enable)
			{
				return;
			}
			cells = row.cells;
			foreach (CELL cELL in cells)
			{
				int iNDEX_REAL = cELL.COLUMN.INDEX_REAL;
				if (dragHeader.i == iNDEX_REAL)
				{
					using SolidBrush brush = new SolidBrush(Colour.FillSecondary.Get(ColorScheme, "Table", Name));
					if (radius > 0f)
					{
						if (cELL.INDEX == 0)
						{
							using GraphicsPath path = cELL.RECT.RoundPath(radius, roundTL: true, roundTR: false, roundBR: false, roundBL: false);
							g.Fill(brush, path);
						}
						else if (cELL.INDEX == row.cells.Length - 1)
						{
							using GraphicsPath path2 = cELL.RECT.RoundPath(radius, roundTL: false, roundTR: true, roundBR: false, roundBL: false);
							g.Fill(brush, path2);
						}
						else
						{
							g.Fill(brush, cELL.RECT);
						}
					}
					else
					{
						g.Fill(brush, cELL.RECT);
					}
				}
				if (dragHeader.im != iNDEX_REAL)
				{
					continue;
				}
				using SolidBrush brush2 = new SolidBrush(Colour.BorderColor.Get(ColorScheme, "Table", Name));
				int num = (int)(2f * Dpi);
				if (dragHeader.last)
				{
					g.Fill(brush2, new Rectangle(cELL.RECT.Right - num, cELL.RECT.Y, num * 2, cELL.RECT.Height));
				}
				else
				{
					g.Fill(brush2, new Rectangle(cELL.RECT.X - num, cELL.RECT.Y, num * 2, cELL.RECT.Height));
				}
			}
			return;
		}
		int num2 = StackedHeaderRows.Length + 1;
		List<int> handkey = new List<int>(num2);
		List<int> dskey = new List<int>(num2);
		GraphicsState graphicsState = g.Save();
		int num3 = row.RECT.Y;
		int num4 = row.Height / num2;
		int num5 = 0;
		int num6 = (int)((float)_gap.Width * Dpi);
		int num7 = num6 * 2;
		StackedHeaderRow[] stackedHeaderRows = StackedHeaderRows;
		for (int i = 0; i < stackedHeaderRows.Length; i++)
		{
			StackedColumn[] stackedColumns = stackedHeaderRows[i].StackedColumns;
			foreach (StackedColumn item in stackedColumns)
			{
				PaintTableHeaderStacked(g, row, fore, column_font, ref handkey, ref dskey, num3, num4, item);
			}
			num3 += num4;
		}
		int num8 = row.RECT.Y + num4 / 2 * (num2 - 1);
		cells = row.cells;
		for (int i = 0; i < cells.Length; i++)
		{
			TCellColumn tCellColumn = (TCellColumn)cells[i];
			if (dskey.Contains(num5) && dividerHs[num5].h)
			{
				dividerHs[num5].h = false;
				if (bordered)
				{
					dividerHs[num5].b = num3;
					dividerHs[num5].c = num4;
				}
				else
				{
					dividerHs[num5].b = num3 + num6;
					dividerHs[num5].c = num4 - num7;
				}
			}
			if (handkey.Contains(tCellColumn.INDEX))
			{
				tCellColumn.offsety = num8;
				g.TranslateTransform(0f, num8);
			}
			tCellColumn.Print(g, ColorScheme, column_font, fore);
			g.Restore(graphicsState);
			graphicsState = g.Save();
			num5++;
		}
		g.Restore(graphicsState);
		if (dragHeader == null || !dragHeader.enable)
		{
			return;
		}
		cells = row.cells;
		foreach (CELL cELL2 in cells)
		{
			int iNDEX_REAL2 = cELL2.COLUMN.INDEX_REAL;
			if (dragHeader.i == iNDEX_REAL2)
			{
				using SolidBrush brush3 = new SolidBrush(Colour.FillSecondary.Get(ColorScheme, "Table", Name));
				if (radius > 0f)
				{
					if (cELL2.INDEX == 0)
					{
						using GraphicsPath path3 = cELL2.RECT.RoundPath(radius, roundTL: true, roundTR: false, roundBR: false, roundBL: false);
						g.Fill(brush3, path3);
					}
					else if (cELL2.INDEX == row.cells.Length - 1)
					{
						using GraphicsPath path4 = cELL2.RECT.RoundPath(radius, roundTL: false, roundTR: true, roundBR: false, roundBL: false);
						g.Fill(brush3, path4);
					}
					else
					{
						g.Fill(brush3, cELL2.RECT);
					}
				}
				else
				{
					g.Fill(brush3, cELL2.RECT);
				}
			}
			if (dragHeader.im != iNDEX_REAL2)
			{
				continue;
			}
			using SolidBrush brush4 = new SolidBrush(Colour.BorderColor.Get(ColorScheme, "Table", Name));
			int num9 = (int)(2f * Dpi);
			if (dragHeader.last)
			{
				g.Fill(brush4, new Rectangle(cELL2.RECT.Right - num9, cELL2.RECT.Y, num9 * 2, cELL2.RECT.Height));
			}
			else
			{
				g.Fill(brush4, new Rectangle(cELL2.RECT.X - num9, cELL2.RECT.Y, num9 * 2, cELL2.RECT.Height));
			}
		}
	}

	private void PaintTableHeaderStacked(Canvas g, RowTemplate row, SolidBrush fore, Font column_font, ref List<int> handkey, ref List<int> dskey, int rY, int rHeight, StackedColumn item)
	{
		CELL[] cells;
		if (item.ChildColumns.Length > 1)
		{
			CELL cELL = null;
			CELL cELL2 = null;
			int num = 0;
			cells = row.cells;
			foreach (CELL cELL3 in cells)
			{
				if (cELL3.COLUMN.Key == item.ChildColumns[0])
				{
					cELL = cELL3;
					dskey.Add(num);
					handkey.Add(cELL.INDEX);
					if (cELL2 == null)
					{
						continue;
					}
					PaintTableHeaderStacked(g, fore, column_font, rY, rHeight, item, cELL, cELL2);
				}
				else
				{
					if (cELL3.COLUMN.Key == item.ChildColumns[item.ChildColumns.Length - 1])
					{
						cELL2 = cELL3;
						if (cELL != null)
						{
							dskey.Add(num);
							handkey.Add(cELL2.INDEX);
							PaintTableHeaderStacked(g, fore, column_font, rY, rHeight, item, cELL, cELL2);
						}
						break;
					}
					if (cELL != null)
					{
						dskey.Add(num);
						handkey.Add(cELL3.INDEX);
					}
				}
				num++;
			}
			return;
		}
		string text = item.ChildColumns[0];
		int num2 = 0;
		cells = row.cells;
		foreach (CELL cELL4 in cells)
		{
			if (cELL4.COLUMN.Key == text)
			{
				handkey.Add(cELL4.INDEX);
				PaintTableHeaderStacked(g, fore, column_font, rY, rHeight, item, cELL4);
				break;
			}
			num2++;
		}
	}

	private void PaintTableHeaderStacked(Canvas g, SolidBrush fore, Font column_font, int rY, int rHeight, StackedColumn item, CELL column)
	{
		GraphicsState graphicsState = g.Save();
		Rectangle rect = new Rectangle(column.RECT_REAL.X, rY, column.RECT_REAL.Width, rHeight);
		if (column is TCellColumn tCellColumn)
		{
			rect.Width += tCellColumn.SFWidth;
		}
		if (bordered)
		{
			PaintBorder(g, new DividerList(rect.Bottom, column.RECT.X, column.RECT.Width));
		}
		g.DrawText(item.HeaderText, column_font, item.ForeColor, fore, rect, StringFormat(column.COLUMN, isColumn: true));
		g.Restore(graphicsState);
	}

	private void PaintTableHeaderStacked(Canvas g, SolidBrush fore, Font column_font, int rY, int rHeight, StackedColumn item, CELL first, CELL last)
	{
		GraphicsState graphicsState = g.Save();
		Rectangle rect = new Rectangle(first.RECT.X, rY, last.RECT.Right - first.RECT.X, rHeight);
		if (bordered)
		{
			PaintBorder(g, new DividerList(rect.Bottom, rect.X, rect.Width));
		}
		g.DrawText(item.HeaderText, column_font, item.ForeColor, fore, rect, StringFormat(ColumnAlign.Center));
		g.Restore(graphicsState);
	}

	/// <summary>
	/// 渲染背景行（前置）
	/// </summary>
	private void PaintBgRowFront(Canvas g, StyleRow row)
	{
		if (row.style != null && row.style.BackColor.HasValue)
		{
			g.Fill(row.style.BackColor.Value, row.row.RECT);
		}
		PaintBgSelect(g, row.row, row.row.RECT);
	}

	/// <summary>
	/// 渲染背景行
	/// </summary>
	private void PaintBg(Canvas g, RowTemplate row)
	{
		OnRowPaintBegin(g, row.RECT, row.RECORD, row.Type, row.INDEX);
		if (dragBody == null)
		{
			PaintBgHover(g, row, row.RECT);
		}
		else if (dragBody.i == row.INDEX)
		{
			g.Fill(Colour.FillSecondary.Get(ColorScheme, "Table", Name), row.RECT);
		}
		else if (dragBody.im == row.INDEX)
		{
			using SolidBrush brush = new SolidBrush(Colour.BorderColor.Get(ColorScheme, "Table", Name));
			int num = (int)(2f * Dpi);
			if (dragBody.last)
			{
				g.Fill(brush, new Rectangle(row.RECT.X, row.RECT.Bottom - num, row.RECT.Width, num * 2));
			}
			else
			{
				g.Fill(brush, new Rectangle(row.RECT.X, row.RECT.Y - num, row.RECT.Width, num * 2));
			}
		}
		OnRowPaint(g, row.RECT, row.RECORD, row.Type, row.INDEX);
	}

	private void PaintBgSelect(Canvas g, RowTemplate row, Rectangle rect)
	{
		if (row.Type == RowType.None && (selectedIndex.Contains(row.INDEX) || (row.Select && ShowCheckBg)))
		{
			if (selectedIndex.Contains(row.INDEX) && row.Select)
			{
				g.Fill(Colour.PrimaryBgHover.Get(ColorScheme, "Table", Name), rect);
			}
			else
			{
				g.Fill(rowSelectedBg ?? Colour.PrimaryBg.Get(ColorScheme, "Table", Name), rect);
			}
		}
	}

	private void PaintBgHover(Canvas g, RowTemplate row, Rectangle rect)
	{
		if (row.AnimationHover)
		{
			g.Fill(Helper.ToColorN(row.AnimationHoverValue, Colour.FillSecondary.Get(ColorScheme, "Table", Name)), rect);
		}
		else if (row.Hover)
		{
			g.Fill(rowHoverBg ?? Colour.FillSecondary.Get(ColorScheme, "Table", Name), rect);
		}
	}

	/// <summary>
	/// 渲染单元格背景
	/// </summary>
	private void PaintItemBgRowStyle(Canvas g, RowTemplate row)
	{
		CELL[] cells = row.cells;
		foreach (CELL cELL in cells)
		{
			if (cELL.COLUMN.Style != null && cELL.COLUMN.Style.BackColor.HasValue)
			{
				g.Fill(cELL.COLUMN.Style.BackColor.Value, cELL.RECT);
			}
			PaintItemBg(g, cELL);
		}
	}

	/// <summary>
	/// 渲染单元格背景
	/// </summary>
	private void PaintItemBgRow(Canvas g, RowTemplate row)
	{
		CELL[] cells = row.cells;
		foreach (CELL it in cells)
		{
			PaintItemBg(g, it);
		}
	}

	/// <summary>
	/// 渲染单元格背景
	/// </summary>
	private void PaintItemBg(Canvas g, CELL it)
	{
		if (!(it is Template template))
		{
			return;
		}
		foreach (ICell item in template.Value)
		{
			item.PaintBack(g);
		}
	}

	/// <summary>
	/// 渲染前景行
	/// </summary>
	private void PaintForeItem(Canvas g, StyleRow row, SolidBrush fore, SolidBrush foreEnable)
	{
		if (selectedIndex.Contains(row.row.INDEX) && rowSelectedFore.HasValue)
		{
			using (SolidBrush solidBrush = new SolidBrush(rowSelectedFore.Value))
			{
				for (int i = 0; i < row.row.cells.Length; i++)
				{
					PaintItem(g, row.row.cells[i], row.row.ENABLE, solidBrush);
				}
				return;
			}
		}
		if (row.style != null && row.style.ForeColor.HasValue)
		{
			using (SolidBrush solidBrush2 = new SolidBrush(row.style.ForeColor.Value))
			{
				for (int j = 0; j < row.row.cells.Length; j++)
				{
					PaintItem(g, row.row.cells[j], row.row.ENABLE, solidBrush2);
				}
				return;
			}
		}
		for (int k = 0; k < row.row.cells.Length; k++)
		{
			PaintItem(g, row.row.cells[k], row.row.ENABLE, row.row.ENABLE ? fore : foreEnable);
		}
	}

	private void PaintItem(Canvas g, CELL it, bool enable, SolidBrush fore)
	{
		if (it.COLUMN.Style == null || !it.COLUMN.Style.ForeColor.HasValue)
		{
			PaintItem(g, it.INDEX, it, enable, fore);
			return;
		}
		using SolidBrush solidBrush = new SolidBrush(it.COLUMN.Style.ForeColor.Value);
		PaintItem(g, it.INDEX, it, enable, solidBrush);
	}

	/// <summary>
	/// 渲染单元格（浮动）
	/// </summary>
	private void PaintItemFixed(Canvas g, CELL it, bool enable, SolidBrush fore, CellStyleInfo? style)
	{
		if (selectedIndex.Contains(it.ROW.INDEX) && rowSelectedFore.HasValue)
		{
			using (SolidBrush solidBrush = new SolidBrush(rowSelectedFore.Value))
			{
				PaintItem(g, it.INDEX, it, enable, solidBrush);
				return;
			}
		}
		if (style != null && style.ForeColor.HasValue)
		{
			using (SolidBrush solidBrush2 = new SolidBrush(style.ForeColor.Value))
			{
				PaintItem(g, it.INDEX, it, enable, solidBrush2);
				return;
			}
		}
		if (it.COLUMN.Style == null || !it.COLUMN.Style.ForeColor.HasValue)
		{
			PaintItem(g, it.INDEX, it, enable, fore);
			return;
		}
		using SolidBrush solidBrush3 = new SolidBrush(it.COLUMN.Style.ForeColor.Value);
		PaintItem(g, it.INDEX, it, enable, solidBrush3);
	}

	private void PaintItem(Canvas g, int columnIndex, CELL it, bool enable, SolidBrush fore)
	{
		GraphicsState graphicsState = g.Save();
		try
		{
			if (OnCellPaintBegin(g, it.RECT, it.RECT_REAL, it.ROW.RECORD, it.ROW.Type, it.ROW.INDEX, columnIndex, it.COLUMN, out SolidBrush CellFore, out Brush CellBack, out Font CellFont))
			{
				if (CellBack != null)
				{
					using (CellBack)
					{
						g.Fill(CellBack, it.RECT);
					}
				}
				using (CellFont)
				{
					using (CellFore)
					{
						PaintItemCore(g, columnIndex, it, enable, CellFont ?? Font, CellFore ?? fore);
					}
				}
			}
			else if (it.ROW.CanExpand)
			{
				PaintItemArrow(g, it, enable, fore);
			}
			OnCellPaint(g, it.RECT, it.RECT_REAL, it.ROW.RECORD, it.ROW.Type, it.ROW.INDEX, columnIndex, it.COLUMN);
		}
		catch
		{
		}
		g.Restore(graphicsState);
	}

	private void PaintItemCore(Canvas g, int columnIndex, CELL it, bool enable, Font font, SolidBrush fore)
	{
		PaintItemFocus(g, it, enable);
		if (it is TCellCheck tCellCheck)
		{
			tCellCheck.Print(g, ColorScheme, font, fore, enable);
		}
		else if (it is TCellRadio tCellRadio)
		{
			tCellRadio.Print(g, ColorScheme, font, fore, enable);
		}
		else if (it is TCellSwitch tCellSwitch)
		{
			tCellSwitch.Print(g, ColorScheme, font, fore, enable);
		}
		else if (it is TCellSelect tCellSelect)
		{
			tCellSelect.Print(g, ColorScheme, font, fore, enable);
		}
		else if (it is TCellSort tCellSort)
		{
			tCellSort.Print(g, ColorScheme, font, fore, enable);
		}
		else if (it is Template template)
		{
			g.SetClip(it.RECT, CombineMode.Intersect);
			foreach (ICell item in template.Value)
			{
				item.Paint(g, font, enable, fore);
			}
		}
		else if (it is TCellText tCellText)
		{
			g.SetClip(it.RECT, CombineMode.Intersect);
			tCellText.Print(g, ColorScheme, font, fore, enable);
		}
		if (dragHeader != null && dragHeader.enable && dragHeader.i == it.COLUMN.INDEX_REAL)
		{
			g.Fill(Colour.FillSecondary.Get(ColorScheme, "Table", Name), it.RECT);
		}
		if (it.ROW.CanExpand)
		{
			PaintItemArrow(g, it, enable, fore);
		}
	}

	private void PaintItemFocus(Canvas g, CELL it, bool enable)
	{
		if (focusedxy == null || it.INDEX != focusedxy[0] || it.ROW.INDEX != focusedxy[1])
		{
			return;
		}
		TableCellFocusedStyle tableCellFocusedStyle = CellFocusedStyle ?? Config.DefaultCellFocusedStyle;
		if (tableCellFocusedStyle == TableCellFocusedStyle.None)
		{
			return;
		}
		GraphicsState graphicsState = g.Save();
		g.SetClip(it.RECT, CombineMode.Intersect);
		switch (tableCellFocusedStyle)
		{
		case TableCellFocusedStyle.Solid:
		{
			g.Fill(CellFocusedBg ?? Colour.BgBase.Get(ColorScheme, "Table", Name), it.RECT);
			using (Pen pen2 = PaintItemFocus(it, dash: false, (int)(Dpi * 2f)))
			{
				g.Draw(pen2, it.RECT);
			}
			break;
		}
		case TableCellFocusedStyle.Dash:
		{
			using (Pen pen = PaintItemFocus(it, dash: true, (int)Dpi))
			{
				g.Draw(pen, it.RECT);
			}
			break;
		}
		}
		g.Restore(graphicsState);
	}

	private Pen PaintItemFocus(CELL it, bool dash, int bor)
	{
		Pen pen = new Pen(CellFocusedBorder ?? Colour.PrimaryActive.Get(ColorScheme, "Table", Name), bor)
		{
			Alignment = PenAlignment.Inset
		};
		if (dash)
		{
			pen.DashStyle = DashStyle.Dash;
		}
		return pen;
	}

	private void PaintItemArrow(Canvas g, CELL it, bool enable, SolidBrush fore)
	{
		switch (TreeArrowStyle)
		{
		case TableTreeStyle.Button:
		{
			using GraphicsPath path = it.ROW.RectExpand.RoundPath(check_radius);
			g.Fill(Colour.BgBase.Get(ColorScheme, "Table", Name), path);
			g.Draw(Colour.BorderColor.Get(ColorScheme, "Table", Name), check_border, path);
			PaintArrow(g, it.ROW, fore, it.ROW.Expand ? 90 : 0);
			break;
		}
		case TableTreeStyle.Arrow:
			PaintArrow(g, it.ROW, fore, it.ROW.Expand ? 90 : 0);
			break;
		default:
			PaintArrowFill(g, it.ROW, fore, it.ROW.Expand ? 90 : 0);
			break;
		}
	}

	private void PaintArrow(Canvas g, RowTemplate item, SolidBrush color, int ArrowProg)
	{
		Rectangle rect = PaintArrow(g, item, ArrowProg, out GraphicsState graphicsState);
		using (Pen pen = new Pen(color, check_border * 2f))
		{
			LineCap startCap = (pen.EndCap = LineCap.Round);
			pen.StartCap = startCap;
			g.DrawLines(pen, rect.TriangleLinesVertical(-1f, 0.6f));
		}
		g.Restore(graphicsState);
	}

	private void PaintArrowFill(Canvas g, RowTemplate item, SolidBrush brush, float ArrowProg)
	{
		g.FillPolygon(brush, PaintArrow(g, item, ArrowProg, out GraphicsState graphicsState).TriangleLinesVertical(-1f, 0.8f));
		g.Restore(graphicsState);
	}

	private Rectangle PaintArrow(Canvas g, RowTemplate item, float ArrowProg, out GraphicsState state)
	{
		state = g.Save();
		int num = item.RectExpand.Width / 2;
		g.TranslateTransform(item.RectExpand.X + num, item.RectExpand.Y + num);
		g.RotateTransform(-90f + ArrowProg);
		return new Rectangle(-num, -num, item.RectExpand.Width, item.RectExpand.Height);
	}

	private void PaintFixedColumnL(Canvas g, Rectangle rect, Rectangle rect_read, RowTemplate[] rows, StyleRow[] shows, SolidBrush fore, SolidBrush foreEnable, SolidBrush forecolumn, Font column_font, int sx, int sy, float radius)
	{
		if (fixedColumnL != null && sx > 0)
		{
			showFixedColumnL = true;
			CELL cELL = shows[shows.Length - 1].row.cells[fixedColumnL[fixedColumnL.Count - 1]];
			Rectangle rectangle = new Rectangle(rect.X, rect_read.Y, cELL.RECT.Right, rect_read.Height);
			rect_fixed_L = rectangle;
			GraphicsPath graphicsPath = null;
			if (_gap.Width > 0)
			{
				int num = (int)((float)_gap.Width * Dpi);
				Rectangle rect2 = new Rectangle(cELL.RECT.Right - num, rectangle.Y, num * 2, rectangle.Height);
				using LinearGradientBrush brush = new LinearGradientBrush(rect2, Colour.FillSecondary.Get(ColorScheme, "Table", Name), Color.Transparent, 0f);
				g.Fill(brush, rect2);
			}
			if (radius > 0f)
			{
				graphicsPath = rectangle.RoundPath(radius, roundTL: true, roundTR: false, roundBR: false, !visibleHeader);
				g.Fill(Colour.BgBase.Get(ColorScheme, "Table", Name), graphicsPath);
				g.SetClip(graphicsPath);
			}
			else
			{
				g.Fill(Colour.BgBase.Get(ColorScheme, "Table", Name), rectangle);
				g.SetClip(rectangle);
			}
			g.TranslateTransform(0f, -sy);
			StyleRow[] array = shows;
			foreach (StyleRow styleRow in array)
			{
				if (styleRow.row.IsColumn)
				{
					PaintTableBgHeader(g, styleRow.row, radius, 0);
					PaintTableHeader(g, styleRow.row, forecolumn, column_font, radius);
				}
				else
				{
					PaintBgRowFront(g, styleRow);
					PaintItemBgRowStyle(g, styleRow.row);
					PaintBg(g, styleRow.row);
				}
			}
			array = shows;
			foreach (StyleRow styleRow2 in array)
			{
				foreach (int item in fixedColumnL)
				{
					if (styleRow2.row.IsOther)
					{
						PaintItemFixed(g, styleRow2.row.cells[item], styleRow2.row.ENABLE, styleRow2.row.ENABLE ? fore : foreEnable, styleRow2.style);
					}
				}
			}
			if (visibleHeader && fixedHeader)
			{
				if (dividers.Length != 0)
				{
					PaintBorder(g, dividers, 1);
				}
				g.ResetTransform();
				PaintTableBgHeader(g, rows[0], radius, 0);
				PaintTableHeader(g, rows[0], forecolumn, column_font, radius);
				if (dividers.Length != 0)
				{
					g.ResetTransform();
					PaintBorder(g, dividers[0]);
				}
			}
			else
			{
				if (dividers.Length != 0)
				{
					PaintBorder(g, dividers);
				}
				g.ResetTransform();
				g.TranslateTransform(0f, (!bordered) ? (-sy) : 0);
			}
			if (dividerHs.Length != 0)
			{
				PaintBorderH(g, dividerHs);
			}
			g.ResetTransform();
			g.ResetClip();
			graphicsPath?.Dispose();
		}
		else
		{
			rect_fixed_L = null;
			showFixedColumnL = false;
		}
	}

	private void PaintFixedColumnR(Canvas g, Rectangle rect, Rectangle rect_read, RowTemplate[] rows, StyleRow[] shows, SolidBrush fore, SolidBrush foreEnable, SolidBrush forecolumn, Font column_font, int sx, int sy, float radius)
	{
		if (fixedColumnR != null)
		{
			try
			{
				StyleRow obj = shows[shows.Length - 1];
				int num = (ScrollBar.ShowY ? ScrollBar.SIZE : 0);
				int num2 = rect_read.Right - num;
				CELL cELL = obj.row.cells[fixedColumnR[fixedColumnR.Count - 1]];
				CELL cELL2 = obj.row.cells[fixedColumnR[0]];
				if (sx + num2 < cELL2.RECT.Right)
				{
					showFixedColumnR = true;
					sFixedR = cELL2.RECT.Right - num2;
					int num3 = cELL2.RECT.Right - cELL.RECT.Left + num;
					Rectangle rectangle = new Rectangle(rect_read.Right - num3, rect_read.Y, num3, rect_read.Height);
					rect_fixed_R = rectangle;
					GraphicsPath graphicsPath = null;
					if (_gap.Width > 0)
					{
						int num4 = (int)((float)_gap.Width * Dpi);
						Rectangle rect2 = new Rectangle(rectangle.X - num4, rectangle.Y, num4 * 2, rectangle.Height);
						using LinearGradientBrush brush = new LinearGradientBrush(rect2, Color.Transparent, Colour.FillSecondary.Get(ColorScheme, "Table", Name), 0f);
						g.Fill(brush, rect2);
					}
					if (radius > 0f)
					{
						graphicsPath = rectangle.RoundPath(radius, roundTL: false, roundTR: true, !visibleHeader, roundBL: false);
						g.Fill(Colour.BgBase.Get(ColorScheme, "Table", Name), graphicsPath);
						g.SetClip(graphicsPath);
					}
					else
					{
						g.Fill(Colour.BgBase.Get(ColorScheme, "Table", Name), rectangle);
						g.SetClip(rectangle);
					}
					StyleRow[] array = shows;
					foreach (StyleRow styleRow in array)
					{
						g.ResetTransform();
						g.TranslateTransform(0f, -sy);
						if (styleRow.row.IsColumn)
						{
							PaintTableBgHeader(g, styleRow.row, radius, sFixedR);
							g.ResetTransform();
							g.TranslateTransform(-sFixedR, -sy);
							PaintTableHeader(g, styleRow.row, forecolumn, column_font, radius);
						}
						else
						{
							PaintBgRowFront(g, styleRow);
							g.ResetTransform();
							g.TranslateTransform(-sFixedR, -sy);
							PaintItemBgRowStyle(g, styleRow.row);
							g.ResetTransform();
							g.TranslateTransform(0f, -sy);
							PaintBg(g, styleRow.row);
						}
					}
					g.ResetTransform();
					g.TranslateTransform(-sFixedR, -sy);
					array = shows;
					foreach (StyleRow styleRow2 in array)
					{
						foreach (int item in fixedColumnR)
						{
							if (styleRow2.row.IsOther)
							{
								PaintItemBg(g, styleRow2.row.cells[item]);
								PaintItemFixed(g, styleRow2.row.cells[item], styleRow2.row.ENABLE, styleRow2.row.ENABLE ? fore : foreEnable, styleRow2.style);
							}
						}
					}
					g.ResetTransform();
					g.TranslateTransform(0f, -sy);
					if (visibleHeader && fixedHeader)
					{
						if (dividers.Length != 0)
						{
							PaintBorder(g, dividers, 1);
						}
						g.ResetTransform();
						PaintTableBgHeader(g, rows[0], radius, sFixedR);
						g.TranslateTransform(-sFixedR, 0f);
						PaintTableHeader(g, rows[0], forecolumn, column_font, radius);
						if (dividers.Length != 0)
						{
							g.ResetTransform();
							PaintBorder(g, dividers[0]);
							g.TranslateTransform(-sFixedR, 0f);
						}
					}
					else
					{
						if (dividers.Length != 0)
						{
							PaintBorder(g, dividers);
						}
						g.ResetTransform();
						g.TranslateTransform(-sFixedR, (!bordered) ? (-sy) : 0);
					}
					if (dividerHs.Length != 0)
					{
						PaintBorderH(g, dividerHs);
					}
					g.ResetTransform();
					g.ResetClip();
					graphicsPath?.Dispose();
				}
				else
				{
					showFixedColumnR = false;
				}
				return;
			}
			catch
			{
				return;
			}
		}
		rect_fixed_R = null;
		showFixedColumnR = false;
	}

	private int PaintFixedSummary(Canvas g, Rectangle rect, Rectangle rect_read, StyleRow[] shows, SolidBrush fore, SolidBrush foreEnable, SolidBrush forecolumn, Font column_font, int sx, int sy, float radius)
	{
		if (ScrollBar.ShowY)
		{
			try
			{
				StyleRow styleRow = shows[shows.Length - 1];
				if (sy + rect_read.Height < styleRow.row.RECT.Bottom)
				{
					int num = (ScrollBar.ShowX ? ScrollBar.SIZE : 0);
					int num2 = styleRow.row.RECT.Bottom - shows[0].row.RECT.Y;
					int num3 = styleRow.row.RECT.Bottom - rect_read.Bottom + num;
					Rectangle rectangle = new Rectangle(rect_read.X, rect_read.Bottom - num2 - num, rect_read.Width, num2 + num);
					rect_fixed_B = rectangle;
					GraphicsPath graphicsPath = null;
					if (_gap.Width > 0)
					{
						int num4 = (int)((float)_gap.Width * Dpi);
						Rectangle rect2 = new Rectangle(rectangle.X, rectangle.Y - num4, rectangle.Width, num4 * 2);
						using LinearGradientBrush brush = new LinearGradientBrush(rect2, Color.Transparent, Colour.FillSecondary.Get(ColorScheme, "Table", Name), 90f);
						g.Fill(brush, rect2);
					}
					if (radius > 0f && !visibleHeader)
					{
						graphicsPath = rectangle.RoundPath(radius, roundTL: false, roundTR: false, roundBR: true, roundBL: true);
						g.Fill(Colour.BgBase.Get(ColorScheme, "Table", Name), graphicsPath);
						g.SetClip(graphicsPath);
					}
					else
					{
						g.Fill(Colour.BgBase.Get(ColorScheme, "Table", Name), rectangle);
						g.SetClip(rectangle);
					}
					g.TranslateTransform(0f, -num3);
					StyleRow[] array = shows;
					foreach (StyleRow row in array)
					{
						PaintBgRowFront(g, row);
					}
					g.ResetTransform();
					g.TranslateTransform(-sx, -num3);
					array = shows;
					foreach (StyleRow styleRow2 in array)
					{
						PaintItemBgRowStyle(g, styleRow2.row);
					}
					g.ResetTransform();
					g.TranslateTransform(0f, -num3);
					g.ResetTransform();
					g.TranslateTransform(-sx, -num3);
					array = shows;
					foreach (StyleRow row2 in array)
					{
						PaintForeItem(g, row2, fore, foreEnable);
					}
					g.ResetTransform();
					g.TranslateTransform(-sx, 0f);
					if (dividerHs.Length != 0)
					{
						PaintBorderH(g, dividerHs);
					}
					g.ResetTransform();
					if (fixedColumnL != null || fixedColumnR != null)
					{
						PaintFixedSummaryL(g, rect, rect_read, shows, fore, foreEnable, forecolumn, column_font, sx, num3, radius);
						PaintFixedSummaryR(g, rect, rect_read, shows, fore, foreEnable, forecolumn, column_font, sx, num3, radius);
					}
					g.ResetClip();
					graphicsPath?.Dispose();
					return num3;
				}
				rect_fixed_B = null;
			}
			catch
			{
			}
		}
		return -1;
	}

	private void PaintFixedSummaryL(Canvas g, Rectangle rect, Rectangle rect_read, StyleRow[] shows, SolidBrush fore, SolidBrush foreEnable, SolidBrush forecolumn, Font column_font, int sx, int sy, float radius)
	{
		if (fixedColumnL == null || sx <= 0)
		{
			return;
		}
		GraphicsState graphicsState = g.Save();
		try
		{
			CELL cELL = shows[shows.Length - 1].row.cells[fixedColumnL[fixedColumnL.Count - 1]];
			Rectangle rect2 = new Rectangle(rect.X, rect.Y, cELL.RECT.Right, rect.Height);
			GraphicsPath graphicsPath = null;
			if (_gap.Width > 0)
			{
				int num = (int)((float)_gap.Width * Dpi);
				Rectangle rect3 = new Rectangle(rect.X + cELL.RECT.Right - num, rect2.Y, num * 2, rect2.Height);
				using LinearGradientBrush brush = new LinearGradientBrush(rect3, Colour.FillSecondary.Get(ColorScheme, "Table", Name), Color.Transparent, 0f);
				g.Fill(brush, rect3);
			}
			if (radius > 0f)
			{
				graphicsPath = rect2.RoundPath(radius, roundTL: true, roundTR: false, roundBR: false, !visibleHeader);
				g.SetClip(graphicsPath, CombineMode.Intersect);
				g.Fill(Colour.BgBase.Get(ColorScheme, "Table", Name), graphicsPath);
			}
			else
			{
				g.SetClip(rect2, CombineMode.Intersect);
				g.Fill(Colour.BgBase.Get(ColorScheme, "Table", Name), rect2);
			}
			g.TranslateTransform(0f, -sy);
			foreach (StyleRow styleRow in shows)
			{
				PaintBgRowFront(g, styleRow);
				PaintItemBgRow(g, styleRow.row);
				foreach (int item in fixedColumnL)
				{
					PaintItemFixed(g, styleRow.row.cells[item], styleRow.row.ENABLE, styleRow.row.ENABLE ? fore : foreEnable, styleRow.style);
				}
			}
			g.ResetTransform();
			if (!fixedHeader)
			{
				g.TranslateTransform(0f, (!bordered) ? (-sy) : 0);
			}
			if (dividerHs.Length != 0)
			{
				PaintBorderH(g, dividerHs);
			}
			graphicsPath?.Dispose();
		}
		catch
		{
		}
		g.Restore(graphicsState);
	}

	private void PaintFixedSummaryR(Canvas g, Rectangle rect, Rectangle rect_read, StyleRow[] shows, SolidBrush fore, SolidBrush foreEnable, SolidBrush forecolumn, Font column_font, int sx, int sy, float radius)
	{
		if (fixedColumnR == null || !ScrollBar.ShowX)
		{
			return;
		}
		GraphicsState graphicsState = g.Save();
		try
		{
			StyleRow obj = shows[shows.Length - 1];
			int num = (ScrollBar.ShowY ? ScrollBar.SIZE : 0);
			int num2 = rect_read.Right - num;
			CELL cELL = obj.row.cells[fixedColumnR[fixedColumnR.Count - 1]];
			CELL cELL2 = obj.row.cells[fixedColumnR[0]];
			if (sx + num2 < cELL2.RECT.Right)
			{
				int num3 = cELL2.RECT.Right - num2;
				int num4 = cELL2.RECT.Right - cELL.RECT.Left + num;
				Rectangle rect2 = new Rectangle(rect_read.Right - num4, rect_read.Y, num4, rect_read.Height);
				GraphicsPath graphicsPath = null;
				if (_gap.Width > 0)
				{
					int num5 = (int)((float)_gap.Width * Dpi);
					Rectangle rect3 = new Rectangle(rect2.X - num5, rect2.Y, num5 * 2, rect2.Height);
					using LinearGradientBrush brush = new LinearGradientBrush(rect3, Color.Transparent, Colour.FillSecondary.Get(ColorScheme, "Table", Name), 0f);
					g.Fill(brush, rect3);
				}
				if (radius > 0f)
				{
					graphicsPath = rect2.RoundPath(radius, roundTL: false, roundTR: true, !visibleHeader, roundBL: false);
					g.SetClip(graphicsPath, CombineMode.Intersect);
					g.Fill(Colour.BgBase.Get(ColorScheme, "Table", Name), graphicsPath);
				}
				else
				{
					g.SetClip(rect2, CombineMode.Intersect);
					g.Fill(Colour.BgBase.Get(ColorScheme, "Table", Name), rect2);
				}
				g.TranslateTransform(0f, -sy);
				StyleRow[] array = shows;
				foreach (StyleRow row in array)
				{
					PaintBgRowFront(g, row);
				}
				g.ResetTransform();
				g.TranslateTransform(-num3, -sy);
				array = shows;
				foreach (StyleRow styleRow in array)
				{
					foreach (int item in fixedColumnR)
					{
						PaintItemBg(g, styleRow.row.cells[item]);
						PaintItemFixed(g, styleRow.row.cells[item], styleRow.row.ENABLE, styleRow.row.ENABLE ? fore : foreEnable, styleRow.style);
					}
				}
				g.ResetTransform();
				g.TranslateTransform(0f, -sy);
				g.ResetTransform();
				g.TranslateTransform(-num3, 0f);
				if (dividerHs.Length != 0)
				{
					PaintBorderH(g, dividerHs);
				}
				graphicsPath?.Dispose();
			}
		}
		catch
		{
		}
		g.Restore(graphicsState);
	}

	private void PaintMergeCells(Canvas g, RowTemplate[] rows, int sx, int sy, SolidBrush fore, SolidBrush foreEnable)
	{
		if (CellRanges == null || CellRanges.Length == 0)
		{
			return;
		}
		GraphicsState graphicsState = g.Save();
		if (visibleHeader && fixedHeader)
		{
			g.SetClip(new Rectangle(rect_read.X, rect_read.Y + rows[0].Height, rect_read.Width, rect_read.Height));
		}
		g.TranslateTransform(-sx, -sy);
		using (SolidBrush bg = new SolidBrush(Colour.BgBase.Get(ColorScheme, "Table", Name)))
		{
			CellRange[] cellRanges = CellRanges;
			foreach (CellRange range in cellRanges)
			{
				PaintMergeCells(g, rows, bg, fore, foreEnable, range);
			}
		}
		g.Restore(graphicsState);
	}

	private void PaintMergeCells(Canvas g, RowTemplate[] rows, SolidBrush bg, SolidBrush fore, SolidBrush foreEnable, CellRange range)
	{
		RowTemplate[] array;
		if (range.FirstRow == range.LastRow)
		{
			array = rows;
			foreach (RowTemplate rowTemplate in array)
			{
				if (rowTemplate.INDEX_REAL == range.FirstRow)
				{
					PaintMergeCells(g, bg, fore, foreEnable, rowTemplate.cells[range.FirstColumn], rowTemplate.cells[range.LastColumn]);
					break;
				}
			}
			return;
		}
		CELL cELL = null;
		CELL cELL2 = null;
		array = rows;
		foreach (RowTemplate rowTemplate2 in array)
		{
			if (rowTemplate2.INDEX_REAL == range.FirstRow)
			{
				cELL = rowTemplate2.cells[range.FirstColumn];
				if (cELL2 != null)
				{
					PaintMergeCells(g, bg, fore, foreEnable, cELL, cELL2);
					break;
				}
			}
			else if (rowTemplate2.INDEX_REAL == range.LastRow)
			{
				cELL2 = rowTemplate2.cells[range.LastColumn];
				if (cELL != null)
				{
					PaintMergeCells(g, bg, fore, foreEnable, cELL, cELL2);
					break;
				}
			}
		}
	}

	private void PaintMergeCells(Canvas g, SolidBrush bg, SolidBrush fore, SolidBrush foreEnable, CELL first, CELL last)
	{
		GraphicsState graphicsState = g.Save();
		Rectangle rect = RectMergeCells(first, last, out var _);
		g.Fill(bg, rect);
		PaintBgSelect(g, first.ROW, rect);
		PaintBgHover(g, first.ROW, rect);
		PaintBorder(g, rect);
		if (first.mergeW != rect.Width)
		{
			first.mergeW = rect.Width;
			int ox = 0;
			if (first.COLUMN.EqualsKeyTree(TreeKey))
			{
				int num = (int)((float)TreeButtonSize * Dpi);
				int num2 = (int)((float)_gapTree * Dpi);
				ox = num2 * first.ROW.Depth + (num2 + num);
			}
			TableGaps gap = new TableGaps(_gap, Dpi);
			Size font_size = g.MeasureString("龍Qq", Font);
			Size size = first.GetSize(g, Font, font_size, rect.Width, gap);
			first.SetSize(g, Font, size, rect, ox, gap);
		}
		PaintItem(g, first, first.ROW.ENABLE, first.ROW.ENABLE ? fore : foreEnable);
		g.Restore(graphicsState);
	}

	private Rectangle RectMergeCells(CELL first, CELL last, out bool fz)
	{
		fz = false;
		if (last.RECT.X >= first.RECT.X && last.RECT.Y >= first.RECT.Y)
		{
			return new Rectangle(first.RECT.X, first.RECT.Y, last.RECT.Right - first.RECT.X, last.RECT.Bottom - first.RECT.Y);
		}
		if (last.RECT.X >= first.RECT.X)
		{
			fz = true;
			return new Rectangle(first.RECT.X, last.RECT.Y, last.RECT.Right - first.RECT.X, first.RECT.Bottom - last.RECT.Y);
		}
		return new Rectangle(last.RECT.X, last.RECT.Y, first.RECT.Right - last.RECT.X, first.RECT.Bottom - last.RECT.Y);
	}

	internal static PointF[] PaintArrow(RectangleF rect)
	{
		float num = rect.Height * 0.15f;
		float num2 = rect.Height * 0.2f;
		float num3 = rect.Height * 0.26f;
		return new PointF[3]
		{
			new PointF(rect.X + num, rect.Y + rect.Height / 2f),
			new PointF(rect.X + rect.Width * 0.4f, rect.Y + (rect.Height - num3)),
			new PointF(rect.X + rect.Width - num2, rect.Y + num2)
		};
	}

	private void PaintBorder(Canvas g, DividerList[] dividers)
	{
		PaintBorder(g, dividers, 0);
	}

	private void PaintBorder(Canvas g, DividerList[] dividers, int s)
	{
		Color color = borderColor ?? Colour.BorderColor.Get(ColorScheme, "Table", Name);
		float num = BorderCellWidth * Dpi;
		switch (BorderRenderMode)
		{
		case TableBorderMode.None:
		{
			int num3 = (int)num;
			int num4 = num3 / 2;
			for (int j = s; j < dividers.Length; j++)
			{
				DividerList dividerList2 = dividers[j];
				g.Fill(color, new Rectangle(dividerList2.b, dividerList2.a - num4, dividerList2.c, num3));
			}
			break;
		}
		case TableBorderMode.Pen:
		{
			using Pen pen = new Pen(color, num);
			for (int k = s; k < dividers.Length; k++)
			{
				DividerList dividerList3 = dividers[k];
				g.DrawLine(pen, dividerList3.b, dividerList3.a, dividerList3.b + dividerList3.c, dividerList3.a);
			}
			break;
		}
		case TableBorderMode.High:
		{
			float num2 = num / 2f;
			for (int i = s; i < dividers.Length; i++)
			{
				DividerList dividerList = dividers[i];
				g.Fill(color, new RectangleF(dividerList.b, (float)dividerList.a - num2, dividerList.c, num));
			}
			break;
		}
		}
	}

	private void PaintBorder(Canvas g, DividerList it)
	{
		Color color = borderColor ?? Colour.BorderColor.Get(ColorScheme, "Table", Name);
		float num = BorderCellWidth * Dpi;
		switch (BorderRenderMode)
		{
		case TableBorderMode.None:
		{
			int num3 = (int)num;
			int num4 = num3 / 2;
			g.Fill(color, new Rectangle(it.b, it.a - num4, it.c, num3));
			break;
		}
		case TableBorderMode.Pen:
		{
			using Pen pen = new Pen(color, num);
			g.DrawLine(pen, it.b, it.a, it.b + it.c, it.a);
			break;
		}
		case TableBorderMode.High:
		{
			float num2 = num / 2f;
			g.Fill(color, new RectangleF(it.b, (float)it.a - num2, it.c, num));
			break;
		}
		}
	}

	private void PaintBorderH(Canvas g, DividerList[] dividers)
	{
		Color color = borderColor ?? Colour.BorderColor.Get(ColorScheme, "Table", Name);
		float num = BorderCellWidth * Dpi;
		switch (BorderRenderMode)
		{
		case TableBorderMode.None:
		{
			int num3 = (int)num;
			int num4 = num3 / 2;
			DividerList[] array = dividers;
			foreach (DividerList dividerList2 in array)
			{
				g.Fill(color, new RectangleF(dividerList2.a - num4, dividerList2.b, num3, dividerList2.c));
			}
			break;
		}
		case TableBorderMode.Pen:
		{
			using Pen pen = new Pen(color, num);
			DividerList[] array = dividers;
			foreach (DividerList dividerList3 in array)
			{
				g.DrawLine(pen, dividerList3.a, dividerList3.b, dividerList3.a, dividerList3.b + dividerList3.c);
			}
			break;
		}
		case TableBorderMode.High:
		{
			float num2 = num / 2f;
			DividerList[] array = dividers;
			foreach (DividerList dividerList in array)
			{
				g.Fill(color, new RectangleF((float)dividerList.a - num2, dividerList.b, num, dividerList.c));
			}
			break;
		}
		}
	}

	private void PaintBorder(Canvas g, Rectangle rect)
	{
		Color color = borderColor ?? Colour.BorderColor.Get(ColorScheme, "Table", Name);
		float num = BorderCellWidth * Dpi;
		switch (BorderRenderMode)
		{
		case TableBorderMode.None:
		{
			int num3 = (int)num;
			int num4 = num3 / 2;
			g.Fill(color, new RectangleF(rect.X, rect.Y - num4, rect.Width, num3));
			g.Fill(color, new RectangleF(rect.X, rect.Bottom - num4, rect.Width, num3));
			g.Fill(color, new RectangleF(rect.X - num4, rect.Y, num3, rect.Height));
			g.Fill(color, new RectangleF(rect.Right - num4, rect.Y, num3, rect.Height));
			break;
		}
		case TableBorderMode.Pen:
		{
			using Pen pen = new Pen(color, num);
			g.Draw(pen, rect);
			break;
		}
		case TableBorderMode.High:
		{
			float num2 = num / 2f;
			g.Fill(color, new RectangleF(rect.X, (float)rect.Y - num2, rect.Width, num));
			g.Fill(color, new RectangleF(rect.X, (float)rect.Bottom - num2, rect.Width, num));
			g.Fill(color, new RectangleF((float)rect.X - num2, rect.Y, num, rect.Height));
			g.Fill(color, new RectangleF((float)rect.Right - num2, rect.Y, num, rect.Height));
			break;
		}
		}
	}

	private void PaintEmpty(Canvas g, Rectangle rect, int offset)
	{
		g.PaintEmpty(rect, Font, fore ?? Colour.Text.GetSymbol(ColorScheme, "emptyFore", "Table", Name), EmptyText, EmptyImage, offset, StringFormat(ColumnAlign.Center));
	}

	public static FormatFlags StringFormat(Column column, bool isColumn)
	{
		if (!isColumn)
		{
			return StringFormat(column);
		}
		return StringFormat(column.ColAlign ?? column.Align, Ellipsis: false, column.ColBreak);
	}

	public static FormatFlags StringFormat(Column column)
	{
		return StringFormat(column.Align, column.Ellipsis, column.LineBreak);
	}

	public static FormatFlags StringFormat(ColumnAlign Align, bool Ellipsis = false, bool LineBreak = false)
	{
		if (Ellipsis & LineBreak)
		{
			return Align switch
			{
				ColumnAlign.Center => FormatFlags.Center | FormatFlags.NoWrapEllipsis, 
				ColumnAlign.Left => FormatFlags.NoWrapEllipsis | FormatFlags.VerticalCenter | FormatFlags.Left, 
				_ => FormatFlags.NoWrapEllipsis | FormatFlags.VerticalCenter | FormatFlags.Right, 
			};
		}
		if (Ellipsis)
		{
			return Align switch
			{
				ColumnAlign.Center => FormatFlags.Center | FormatFlags.EllipsisCharacter, 
				ColumnAlign.Left => FormatFlags.VerticalCenter | FormatFlags.Left | FormatFlags.EllipsisCharacter, 
				_ => FormatFlags.VerticalCenter | FormatFlags.Right | FormatFlags.EllipsisCharacter, 
			};
		}
		if (LineBreak)
		{
			return Align switch
			{
				ColumnAlign.Center => FormatFlags.Center, 
				ColumnAlign.Left => FormatFlags.VerticalCenter | FormatFlags.Left, 
				_ => FormatFlags.VerticalCenter | FormatFlags.Right, 
			};
		}
		return Align switch
		{
			ColumnAlign.Center => FormatFlags.Center | FormatFlags.NoWrap, 
			ColumnAlign.Left => FormatFlags.VerticalCenter | FormatFlags.Left | FormatFlags.NoWrap, 
			_ => FormatFlags.VerticalCenter | FormatFlags.Right | FormatFlags.NoWrap, 
		};
	}

	protected IContextMenuStripItem[] InitSummaryMenu(TSummaryType type, bool selectionMode)
	{
		ContextMenuStripItem menu = GetMenu(TSummaryType.SUM, "总和", "SUM", "<svg viewBox=\"0 0 1024 1024\"><path d=\"M843.776 205.312v-88.064H266.24l339.968 362.496c21.504 22.528 20.992 57.856-0.512 80.384L269.824 906.752h574.464v-53.248c0-32.256 26.624-58.368 58.88-58.368 32.768 0 58.88 26.112 58.88 58.368V965.12c0 32.256-26.624 58.368-58.88 58.368H131.072c-23.552 0-45.056-13.824-54.272-35.84-9.216-21.504-4.608-46.592 11.776-63.488l393.728-406.016L88.064 97.792C71.68 81.408 67.072 56.32 76.8 35.328 86.016 13.824 107.52 0 131.072 0h772.096c32.768 0 58.88 26.112 58.88 58.368v146.944c0 32.256-26.624 58.368-58.88 58.368-32.768 0-59.392-26.112-59.392-58.368z\"></path></svg>");
		ContextMenuStripItem menu2 = GetMenu(TSummaryType.AVG, "平均", "AVG", "<svg viewBox=\"0 0 1024 1024\"><path d=\"M89.6 0m38.4 0l0 0q38.4 0 38.4 38.4l0 947.2q0 38.4-38.4 38.4l0 0q-38.4 0-38.4-38.4l0-947.2q0-38.4 38.4-38.4Z\"></path><path d=\"M428.736 256m83.2 0l0 0q83.2 0 83.2 83.2l0 345.6q0 83.2-83.2 83.2l0 0q-83.2 0-83.2-83.2l0-345.6q0-83.2 83.2-83.2Z\" fill=\"#d81e06\"></path><path d=\"M857.6 0m38.4 0l0 0q38.4 0 38.4 38.4l0 947.2q0 38.4-38.4 38.4l0 0q-38.4 0-38.4-38.4l0-947.2q0-38.4 38.4-38.4Z\"></path></svg>");
		ContextMenuStripItem menu3 = GetMenu(TSummaryType.MIN, "最小", "MIN", "<svg viewBox=\"0 0 1194 1024\"><path d=\"M682.666667 256h170.666666v768H682.666667z\"></path><path d=\"M0 768h170.666667v256H0z\" fill=\"#FF0000\"></path><path d=\"M341.333333 512h170.666667v512H341.333333zM1024 0h170.666667v1024h-170.666667z\"></path></svg>");
		ContextMenuStripItem menu4 = GetMenu(TSummaryType.MAX, "最大", "MAX", "<svg viewBox=\"0 0 1194 1024\"><path d=\"M682.666667 256h170.666666v768H682.666667zM0 768h170.666667v256H0zM341.333333 512h170.666667v512H341.333333z\"></path><path d=\"M1024 0h170.666667v1024h-170.666667z\" fill=\"#FF0000\"></path></svg>");
		ContextMenuStripItem menu5 = GetMenu(TSummaryType.Count, "计数", "COUNT", "<svg viewBox=\"0 0 1024 1024\"><path d=\"M609.792 877.568H166.4l283.136-346.624-282.624-427.52h419.328c84.992 0 157.184 56.832 216.576 169.984h14.336l-30.208-224.768H14.848L353.28 531.456 14.848 966.144h772.608l30.208-225.28h-14.336c-56.832 91.136-121.344 136.704-193.536 136.704z\" fill=\"#13227a\"></path><path d=\"M593.408 439.296c16.384 0 30.72 2.56 43.008 8.192V395.776c-11.776-4.608-30.72-7.168-49.664-7.168-59.904 0-95.232 25.6-95.232 118.272 0 92.16 35.328 118.272 95.232 118.784 17.408 0 39.936-3.072 51.2-8.704v-50.176c-13.824 6.144-28.16 8.192-43.52 8.192-35.328 0-50.176-14.848-50.176-67.584 0-52.736 14.848-68.096 49.152-68.096zM792.064 388.608c-28.16 0-50.176 10.24-60.416 31.744v-27.648h-51.712v228.352h51.712V470.016c5.632-23.552 19.456-31.232 40.96-31.232 25.088 0 31.232 11.776 31.232 42.496v139.264h51.712V464.896c0-50.176-15.872-76.288-63.488-76.288zM1002.496 441.344v-48.128H962.56V338.944h-52.224v53.76h-25.6v48.128h25.6v106.496c0 59.904 16.896 75.776 64.512 75.776 8.704 0 17.408-1.024 25.088-2.048v-47.104c-4.096 1.024-9.216 1.536-15.36 1.536-17.92 0-22.016-6.144-22.016-32.768V441.344h39.936z\"></path></svg>");
		ContextMenuStripItem contextMenuStripItem = new ContextMenuStripItem().SetText("不汇总", "Table.Summary.NONE").SetIcon("<svg viewBox=\"0 0 1024 1024\"><path d=\"M832 460.8l128-128c12.8-12.8 38.4-12.8 51.2 0 12.8 12.8 12.8 38.4 0 51.2l-128 128 128 128c12.8 12.8 12.8 38.4 0 51.2s-38.4 12.8-51.2 0l-128-128-128 128c-12.8 12.8-38.4 12.8-51.2 0s-12.8-38.4 0-51.2l128-128-128-128c-12.8-12.8-12.8-38.4 0-51.2 12.8-12.8 38.4-12.8 51.2 0l128 128z\" fill=\"#d81e06\"></path><path d=\"M736 64H38.4c-12.8 0-19.2 0-25.6 6.4-12.8 12.8-12.8 38.4 0 51.2l38.4 38.4 313.6 313.6c6.4 6.4 6.4 25.6 0 32L64 806.4l-64 64v51.2c0 19.2 19.2 38.4 38.4 38.4h704c19.2 0 32-12.8 32-32v-64c0-19.2-12.8-32-32-32s-38.4 12.8-38.4 32v32H70.4l345.6-345.6c32-32 32-89.6 0-121.6L108.8 128H704v32c0 19.2 12.8 32 32 32s32-12.8 32-32v-64c0-19.2-12.8-32-32-32z\"></path></svg>").SetTag(TSummaryType.None);
		ContextMenuStripItem contextMenuStripItem2 = null;
		if (MultipleRows)
		{
			contextMenuStripItem2 = ((!selectionMode) ? new ContextMenuStripItem().SetText("所有行模式", "Table.Summary.ModeALL").SetIcon(SvgSummaryAllMode).SetID("MODE") : new ContextMenuStripItem().SetText("选择行模式", "Table.Summary.Mode").SetIcon("<svg viewBox=\"0 0 1024 1024\"><path d=\"M921.6 0 102.4 0C46.08 0 0 46.08 0 102.4l0 819.2c0 56.32 46.08 102.4 102.4 102.4l819.2 0c56.32 0 102.4-46.08 102.4-102.4L1024 102.4C1024 46.08 977.92 0 921.6 0zM972.8 921.6c0 28.2368-22.9632 51.2-51.2 51.2L102.4 972.8c-28.2368 0-51.2-22.9632-51.2-51.2L51.2 102.4c0-28.2368 22.9632-51.2 51.2-51.2l819.2 0c28.2368 0 51.2 22.9632 51.2 51.2L972.8 921.6z\"></path><path d=\"M128 422.4l768 0 0 179.2-768 0 0-179.2Z\"></path><path d=\"M128 704l768 0 0 179.2-768 0 0-179.2Z\" fill=\"#d81e06\"></path><path d=\"M128 140.8l768 0 0 179.2-768 0 0-179.2Z\" fill=\"#d81e06\"></path></svg>").SetID("MODE")
				.SetChecked());
		}
		switch (type)
		{
		case TSummaryType.SUM:
			menu.Checked = true;
			break;
		case TSummaryType.AVG:
			menu2.Checked = true;
			break;
		case TSummaryType.MIN:
			menu3.Checked = true;
			break;
		case TSummaryType.MAX:
			menu4.Checked = true;
			break;
		case TSummaryType.Count:
			menu5.Checked = true;
			break;
		case TSummaryType.Custom:
		{
			bool flag = (menu.Enabled = false);
			bool flag3 = (contextMenuStripItem.Enabled = flag);
			bool flag5 = (menu2.Enabled = flag3);
			bool flag7 = (menu3.Enabled = flag5);
			bool enabled = (menu4.Enabled = flag7);
			menu5.Enabled = enabled;
			break;
		}
		default:
			contextMenuStripItem.Checked = true;
			break;
		}
		if (contextMenuStripItem2 != null)
		{
			return new IContextMenuStripItem[9]
			{
				contextMenuStripItem2,
				new ContextMenuStripItemDivider(),
				menu,
				menu2,
				menu3,
				menu4,
				menu5,
				new ContextMenuStripItemDivider(),
				contextMenuStripItem
			};
		}
		return new IContextMenuStripItem[7]
		{
			menu,
			menu2,
			menu3,
			menu4,
			menu5,
			new ContextMenuStripItemDivider(),
			contextMenuStripItem
		};
	}

	private ContextMenuStripItem GetMenu(TSummaryType value, string key, string id, string svg)
	{
		return new ContextMenuStripItem().SetText(key, "Table.Summary." + id).SetSubText(id).SetIcon(svg)
			.SetTag(value);
	}

	private void Summary_RClick(CELLDB cell)
	{
		FMenuStrip?.Dispose();
		Column colSummary = cell.col;
		if (colSummary.SummaryItem == null)
		{
			colSummary.SummaryItem = new SummaryItemOption(TSummaryType.None);
		}
		FMenuStrip = new ContextMenuStrip.Config(this, (IContextMenuStrip item) =>
		{
			FMenuStrip = null;
			if (item.Tag is TSummaryType summaryType)
			{
				colSummary.SummaryItem.SummaryType = summaryType;
				OnUpdateSummaries();
			}
			else if (item.ID == "MODE")
			{
				colSummary.SummaryItem.SelectionMode = !item.Checked;
				OnUpdateSummaries();
			}
		}, InitSummaryMenu(colSummary.SummaryItem.SummaryType, colSummary.SummaryItem.SelectionMode)).SetFont(Font).SetAlign(TAlign.Top).SetRatio()
			.SetClose(() =>
			{
				FMenuStrip = null;
			})
			.open();
	}

	/// <summary>
	/// 更新汇总列的数据（异步）
	/// </summary>
	public void OnUpdateSummaries()
	{
		if (summaryCustomize)
		{
			if (dataTmp == null || dataTmp.rows.Length == 0)
			{
				Summary = null;
			}
			else
			{
				ITask.Run((Action)UpdateSummaries);
			}
		}
	}

	/// <summary>
	/// 更新汇总列的数据
	/// </summary>
	private void UpdateSummaries()
	{
		lock (dataTmp)
		{
			Column[] summaryColumns = SummaryColumns;
			if (summaryColumns == null || summaryColumns.Length == 0)
			{
				return;
			}
			List<AntItem> list = new List<AntItem>(summaryColumns.Length);
			bool flag = MultipleRows && selectedIndex.Length != 0;
			Column[] array = summaryColumns;
			foreach (Column column in array)
			{
				if (column.SummaryItem == null)
				{
					continue;
				}
				SummaryItemOption? summaryItem = column.SummaryItem;
				if (summaryItem != null && summaryItem.SummaryType == TSummaryType.None)
				{
					continue;
				}
				IRow[] array2 = dataTmp.rows;
				if (flag && column.SummaryItem.SelectionMode)
				{
					List<IRow> list2 = new List<IRow>(array2.Length);
					IRow[] array3 = array2;
					foreach (IRow row in array3)
					{
						if (selectedIndex.Contains(row.i + 1))
						{
							list2.Add(row);
						}
					}
					if (list2.Count > 0)
					{
						array2 = list2.ToArray();
					}
				}
				AntItem antItem = new AntItem(column.Key);
				switch (column.SummaryItem?.SummaryType)
				{
				case TSummaryType.Text:
					antItem.value = column.SummaryItem.DisplayText;
					break;
				case TSummaryType.SUM:
				{
					double num5 = 0.0;
					IRow[] array3 = array2;
					for (int j = 0; j < array3.Length; j++)
					{
						object obj2 = array3[j][column.Key];
						if (obj2 != null && double.TryParse(obj2.ToString(), out var result2))
						{
							num5 += result2;
						}
					}
					antItem.value = GetSummaryItemText(num5, column);
					break;
				}
				case TSummaryType.AVG:
				{
					double num9 = 0.0;
					int num10 = 0;
					IRow[] array3 = array2;
					for (int j = 0; j < array3.Length; j++)
					{
						object obj4 = array3[j][column.Key];
						if (obj4 != null && double.TryParse(obj4.ToString(), out var result4))
						{
							num9 += result4;
							num10++;
						}
					}
					double num11 = ((num10 > 0) ? (num9 / (double)num10) : 0.0);
					antItem.value = GetSummaryItemText(num11, column);
					break;
				}
				case TSummaryType.MIN:
				{
					bool flag2 = false;
					double num2 = double.MaxValue;
					string text = string.Empty;
					int num3 = 0;
					IRow[] array3 = array2;
					for (int j = 0; j < array3.Length; j++)
					{
						object obj = array3[j][column.Key];
						double result;
						if (!flag2 && obj is string)
						{
							flag2 = true;
							text = obj?.ToString();
						}
						else if (flag2)
						{
							text = CompareString(obj?.ToString(), text, MAX: false);
						}
						else if (!flag2 && obj != null && double.TryParse(obj.ToString(), out result))
						{
							if (result < num2)
							{
								num2 = result;
							}
							num3++;
						}
					}
					double num4 = ((num3 > 0) ? num2 : 0.0);
					antItem.value = (flag2 ? text : GetSummaryItemText(num4, column));
					break;
				}
				case TSummaryType.MAX:
				{
					bool flag3 = false;
					double num6 = double.MinValue;
					string text2 = string.Empty;
					int num7 = 0;
					IRow[] array3 = array2;
					for (int j = 0; j < array3.Length; j++)
					{
						object obj3 = array3[j][column.Key];
						double result3;
						if (!flag3 && obj3 is string)
						{
							flag3 = true;
							text2 = obj3?.ToString();
						}
						else if (flag3)
						{
							text2 = CompareString(text2, obj3?.ToString(), MAX: true);
						}
						else if (obj3 != null && double.TryParse(obj3.ToString(), out result3))
						{
							if (result3 > num6)
							{
								num6 = result3;
							}
							num7++;
						}
					}
					double num8 = ((num7 > 0) ? num6 : 0.0);
					antItem.value = (flag3 ? text2 : GetSummaryItemText(num8, column));
					break;
				}
				case TSummaryType.Count:
					antItem.value = GetSummaryItemText(array2.Length, column.SummaryItem?.DisplayFormat);
					break;
				default:
				{
					double num = 0.0;
					if (CustomSummaryCalculate != null)
					{
						IRow[] array3 = array2;
						foreach (IRow row2 in array3)
						{
							_ = row2[column.Key];
							TableCustomSummaryEventArgs e = new TableCustomSummaryEventArgs(column, row2.record, row2.i, end: false);
							CustomSummaryCalculate(this, e);
							if (e.TotalValue.HasValue)
							{
								num += e.TotalValue.Value;
							}
						}
						TableCustomSummaryEventArgs e2 = new TableCustomSummaryEventArgs(column, array2, 0, end: true);
						CustomSummaryCalculate(this, e2);
						if (e2.TotalValue.HasValue)
						{
							num += e2.TotalValue.Value;
						}
					}
					antItem.value = GetSummaryItemText(num, column);
					break;
				}
				}
				list.Add(antItem);
			}
			if (list.Count > 0)
			{
				Summary = new List<AntItem[]> { list.ToArray() };
			}
		}
	}

	private string? GetSummaryItemText(object? value, string? format)
	{
		return Column.GetDisplayText(value, string.IsNullOrEmpty(format) ? "0.#" : format);
	}

	private string? GetSummaryItemText(object? value, Column column)
	{
		object? value2 = value;
		string? format;
		if (column.SummaryItem != null && !string.IsNullOrEmpty(column.SummaryItem.DisplayFormat))
		{
			format = column.SummaryItem.DisplayFormat;
		}
		else
		{
			format = (string.IsNullOrEmpty(column.DisplayFormat) ? "0.#" : column.DisplayFormat);
		}
		return Column.GetDisplayText(value2, format);
	}

	private string? CompareString(string? str1, string? str2, bool MAX, StringComparison comparisonType = StringComparison.CurrentCulture)
	{
		int num = ((str1?.Length != str2?.Length) ? ((str1?.Length > str2?.Length) ? 1 : 0) : string.Compare(str1, str2, comparisonType));
		if (!MAX)
		{
			if (num > 0)
			{
				return str2;
			}
			return str1;
		}
		if (num <= 0)
		{
			return str2;
		}
		return str1;
	}
}
