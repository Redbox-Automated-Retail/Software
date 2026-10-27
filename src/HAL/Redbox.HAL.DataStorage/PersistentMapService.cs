using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.DataStorage;

public sealed class PersistentMapService : IPersistentMap, IPersistentMapService
{
	private readonly List<IPersistentOption> Options = new List<IPersistentOption>();

	private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

	private readonly IDataTable<IPersistentOption> Table;

	public IPersistentMap GetMap()
	{
		return this;
	}

	public void Remove(string key)
	{
		using (new WithUpgradeableReadLock(Lock))
		{
			IPersistentOption persistentOption = Options.Find((IPersistentOption each) => each.Key == key);
			if (persistentOption != null)
			{
				using (new WithWriteLock(Lock))
				{
					Table.Delete(persistentOption);
					Options.Remove(persistentOption);
					return;
				}
			}
		}
	}

	public T GetValue<T>(string name, T defVal)
	{
		using (new WithUpgradeableReadLock(Lock))
		{
			IPersistentOption persistentOption = Options.Find((IPersistentOption each) => each.Key == name);
			if (persistentOption == null)
			{
				using (new WithWriteLock(Lock))
				{
					InsertUnderLock(name, defVal);
					return defVal;
				}
			}
			return ConversionHelper.ChangeType<T>(persistentOption.Value);
		}
	}

	public void SetValue<T>(string key, T value)
	{
		using (new WithWriteLock(Lock))
		{
			try
			{
				IPersistentOption persistentOption = Options.Find((IPersistentOption each) => each.Key == key);
				if (persistentOption != null)
				{
					persistentOption.UpdateValue(value.ToString());
					Table.Update(persistentOption);
				}
				else
				{
					InsertUnderLock(key, value);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("[PersistentMapService] SetValue caught an exception.", e);
			}
		}
	}

	public PersistentMapService()
	{
		IDataTableService service = ServiceLocator.Instance.GetService<IDataTableService>();
		Table = service.GetTable<IPersistentOption>();
		Options = Table.LoadEntries();
	}

	private void InsertUnderLock<T>(string key, T val)
	{
		IPersistentOption persistentOption = ServiceLocator.Instance.GetService<ITableTypeFactory>().NewPersistentOption(key, val.ToString());
		Table.Insert(persistentOption);
		Options.Add(persistentOption);
	}
}
