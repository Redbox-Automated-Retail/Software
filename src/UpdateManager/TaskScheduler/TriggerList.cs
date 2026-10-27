using System;
using System.Collections;
using TaskSchedulerInterop;

namespace TaskScheduler;

public class TriggerList : IList, ICollection, IEnumerable, IDisposable
{
	private class Enumerator : IEnumerator
	{
		private TriggerList outer;

		private int currentIndex;

		public object Current => outer.oTriggers[currentIndex];

		internal Enumerator(TriggerList outer)
		{
			this.outer = outer;
			Reset();
		}

		public bool MoveNext()
		{
			return ++currentIndex < outer.oTriggers.Count;
		}

		public void Reset()
		{
			currentIndex = -1;
		}
	}

	private ITask iTask;

	private ArrayList oTriggers;

	public bool IsReadOnly => false;

	public Trigger this[int index]
	{
		get
		{
			if (index >= Count)
			{
				throw new ArgumentOutOfRangeException("index", index, "TriggerList collection");
			}
			return (Trigger)oTriggers[index];
		}
		set
		{
			if (index >= Count)
			{
				throw new ArgumentOutOfRangeException("index", index, "TriggerList collection");
			}
			Trigger trigger = (Trigger)oTriggers[index];
			value.Bind(trigger);
			oTriggers[index] = value;
		}
	}

	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = value as Trigger;
		}
	}

	public bool IsFixedSize => false;

	public int Count => oTriggers.Count;

	public bool IsSynchronized => false;

	public object SyncRoot => null;

	internal TriggerList(ITask iTask)
	{
		this.iTask = iTask;
		ushort Count = 0;
		iTask.GetTriggerCount(out Count);
		oTriggers = new ArrayList(Count + 5);
		for (int i = 0; i < Count; i++)
		{
			iTask.GetTrigger((ushort)i, out var Trigger);
			oTriggers.Add(TaskScheduler.Trigger.CreateTrigger(Trigger));
		}
	}

	public void RemoveAt(int index)
	{
		if (index >= Count)
		{
			throw new ArgumentOutOfRangeException("index", index, "Failed to remove Trigger. Index out of range.");
		}
		((Trigger)oTriggers[index]).Unbind();
		oTriggers.RemoveAt(index);
		iTask.DeleteTrigger((ushort)index);
	}

	void IList.Insert(int index, object value)
	{
		throw new NotImplementedException("TriggerList does not support Insert().");
	}

	public void Remove(Trigger trigger)
	{
		int num = IndexOf(trigger);
		if (num != -1)
		{
			RemoveAt(num);
		}
	}

	void IList.Remove(object value)
	{
		Remove(value as Trigger);
	}

	public bool Contains(Trigger trigger)
	{
		return IndexOf(trigger) != -1;
	}

	bool IList.Contains(object value)
	{
		return Contains(value as Trigger);
	}

	public void Clear()
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			RemoveAt(num);
		}
	}

	public int IndexOf(Trigger trigger)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i].Equals(trigger))
			{
				return i;
			}
		}
		return -1;
	}

	int IList.IndexOf(object value)
	{
		return IndexOf(value as Trigger);
	}

	public int Add(Trigger trigger)
	{
		if (trigger.Bound)
		{
			throw new ArgumentException("A Trigger cannot be added if it is already in a list.");
		}
		iTask.CreateTrigger(out var NewTriggerIndex, out var Trigger);
		trigger.Bind(Trigger);
		int num = oTriggers.Add(trigger);
		if (num != NewTriggerIndex)
		{
			throw new ApplicationException("Assertion Failure");
		}
		return NewTriggerIndex;
	}

	int IList.Add(object value)
	{
		return Add(value as Trigger);
	}

	public void CopyTo(Array array, int index)
	{
		if (oTriggers.Count > array.Length - index)
		{
			throw new ArgumentException("Array has insufficient space to copy the collection.");
		}
		for (int i = 0; i < oTriggers.Count; i++)
		{
			array.SetValue(((Trigger)oTriggers[i]).Clone(), index + i);
		}
	}

	public IEnumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public void Dispose()
	{
		foreach (object oTrigger in oTriggers)
		{
			((Trigger)oTrigger).Unbind();
		}
		oTriggers = null;
		iTask = null;
	}
}
