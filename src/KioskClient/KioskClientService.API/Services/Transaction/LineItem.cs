namespace KioskClientService.API.Services.Transaction;

public class LineItem
{
	public long ProductId { get; set; }

	public string SubscriptionId { get; set; }

	public string Barcode { get; set; }

	public VendStatus VendStatus { get; set; }

	public string ProductFamily { get; set; }

	public string ProductType { get; set; }

	public string ProductName { get; set; }

	public Pricing Pricing { get; set; }

	public BlurayUpsell BlurayUpsell { get; set; }

	public ItemSourceType? SourceType { get; set; }

	public string OfferCode { get; set; }

	public string TivoQueryId { get; set; }

	public string TempPassword { get; set; }
}
