namespace KioskClientService.API.Services.Configuration;

[Category(Name = "Pricing")]
public class PricingConfiguration : BaseCategorySetting
{
	public int RefreshIntervalMinutes { get; set; } = 30;
}
