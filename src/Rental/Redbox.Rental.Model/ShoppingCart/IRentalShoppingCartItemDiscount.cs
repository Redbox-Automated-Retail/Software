namespace Redbox.Rental.Model.ShoppingCart;

public interface IRentalShoppingCartItemDiscount
{
	string DiscountCode { get; set; }

	decimal DiscountAmount { get; set; }
}
