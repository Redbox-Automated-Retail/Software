using KioskClientService.API.App;

namespace KioskClientService.API.Services.Campaigns;

public class GetStartScreenResponse : ApiBaseResponse
{
	public KioskStartScreenDetails StartScreen { get; set; }

	public override object Scrub()
	{
		return this.ToJson();
	}
}
