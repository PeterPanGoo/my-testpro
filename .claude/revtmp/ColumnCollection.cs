using System;
using System.Collections;
using System.Collections.Generic;

namespace AntdUI;

public class ColumnCollection : IEnumerable<Column>, IEnumerable
{
	internal Table? table;

	private List<Column> list;

	public Column this[int index]
	{
		get
		{
			return list[index];
		}
		set
		{
			list[index] = value;
		}
	}

	public Column? this[string key]
	{
		get
		{
			foreach (Column item in list)
			{
				if (item.Key == key)
				{
					return item;
				}
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				return;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Key == key)
				{
					list[i] = value;
					break;
				}
			}
		}
	}

	public int Count => list.Count;

	private void R()
	{
		if (table != null)
		{
			table.ExtractHeaderFixed();
			if (table.LoadLayout())
			{
				table.Invalidate();
			}
		}
	}

	public ColumnCollection()
	{
		list = new List<Column>();
	}

	public ColumnCollection(int count)
	{
		list = new List<Column>(count);
	}

	public ColumnCollection(IEnumerable<Column> collection)
	{
		list = new List<Column>(collection);
	}

	public IEnumerator<Column> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return list.GetEnumerator();
	}

	public Column[] ToArray()
	{
		return list.ToArray();
	}

	public void ForEach(Action<Column> action)
	{
		list.ForEach(action);
	}

	public bool TrueForAll(Predicate<Column> match)
	{
		return list.TrueForAll(match);
	}

	public void Add(Column item)
	{
		list.Add(item);
		R();
	}

	public void AddRange(IEnumerable<Column> collection)
	{
		list.AddRange(collection);
		R();
	}

	public void Insert(int index, Column item)
	{
		list.Insert(index, item);
		R();
	}

	public void InsertRange(int index, IEnumerable<Column> collection)
	{
		list.InsertRange(index, collection);
		R();
	}

	public bool Contains(Column item)
	{
		return list.Contains(item);
	}

	public int IndexOf(Column item)
	{
		return list.IndexOf(item);
	}

	public int IndexOf(Column item, int index)
	{
		return list.IndexOf(item, index);
	}

	public int IndexOf(Column item, int index, int count)
	{
		return list.IndexOf(item, index, count);
	}

	public int LastIndexOf(Column item)
	{
		return list.LastIndexOf(item);
	}

	public int LastIndexOf(Column item, int index)
	{
		return list.LastIndexOf(item, index);
	}

	public int LastIndexOf(Column item, int index, int count)
	{
		return list.LastIndexOf(item, index, count);
	}

	public bool Exists(Predicate<Column> match)
	{
		return list.Exists(match);
	}

	public Column? Find(Predicate<Column> match)
	{
		return list.Find(match);
	}

	public List<Column> FindAll(Predicate<Column> match)
	{
		return list.FindAll(match);
	}

	public int FindIndex(Predicate<Column> match)
	{
		return list.FindIndex(match);
	}

	public int FindIndex(int startIndex, Predicate<Column> match)
	{
		return list.FindIndex(startIndex, match);
	}

	public int FindIndex(int startIndex, int count, Predicate<Column> match)
	{
		return list.FindIndex(startIndex, count, match);
	}

	public Column? FindLast(Predicate<Column> match)
	{
		return list.FindLast(match);
	}

	public int FindLastIndex(Predicate<Column> match)
	{
		return list.FindLastIndex(match);
	}

	public int FindLastIndex(int startIndex, Predicate<Column> match)
	{
		return list.FindLastIndex(startIndex, match);
	}

	public int FindLastIndex(int startIndex, int count, Predicate<Column> match)
	{
		return list.FindLastIndex(startIndex, count, match);
	}

	public List<Column> GetRange(int index, int count)
	{
		return list.GetRange(index, count);
	}

	public void Clear()
	{
		list.Clear();
	}

	public bool Remove(Column item)
	{
		bool result = list.Remove(item);
		R();
		return result;
	}

	public void RemoveAt(int index)
	{
		list.RemoveAt(index);
		R();
	}

	public int RemoveAll(Predicate<Column> match)
	{
		int result = list.RemoveAll(match);
		R();
		return result;
	}

	public void RemoveRange(int index, int count)
	{
		list.RemoveRange(index, count);
		R();
	}

	public void Reverse()
	{
		list.Reverse();
	}

	public void Reverse(int index, int count)
	{
		list.Reverse(index, count);
	}

	public void Sort()
	{
		list.Sort();
	}

	public void Sort(IComparer<Column> comparer)
	{
		list.Sort(comparer);
	}

	public void Sort(Comparison<Column> comparison)
	{
		list.Sort(comparison);
	}

	public void Sort(int index, int count, IComparer<Column> comparer)
	{
		list.Sort(index, count, comparer);
	}

	public void CopyTo(Column[] array)
	{
		list.CopyTo(array);
	}

	public void CopyTo(Column[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public void CopyTo(int index, Column[] array, int arrayIndex, int count)
	{
		list.CopyTo(index, array, arrayIndex, count);
	}

	public int BinarySearch(Column item)
	{
		return list.BinarySearch(item);
	}

	public int BinarySearch(Column item, IComparer<Column> comparer)
	{
		return list.BinarySearch(item, comparer);
	}

	public int BinarySearch(int index, int count, Column item, IComparer<Column> comparer)
	{
		return list.BinarySearch(index, count, item, comparer);
	}
}
