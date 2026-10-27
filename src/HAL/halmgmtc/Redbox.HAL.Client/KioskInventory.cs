using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Redbox.HAL.Client;

public sealed class KioskInventory : IDisposable
{
	private class InventoryLocationSorter : IComparer<IInventoryLocation>
	{
		public int Compare(IInventoryLocation x, IInventoryLocation y)
		{
			if (x.Location.Deck > y.Location.Deck)
			{
				return 1;
			}
			if (x.Location.Deck < y.Location.Deck)
			{
				return -1;
			}
			if (x.Location.Slot == y.Location.Slot)
			{
				return 0;
			}
			if (x.Location.Slot <= y.Location.Slot)
			{
				return -1;
			}
			return 1;
		}
	}

	private bool Disposed;

	private readonly HardwareService Service;

	private readonly List<IDumpbinItem> DumpbinInfo = new List<IDumpbinItem>();

	private readonly List<IInventoryLocation> DeckInfo = new List<IInventoryLocation>();

	public IList<IInventoryLocation> DeckInventory => DeckInfo;

	public IList<IDumpbinItem> DumpbinItems => DumpbinInfo;

	public void Dispose()
	{
		Dispose(fromDispose: true);
	}

	~KioskInventory()
	{
		Dispose(fromDispose: false);
	}

	public KioskInventory(HardwareService service)
	{
		Service = service;
		Load();
	}

	private void Dispose(bool fromDispose)
	{
		if (!Disposed)
		{
			Disposed = true;
			if (fromDispose)
			{
				DeckInfo.Clear();
				DumpbinInfo.Clear();
			}
		}
	}

	private void Load()
	{
		HardwareCommandResult inventoryState = Service.GetInventoryState();
		if (!inventoryState.Success)
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		using StringReader txtReader = new StringReader(inventoryState.CommandMessages[0]);
		xmlDocument.Load(txtReader);
		if (xmlDocument.DocumentElement == null)
		{
			return;
		}
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("item");
		for (int i = 0; i < elementsByTagName.Count; i++)
		{
			string value = elementsByTagName[i].Attributes["id"].Value;
			string value2 = elementsByTagName[i].Attributes["ReturnTime"].Value;
			DateTime? returnTime = null;
			if (value2 != "NONE")
			{
				returnTime = DateTime.Parse(value2);
			}
			string value3 = elementsByTagName[i].Attributes["excluded"].Value;
			string value4 = elementsByTagName[i].Attributes["emptyStuckCount"].Value;
			string value5 = elementsByTagName[i].Attributes["merchFlags"].Value;
			InventoryLocation item = new InventoryLocation(Convert.ToInt32(elementsByTagName[i].Attributes["deck"].Value), Convert.ToInt32(elementsByTagName[i].Attributes["slot"].Value), value, Convert.ToInt32(value4), returnTime, value5, Convert.ToBoolean(value3));
			DeckInfo.Add(item);
		}
		DeckInfo.Sort(new InventoryLocationSorter());
		elementsByTagName = xmlDocument.GetElementsByTagName("bin-item");
		for (int j = 0; j < elementsByTagName.Count; j++)
		{
			DateTime putTime = DateTime.Parse(elementsByTagName[j].Attributes["PutTime"].Value);
			DumpbinInfo.Add(new DumpbinItem(elementsByTagName[j].Attributes["id"].Value, putTime));
		}
	}
}
