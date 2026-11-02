using System;

namespace Redbox.ProductLookupCatalog;

public class Inventory : IComparable<Inventory>
{
	// Set by both archive readers right after construction.
	public string Barcode { get; set; } = null!;

	public uint TitleId { get; set; }

	public InventoryStatusCode Code { get; set; }

	public uint TotalRentalCount { get; set; }

	public int CompareTo(Inventory? other)
	{
		// Throws for a null other, as before.
		return Barcode.CompareTo(other!.Barcode);
	}
}
