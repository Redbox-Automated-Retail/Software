using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class VmzExcludePolicy : IExcludeEmptySearchLocationObserver
{
	private readonly int Deck = 8;

	private readonly IRange<int> Slots = new Range(82, 84);

	public bool ShouldExclude(ILocation location)
	{
		if (location.Deck == Deck)
		{
			return Slots.Includes(location.Slot);
		}
		return false;
	}

	internal VmzExcludePolicy()
	{
	}
}
