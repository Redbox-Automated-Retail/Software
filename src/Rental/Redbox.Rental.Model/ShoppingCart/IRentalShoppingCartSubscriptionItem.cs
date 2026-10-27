using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.ShoppingCart;

public interface IRentalShoppingCartSubscriptionItem : IBaseRentalShoppingCartItem
{
	ISubscriptionProduct SubscriptionProduct { get; set; }

	string TempPassword { get; set; }
}
