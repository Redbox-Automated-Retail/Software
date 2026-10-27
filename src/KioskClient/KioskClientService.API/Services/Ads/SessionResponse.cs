using KioskClientService.API.App;

namespace KioskClientService.API.Services.Ads;

public class SessionResponse : BaseResponse, IMessageScrub
{
	public SessionResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
