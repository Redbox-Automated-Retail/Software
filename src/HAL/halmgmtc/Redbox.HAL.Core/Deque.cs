using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;

namespace Redbox.HAL.Core;

public sealed class Deque<T>
{
	private Wintellect.PowerCollections.Deque<T> m_impl = new Wintellect.PowerCollections.Deque<T>();

	public int Count => m_impl.Count;

	public T PopTop()
	{
		if (m_impl.Count != 0)
		{
			return m_impl.RemoveFromFront();
		}
		return default(T);
	}

	public T PopBottom()
	{
		if (m_impl.Count != 0)
		{
			return m_impl.RemoveFromBack();
		}
		return default(T);
	}

	public void PushTop(T instance)
	{
		m_impl.AddToFront(instance);
	}

	public void PushBottom(T instance)
	{
		m_impl.AddToBack(instance);
	}

	public void AddManyToFront(IEnumerable<T> toAdd)
	{
		m_impl.AddManyToFront(toAdd);
	}

	public void AddToBack(T item)
	{
		m_impl.AddToBack(item);
	}

	public void Clear()
	{
		m_impl.Clear();
	}

	public ICollection<T> RemoveAll(Predicate<T> predicate)
	{
		return m_impl.RemoveAll(predicate);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_impl.GetEnumerator();
	}
}
