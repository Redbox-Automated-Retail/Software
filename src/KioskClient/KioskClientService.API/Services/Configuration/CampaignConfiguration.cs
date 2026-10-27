namespace KioskClientService.API.Services.Configuration;

[Category(Name = "Campaigns")]
public class CampaignConfiguration : BaseCategorySetting
{
	public bool JobActive { get; set; } = true;

	public int RefreshInterval { get; set; } = 30;

	public bool InCartsActive { get; set; } = true;

	public bool StartScreensActive { get; set; } = true;

	public int PromoCodeCacheTTLMinutes { get; set; } = 60;
}
