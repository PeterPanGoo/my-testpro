namespace AntdUI;

public class SegmentedItemCollection : iCollection<SegmentedItem>
{
	public SegmentedItemCollection(Segmented it)
	{
		BindData(it);
	}

	internal SegmentedItemCollection BindData(Segmented it)
	{
		action = (bool render) =>
		{
			if (render)
			{
				it.ChangeItems(print: true);
			}
			else
			{
				it.Invalidate();
			}
		};
		return this;
	}
}
