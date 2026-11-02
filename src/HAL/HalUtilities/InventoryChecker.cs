using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities;

internal class InventoryChecker
{
	private class InventoryLocation
	{
		internal int Deck { get; set; }

		internal int Slot { get; set; }

		internal string? ID { get; set; }

		internal string? ReturnTime { get; set; }

		internal int StuckCount { get; set; }

		internal string? Flags { get; set; }

		internal bool IsEquivalent(InventoryLocation loc)
		{
			if (Deck != loc.Deck || Slot != loc.Slot)
			{
				return false;
			}
			if (ID != loc.ID)
			{
				return false;
			}
			if (ReturnTime != loc.ReturnTime)
			{
				return false;
			}
			if (StuckCount == loc.StuckCount)
			{
				return Flags == loc.Flags;
			}
			return false;
		}

		public override string ToString()
		{
			return $"Deck = {Deck}, Slot = {Slot}, ID = {ID} ReturnTime = {ReturnTime} Count = {StuckCount} Flags = {Flags}";
		}
	}

	internal static InventoryChecker Instance => Singleton<InventoryChecker>.Instance;

	internal bool CheckInventory(HardwareService service, string against, TextWriter log)
	{
		if (service == null)
		{
			log.WriteLine("The service is null.");
			return false;
		}
		if (!File.Exists(against))
		{
			log.WriteLine("The comparison file '{0}' doesn't exist.", against);
			return false;
		}
		HardwareCommandResult inventoryState = service.GetInventoryState();
		if (!inventoryState.Success)
		{
			log.WriteLine("Unable to retrieve the inventory state.");
			return false;
		}
		string xml = inventoryState.CommandMessages[0];
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xml);
		List<InventoryLocation> locations = GetLocations(xmlDocument);
		XmlDocument fileDocument = new XmlDocument();
		fileDocument.Load(against);
		List<InventoryLocation> fileLocations = GetLocations(fileDocument);
		if (fileLocations.Count != locations.Count)
		{
			log.WriteLine("The counts don't match.");
			return false;
		}
		int mismatches = 0;
		for (int i = 0; i < fileLocations.Count; i++)
		{
			InventoryLocation fileLocation = fileLocations[i];
			InventoryLocation serviceLocation = locations[i];
			if (!fileLocation.IsEquivalent(serviceLocation))
			{
				log.WriteLine("{0} (service) didn't compare to {1} (inv-file)", serviceLocation, fileLocation);
				mismatches++;
			}
		}
		log.WriteLine("There were {0} comparison errors.", mismatches);
		return mismatches == 0;
	}

	private List<InventoryLocation> GetLocations(XmlDocument document)
	{
		List<InventoryLocation> list = new List<InventoryLocation>();
		foreach (XmlNode item in document.SelectNodes("inventory-items/item")!)
		{
			int deck = Convert.ToInt32(item.Attributes!["deck"]!.Value);
			int slot = Convert.ToInt32(item.Attributes!["slot"]!.Value);
			string value = item.Attributes!["id"]!.Value;
			list.Add(new InventoryLocation
			{
				Deck = deck,
				Slot = slot,
				ID = value
			});
		}
		return list;
	}

	private InventoryChecker()
	{
	}
}
