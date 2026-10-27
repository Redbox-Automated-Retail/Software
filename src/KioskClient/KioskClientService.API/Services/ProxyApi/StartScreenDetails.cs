using KioskClientService.API.Services.Campaigns;

namespace KioskClientService.API.Services.ProxyApi;

public class StartScreenDetails : BaseDetails
{
	public ControlList Controls { get; set; }

	public StartScreenDetails()
	{
		base.CampaignType = CampaignType.StartScreen;
	}
}
