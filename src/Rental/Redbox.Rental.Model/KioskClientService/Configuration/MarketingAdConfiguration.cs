namespace Redbox.Rental.Model.KioskClientService.Configuration;

[Category(Name = "MarketingAd")]
public class MarketingAdConfiguration : BaseCategorySetting
{
	public bool EnableVistarAds { get; set; } = true;

	public int VistarGetAdTimeout { get; set; } = 1000;

	public int StartViewAdTimeout { get; set; } = 300000;
}
