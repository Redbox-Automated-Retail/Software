using System.Collections.Generic;

namespace Redbox.HAL.Client.Executors;

public sealed class ExcludedLocsJob(HardwareService service) : JobExecutor(service)
{
	private readonly List<Location> m_excluded = new List<Location>();

	public IList<Location> ExcludedLocations
	{
		get
		{
			foreach (ProgramResult result in base.Results)
			{
				if (result.Code == "ExcludedSlot")
				{
					m_excluded.Add(new Location
					{
						Deck = result.Deck,
						Slot = result.Slot
					});
				}
			}
			return m_excluded;
		}
	}

	protected override string JobName => "get-excluded-locations";

	protected override void DisposeInner()
	{
		m_excluded.Clear();
	}
}
