using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Range = Redbox.HAL.Component.Model.Range;

namespace Redbox.HAL.Script.Framework;

internal sealed class VMZ
{
	private readonly Range m_slots = new Range(1, 15);

	internal static VMZ Instance => Singleton<VMZ>.Instance;

	public IRange<int> Bounds => new Range(m_slots.Start, m_slots.End);

	internal ILocation First
	{
		get
		{
			IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
			IDecksService decksService = ServiceLocator.Instance.GetService<IDecksService>();
			return inventoryService.Get(decksService.Last.Number, m_slots.Start);
		}
	}

	internal ILocation Last
	{
		get
		{
			IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
			IDecksService decksService = ServiceLocator.Instance.GetService<IDecksService>();
			return inventoryService.Get(decksService.First.Number, m_slots.End);
		}
	}

	internal int ReserveUnload(int lowDeck)
	{
		int reserved = 0;
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		IDecksService decksService = ServiceLocator.Instance.GetService<IDecksService>();
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			decksService.ForAllDecksDo(delegate(IDeck deck)
			{
				if (deck.Number <= lowDeck)
				{
					reserved += inventoryService.SwapEmptyWith(deck, "UNKNOWN", MerchFlags.Unload, m_slots).Count;
				}
				return true;
			});
			executionTimer.Stop();
			LogHelper.Instance.Log("Reserved unloads through deck {0} ( {1}ms )", lowDeck, executionTimer.ElapsedMilliseconds);
		}
		return reserved;
	}

	internal IteratorComareResult RelativeTo(ILocation location, ILocation reference)
	{
		if (location.Deck < reference.Deck)
		{
			return IteratorComareResult.Above;
		}
		if (location.Deck > reference.Deck)
		{
			return IteratorComareResult.Below;
		}
		if (reference.Slot == location.Slot)
		{
			return IteratorComareResult.Equal;
		}
		if (reference.Slot <= location.Slot)
		{
			return IteratorComareResult.Above;
		}
		return IteratorComareResult.Below;
	}

	internal void ForAllBetweenDo(Predicate<ILocation> callback, ILocation end)
	{
		if (InVMZ(end))
		{
			Execute(callback, First, end);
		}
	}

	internal void ForAllBetweenDo(Predicate<ILocation> callback, ILocation low, ILocation high)
	{
		if (InVMZ(low) && InVMZ(high))
		{
			Execute(callback, low, high);
		}
	}

	internal ILocation FindFirstLocation(MerchFlags flags)
	{
		IDecksService decksService = ServiceLocator.Instance.GetService<IDecksService>();
		IInventoryService inventory = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation found = null;
		decksService.ForAllReverseDecksDo(delegate(IDeck deck)
		{
			for (int i = m_slots.Start; i <= m_slots.End; i++)
			{
				ILocation location = inventory.Get(deck.Number, i);
				if (location.Flags == flags)
				{
					found = location;
					return false;
				}
			}
			return true;
		});
		return found;
	}

	internal ILocation FindHighestLocation(MerchFlags flags)
	{
		IDecksService decksService = ServiceLocator.Instance.GetService<IDecksService>();
		IInventoryService inventory = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation found = null;
		decksService.ForAllDecksDo(delegate(IDeck deck)
		{
			for (int slot = m_slots.End; slot >= m_slots.Start; slot--)
			{
				ILocation location = inventory.Get(deck.Number, slot);
				if (location.Flags == flags)
				{
					found = location;
					return false;
				}
			}
			return true;
		});
		return found;
	}

	internal int ResetCompressedZone(ILocation high)
	{
		int resetCount = 0;
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		List<ILocation> updates = new List<ILocation>();
		using (new DisposeableList<ILocation>(updates))
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			Execute(delegate(ILocation loc)
			{
				if (loc.ID != "EMPTY")
				{
					loc.ID = "EMPTY";
					loc.Flags = MerchFlags.None;
					updates.Add(loc);
				}
				return true;
			}, First, high);
			resetCount = updates.Count;
			inventoryService.Save(updates);
			executionTimer.Stop();
			LogHelper.Instance.Log("VMZ.ClearZone: reset {0} locations ( time = {1}ms ).", resetCount, executionTimer.ElapsedMilliseconds);
			return resetCount;
		}
	}

	internal ILocation ComputeNextHigherLocation(ILocation location)
	{
		int slot = location.Slot + 1;
		int deck = location.Deck;
		if (slot > m_slots.End)
		{
			slot = m_slots.Start;
			deck--;
			IDecksService decksService = ServiceLocator.Instance.GetService<IDecksService>();
			if (deck < decksService.First.Number)
			{
				return null;
			}
		}
		return ServiceLocator.Instance.GetService<IInventoryService>().Get(deck, slot);
	}

	internal void DumpZone(IFormattedLog log)
	{
		if (ControllerConfiguration.Instance.DumpVMZState)
		{
			log.WriteFormatted("-- Dump VMZ Locations --");
			Execute(delegate(ILocation loc)
			{
				log.WriteFormatted("{0} Inventory {1} Flags {2} Stuck {3}", loc.ToString(), loc.ID, loc.Flags.ToString(), loc.StuckCount);
				return true;
			}, First, Last);
		}
	}

	private void Execute(Predicate<ILocation> match, ILocation start, ILocation end)
	{
		ServiceLocator.Instance.GetService<IDecksService>();
		ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation location = start;
		bool keepGoing = true;
		while (keepGoing)
		{
			if (location == end)
			{
				keepGoing = false;
			}
			if (match(location))
			{
				location = ComputeNextHigherLocation(location);
				if (location == null)
				{
					keepGoing = false;
				}
				continue;
			}
			break;
		}
	}

	private bool InVMZ(ILocation location)
	{
		if (location != null && location.Slot >= m_slots.Start)
		{
			return location.Slot <= m_slots.End;
		}
		return false;
	}

	private VMZ()
	{
	}
}
