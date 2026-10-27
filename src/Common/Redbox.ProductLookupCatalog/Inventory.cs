using System;

namespace Redbox.ProductLookupCatalog;

public class Inventory : IComparable<Inventory>
{
	public string Barcode { get; set; }

	public uint TitleId { get; set; }

	public InventoryStatusCode Code { get; set; }

	public uint TotalRentalCount { get; set; }

	public int CompareTo(Inventory other)
	{
		return Barcode.CompareTo(other.Barcode);
	}
}
