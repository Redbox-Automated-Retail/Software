namespace Redbox.Rental.Model.ShoppingCart;

public interface IBaseRentalShoppingCartItem
{
	RentalShoppingCartItemSequence Sequence { get; set; }

	RentalShoppingCartItemAction Action { get; set; }

	decimal Quantity { get; set; }

	decimal Price { get; set; }

	decimal ExtendedPrice { get; }

	decimal TaxRate { get; set; }
}
