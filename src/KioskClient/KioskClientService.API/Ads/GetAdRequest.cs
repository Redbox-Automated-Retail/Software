using KioskClientService.API.App;

namespace KioskClientService.API.Ads;

public class GetAdRequest : BaseRequest, IMessageScrub
{
	public AdLocation AdLocation { get; set; }

	public object Scrub()
	{
		return this.ToJsonIndented();
	}
}
