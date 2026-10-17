using System.Windows.Forms;

namespace AntdUI;

public class ITableMouseNullEventArgs : MouseEventArgs
{
	/// <summary>
	/// 原始行
	/// </summary>
	public object? Record { get; private set; }

	/// <summary>
	/// 行类型
	/// </summary>
	public RowType RowType { get; private set; }

	/// <summary>
	/// 行序号
	/// </summary>
	public int RowIndex { get; private set; }

	/// <summary>
	/// 列序号
	/// </summary>
	public int ColumnIndex { get; private set; }

	/// <summary>
	/// 表头
	/// </summary>
	public Column? Column { get; private set; }

	public ITableMouseNullEventArgs(object record, RowType rowType, int rowIndex, int columnIndex, Column? column, MouseEventArgs e)
		: base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
	{
		Record = record;
		RowType = rowType;
		RowIndex = rowIndex;
		ColumnIndex = columnIndex;
		Column = column;
	}

	public ITableMouseNullEventArgs(MouseEventArgs e)
		: base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
	{
		RowIndex = (ColumnIndex = -1);
		RowType = RowType.None;
	}
}
