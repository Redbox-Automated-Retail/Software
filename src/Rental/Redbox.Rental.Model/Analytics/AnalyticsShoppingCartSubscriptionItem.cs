using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Analytics;

public class AnalyticsShoppingCartSubscriptionItem
{
	public string SubscriptionId { get; set; }

	public string SubscriptionType { get; set; }

	public decimal Price { get; set; }

	public decimal TaxRate { get; set; }

	public static AnalyticsShoppingCartSubscriptionItem ToAnalyticsShoppingCartSubscriptionItem(IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem)
	{
		return new AnalyticsShoppingCartSubscriptionItem
		{
			SubscriptionId = rentalShoppingCartSubscriptionItem?.SubscriptionProduct?.SubscriptionId,
			SubscriptionType = rentalShoppingCartSubscriptionItem?.SubscriptionProduct?.SubscriptionType.ToString(),
			Price = (rentalShoppingCartSubscriptionItem?.Price ?? 0m),
			TaxRate = (rentalShoppingCartSubscriptionItem?.TaxRate ?? 0m)
		};
	}
}
