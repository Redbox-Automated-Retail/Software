namespace Redbox.KioskEngine.ComponentModel;

public class Inventory
{
	public string Barcode { get; set; }

	public InventoryStatusCode Code { get; set; }

	public uint TitleId { get; set; }

	public uint TotalRentalCount { get; set; }
}
