using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework.Services;

internal sealed class PersistentCounterService : IPersistentCounterService
{
	private readonly IDataTable<IPersistentCounter> CounterTable;

	private readonly List<string> WeeklyResettableCounters = new List<string>();

	private readonly Dictionary<string, IPersistentCounter> Counters = new Dictionary<string, IPersistentCounter>();

	public IPersistentCounter Find(string name)
	{
		if (Counters.ContainsKey(name))
		{
			return Counters[name];
		}
		IPersistentCounter persistentCounter = ServiceLocator.Instance.GetService<ITableTypeFactory>().NewCounter(name, 0);
		if (CounterTable.Insert(persistentCounter))
		{
			Counters[name] = persistentCounter;
			return persistentCounter;
		}
		LogHelper.Instance.Log("[PersistentCounterService] Unable to create counter {0}", name);
		return null;
	}

	public IPersistentCounter Find(TimeoutCounters counter)
	{
		return Find(KeyFrom(counter));
	}

	public IPersistentCounter Increment(string name)
	{
		try
		{
			IPersistentCounter persistentCounter = Find(name);
			if (persistentCounter == null)
			{
				return null;
			}
			persistentCounter.Increment();
			return CounterTable.Update(persistentCounter) ? persistentCounter : null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"FindAndIncrementCounter ( name = {name} ) caught an exception.", e);
			return null;
		}
	}

	public IPersistentCounter Increment(TimeoutCounters counter)
	{
		return Increment(KeyFrom(counter));
	}

	public IPersistentCounter Decrement(string name)
	{
		try
		{
			IPersistentCounter persistentCounter = Find(name);
			if (persistentCounter == null)
			{
				return null;
			}
			persistentCounter.Decrement();
			return CounterTable.Update(persistentCounter) ? persistentCounter : null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[PersistentCounterService] Decrement counter ( {name} ) caught an exception.", e);
			return null;
		}
	}

	public IPersistentCounter Decrement(TimeoutCounters counter)
	{
		return Decrement(KeyFrom(counter));
	}

	public bool Reset(IPersistentCounter counter)
	{
		if (counter == null)
		{
			return false;
		}
		if (counter.Value == 0)
		{
			return true;
		}
		counter.Reset();
		return CounterTable.Update(counter);
	}

	public bool Reset(string name)
	{
		IPersistentCounter persistentCounter = Find(name);
		if (persistentCounter == null)
		{
			return false;
		}
		return Reset(persistentCounter);
	}

	public void AddWeeklyResettable(IPersistentCounter counter)
	{
		if (!WeeklyResettableCounters.Contains(counter.Name))
		{
			WeeklyResettableCounters.Add(counter.Name);
		}
	}

	public void ResetWeekly()
	{
		List<IPersistentCounter> list = new List<IPersistentCounter>();
		using (new DisposeableList<IPersistentCounter>(list))
		{
			foreach (string weeklyResettableCounter in WeeklyResettableCounters)
			{
				IPersistentCounter persistentCounter = Find(weeklyResettableCounter);
				if (persistentCounter != null && persistentCounter.Value != 0)
				{
					persistentCounter.Reset();
					list.Add(persistentCounter);
				}
			}
			CounterTable.Update(list);
		}
	}

	internal PersistentCounterService(IDataTableService dts)
	{
		foreach (TimeoutCounters value in Enum.GetValues(typeof(TimeoutCounters)))
		{
			string item = KeyFrom(value);
			if (!WeeklyResettableCounters.Contains(item))
			{
				WeeklyResettableCounters.Add(item);
			}
		}
		CounterTable = dts.GetTable<IPersistentCounter>();
		List<IPersistentCounter> list = CounterTable.LoadEntries();
		using (new DisposeableList<IPersistentCounter>(list))
		{
			foreach (IPersistentCounter item2 in list)
			{
				Counters[item2.Name] = item2;
			}
		}
	}

	private string KeyFrom(TimeoutCounters counter)
	{
		return $"{counter.ToString()}Timeout";
	}
}
