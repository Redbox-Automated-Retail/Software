using KioskClientService.API.App;

namespace KioskClientService.API.Services.Ads;

public class CacheRequest : BaseRequest, IMessageScrub
{
	public object Scrub()
	{
		return this.ToJsonIndented();
	}
}
