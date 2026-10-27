using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services.ShoppingCart;

public class RentalShoppingCartSubscriptionItem : BaseRentalShoppingCartItem, IRentalShoppingCartSubscriptionItem, IBaseRentalShoppingCartItem
{
	public ISubscriptionProduct SubscriptionProduct { get; set; }

	public string TempPassword { get; set; }

	public RentalShoppingCartSubscriptionItem(ISubscriptionProduct subscriptionProduct)
	{
		SubscriptionProduct = subscriptionProduct;
		base.Action = RentalShoppingCartItemAction.subscription;
		base.Quantity = 1m;
		base.TaxRate = subscriptionProduct.TaxRate;
		TempPassword = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.SubscriptionData?.TempPassword;
	}

	public override string ToString()
	{
		return string.Format($"RentalShoppingCartSubscriptionItem: SubscriptionType: {SubscriptionProduct?.SubscriptionType}, SubscriptionId: {SubscriptionProduct?.SubscriptionId}");
	}

	protected override decimal GetPrice()
	{
		return SubscriptionProduct?.Price ?? 0m;
	}

	protected override decimal GetTaxRate()
	{
		return SubscriptionProduct?.TaxRate ?? 0m;
	}
}
