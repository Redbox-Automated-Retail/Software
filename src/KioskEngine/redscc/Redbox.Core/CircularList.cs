using System;
using System.Collections.Generic;

namespace Redbox.Core;

public class CircularList<T>
{
	private List<T> m_items;

	public int Count => Items.Count;

	public T this[int index]
	{
		get
		{
			return Items[index];
		}
		set
		{
			Items[index] = value;
		}
	}

	internal List<T> Items
	{
		get
		{
			if (m_items == null)
			{
				m_items = new List<T>();
			}
			return m_items;
		}
	}

	internal int CurrentIndex { get; set; }

	public CircularList()
	{
		Reset();
	}

	public CircularList(IEnumerable<T> items)
	{
		AddRange(items);
		Reset();
	}

	public void AddRange(IEnumerable<T> items)
	{
		Items.AddRange(items);
		Reset();
	}

	public void Add(T item)
	{
		Items.Add(item);
		Reset();
	}

	public void Remove(T item)
	{
		Items.Remove(item);
		Reset();
	}

	public void RemoveAt(int index)
	{
		Items.RemoveAt(index);
		Reset();
	}

	public void Insert(int index, T item)
	{
		Items.Insert(index, item);
		Reset();
	}

	public void Clear()
	{
		Items.Clear();
		Reset();
	}

	public void Reset()
	{
		CurrentIndex = -1;
	}

	public T Next()
	{
		if (Items.Count == 0)
		{
			return default(T);
		}
		CurrentIndex++;
		if (CurrentIndex >= Items.Count)
		{
			CurrentIndex = 0;
		}
		return Items[CurrentIndex];
	}

	public T Find(Predicate<T> match)
	{
		return Items.Find(match);
	}
}
