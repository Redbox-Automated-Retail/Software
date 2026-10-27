using Newtonsoft.Json;

namespace KioskClientService.API.Services.Pricing;

public class ProductPurchasePrice
{
	public decimal PurchasePrice { get; set; }

	[JsonIgnore]
	public bool IsValid => PurchasePrice >= 0m;
}
