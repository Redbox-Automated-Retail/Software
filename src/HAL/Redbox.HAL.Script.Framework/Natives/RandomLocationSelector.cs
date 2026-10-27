using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

internal sealed class RandomLocationSelector
{
	private readonly List<ILocation> Locations = new List<ILocation>();

	internal ILocation SelectTargetLocation()
	{
		if (0 >= Locations.Count)
		{
			return null;
		}
		int index = new Random(DateTime.Now.Millisecond).Next(0, Locations.Count - 1);
		ILocation result = Locations[index];
		Locations.RemoveAt(index);
		return result;
	}

	internal RandomLocationSelector(IDecksService ds, IInventoryService iis)
	{
		RandomLocationSelector randomLocationSelector = this;
		ds.ForAllDecksDo(delegate(IDeck _deck)
		{
			for (int i = 1; i <= _deck.NumberOfSlots; i++)
			{
				randomLocationSelector.Locations.Add(iis.Get(_deck.Number, i));
			}
			return true;
		});
	}
}
