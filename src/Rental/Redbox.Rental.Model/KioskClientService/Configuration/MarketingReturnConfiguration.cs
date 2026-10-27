namespace Redbox.Rental.Model.KioskClientService.Configuration;

[Category(Name = "MarketingReturn")]
public class MarketingReturnConfiguration : BaseCategorySetting
{
	public bool ReturnVisitPromoOfferEnabled { get; set; }

	public int ReturnVisitPromoCheckTimeout { get; set; } = 10000;
}
