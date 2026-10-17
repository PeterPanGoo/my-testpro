using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class TableClickEventArgs : ITableMouseNullEventArgs
{
	/// <summary>
	/// ±Ì∏Ò«¯”Ú
	/// </summary>
	public Rectangle Rect { get; private set; }

	public TableClickEventArgs(object record, RowType rowType, int rowIndex, int columnIndex, Column? column, Rectangle rect, MouseEventArgs e)
		: base(record, rowType, rowIndex, columnIndex, column, e)
	{
		Rect = rect;
	}
}
