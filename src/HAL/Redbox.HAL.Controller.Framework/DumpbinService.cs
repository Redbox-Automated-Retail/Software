using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DumpbinService : IDumpbinService
{
	private int m_itemCount;

	private readonly IDataTable<IDumpBinInventoryItem> Table;

	private readonly int Deck;

	private readonly IRange<int> Slots;

	public ILocation PutLocation => ServiceLocator.Instance.GetService<IInventoryService>().Get(Deck, Slots.Start + 1);

	public int Capacity => 60;

	public ILocation RotationLocation => ServiceLocator.Instance.GetService<IInventoryService>().Get(Deck, 1);

	public bool IsFull()
	{
		return CurrentCount() >= Capacity;
	}

	public bool IsBin(ILocation loc)
	{
		if (loc.Deck == Deck && loc.Slot >= Slots.Start)
		{
			return loc.Slot <= Slots.End;
		}
		return false;
	}

	public int CurrentCount()
	{
		return m_itemCount;
	}

	public int RemainingSpace()
	{
		if (!IsFull())
		{
			return Capacity - CurrentCount();
		}
		return 0;
	}

	public bool ClearItems()
	{
		LogHelper.Instance.Log("Reset dumpbin counter: current bin count = {0}", CurrentCount());
		ServiceLocator.Instance.GetService<IRuntimeService>();
		try
		{
			using StreamWriter streamWriter = new StreamWriter(File.Open(Path.Combine(ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().CreateSubpath("Service"), "DumpBinInventory.log"), FileMode.Append, FileAccess.Write, FileShare.Read));
			DumpContents(streamWriter);
			streamWriter.WriteLine();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to log dump bin contents.", e);
		}
		int num = CurrentCount();
		m_itemCount = 0;
		return Table.DeleteAll() == num;
	}

	public void DumpContents(TextWriter log)
	{
		List<IDumpBinInventoryItem> list = Table.LoadEntries();
		using (new DisposeableList<IDumpBinInventoryItem>(list))
		{
			log.WriteLine("-- Dump bin inventory ( {0} total items ) --", list.Count);
			foreach (IDumpBinInventoryItem item in list)
			{
				log.WriteLine("Barcode {0} put in at {1}", item.ID, item.PutTime.ToString());
			}
		}
	}

	public IList<IDumpBinInventoryItem> GetBarcodesInBin()
	{
		return Table.LoadEntries();
	}

	public bool AddBinItem(string matrix)
	{
		ITableTypeFactory service = ServiceLocator.Instance.GetService<ITableTypeFactory>();
		return AddBinItem(service.NewBinItem(matrix, DateTime.Now));
	}

	public bool AddBinItem(IDumpBinInventoryItem item)
	{
		m_itemCount++;
		return Table.Insert(item);
	}

	public void GetState(XmlTextWriter writer)
	{
		List<IDumpBinInventoryItem> list = Table.LoadEntries();
		using (new DisposeableList<IDumpBinInventoryItem>(list))
		{
			foreach (IDumpBinInventoryItem item in list)
			{
				writer.WriteStartElement("bin-item");
				writer.WriteAttributeString("PutTime", item.PutTime.ToString());
				writer.WriteAttributeString("id", item.ID);
				writer.WriteEndElement();
			}
		}
	}

	public void ResetState(XmlDocument xmlDocument, ErrorList errors)
	{
		ITableTypeFactory service = ServiceLocator.Instance.GetService<ITableTypeFactory>();
		ClearItems();
		XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("bin-item");
		if (xmlNodeList == null || xmlNodeList.Count <= 0)
		{
			return;
		}
		List<IDumpBinInventoryItem> list = new List<IDumpBinInventoryItem>();
		using (new DisposeableList<IDumpBinInventoryItem>(list))
		{
			foreach (XmlNode item in xmlNodeList)
			{
				string attributeValue = item.GetAttributeValue("id", "UNKNOWN");
				string attributeValue2 = item.GetAttributeValue<string>("PutTime", null);
				DateTime timestamp = DateTime.Now;
				if (attributeValue2 != null)
				{
					try
					{
						timestamp = DateTime.Parse(attributeValue2);
					}
					catch (Exception)
					{
						LogHelper.Instance.Log(LogEntryType.Error, "[DumpbinService] ResetState: unable to parse date time {0}", attributeValue2);
						timestamp = DateTime.Now;
					}
				}
				list.Add(service.NewBinItem(attributeValue, timestamp));
			}
			if (list.Count > 0)
			{
				Table.Insert(list);
				m_itemCount += list.Count;
			}
		}
	}

	internal DumpbinService()
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		Deck = service.Last.Number;
		Slots = new Range(82, 84);
		IDataTableService service2 = ServiceLocator.Instance.GetService<IDataTableService>();
		Table = service2.GetTable<IDumpBinInventoryItem>();
		m_itemCount = Table.GetRowCount();
		LogHelper.Instance.Log("[DumpbinService] Loaded {0} items.", m_itemCount);
	}
}
