using System;
using System.Collections.Generic;
using Redbox.HAL.Client;

namespace HALUtilities;

internal sealed class RandomInventorySelector : IDisposable
{
	private readonly List<IInventoryLocation> m_locs = new List<IInventoryLocation>();

	public void Dispose()
	{
		m_locs.Clear();
	}

	internal IInventoryLocation Select()
	{
		if (0 >= m_locs.Count)
		{
			return null;
		}
		int index = new Random(DateTime.Now.Millisecond).Next(0, m_locs.Count - 1);
		IInventoryLocation result = m_locs[index];
		m_locs.RemoveAt(index);
		return result;
	}

	internal RandomInventorySelector(HardwareService service)
	{
		using KioskInventory kioskInventory = new KioskInventory(service);
		if (kioskInventory.DeckInventory.Count == 0)
		{
			throw new Exception("Cannot load inventory.");
		}
		kioskInventory.DeckInventory.ForEach(delegate(IInventoryLocation element)
		{
			if (element.Matrix != "EMPTY")
			{
				m_locs.Add(element);
			}
		});
	}
}
