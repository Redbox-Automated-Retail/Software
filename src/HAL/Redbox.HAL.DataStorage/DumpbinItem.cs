using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class DumpbinItem : IDumpBinInventoryItem
{
	public DateTime PutTime { get; private set; }

	public string ID { get; private set; }

	internal DumpbinItem(string id, DateTime dt)
	{
		ID = id;
		PutTime = dt;
	}
}
