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
		int available = SlotsAvailable();
		if (available == 0)
		{
			return emptySearchResult;
		}
		foreach (ESPNode item in Pattern)
		{
			ILocation location = InventoryService.Get(item.Deck, item.Slot);
			if (location.IsEmpty)
			{
				emptySearchResult.EmptyLocations.Add(location);
				if (emptySearchResult.EmptyLocations.Count == available)
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
		int available = SlotsAvailable();
		if (available == 0)
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
			if (emptySearchResult.EmptyLocations.Count == available)
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
		int available = InventoryService.GetMachineEmptyCount() - returnSlotBuffer;
		if (available <= 0)
		{
			LogHelper.Instance.Log("[EmptySearchService] There are no empty slots available.");
			return 0;
		}
		if (available > ControllerConfiguration.Instance.PutAwayItemAttempts)
		{
			available = ControllerConfiguration.Instance.PutAwayItemAttempts;
		}
		return available;
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

	// The order in which empty slots are searched: quadrant by quadrant across decks 3, 2, 4, 1 and 5, then the remaining
	// decks. Non-VMZ: six quadrants (12 slots on a sparse deck, else 15) on decks 1-5, then all of decks 6 and 7, and deck 8
	// unless it is the QLM deck. VMZ: quadrants 2-6 on decks 1-5, slots 16 and up of decks 6, 8 and 7, then the first
	// quadrant (slots 15 down to 1) of every deck.
	private void ComputeList()
	{
		IDecksService decksService = ServiceLocator.Instance.GetService<IDecksService>();
		int[] deckOrder = new int[5] { 3, 2, 4, 1, 5 };
		int idx = 0;
		IDeck deck = null;
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			for (int quadrant = 0; quadrant < 6; quadrant++)
			{
				foreach (int deckNumber in deckOrder)
				{
					deck = decksService.GetByNumber(deckNumber);
					int slotsPerQuadrant = (deck.IsSparse ? 12 : 15);
					int firstSlot = quadrant * slotsPerQuadrant + 1;
					for (int slot = firstSlot; slot <= firstSlot + slotsPerQuadrant - 1; slot++)
					{
						AddLocation(deck, slot, ref idx);
					}
				}
			}
			deck = decksService.GetByNumber(6);
			for (int slot = 1; slot <= deck.NumberOfSlots; slot++)
			{
				AddLocation(deck, slot, ref idx);
			}
			deck = decksService.GetByNumber(7);
			for (int slot = 1; slot <= deck.NumberOfSlots; slot++)
			{
				AddLocation(deck, slot, ref idx);
			}
			deck = decksService.GetByNumber(8);
			if (!deck.IsQlm)
			{
				for (int slot = 1; slot <= deck.NumberOfSlots; slot++)
				{
					AddLocation(deck, slot, ref idx);
				}
			}
			return;
		}
		for (int quadrant = 1; quadrant < 6; quadrant++)
		{
			foreach (int deckNumber in deckOrder)
			{
				deck = decksService.GetByNumber(deckNumber);
				int firstSlot = quadrant * deck.SlotsPerQuadrant + 1;
				for (int slot = firstSlot; slot <= firstSlot + deck.SlotsPerQuadrant - 1; slot++)
				{
					AddLocation(deck, slot, ref idx);
				}
			}
		}
		foreach (int deckNumber in new int[3] { 6, 8, 7 })
		{
			deck = decksService.GetByNumber(deckNumber);
			for (int slot = 16; slot <= deck.NumberOfSlots; slot++)
			{
				AddLocation(deck, slot, ref idx);
			}
		}
		for (int deckNumber = 1; deckNumber <= 8; deckNumber++)
		{
			deck = decksService.GetByNumber(deckNumber);
			for (int slot = 15; slot >= 1; slot--)
			{
				AddLocation(deck, slot, ref idx);
			}
		}
	}
}
