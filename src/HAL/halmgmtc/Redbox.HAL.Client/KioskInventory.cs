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
		XmlNodeList nodes = xmlDocument.GetElementsByTagName("item");
		for (int i = 0; i < nodes.Count; i++)
		{
			string id = nodes[i].Attributes["id"].Value;
			string returnTimeText = nodes[i].Attributes["ReturnTime"].Value;
			DateTime? returnTime = null;
			if (returnTimeText != "NONE")
			{
				returnTime = DateTime.Parse(returnTimeText);
			}
			string excluded = nodes[i].Attributes["excluded"].Value;
			string emptyStuckCount = nodes[i].Attributes["emptyStuckCount"].Value;
			string merchFlags = nodes[i].Attributes["merchFlags"].Value;
			InventoryLocation item = new InventoryLocation(Convert.ToInt32(nodes[i].Attributes["deck"].Value), Convert.ToInt32(nodes[i].Attributes["slot"].Value), id, Convert.ToInt32(emptyStuckCount), returnTime, merchFlags, Convert.ToBoolean(excluded));
			DeckInfo.Add(item);
		}
		DeckInfo.Sort(new InventoryLocationSorter());
		nodes = xmlDocument.GetElementsByTagName("bin-item");
		for (int j = 0; j < nodes.Count; j++)
		{
			DateTime putTime = DateTime.Parse(nodes[j].Attributes["PutTime"].Value);
			DumpbinInfo.Add(new DumpbinItem(nodes[j].Attributes["id"].Value, putTime));
		}
	}
}
