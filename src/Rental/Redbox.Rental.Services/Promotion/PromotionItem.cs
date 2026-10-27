using Redbox.Rental.Model;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.Promotion;

public class PromotionItem : IPromotionItem
{
	public IRentalShoppingCartTitleItem RentalShoppingCartItem { get; set; }

	public decimal PromotionAmount { get; set; }
}
