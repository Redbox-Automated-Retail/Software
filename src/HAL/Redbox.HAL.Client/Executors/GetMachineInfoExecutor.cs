using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class GetMachineInfoExecutor : JobExecutor
{
	private class DeckConfiguration : IDeckConfig
	{
		public int Number { get; internal set; }

		public int SlotCount { get; internal set; }

		public bool IsQlm { get; internal set; }
	}

	public MachineInformation Info { get; private set; }

	protected override string JobName => "get-machine-info";

	public GetMachineInfoExecutor(HardwareService s)
		: base(s)
	{
		Info = new MachineInformation();
	}

	protected override void DisposeInner()
	{
		Info.Dispose();
	}

	protected override void OnJobCompleted()
	{
		List<DeckConfiguration> deckConfigurations = new List<DeckConfiguration>();
		try
		{
			bool hasQlm = false;
			foreach (ProgramResult result in base.Results)
			{
				string code = result.Code;
				if (!(code == "QlmInfo"))
				{
					if (code == "DeckInfo")
					{
						LocateOrCreate(result.Deck, deckConfigurations).SlotCount = result.Slot;
					}
				}
				else
				{
					hasQlm = true;
					LocateOrCreate(result.Deck, deckConfigurations).IsQlm = true;
				}
			}
			deckConfigurations.Sort((DeckConfiguration x, DeckConfiguration y) => x.Number.CompareTo(y.Number));
			if (!hasQlm)
			{
				Info.Configuration = KioskConfiguration.R717;
			}
			else
			{
				DeckConfiguration deckConfiguration = deckConfigurations[0];
				Info.Configuration = ((deckConfiguration.SlotCount != 90) ? KioskConfiguration.R504 : KioskConfiguration.R630);
			}
			deckConfigurations.ForEach(delegate(DeckConfiguration each)
			{
				Info.DecksConfiguration.Add(each);
			});
		}
		finally
		{
			deckConfigurations.Clear();
		}
	}

	private DeckConfiguration LocateOrCreate(int deck, List<DeckConfiguration> list)
	{
		DeckConfiguration deckConfiguration = list.Find((DeckConfiguration each) => each.Number == deck);
		if (deckConfiguration == null)
		{
			deckConfiguration = new DeckConfiguration
			{
				Number = deck
			};
			list.Add(deckConfiguration);
		}
		return deckConfiguration;
	}
}
