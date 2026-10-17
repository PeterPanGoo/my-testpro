using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace AntdUI;

public class iCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection
{
	public Action<bool>? action;

	public Action<T>? action_add;

	public Action<object>? action_del;

	private List<T> list;

	public T this[int index]
	{
		get
		{
			return list[index];
		}
		set
		{
			if (action_add == null && action_del == null)
			{
				list[index] = value;
			}
			else
			{
				action_del?.Invoke(new object[2]
				{
					list[index],
					index
				});
				list[index] = value;
				action_add?.Invoke(value);
			}
			PropertyChanged(value);
		}
	}

	object? IList.this[int index]
	{
		get
		{
			return list[index];
		}
		set
		{
			if (value is T val)
			{
				if (action_add == null && action_del == null)
				{
					list[index] = val;
				}
				else
				{
					action_del?.Invoke(new object[2]
					{
						list[index],
						index
					});
					list[index] = val;
					action_add?.Invoke(val);
				}
				PropertyChanged(val);
			}
		}
	}

	public T[] Safe => list.ToArray();

	public int Count => list.Count;

	public bool IsReadOnly => false;

	public bool IsFixedSize => false;

	public bool IsSynchronized => true;

	public object SyncRoot => this;

	private void PropertyChanged(T value)
	{
		if (value is NotifyProperty notifyProperty)
		{
			notifyProperty.PropertyChanged += (object a, PropertyChangedEventArgs b) =>
			{
				action?.Invoke(obj: false);
			};
		}
	}

	public iCollection()
	{
		list = new List<T>();
	}

	public iCollection(int capacity)
	{
		list = new List<T>(capacity);
	}

	public iCollection(IEnumerable<T> collection)
	{
		list = new List<T>(collection);
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < list.Count; i++)
		{
			yield return list[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		for (int i = 0; i < list.Count; i++)
		{
			yield return list[i];
		}
	}

	public T[] ToArray()
	{
		return list.ToArray();
	}

	public void ForEach(Action<T> action)
	{
		list.ForEach(action);
	}

	public bool TrueForAll(Predicate<T> match)
	{
		return list.TrueForAll(match);
	}

	public virtual void Add(T item)
	{
		list.Add(item);
		PropertyChanged(item);
		action_add?.Invoke(item);
		action?.Invoke(obj: true);
	}

	public virtual int Add(object? value)
	{
		if (value is T val)
		{
			list.Add(val);
			PropertyChanged(val);
			action_add?.Invoke(val);
			action?.Invoke(obj: true);
		}
		return list.Count;
	}

	public void AddRange(IEnumerable<T> collection)
	{
		int count = list.Count;
		list.AddRange(collection);
		if (list.Count == count)
		{
			return;
		}
		foreach (T item in collection)
		{
			PropertyChanged(item);
			action_add?.Invoke(item);
		}
		action?.Invoke(obj: true);
	}

	public void Insert(int index, T item)
	{
		list.Insert(index, item);
		PropertyChanged(item);
		action_add?.Invoke(item);
		action?.Invoke(obj: true);
	}

	public void InsertAntRemove(int index, T item)
	{
		list.Remove(item);
		list.Insert(index, item);
		PropertyChanged(item);
		action?.Invoke(obj: true);
	}

	public void Insert(int index, object? value)
	{
		if (value is T val)
		{
			list.Insert(index, val);
			action_add?.Invoke(val);
		}
	}

	public void InsertRange(int index, IEnumerable<T> collection)
	{
		int count = list.Count;
		list.InsertRange(index, collection);
		if (list.Count == count)
		{
			return;
		}
		foreach (T item in collection)
		{
			PropertyChanged(item);
			action_add?.Invoke(item);
		}
		action?.Invoke(obj: true);
	}

	public bool Contains(T item)
	{
		return list.Contains(item);
	}

	public bool Contains(object? value)
	{
		if (value is T item)
		{
			return list.Contains(item);
		}
		return false;
	}

	public int IndexOf(object? value)
	{
		if (value is T item)
		{
			return list.IndexOf(item);
		}
		return -1;
	}

	public int IndexOf(T item)
	{
		return list.IndexOf(item);
	}

	public int IndexOf(T item, int index)
	{
		return list.IndexOf(item, index);
	}

	public int IndexOf(T item, int index, int count)
	{
		return list.IndexOf(item, index, count);
	}

	public int LastIndexOf(T item)
	{
		return list.LastIndexOf(item);
	}

	public int LastIndexOf(T item, int index)
	{
		return list.LastIndexOf(item, index);
	}

	public int LastIndexOf(T item, int index, int count)
	{
		return list.LastIndexOf(item, index, count);
	}

	public bool Exists(Predicate<T> match)
	{
		return list.Exists(match);
	}

	public T? Find(Predicate<T> match)
	{
		return list.Find(match);
	}

	public List<T> FindAll(Predicate<T> match)
	{
		return list.FindAll(match);
	}

	public int FindIndex(Predicate<T> match)
	{
		return list.FindIndex(match);
	}

	public int FindIndex(int startIndex, Predicate<T> match)
	{
		return list.FindIndex(startIndex, match);
	}

	public int FindIndex(int startIndex, int count, Predicate<T> match)
	{
		return list.FindIndex(startIndex, count, match);
	}

	public T? FindLast(Predicate<T> match)
	{
		return list.FindLast(match);
	}

	public int FindLastIndex(Predicate<T> match)
	{
		return list.FindLastIndex(match);
	}

	public int FindLastIndex(int startIndex, Predicate<T> match)
	{
		return list.FindLastIndex(startIndex, match);
	}

	public int FindLastIndex(int startIndex, int count, Predicate<T> match)
	{
		return list.FindLastIndex(startIndex, count, match);
	}

	public List<T> GetRange(int index, int count)
	{
		return list.GetRange(index, count);
	}

	public void Dispose()
	{
		list.Clear();
	}

	public void Clear()
	{
		action_del?.Invoke(-1);
		list.Clear();
		action?.Invoke(obj: true);
	}

	public void Remove(object? value)
	{
		if (value is T val)
		{
			int num = IndexOf(val);
			list.Remove(val);
			action_del?.Invoke(new object[2] { val, num });
			action?.Invoke(obj: true);
		}
	}

	public bool Remove(T item)
	{
		int num = IndexOf(item);
		bool flag = list.Remove(item);
		if (flag)
		{
			action_del?.Invoke(new object[2] { item, num });
			action?.Invoke(obj: true);
		}
		return flag;
	}

	public void RemoveAt(int index)
	{
		if (action_del != null)
		{
			try
			{
				action_del?.Invoke(new object[2]
				{
					list[index],
					index
				});
			}
			catch
			{
			}
		}
		list.RemoveAt(index);
		action?.Invoke(obj: true);
	}

	public List<T> RemoveAll(Predicate<T> match)
	{
		List<T> list = this.list.FindAll(match);
		if (list.Count == 0)
		{
			return list;
		}
		this.list.RemoveAll(match);
		action_del?.Invoke(list);
		action?.Invoke(obj: true);
		return list;
	}

	public void RemoveRange(int index, int count)
	{
		List<T> range = list.GetRange(index, count);
		list.RemoveRange(index, count);
		action_del?.Invoke(range);
		action?.Invoke(obj: true);
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

	public void Sort(IComparer<T> comparer)
	{
		list.Sort(comparer);
	}

	public void Sort(Comparison<T> comparison)
	{
		list.Sort(comparison);
	}

	public void Sort(int index, int count, IComparer<T> comparer)
	{
		list.Sort(index, count, comparer);
	}

	public void CopyTo(T[] array)
	{
		list.CopyTo(array);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public void CopyTo(int index, T[] array, int arrayIndex, int count)
	{
		list.CopyTo(index, array, arrayIndex, count);
	}

	public int BinarySearch(T item)
	{
		return list.BinarySearch(item);
	}

	public int BinarySearch(T item, IComparer<T> comparer)
	{
		return list.BinarySearch(item, comparer);
	}

	public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
	{
		return list.BinarySearch(index, count, item, comparer);
	}

	public void CopyTo(Array array, int index)
	{
	}
}
