using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmptySearchPatternService : IInventoryObserver, IEmptySearchPatternService
{
	private class ESPNode
	{
		internal readonly int Deck;

		internal readonly int Slot;

		internal readonly int Index;

		internal ESPNode(int deck, int slot, int idx)
		{
			Deck = deck;
			Slot = slot;
			Index = idx;
		}
	}

	private readonly List<IExcludeEmptySearchLocationObserver> Observers = new List<IExcludeEmptySearchLocationObserver>();

	private readonly List<ESPNode> Pattern = new List<ESPNode>();

	private readonly object Lock = new object();

	private readonly IInventoryService InventoryService;

	public void OnInventoryInitialize()
	{
		LogHelper.Instance.Log("[EmptySearchService] Initialize.");
		Rebuild();
	}

	public void OnInventoryChange()
	{
		LogHelper.Instance.Log("[EmptySearchService] OnInventoryRebuild.");
		Rebuild();
	}

	public void OnInventoryRebuild()
	{
		LogHelper.Instance.Log("[EmptySearchService] OnInventoryRebuild.");
		Rebuild();
	}

	public void DumpESP(bool dumpStore)
	{
		string path = Path.Combine(ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().CreateSubpath("Service"), "ESP.log");
		StreamWriter log = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read));
		try
		{
			string value = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}: *** Dump of ESP list ***";
			log.WriteLine(value);
			Pattern.ForEach(delegate(ESPNode node)
			{
				ILocation location = InventoryService.Get(node.Deck, node.Slot);
				log.WriteLine("{0} Excluded = {1}", location.ToString(), location.Excluded.ToString());
			});
			log.WriteLine();
		}
		finally
		{
			if (log != null)
			{
				((IDisposable)log).Dispose();
			}
		}
	}

	public void AddObserver(IExcludeEmptySearchLocationObserver veto)
	{
		lock (Lock)
		{
			Observers.Add(veto);
		}
	}

	public void RemoveObserver(IExcludeEmptySearchLocationObserver v)
	{
		lock (Lock)
		{
			Observers.Remove(v);
		}
	}

	public IEmptySearchResult FindEmptyLocations()
	{
		EmptySearchResult emptySearchResult = new EmptySearchResult();
		int num = SlotsAvailable();
		if (num == 0)
		{
			return emptySearchResult;
		}
		foreach (ESPNode item in Pattern)
		{
			ILocation location = InventoryService.Get(item.Deck, item.Slot);
			if (location.IsEmpty)
			{
				emptySearchResult.EmptyLocations.Add(location);
				if (emptySearchResult.EmptyLocations.Count == num)
				{
					break;
				}
			}
		}
		return emptySearchResult;
	}

	public IEmptySearchResult FindEmptyOutsideOf(ILocation top)
	{
		EmptySearchResult emptySearchResult = new EmptySearchResult();
		int num = SlotsAvailable();
		if (num == 0)
		{
			return emptySearchResult;
		}
		foreach (ESPNode item in Pattern)
		{
			ILocation location = InventoryService.Get(item.Deck, item.Slot);
			if (top.Equals(location))
			{
				break;
			}
			if (location.IsEmpty)
			{
				emptySearchResult.EmptyLocations.Add(location);
			}
			if (emptySearchResult.EmptyLocations.Count == num)
			{
				break;
			}
		}
		return emptySearchResult;
	}

	internal EmptySearchPatternService(IInventoryService iis)
	{
		InventoryService = iis;
		InventoryService.AddObserver(this);
	}

	private void Rebuild()
	{
		lock (Lock)
		{
			Pattern.Clear();
			ComputeList();
			LogHelper.Instance.Log(" ESP statistics:");
			LogHelper.Instance.Log("  Total entries: {0}", Pattern.Count);
			LogHelper.Instance.Log("  There are {0} total excluded slots.", InventoryService.GetExcludedSlotsCount());
		}
	}

	private int SlotsAvailable()
	{
		if (InventoryService.CheckIntegrity() != ErrorCodes.Success)
		{
			return 0;
		}
		int returnSlotBuffer = ControllerConfiguration.Instance.ReturnSlotBuffer;
		int num = InventoryService.GetMachineEmptyCount() - returnSlotBuffer;
		if (num <= 0)
		{
			LogHelper.Instance.Log("[EmptySearchService] There are no empty slots available.");
			return 0;
		}
		if (num > ControllerConfiguration.Instance.PutAwayItemAttempts)
		{
			num = ControllerConfiguration.Instance.PutAwayItemAttempts;
		}
		return num;
	}

	private void AddLocation(IDeck deck, int slot, ref int idx)
	{
		ILocation location = InventoryService.Get(deck.Number, slot);
		if (location.Excluded)
		{
			LogHelper.Instance.Log(LogEntryType.Debug, "Location {0} is already excluded.", location);
			return;
		}
		foreach (IExcludeEmptySearchLocationObserver observer in Observers)
		{
			if (observer.ShouldExclude(location))
			{
				LogHelper.Instance.Log("[EmptySearchService] Apply exclude policy to {0}", location);
				location.Excluded = true;
				InventoryService.Save(location);
				return;
			}
		}
		Pattern.Add(new ESPNode(deck.Number, slot, idx));
	}

	private void ComputeList()
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		int[] array = new int[5] { 3, 2, 4, 1, 5 };
		int idx = 0;
		IDeck deck = null;
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			for (int i = 0; i < 6; i++)
			{
				int[] array2 = array;
				foreach (int number in array2)
				{
					deck = service.GetByNumber(number);
					int num = (deck.IsSparse ? 12 : 15);
					int num2 = i * num + 1;
					for (int k = num2; k <= num2 + num - 1; k++)
					{
						AddLocation(deck, k, ref idx);
					}
				}
			}
			deck = service.GetByNumber(6);
			for (int l = 1; l <= deck.NumberOfSlots; l++)
			{
				AddLocation(deck, l, ref idx);
			}
			deck = service.GetByNumber(7);
			for (int m = 1; m <= deck.NumberOfSlots; m++)
			{
				AddLocation(deck, m, ref idx);
			}
			deck = service.GetByNumber(8);
			if (!deck.IsQlm)
			{
				for (int n = 1; n <= deck.NumberOfSlots; n++)
				{
					AddLocation(deck, n, ref idx);
				}
			}
			return;
		}
		for (int num3 = 1; num3 < 6; num3++)
		{
			int[] array2 = array;
			foreach (int number2 in array2)
			{
				deck = service.GetByNumber(number2);
				int num4 = num3 * deck.SlotsPerQuadrant + 1;
				for (int num5 = num4; num5 <= num4 + deck.SlotsPerQuadrant - 1; num5++)
				{
					AddLocation(deck, num5, ref idx);
				}
			}
		}
		deck = service.GetByNumber(6);
		for (int num6 = 16; num6 <= deck.NumberOfSlots; num6++)
		{
			AddLocation(deck, num6, ref idx);
		}
		deck = service.GetByNumber(8);
		for (int num7 = 16; num7 <= deck.NumberOfSlots; num7++)
		{
			AddLocation(deck, num7, ref idx);
		}
		deck = service.GetByNumber(7);
		for (int num8 = 16; num8 <= deck.NumberOfSlots; num8++)
		{
			AddLocation(deck, num8, ref idx);
		}
		for (int num9 = 1; num9 <= 8; num9++)
		{
			deck = service.GetByNumber(num9);
			for (int num10 = 15; num10 >= 1; num10--)
			{
				AddLocation(deck, num10, ref idx);
			}
		}
	}
}
