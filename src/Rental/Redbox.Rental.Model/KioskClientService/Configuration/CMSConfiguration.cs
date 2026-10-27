namespace Redbox.Rental.Model.KioskClientService.Configuration;

[Category(Name = "CMS")]
public class CMSConfiguration : BaseCategorySetting
{
	public bool EnableInCart { get; set; } = true;

	public bool ShowStartScreen { get; set; } = true;

	public int LoadCampaignTimerDuration { get; set; } = 2220000;
}
