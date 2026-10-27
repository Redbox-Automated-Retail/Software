namespace ProductPricing.Model;

public interface IPurchasePricing : IPricing
{
	long ProductId { get; set; }

	decimal PurchasePrice { get; set; }
}
