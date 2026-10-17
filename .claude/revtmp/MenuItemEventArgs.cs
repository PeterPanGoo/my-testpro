using System.Windows.Forms;

namespace AntdUI;

public class MenuItemEventArgs : VMEventArgs<MenuItem>
{
	public MenuItemEventArgs(MenuItem item, MouseEventArgs e)
		: base(item, e)
	{
	}

	public MenuItemEventArgs(MenuItem item, MouseEventArgs e, int click)
		: base(item, e, click)
	{
	}
}
