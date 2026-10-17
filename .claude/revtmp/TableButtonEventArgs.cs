using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class TableButtonEventArgs : TableClickEventArgs
{
	/// <summary>
	/// ´¥·¢°´Å¥
	/// </summary>
	public CellLink Btn { get; private set; }

	public TableButtonEventArgs(CellLink btn, object record, RowType rowType, int rowIndex, int columnIndex, Column column, Rectangle rect, MouseEventArgs e)
		: base(record, rowType, rowIndex, columnIndex, column, rect, e)
	{
		Btn = btn;
	}
}
