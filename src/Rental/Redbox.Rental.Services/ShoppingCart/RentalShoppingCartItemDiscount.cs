using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class RentalShoppingCartItemDiscount : IRentalShoppingCartItemDiscount
{
	public string DiscountCode { get; set; }

	public decimal DiscountAmount { get; set; }
}
