using System;
using System.Collections.Generic;
using Redbox.HAL.Client;

namespace HALUtilities;

internal sealed class PrepositionTest : IDisposable
{
	private readonly HardwareJob m_preposJob;

	internal HardwareJob PreposJob => m_preposJob;

	public void Dispose()
	{
	}

	internal PrepositionTest(int items, HardwareService s, IConsole console)
	{
		List<string> list = new List<string>();
		using RandomInventorySelector randomInventorySelector = new RandomInventorySelector(s);
		for (int i = 0; i < items; i++)
		{
			IInventoryLocation inventoryLocation = randomInventorySelector.Select();
			if (inventoryLocation == null)
			{
				break;
			}
			list.Add(inventoryLocation.Matrix);
		}
		if (!s.PreposVend(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.High
		}, list.ToArray(), out m_preposJob).Success)
		{
			throw new Exception("Can't schedule job.");
		}
		m_preposJob.Pend();
	}
}
