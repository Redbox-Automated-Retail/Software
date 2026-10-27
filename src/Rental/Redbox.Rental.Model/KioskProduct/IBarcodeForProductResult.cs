using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.KioskProduct;

public interface IBarcodeForProductResult
{
	long ProductId { get; set; }

	string Barcode { get; set; }

	bool BarcodeAssigned { get; set; }

	LineItemStatus? Status { get; set; }

	RentalShoppingCartItemAction Action { get; set; }
}
