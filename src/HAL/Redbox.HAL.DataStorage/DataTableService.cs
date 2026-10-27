using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

public sealed class DataTableService : IDataTableService
{
	private readonly Dictionary<Type, object> TypeMap = new Dictionary<Type, object>();

	private readonly Dictionary<Type, object> LegacyTypeMap = new Dictionary<Type, object>();

	public IDataTable<T> GetTable<T>()
	{
		Type typeFromHandle = typeof(T);
		if (!TypeMap.ContainsKey(typeFromHandle))
		{
			return null;
		}
		return (IDataTable<T>)TypeMap[typeFromHandle];
	}

	public IDataTable<T> GetLegacyTable<T>()
	{
		Type typeFromHandle = typeof(T);
		if (!LegacyTypeMap.ContainsKey(typeFromHandle))
		{
			return null;
		}
		return (IDataTable<T>)LegacyTypeMap[typeFromHandle];
	}

	public bool Add<T>(IDataTable<T> table)
	{
		Type typeFromHandle = typeof(T);
		if (!TypeMap.ContainsKey(typeFromHandle))
		{
			TypeMap[typeFromHandle] = table;
			return true;
		}
		return false;
	}

	public DataTableService(bool exclusive)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		DataDescriptor dataDescriptor = new DataDescriptor(service.DataPath);
		CountersDescriptor d = new CountersDescriptor(service.DataPath, exclusive);
		TypeMap.Add(typeof(IPersistentCounter), new PersistentCounterTable(d));
		TypeMap.Add(typeof(ILocation), new InventoryTable(dataDescriptor));
		TypeMap.Add(typeof(IDumpBinInventoryItem), new DumpbinTable(dataDescriptor));
		TypeMap.Add(typeof(IHardwareCorrectionStatistic), new HardwareCorrectionStatsTable(d));
		TypeMap.Add(typeof(IKioskFunctionCheckData), new KioskFunctionCheckTable(d));
		TypeMap.Add(typeof(IPersistentOption), new SecretOptionsTable(d));
		LegacyTypeMap.Add(typeof(IDumpBinInventoryItem), new LegacyDumpbinTable(d));
		LegacyTypeMap.Add(typeof(IHardwareCorrectionStatistic), new LegacyHardwareCorrectionStatsTable(d));
	}
}
