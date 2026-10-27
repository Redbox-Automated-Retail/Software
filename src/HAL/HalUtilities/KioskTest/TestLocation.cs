using System;
using Redbox.HAL.Component.Model;

namespace HALUtilities.KioskTest;

internal sealed class TestLocation : ILocation
{
	public bool IsEmpty { get; private set; }

	public bool IsWide { get; private set; }

	public int Deck { get; private set; }

	public int Slot { get; private set; }

	public string ID { get; set; }

	public DateTime? ReturnDate { get; set; }

	public bool Excluded { get; set; }

	public int StuckCount { get; set; }

	public MerchFlags Flags { get; set; }

	internal TestLocation(int deck, int slot)
	{
		Deck = deck;
		Slot = slot;
		ID = "EMPTY";
		StuckCount = 0;
		Excluded = false;
	}
}
