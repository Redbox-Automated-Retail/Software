namespace Redbox.Rental.Model;

public interface IInventoryItem
{
	int? Deck { get; set; }

	int? Slot { get; set; }

	string Barcode { get; set; }

	InventoryItemStatus Status { get; set; }
}
