namespace HALUtilities;

internal class LocStat
{
	internal int Deck { get; private set; }

	internal int Slot { get; private set; }

	internal int Count { get; private set; }

	internal void Increment()
	{
		int count = Count + 1;
		Count = count;
	}

	internal LocStat(int deck, int slot)
	{
		Deck = deck;
		Slot = slot;
	}
}
