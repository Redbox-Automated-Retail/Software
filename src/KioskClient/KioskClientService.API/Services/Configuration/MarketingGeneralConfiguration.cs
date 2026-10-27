namespace KioskClientService.API.Services.Configuration;

[Category(Name = "MarketingGeneral")]
public class MarketingGeneralConfiguration : BaseCategorySetting
{
	public bool ShowAfterSwipePersonalizedOffers { get; set; }

	public bool UseDiscountsForPmnpOffers { get; set; }

	public bool EnableBrowseMessagePopup { get; set; }

	public int BrowseMessagePopupDuration { get; set; } = 15000;

	public bool EnableRedboxSweepstakes { get; set; }

	public bool EnableAvodLeadGenerationOfferWithoutBoxArt { get; set; }

	public bool EnableAvodLeadGenerationOfferWithBoxArt { get; set; }

	public bool EnableRedboxPlusLeadGenerationOffer { get; set; }

	public bool EnableRedboxPlusOffer { get; set; }

	public bool EnableRedboxPlusRedemption { get; set; } = true;

	public bool ShowKioskClosingVSM { get; set; }

	public bool DisplayUpdatedPerksPoints { get; set; } = true;

	public string PriceFiltersOnBrowse { get; set; } = "A";
}
