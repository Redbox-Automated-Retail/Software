using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model;

public interface IPromotionItem
{
	IRentalShoppingCartTitleItem RentalShoppingCartItem { get; set; }

	decimal PromotionAmount { get; set; }
}
