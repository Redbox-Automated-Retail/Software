namespace Redbox.HAL.MSHALTester;

internal sealed class TransferLocation
{
	internal readonly int Deck;

	internal readonly int Slot;

	internal bool IsValid
	{
		get
		{
			if (-1 != Deck)
			{
				return -1 != Slot;
			}
			return false;
		}
	}

	internal TransferLocation(int deck, int slot)
	{
		Deck = deck;
		Slot = slot;
	}
}
