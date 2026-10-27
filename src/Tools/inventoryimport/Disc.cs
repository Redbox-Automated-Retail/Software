using Redbox.Core;

namespace InventoryImport;

public class Disc
{
	public string Deck { get; private set; }

	public string Slot { get; private set; }

	public int DiskType { get; private set; }

	public string Barcode { get; private set; }

	public Disc(string deck, string slot, string barcode, int disktype)
	{
		Deck = deck;
		Slot = slot;
		Barcode = barcode;
		DiskType = disktype;
	}

	public override string ToString()
	{
		return this.ToJson();
	}
}
