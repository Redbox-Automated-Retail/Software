using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.KioskProduct;

public interface ISearchBarcodeForProduct
{
	long ProductId { get; set; }

	string Barcode { get; set; }

	RentalShoppingCartItemAction Action { get; set; }
}
