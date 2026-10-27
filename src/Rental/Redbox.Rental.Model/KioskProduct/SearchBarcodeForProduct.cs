using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.KioskProduct;

public class SearchBarcodeForProduct : ISearchBarcodeForProduct
{
	public long ProductId { get; set; }

	public string Barcode { get; set; }

	public RentalShoppingCartItemAction Action { get; set; }
}
