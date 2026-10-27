namespace ProductPricing.Model;

public class PurchasePricingDTO
{
	public long ProductPriceId { get; set; }

	public decimal PurchasePrice { get; set; }

	public static PurchasePricingDTO FromPurchasePricing(PurchasePricing pp)
	{
		return new PurchasePricingDTO
		{
			ProductPriceId = pp.ProductPriceId,
			PurchasePrice = pp.PurchasePrice
		};
	}
}
