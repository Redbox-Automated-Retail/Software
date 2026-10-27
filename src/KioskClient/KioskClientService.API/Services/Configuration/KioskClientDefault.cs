namespace KioskClientService.API.Services.Configuration;

public class KioskClientDefault : BaseCategorySetting
{
	public string KioskDataServiceUrl { get; set; }

	public int KioskDataServiceTimeout { get; set; } = 20000;

	[MaskLogValue(VisibleChars = 4)]
	public string KioskDataServiceAPIKey { get; set; }

	[MaskLogValue(VisibleChars = 4)]
	public string KioskDataServiceAuthHeader { get; set; } = "Authorization";

	public string ProductReelsApiUrl { get; set; }

	public int ProductReelsApiTimeout { get; set; } = 60000;

	public int MoreLikeThisDataRefreshFrequency { get; set; } = 7;

	public string InventoryApiUrl { get; set; }

	public int InventoryApiTimeout { get; set; } = 60000;

	[MaskLogValue(VisibleChars = 4)]
	public string InventoryApiKey { get; set; }

	public int ProductGroupSortOrderType { get; set; }

	public string TransactionApiUrl { get; set; }

	public int TransactionApiTimeout { get; set; } = 60000;

	[MaskLogValue(VisibleChars = 4)]
	public string TransactionApiKey { get; set; }

	public int ReturnPromoCheckTimeout { get; set; } = 8000;

	public bool PricingActive { get; set; } = true;

	public bool PlanogramSyncEnabled { get; set; } = true;
}
